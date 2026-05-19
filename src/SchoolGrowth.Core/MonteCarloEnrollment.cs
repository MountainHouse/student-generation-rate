namespace SchoolGrowth.Core;

public sealed record MonteCarloParameters(
    int Runs = 50,
    int Seed = 2026,
    double OwnershipChangeProbability = 0.05,
    double MoveInZeroChildShare = 0.1109,
    double MoveInOneChildShare = 0.2390,
    double MoveInTwoChildShare = 0.6150,
    double MoveInThreeChildShare = 0.0350,
    double MoveInFourChildShare = 0.0,
    double StudentExitProbability = 0.0,
    double MoveInTkKWeight = 0.232,
    double MoveInElementaryWeight = 0.58,
    double MoveInMiddleWeight = 0.27,
    double MoveInHighWeight = 0.16,
    double AnnualFirstNewChildProbability = 0.045,
    double AnnualSecondNewChildProbability = 0.033,
    double AnnualThirdNewChildProbability = 0.0034,
    double AnnualFourthPlusNewChildProbability = 0.0,
    double Tk8ExitProbability = 0.0,
    double HighSchoolExitProbability = 0.0,
    double SpecialExitProbability = 0.0,
    double SameSchoolYearProbability = 0.3625,
    double MoveInPreschoolWeight = 0.464,
    double MoveInPostSchoolWeight = 0.03,
    double SpecialEducationProbability = 0.012,
    int MaxDegreeOfParallelism = 0,
    int GradeSmoothingWindow = 2,
    double ScoreTotalWeight = 1.0,
    double ScoreGridWeight = 6.0,
    double ScoreGradeWeight = 1.0,
    double ScoreHighSchoolTotalWeight = 2.0,
    double ScoreHighSchoolGradeWeight = 1.0,
    double DensityLowFactor = 1.0,
    double DensityMediumFactor = 1.0,
    double DensityMediumHighFactor = 0.90,
    double DensityHighFactor = 0.95,
    double DensityLowFirstChildFactor = 1.0,
    double DensityLowSecondChildFactor = 1.0,
    double DensityLowThirdChildFactor = 1.0,
    double DensityLowFourthChildFactor = 1.0,
    double DensityMediumFirstChildFactor = 1.0,
    double DensityMediumSecondChildFactor = 1.0,
    double DensityMediumThirdChildFactor = 1.0,
    double DensityMediumFourthChildFactor = 1.0,
    double DensityMediumHighFirstChildFactor = 1.0,
    double DensityMediumHighSecondChildFactor = 1.0,
    double DensityMediumHighThirdChildFactor = 1.0,
    double DensityMediumHighFourthChildFactor = 1.0,
    double DensityHighFirstChildFactor = 1.0,
    double DensityHighSecondChildFactor = 1.0,
    double DensityHighThirdChildFactor = 0.59,
    double DensityHighFourthChildFactor = 1.0,
    double AnchorYearWeight = 0.25,
    double YearWeightSlope = 0.15,
    double YearWeightCap = 2.0);

public sealed record MonteCarloParameterPreset(
    string Name,
    string Description,
    MonteCarloParameters Parameters);

public sealed record MonteCarloValidationRequest(
    int StartYear,
    int EndYear,
    MonteCarloParameters Parameters,
    IReadOnlyList<ScenarioHome>? ScenarioHomes = null);

public sealed record MonteCarloYearComparison(
    int Year,
    Dictionary<string, double?> ActualGridTotals,
    Dictionary<string, double> ModeledGridTotals,
    Dictionary<string, double> ModeledGridTotalStandardDeviations,
    Dictionary<string, Dictionary<string, double>> ModeledGridGrades,
    Dictionary<string, Dictionary<string, double>> ModeledGridGradeStandardDeviations,
    Dictionary<string, double?> ActualGrades,
    Dictionary<string, double> AdjustedActualGrades,
    Dictionary<string, double> ModeledGrades,
    Dictionary<string, double> ModeledGradeStandardDeviations,
    double ActualGridTotal,
    double ModeledGridTotal,
    double ModeledGridTotalStandardDeviation,
    double GridError,
    double GridAbsolutePercentageError,
    double GridLevelMeanAbsoluteError,
    double GridLevelMeanAbsolutePercentageError,
    double ActualGradeTotal,
    double ModeledGradeTotal,
    double ModeledGradeTotalStandardDeviation,
    double GradeError,
    double GradeAbsolutePercentageError,
    double GradeLevelMeanAbsoluteError,
    double GradeLevelMeanAbsolutePercentageError,
    double HighSchoolActualTotal,
    double HighSchoolModeledTotal,
    double HighSchoolModeledTotalStandardDeviation,
    double HighSchoolError,
    double HighSchoolAbsolutePercentageError,
    double HighSchoolGradeMeanAbsoluteError,
    double HighSchoolGradeMeanAbsolutePercentageError);

public sealed record MonteCarloSimulationYearInfo(
    int Year,
    double TotalHomes,
    double LowDensityHomes,
    double MediumDensityHomes,
    double MediumHighDensityHomes,
    double HighDensityHomes,
    double OtherDensityHomes,
    double K12Students,
    double TkStudents,
    double K8Students,
    double HighSchoolStudents,
    double K12StudentsPerHome,
    double K8StudentsPerHome,
    double HighSchoolStudentsPerHome,
    double LowMediumK12StudentsPerHome,
    double LowMediumK8StudentsPerHome,
    double LowMediumHighSchoolStudentsPerHome,
    double MediumHighHighK12StudentsPerHome,
    double MediumHighHighK8StudentsPerHome,
    double MediumHighHighHighSchoolStudentsPerHome,
    double FamiliesWithZeroChildren,
    double FamiliesWithOneChild,
    double FamiliesWithTwoChildren,
    double FamiliesWithThreeChildren,
    double FamiliesWithFourPlusChildren,
    double FamiliesWithZeroChildrenShare,
    double FamiliesWithOneChildShare,
    double FamiliesWithTwoChildrenShare,
    double FamiliesWithThreeChildrenShare,
    double FamiliesWithFourPlusChildrenShare,
    double TurnoverEvents,
    double TurnoverFactor,
    double AverageLongevityBeforeTurnover,
    IReadOnlyList<MonteCarloSimulationSegmentInfo> Segments);

public sealed record MonteCarloSimulationSegmentInfo(
    string Grid,
    string Density,
    double TotalHomes,
    Dictionary<string, double> Grades,
    double FamiliesWithZeroChildren,
    double FamiliesWithOneChild,
    double FamiliesWithTwoChildren,
    double FamiliesWithThreeChildren,
    double FamiliesWithFourPlusChildren,
    double TurnoverEvents,
    double AverageLongevityBeforeTurnover);

public sealed record MonteCarloSyntheticHomeDiagnostic(
    string Grid,
    int BaselineYear,
    double AdjustedBaselineStudents,
    double ListedHomesThroughBaseline,
    double ExpectedStudentsPerHome,
    double SyntheticHomes);

public sealed record MonteCarloValidationResult(
    MonteCarloParameters Parameters,
    IReadOnlyList<MonteCarloYearComparison> Comparisons,
    IReadOnlyList<MonteCarloSimulationYearInfo> SimulationInfo,
    IReadOnlyList<MonteCarloSyntheticHomeDiagnostic> SyntheticHomeDiagnostics,
    double GridMeanAbsoluteError,
    double GridMeanAbsolutePercentageError,
    double GridLevelMeanAbsoluteError,
    double GridLevelMeanAbsolutePercentageError,
    double GradeMeanAbsoluteError,
    double GradeMeanAbsolutePercentageError,
    double GradeLevelMeanAbsoluteError,
    double GradeLevelMeanAbsolutePercentageError,
    double HighSchoolMeanAbsoluteError,
    double HighSchoolMeanAbsolutePercentageError,
    double HighSchoolGradeMeanAbsoluteError,
    double HighSchoolGradeMeanAbsolutePercentageError,
    double CombinedScore);

public sealed record MonteCarloSearchRequest(
    int StartYear,
    int EndYear,
    int RunsPerCandidate,
    int Seed,
    IReadOnlyList<double> OwnershipChangeProbabilities,
    IReadOnlyList<double> MoveInZeroChildShares,
    IReadOnlyList<double> MoveInOneChildShares,
    IReadOnlyList<double> MoveInTwoChildShares,
    IReadOnlyList<double> MoveInThreeChildShares,
    IReadOnlyList<double> MoveInFourChildShares,
    IReadOnlyList<double> StudentExitProbabilities,
    MonteCarloParameters? BaseParameters = null);

public sealed record MonteCarloSearchResult(
    MonteCarloValidationResult Best,
    IReadOnlyList<MonteCarloValidationResult> Candidates);

public sealed record MonteCarloLifecycleRequest(
    int Years,
    int Runs,
    int HomesPerRun,
    string Grid,
    string Density,
    MonteCarloParameters Parameters);

public sealed record MonteCarloLifecycleYear(
    int YearsAfterMoveIn,
    double StudentsPerHome,
    double Tk8StudentsPerHome,
    double HighSchoolStudentsPerHome,
    double SpecialEducationStudentsPerHome,
    double ChildrenPerHome,
    double HomesWithoutChildrenShare,
    double HomesWithOneChildShare,
    double HomesWithTwoChildrenShare,
    double HomesWithThreeChildrenShare,
    double HomesWithFourPlusChildrenShare);

public sealed record MonteCarloChildCountDistribution(
    double HouseholdCount,
    double HomesWithoutChildrenShare,
    double HomesWithOneChildShare,
    double HomesWithTwoChildrenShare,
    double HomesWithThreeChildrenShare,
    double HomesWithFourPlusChildrenShare);

public sealed record MonteCarloFamilyLongevityYear(
    int YearsInHome,
    double FamilyCount,
    double Share);

public sealed record MonteCarloTurnoverYear(
    int Year,
    double ActiveHomes,
    double TurnoverEvents,
    double TurnoverRate);

public sealed record MonteCarloLifecycleResult(
    MonteCarloParameters Parameters,
    MonteCarloChildCountDistribution InitialHouseholds,
    MonteCarloChildCountDistribution TurnoverHouseholds,
    MonteCarloChildCountDistribution EndingHouseholds,
    double RealizedTurnoverRate,
    IReadOnlyList<MonteCarloTurnoverYear> TurnoverByYear,
    IReadOnlyList<MonteCarloFamilyLongevityYear> CompletedFamilyLongevity,
    IReadOnlyList<MonteCarloFamilyLongevityYear> ActiveAtEndFamilyLongevity,
    IReadOnlyList<MonteCarloLifecycleYear> Years);

public sealed class MonteCarloEnrollmentModel
{
    private const int PostSchoolChildIndex = 200;
    private const int ForcedOwnershipTurnoverYears = 50;
    private static readonly HashSet<string> ExcludedValidationGrids = new(StringComparer.OrdinalIgnoreCase)
    {
        "Inter-Districts",
        "Lammersville",
        "MHESD"
    };
    private static readonly string[] GradeOrder =
    [
        "TK", "K", "1st", "2nd", "3rd", "4th", "5th", "6th", "7th", "8th",
        "9th", "10th", "11th", "12th", "Sp. Ed."
    ];

    private static readonly string[] ValidationGrades = GradeOrder.Skip(1).Take(13).ToArray();
    private static readonly string[] HighSchoolGrades = ["9th", "10th", "11th", "12th"];
    private readonly EnrollmentData data;
    public MonteCarloEnrollmentModel(EnrollmentData data)
    {
        this.data = data;
    }

    public MonteCarloValidationResult Validate(MonteCarloValidationRequest request)
    {
        var scenarioHomes = (request.ScenarioHomes ?? [])
            .Where(home => home.Homes > 0 && !IsExcludedValidationGrid(home.Neighborhood))
            .ToList();
        var minimumStartYear = FirstHomeBuildYear();
        var maximumEndYear = Math.Max(
            Math.Max(data.AugustYears.Last(), LastHomeBuildYear()),
            scenarioHomes.Select(home => home.Year).DefaultIfEmpty(0).Max()) + 50;
        var startYear = Math.Clamp(request.StartYear, minimumStartYear, maximumEndYear);
        var endYear = Math.Clamp(request.EndYear, startYear, maximumEndYear);
        var anchorStartYear = HasActualGradeData(startYear) && HasActualGridData(startYear);
        var baselineYear = anchorStartYear
            ? startYear
            : data.AugustYears
                .Where(year => year < startYear)
                .Select(year => (int?)year)
                .Max() ?? startYear - 1;
        var parameters = Sanitize(request.Parameters);
        var accumulators = CreateYearAccumulators(startYear, endYear);
        var mergeLock = new object();
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = parameters.MaxDegreeOfParallelism <= 0
                ? Environment.ProcessorCount
                : parameters.MaxDegreeOfParallelism
        };

        Parallel.For(
            0,
            parameters.Runs,
            parallelOptions,
            () => CreateYearAccumulators(startYear, endYear),
            (run, _, localAccumulators) =>
            {
                var random = new Random(parameters.Seed + run);
                var homes = InitializeHomes(baselineYear, scenarioHomes, parameters, random);

                for (var year = startYear; year <= endYear; year++)
                {
                    if (anchorStartYear && year == startYear)
                    {
                        localAccumulators[year].Add(homes, year);
                        continue;
                    }

                    AddBuiltHomes(homes, year, scenarioHomes, parameters, random);
                    AdvanceHomes(
                        homes,
                        year,
                        parameters,
                        random,
                        segmentTurnoverObserver: (home, longevity) => localAccumulators[year].AddTurnover(home, longevity),
                        turnoverObserver: () => { });
                    localAccumulators[year].Add(homes, year);
                }

                return localAccumulators;
            },
            localAccumulators =>
            {
                lock (mergeLock)
                {
                    foreach (var (year, accumulator) in localAccumulators)
                    {
                        accumulators[year].Merge(accumulator);
                    }
                }
            }
        );

        var comparisons = accumulators
            .OrderBy(kvp => kvp.Key)
            .Select(kvp => BuildComparison(kvp.Key, kvp.Value, parameters))
            .ToList();
        var simulationInfo = accumulators
            .OrderBy(kvp => kvp.Key)
            .Select(kvp => BuildSimulationInfo(kvp.Key, kvp.Value, parameters))
            .ToList();
        var syntheticHomeDiagnostics = BuildSyntheticHomeDiagnostics(baselineYear, parameters);
        var referenceYearWeights = comparisons.ToDictionary(
            item => item.Year,
            item => ReferenceYearWeight(item.Year, startYear, anchorStartYear, parameters));

        var gridReferenceComparisons = comparisons.Where(item => item.ActualGridTotal > 0).ToList();
        var gradeReferenceComparisons = comparisons.Where(item => item.ActualGradeTotal > 0).ToList();
        var gridMae = WeightedAverage(gridReferenceComparisons, item => Math.Abs(item.GridError), item => referenceYearWeights[item.Year]);
        var gridMape = WeightedAverage(gridReferenceComparisons, item => item.GridAbsolutePercentageError, item => referenceYearWeights[item.Year]);
        var gridLevelMae = WeightedAverage(gridReferenceComparisons, item => item.GridLevelMeanAbsoluteError, item => referenceYearWeights[item.Year]);
        var gridLevelMape = WeightedAverage(gridReferenceComparisons, item => item.GridLevelMeanAbsolutePercentageError, item => referenceYearWeights[item.Year]);
        var gradeMae = WeightedAverage(gradeReferenceComparisons, item => Math.Abs(item.GradeError), item => referenceYearWeights[item.Year]);
        var gradeMape = WeightedAverage(gradeReferenceComparisons, item => item.GradeAbsolutePercentageError, item => referenceYearWeights[item.Year]);
        var gradeLevelMae = WeightedAverage(gradeReferenceComparisons, item => item.GradeLevelMeanAbsoluteError, item => referenceYearWeights[item.Year]);
        var gradeLevelMape = WeightedAverage(gradeReferenceComparisons, item => item.GradeLevelMeanAbsolutePercentageError, item => referenceYearWeights[item.Year]);
        var highSchoolReferenceComparisons = comparisons.Where(item => item.HighSchoolActualTotal > 0).ToList();
        var highSchoolMae = WeightedAverage(highSchoolReferenceComparisons, item => Math.Abs(item.HighSchoolError), item => referenceYearWeights[item.Year]);
        var highSchoolMape = WeightedAverage(highSchoolReferenceComparisons, item => item.HighSchoolAbsolutePercentageError, item => referenceYearWeights[item.Year]);
        var highSchoolGradeMae = WeightedAverage(highSchoolReferenceComparisons, item => item.HighSchoolGradeMeanAbsoluteError, item => referenceYearWeights[item.Year]);
        var highSchoolGradeMape = WeightedAverage(highSchoolReferenceComparisons, item => item.HighSchoolGradeMeanAbsolutePercentageError, item => referenceYearWeights[item.Year]);
        var scoreWeights = NormalizeScoreWeights(parameters);
        var score = gridMape * scoreWeights.Total
            + gridLevelMape * scoreWeights.Grid
            + gradeLevelMape * scoreWeights.Grade
            + highSchoolMape * scoreWeights.HighSchoolTotal
            + highSchoolGradeMape * scoreWeights.HighSchoolGrade;

        return new MonteCarloValidationResult(parameters, comparisons, simulationInfo, syntheticHomeDiagnostics, gridMae, gridMape, gridLevelMae, gridLevelMape, gradeMae, gradeMape, gradeLevelMae, gradeLevelMape, highSchoolMae, highSchoolMape, highSchoolGradeMae, highSchoolGradeMape, score);
    }

    private Dictionary<int, YearAccumulator> CreateYearAccumulators(int startYear, int endYear)
    {
        return Enumerable.Range(startYear, endYear - startYear + 1)
            .ToDictionary(
                year => year,
                _ => new YearAccumulator(data.GridRows.Select(row => row.Name), GradeOrder));
    }

    private int FirstHomeBuildYear()
    {
        return data.HomeRows
            .SelectMany(row => row.HomesByYear.Keys)
            .DefaultIfEmpty(data.AugustYears.FirstOrDefault(2003))
            .Min();
    }

    private int LastHomeBuildYear()
    {
        return data.HomeRows
            .SelectMany(row => row.HomesByYear.Keys)
            .DefaultIfEmpty(data.AugustYears.LastOrDefault(2024))
            .Max();
    }

    private bool HasActualGradeData(int year)
    {
        return data.GradeRows.Any(row => (ActualSeriesValueOrNull(row.Name, year, data.GradeRows) ?? 0) > 0);
    }

    private bool HasActualGridData(int year)
    {
        return data.GridRows.Any(row => (ActualSeriesValueOrNull(row.Name, year, data.GridRows) ?? 0) > 0);
    }

    private IReadOnlyList<MonteCarloSyntheticHomeDiagnostic> BuildSyntheticHomeDiagnostics(int baselineYear, MonteCarloParameters parameters)
    {
        var baselineGradeShares = BuildDistrictGradeShares(data, baselineYear);
        var expectedStudentsPerHome = Math.Max(0.25, ExpectedStudentsPerNewHousehold(parameters));

        return data.GridRows
            .Where(grid => !IsExcludedValidationGrid(grid.Name))
            .Select(grid =>
            {
                var adjustedBaselineStudents = AllocateGradesForGrid(
                        grid.Name,
                        ActualGridValue(grid.Name, baselineYear),
                        baselineGradeShares)
                    .Values
                    .Sum();
                var listedHomes = data.HomeRows
                    .Where(row => row.Neighborhood.Equals(grid.Name, StringComparison.OrdinalIgnoreCase))
                    .Sum(row => row.HomesByYear
                        .Where(kvp => kvp.Key <= baselineYear)
                        .Sum(kvp => kvp.Value));
                var syntheticHomes = listedHomes <= 0 && adjustedBaselineStudents > 0
                    ? Math.Ceiling(adjustedBaselineStudents / expectedStudentsPerHome)
                    : 0;

                return new MonteCarloSyntheticHomeDiagnostic(
                    grid.Name,
                    baselineYear,
                    adjustedBaselineStudents,
                    listedHomes,
                    expectedStudentsPerHome,
                    syntheticHomes);
            })
            .Where(row => row.SyntheticHomes > 0)
            .OrderByDescending(row => row.SyntheticHomes)
            .ThenBy(row => row.Grid, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public MonteCarloSearchResult FindBest(MonteCarloSearchRequest request)
    {
        var candidates = new List<MonteCarloValidationResult>();
        var index = 0;
        var baseParameters = Sanitize(request.BaseParameters ?? new MonteCarloParameters(
            Runs: request.RunsPerCandidate,
            Seed: request.Seed));
        var exitCandidates = request.StudentExitProbabilities.Count > 0
            ? request.StudentExitProbabilities.Select(value => (double?)value)
            : new double?[] { null };

        foreach (var turnover in request.OwnershipChangeProbabilities.DefaultIfEmpty(0.04))
        foreach (var zero in request.MoveInZeroChildShares.DefaultIfEmpty(0.05))
        foreach (var one in request.MoveInOneChildShares.DefaultIfEmpty(0.30))
        foreach (var two in request.MoveInTwoChildShares.DefaultIfEmpty(0.60))
        foreach (var three in request.MoveInThreeChildShares.DefaultIfEmpty(0.05))
        foreach (var fourChild in request.MoveInFourChildShares.DefaultIfEmpty(0.0))
        foreach (var exit in exitCandidates)
        {
            var parameters = baseParameters with
            {
                Runs = request.RunsPerCandidate,
                Seed = request.Seed + index * 7919,
                OwnershipChangeProbability = turnover,
                MoveInZeroChildShare = zero,
                MoveInOneChildShare = one,
                MoveInTwoChildShare = two,
                MoveInThreeChildShare = three,
                MoveInFourChildShare = fourChild
            };
            if (exit is double exitProbability)
            {
                parameters = parameters with
                {
                    StudentExitProbability = exitProbability,
                    Tk8ExitProbability = exitProbability,
                    HighSchoolExitProbability = exitProbability,
                    SpecialExitProbability = exitProbability
                };
            }

            candidates.Add(Validate(new MonteCarloValidationRequest(request.StartYear, request.EndYear, parameters)));
            index++;
        }

        return new MonteCarloSearchResult(
            candidates.OrderBy(item => item.CombinedScore).First(),
            candidates.OrderBy(item => item.CombinedScore).ToList());
    }

    public MonteCarloLifecycleResult SimulateLifecycle(MonteCarloLifecycleRequest request)
    {
        var years = Math.Clamp(request.Years, 1, 100);
        var runs = Math.Clamp(request.Runs, 1, 10000);
        var homesPerRun = Math.Clamp(request.HomesPerRun, 1, 100000);
        var parameters = Sanitize(request.Parameters with { Runs = runs });
        var accumulators = Enumerable.Range(0, years + 1)
            .ToDictionary(year => year, _ => new LifecycleAccumulator());
        var initialDistribution = new ChildCountDistributionAccumulator();
        var turnoverDistribution = new ChildCountDistributionAccumulator();
        var completedFamilyLongevity = new FamilyLongevityAccumulator();
        var activeAtEndFamilyLongevity = new FamilyLongevityAccumulator();
        var turnoverByYear = Enumerable.Range(0, years)
            .ToDictionary(year => year, _ => new TurnoverAccumulator());

        for (var run = 0; run < runs; run++)
        {
            var random = new Random(parameters.Seed + run);
            var homes = new List<SimHome>();
            for (var i = 0; i < homesPerRun; i++)
            {
                var home = new SimHome(request.Grid, request.Density, 0, 0, 0);
                home.Children.AddRange(GenerateHouseholdChildren(request.Grid, request.Density, parameters, random));
                initialDistribution.Add(home.Children.Count);
                homes.Add(home);
            }

            for (var year = 0; year <= years; year++)
            {
                accumulators[year].Add(homes, year);
                if (year < years)
                {
                    turnoverByYear[year].AddActiveHomes(homes.Count(home => home.IsActive(year)));
                    AdvanceHomes(
                        homes,
                        year,
                        parameters,
                        random,
                        turnoverDistribution.Add,
                        completedFamilyLongevity.Add,
                        turnoverObserver: turnoverByYear[year].AddTurnover);
                }
            }

            foreach (var home in homes.Where(home => home.IsActive(years)))
            {
                activeAtEndFamilyLongevity.Add(Math.Max(0, years - home.OwnershipStartYear));
            }
        }

        var denominator = runs * homesPerRun;
        var resultYears = accumulators
            .OrderBy(kvp => kvp.Key)
            .Select(kvp => kvp.Value.ToResult(kvp.Key, denominator))
            .ToList();

        return new MonteCarloLifecycleResult(
            parameters,
            initialDistribution.ToResult(),
            turnoverDistribution.ToResult(),
            ToChildCountDistribution(resultYears.Last(), denominator),
            ToRealizedTurnoverRate(turnoverByYear.Values),
            turnoverByYear.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value.ToResult(kvp.Key)).ToList(),
            completedFamilyLongevity.ToResult(),
            activeAtEndFamilyLongevity.ToResult(),
            resultYears);
    }

    private List<SimHome> InitializeHomes(
        int baselineYear,
        IReadOnlyList<ScenarioHome> scenarioHomes,
        MonteCarloParameters parameters,
        Random random)
    {
        var homes = new List<SimHome>();
        var baselineGradeShares = BuildDistrictGradeShares(data, baselineYear);
        var baselineGrades = data.GridRows.ToDictionary(
            grid => grid.Name,
            grid => IsExcludedValidationGrid(grid.Name)
                ? GradeOrder.ToDictionary(grade => grade, _ => 0.0, StringComparer.OrdinalIgnoreCase)
                : AllocateGradesForGrid(grid.Name, ActualGridValue(grid.Name, baselineYear), baselineGradeShares),
            StringComparer.OrdinalIgnoreCase);

        var firstBuildYear = data.HomeRows
            .SelectMany(row => row.HomesByYear.Keys)
            .Where(year => year <= baselineYear)
            .DefaultIfEmpty(baselineYear)
            .Min();

        for (var year = firstBuildYear; year <= baselineYear; year++)
        {
            AddBuiltHomes(homes, year, scenarioHomes, parameters, random);
            if (year < baselineYear)
            {
                AdvanceHomes(homes, year, parameters, random);
            }
        }

        foreach (var (grid, grades) in baselineGrades)
        {
            if (IsExcludedValidationGrid(grid)) continue;

            var existingHomes = homes.Count(home =>
                home.Grid.Equals(grid, StringComparison.OrdinalIgnoreCase)
                && home.IsActive(baselineYear));
            if (existingHomes == 0 && grades.Values.Sum() > 0)
            {
                var expectedStudentsPerHome = Math.Max(0.25, ExpectedStudentsPerNewHousehold(parameters));
                var syntheticHomes = Math.Ceiling(grades.Values.Sum() / expectedStudentsPerHome);
                AddPlayedBackSyntheticHomes(homes, grid, baselineYear, syntheticHomes, parameters, random);
            }

            ReconcileBaselineChildren(homes, grid, baselineYear, grades, random);
        }

        SeedBaselineKindergartenPipeline(homes, baselineYear, random);

        return homes;
    }

    private void AddPlayedBackSyntheticHomes(
        List<SimHome> homes,
        string grid,
        int baselineYear,
        double count,
        MonteCarloParameters parameters,
        Random random)
    {
        var syntheticBuildYear = Math.Max(data.AugustYears.First(), baselineYear - 20);
        var startIndex = homes.Count;
        AddHomes(homes, grid, "Existing", syntheticBuildYear, count, generateChildren: true, parameters, random);
        var syntheticHomes = homes.Skip(startIndex).ToList();

        for (var year = syntheticBuildYear; year < baselineYear; year++)
        {
            AdvanceHomes(syntheticHomes, year, parameters, random);
        }
    }

    private void AddBuiltHomes(
        List<SimHome> homes,
        int year,
        IReadOnlyList<ScenarioHome> scenarioHomes,
        MonteCarloParameters parameters,
        Random random)
    {
        foreach (var homeRow in data.HomeRows)
        {
            if (IsExcludedValidationGrid(homeRow.Neighborhood)) continue;

            var count = homeRow.HomesByYear.GetValueOrDefault(year);
            AddHomes(homes, homeRow.Neighborhood, homeRow.Density, year, count, generateChildren: true, parameters, random);
        }

        foreach (var home in scenarioHomes.Where(home => home.Year == year))
        {
            AddHomes(homes, home.Neighborhood, home.Density, year, home.Homes, generateChildren: true, parameters, random);
        }
    }

    private void AddHomes(
        List<SimHome> homes,
        string grid,
        string density,
        int year,
        double count,
        bool generateChildren,
        MonteCarloParameters parameters,
        Random random)
    {
        var wholeHomes = (int)Math.Floor(Math.Max(0, count));
        var fractional = Math.Max(0, count) - wholeHomes;
        if (random.NextDouble() < fractional) wholeHomes++;

        for (var i = 0; i < wholeHomes; i++)
        {
            var activeSchoolYear = generateChildren && random.NextDouble() > parameters.SameSchoolYearProbability
                ? year + 1
                : year;
            var home = new SimHome(grid, density, year, activeSchoolYear, activeSchoolYear);
            if (generateChildren)
            {
                home.Children.AddRange(GenerateHouseholdChildren(grid, density, parameters, random));
            }

            homes.Add(home);
        }
    }

    private void AdvanceHomes(
        List<SimHome> homes,
        int year,
        MonteCarloParameters parameters,
        Random random,
        Action<int>? turnoverChildCountObserver = null,
        Action<int>? turnoverLongevityObserver = null,
        Action<SimHome, int>? segmentTurnoverObserver = null,
        Action? turnoverObserver = null)
    {
        foreach (var home in homes)
        {
            if (!home.IsActive(year)) continue;

            if (random.NextDouble() < OwnershipChangeProbabilityForHome(home, parameters))
            {
                turnoverObserver?.Invoke();
                var longevity = Math.Max(0, home.CurrentYear - home.OwnershipStartYear);
                turnoverLongevityObserver?.Invoke(longevity);
                segmentTurnoverObserver?.Invoke(home, longevity);
                home.Children.Clear();
                home.Children.AddRange(GenerateHouseholdChildren(home.Grid, home.Density, parameters, random));
                turnoverChildCountObserver?.Invoke(home.Children.Count);
                home.OwnershipStartYear = home.CurrentYear;
                home.CurrentYear++;
                continue;
            }

            for (var i = home.Children.Count - 1; i >= 0; i--)
            {
                var child = home.Children[i];
                if (child.IsStudent && random.NextDouble() < ExitProbability(child, parameters))
                {
                    home.Children.RemoveAt(i);
                    continue;
                }

                child.Advance();
            }

            var newChildProbability = NewChildProbability(home, parameters);
            if (random.NextDouble() < newChildProbability)
            {
                home.Children.Add(SimChild.Newborn(DrawSpecialEducation(parameters, random)));
            }

            home.CurrentYear++;
        }
    }

    private IEnumerable<SimChild> GenerateHouseholdChildren(string grid, string density, MonteCarloParameters parameters, Random random)
    {
        var childCount = DrawMoveInChildCount(density, parameters, random);
        for (var i = 0; i < childCount; i++)
        {
            yield return new SimChild(DrawGradeIndex(grid, parameters, random), DrawSpecialEducation(parameters, random));
        }
    }

    private void ReconcileBaselineChildren(List<SimHome> homes, string grid, int baselineYear, Dictionary<string, double> grades, Random random)
    {
        var gridHomes = homes
            .Where(home => home.Grid.Equals(grid, StringComparison.OrdinalIgnoreCase))
            .Where(home => home.IsActive(baselineYear))
            .ToList();
        if (gridHomes.Count == 0) return;

        foreach (var (grade, value) in grades)
        {
            if (grade.Equals("TK", StringComparison.OrdinalIgnoreCase)) continue;

            var target = (int)Math.Round(Math.Max(0, value));
            var current = gridHomes.Sum(home => home.Children.Count(child => ChildMatchesGrade(child, grade)));
            var difference = target - current;
            if (difference > 0)
            {
                AddBaselineChildren(gridHomes, grade, difference, random);
            }
            else if (difference < 0)
            {
                RemoveBaselineChildren(gridHomes, grade, -difference, random);
            }
        }
    }

    private static bool ChildMatchesGrade(SimChild child, string grade)
    {
        return grade.Equals("Sp. Ed.", StringComparison.OrdinalIgnoreCase)
            ? child.IsSpecialEducation && child.IsStudent
            : !child.IsSpecialEducation && child.Grade.Equals(grade, StringComparison.OrdinalIgnoreCase);
    }

    private static void AddBaselineChildren(List<SimHome> gridHomes, string grade, int count, Random random)
    {
        for (var i = 0; i < count; i++)
        {
            var isSpecialEducation = grade.Equals("Sp. Ed.", StringComparison.OrdinalIgnoreCase);
            var gradeIndex = isSpecialEducation
                ? DrawFromIndices(Enumerable.Range(0, 14).ToArray(), random)
                : GradeIndexFor(grade);
            gridHomes[random.Next(gridHomes.Count)].Children.Add(new SimChild(gradeIndex, isSpecialEducation));
        }
    }

    private static void RemoveBaselineChildren(List<SimHome> gridHomes, string grade, int count, Random random)
    {
        for (var i = 0; i < count; i++)
        {
            var candidates = gridHomes
                .Where(home => home.Children.Any(child => ChildMatchesGrade(child, grade)))
                .ToList();
            if (candidates.Count == 0) return;

            var home = candidates[random.Next(candidates.Count)];
            var childIndexes = home.Children
                .Select((child, index) => (child, index))
                .Where(item => ChildMatchesGrade(item.child, grade))
                .Select(item => item.index)
                .ToList();
            home.Children.RemoveAt(childIndexes[random.Next(childIndexes.Count)]);
        }
    }

    private void SeedBaselineKindergartenPipeline(List<SimHome> homes, int baselineYear, Random random)
    {
        var kindergartenYear = baselineYear + 1;
        var actualGrades = data.GradeRows.ToDictionary(
            row => row.Name,
            row => ActualSeriesValueOrNull(row.Name, kindergartenYear, data.GradeRows),
            StringComparer.OrdinalIgnoreCase);
        if (!actualGrades.GetValueOrDefault("K").HasValue)
        {
            return;
        }

        var actualGridTotals = data.GridRows.ToDictionary(
            row => row.Name,
            row => ActualGridValueOrNull(row.Name, kindergartenYear),
            StringComparer.OrdinalIgnoreCase);
        var adjustedGrades = BuildAdjustedActualGrades(kindergartenYear, actualGrades, actualGridTotals);
        var kindergartenTarget = adjustedGrades.GetValueOrDefault("K");
        if (kindergartenTarget <= 0)
        {
            return;
        }

        var includedGridTotals = actualGridTotals
            .Where(kvp => !IsExcludedValidationGrid(kvp.Key))
            .Select(kvp => (Grid: kvp.Key, Total: kvp.Value ?? ActualGridValue(kvp.Key, baselineYear)))
            .Where(item => item.Total > 0)
            .ToList();
        var totalGridStudents = includedGridTotals.Sum(item => item.Total);
        if (totalGridStudents <= 0)
        {
            return;
        }

        foreach (var (grid, gridTotal) in includedGridTotals)
        {
            var gridHomes = homes
                .Where(home => home.Grid.Equals(grid, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (gridHomes.Count == 0) continue;

            var target = (int)Math.Round(kindergartenTarget * gridTotal / totalGridStudents);
            var current = gridHomes.Sum(home => home.Children.Count(IsKindergartenReadyChild));
            var difference = target - current;
            if (difference > 0)
            {
                AddBaselineKindergartenReadyChildren(gridHomes, difference, random);
            }
            else if (difference < 0)
            {
                RemoveBaselineKindergartenReadyChildren(gridHomes, -difference, random);
            }
        }
    }

    private static bool IsKindergartenReadyChild(SimChild child)
    {
        return !child.IsSpecialEducation && child.GradeIndex == 0;
    }

    private static void AddBaselineKindergartenReadyChildren(List<SimHome> gridHomes, int count, Random random)
    {
        for (var i = 0; i < count; i++)
        {
            gridHomes[random.Next(gridHomes.Count)].Children.Add(new SimChild(0, isSpecialEducation: false));
        }
    }

    private static void RemoveBaselineKindergartenReadyChildren(List<SimHome> gridHomes, int count, Random random)
    {
        for (var i = 0; i < count; i++)
        {
            var candidates = gridHomes
                .Where(home => home.Children.Any(IsKindergartenReadyChild))
                .ToList();
            if (candidates.Count == 0) return;

            var home = candidates[random.Next(candidates.Count)];
            var childIndexes = home.Children
                .Select((child, index) => (child, index))
                .Where(item => IsKindergartenReadyChild(item.child))
                .Select(item => item.index)
                .ToList();
            home.Children.RemoveAt(childIndexes[random.Next(childIndexes.Count)]);
        }
    }

    private Dictionary<string, double> AllocateGradesForGrid(string grid, double total, Dictionary<string, double> gradeShares)
    {
        var shares = SharesForGrid(grid, gradeShares);
        return GradeOrder.ToDictionary(
            grade => grade,
            grade => total * shares.GetValueOrDefault(grade),
            StringComparer.OrdinalIgnoreCase);
    }

    private Dictionary<string, double> SharesForGrid(string grid, Dictionary<string, double> gradeShares)
    {
        if (grid.Equals("MHESD", StringComparison.OrdinalIgnoreCase))
        {
            return Normalize(HighSchoolGrades.ToDictionary(
                grade => grade,
                grade => gradeShares.GetValueOrDefault(grade),
                StringComparer.OrdinalIgnoreCase));
        }

        return new Dictionary<string, double>(gradeShares, StringComparer.OrdinalIgnoreCase);
    }

    private int DrawGradeIndex(string grid, MonteCarloParameters parameters, Random random)
    {
        if (grid.Equals("MHESD", StringComparison.OrdinalIgnoreCase))
        {
            return DrawFromIndices([10, 11, 12, 13], random);
        }

        var tkK = Math.Max(0, parameters.MoveInTkKWeight);
        var elementary = Math.Max(0, parameters.MoveInElementaryWeight);
        var middle = Math.Max(0, parameters.MoveInMiddleWeight);
        var high = Math.Max(0, parameters.MoveInHighWeight);
        var preschool = Math.Max(0, parameters.MoveInPreschoolWeight);
        var postSchool = Math.Max(0, parameters.MoveInPostSchoolWeight);
        var total = preschool + tkK + elementary + middle + high + postSchool;
        if (total <= 0)
        {
            return DrawFromIndices(Enumerable.Range(0, 14).ToArray(), random);
        }

        var roll = random.NextDouble();
        var cumulative = 0.0;
        cumulative += preschool / total;
        if (roll <= cumulative) return DrawFromIndices([-4, -3, -2, -1], random);
        cumulative += tkK / total;
        if (roll <= cumulative) return DrawFromIndices([0, 1], random);
        cumulative += elementary / total;
        if (roll <= cumulative) return DrawFromIndices([2, 3, 4, 5, 6], random);
        cumulative += middle / total;
        if (roll <= cumulative) return DrawFromIndices([7, 8, 9], random);
        cumulative += high / total;
        if (roll <= cumulative) return DrawFromIndices([10, 11, 12, 13], random);
        return PostSchoolChildIndex;
    }

    private MonteCarloYearComparison BuildComparison(int year, YearAccumulator accumulator, MonteCarloParameters parameters)
    {
        var runs = Math.Max(1, parameters.Runs);
        var modeledGridTotals = accumulator.GridTotals.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value / runs,
            StringComparer.OrdinalIgnoreCase);
        var modeledGridTotalStandardDeviations = accumulator.GridTotalSquares.ToDictionary(
            kvp => kvp.Key,
            kvp => StandardDeviation(accumulator.GridTotals.GetValueOrDefault(kvp.Key), kvp.Value, runs),
            StringComparer.OrdinalIgnoreCase);
        var modeledGridGrades = accumulator.GridGrades.ToDictionary(
            grid => grid.Key,
            grid => grid.Value.ToDictionary(
                grade => grade.Key,
                grade => grade.Value / runs,
                StringComparer.OrdinalIgnoreCase),
            StringComparer.OrdinalIgnoreCase);
        var modeledGridGradeStandardDeviations = accumulator.GridGradeSquares.ToDictionary(
            grid => grid.Key,
            grid => grid.Value.ToDictionary(
                grade => grade.Key,
                grade => StandardDeviation(
                    accumulator.GridGrades.GetValueOrDefault(grid.Key)?.GetValueOrDefault(grade.Key) ?? 0,
                    grade.Value,
                    runs),
                StringComparer.OrdinalIgnoreCase),
            StringComparer.OrdinalIgnoreCase);
        var modeledGrades = accumulator.GradeTotals.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value / runs,
            StringComparer.OrdinalIgnoreCase);
        var modeledGradeStandardDeviations = accumulator.GradeTotalSquares.ToDictionary(
            kvp => kvp.Key,
            kvp => StandardDeviation(accumulator.GradeTotals.GetValueOrDefault(kvp.Key), kvp.Value, runs),
            StringComparer.OrdinalIgnoreCase);
        var actualGridTotals = data.GridRows.ToDictionary(
            grid => grid.Name,
            grid => ActualGridValueOrNull(grid.Name, year),
            StringComparer.OrdinalIgnoreCase);
        var actualGrades = data.GradeRows.ToDictionary(
            grade => grade.Name,
            grade => ActualSeriesValueOrNull(grade.Name, year, data.GradeRows),
            StringComparer.OrdinalIgnoreCase);
        var adjustedActualGrades = BuildAdjustedActualGrades(year, actualGrades, actualGridTotals);

        var actualGridTotal = actualGridTotals
            .Where(kvp => !IsExcludedValidationGrid(kvp.Key))
            .Sum(kvp => kvp.Value ?? 0);
        var modeledGridTotal = modeledGridTotals
            .Where(kvp => !IsExcludedValidationGrid(kvp.Key))
            .Sum(kvp => kvp.Value);
        var modeledGridTotalStdDev = StandardDeviation(
            accumulator.GridTotalPerRunSum,
            accumulator.GridTotalPerRunSquareSum,
            runs);
        var actualGradeTotal = GradeOrder.Sum(grade => adjustedActualGrades.GetValueOrDefault(grade));
        var modeledGradeTotal = GradeOrder.Sum(grade => modeledGrades.GetValueOrDefault(grade));
        var modeledGradeTotalStdDev = StandardDeviation(
            accumulator.GradeTotalPerRunSum,
            accumulator.GradeTotalPerRunSquareSum,
            runs);
        var actualHighSchoolTotal = HighSchoolGrades.Sum(grade => adjustedActualGrades.GetValueOrDefault(grade));
        var modeledHighSchoolTotal = HighSchoolGrades.Sum(grade => modeledGrades.GetValueOrDefault(grade));
        var modeledHighSchoolTotalStdDev = StandardDeviation(
            accumulator.HighSchoolTotalPerRunSum,
            accumulator.HighSchoolTotalPerRunSquareSum,
            runs);
        var gridErrors = actualGridTotals
            .Where(kvp => !IsExcludedValidationGrid(kvp.Key))
            .Select(kvp =>
            {
                var actual = kvp.Value ?? 0;
                var modeled = modeledGridTotals.GetValueOrDefault(kvp.Key);
                return (actual, absoluteError: Math.Abs(modeled - actual));
            })
            .Where(item => item.actual > 0)
            .ToList();
        var gridLevelMae = gridErrors.Select(item => item.absoluteError).DefaultIfEmpty(0).Average();
        var gridLevelMape = gridErrors.Select(item => item.absoluteError / item.actual).DefaultIfEmpty(0).Average();
        var gradeErrors = actualGradeTotal > 0
            ? ValidationGrades
                .Select(grade =>
                {
                    var actual = SmoothedAdjustedActualGradeValue(year, grade, parameters.GradeSmoothingWindow);
                    var modeled = modeledGrades.GetValueOrDefault(grade);
                    return (actual, absoluteError: Math.Abs(modeled - actual));
                })
                .Where(item => item.actual > 0)
                .ToList()
            : [];
        var gradeLevelMae = gradeErrors.Select(item => item.absoluteError).DefaultIfEmpty(0).Average();
        var gradeLevelMape = gradeErrors.Select(item => item.absoluteError / item.actual).DefaultIfEmpty(0).Average();
        var highSchoolGradeErrors = actualGradeTotal > 0
            ? HighSchoolGrades
                .Select(grade =>
                {
                    var actual = SmoothedAdjustedActualGradeValue(year, grade, parameters.GradeSmoothingWindow);
                    var modeled = modeledGrades.GetValueOrDefault(grade);
                    return (actual, absoluteError: Math.Abs(modeled - actual));
                })
                .Where(item => item.actual > 0)
                .ToList()
            : [];
        var highSchoolGradeMae = highSchoolGradeErrors.Select(item => item.absoluteError).DefaultIfEmpty(0).Average();
        var highSchoolGradeMape = highSchoolGradeErrors.Select(item => item.absoluteError / item.actual).DefaultIfEmpty(0).Average();

        return new MonteCarloYearComparison(
            year,
            actualGridTotals,
            modeledGridTotals,
            modeledGridTotalStandardDeviations,
            modeledGridGrades,
            modeledGridGradeStandardDeviations,
            actualGrades,
            adjustedActualGrades,
            modeledGrades,
            modeledGradeStandardDeviations,
            actualGridTotal,
            modeledGridTotal,
            modeledGridTotalStdDev,
            modeledGridTotal - actualGridTotal,
            actualGridTotal > 0 ? Math.Abs(modeledGridTotal - actualGridTotal) / actualGridTotal : 0,
            gridLevelMae,
            gridLevelMape,
            actualGradeTotal,
            modeledGradeTotal,
            modeledGradeTotalStdDev,
            modeledGradeTotal - actualGradeTotal,
            actualGradeTotal > 0 ? Math.Abs(modeledGradeTotal - actualGradeTotal) / actualGradeTotal : 0,
            gradeLevelMae,
            gradeLevelMape,
            actualHighSchoolTotal,
            modeledHighSchoolTotal,
            modeledHighSchoolTotalStdDev,
            modeledHighSchoolTotal - actualHighSchoolTotal,
            actualHighSchoolTotal > 0 ? Math.Abs(modeledHighSchoolTotal - actualHighSchoolTotal) / actualHighSchoolTotal : 0,
            highSchoolGradeMae,
            highSchoolGradeMape);
    }

    private static MonteCarloSimulationYearInfo BuildSimulationInfo(int year, YearAccumulator accumulator, MonteCarloParameters parameters)
    {
        var runs = Math.Max(1, parameters.Runs);
        var totalHomes = accumulator.TotalHomes / runs;
        var lowHomes = accumulator.LowDensityHomes / runs;
        var mediumHomes = accumulator.MediumDensityHomes / runs;
        var mediumHighHomes = accumulator.MediumHighDensityHomes / runs;
        var highHomes = accumulator.HighDensityHomes / runs;
        var otherHomes = accumulator.OtherDensityHomes / runs;
        var lowMediumHomes = lowHomes + mediumHomes + otherHomes;
        var mediumHighHighHomes = mediumHighHomes + highHomes;
        var k12Students = accumulator.K12Students / runs;
        var k8Students = accumulator.K8Students / runs;
        var highSchoolStudents = accumulator.HighSchoolStudents / runs;
        var turnoverEvents = accumulator.TurnoverEvents / runs;

        return new MonteCarloSimulationYearInfo(
            year,
            totalHomes,
            lowHomes,
            mediumHomes,
            mediumHighHomes,
            highHomes,
            otherHomes,
            k12Students,
            accumulator.TkStudents / runs,
            k8Students,
            highSchoolStudents,
            Ratio(k12Students, totalHomes),
            Ratio(k8Students, totalHomes),
            Ratio(highSchoolStudents, totalHomes),
            Ratio(accumulator.LowMediumK12Students / runs, lowMediumHomes),
            Ratio(accumulator.LowMediumK8Students / runs, lowMediumHomes),
            Ratio(accumulator.LowMediumHighSchoolStudents / runs, lowMediumHomes),
            Ratio(accumulator.MediumHighHighK12Students / runs, mediumHighHighHomes),
            Ratio(accumulator.MediumHighHighK8Students / runs, mediumHighHighHomes),
            Ratio(accumulator.MediumHighHighHighSchoolStudents / runs, mediumHighHighHomes),
            accumulator.ChildCountBuckets[0] / runs,
            accumulator.ChildCountBuckets[1] / runs,
            accumulator.ChildCountBuckets[2] / runs,
            accumulator.ChildCountBuckets[3] / runs,
            accumulator.ChildCountBuckets[4] / runs,
            Ratio(accumulator.ChildCountBuckets[0], accumulator.TotalHomes),
            Ratio(accumulator.ChildCountBuckets[1], accumulator.TotalHomes),
            Ratio(accumulator.ChildCountBuckets[2], accumulator.TotalHomes),
            Ratio(accumulator.ChildCountBuckets[3], accumulator.TotalHomes),
            Ratio(accumulator.ChildCountBuckets[4], accumulator.TotalHomes),
            turnoverEvents,
            Ratio(accumulator.TurnoverEvents, accumulator.TotalHomes),
            Ratio(accumulator.TurnoverLongevityTotal, accumulator.TurnoverEvents),
            accumulator.Segments.Values
                .Select(segment => segment.ToInfo(runs))
                .OrderBy(segment => segment.Grid, StringComparer.OrdinalIgnoreCase)
                .ThenBy(segment => segment.Density, StringComparer.OrdinalIgnoreCase)
                .ToList());
    }

    private double ActualGridValue(string grid, int year)
    {
        return ActualGridValueOrNull(grid, year) ?? 0;
    }

    private double? ActualGridValueOrNull(string grid, int year)
    {
        return ActualSeriesValueOrNull(grid, year, data.GridRows);
    }

    private Dictionary<string, double> BuildAdjustedActualGrades(
        int year,
        Dictionary<string, double?> actualGrades,
        Dictionary<string, double?> actualGridTotals)
    {
        var adjusted = GradeOrder.ToDictionary(
            grade => grade,
            grade => actualGrades.GetValueOrDefault(grade) ?? 0,
            StringComparer.OrdinalIgnoreCase);
        var gradeShares = Normalize(adjusted);

        foreach (var grid in ExcludedValidationGrids)
        {
            var total = actualGridTotals.GetValueOrDefault(grid) ?? ActualGridValue(grid, year);
            if (total <= 0) continue;

            var shares = SharesForGrid(grid, gradeShares);
            foreach (var grade in GradeOrder)
            {
                adjusted[grade] = Math.Max(0, adjusted.GetValueOrDefault(grade) - total * shares.GetValueOrDefault(grade));
            }
        }

        return adjusted;
    }

    private static double? ActualSeriesValueOrNull(string name, int year, IReadOnlyList<SeriesRow> rows)
    {
        var term = $"Aug '{year % 100:00}";
        return rows.FirstOrDefault(row => row.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            ?.Values.GetValueOrDefault(term);
    }

    private double SmoothedActualGradeValue(int year, string grade, int smoothingWindow)
    {
        var gradeIndex = GradeIndexFor(grade);
        var window = Math.Clamp(smoothingWindow, 0, 2);
        var weightedTotal = 0.0;
        var weightTotal = 0.0;

        for (var offset = -window; offset <= window; offset++)
        {
            var comparisonYear = year + offset;
            if (comparisonYear < data.AugustYears.First() || comparisonYear > data.AugustYears.Last())
            {
                continue;
            }

            var comparisonGradeIndex = gradeIndex + offset;
            if (comparisonGradeIndex is < 1 or > 13)
            {
                continue;
            }

            var comparisonGrade = GradeForIndex(comparisonGradeIndex);
            var actual = ActualSeriesValueOrNull(comparisonGrade, comparisonYear, data.GradeRows);
            if (!actual.HasValue)
            {
                continue;
            }

            var weight = offset == 0 ? Math.Max(1, window * 2) : window + 1 - Math.Abs(offset);
            weightedTotal += actual.Value * weight;
            weightTotal += weight;
        }

        return weightTotal > 0 ? weightedTotal / weightTotal : ActualSeriesValueOrNull(grade, year, data.GradeRows) ?? 0;
    }

    private double SmoothedAdjustedActualGradeValue(int year, string grade, int smoothingWindow)
    {
        var gradeIndex = GradeIndexFor(grade);
        var window = Math.Clamp(smoothingWindow, 0, 2);
        var weightedTotal = 0.0;
        var weightTotal = 0.0;

        for (var offset = -window; offset <= window; offset++)
        {
            var comparisonYear = year + offset;
            if (comparisonYear < data.AugustYears.First() || comparisonYear > data.AugustYears.Last())
            {
                continue;
            }

            var comparisonGradeIndex = gradeIndex + offset;
            if (comparisonGradeIndex is < 1 or > 13)
            {
                continue;
            }

            var comparisonGrade = GradeForIndex(comparisonGradeIndex);
            var actual = AdjustedActualGradeValue(comparisonYear, comparisonGrade);
            if (actual <= 0)
            {
                continue;
            }

            var weight = offset == 0 ? Math.Max(1, window * 2) : window + 1 - Math.Abs(offset);
            weightedTotal += actual * weight;
            weightTotal += weight;
        }

        return weightTotal > 0 ? weightedTotal / weightTotal : AdjustedActualGradeValue(year, grade);
    }

    private double AdjustedActualGradeValue(int year, string grade)
    {
        var actualGrades = data.GradeRows.ToDictionary(
            row => row.Name,
            row => ActualSeriesValueOrNull(row.Name, year, data.GradeRows),
            StringComparer.OrdinalIgnoreCase);
        var actualGridTotals = data.GridRows.ToDictionary(
            row => row.Name,
            row => ActualGridValueOrNull(row.Name, year),
            StringComparer.OrdinalIgnoreCase);
        return BuildAdjustedActualGrades(year, actualGrades, actualGridTotals).GetValueOrDefault(grade);
    }

    private static Dictionary<string, double> BuildDistrictGradeShares(EnrollmentData data, int year)
    {
        var term = $"Aug '{year % 100:00}";
        var counts = GradeOrder.ToDictionary(
            grade => grade,
            grade => data.GradeRows.FirstOrDefault(row => row.Name.Equals(grade, StringComparison.OrdinalIgnoreCase))
                ?.Values.GetValueOrDefault(term) ?? 0,
            StringComparer.OrdinalIgnoreCase);
        return Normalize(counts);
    }

    private static Dictionary<string, double> Normalize(Dictionary<string, double> values)
    {
        var result = GradeOrder.ToDictionary(grade => grade, grade => Math.Max(0, values.GetValueOrDefault(grade)), StringComparer.OrdinalIgnoreCase);
        var total = result.Values.Sum();
        if (total <= 0)
        {
            return GradeOrder.ToDictionary(grade => grade, _ => 1.0 / GradeOrder.Length, StringComparer.OrdinalIgnoreCase);
        }

        return result.ToDictionary(kvp => kvp.Key, kvp => kvp.Value / total, StringComparer.OrdinalIgnoreCase);
    }

    private static double ExitProbability(SimChild child, MonteCarloParameters parameters)
    {
        if (child.IsSpecialEducation) return parameters.SpecialExitProbability;

        return child.GradeIndex switch
        {
            >= 0 and <= 9 => parameters.Tk8ExitProbability,
            >= 10 and <= 13 => parameters.HighSchoolExitProbability,
            _ => parameters.StudentExitProbability
        };
    }

    private static int DrawFromIndices(IReadOnlyList<int> indices, Random random)
    {
        return indices[random.Next(indices.Count)];
    }

    private static int GradeIndexFor(string grade)
    {
        var index = Array.FindIndex(GradeOrder, item => item.Equals(grade, StringComparison.OrdinalIgnoreCase));
        return index < 0 ? 1 : index;
    }

    private static string GradeForIndex(int gradeIndex)
    {
        return gradeIndex >= 0 && gradeIndex < GradeOrder.Length ? GradeOrder[gradeIndex] : "";
    }

    private static MonteCarloParameters Sanitize(MonteCarloParameters parameters)
    {
        var childShares = NormalizeMoveInChildShares(parameters);
        return parameters with
        {
            Runs = Math.Clamp(parameters.Runs, 1, 10000),
            OwnershipChangeProbability = ClampProbability(parameters.OwnershipChangeProbability),
            MoveInZeroChildShare = childShares.Zero,
            MoveInOneChildShare = childShares.One,
            MoveInTwoChildShare = childShares.Two,
            MoveInThreeChildShare = childShares.Three,
            MoveInFourChildShare = childShares.Four,
            StudentExitProbability = ClampProbability(parameters.StudentExitProbability),
            MoveInTkKWeight = Math.Max(0, parameters.MoveInTkKWeight),
            MoveInElementaryWeight = Math.Max(0, parameters.MoveInElementaryWeight),
            MoveInMiddleWeight = Math.Max(0, parameters.MoveInMiddleWeight),
            MoveInHighWeight = Math.Max(0, parameters.MoveInHighWeight),
            MoveInPreschoolWeight = Math.Max(0, parameters.MoveInPreschoolWeight),
            MoveInPostSchoolWeight = Math.Max(0, parameters.MoveInPostSchoolWeight),
            AnnualFirstNewChildProbability = ClampProbability(parameters.AnnualFirstNewChildProbability),
            AnnualSecondNewChildProbability = ClampProbability(parameters.AnnualSecondNewChildProbability),
            AnnualThirdNewChildProbability = ClampProbability(parameters.AnnualThirdNewChildProbability),
            AnnualFourthPlusNewChildProbability = ClampProbability(parameters.AnnualFourthPlusNewChildProbability),
            Tk8ExitProbability = ClampProbability(parameters.Tk8ExitProbability),
            HighSchoolExitProbability = ClampProbability(parameters.HighSchoolExitProbability),
            SpecialExitProbability = ClampProbability(parameters.SpecialExitProbability),
            SameSchoolYearProbability = ClampProbability(parameters.SameSchoolYearProbability),
            SpecialEducationProbability = ClampProbability(parameters.SpecialEducationProbability),
            MaxDegreeOfParallelism = Math.Clamp(parameters.MaxDegreeOfParallelism, 0, Environment.ProcessorCount),
            GradeSmoothingWindow = Math.Clamp(parameters.GradeSmoothingWindow, 0, 2),
            ScoreTotalWeight = Math.Max(0, parameters.ScoreTotalWeight),
            ScoreGridWeight = Math.Max(0, parameters.ScoreGridWeight),
            ScoreGradeWeight = Math.Max(0, parameters.ScoreGradeWeight),
            ScoreHighSchoolTotalWeight = Math.Max(0, parameters.ScoreHighSchoolTotalWeight),
            ScoreHighSchoolGradeWeight = Math.Max(0, parameters.ScoreHighSchoolGradeWeight),
            AnchorYearWeight = Math.Clamp(parameters.AnchorYearWeight, 0, 10),
            YearWeightSlope = Math.Clamp(parameters.YearWeightSlope, 0, 10),
            YearWeightCap = Math.Clamp(parameters.YearWeightCap, 0.01, 100),
            DensityLowFactor = Math.Clamp(parameters.DensityLowFactor, 0.2, 2.0),
            DensityMediumFactor = Math.Clamp(parameters.DensityMediumFactor, 0.2, 2.0),
            DensityMediumHighFactor = Math.Clamp(parameters.DensityMediumHighFactor, 0.1, 2.0),
            DensityHighFactor = Math.Clamp(parameters.DensityHighFactor, 0.0, 2.0),
            DensityLowFirstChildFactor = Math.Clamp(parameters.DensityLowFirstChildFactor, 0.5, 2.0),
            DensityLowSecondChildFactor = Math.Clamp(parameters.DensityLowSecondChildFactor, 0.5, 2.0),
            DensityLowThirdChildFactor = Math.Clamp(parameters.DensityLowThirdChildFactor, 0.0, 2.0),
            DensityLowFourthChildFactor = Math.Clamp(parameters.DensityLowFourthChildFactor, 0.0, 2.0),
            DensityMediumFirstChildFactor = Math.Clamp(parameters.DensityMediumFirstChildFactor, 0.5, 2.0),
            DensityMediumSecondChildFactor = Math.Clamp(parameters.DensityMediumSecondChildFactor, 0.5, 2.0),
            DensityMediumThirdChildFactor = Math.Clamp(parameters.DensityMediumThirdChildFactor, 0.0, 2.0),
            DensityMediumFourthChildFactor = Math.Clamp(parameters.DensityMediumFourthChildFactor, 0.0, 2.0),
            DensityMediumHighFirstChildFactor = Math.Clamp(parameters.DensityMediumHighFirstChildFactor, 0.2, 2.0),
            DensityMediumHighSecondChildFactor = Math.Clamp(parameters.DensityMediumHighSecondChildFactor, 0.0, 2.0),
            DensityMediumHighThirdChildFactor = Math.Clamp(parameters.DensityMediumHighThirdChildFactor, 0.0, 2.0),
            DensityMediumHighFourthChildFactor = Math.Clamp(parameters.DensityMediumHighFourthChildFactor, 0.0, 2.0),
            DensityHighFirstChildFactor = Math.Clamp(parameters.DensityHighFirstChildFactor, 0.2, 2.0),
            DensityHighSecondChildFactor = Math.Clamp(parameters.DensityHighSecondChildFactor, 0.0, 2.0),
            DensityHighThirdChildFactor = Math.Clamp(parameters.DensityHighThirdChildFactor, 0.0, 2.0),
            DensityHighFourthChildFactor = Math.Clamp(parameters.DensityHighFourthChildFactor, 0.0, 2.0)
        };
    }

    private static double ClampProbability(double value) => Math.Clamp(value, 0, 1);

    private static double ReferenceYearWeight(int year, int startYear, bool anchorStartYear, MonteCarloParameters parameters)
    {
        if (anchorStartYear && year == startYear)
        {
            return parameters.AnchorYearWeight;
        }

        var yearsAfterStart = Math.Max(0, year - startYear);
        var weight = 1.0 + yearsAfterStart * parameters.YearWeightSlope;
        return Math.Min(parameters.YearWeightCap, weight);
    }

    private static double WeightedAverage<T>(
        IReadOnlyCollection<T> items,
        Func<T, double> valueSelector,
        Func<T, double> weightSelector)
    {
        var weightTotal = 0.0;
        var weightedValueTotal = 0.0;
        foreach (var item in items)
        {
            var weight = Math.Max(0, weightSelector(item));
            if (weight <= 0)
            {
                continue;
            }

            weightTotal += weight;
            weightedValueTotal += valueSelector(item) * weight;
        }

        return weightTotal > 0 ? weightedValueTotal / weightTotal : 0;
    }

    private static double Ratio(double numerator, double denominator)
    {
        return denominator > 0 ? numerator / denominator : 0;
    }

    private static double Variance(double sum, double squareSum, double count)
    {
        if (count <= 1)
        {
            return 0;
        }

        var mean = sum / count;
        return Math.Max(0, (squareSum / count) - (mean * mean));
    }

    private static double StandardDeviation(double sum, double squareSum, double count)
    {
        return Math.Sqrt(Variance(sum, squareSum, count));
    }

    private static bool IsExcludedValidationGrid(string grid)
    {
        return ExcludedValidationGrids.Contains(grid);
    }

    private static (double Total, double Grid, double Grade, double HighSchoolTotal, double HighSchoolGrade) NormalizeScoreWeights(MonteCarloParameters parameters)
    {
        var total = Math.Max(0, parameters.ScoreTotalWeight);
        var grid = Math.Max(0, parameters.ScoreGridWeight);
        var grade = Math.Max(0, parameters.ScoreGradeWeight);
        var highSchoolTotal = Math.Max(0, parameters.ScoreHighSchoolTotalWeight);
        var highSchoolGrade = Math.Max(0, parameters.ScoreHighSchoolGradeWeight);
        var sum = total + grid + grade + highSchoolTotal + highSchoolGrade;
        return sum > 0
            ? (total / sum, grid / sum, grade / sum, highSchoolTotal / sum, highSchoolGrade / sum)
            : (0.25, 0.35, 0.15, 0.15, 0.10);
    }

    private static double NewChildProbability(SimHome home, MonteCarloParameters parameters)
    {
        var nextChildNumber = home.Children.Count + 1;
        var baseProbability = nextChildNumber switch
        {
            <= 1 => parameters.AnnualFirstNewChildProbability,
            2 => parameters.AnnualSecondNewChildProbability,
            3 => parameters.AnnualThirdNewChildProbability,
            _ => parameters.AnnualFourthPlusNewChildProbability
        };

        return ClampProbability(baseProbability * DensityBirthMultiplier(home.Density, nextChildNumber, parameters));
    }

    private static double OwnershipChangeProbabilityForHome(SimHome home, MonteCarloParameters parameters)
    {
        var ownershipAge = Math.Max(0, home.CurrentYear - home.OwnershipStartYear);
        if (ownershipAge >= ForcedOwnershipTurnoverYears)
        {
            return 1.0;
        }

        var multiplier = ownershipAge switch
        {
            0 => 0.15,
            1 => 0.35,
            2 => 0.60,
            3 => 0.80,
            >= 26 and <= 30 => 1.25,
            >= 31 and <= 35 => 1.60,
            >= 36 and <= 40 => 2.10,
            >= 41 and <= 45 => 3.00,
            46 => 4.00,
            47 => 5.50,
            48 => 8.00,
            49 => 12.00,
            _ => 1.0
        };

        return ClampProbability(parameters.OwnershipChangeProbability * multiplier);
    }

    private static double ExpectedStudentsPerNewHousehold(MonteCarloParameters parameters)
    {
        var studentWeight =
            Math.Max(0, parameters.MoveInTkKWeight) +
            Math.Max(0, parameters.MoveInElementaryWeight) +
            Math.Max(0, parameters.MoveInMiddleWeight) +
            Math.Max(0, parameters.MoveInHighWeight);
        var totalWeight =
            studentWeight +
            Math.Max(0, parameters.MoveInPreschoolWeight) +
            Math.Max(0, parameters.MoveInPostSchoolWeight);
        var studentShare = totalWeight > 0 ? studentWeight / totalWeight : 1;
        var childShares = NormalizeMoveInChildShares(parameters);
        var expectedChildren =
            childShares.One +
            childShares.Two * 2 +
            childShares.Three * 3 +
            childShares.Four * 4;
        return expectedChildren * studentShare;
    }

    private static bool DrawSpecialEducation(MonteCarloParameters parameters, Random random)
    {
        return random.NextDouble() < parameters.SpecialEducationProbability;
    }

    private static int DrawMoveInChildCount(string density, MonteCarloParameters parameters, Random random)
    {
        var childShares = NormalizeMoveInChildShares(parameters, density);
        var roll = random.NextDouble();
        var cumulative = childShares.Zero;
        if (roll <= cumulative) return 0;
        cumulative += childShares.One;
        if (roll <= cumulative) return 1;
        cumulative += childShares.Two;
        if (roll <= cumulative) return 2;
        cumulative += childShares.Three;
        if (roll <= cumulative) return 3;
        return 4;
    }

    private static (double Zero, double One, double Two, double Three, double Four) NormalizeMoveInChildShares(MonteCarloParameters parameters, string? density = null)
    {
        var densityFactors = DensityMoveInChildFactors(density, parameters);
        var zero = Math.Max(0, parameters.MoveInZeroChildShare);
        var one = Math.Max(0, parameters.MoveInOneChildShare) * densityFactors.One;
        var two = Math.Max(0, parameters.MoveInTwoChildShare) * densityFactors.Two;
        var three = Math.Max(0, parameters.MoveInThreeChildShare) * densityFactors.Three;
        var four = Math.Max(0, parameters.MoveInFourChildShare) * densityFactors.Four;
        var total = zero + one + two + three + four;
        if (total <= 0)
        {
            return (0.05, 0.30, 0.60, 0.05, 0.0);
        }

        return (zero / total, one / total, two / total, three / total, four / total);
    }

    private static (double One, double Two, double Three, double Four) DensityMoveInChildFactors(string? density, MonteCarloParameters parameters)
    {
        var densityName = NormalizeDensity(density);
        var profile = densityName switch
        {
            "RMH" => (0.95, 0.70, 0.20, 0.05),
            "RH" => (0.90, 0.50, 0.05, 0.00),
            _ => (1.00, 1.00, 1.00, 1.00)
        };
        var multiplier = DensityGroupFactor(densityName, parameters);
        return (
            profile.Item1 * multiplier * DensityChildFactor(densityName, 1, parameters),
            profile.Item2 * multiplier * DensityChildFactor(densityName, 2, parameters),
            profile.Item3 * multiplier * DensityChildFactor(densityName, 3, parameters),
            profile.Item4 * multiplier * DensityChildFactor(densityName, 4, parameters));
    }

    private static double DensityBirthMultiplier(string? density, int nextChildNumber, MonteCarloParameters parameters)
    {
        var densityName = NormalizeDensity(density);
        var profile = (densityName, nextChildNumber) switch
        {
            ("RMH", <= 1) => 0.95,
            ("RMH", 2) => 0.65,
            ("RMH", 3) => 0.15,
            ("RMH", _) => 0.05,
            ("RH", <= 1) => 0.85,
            ("RH", 2) => 0.45,
            ("RH", 3) => 0.05,
            ("RH", _) => 0.00,
            _ => 1.00
        };
        return profile * DensityGroupFactor(densityName, parameters) * DensityChildFactor(densityName, nextChildNumber, parameters);
    }

    private static double DensityGroupFactor(string density, MonteCarloParameters parameters)
    {
        return density switch
        {
            "RL" => parameters.DensityLowFactor,
            "RM" => parameters.DensityMediumFactor,
            "RMH" => parameters.DensityMediumHighFactor,
            "RH" => parameters.DensityHighFactor,
            _ => parameters.DensityLowFactor
        };
    }

    private static double DensityChildFactor(string density, int childNumber, MonteCarloParameters parameters)
    {
        return (density, childNumber) switch
        {
            ("RL", <= 1) => parameters.DensityLowFirstChildFactor,
            ("RL", 2) => parameters.DensityLowSecondChildFactor,
            ("RL", 3) => parameters.DensityLowThirdChildFactor,
            ("RL", _) => parameters.DensityLowFourthChildFactor,
            ("RM", <= 1) => parameters.DensityMediumFirstChildFactor,
            ("RM", 2) => parameters.DensityMediumSecondChildFactor,
            ("RM", 3) => parameters.DensityMediumThirdChildFactor,
            ("RM", _) => parameters.DensityMediumFourthChildFactor,
            ("RMH", <= 1) => parameters.DensityMediumHighFirstChildFactor,
            ("RMH", 2) => parameters.DensityMediumHighSecondChildFactor,
            ("RMH", 3) => parameters.DensityMediumHighThirdChildFactor,
            ("RMH", _) => parameters.DensityMediumHighFourthChildFactor,
            ("RH", <= 1) => parameters.DensityHighFirstChildFactor,
            ("RH", 2) => parameters.DensityHighSecondChildFactor,
            ("RH", 3) => parameters.DensityHighThirdChildFactor,
            ("RH", _) => parameters.DensityHighFourthChildFactor,
            (_, <= 1) => parameters.DensityLowFirstChildFactor,
            (_, 2) => parameters.DensityLowSecondChildFactor,
            (_, 3) => parameters.DensityLowThirdChildFactor,
            _ => parameters.DensityLowFourthChildFactor
        };
    }

    private static string NormalizeDensity(string? density)
    {
        return (density ?? "").Trim().ToUpperInvariant();
    }

    private static string DensityLabel(string? density)
    {
        return NormalizeDensity(density) switch
        {
            "RL" => "Low",
            "RM" => "Medium",
            "RMH" => "Medium-high",
            "RH" => "High",
            _ => "Other/existing"
        };
    }

    private sealed class SimHome(string grid, string density, int builtYear, int activeSchoolYear, int ownershipStartYear)
    {
        public string Grid { get; } = grid;
        public string Density { get; } = density;
        public int BuiltYear { get; } = builtYear;
        public int ActiveSchoolYear { get; } = activeSchoolYear;
        public int OwnershipStartYear { get; set; } = ownershipStartYear;
        public int CurrentYear { get; set; } = activeSchoolYear;
        public List<SimChild> Children { get; } = [];

        public bool IsActive(int year) => year >= ActiveSchoolYear;
    }

    private sealed class SimChild(int gradeIndex, bool isSpecialEducation)
    {
        public int GradeIndex { get; private set; } = gradeIndex;
        public bool IsSpecialEducation { get; } = isSpecialEducation;
        public bool IsStudent => GradeIndex is >= 0 and <= 13;
        public string Grade => IsSpecialEducation ? "Sp. Ed." : GradeForIndex(GradeIndex);

        public static SimChild Newborn(bool isSpecialEducation) => new(-4, isSpecialEducation);

        public void Advance()
        {
            if (GradeIndex == PostSchoolChildIndex) return;
            GradeIndex++;
        }
    }

    private sealed class YearAccumulator
    {
        public YearAccumulator(IEnumerable<string> grids, IEnumerable<string> grades)
        {
            var gradeList = grades.ToList();
            GridTotals = grids.ToDictionary(grid => grid, _ => 0.0, StringComparer.OrdinalIgnoreCase);
            GridGrades = grids.ToDictionary(
                grid => grid,
                _ => gradeList.ToDictionary(grade => grade, _ => 0.0, StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase);
            GradeTotals = gradeList.ToDictionary(grade => grade, _ => 0.0, StringComparer.OrdinalIgnoreCase);
            GridTotalSquares = grids.ToDictionary(grid => grid, _ => 0.0, StringComparer.OrdinalIgnoreCase);
            GridGradeSquares = grids.ToDictionary(
                grid => grid,
                _ => gradeList.ToDictionary(grade => grade, _ => 0.0, StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase);
            GradeTotalSquares = gradeList.ToDictionary(grade => grade, _ => 0.0, StringComparer.OrdinalIgnoreCase);
        }

        public Dictionary<string, double> GridTotals { get; }
        public Dictionary<string, Dictionary<string, double>> GridGrades { get; }
        public Dictionary<string, double> GradeTotals { get; }
        public Dictionary<string, double> GridTotalSquares { get; }
        public Dictionary<string, Dictionary<string, double>> GridGradeSquares { get; }
        public Dictionary<string, double> GradeTotalSquares { get; }
        public Dictionary<string, SegmentAccumulator> Segments { get; } = new(StringComparer.OrdinalIgnoreCase);
        public double GridTotalPerRunSum { get; private set; }
        public double GridTotalPerRunSquareSum { get; private set; }
        public double GradeTotalPerRunSum { get; private set; }
        public double GradeTotalPerRunSquareSum { get; private set; }
        public double HighSchoolTotalPerRunSum { get; private set; }
        public double HighSchoolTotalPerRunSquareSum { get; private set; }
        public double TotalHomes { get; private set; }
        public double LowDensityHomes { get; private set; }
        public double MediumDensityHomes { get; private set; }
        public double MediumHighDensityHomes { get; private set; }
        public double HighDensityHomes { get; private set; }
        public double OtherDensityHomes { get; private set; }
        public double K12Students { get; private set; }
        public double TkStudents { get; private set; }
        public double K8Students { get; private set; }
        public double HighSchoolStudents { get; private set; }
        public double LowMediumK12Students { get; private set; }
        public double LowMediumK8Students { get; private set; }
        public double LowMediumHighSchoolStudents { get; private set; }
        public double MediumHighHighK12Students { get; private set; }
        public double MediumHighHighK8Students { get; private set; }
        public double MediumHighHighHighSchoolStudents { get; private set; }
        public double[] ChildCountBuckets { get; } = new double[5];
        public double TurnoverEvents { get; private set; }
        public double TurnoverLongevityTotal { get; private set; }

        public void Add(IEnumerable<SimHome> homes, int year)
        {
            var runGridTotals = GridTotals.Keys.ToDictionary(grid => grid, _ => 0.0, StringComparer.OrdinalIgnoreCase);
            var runGridGrades = GridGrades.Keys.ToDictionary(
                grid => grid,
                _ => GradeTotals.Keys.ToDictionary(grade => grade, _ => 0.0, StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase);
            var runGradeTotals = GradeTotals.Keys.ToDictionary(grade => grade, _ => 0.0, StringComparer.OrdinalIgnoreCase);
            var runGridTotal = 0.0;
            var runGradeTotal = 0.0;
            var runHighSchoolTotal = 0.0;

            foreach (var home in homes)
            {
                if (!home.IsActive(year)) continue;

                AddHome(home);
                var segment = SegmentFor(home);
                segment.AddHome(home);
                foreach (var child in home.Children)
                {
                    if (!child.IsStudent) continue;
                    GridTotals[home.Grid] = GridTotals.GetValueOrDefault(home.Grid) + 1;
                    if (!GridGrades.TryGetValue(home.Grid, out var gridGrades))
                    {
                        gridGrades = GradeTotals.Keys.ToDictionary(grade => grade, _ => 0.0, StringComparer.OrdinalIgnoreCase);
                        GridGrades[home.Grid] = gridGrades;
                    }
                    gridGrades[child.Grade] = gridGrades.GetValueOrDefault(child.Grade) + 1;
                    GradeTotals[child.Grade] = GradeTotals.GetValueOrDefault(child.Grade) + 1;
                    runGridTotals[home.Grid] = runGridTotals.GetValueOrDefault(home.Grid) + 1;
                    if (!runGridGrades.TryGetValue(home.Grid, out var runGradesForGrid))
                    {
                        runGradesForGrid = GradeTotals.Keys.ToDictionary(grade => grade, _ => 0.0, StringComparer.OrdinalIgnoreCase);
                        runGridGrades[home.Grid] = runGradesForGrid;
                    }
                    runGradesForGrid[child.Grade] = runGradesForGrid.GetValueOrDefault(child.Grade) + 1;
                    runGradeTotals[child.Grade] = runGradeTotals.GetValueOrDefault(child.Grade) + 1;
                    if (!IsExcludedValidationGrid(home.Grid))
                    {
                        runGridTotal++;
                    }
                    if (GradeOrder.Contains(child.Grade))
                    {
                        runGradeTotal++;
                    }
                    if (HighSchoolGrades.Contains(child.Grade))
                    {
                        runHighSchoolTotal++;
                    }
                    segment.AddStudent(child);
                    AddStudent(home, child);
                }
            }

            foreach (var (grid, total) in runGridTotals)
            {
                GridTotalSquares[grid] = GridTotalSquares.GetValueOrDefault(grid) + total * total;
            }

            foreach (var (grid, grades) in runGridGrades)
            {
                if (!GridGradeSquares.TryGetValue(grid, out var targetSquares))
                {
                    targetSquares = GradeTotals.Keys.ToDictionary(grade => grade, _ => 0.0, StringComparer.OrdinalIgnoreCase);
                    GridGradeSquares[grid] = targetSquares;
                }

                foreach (var (grade, total) in grades)
                {
                    targetSquares[grade] = targetSquares.GetValueOrDefault(grade) + total * total;
                }
            }

            foreach (var (grade, total) in runGradeTotals)
            {
                GradeTotalSquares[grade] = GradeTotalSquares.GetValueOrDefault(grade) + total * total;
            }

            GridTotalPerRunSum += runGridTotal;
            GridTotalPerRunSquareSum += runGridTotal * runGridTotal;
            GradeTotalPerRunSum += runGradeTotal;
            GradeTotalPerRunSquareSum += runGradeTotal * runGradeTotal;
            HighSchoolTotalPerRunSum += runHighSchoolTotal;
            HighSchoolTotalPerRunSquareSum += runHighSchoolTotal * runHighSchoolTotal;
        }

        public void AddTurnover(SimHome home, int longevity)
        {
            AddTurnover(longevity);
            SegmentFor(home).AddTurnover(longevity);
        }

        public void AddTurnover(int longevity)
        {
            TurnoverEvents++;
            TurnoverLongevityTotal += Math.Max(0, longevity);
        }

        public void Merge(YearAccumulator other)
        {
            foreach (var (grid, total) in other.GridTotals)
            {
                GridTotals[grid] = GridTotals.GetValueOrDefault(grid) + total;
            }

            foreach (var (grid, total) in other.GridTotalSquares)
            {
                GridTotalSquares[grid] = GridTotalSquares.GetValueOrDefault(grid) + total;
            }

            foreach (var (grid, grades) in other.GridGrades)
            {
                if (!GridGrades.TryGetValue(grid, out var targetGrades))
                {
                    targetGrades = GradeTotals.Keys.ToDictionary(grade => grade, _ => 0.0, StringComparer.OrdinalIgnoreCase);
                    GridGrades[grid] = targetGrades;
                }

                foreach (var (grade, total) in grades)
                {
                    targetGrades[grade] = targetGrades.GetValueOrDefault(grade) + total;
                }
            }

            foreach (var (grid, grades) in other.GridGradeSquares)
            {
                if (!GridGradeSquares.TryGetValue(grid, out var targetGrades))
                {
                    targetGrades = GradeTotals.Keys.ToDictionary(grade => grade, _ => 0.0, StringComparer.OrdinalIgnoreCase);
                    GridGradeSquares[grid] = targetGrades;
                }

                foreach (var (grade, total) in grades)
                {
                    targetGrades[grade] = targetGrades.GetValueOrDefault(grade) + total;
                }
            }

            foreach (var (key, segment) in other.Segments)
            {
                if (!Segments.TryGetValue(key, out var targetSegment))
                {
                    targetSegment = new SegmentAccumulator(segment.Grid, segment.Density);
                    Segments[key] = targetSegment;
                }

                targetSegment.Merge(segment);
            }

            foreach (var (grade, total) in other.GradeTotals)
            {
                GradeTotals[grade] = GradeTotals.GetValueOrDefault(grade) + total;
            }

            foreach (var (grade, total) in other.GradeTotalSquares)
            {
                GradeTotalSquares[grade] = GradeTotalSquares.GetValueOrDefault(grade) + total;
            }

            GridTotalPerRunSum += other.GridTotalPerRunSum;
            GridTotalPerRunSquareSum += other.GridTotalPerRunSquareSum;
            GradeTotalPerRunSum += other.GradeTotalPerRunSum;
            GradeTotalPerRunSquareSum += other.GradeTotalPerRunSquareSum;
            HighSchoolTotalPerRunSum += other.HighSchoolTotalPerRunSum;
            HighSchoolTotalPerRunSquareSum += other.HighSchoolTotalPerRunSquareSum;
            TotalHomes += other.TotalHomes;
            LowDensityHomes += other.LowDensityHomes;
            MediumDensityHomes += other.MediumDensityHomes;
            MediumHighDensityHomes += other.MediumHighDensityHomes;
            HighDensityHomes += other.HighDensityHomes;
            OtherDensityHomes += other.OtherDensityHomes;
            K12Students += other.K12Students;
            TkStudents += other.TkStudents;
            K8Students += other.K8Students;
            HighSchoolStudents += other.HighSchoolStudents;
            LowMediumK12Students += other.LowMediumK12Students;
            LowMediumK8Students += other.LowMediumK8Students;
            LowMediumHighSchoolStudents += other.LowMediumHighSchoolStudents;
            MediumHighHighK12Students += other.MediumHighHighK12Students;
            MediumHighHighK8Students += other.MediumHighHighK8Students;
            MediumHighHighHighSchoolStudents += other.MediumHighHighHighSchoolStudents;
            TurnoverEvents += other.TurnoverEvents;
            TurnoverLongevityTotal += other.TurnoverLongevityTotal;
            for (var i = 0; i < ChildCountBuckets.Length; i++)
            {
                ChildCountBuckets[i] += other.ChildCountBuckets[i];
            }
        }

        private SegmentAccumulator SegmentFor(SimHome home)
        {
            var density = DensityLabel(home.Density);
            var key = $"{home.Grid}|{density}";
            if (!Segments.TryGetValue(key, out var segment))
            {
                segment = new SegmentAccumulator(home.Grid, density);
                Segments[key] = segment;
            }

            return segment;
        }

        private void AddHome(SimHome home)
        {
            TotalHomes++;
            ChildCountBuckets[Math.Min(Math.Max(0, home.Children.Count), 4)]++;

            switch (NormalizeDensity(home.Density))
            {
                case "RL":
                    LowDensityHomes++;
                    break;
                case "RM":
                    MediumDensityHomes++;
                    break;
                case "RMH":
                    MediumHighDensityHomes++;
                    break;
                case "RH":
                    HighDensityHomes++;
                    break;
                default:
                    OtherDensityHomes++;
                    break;
            }
        }

        private void AddStudent(SimHome home, SimChild child)
        {
            var isTk = child.GradeIndex == 0;
            var isK12 = child.GradeIndex is >= 1 and <= 13;
            var isK8 = child.GradeIndex is >= 1 and <= 9;
            var isHighSchool = child.GradeIndex is >= 10 and <= 13;
            var density = NormalizeDensity(home.Density);
            var isMediumHighHigh = density is "RMH" or "RH";

            if (isTk) TkStudents++;
            if (isK12) K12Students++;
            if (isK8) K8Students++;
            if (isHighSchool) HighSchoolStudents++;

            if (isMediumHighHigh)
            {
                if (isK12) MediumHighHighK12Students++;
                if (isK8) MediumHighHighK8Students++;
                if (isHighSchool) MediumHighHighHighSchoolStudents++;
            }
            else
            {
                if (isK12) LowMediumK12Students++;
                if (isK8) LowMediumK8Students++;
                if (isHighSchool) LowMediumHighSchoolStudents++;
            }
        }
    }

    private sealed class SegmentAccumulator(string grid, string density)
    {
        public string Grid { get; } = grid;
        public string Density { get; } = density;
        public double TotalHomes { get; private set; }
        public Dictionary<string, double> Grades { get; } = GradeOrder.ToDictionary(grade => grade, _ => 0.0, StringComparer.OrdinalIgnoreCase);
        public double[] ChildCountBuckets { get; } = new double[5];
        public double TurnoverEvents { get; private set; }
        public double TurnoverLongevityTotal { get; private set; }

        public void AddHome(SimHome home)
        {
            TotalHomes++;
            ChildCountBuckets[Math.Min(Math.Max(0, home.Children.Count), 4)]++;
        }

        public void AddStudent(SimChild child)
        {
            Grades[child.Grade] = Grades.GetValueOrDefault(child.Grade) + 1;
        }

        public void AddTurnover(int longevity)
        {
            TurnoverEvents++;
            TurnoverLongevityTotal += Math.Max(0, longevity);
        }

        public void Merge(SegmentAccumulator other)
        {
            TotalHomes += other.TotalHomes;
            foreach (var (grade, total) in other.Grades)
            {
                Grades[grade] = Grades.GetValueOrDefault(grade) + total;
            }

            for (var i = 0; i < ChildCountBuckets.Length; i++)
            {
                ChildCountBuckets[i] += other.ChildCountBuckets[i];
            }

            TurnoverEvents += other.TurnoverEvents;
            TurnoverLongevityTotal += other.TurnoverLongevityTotal;
        }

        public MonteCarloSimulationSegmentInfo ToInfo(double runs)
        {
            var denominator = Math.Max(1, runs);
            return new MonteCarloSimulationSegmentInfo(
                Grid,
                Density,
                TotalHomes / denominator,
                Grades.ToDictionary(kvp => kvp.Key, kvp => kvp.Value / denominator, StringComparer.OrdinalIgnoreCase),
                ChildCountBuckets[0] / denominator,
                ChildCountBuckets[1] / denominator,
                ChildCountBuckets[2] / denominator,
                ChildCountBuckets[3] / denominator,
                ChildCountBuckets[4] / denominator,
                TurnoverEvents / denominator,
                Ratio(TurnoverLongevityTotal, TurnoverEvents));
        }
    }

    private sealed class LifecycleAccumulator
    {
        private readonly double[] childCountBuckets = new double[5];

        public double Students { get; private set; }
        public double Tk8Students { get; private set; }
        public double HighSchoolStudents { get; private set; }
        public double SpecialEducationStudents { get; private set; }
        public double Children { get; private set; }

        public void Add(IEnumerable<SimHome> homes, int year)
        {
            foreach (var home in homes)
            {
                if (!home.IsActive(year)) continue;

                var childCount = home.Children.Count;
                Children += childCount;
                childCountBuckets[Math.Min(childCount, 4)]++;

                foreach (var child in home.Children)
                {
                    if (!child.IsStudent) continue;

                    Students++;
                    if (child.GradeIndex <= 9) Tk8Students++;
                    if (child.GradeIndex is >= 10 and <= 13) HighSchoolStudents++;
                    if (child.IsSpecialEducation) SpecialEducationStudents++;
                }
            }
        }

        public MonteCarloLifecycleYear ToResult(int yearsAfterMoveIn, double denominator)
        {
            return new MonteCarloLifecycleYear(
                yearsAfterMoveIn,
                Students / denominator,
                Tk8Students / denominator,
                HighSchoolStudents / denominator,
                SpecialEducationStudents / denominator,
                Children / denominator,
                childCountBuckets[0] / denominator,
                childCountBuckets[1] / denominator,
                childCountBuckets[2] / denominator,
                childCountBuckets[3] / denominator,
                childCountBuckets[4] / denominator);
        }
    }

    private sealed class ChildCountDistributionAccumulator
    {
        private readonly double[] buckets = new double[5];

        public double HouseholdCount { get; private set; }

        public void Add(int childCount)
        {
            HouseholdCount++;
            buckets[Math.Min(childCount, 4)]++;
        }

        public MonteCarloChildCountDistribution ToResult()
        {
            if (HouseholdCount <= 0)
            {
                return new MonteCarloChildCountDistribution(0, 0, 0, 0, 0, 0);
            }

            return new MonteCarloChildCountDistribution(
                HouseholdCount,
                buckets[0] / HouseholdCount,
                buckets[1] / HouseholdCount,
                buckets[2] / HouseholdCount,
                buckets[3] / HouseholdCount,
                buckets[4] / HouseholdCount);
        }
    }

    private sealed class FamilyLongevityAccumulator
    {
        private readonly Dictionary<int, double> buckets = [];

        public void Add(int yearsInHome)
        {
            var bucket = Math.Max(0, yearsInHome);
            buckets[bucket] = buckets.GetValueOrDefault(bucket) + 1;
        }

        public IReadOnlyList<MonteCarloFamilyLongevityYear> ToResult()
        {
            var total = buckets.Values.Sum();
            if (total <= 0)
            {
                return [];
            }

            return buckets
                .OrderBy(kvp => kvp.Key)
                .Select(kvp => new MonteCarloFamilyLongevityYear(kvp.Key, kvp.Value, kvp.Value / total))
                .ToList();
        }
    }

    private sealed class TurnoverAccumulator
    {
        public double ActiveHomes { get; private set; }
        public double TurnoverEvents { get; private set; }

        public void AddActiveHomes(double activeHomes)
        {
            ActiveHomes += activeHomes;
        }

        public void AddTurnover()
        {
            TurnoverEvents++;
        }

        public MonteCarloTurnoverYear ToResult(int year)
        {
            return new MonteCarloTurnoverYear(
                year,
                ActiveHomes,
                TurnoverEvents,
                ActiveHomes > 0 ? TurnoverEvents / ActiveHomes : 0);
        }
    }

    private static double ToRealizedTurnoverRate(IEnumerable<TurnoverAccumulator> turnover)
    {
        var items = turnover.ToList();
        var activeHomes = items.Sum(item => item.ActiveHomes);
        return activeHomes > 0 ? items.Sum(item => item.TurnoverEvents) / activeHomes : 0;
    }

    private static MonteCarloChildCountDistribution ToChildCountDistribution(MonteCarloLifecycleYear year, double householdCount)
    {
        return new MonteCarloChildCountDistribution(
            householdCount,
            year.HomesWithoutChildrenShare,
            year.HomesWithOneChildShare,
            year.HomesWithTwoChildrenShare,
            year.HomesWithThreeChildrenShare,
            year.HomesWithFourPlusChildrenShare);
    }
}
