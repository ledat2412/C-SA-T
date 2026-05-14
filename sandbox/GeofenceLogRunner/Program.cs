// Console log runner that mirrors the geofence decision flow from
// C-SA-T/Services/GeofenceEngineService.cs without requiring MAUI runtime.

var stores = new[]
{
    new Store(1, "Banh Mi Booth", 10.7630000, 106.6605000, RadiusMeters: 8, MonthlyFee: 100_000m, AudioUrl: "audio/banh-mi.mp3"),
    new Store(2, "VIP Coffee", 10.7630450, 106.6605000, RadiusMeters: 8, MonthlyFee: 300_000m, AudioUrl: "audio/vip-coffee.mp3"),
    new Store(3, "Food Court", 10.7631500, 106.6605000, RadiusMeters: 15, MonthlyFee: 500_000m, AudioUrl: "audio/food-court.mp3"),
};

var priorityBoosts = new Dictionary<int, int>
{
    [2] = 5
};

var route = new[]
{
    new RoutePoint("Start outside all booths", 10.7628500, 106.6605000),
    new RoutePoint("Walk into Banh Mi radius", 10.7629650, 106.6605000),
    new RoutePoint("Overlap Banh Mi + VIP Coffee", 10.7630250, 106.6605000),
    new RoutePoint("Closer to VIP Coffee", 10.7630500, 106.6605000),
    new RoutePoint("Between VIP Coffee and Food Court", 10.7630950, 106.6605000),
    new RoutePoint("Inside Food Court only", 10.7631500, 106.6605000),
    new RoutePoint("Exit all booths", 10.7633400, 106.6605000),
};

var insideIds = new HashSet<int>();

Console.WriteLine("=== Geofence LogRunner demo ===");
Console.WriteLine("Rule: boost desc > distance/radius asc > monthly fee desc > id asc");
Console.WriteLine("Boosts: store #2 VIP Coffee = 5");
Console.WriteLine();

for (var tick = 0; tick < route.Length; tick++)
{
    var point = route[tick];
    var triggers = stores
        .Select(store =>
        {
            var distance = HaversineMeters(point.Latitude, point.Longitude, store.Latitude, store.Longitude);
            return new Trigger(store, distance, distance <= store.RadiusMeters);
        })
        .ToArray();

    var currentlyInside = triggers.Where(x => x.IsInside).ToList();
    var currentInsideIds = currentlyInside.Select(x => x.Store.Id).ToHashSet();
    var entered = currentInsideIds.Except(insideIds).Order().ToList();
    var exited = insideIds.Except(currentInsideIds).Order().ToList();
    insideIds.Clear();
    foreach (var id in currentInsideIds)
        insideIds.Add(id);

    Console.WriteLine($"Tick {tick + 1}: {point.Label}");
    Console.WriteLine($"  GPS: {point.Latitude:F7}, {point.Longitude:F7}");

    foreach (var trigger in triggers.OrderBy(x => x.Store.Id))
    {
        var boost = priorityBoosts.TryGetValue(trigger.Store.Id, out var value) ? value : 0;
        var ratio = trigger.Store.RadiusMeters > 0
            ? trigger.DistanceMeters / trigger.Store.RadiusMeters
            : double.MaxValue;

        Console.WriteLine(
            $"  - #{trigger.Store.Id} {trigger.Store.Name,-15} " +
            $"d={trigger.DistanceMeters,5:F1}m r={trigger.Store.RadiusMeters,4:F0}m " +
            $"ratio={ratio,5:F2} boost={boost,2} status={(trigger.IsInside ? "INSIDE" : "outside")}");
    }

    foreach (var id in entered)
    {
        var store = stores.First(x => x.Id == id);
        Console.WriteLine($"  EVENT: ENTER #{store.Id} {store.Name}");
    }

    foreach (var id in exited)
    {
        var store = stores.First(x => x.Id == id);
        Console.WriteLine($"  EVENT: EXIT  #{store.Id} {store.Name}");
    }

    if (currentlyInside.Count == 0)
    {
        Console.WriteLine("  RESULT: no geofence active, audio hidden");
    }
    else
    {
        var ordered = Prioritize(currentlyInside, priorityBoosts).ToList();
        var winner = ordered[0];
        var queue = string.Join(" -> ", ordered.Select(x => $"#{x.Store.Id} {x.Store.Name}"));

        Console.WriteLine($"  PRIORITY QUEUE: {queue}");
        Console.WriteLine($"  RESULT: pending autoplay for #{winner.Store.Id} {winner.Store.Name} ({winner.Store.AudioUrl})");
    }

    Console.WriteLine();
}

Console.WriteLine("=== End of demo ===");

static IOrderedEnumerable<Trigger> Prioritize(
    IEnumerable<Trigger> triggers,
    IReadOnlyDictionary<int, int> priorityBoosts)
{
    return triggers
        .OrderByDescending(x => priorityBoosts.TryGetValue(x.Store.Id, out var priority) ? priority : 0)
        .ThenBy(x => x.Store.RadiusMeters > 0
            ? x.DistanceMeters / x.Store.RadiusMeters
            : double.MaxValue)
        .ThenByDescending(x => x.Store.MonthlyFee)
        .ThenBy(x => x.Store.Id);
}

static double HaversineMeters(double lat1, double lon1, double lat2, double lon2)
{
    const double earthRadiusMeters = 6_371_000d;

    var dLat = ToRadians(lat2 - lat1);
    var dLon = ToRadians(lon2 - lon1);
    var rLat1 = ToRadians(lat1);
    var rLat2 = ToRadians(lat2);

    var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(rLat1) * Math.Cos(rLat2) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

    return earthRadiusMeters * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
}

static double ToRadians(double degrees) => degrees * Math.PI / 180d;

record Store(
    int Id,
    string Name,
    double Latitude,
    double Longitude,
    double RadiusMeters,
    decimal MonthlyFee,
    string AudioUrl);

record RoutePoint(string Label, double Latitude, double Longitude);

record Trigger(Store Store, double DistanceMeters, bool IsInside);
