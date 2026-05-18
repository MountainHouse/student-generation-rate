using SchoolGrowth.Core;

var options = CliOptions.Parse(args);
if (options.ShowHelp)
{
    PrintHelp();
    return;
}

var dataRoot = ResolveDataRoot(options.DataRoot);
var data = EnrollmentData.Load(dataRoot);
var model = new MonteCarloEnrollmentModel(data);

if (options.Command.Equals("search", StringComparison.OrdinalIgnoreCase))
{
    RunSearch(model, options);
}
else if (options.Command.Equals("optimize", StringComparison.OrdinalIgnoreCase))
{
    RunOptimize(model, options);
}
else if (options.Command.Equals("lifecycle", StringComparison.OrdinalIgnoreCase))
{
    RunLifecycle(model, options);
}
else
{
    RunValidation(model, options);
}

static void RunValidation(MonteCarloEnrollmentModel model, CliOptions options)
{
    var result = model.Validate(new MonteCarloValidationRequest(
        options.StartYear,
        options.EndYear,
        options.Parameters));

    PrintResult(result, options.ShowGradeDetails);
}

static void RunSearch(MonteCarloEnrollmentModel model, CliOptions options)
{
    var candidates = BuildCandidates(options).ToList();
    if (candidates.Count == 0)
    {
        Console.Error.WriteLine("No search candidates were provided.");
        Environment.ExitCode = 1;
        return;
    }

    MonteCarloValidationResult? best = null;
    var results = new List<MonteCarloValidationResult>();

    for (var i = 0; i < candidates.Count; i++)
    {
        var parameters = candidates[i] with
        {
        Runs = options.SearchRuns,
        Seed = options.Seed + i * 7919,
        MaxDegreeOfParallelism = options.Parallelism,
        GradeSmoothingWindow = options.GradeSmoothingWindow
        };
        var result = model.Validate(new MonteCarloValidationRequest(options.StartYear, options.EndYear, parameters));
        results.Add(result);

        if (best is null || result.CombinedScore < best.CombinedScore)
        {
            best = result;
        }

        Console.WriteLine(
            "[{0,4}/{1,4}] score={2:P2} total={3:P1} grid-detail={4:P1} grade-detail={5:P1} hs-total={6:P1} hs-detail={7:P1} turnover={8:P1} move-in={9:P1}/{10:P1}/{11:P1}/{12:P1}/{13:P1} births={14:P1}/{15:P1}/{16:P1}/{17:P1} exit={18:P1}",
            i + 1,
            candidates.Count,
            result.CombinedScore,
            result.GridMeanAbsolutePercentageError,
            result.GridLevelMeanAbsolutePercentageError,
            result.GradeLevelMeanAbsolutePercentageError,
            result.HighSchoolMeanAbsolutePercentageError,
            result.HighSchoolGradeMeanAbsolutePercentageError,
            parameters.OwnershipChangeProbability,
            parameters.MoveInZeroChildShare,
            parameters.MoveInOneChildShare,
            parameters.MoveInTwoChildShare,
            parameters.MoveInThreeChildShare,
            parameters.MoveInFourPlusChildShare,
            parameters.AnnualFirstNewChildProbability,
            parameters.AnnualSecondNewChildProbability,
            parameters.AnnualThirdNewChildProbability,
            parameters.AnnualFourthPlusNewChildProbability,
            parameters.StudentExitProbability);
    }

    Console.WriteLine();
    Console.WriteLine("Best candidate");
    PrintResult(best!, options.ShowGradeDetails);

    if (!string.IsNullOrWhiteSpace(options.OutputPath))
    {
        WriteCsv(options.OutputPath, results);
        Console.WriteLine();
        Console.WriteLine($"Wrote candidate results to {options.OutputPath}");
    }
}

static void RunOptimize(MonteCarloEnrollmentModel model, CliOptions options)
{
    var variables = BuildOptimizeVariables(options).ToList();
    var results = new List<MonteCarloValidationResult>();
    var evaluation = 0;
    var startingParameters = options.Parameters with
    {
        OwnershipChangeProbability = Math.Min(
            options.Parameters.OwnershipChangeProbability,
            Math.Max(0.035, options.MaxOptimizeTurnover))
    };

    var best = EvaluateOptimizeCandidate(model, options, startingParameters, ++evaluation);
    results.Add(best);

    Console.WriteLine(
        "[eval {0,4}] initial score={1:P2} total={2:P1} grid-detail={3:P1} grade-detail={4:P1} hs-total={5:P1} hs-detail={6:P1}",
        evaluation,
        best.CombinedScore,
        best.GridMeanAbsolutePercentageError,
        best.GridLevelMeanAbsolutePercentageError,
        best.GradeLevelMeanAbsolutePercentageError,
        best.HighSchoolMeanAbsolutePercentageError,
        best.HighSchoolGradeMeanAbsolutePercentageError);

    var current = best.Parameters;
    for (var iteration = 0; iteration < options.OptimizeIterations; iteration++)
    {
        var scale = Math.Pow(0.5, iteration);
        var improved = false;
        Console.WriteLine();
        Console.WriteLine($"Iteration {iteration + 1:N0}/{options.OptimizeIterations:N0}, step scale {scale:N3}");

        foreach (var variable in variables)
        {
            var baseValue = variable.Get(current);
            foreach (var direction in new[] { 1.0, -1.0 })
            {
                var nextValue = Math.Clamp(baseValue + direction * variable.InitialStep * scale, variable.Minimum, variable.Maximum);
                if (Math.Abs(nextValue - baseValue) < 0.0000001)
                {
                    continue;
                }

                var candidateParameters = variable.Set(current, nextValue);
                var result = EvaluateOptimizeCandidate(model, options, candidateParameters, ++evaluation);
                results.Add(result);

                var directionText = direction > 0 ? "+" : "-";
                var status = result.CombinedScore < best.CombinedScore ? "keep" : "skip";
                Console.WriteLine(
                    "[eval {0,4}] {1,-13} {2}{3:N4} score={4:P2} total={5:P1} grid-detail={6:P1} grade-detail={7:P1} hs-total={8:P1} hs-detail={9:P1} {10}",
                    evaluation,
                    variable.Name,
                    directionText,
                    Math.Abs(nextValue - baseValue),
                    result.CombinedScore,
                    result.GridMeanAbsolutePercentageError,
                    result.GridLevelMeanAbsolutePercentageError,
                    result.GradeLevelMeanAbsolutePercentageError,
                    result.HighSchoolMeanAbsolutePercentageError,
                    result.HighSchoolGradeMeanAbsolutePercentageError,
                    status);

                if (result.CombinedScore >= best.CombinedScore)
                {
                    continue;
                }

                best = result;
                current = result.Parameters;
                improved = true;
                baseValue = variable.Get(current);
            }
        }

        if (!improved)
        {
            Console.WriteLine("No improvement at this scale; shrinking steps.");
        }
    }

    Console.WriteLine();
    Console.WriteLine("Best optimized candidate");
    PrintResult(best, options.ShowGradeDetails);

    if (!string.IsNullOrWhiteSpace(options.OutputPath))
    {
        WriteCsv(options.OutputPath, results);
        Console.WriteLine();
        Console.WriteLine($"Wrote optimization trace to {options.OutputPath}");
    }
}

static MonteCarloValidationResult EvaluateOptimizeCandidate(
    MonteCarloEnrollmentModel model,
    CliOptions options,
    MonteCarloParameters parameters,
    int evaluation)
{
    var candidate = parameters with
    {
        Runs = options.SearchRuns,
        Seed = options.Seed,
        MaxDegreeOfParallelism = options.Parallelism,
        GradeSmoothingWindow = options.GradeSmoothingWindow
    };

    return model.Validate(new MonteCarloValidationRequest(options.StartYear, options.EndYear, candidate));
}

static void RunLifecycle(MonteCarloEnrollmentModel model, CliOptions options)
{
    var result = model.SimulateLifecycle(new MonteCarloLifecycleRequest(
        options.LifecycleYears,
        options.Parameters.Runs,
        options.HomesPerRun,
        options.Grid,
        options.Density,
        options.Parameters));

    Console.WriteLine("Parameters");
    Console.WriteLine($"  runs:       {result.Parameters.Runs:N0}");
    Console.WriteLine($"  homes/run:  {options.HomesPerRun:N0}");
    Console.WriteLine($"  grid:       {options.Grid}");
    Console.WriteLine($"  density:    {options.Density}");
    Console.WriteLine($"  turnover:   {result.Parameters.OwnershipChangeProbability:P2}");
    Console.WriteLine($"  realized:   {result.RealizedTurnoverRate:P2}");
    Console.WriteLine($"  move-in:    0 child {result.Parameters.MoveInZeroChildShare:P2}, 1 child {result.Parameters.MoveInOneChildShare:P2}, 2 child {result.Parameters.MoveInTwoChildShare:P2}, 3 child {result.Parameters.MoveInThreeChildShare:P2}, 4 child {result.Parameters.MoveInFourPlusChildShare:P2}");
    Console.WriteLine($"  births/yr:  1st {result.Parameters.AnnualFirstNewChildProbability:P2}, 2nd {result.Parameters.AnnualSecondNewChildProbability:P2}, 3rd {result.Parameters.AnnualThirdNewChildProbability:P2}, 4th+ {result.Parameters.AnnualFourthPlusNewChildProbability:P2}");
    Console.WriteLine($"  exits:      TK-8 {result.Parameters.Tk8ExitProbability:P2}, HS {result.Parameters.HighSchoolExitProbability:P2}, SpEd {result.Parameters.SpecialExitProbability:P2}");
    Console.WriteLine($"  weights:    pre-school {result.Parameters.MoveInPreschoolWeight:0.###}, TK/K {result.Parameters.MoveInTkKWeight:0.###}, elem {result.Parameters.MoveInElementaryWeight:0.###}, middle {result.Parameters.MoveInMiddleWeight:0.###}, high {result.Parameters.MoveInHighWeight:0.###}, post-school {result.Parameters.MoveInPostSchoolWeight:0.###}");
    Console.WriteLine();
    PrintDistribution("Initial move-in household child count", result.InitialHouseholds);
    PrintDistribution("Turnover replacement household child count", result.TurnoverHouseholds);
    PrintDistribution("Ending active household child count", result.EndingHouseholds);
    PrintTurnoverByYear(result.TurnoverByYear);
    PrintFamilyLongevity("Completed family longevity", result.CompletedFamilyLongevity);
    PrintFamilyLongevity("Active-at-end family longevity", result.ActiveAtEndFamilyLongevity);
    Console.WriteLine();
    Console.WriteLine("Years   Students/Home   TK-8/Home    HS/Home   SpEd/Home Children/Home  0 child  1 child  2 child  3 child    4 child");
    foreach (var year in result.Years)
    {
        Console.WriteLine(
            "{0,5} {1,15:N3} {2,11:N3} {3,10:N3} {4,11:N3} {5,13:N3} {6,8:P1} {7,8:P1} {8,8:P1} {9,8:P1} {10,10:P1}",
            year.YearsAfterMoveIn,
            year.StudentsPerHome,
            year.Tk8StudentsPerHome,
            year.HighSchoolStudentsPerHome,
            year.SpecialEducationStudentsPerHome,
            year.ChildrenPerHome,
            year.HomesWithoutChildrenShare,
            year.HomesWithOneChildShare,
            year.HomesWithTwoChildrenShare,
            year.HomesWithThreeChildrenShare,
            year.HomesWithFourPlusChildrenShare);
    }
}

static void PrintTurnoverByYear(IReadOnlyList<MonteCarloTurnoverYear> turnover)
{
    Console.WriteLine("Realized turnover by simulation year");
    Console.WriteLine("  Year Active homes  Turnovers       Rate");
    foreach (var item in turnover)
    {
        Console.WriteLine("  {0,4} {1,12:N0} {2,10:N0} {3,10:P2}", item.Year, item.ActiveHomes, item.TurnoverEvents, item.TurnoverRate);
    }
}

static void PrintFamilyLongevity(string label, IReadOnlyList<MonteCarloFamilyLongevityYear> longevity)
{
    Console.WriteLine(label);
    Console.WriteLine("  Years  Families       Share");
    foreach (var item in longevity)
    {
        Console.WriteLine("  {0,5} {1,9:N0} {2,11:P2}", item.YearsInHome, item.FamilyCount, item.Share);
    }
}

static void PrintDistribution(string label, MonteCarloChildCountDistribution distribution)
{
    Console.WriteLine(label);
    Console.WriteLine(
        "  households: {0:N0}; 0 child {1:P1}, 1 child {2:P1}, 2 child {3:P1}, 3 child {4:P1}, 4 child {5:P1}",
        distribution.HouseholdCount,
        distribution.HomesWithoutChildrenShare,
        distribution.HomesWithOneChildShare,
        distribution.HomesWithTwoChildrenShare,
        distribution.HomesWithThreeChildrenShare,
        distribution.HomesWithFourPlusChildrenShare);
}

static IEnumerable<MonteCarloParameters> BuildCandidates(CliOptions options)
{
    foreach (var turnover in options.OwnershipCandidates.DefaultIfEmpty(options.Parameters.OwnershipChangeProbability))
    foreach (var zero in options.ZeroChildCandidates.DefaultIfEmpty(options.Parameters.MoveInZeroChildShare))
    foreach (var one in options.OneChildCandidates.DefaultIfEmpty(options.Parameters.MoveInOneChildShare))
    foreach (var two in options.TwoChildCandidates.DefaultIfEmpty(options.Parameters.MoveInTwoChildShare))
    foreach (var three in options.ThreeChildCandidates.DefaultIfEmpty(options.Parameters.MoveInThreeChildShare))
    foreach (var fourPlus in options.FourPlusChildCandidates.DefaultIfEmpty(options.Parameters.MoveInFourPlusChildShare))
    foreach (var exit in options.ExitCandidates.DefaultIfEmpty(options.Parameters.StudentExitProbability))
    foreach (var firstBirth in options.FirstBirthCandidates.DefaultIfEmpty(options.Parameters.AnnualFirstNewChildProbability))
    foreach (var secondBirth in options.SecondBirthCandidates.DefaultIfEmpty(options.Parameters.AnnualSecondNewChildProbability))
    foreach (var thirdBirth in options.ThirdBirthCandidates.DefaultIfEmpty(options.Parameters.AnnualThirdNewChildProbability))
    foreach (var fourthBirth in options.FourthBirthCandidates.DefaultIfEmpty(options.Parameters.AnnualFourthPlusNewChildProbability))
    foreach (var tk8Exit in options.Tk8ExitCandidates.DefaultIfEmpty(exit))
    foreach (var highExit in options.HighExitCandidates.DefaultIfEmpty(exit))
    foreach (var specialExit in options.SpecialExitCandidates.DefaultIfEmpty(exit))
    foreach (var sameYear in options.SameYearCandidates.DefaultIfEmpty(options.Parameters.SameSchoolYearProbability))
    {
        yield return options.Parameters with
        {
            OwnershipChangeProbability = turnover,
            MoveInZeroChildShare = zero,
            MoveInOneChildShare = one,
            MoveInTwoChildShare = two,
            MoveInThreeChildShare = three,
            MoveInFourPlusChildShare = fourPlus,
            StudentExitProbability = exit,
            AnnualFirstNewChildProbability = firstBirth,
            AnnualSecondNewChildProbability = secondBirth,
            AnnualThirdNewChildProbability = thirdBirth,
            AnnualFourthPlusNewChildProbability = fourthBirth,
            Tk8ExitProbability = tk8Exit,
            HighSchoolExitProbability = highExit,
            SpecialExitProbability = specialExit,
            SameSchoolYearProbability = sameYear
        };
    }
}

static IEnumerable<OptimizeVariable> BuildOptimizeVariables(CliOptions options)
{
    yield return new OptimizeVariable(
        "turnover",
        0.01,
        0.035,
        Math.Max(0.035, options.MaxOptimizeTurnover),
        p => p.OwnershipChangeProbability,
        (p, value) => p with { OwnershipChangeProbability = value });

    yield return new OptimizeVariable(
        "zero-child",
        0.02,
        0.01,
        0.12,
        p => p.MoveInZeroChildShare,
        (p, value) => p with { MoveInZeroChildShare = value });

    yield return new OptimizeVariable(
        "one-child",
        0.05,
        0.10,
        0.45,
        p => p.MoveInOneChildShare,
        (p, value) => p with { MoveInOneChildShare = value });

    yield return new OptimizeVariable(
        "two-child",
        0.05,
        0.45,
        0.82,
        p => p.MoveInTwoChildShare,
        (p, value) => p with { MoveInTwoChildShare = value });

    yield return new OptimizeVariable(
        "three-child",
        0.02,
        0.00,
        0.08,
        p => p.MoveInThreeChildShare,
        (p, value) => p with { MoveInThreeChildShare = value });

    yield return new OptimizeVariable(
        "four-plus-child",
        0.005,
        0.00,
        0.03,
        p => p.MoveInFourPlusChildShare,
        (p, value) => p with { MoveInFourPlusChildShare = value });

    yield return new OptimizeVariable(
        "first-birth",
        0.01,
        0.02,
        0.09,
        p => p.AnnualFirstNewChildProbability,
        (p, value) => p with { AnnualFirstNewChildProbability = value });

    yield return new OptimizeVariable(
        "second-birth",
        0.0075,
        0.01,
        0.06,
        p => p.AnnualSecondNewChildProbability,
        (p, value) => p with { AnnualSecondNewChildProbability = value });

    yield return new OptimizeVariable(
        "third-birth",
        0.001,
        0.00,
        0.006,
        p => p.AnnualThirdNewChildProbability,
        (p, value) => p with { AnnualThirdNewChildProbability = value });

    yield return new OptimizeVariable(
        "fourth-birth",
        0.0005,
        0.00,
        0.003,
        p => p.AnnualFourthPlusNewChildProbability,
        (p, value) => p with { AnnualFourthPlusNewChildProbability = value });

    yield return new OptimizeVariable(
        "tk8-exit",
        0.0025,
        0.00,
        0.02,
        p => p.Tk8ExitProbability,
        (p, value) => p with { Tk8ExitProbability = value });

    yield return new OptimizeVariable(
        "high-exit",
        0.0025,
        0.00,
        0.02,
        p => p.HighSchoolExitProbability,
        (p, value) => p with { HighSchoolExitProbability = value });

    yield return new OptimizeVariable(
        "special-exit",
        0.0025,
        0.00,
        0.03,
        p => p.SpecialExitProbability,
        (p, value) => p with { SpecialExitProbability = value });

    yield return new OptimizeVariable(
        "special-prob",
        0.0025,
        0.002,
        0.03,
        p => p.SpecialEducationProbability,
        (p, value) => p with { SpecialEducationProbability = value });

    yield return new OptimizeVariable(
        "same-year",
        0.05,
        0.25,
        0.65,
        p => p.SameSchoolYearProbability,
        (p, value) => p with { SameSchoolYearProbability = value });

    yield return new OptimizeVariable(
        "preschool-wt",
        0.05,
        0.10,
        1.00,
        p => p.MoveInPreschoolWeight,
        (p, value) => p with { MoveInPreschoolWeight = value });

    yield return new OptimizeVariable(
        "tkk-wt",
        0.04,
        0.05,
        0.60,
        p => p.MoveInTkKWeight,
        (p, value) => p with { MoveInTkKWeight = value });

    yield return new OptimizeVariable(
        "elem-wt",
        0.05,
        0.20,
        1.20,
        p => p.MoveInElementaryWeight,
        (p, value) => p with { MoveInElementaryWeight = value });

    yield return new OptimizeVariable(
        "middle-wt",
        0.04,
        0.05,
        0.70,
        p => p.MoveInMiddleWeight,
        (p, value) => p with { MoveInMiddleWeight = value });

    yield return new OptimizeVariable(
        "high-wt",
        0.04,
        0.03,
        0.70,
        p => p.MoveInHighWeight,
        (p, value) => p with { MoveInHighWeight = value });

    yield return new OptimizeVariable(
        "postschool-wt",
        0.01,
        0.00,
        0.20,
        p => p.MoveInPostSchoolWeight,
        (p, value) => p with { MoveInPostSchoolWeight = value });

    yield return new OptimizeVariable(
        "density-low",
        0.10,
        0.80,
        1.40,
        p => p.DensityLowMediumFactor,
        (p, value) => p with { DensityLowMediumFactor = value });

    yield return new OptimizeVariable(
        "density-rmh",
        0.10,
        0.40,
        1.20,
        p => p.DensityMediumHighFactor,
        (p, value) => p with { DensityMediumHighFactor = value });

    yield return new OptimizeVariable(
        "density-high",
        0.10,
        0.20,
        1.10,
        p => p.DensityHighFactor,
        (p, value) => p with { DensityHighFactor = value });

    yield return new OptimizeVariable("low-1st", 0.10, 0.80, 1.50, p => p.DensityLowMediumFirstChildFactor, (p, value) => p with { DensityLowMediumFirstChildFactor = value });
    yield return new OptimizeVariable("low-2nd", 0.10, 0.80, 1.50, p => p.DensityLowMediumSecondChildFactor, (p, value) => p with { DensityLowMediumSecondChildFactor = value });
    yield return new OptimizeVariable("low-3rd", 0.10, 0.50, 1.50, p => p.DensityLowMediumThirdChildFactor, (p, value) => p with { DensityLowMediumThirdChildFactor = value });
    yield return new OptimizeVariable("rmh-1st", 0.10, 0.70, 1.30, p => p.DensityMediumHighFirstChildFactor, (p, value) => p with { DensityMediumHighFirstChildFactor = value });
    yield return new OptimizeVariable("rmh-2nd", 0.10, 0.40, 1.20, p => p.DensityMediumHighSecondChildFactor, (p, value) => p with { DensityMediumHighSecondChildFactor = value });
    yield return new OptimizeVariable("rmh-3rd", 0.05, 0.00, 0.80, p => p.DensityMediumHighThirdChildFactor, (p, value) => p with { DensityMediumHighThirdChildFactor = value });
    yield return new OptimizeVariable("high-1st", 0.10, 0.60, 1.30, p => p.DensityHighFirstChildFactor, (p, value) => p with { DensityHighFirstChildFactor = value });
    yield return new OptimizeVariable("high-2nd", 0.10, 0.20, 1.20, p => p.DensityHighSecondChildFactor, (p, value) => p with { DensityHighSecondChildFactor = value });
    yield return new OptimizeVariable("high-3rd", 0.05, 0.00, 0.60, p => p.DensityHighThirdChildFactor, (p, value) => p with { DensityHighThirdChildFactor = value });
}

static void PrintResult(MonteCarloValidationResult result, bool showGradeDetails = false)
{
    Console.WriteLine("Parameters");
    Console.WriteLine($"  runs:       {result.Parameters.Runs:N0}");
    Console.WriteLine($"  seed:       {result.Parameters.Seed}");
    Console.WriteLine($"  parallel:   {FormatParallelism(result.Parameters.MaxDegreeOfParallelism)}");
    Console.WriteLine($"  grade win:  +/-{result.Parameters.GradeSmoothingWindow:N0} years");
    Console.WriteLine($"  score wts:  total {result.Parameters.ScoreTotalWeight:N2}, grid {result.Parameters.ScoreGridWeight:N2}, grade {result.Parameters.ScoreGradeWeight:N2}, HS total {result.Parameters.ScoreHighSchoolTotalWeight:N2}, HS grade {result.Parameters.ScoreHighSchoolGradeWeight:N2}");
    Console.WriteLine($"  year wts:   anchor {result.Parameters.AnchorYearWeight:N2}, slope {result.Parameters.YearWeightSlope:N2}/yr, cap {result.Parameters.YearWeightCap:N2}");
    Console.WriteLine($"  density:    low/med {result.Parameters.DensityLowMediumFactor:N2}, med-high {result.Parameters.DensityMediumHighFactor:N2}, high {result.Parameters.DensityHighFactor:N2}");
    Console.WriteLine($"  density 1:  low/med {result.Parameters.DensityLowMediumFirstChildFactor:N2}, med-high {result.Parameters.DensityMediumHighFirstChildFactor:N2}, high {result.Parameters.DensityHighFirstChildFactor:N2}");
    Console.WriteLine($"  density 2:  low/med {result.Parameters.DensityLowMediumSecondChildFactor:N2}, med-high {result.Parameters.DensityMediumHighSecondChildFactor:N2}, high {result.Parameters.DensityHighSecondChildFactor:N2}");
    Console.WriteLine($"  density 3:  low/med {result.Parameters.DensityLowMediumThirdChildFactor:N2}, med-high {result.Parameters.DensityMediumHighThirdChildFactor:N2}, high {result.Parameters.DensityHighThirdChildFactor:N2}");
    Console.WriteLine($"  turnover:   {result.Parameters.OwnershipChangeProbability:P2}");
    Console.WriteLine($"  0 child:    {result.Parameters.MoveInZeroChildShare:P2}");
    Console.WriteLine($"  1 child:    {result.Parameters.MoveInOneChildShare:P2}");
    Console.WriteLine($"  2 child:    {result.Parameters.MoveInTwoChildShare:P2}");
    Console.WriteLine($"  3 child:    {result.Parameters.MoveInThreeChildShare:P2}");
    Console.WriteLine($"  4 child:    {result.Parameters.MoveInFourPlusChildShare:P2}");
    Console.WriteLine($"  births/yr:  1st {result.Parameters.AnnualFirstNewChildProbability:P2}, 2nd {result.Parameters.AnnualSecondNewChildProbability:P2}, 3rd {result.Parameters.AnnualThirdNewChildProbability:P2}, 4th+ {result.Parameters.AnnualFourthPlusNewChildProbability:P2}");
    Console.WriteLine($"  TK-8 exit:  {result.Parameters.Tk8ExitProbability:P2}");
    Console.WriteLine($"  HS exit:    {result.Parameters.HighSchoolExitProbability:P2}");
    Console.WriteLine($"  SpEd exit:  {result.Parameters.SpecialExitProbability:P2}");
    Console.WriteLine($"  SpEd attr:  {result.Parameters.SpecialEducationProbability:P2}");
    Console.WriteLine($"  same year:  {result.Parameters.SameSchoolYearProbability:P2}");
    Console.WriteLine($"  weights:    pre-school {result.Parameters.MoveInPreschoolWeight:0.##}, TK/K {result.Parameters.MoveInTkKWeight:0.##}, elem {result.Parameters.MoveInElementaryWeight:0.##}, middle {result.Parameters.MoveInMiddleWeight:0.##}, high {result.Parameters.MoveInHighWeight:0.##}, post-school {result.Parameters.MoveInPostSchoolWeight:0.##}");
    Console.WriteLine();
    Console.WriteLine("Validation");
    Console.WriteLine($"  score:      {result.CombinedScore:P2}");
    Console.WriteLine($"  total MAE:        {result.GridMeanAbsoluteError:N0}");
    Console.WriteLine($"  total MAPE:       {result.GridMeanAbsolutePercentageError:P2}");
    Console.WriteLine($"  grid/year MAE:    {result.GridLevelMeanAbsoluteError:N1}");
    Console.WriteLine($"  grid/year MAPE:   {result.GridLevelMeanAbsolutePercentageError:P2}");
    Console.WriteLine($"  grade total MAE:  {result.GradeMeanAbsoluteError:N0}");
    Console.WriteLine($"  grade total MAPE: {result.GradeMeanAbsolutePercentageError:P2} (diagnostic)");
    Console.WriteLine($"  grade/year MAE:   {result.GradeLevelMeanAbsoluteError:N1} (TK excluded, +/-{result.Parameters.GradeSmoothingWindow:N0} cohort window)");
    Console.WriteLine($"  grade/year MAPE:  {result.GradeLevelMeanAbsolutePercentageError:P2} (TK excluded, +/-{result.Parameters.GradeSmoothingWindow:N0} cohort window)");
    Console.WriteLine($"  HS total MAE:     {result.HighSchoolMeanAbsoluteError:N0}");
    Console.WriteLine($"  HS total MAPE:    {result.HighSchoolMeanAbsolutePercentageError:P2}");
    Console.WriteLine($"  HS grade MAE:     {result.HighSchoolGradeMeanAbsoluteError:N1} (+/-{result.Parameters.GradeSmoothingWindow:N0} cohort window)");
    Console.WriteLine($"  HS grade MAPE:    {result.HighSchoolGradeMeanAbsolutePercentageError:P2} (+/-{result.Parameters.GradeSmoothingWindow:N0} cohort window)");
    Console.WriteLine();
    Console.WriteLine("Year                 Actual Grid   Modeled Grid     Grid Err   Grid/Yr MAPE   Actual Grade  Modeled Grade    Grade Err   Grade/Yr MAPE      Actual HS     Modeled HS       HS Err       HS MAPE");
    foreach (var comparison in result.Comparisons)
    {
        Console.WriteLine(
            "{0} {1,20:N0} {2,14:N0} {3,12:+#,##0;-#,##0;0} {4,14:P2} {5,14:N0} {6,14:N0} {7,12:+#,##0;-#,##0;0} {8,15:P2} {9,14:N0} {10,14:N0} {11,12:+#,##0;-#,##0;0} {12,13:P2}",
            comparison.Year,
            comparison.ActualGridTotal,
            comparison.ModeledGridTotal,
            comparison.GridError,
            comparison.GridLevelMeanAbsolutePercentageError,
            comparison.ActualGradeTotal,
            comparison.ModeledGradeTotal,
            comparison.GradeError,
            comparison.GradeLevelMeanAbsolutePercentageError,
            comparison.HighSchoolActualTotal,
            comparison.HighSchoolModeledTotal,
            comparison.HighSchoolError,
            comparison.HighSchoolAbsolutePercentageError);
    }

    if (!showGradeDetails)
    {
        return;
    }

    Console.WriteLine();
    Console.WriteLine("Grade detail by year");
    foreach (var comparison in result.Comparisons)
    {
        Console.WriteLine();
        Console.WriteLine($"{comparison.Year}");
        Console.WriteLine("Grade        Actual  Adj Actual      Modeled        Error      Error %");
        foreach (var (grade, actual, modeled) in comparison.ModeledGrades
            .OrderBy(kvp => GradeSortKey(kvp.Key))
            .Select(kvp => (kvp.Key, comparison.ActualGrades.GetValueOrDefault(kvp.Key), kvp.Value)))
        {
            var adjustedActual = comparison.AdjustedActualGrades.GetValueOrDefault(grade);
            var errorText = adjustedActual > 0 ? (modeled - adjustedActual).ToString("+#,##0;-#,##0;0") : "n/a";
            var percentText = adjustedActual > 0 ? (Math.Abs(modeled - adjustedActual) / adjustedActual).ToString("P1") : "n/a";
            Console.WriteLine("{0,-8} {1,10} {2,11:N0} {3,12:N0} {4,12} {5,10}", grade, FormatNullable(actual), adjustedActual, modeled, errorText, percentText);
        }
    }
}

static int GradeSortKey(string grade)
{
    return grade switch
    {
        "TK" => 0,
        "K" => 1,
        "1st" => 2,
        "2nd" => 3,
        "3rd" => 4,
        "4th" => 5,
        "5th" => 6,
        "6th" => 7,
        "7th" => 8,
        "8th" => 9,
        "9th" => 10,
        "10th" => 11,
        "11th" => 12,
        "12th" => 13,
        "Sp. Ed." => 14,
        _ => 100
    };
}

static string FormatNullable(double? value)
{
    return value.HasValue ? value.Value.ToString("N0") : "n/a";
}

static string FormatParallelism(int maxDegreeOfParallelism)
{
    return maxDegreeOfParallelism <= 0 ? "CPU count" : maxDegreeOfParallelism.ToString("N0");
}

static void WriteCsv(string path, IReadOnlyList<MonteCarloValidationResult> results)
{
    using var writer = new StreamWriter(path);
    writer.WriteLine("score,total_mape,grid_year_mape,grade_total_mape,grade_year_mape,hs_total_mape,hs_grade_mape,total_mae,grid_year_mae,grade_total_mae,grade_year_mae,hs_total_mae,hs_grade_mae,runs,seed,parallelism,grade_smoothing_window,score_total_weight,score_grid_weight,score_grade_weight,score_hs_total_weight,score_hs_grade_weight,anchor_year_weight,year_weight_slope,year_weight_cap,density_low_medium,density_medium_high,density_high,turnover,zero_child_share,one_child_share,two_child_share,three_child_share,four_plus_child_share,student_exit,annual_first_birth,annual_second_birth,annual_third_birth,annual_fourth_plus_birth,tk8_exit,high_exit,special_exit,special_education_probability,same_school_year,preschool_weight,tkk_weight,elementary_weight,middle_weight,high_weight,postschool_weight");
    foreach (var result in results.OrderBy(item => item.CombinedScore))
    {
        var p = result.Parameters;
        writer.WriteLine(string.Join(',',
            result.CombinedScore,
            result.GridMeanAbsolutePercentageError,
            result.GridLevelMeanAbsolutePercentageError,
            result.GradeMeanAbsolutePercentageError,
            result.GradeLevelMeanAbsolutePercentageError,
            result.HighSchoolMeanAbsolutePercentageError,
            result.HighSchoolGradeMeanAbsolutePercentageError,
            result.GridMeanAbsoluteError,
            result.GridLevelMeanAbsoluteError,
            result.GradeMeanAbsoluteError,
            result.GradeLevelMeanAbsoluteError,
            result.HighSchoolMeanAbsoluteError,
            result.HighSchoolGradeMeanAbsoluteError,
            p.Runs,
            p.Seed,
            p.MaxDegreeOfParallelism,
            p.GradeSmoothingWindow,
            p.ScoreTotalWeight,
            p.ScoreGridWeight,
            p.ScoreGradeWeight,
            p.ScoreHighSchoolTotalWeight,
            p.ScoreHighSchoolGradeWeight,
            p.AnchorYearWeight,
            p.YearWeightSlope,
            p.YearWeightCap,
            p.DensityLowMediumFactor,
            p.DensityMediumHighFactor,
            p.DensityHighFactor,
            p.OwnershipChangeProbability,
            p.MoveInZeroChildShare,
            p.MoveInOneChildShare,
            p.MoveInTwoChildShare,
            p.MoveInThreeChildShare,
            p.MoveInFourPlusChildShare,
            p.StudentExitProbability,
            p.AnnualFirstNewChildProbability,
            p.AnnualSecondNewChildProbability,
            p.AnnualThirdNewChildProbability,
            p.AnnualFourthPlusNewChildProbability,
            p.Tk8ExitProbability,
            p.HighSchoolExitProbability,
            p.SpecialExitProbability,
            p.SpecialEducationProbability,
            p.SameSchoolYearProbability,
            p.MoveInPreschoolWeight,
            p.MoveInTkKWeight,
            p.MoveInElementaryWeight,
            p.MoveInMiddleWeight,
            p.MoveInHighWeight,
            p.MoveInPostSchoolWeight));
    }
}

static string ResolveDataRoot(string? provided)
{
    if (!string.IsNullOrWhiteSpace(provided))
    {
        return provided;
    }

    var directory = new DirectoryInfo(AppContext.BaseDirectory);
    while (directory is not null)
    {
        var candidate = Path.Combine(directory.FullName, "data");
        if (File.Exists(Path.Combine(candidate, "homes.csv")))
        {
            return candidate;
        }

        directory = directory.Parent;
    }

    throw new DirectoryNotFoundException("Could not find data directory. Pass --data <path>.");
}

static void PrintHelp()
{
    Console.WriteLine("""
SchoolGrowth.Cli Monte Carlo tools

Usage:
  dotnet run --project src/SchoolGrowth.Cli -- validate [options]
  dotnet run --project src/SchoolGrowth.Cli -- search [options]
  dotnet run --project src/SchoolGrowth.Cli -- optimize [options]
  dotnet run --project src/SchoolGrowth.Cli -- lifecycle [options]

Common options:
  --data <path>       Data folder containing homes.csv, grid.csv, grade.csv, schools.csv
  --start <year>      Validation start year, default 2020
  --end <year>        Validation end year, default 2024
  --runs <n>          Runs for validate, default 1000
  --parallelism <n>   Worker threads for Monte Carlo validation runs, default CPU count
  --grade-window <n>  Cohort smoothing years for grade/year validation, 0-2, default 2
  --score-total <n>   Score weight for district total MAPE, default 1.00
  --score-grid <n>    Score weight for grid/year MAPE, default 6.00
  --score-grade <n>   Score weight for grade/year MAPE, default 1.00
  --score-hs-total <n> Score weight for high-school total MAPE, default 2.00
  --score-hs-grade <n> Score weight for high-school grade/year MAPE, default 1.00
  --anchor-weight <n>  Weight for anchored start year when actual data exists, default 0.25
  --year-weight-slope <n> Extra validation weight per year after start, default 0.15
  --year-weight-cap <n> Maximum validation year weight, default 2.00
  --density-low <n>   Low/medium density student factor, default 1.00
  --density-rmh <n>   Medium-high density student factor, default 0.90
  --density-high <n>  High density student factor, default 0.95
  --density-low-1st <n>  Low/medium 1st-child density factor, default 1.00
  --density-low-2nd <n>  Low/medium 2nd-child density factor, default 1.00
  --density-low-3rd <n>  Low/medium 3rd-child density factor, default 1.00
  --density-rmh-1st <n>  Medium-high 1st-child density factor, default 1.00
  --density-rmh-2nd <n>  Medium-high 2nd-child density factor, default 1.00
  --density-rmh-3rd <n>  Medium-high 3rd-child density factor, default 1.00
  --density-high-1st <n> High 1st-child density factor, default 1.00
  --density-high-2nd <n> High 2nd-child density factor, default 1.00
  --density-high-3rd <n> High 3rd-child density factor, default 0.59
  --seed <n>          Random seed, default 2026
  --turnover <p>      Household reset probability, default 0.05
  --zero-child <p>    Move-in share with 0 children, default 0.1109
  --one-child <p>     Move-in share with 1 child, default 0.2390
  --two-child <p>     Move-in share with 2 children, default 0.6150
  --three-child <p>   Move-in share with 3 children, default 0.0350
  --four-plus-child <p> Move-in share with 4 children, default 0.00
  --exit <p>          Student exit probability, default 0.00
  --first-birth <p>   Yearly probability of a household's 1st new child, default 0.045
  --second-birth <p>  Yearly probability of a household's 2nd new child, default 0.033
  --third-birth <p>   Yearly probability of a household's 3rd new child, default 0.0034
  --fourth-birth <p>  Yearly probability of a household's 4th+ new child, default 0.00
  --tk8-exit <p>      TK-8 student exit probability, default uses --exit
  --high-exit <p>     High school student exit probability, default uses --exit
  --special-exit <p>  Special education exit probability, default uses --exit
  --special-probability <p> Probability each child has special education placement, default 0.012
  --same-year <p>     Probability a built home affects the same school year, default 0.3625
  --grade-details     Print actual/modeled/error for each grade in each year
  --preschool-weight <n> Move-in pre-school child weight, default 0.464
  --tkk-weight <n>    Move-in TK/K grade-band weight, default 0.232
  --elem-weight <n>   Move-in 1-5 grade-band weight, default 0.58
  --middle-weight <n> Move-in 6-8 grade-band weight, default 0.27
  --high-weight <n>   Move-in 9-12 grade-band weight, default 0.16
  --postschool-weight <n> Move-in post-school child weight, default 0.03
  --years <n>         Lifecycle years for lifecycle command, default 30
  --homes <n>         Simulated homes per run for lifecycle command, default 10000
  --grid <name>       Source grid for lifecycle command, default Questa
  --density <name>    Density label for lifecycle command, default RL

Search options:
  --search-runs <n>   Runs per candidate, default 200
  --optimize-iterations <n> Coordinate-search passes for optimize, default 4
  --max-turnover <p>  Maximum turnover used by optimize, default 0.08
  --turnovers <csv>   Candidate ownership probabilities
  --zero-children <csv> Candidate move-in shares with 0 children
  --one-children <csv> Candidate move-in shares with 1 child
  --two-children <csv> Candidate move-in shares with 2 children
  --three-children <csv> Candidate move-in shares with 3 children
  --four-plus-children <csv> Candidate move-in shares with 4 children
  --exits <csv>       Candidate exit probabilities
  --first-births <csv> Candidate yearly 1st new-child probabilities
  --second-births <csv> Candidate yearly 2nd new-child probabilities
  --third-births <csv> Candidate yearly 3rd new-child probabilities
  --fourth-births <csv> Candidate yearly 4th+ new-child probabilities
  --same-years <csv> Candidate same-school-year probabilities
  --out <path>        Optional CSV output for all candidates
""");
}

sealed class CliOptions
{
    public string Command { get; init; } = "validate";
    public string? DataRoot { get; init; }
    public int StartYear { get; init; } = 2020;
    public int EndYear { get; init; } = 2024;
    public int SearchRuns { get; init; } = 200;
    public int OptimizeIterations { get; init; } = 4;
    public double MaxOptimizeTurnover { get; init; } = 0.08;
    public int Parallelism { get; init; }
    public int GradeSmoothingWindow { get; init; } = 2;
    public int LifecycleYears { get; init; } = 30;
    public int HomesPerRun { get; init; } = 10000;
    public int Seed { get; init; } = 2026;
    public string Grid { get; init; } = "Questa";
    public string Density { get; init; } = "RL";
    public string? OutputPath { get; init; }
    public bool ShowHelp { get; init; }
    public bool ShowGradeDetails { get; init; }
    public MonteCarloParameters Parameters { get; init; } = new();
    public IReadOnlyList<double> OwnershipCandidates { get; init; } = [];
    public IReadOnlyList<double> ZeroChildCandidates { get; init; } = [];
    public IReadOnlyList<double> OneChildCandidates { get; init; } = [];
    public IReadOnlyList<double> TwoChildCandidates { get; init; } = [];
    public IReadOnlyList<double> ThreeChildCandidates { get; init; } = [];
    public IReadOnlyList<double> FourPlusChildCandidates { get; init; } = [];
    public IReadOnlyList<double> ExitCandidates { get; init; } = [];
    public IReadOnlyList<double> FirstBirthCandidates { get; init; } = [];
    public IReadOnlyList<double> SecondBirthCandidates { get; init; } = [];
    public IReadOnlyList<double> ThirdBirthCandidates { get; init; } = [];
    public IReadOnlyList<double> FourthBirthCandidates { get; init; } = [];
    public IReadOnlyList<double> Tk8ExitCandidates { get; init; } = [];
    public IReadOnlyList<double> HighExitCandidates { get; init; } = [];
    public IReadOnlyList<double> SpecialExitCandidates { get; init; } = [];
    public IReadOnlyList<double> SameYearCandidates { get; init; } = [];

    public static CliOptions Parse(string[] args)
    {
        if (args.Any(arg => arg is "-h" or "--help"))
        {
            return new CliOptions { ShowHelp = true };
        }

        var command = args.FirstOrDefault(arg => !arg.StartsWith('-')) ?? "validate";
        var values = ReadOptions(args.Skip(command == args.FirstOrDefault() ? 1 : 0).ToArray());
        var runs = ReadInt(values, "runs", 1000);
        var seed = ReadInt(values, "seed", 2026);

        return new CliOptions
        {
            Command = command,
            DataRoot = ReadString(values, "data"),
            StartYear = ReadInt(values, "start", 2020),
            EndYear = ReadInt(values, "end", 2024),
            SearchRuns = ReadInt(values, "search-runs", 200),
            OptimizeIterations = ReadInt(values, "optimize-iterations", 4),
            MaxOptimizeTurnover = ReadDouble(values, "max-turnover", 0.08),
            Parallelism = ReadInt(values, "parallelism", 0),
            GradeSmoothingWindow = ReadInt(values, "grade-window", 2),
            LifecycleYears = ReadInt(values, "years", 30),
            HomesPerRun = ReadInt(values, "homes", 10000),
            Seed = seed,
            Grid = ReadString(values, "grid") ?? "Questa",
            Density = ReadString(values, "density") ?? "RL",
            OutputPath = ReadString(values, "out"),
            ShowGradeDetails = ReadBool(values, "grade-details"),
            Parameters = new MonteCarloParameters(
                runs,
                seed,
                ReadDouble(values, "turnover", 0.05),
                ReadDouble(values, "zero-child", 0.1109),
                ReadDouble(values, "one-child", ReadDouble(values, "first", 0.2390)),
                ReadDouble(values, "two-child", ReadDouble(values, "second", 0.6150)),
                ReadDouble(values, "three-child", ReadDouble(values, "third", 0.0350)),
                ReadDouble(values, "four-plus-child", 0.0),
                ReadDouble(values, "exit", 0.0),
                ReadDouble(values, "tkk-weight", 0.232),
                ReadDouble(values, "elem-weight", 0.58),
                ReadDouble(values, "middle-weight", 0.27),
                ReadDouble(values, "high-weight", 0.16),
                ReadDouble(values, "first-birth", 0.045),
                ReadDouble(values, "second-birth", 0.033),
                ReadDouble(values, "third-birth", 0.0034),
                ReadDouble(values, "fourth-birth", 0.0),
                ReadDouble(values, "tk8-exit", ReadDouble(values, "exit", 0.0)),
                ReadDouble(values, "high-exit", ReadDouble(values, "exit", 0.0)),
                ReadDouble(values, "special-exit", ReadDouble(values, "exit", 0.0)),
                ReadDouble(values, "same-year", 0.3625),
                ReadDouble(values, "preschool-weight", 0.464),
                ReadDouble(values, "postschool-weight", 0.03),
                ReadDouble(values, "special-probability", 0.012),
                ReadInt(values, "parallelism", 0),
                ReadInt(values, "grade-window", 2),
                ReadDouble(values, "score-total", 1.0),
                ReadDouble(values, "score-grid", 6.0),
                ReadDouble(values, "score-grade", 1.0),
                ReadDouble(values, "score-hs-total", 2.0),
                ReadDouble(values, "score-hs-grade", 1.0),
                ReadDouble(values, "density-low", 1.0),
                ReadDouble(values, "density-rmh", 0.90),
                ReadDouble(values, "density-high", 0.95),
                ReadDouble(values, "density-low-1st", 1.0),
                ReadDouble(values, "density-low-2nd", 1.0),
                ReadDouble(values, "density-low-3rd", 1.0),
                ReadDouble(values, "density-rmh-1st", 1.0),
                ReadDouble(values, "density-rmh-2nd", 1.0),
                ReadDouble(values, "density-rmh-3rd", 1.0),
                ReadDouble(values, "density-high-1st", 1.0),
                ReadDouble(values, "density-high-2nd", 1.0),
                ReadDouble(values, "density-high-3rd", 0.59),
                ReadDouble(values, "anchor-weight", 0.25),
                ReadDouble(values, "year-weight-slope", 0.15),
                ReadDouble(values, "year-weight-cap", 2.0)),
            OwnershipCandidates = ReadDoubles(values, "turnovers"),
            ZeroChildCandidates = ReadDoubles(values, "zero-children"),
            OneChildCandidates = ReadDoubles(values, "one-children", "firsts"),
            TwoChildCandidates = ReadDoubles(values, "two-children", "seconds"),
            ThreeChildCandidates = ReadDoubles(values, "three-children", "thirds"),
            FourPlusChildCandidates = ReadDoubles(values, "four-plus-children"),
            ExitCandidates = ReadDoubles(values, "exits"),
            FirstBirthCandidates = ReadDoubles(values, "first-births"),
            SecondBirthCandidates = ReadDoubles(values, "second-births"),
            ThirdBirthCandidates = ReadDoubles(values, "third-births"),
            FourthBirthCandidates = ReadDoubles(values, "fourth-births"),
            Tk8ExitCandidates = ReadDoubles(values, "tk8-exits"),
            HighExitCandidates = ReadDoubles(values, "high-exits"),
            SpecialExitCandidates = ReadDoubles(values, "special-exits"),
            SameYearCandidates = ReadDoubles(values, "same-years")
        };
    }

    private static Dictionary<string, string> ReadOptions(string[] args)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < args.Length; i++)
        {
            if (!args[i].StartsWith("--", StringComparison.Ordinal)) continue;
            var key = args[i][2..];
            var value = i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal)
                ? args[++i]
                : "true";
            result[key] = value;
        }

        return result;
    }

    private static string? ReadString(Dictionary<string, string> values, string key)
    {
        return values.GetValueOrDefault(key);
    }

    private static int ReadInt(Dictionary<string, string> values, string key, int fallback)
    {
        return int.TryParse(values.GetValueOrDefault(key), out var value) ? value : fallback;
    }

    private static bool ReadBool(Dictionary<string, string> values, string key)
    {
        return values.TryGetValue(key, out var value) && bool.TryParse(value, out var parsed) ? parsed : values.ContainsKey(key);
    }

    private static double ReadDouble(Dictionary<string, string> values, string key, double fallback)
    {
        return double.TryParse(values.GetValueOrDefault(key), out var value) ? value : fallback;
    }

    private static IReadOnlyList<double> ReadDoubles(Dictionary<string, string> values, string key)
    {
        return values.TryGetValue(key, out var text)
            ? text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(item => double.TryParse(item, out var value) ? Math.Clamp(value, 0, 1) : double.NaN)
                .Where(value => !double.IsNaN(value))
                .Distinct()
                .ToList()
            : [];
    }

    private static IReadOnlyList<double> ReadDoubles(Dictionary<string, string> values, string key, string fallbackKey)
    {
        var result = ReadDoubles(values, key);
        return result.Count > 0 ? result : ReadDoubles(values, fallbackKey);
    }
}

sealed record OptimizeVariable(
    string Name,
    double InitialStep,
    double Minimum,
    double Maximum,
    Func<MonteCarloParameters, double> Get,
    Func<MonteCarloParameters, double, MonteCarloParameters> Set);
