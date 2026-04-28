// Sandbox test for GeofenceEngineService.PrioritizeGeofenceTargets
// Mirrors the production logic with minimal stand-ins so we can run without MAUI.

int passed = 0, failed = 0;

void Case(string label, IEnumerable<Trigger> input, int expectedWinnerId, string reason)
{
    var winner = Priority.Prioritize(input).First();
    var ok = winner.Target.Id == expectedWinnerId;
    var ratios = string.Join(", ", input.Select(t =>
        $"{t.Target.Name}(r={t.Target.RadiusMeters}m, fee={t.Target.MonthlyFee}, d={t.DistanceMeters}m, ratio={(t.Target.RadiusMeters > 0 ? t.DistanceMeters / t.Target.RadiusMeters : double.NaN):F3})"));
    Console.WriteLine($"[{(ok ? "PASS" : "FAIL")}] {label}");
    Console.WriteLine($"        targets: {ratios}");
    Console.WriteLine($"        winner : {winner.Target.Name} (id={winner.Target.Id})");
    Console.WriteLine($"        expect : id={expectedWinnerId} — {reason}");
    Console.WriteLine();
    if (ok) passed++; else failed++;
}

Case("1. Kiosk nho overlap FoodCourt lon - du khach gan tam Kiosk",
    new[] {
        new Trigger(new Target(1, "Kiosk",     RadiusMeters: 5,  MonthlyFee: 50_000m),  DistanceMeters: 2),
        new Trigger(new Target(2, "FoodCourt", RadiusMeters: 30, MonthlyFee: 200_000m), DistanceMeters: 15),
    },
    expectedWinnerId: 1,
    reason: "Kiosk ratio=0.40 < FoodCourt ratio=0.50, kiosk thang du fee thap");

Case("2. Hai gian cung radius, du khach dung dung giua => fee tie-break",
    new[] {
        new Trigger(new Target(1, "GianA-100K", RadiusMeters: 10, MonthlyFee: 100_000m), DistanceMeters: 5),
        new Trigger(new Target(2, "GianB-200K", RadiusMeters: 10, MonthlyFee: 200_000m), DistanceMeters: 5),
    },
    expectedWinnerId: 2,
    reason: "Ratio bang nhau (0.5), fee cao hon thang");

Case("3. Cung radius, cung fee, gian gan hon thang",
    new[] {
        new Trigger(new Target(1, "Gan",  RadiusMeters: 10, MonthlyFee: 100_000m), DistanceMeters: 4),
        new Trigger(new Target(2, "Xa",   RadiusMeters: 10, MonthlyFee: 100_000m), DistanceMeters: 6),
    },
    expectedWinnerId: 1,
    reason: "Ratio 0.4 < 0.6 (distance/radius da bao ham distance khi radius bang)");

Case("4. Gian fee cao nhung dung sat ria, gian fee thap nhung dung giua",
    new[] {
        new Trigger(new Target(1, "GianRia-VIP",  RadiusMeters: 20, MonthlyFee: 500_000m), DistanceMeters: 19),
        new Trigger(new Target(2, "GianGiua-Re",  RadiusMeters: 20, MonthlyFee: 50_000m),  DistanceMeters: 3),
    },
    expectedWinnerId: 2,
    reason: "0.95 vs 0.15 — du khach ro rang dang o GianGiua, fee khong cuu duoc");

Case("5. Tat ca tie tuyet doi => Id nho thang (final tie-break)",
    new[] {
        new Trigger(new Target(7, "B", RadiusMeters: 10, MonthlyFee: 100_000m), DistanceMeters: 5),
        new Trigger(new Target(3, "A", RadiusMeters: 10, MonthlyFee: 100_000m), DistanceMeters: 5),
    },
    expectedWinnerId: 3,
    reason: "Id=3 < Id=7");

Case("6. Edge: radius=0 (du lieu hong) khong duoc lam div-by-zero",
    new[] {
        new Trigger(new Target(1, "GianHong", RadiusMeters: 0,  MonthlyFee: 999_000m), DistanceMeters: 0),
        new Trigger(new Target(2, "GianOK",   RadiusMeters: 10, MonthlyFee: 50_000m),  DistanceMeters: 9),
    },
    expectedWinnerId: 2,
    reason: "Radius=0 bi day xuong cuoi (ratio=double.MaxValue), GianOK thang du fee thap");

Case("7. Mot gian duy nhat bao trum",
    new[] {
        new Trigger(new Target(42, "Solo", RadiusMeters: 15, MonthlyFee: 100_000m), DistanceMeters: 7),
    },
    expectedWinnerId: 42,
    reason: "Khong co competitor, gian duy nhat thang");

Case("8. Du khach o sat tam ca hai (ratio gan 0), fee quyet dinh",
    new[] {
        new Trigger(new Target(1, "Tam-A", RadiusMeters: 10, MonthlyFee: 80_000m),  DistanceMeters: 0.5),
        new Trigger(new Target(2, "Tam-B", RadiusMeters: 10, MonthlyFee: 300_000m), DistanceMeters: 0.5),
    },
    expectedWinnerId: 2,
    reason: "Ratio 0.05 == 0.05, fee 300K thang");

// === Re-evaluation scenarios: simulate per-tick orchestration ===
// Production code now passes the FULL inside set to Prioritize() each tick,
// not only newly-entered triggers. These cases lock in that behavior.

void ReEvalCase(string label, IEnumerable<Trigger> oldImpl_newlyEnteredOnly, IEnumerable<Trigger> newImpl_fullInsideSet, int expectedWinnerId, string reason)
{
    var oldWinner = Priority.Prioritize(oldImpl_newlyEnteredOnly).First();
    var newWinner = Priority.Prioritize(newImpl_fullInsideSet).First();
    var ok = newWinner.Target.Id == expectedWinnerId;
    Console.WriteLine($"[{(ok ? "PASS" : "FAIL")}] {label}");
    Console.WriteLine($"        old (newly-entered only) -> {oldWinner.Target.Name} (id={oldWinner.Target.Id})");
    Console.WriteLine($"        new (full inside set)    -> {newWinner.Target.Name} (id={newWinner.Target.Id})");
    Console.WriteLine($"        expect: id={expectedWinnerId} — {reason}");
    Console.WriteLine();
    if (ok) passed++; else failed++;
}

// Tick N: visitor was already inside Kiosk (deep, ratio 0.2), now also enters FoodCourt edge (ratio 0.6).
// Old impl: newlyEntered = [FoodCourt] -> FoodCourt wins (wrong).
// New impl: insideSet = [Kiosk, FoodCourt] -> Kiosk wins (ratio 0.2 < 0.6).
ReEvalCase("9. Da o trong Kiosk roi moi vao FoodCourt -> Kiosk van thang",
    oldImpl_newlyEnteredOnly: new[] {
        new Trigger(new Target(2, "FoodCourt", RadiusMeters: 30, MonthlyFee: 200_000m), DistanceMeters: 18),
    },
    newImpl_fullInsideSet: new[] {
        new Trigger(new Target(1, "Kiosk",     RadiusMeters: 5,  MonthlyFee: 50_000m),  DistanceMeters: 1),
        new Trigger(new Target(2, "FoodCourt", RadiusMeters: 30, MonthlyFee: 200_000m), DistanceMeters: 18),
    },
    expectedWinnerId: 1,
    reason: "Re-eval moi tick chon Kiosk dua tren full inside set, khong bi anh huong boi thu tu nhap vung");

// Tick N: visitor was already inside FoodCourt edge, now enters deep Kiosk.
// Old impl: newlyEntered = [Kiosk] -> Kiosk wins (correct by luck).
// New impl: insideSet = [FoodCourt, Kiosk] -> Kiosk still wins.
// This case shows new impl doesn't break the case that already worked.
ReEvalCase("10. Da o trong FoodCourt roi vao Kiosk -> Kiosk van thang (khong regression)",
    oldImpl_newlyEnteredOnly: new[] {
        new Trigger(new Target(1, "Kiosk",     RadiusMeters: 5,  MonthlyFee: 50_000m),  DistanceMeters: 1),
    },
    newImpl_fullInsideSet: new[] {
        new Trigger(new Target(2, "FoodCourt", RadiusMeters: 30, MonthlyFee: 200_000m), DistanceMeters: 18),
        new Trigger(new Target(1, "Kiosk",     RadiusMeters: 5,  MonthlyFee: 50_000m),  DistanceMeters: 1),
    },
    expectedWinnerId: 1,
    reason: "Du thu tu nhap khac, ket qua giong nhau — priority deterministic theo full set");

Console.WriteLine($"=== Total: {passed} passed, {failed} failed ===");
return failed == 0 ? 0 : 1;

record Target(int Id, string Name, double RadiusMeters, decimal MonthlyFee);
record Trigger(Target Target, double DistanceMeters);

static class Priority
{
    public static IOrderedEnumerable<Trigger> Prioritize(IEnumerable<Trigger> targets)
    {
        return targets
            .OrderBy(x => x.Target.RadiusMeters > 0
                ? x.DistanceMeters / x.Target.RadiusMeters
                : double.MaxValue)
            .ThenByDescending(x => x.Target.MonthlyFee)
            .ThenBy(x => x.Target.Id);
    }
}
