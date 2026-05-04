// Sandbox: simulate tour flow.
// Visitor picks a 5-stop tour, walks through stops in order. At each stop:
//   - Priority is overridden so the tour's "next stop" wins audio (regardless of fee)
//   - When visitor enters next stop -> AdvanceAsync -> step++
//   - Tour completes after last stop
// Test cases verify state machine + priority override + edge cases (skip, repeat).

int passed = 0, failed = 0;

void Assert(bool ok, string msg)
{
    Console.WriteLine($"  [{(ok ? "PASS" : "FAIL")}] {msg}");
    if (ok) passed++; else failed++;
}

void Section(string title)
{
    Console.WriteLine();
    Console.WriteLine($"=== {title} ===");
}

// Distance helper (planar, since sandbox uses simple coords)
double Dist(double x1, double y1, double x2, double y2) =>
    Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));

// AdvanceAsync mirror — production logic also uses GREATEST so step never goes backward.
(bool ok, int? nextStep, int? nextBoothId, bool completed, string msg) Advance(Tour tour, TourState state, int boothJustEnteredId)
{
    var stop = tour.Stops.FirstOrDefault(s => s.BoothId == boothJustEnteredId);
    if (stop is null) return (false, null, null, false, "Booth khong thuoc tour");

    if (stop.Order > state.CurrentStep)
    {
        for (var s = state.CurrentStep; s < stop.Order; s++)
            state.SkippedStops.Add(s);
    }

    var totalStops = tour.Stops.Count;
    var newStep = stop.Order >= totalStops ? totalStops : stop.Order + 1;
    state.CurrentStep = Math.Max(state.CurrentStep, newStep); // GREATEST: never go backward

    var completed = stop.Order >= totalStops;
    if (completed) state.Completed = true;

    int? nextBoothId = null;
    if (!completed)
    {
        var nextStop = tour.Stops.FirstOrDefault(s => s.Order == state.CurrentStep);
        if (nextStop is not null) nextBoothId = nextStop.BoothId;
    }

    return (true, state.CurrentStep, nextBoothId, completed, completed ? "Hoan thanh" : "Advance OK");
}

// === Setup tour: 5 stops ===
var booths = new[]
{
    new Booth(1, "Welcome",   Lat: 0,   Lon: 0,   RadiusMeters: 5, MonthlyFee: 50_000m,  AudioUrl: "/audio/1.mp3"),
    new Booth(2, "Vietnam",   Lat: 50,  Lon: 0,   RadiusMeters: 5, MonthlyFee: 100_000m, AudioUrl: "/audio/2.mp3"),
    new Booth(3, "Thailand",  Lat: 100, Lon: 30,  RadiusMeters: 5, MonthlyFee: 80_000m,  AudioUrl: "/audio/3.mp3"),
    new Booth(4, "Korea",     Lat: 80,  Lon: 80,  RadiusMeters: 5, MonthlyFee: 200_000m, AudioUrl: "/audio/4.mp3"),
    new Booth(5, "Japan",     Lat: 30,  Lon: 100, RadiusMeters: 5, MonthlyFee: 500_000m, AudioUrl: "/audio/5.mp3"),
    // 1 booth ngoai tour, fee cao -> de test priority override
    new Booth(99, "Outsider", Lat: 50,  Lon: 50,  RadiusMeters: 10, MonthlyFee: 1_000_000m, AudioUrl: "/audio/99.mp3"),
};

var tour = new Tour(1, "Asia Food Tour", new List<TourStop>
{
    new(1, 1, AudioIntroUrl: "/audio/tour1-intro1.mp3"),
    new(2, 2, AudioIntroUrl: null),  // dung audio mac dinh cua gian hang
    new(3, 3, AudioIntroUrl: null),
    new(4, 4, AudioIntroUrl: "/audio/tour1-intro4.mp3"),
    new(5, 5, AudioIntroUrl: null),
});

// === Test cases ===

Section("1. Walk happy path: vao dung thu tu 1->5");
{
    var state = new TourState();
    var visited = new List<int>();
    foreach (var stop in tour.Stops)
    {
        var booth = booths.First(b => b.Id == stop.BoothId);
        var nextBoothId = state.Completed ? (int?)null
            : tour.Stops.First(s => s.Order == state.CurrentStep).BoothId;

        // Visitor di sat tam booth, day cung la booth ngoai tour overlap
        var triggers = booths
            .Select(b => new Trigger(b, Dist(b.Lat, b.Lon, booth.Lat, booth.Lon)))
            .Where(t => t.Distance <= t.Booth.RadiusMeters)
            .ToList();
        var winner = Priority.Prioritize(triggers, nextBoothId).First();
        visited.Add(winner.Booth.Id);

        Advance(tour, state, winner.Booth.Id);
    }
    Assert(visited.SequenceEqual(new[] { 1, 2, 3, 4, 5 }), "Visited theo dung thu tu 1->5");
    Assert(state.Completed, "Tour completed");
    Assert(state.SkippedStops.Count == 0, "Khong skip stop nao");
}

Section("2. Priority override: stop ke tiep thang du Outsider fee 1M");
{
    // Visitor o vi tri (50, 50) - vua trong vung Outsider (radius 10) vua trong vung Vietnam (Lat=50, distance=50)
    // Wait -- Vietnam la (50, 0), distance to (50, 50) = 50. Khong trong vung 5m. Reset:
    // Place visitor inside both Outsider AND Vietnam: visitor at (50, 1) -> distance Vietnam = 1 (in), Outsider distance to (50,50) = ~49 (out)
    // Need to find pos where both overlap. Place Outsider at center (50, 50, r=10). Place visitor at (50, 5) -> Outsider dist=45 (out).
    // Move Outsider to (50, 0) r=15 -> overlap Vietnam (also at 50, 0 r=5).
    var outsider = new Booth(99, "Outsider", 50, 0, 15, 1_000_000m, "/audio/99.mp3");
    var vietnam = booths.First(b => b.Id == 2);
    var visitorAt = (Lat: 50.0, Lon: 0.0);

    var triggers = new[] { vietnam, outsider }
        .Select(b => new Trigger(b, Dist(b.Lat, b.Lon, visitorAt.Lat, visitorAt.Lon)))
        .Where(t => t.Distance <= t.Booth.RadiusMeters)
        .ToList();

    // Khong co tour -> Outsider thang (ratio 0/15=0, fee cao hon)
    var noTourWinner = Priority.Prioritize(triggers, tourNextStopBoothId: null).First();
    Assert(noTourWinner.Booth.Id == 99, $"Khong tour: Outsider fee 1M thang (winner = {noTourWinner.Booth.Name})");

    // Co tour, stop ke tiep = Vietnam (id=2) -> Vietnam thang du fee chi 100K
    var tourWinner = Priority.Prioritize(triggers, tourNextStopBoothId: 2).First();
    Assert(tourWinner.Booth.Id == 2, $"Co tour, stop ke tiep id=2: Vietnam thang (winner = {tourWinner.Booth.Name})");
}

Section("3. Skip behavior: visitor nhay tu stop 1 -> stop 4");
{
    var state = new TourState();
    Advance(tour, state, 1); // step 1 done -> currentStep = 2
    var result = Advance(tour, state, 4); // jump to stop 4
    Assert(state.SkippedStops.SequenceEqual(new[] { 2, 3 }), $"Skipped 2,3 (actual: {string.Join(",", state.SkippedStops)})");
    Assert(state.CurrentStep == 5, $"CurrentStep = 5 (actual: {state.CurrentStep})");
    Assert(!state.Completed, "Chua completed (con stop 5)");
}

Section("4. Replay stop da nghe: khong advance lui");
{
    var state = new TourState { CurrentStep = 4 }; // dang o stop 4
    var r = Advance(tour, state, 2); // quay ve stop 2
    // GREATEST(currentStep, newStep) -> currentStep van 4
    // (Trong sandbox sample tren, neu dung Advance moi voi stop 2 thi state.CurrentStep tro thanh 3 -- BUG!)
    // Verify: advance phai khong giam currentStep
    Assert(r.ok, "Advance OK");
    // Trong logic Advance hien tai cua sandbox: stop.Order < currentStep (2 < 4) -> set currentStep = 3 -> SAI
    // De fix: chi advance khi stop.Order >= currentStep
    // Trong production: SQL dung GREATEST de bao ve. Sandbox can mirror.
    // -> Test nay flag thieu sot:
    Assert(state.CurrentStep >= 4, $"CurrentStep khong duoc lui (actual: {state.CurrentStep}). Production SQL dung GREATEST nen OK, sandbox phai mirror.");
}

Section("5. Hoan thanh: vao stop cuoi 5");
{
    var state = new TourState { CurrentStep = 5 };
    var r = Advance(tour, state, 5);
    Assert(r.completed, "Tour completed = true");
    Assert(state.Completed, "State.Completed = true");
    Assert(r.nextBoothId is null, "Khong con next stop");
}

Section("6. Booth ngoai tour: Advance reject");
{
    var state = new TourState();
    var r = Advance(tour, state, 99);
    Assert(!r.ok, "Reject khi booth khong thuoc tour");
    Assert(state.CurrentStep == 1, "State khong thay doi");
}

Section("7. Audio resolve: stop 1 dung audioIntro, stop 2 dung audio mac dinh");
{
    string ResolveAudio(TourStop stop, Booth booth) =>
        !string.IsNullOrEmpty(stop.AudioIntroUrl) ? stop.AudioIntroUrl! : booth.AudioUrl;

    var stop1 = tour.Stops.First(s => s.Order == 1);
    var stop2 = tour.Stops.First(s => s.Order == 2);
    var audio1 = ResolveAudio(stop1, booths.First(b => b.Id == stop1.BoothId));
    var audio2 = ResolveAudio(stop2, booths.First(b => b.Id == stop2.BoothId));
    Assert(audio1 == "/audio/tour1-intro1.mp3", $"Stop 1 dung tour intro (actual: {audio1})");
    Assert(audio2 == "/audio/2.mp3", $"Stop 2 fallback audio gian hang (actual: {audio2})");
}

Section("8. Lazy prefetch tour-aware: prefetch theo thu tu tour, khong theo distance");
{
    var state = new TourState { CurrentStep = 2 };
    int topN = 3;
    var prefetchOrder = tour.Stops
        .Where(s => s.Order >= state.CurrentStep)
        .OrderBy(s => s.Order)
        .Take(topN)
        .Select(s => s.BoothId)
        .ToList();
    Assert(prefetchOrder.SequenceEqual(new[] { 2, 3, 4 }), $"Prefetch booth 2,3,4 (actual: {string.Join(",", prefetchOrder)})");
}

Console.WriteLine();
Console.WriteLine($"=== Total: {passed} passed, {failed} failed ===");
return failed == 0 ? 0 : 1;

// === Domain types (mirror VinhKhanh DTOs) ===
record Booth(int Id, string Name, double Lat, double Lon, double RadiusMeters, decimal MonthlyFee, string AudioUrl);
record TourStop(int Order, int BoothId, string? AudioIntroUrl);
record Tour(int Id, string Name, List<TourStop> Stops);
record Trigger(Booth Booth, double Distance);

class TourState
{
    public int CurrentStep { get; set; } = 1;
    public bool Completed { get; set; }
    public List<int> SkippedStops { get; set; } = new();
}

static class Priority
{
    public static IOrderedEnumerable<Trigger> Prioritize(IEnumerable<Trigger> triggers, int? tourNextStopBoothId)
    {
        return triggers
            .OrderByDescending(t => tourNextStopBoothId.HasValue && t.Booth.Id == tourNextStopBoothId.Value)
            .ThenBy(t => t.Booth.RadiusMeters > 0 ? t.Distance / t.Booth.RadiusMeters : double.MaxValue)
            .ThenByDescending(t => t.Booth.MonthlyFee)
            .ThenBy(t => t.Booth.Id);
    }
}
