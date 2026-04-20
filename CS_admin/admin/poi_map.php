<?php
$auth = isset($_SESSION['admin_auth']) && is_array($_SESSION['admin_auth']) ? $_SESSION['admin_auth'] : array();
$idTaiKhoan = isset($auth['idTaiKhoan']) ? (int) $auth['idTaiKhoan'] : 0;
$loaiTaiKhoan = isset($auth['loaiTaiKhoan']) ? (string) $auth['loaiTaiKhoan'] : 'admin';
$isOwner = $loaiTaiKhoan === 'chu_quan_ly';

if (!function_exists('poi_map_google_api_key')) {
    function poi_map_google_api_key()
    {
        $browserKeyPath = dirname(__DIR__) . '/Secret/google-maps-browser-key.txt';
        if (is_file($browserKeyPath)) {
            $browserKey = trim((string) file_get_contents($browserKeyPath));
            if ($browserKey !== '') {
                return $browserKey;
            }
        }

        $envKeys = array('CSA_GOOGLE_MAPS_BROWSER_KEY', 'GOOGLE_MAPS_BROWSER_KEY');
        foreach ($envKeys as $envKey) {
            $value = getenv($envKey);
            if (is_string($value) && trim($value) !== '') {
                return trim($value);
            }
        }

        return '';
    }
}

if (!function_exists('poi_map_google_api_key_source')) {
    function poi_map_google_api_key_source()
    {
        $browserKeyPath = dirname(__DIR__) . '/Secret/google-maps-browser-key.txt';
        if (is_file($browserKeyPath) && trim((string) file_get_contents($browserKeyPath)) !== '') {
            return 'file';
        }

        $browserEnvKeys = array('CSA_GOOGLE_MAPS_BROWSER_KEY', 'GOOGLE_MAPS_BROWSER_KEY');
        foreach ($browserEnvKeys as $envKey) {
            $value = getenv($envKey);
            if (is_string($value) && trim($value) !== '') {
                return 'env';
            }
        }

        return 'missing';
    }
}

if (!function_exists('poi_map_google_map_id')) {
    function poi_map_google_map_id()
    {
        $envKeys = array('CSA_GOOGLE_MAPS_MAP_ID', 'GOOGLE_MAPS_MAP_ID');
        foreach ($envKeys as $envKey) {
            $value = getenv($envKey);
            if (is_string($value) && trim($value) !== '') {
                return trim($value);
            }
        }

        return 'DEMO_MAP_ID';
    }
}

if (!function_exists('poi_map_image_url')) {
    function poi_map_image_url($path)
    {
        $path = trim((string) $path);
        if ($path === '') {
            return '';
        }

        return admin_url('api/image-proxy.php') . '?path=' . rawurlencode($path);
    }
}

if (!function_exists('poi_map_status_meta')) {
    function poi_map_status_meta($status)
    {
        $status = strtolower(trim((string) $status));
        switch ($status) {
            case 'dang_hoat_dong':
            case 'hoat_dong':
                return array('label' => 'Dang hoat dong', 'class' => 'active');
            case 'tam_ngung':
            case 'tam_dung':
                return array('label' => 'Tam ngung', 'class' => 'paused');
            case 'dong_cua':
                return array('label' => 'Dong cua', 'class' => 'closed');
            default:
                return array('label' => 'Khong ro', 'class' => 'unknown');
        }
    }
}

if (!function_exists('poi_map_money')) {
    function poi_map_money($value)
    {
        return number_format((float) $value, 0, ',', '.') . 'd';
    }
}

if (!function_exists('poi_map_fetch_pois')) {
    function poi_map_fetch_pois($isOwner, $idTaiKhoan, &$error)
    {
        $error = '';
        $conn = admin_db_connection();
        if (!$conn instanceof mysqli) {
            $error = 'Khong the ket noi DB de tai POI.';
            return array();
        }

        $sql = "
            SELECT
                gh.idGianHang,
                gh.ten,
                gh.diaChi,
                gh.lat,
                gh.lon,
                gh.vongBo,
                gh.tinhTrang,
                gh.phiHangThang,
                cql.hoTen AS tenChuQuanLy,
                tk.username AS usernameChuQuanLy,
                tk.email AS emailChuQuanLy,
                (
                    SELECT hgg.duongDan
                    FROM hinhanhgianhang hgg
                    WHERE hgg.idGianHang = gh.idGianHang
                    ORDER BY hgg.idHinhAnh
                    LIMIT 1
                ) AS hinhAnh
            FROM gianhang gh
            LEFT JOIN chu_quan_ly cql ON cql.idChuQuanLy = gh.idChuQuanLy
            LEFT JOIN taikhoan tk ON tk.idTaiKhoan = cql.idTaiKhoan
            WHERE gh.lat IS NOT NULL
              AND gh.lon IS NOT NULL
              AND gh.lat BETWEEN -90 AND 90
              AND gh.lon BETWEEN -180 AND 180";

        if ($isOwner) {
            $sql .= " AND cql.idTaiKhoan = ?";
        }

        $sql .= "
            ORDER BY
                CASE WHEN gh.tinhTrang = 'dang_hoat_dong' THEN 0 ELSE 1 END,
                gh.phiHangThang DESC,
                gh.idGianHang ASC";

        $stmt = $conn->prepare($sql);
        if (!$stmt) {
            $error = $conn->error;
            $conn->close();
            return array();
        }

        if ($isOwner) {
            $stmt->bind_param('i', $idTaiKhoan);
        }

        if (!$stmt->execute()) {
            $error = $stmt->error;
            $stmt->close();
            $conn->close();
            return array();
        }

        $result = $stmt->get_result();
        $pois = array();
        while ($result && ($row = $result->fetch_assoc())) {
            $statusMeta = poi_map_status_meta($row['tinhTrang'] ?? '');
            $ownerName = '';
            if (!empty($row['tenChuQuanLy'])) {
                $ownerName = (string) $row['tenChuQuanLy'];
            } elseif (!empty($row['usernameChuQuanLy'])) {
                $ownerName = (string) $row['usernameChuQuanLy'];
            } elseif (!empty($row['emailChuQuanLy'])) {
                $ownerName = (string) $row['emailChuQuanLy'];
            }

            $radius = isset($row['vongBo']) && $row['vongBo'] !== null ? (float) $row['vongBo'] : 10.0;
            if ($radius <= 0) {
                $radius = 10.0;
            }

            $fee = isset($row['phiHangThang']) ? (float) $row['phiHangThang'] : 0.0;
            $pois[] = array(
                'id' => isset($row['idGianHang']) ? (int) $row['idGianHang'] : 0,
                'name' => isset($row['ten']) ? (string) $row['ten'] : 'Gian hang',
                'address' => !empty($row['diaChi']) ? (string) $row['diaChi'] : 'Chua cap nhat dia chi',
                'lat' => (float) $row['lat'],
                'lng' => (float) $row['lon'],
                'radiusMeters' => $radius,
                'status' => isset($row['tinhTrang']) ? (string) $row['tinhTrang'] : '',
                'statusLabel' => $statusMeta['label'],
                'statusClass' => $statusMeta['class'],
                'monthlyFee' => $fee,
                'monthlyFeeLabel' => poi_map_money($fee),
                'ownerName' => $ownerName !== '' ? $ownerName : 'Chua gan chu quan ly',
                'imageUrl' => !empty($row['hinhAnh']) ? poi_map_image_url((string) $row['hinhAnh']) : '',
            );
        }

        if ($result) {
            $result->free();
        }
        $stmt->close();
        $conn->close();

        return $pois;
    }
}

$poiError = '';
$pois = poi_map_fetch_pois($isOwner, $idTaiKhoan, $poiError);
$googleMapsApiKey = poi_map_google_api_key();
$googleMapsApiKeySource = poi_map_google_api_key_source();
$googleMapsMapId = poi_map_google_map_id();

$activeCount = 0;
$pausedCount = 0;
$closedCount = 0;
$totalFee = 0.0;
$centerLat = 10.762622;
$centerLng = 106.660172;

if (count($pois) > 0) {
    $sumLat = 0.0;
    $sumLng = 0.0;
    foreach ($pois as $poi) {
        $sumLat += (float) $poi['lat'];
        $sumLng += (float) $poi['lng'];
        $totalFee += (float) $poi['monthlyFee'];

        if ($poi['statusClass'] === 'active') {
            $activeCount++;
        } elseif ($poi['statusClass'] === 'paused') {
            $pausedCount++;
        } elseif ($poi['statusClass'] === 'closed') {
            $closedCount++;
        }
    }

    $centerLat = $sumLat / count($pois);
    $centerLng = $sumLng / count($pois);
}

$mapPayload = json_encode($pois, JSON_UNESCAPED_SLASHES | JSON_UNESCAPED_UNICODE | JSON_HEX_TAG | JSON_HEX_AMP | JSON_HEX_APOS | JSON_HEX_QUOT);
$mapConfig = json_encode(array(
    'center' => array('lat' => $centerLat, 'lng' => $centerLng),
    'mapId' => $googleMapsMapId,
    'hasApiKey' => $googleMapsApiKey !== '',
    'apiKeySource' => $googleMapsApiKeySource,
    'apiKeyPrefix' => $googleMapsApiKey !== '' ? substr($googleMapsApiKey, 0, 10) . '...' : '',
), JSON_UNESCAPED_SLASHES | JSON_UNESCAPED_UNICODE | JSON_HEX_TAG | JSON_HEX_AMP | JSON_HEX_APOS | JSON_HEX_QUOT);
?>
<main class="main-content">
  <section class="poi-map-page">
    <div class="poi-map-header">
      <div>
        <p class="poi-map-kicker">Google Maps 3D</p>
        <h2>Ban do POI gian hang</h2>
        <p>Theo doi vi tri, vung geofence va trang thai cua cac cua hang tren ban do nghieng 3D.</p>
      </div>
      <a class="poi-map-action" href="<?php echo htmlspecialchars(admin_url('index1st.php?usecase=store'), ENT_QUOTES, 'UTF-8'); ?>">
        <i class="fa-solid fa-store"></i>
        <span>Danh sach gian hang</span>
      </a>
    </div>

    <?php if ($poiError !== '') { ?>
    <div class="poi-map-alert error">
      <i class="fa-solid fa-triangle-exclamation"></i>
      <span><?php echo htmlspecialchars($poiError, ENT_QUOTES, 'UTF-8'); ?></span>
    </div>
    <?php } elseif ($googleMapsApiKey === '') { ?>
    <div class="poi-map-alert warning">
      <i class="fa-solid fa-key"></i>
      <span>Chua cau hinh Google Maps browser key. Dat key web vao CS_admin/Secret/google-maps-browser-key.txt de hien thi ban do.</span>
    </div>
    <?php } elseif ($googleMapsApiKeySource !== 'file') { ?>
    <div class="poi-map-alert warning">
      <i class="fa-solid fa-globe"></i>
      <span>Dang dung Google Maps key tu bien moi truong. Neu Google Maps van khong hien, hay cho phep key nay dung Maps JavaScript API voi HTTP referrer localhost trong Google Cloud.</span>
    </div>
    <?php } ?>

    <div class="poi-map-stats">
      <div class="poi-stat">
        <span>Tong POI</span>
        <strong><?php echo count($pois); ?></strong>
      </div>
      <div class="poi-stat active">
        <span>Dang hoat dong</span>
        <strong><?php echo $activeCount; ?></strong>
      </div>
      <div class="poi-stat paused">
        <span>Tam ngung</span>
        <strong><?php echo $pausedCount; ?></strong>
      </div>
      <div class="poi-stat">
        <span>Phi thang</span>
        <strong><?php echo htmlspecialchars(poi_map_money($totalFee), ENT_QUOTES, 'UTF-8'); ?></strong>
      </div>
    </div>

    <div class="poi-map-layout">
      <section class="poi-map-stage">
        <div class="poi-map-toolbar">
          <label class="poi-search">
            <i class="fa-solid fa-magnifying-glass"></i>
            <input id="poiMapSearch" type="search" placeholder="Tim POI, dia chi, chu quan ly" autocomplete="off" />
          </label>
          <div class="poi-map-filter" aria-label="Loc POI">
            <button type="button" class="active" data-poi-status="all">Tat ca</button>
            <button type="button" data-poi-status="active">Hoat dong</button>
            <button type="button" data-poi-status="paused">Tam ngung</button>
            <button type="button" data-poi-status="closed">Dong cua</button>
          </div>
        </div>

        <div id="poiGoogleMap" class="poi-google-map" aria-label="Ban do POI 3D">
          <div class="poi-map-loading">
            <i class="fa-solid fa-map-location-dot"></i>
            <span>Dang tai Google Maps 3D...</span>
          </div>
        </div>

        <div class="poi-map-controls">
          <button type="button" id="poiFitBounds" title="Can vua tat ca POI" aria-label="Can vua tat ca POI">
            <i class="fa-solid fa-compress"></i>
          </button>
          <button type="button" id="poiToggleTilt" title="Bat/tat nghieng 3D" aria-label="Bat/tat nghieng 3D">
            <i class="fa-solid fa-cube"></i>
          </button>
          <button type="button" id="poiRotateLeft" title="Xoay trai" aria-label="Xoay trai">
            <i class="fa-solid fa-rotate-left"></i>
          </button>
          <button type="button" id="poiRotateRight" title="Xoay phai" aria-label="Xoay phai">
            <i class="fa-solid fa-rotate-right"></i>
          </button>
        </div>
      </section>

      <aside class="poi-map-panel">
        <div class="poi-panel-head">
          <div>
            <h3>POI cua hang</h3>
            <p><span id="poiVisibleCount"><?php echo count($pois); ?></span> diem co toa do hop le</p>
          </div>
          <span class="poi-live-pill">3D</span>
        </div>

        <div id="poiList" class="poi-list"></div>

        <div class="poi-empty" id="poiEmptyState">
          <i class="fa-regular fa-map"></i>
          <strong>Khong co POI phu hop</strong>
          <span>Thu bo loc khac hoac cap nhat toa do gian hang.</span>
        </div>
      </aside>
    </div>
  </section>
</main>

<script>
window.POI_ADMIN_MAP_DATA = <?php echo $mapPayload ?: '[]'; ?>;
window.POI_ADMIN_MAP_CONFIG = <?php echo $mapConfig ?: '{}'; ?>;
</script>
<script src="<?php echo htmlspecialchars(admin_url('asset/admin/js/poi-map.js'), ENT_QUOTES, 'UTF-8'); ?>?v=<?php echo filemtime(__DIR__ . '/../asset/admin/js/poi-map.js'); ?>"></script>
<?php if ($googleMapsApiKey !== '') { ?>
<script async defer src="https://maps.googleapis.com/maps/api/js?key=<?php echo rawurlencode($googleMapsApiKey); ?>&libraries=marker&callback=initPoiAdminMap&loading=async"></script>
<?php } else { ?>
<script>
if (window.initPoiAdminMap) {
  window.initPoiAdminMap();
}
</script>
<?php } ?>
