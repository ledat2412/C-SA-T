using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using Windows.Devices.Geolocation;

class MapForm : Form
{
    private readonly WebView2 web = new() { Dock = DockStyle.Fill };

    private readonly Button btnStart = new() { Text = "Start", AutoSize = true };
    private readonly Button btnStop = new() { Text = "Stop", AutoSize = true, Enabled = false };
    private readonly Button btnDevTools = new() { Text = "DevTools", AutoSize = true };
    private readonly NumericUpDown numMaxAcc = new() { Minimum = 10, Maximum = 5000, Value = 150, Width = 80 };
    private readonly Label lblMaxAcc = new() { AutoSize = true, Text = "MaxAcc(m):" };
    private readonly Label lblInfo = new() { AutoSize = true, Text = "Status: init" };

    private Geolocator? geo;
    private bool mapReady = false;
    private bool tracking = false;

    public MapForm()
    {
        Text = "GPS Tracking Map (WinForms + WebView2 + Leaflet)";
        Width = 1100;
        Height = 720;

        var top = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 44,
            Padding = new Padding(8),
            WrapContents = false
        };

        top.Controls.Add(btnStart);
        top.Controls.Add(btnStop);
        top.Controls.Add(btnDevTools);
        top.Controls.Add(lblMaxAcc);
        top.Controls.Add(numMaxAcc);
        top.Controls.Add(lblInfo);

        Controls.Add(web);
        Controls.Add(top);

        Shown += async (_, __) => await InitAsync();
        FormClosing += (_, __) => StopTracking();

        btnStart.Click += async (_, __) => await StartTrackingAsync();
        btnStop.Click += (_, __) => StopTracking();
        btnDevTools.Click += (_, __) =>
        {
            try { web.CoreWebView2?.OpenDevToolsWindow(); } catch { }
        };
    }

    private async Task InitAsync()
    {
        lblInfo.Text = "Status: initializing WebView2...";
        await web.EnsureCoreWebView2Async();

        // Khi page load xong, mapReady = true (JS sẽ set window.mapReady = true)
        web.CoreWebView2.NavigationCompleted += async (_, __) =>
        {
            // đợi JS chạy xong một chút
            await Task.Delay(150);
            mapReady = true;
            lblInfo.Text = "Status: map ready (click Start)";
        };

        // Leaflet + OSM (CDN + tile). Cần internet để hiển thị bản đồ.
        var html = @"
<!doctype html><html><head>
<meta charset='utf-8' />
<meta name='viewport' content='width=device-width, initial-scale=1.0'/>
<link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css'/>
<script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>
<style>html,body,#map{height:100%;margin:0}</style>
</head><body>
<div id='map'></div>
<script>
    const map = L.map('map').setView([10.7769, 106.7009], 16);
  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { maxZoom: 19 }).addTo(map);

  const marker = L.marker([10.7769, 106.7009]).addTo(map);
  const path = L.polyline([], {}).addTo(map);

  let firstFix = true;

  window.setPos = (lat, lng) => {
    const ll = [lat, lng];
    marker.setLatLng(ll);
    path.addLatLng(ll);

    if (firstFix) { map.setView(ll, 16); firstFix = false; }
    else { map.panTo(ll, { animate: true }); }
  };

  window.clearPath = () => {
    path.setLatLngs([]);
    firstFix = true;
  };

  window.mapReady = true;
</script>
</body></html>";

        web.CoreWebView2.NavigateToString(html);
    }

    private async Task StartTrackingAsync()
    {
        if (tracking) return;

        if (!mapReady || web.CoreWebView2 == null)
        {
            MessageBox.Show("Map chưa sẵn sàng. Đợi vài giây rồi bấm Start lại.");
            return;
        }

        // Xin quyền Location
        var access = await Geolocator.RequestAccessAsync();
        if (access != GeolocationAccessStatus.Allowed)
        {
            MessageBox.Show(
                $"Location not allowed: {access}\n" +
                "Hãy bật: Settings → Privacy & security → Location\n" +
                "và Let desktop apps access your location."
            );
            return;
        }

        // Clear đường đi khi start (tuỳ bạn: bỏ nếu muốn giữ)
        await SafeScriptAsync("clearPath();");

        geo = new Geolocator
        {
            DesiredAccuracyInMeters = 10,
            ReportInterval = 2000,     // ms
            MovementThreshold = 5      // m (chỉ update khi di chuyển >= 5m)
        };

        geo.StatusChanged += Geo_StatusChanged;
        geo.PositionChanged += Geo_PositionChanged;

        tracking = true;
        btnStart.Enabled = false;
        btnStop.Enabled = true;

        lblInfo.Text = "Status: tracking...";
    }

    private void StopTracking()
    {
        if (!tracking) return;

        try
        {
            if (geo != null)
            {
                geo.StatusChanged -= Geo_StatusChanged;
                geo.PositionChanged -= Geo_PositionChanged;
                geo = null;
            }
        }
        catch { /* ignore */ }

        tracking = false;
        btnStart.Enabled = true;
        btnStop.Enabled = false;

        lblInfo.Text = "Status: stopped";
    }

    private void Geo_StatusChanged(Geolocator sender, StatusChangedEventArgs args)
    {
        // event có thể không ở UI thread
        BeginInvoke(new Action(() =>
        {
            lblInfo.Text = $"Status: {args.Status}";
        }));
    }

    private void Geo_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
    {
        // event có thể không ở UI thread
        BeginInvoke(new Action(async () =>
        {
            try
            {
                if (!mapReady || web.CoreWebView2 == null) return;

                var c = args.Position.Coordinate;
                var p = c.Point.Position;
                var acc = c.Accuracy; // meters

                var maxAcc = (double)numMaxAcc.Value;

                // Lọc điểm quá tệ (giảm nhảy loạn)
                if (acc > maxAcc)
                {
                    lblInfo.Text = $"Skipped: Acc {acc:0}m > {maxAcc:0}m";
                    return;
                }

                // FIX QUAN TRỌNG: InvariantCulture để không bị dấu phẩy
                string lat = p.Latitude.ToString(CultureInfo.InvariantCulture);
                string lng = p.Longitude.ToString(CultureInfo.InvariantCulture);

                // Log để bạn đối chiếu (mở console bằng cách đổi OutputType=Exe nếu muốn)
                // System.Diagnostics.Debug.WriteLine($"RAW: {p.Latitude} {p.Longitude} acc={acc}");

                await SafeScriptAsync($"setPos({lat},{lng});");

                lblInfo.Text =
                $"RAW Lat={p.Latitude:0.000000}, Lng={p.Longitude:0.000000}, Acc={acc:0}m | MaxAcc={(double)numMaxAcc.Value:0}m";

            }
            catch (Exception ex)
            {
                lblInfo.Text = $"Error: {ex.Message}";
            }
        }));
    }

    private async Task SafeScriptAsync(string js)
    {
        try
        {
            if (web.CoreWebView2 != null)
                await web.ExecuteScriptAsync(js);
        }
        catch
        {
            // ignore: web not ready/disposed
        }
    }
}

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MapForm());
    }
}
