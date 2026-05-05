// Sandbox: test tour-aware geofence priority without MAUI/Android runtime.
//
// Mirrors the current app behavior:
// - Normal mode: priority = distance/radius ASC, monthly fee DESC, id ASC.
// - Active tour: all tour stops get priority boost.
// - Current/next tour stop get strongest boost.
// - Stop tour: boosts are cleared and normal priority returns.
// - Entering a tour geofence calls AdvanceAsync-like state transition.
// - Auto audio: inside geofence schedules/plays audio for the priority winner.

int passed = 0, failed = 0;

void Assert(bool ok, string message)
{
    Console.WriteLine($"  [{(ok ? "PASS" : "FAIL")}] {message}");
    if (ok) passed++; else failed++;
}

void Section(string title)
{
    Console.WriteLine();
    Console.WriteLine($"=== {title} ===");
}

var booths = new Dictionary<int, Booth>
{
    [1] = new(1, "Stop 1 - Welcome", 0, 0, 10, 100_000m, "/audio/stop-1.mp3"),
    [2] = new(2, "Stop 2 - Vietnam", 0, 0, 10, 80_000m, "/audio/stop-2.mp3"),
    [3] = new(3, "Stop 3 - Korea", 0, 0, 10, 60_000m, "/audio/stop-3.mp3"),
    [4] = new(4, "Stop 4 - Japan", 0, 0, 10, 50_000m, "/audio/stop-4.mp3"),
    [98] = new(98, "Silent Booth", 0, 0, 10, 900_000m, null),
    [99] = new(99, "Outsider VIP", 0, 0, 20, 2_000_000m, "/audio/outsider.mp3"),
};

var tour = new Tour(7, "Sandbox Food Tour", new List<TourStop>
{
    new(1, 1),
    new(2, 2),
    new(3, 3),
    new(4, 4),
});

Section("1. Normal geofence priority before tour");
{
    var triggers = new[]
    {
        new Trigger(booths[2], DistanceMeters: 1),
        new Trigger(booths[99], DistanceMeters: 0),
    };

    var winner = Priority.Prioritize(triggers, new Dictionary<int, int>()).First();
    Assert(winner.Booth.Id == 99, "No active tour: outsider wins by better ratio and higher fee.");
}

Section("2. Active tour boosts all tour stops above normal booths");
{
    var state = new TourState();
    var boosts = TourPriority.BuildBoosts(tour, state);
    var triggers = new[]
    {
        new Trigger(booths[2], DistanceMeters: 9),
        new Trigger(booths[99], DistanceMeters: 0),
    };

    var winner = Priority.Prioritize(triggers, boosts).First();
    Assert(winner.Booth.Id == 2, "Tour stop wins even when outsider is closer and pays more.");
}

Section("3. Current/next tour stop beats later tour stops");
{
    var state = new TourState { StepHienTai = 2 };
    var boosts = TourPriority.BuildBoosts(tour, state);
    var triggers = new[]
    {
        new Trigger(booths[2], DistanceMeters: 8),
        new Trigger(booths[4], DistanceMeters: 0),
    };

    var winner = Priority.Prioritize(triggers, boosts).First();
    Assert(winner.Booth.Id == 2, "Current/next stop has stronger boost than a later tour stop.");
}

Section("4. Clear boosts after stopping tour");
{
    var triggers = new[]
    {
        new Trigger(booths[2], DistanceMeters: 9),
        new Trigger(booths[99], DistanceMeters: 0),
    };

    var winner = Priority.Prioritize(triggers, new Dictionary<int, int>()).First();
    Assert(winner.Booth.Id == 99, "After stop tour: normal priority returns and outsider wins again.");
}

Section("5. Geofence entry advances tour progress");
{
    var state = new TourState();
    var r1 = TourProgress.Advance(tour, state, idGianHangVuaDen: 1);
    var r2 = TourProgress.Advance(tour, state, idGianHangVuaDen: 2);

    Assert(r1.Success && r1.StepKeTiep == 2, "Enter stop 1 -> next step is 2.");
    Assert(r2.Success && r2.StepKeTiep == 3, "Enter stop 2 -> next step is 3.");
    Assert(!state.Completed, "Tour is still running.");
}

Section("6. Outsider geofence does not advance tour");
{
    var state = new TourState { StepHienTai = 2 };
    var result = TourProgress.Advance(tour, state, idGianHangVuaDen: 99);

    Assert(!result.Success, "Outsider is rejected by Advance.");
    Assert(state.StepHienTai == 2, "Progress is unchanged.");
}

Section("7. Re-entering an old stop never moves progress backward");
{
    var state = new TourState { StepHienTai = 4 };
    var result = TourProgress.Advance(tour, state, idGianHangVuaDen: 2);

    Assert(result.Success, "Old tour stop still belongs to tour.");
    Assert(state.StepHienTai == 4, "StepHienTai remains 4.");
}

Section("8. Last stop completes tour and keeps boosts harmless");
{
    var state = new TourState { StepHienTai = 4 };
    var result = TourProgress.Advance(tour, state, idGianHangVuaDen: 4);

    Assert(result.Success, "Last stop advance succeeds.");
    Assert(result.IsCompleted && state.Completed, "Tour is completed.");
    Assert(result.StepKeTiep == 4 && result.IdGianHangKeTiep is null, "No next booth after final stop.");
}

Section("9. Audio does not play when no geofence is active");
{
    var audio = new MockAudioEngine();
    audio.EvaluateGeofence(Array.Empty<Trigger>(), new Dictionary<int, int>());

    Assert(audio.PlayCount == 0, "No inside geofence -> no audio.");
    Assert(audio.CurrentStoreId is null, "Current audio remains empty.");
}

Section("10. Normal geofence winner plays audio");
{
    var audio = new MockAudioEngine();
    var triggers = new[]
    {
        new Trigger(booths[2], DistanceMeters: 1),
        new Trigger(booths[99], DistanceMeters: 0),
    };

    audio.EvaluateGeofence(triggers, new Dictionary<int, int>());

    Assert(audio.CurrentStoreId == 99, "Normal mode plays outsider winner.");
    Assert(audio.CurrentAudioUrl == "/audio/outsider.mp3", "Outsider audio URL is playing.");
    Assert(audio.PlayCount == 1, "Audio was started once.");
}

Section("11. Active tour geofence plays boosted tour stop audio");
{
    var state = new TourState();
    var boosts = TourPriority.BuildBoosts(tour, state);
    var audio = new MockAudioEngine();
    var triggers = new[]
    {
        new Trigger(booths[2], DistanceMeters: 9),
        new Trigger(booths[99], DistanceMeters: 0),
    };

    audio.EvaluateGeofence(triggers, boosts);

    Assert(audio.CurrentStoreId == 2, "Tour boost makes stop 2 audio win.");
    Assert(audio.CurrentAudioUrl == "/audio/stop-2.mp3", "Stop 2 audio URL is playing.");
}

Section("12. Repeated same geofence tick does not restart same audio");
{
    var audio = new MockAudioEngine();
    var triggers = new[] { new Trigger(booths[1], DistanceMeters: 0) };

    audio.EvaluateGeofence(triggers, new Dictionary<int, int>());
    audio.EvaluateGeofence(triggers, new Dictionary<int, int>());

    Assert(audio.CurrentStoreId == 1, "Stop 1 remains current audio.");
    Assert(audio.PlayCount == 1, "Same current audio is not restarted.");
}

Section("13. Winner without audio is skipped by autoplay");
{
    var audio = new MockAudioEngine();
    var triggers = new[] { new Trigger(booths[98], DistanceMeters: 0) };

    audio.EvaluateGeofence(triggers, new Dictionary<int, int>());

    Assert(audio.CurrentStoreId is null, "Silent booth has no playable audio.");
    Assert(audio.PlayCount == 0, "No audio start for silent booth.");
}

Section("14. After tour stop, normal geofence audio returns");
{
    var audio = new MockAudioEngine();
    var triggers = new[]
    {
        new Trigger(booths[2], DistanceMeters: 9),
        new Trigger(booths[99], DistanceMeters: 0),
    };

    audio.EvaluateGeofence(triggers, new Dictionary<int, int>());

    Assert(audio.CurrentStoreId == 99, "After clearing boosts, outsider audio wins again.");
    Assert(audio.CurrentAudioUrl == "/audio/outsider.mp3", "Normal audio URL is restored.");
}

Console.WriteLine();
Console.WriteLine($"=== Total: {passed} passed, {failed} failed ===");
return failed == 0 ? 0 : 1;

record Booth(int Id, string Name, double Lat, double Lon, double RadiusMeters, decimal MonthlyFee, string? AudioUrl);
record Trigger(Booth Booth, double DistanceMeters);
record TourStop(int ThuTu, int IdGianHang);
record Tour(int IdTour, string Ten, List<TourStop> Stops);
record AdvanceResult(bool Success, int? StepKeTiep, int? IdGianHangKeTiep, bool IsCompleted, string Message);

sealed class TourState
{
    public int StepHienTai { get; set; } = 1;
    public bool Completed { get; set; }
}

static class Priority
{
    public static IOrderedEnumerable<Trigger> Prioritize(
        IEnumerable<Trigger> triggers,
        IReadOnlyDictionary<int, int> priorityBoosts)
    {
        return triggers
            .OrderByDescending(t => priorityBoosts.TryGetValue(t.Booth.Id, out var boost) ? boost : 0)
            .ThenBy(t => t.Booth.RadiusMeters > 0
                ? t.DistanceMeters / t.Booth.RadiusMeters
                : double.MaxValue)
            .ThenByDescending(t => t.Booth.MonthlyFee)
            .ThenBy(t => t.Booth.Id);
    }
}

static class TourPriority
{
    public static Dictionary<int, int> BuildBoosts(Tour tour, TourState state)
    {
        var boosts = tour.Stops.ToDictionary(s => s.IdGianHang, _ => 1000);
        var current = ResolveCurrentStop(tour, state);
        var next = ResolveNextStop(tour, current);

        if (current is not null)
            boosts[current.IdGianHang] = state.StepHienTai <= 1 ? 4000 : 3000;

        if (next is not null)
            boosts[next.IdGianHang] = 4000;

        return boosts;
    }

    private static TourStop? ResolveCurrentStop(Tour tour, TourState state)
    {
        var stops = tour.Stops.OrderBy(s => s.ThuTu).ToList();
        if (stops.Count == 0)
            return null;

        if (state.StepHienTai <= 1)
            return stops[0];

        var nextAvailable = stops.FirstOrDefault(s => s.ThuTu >= state.StepHienTai);
        if (nextAvailable is null)
            return stops[^1];

        return stops.LastOrDefault(s => s.ThuTu < nextAvailable.ThuTu) ?? nextAvailable;
    }

    private static TourStop? ResolveNextStop(Tour tour, TourStop? current)
    {
        if (current is null)
            return null;

        return tour.Stops
            .OrderBy(s => s.ThuTu)
            .FirstOrDefault(s => s.ThuTu > current.ThuTu);
    }
}

static class TourProgress
{
    public static AdvanceResult Advance(Tour tour, TourState state, int idGianHangVuaDen)
    {
        var stop = tour.Stops.FirstOrDefault(s => s.IdGianHang == idGianHangVuaDen);
        if (stop is null)
            return new(false, null, null, false, "Booth is not in tour.");

        var totalStops = tour.Stops.Count;
        var newStep = stop.ThuTu >= totalStops ? totalStops : stop.ThuTu + 1;
        state.StepHienTai = Math.Max(state.StepHienTai, newStep);

        var completed = stop.ThuTu >= totalStops;
        if (completed)
            state.Completed = true;

        int? nextBoothId = null;
        if (!completed)
            nextBoothId = tour.Stops.FirstOrDefault(s => s.ThuTu == state.StepHienTai)?.IdGianHang;

        return new(true, state.StepHienTai, nextBoothId, completed, completed ? "Completed." : "Advanced.");
    }
}

sealed class MockAudioEngine
{
    public int? CurrentStoreId { get; private set; }
    public string? CurrentAudioUrl { get; private set; }
    public int PlayCount { get; private set; }

    public void EvaluateGeofence(
        IReadOnlyCollection<Trigger> currentlyInside,
        IReadOnlyDictionary<int, int> priorityBoosts)
    {
        if (currentlyInside.Count == 0)
            return;

        var winner = Priority.Prioritize(currentlyInside, priorityBoosts).First();
        ScheduleAutoPlay(winner.Booth);
    }

    private void ScheduleAutoPlay(Booth booth)
    {
        if (string.IsNullOrWhiteSpace(booth.AudioUrl))
            return;

        if (CurrentStoreId == booth.Id &&
            string.Equals(CurrentAudioUrl, booth.AudioUrl, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        CurrentStoreId = booth.Id;
        CurrentAudioUrl = booth.AudioUrl;
        PlayCount++;
    }
}
