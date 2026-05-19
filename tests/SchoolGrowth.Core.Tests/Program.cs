using SchoolGrowth.Core;

var dataRoot = ResolveDataRoot(AppContext.BaseDirectory);
var data = EnrollmentData.Load(dataRoot);
var model = EnrollmentModel.Calibrate(data);
var result = model.Project(new SimulationRequest(
    StartYear: 2026,
    Years: 3,
    ScenarioHomes:
    [
        new ScenarioHome("Pombo", "RL", 2026, 100),
        new ScenarioHome("Wicklund", "RH", 2026, 50)
    ]));

foreach (var year in result.Projection)
{
    AssertClose(year.TotalStudents, year.GridTotals.Values.Sum(), $"grid totals reconcile in {year.Year}");

    var mhhs912 = year.SchoolGrades["MHHS"]
        .Where(kvp => kvp.Key is "9th" or "10th" or "11th" or "12th")
        .Sum(kvp => kvp.Value);
    AssertClose(year.SchoolTotals["MHHS"], mhhs912, $"MHHS is 9-12 only in {year.Year}");

    var mhesdTk8 = year.GridGrades["MHESD"]
        .Where(kvp => kvp.Key is "TK" or "K" or "1st" or "2nd" or "3rd" or "4th" or "5th" or "6th" or "7th" or "8th")
        .Sum(kvp => kvp.Value);
    AssertClose(0, mhesdTk8, $"MHESD has no TK-8 in {year.Year}");

    AssertPositiveTk8AndHighSchool(year, "Lammersville");
    AssertPositiveTk8AndHighSchool(year, "Inter-Districts");
    AssertClose(year.TotalStudents, year.SchoolTotals.Values.Sum(), $"school totals reconcile in {year.Year}");
}

var monteCarlo = new MonteCarloEnrollmentModel(data);
var densityProfiles = MonteCarloEnrollmentModel.BuildDensityDirectParameters(new MonteCarloParameters()).ToDictionary(profile => profile.DensityCode);
AssertClose(1, densityProfiles["RL"].MoveInChildShares.Zero
    + densityProfiles["RL"].MoveInChildShares.One
    + densityProfiles["RL"].MoveInChildShares.Two
    + densityProfiles["RL"].MoveInChildShares.Three
    + densityProfiles["RL"].MoveInChildShares.Four,
    "direct low-density move-in shares normalize");
if (densityProfiles["RH"].MoveInChildShares.Four != 0)
{
    throw new InvalidOperationException("Default high-density direct profile should keep 4-child move-ins at zero.");
}

var monteCarloResult = monteCarlo.Validate(new MonteCarloValidationRequest(
    StartYear: 2020,
    EndYear: 2022,
    Parameters: new MonteCarloParameters(Runs: 25, Seed: 1234)));

if (monteCarloResult.Comparisons.Count != 3)
{
    throw new InvalidOperationException($"Monte Carlo validation should return 3 comparison years, got {monteCarloResult.Comparisons.Count}.");
}

foreach (var comparison in monteCarloResult.Comparisons)
{
    if (comparison.ModeledGridTotal <= 0 || comparison.ModeledGradeTotal <= 0)
    {
        throw new InvalidOperationException($"Monte Carlo validation returned an empty modeled total for {comparison.Year}.");
    }

    AssertClose(comparison.ModeledGridTotal, comparison.ModeledGradeTotal, $"Monte Carlo grid and grade totals reconcile in {comparison.Year}");
}

var futureGapParameters = new MonteCarloParameters(Runs: 20, Seed: 2468, MaxDegreeOfParallelism: 1);
var fullHistoryBackmodel = monteCarlo.Validate(new MonteCarloValidationRequest(
    StartYear: 2003,
    EndYear: 2015,
    Parameters: futureGapParameters));
var directPreReferenceStart = monteCarlo.Validate(new MonteCarloValidationRequest(
    StartYear: 2015,
    EndYear: 2015,
    Parameters: futureGapParameters));
AssertRelativeClose(
    fullHistoryBackmodel.Comparisons.Single(comparison => comparison.Year == 2015).ModeledGridTotal,
    directPreReferenceStart.Comparisons.Single().ModeledGridTotal,
    0.02,
    "Monte Carlo pre-reference start preserves homes-only backmodel population");

var futureFromBaseline = monteCarlo.Validate(new MonteCarloValidationRequest(
    StartYear: 2026,
    EndYear: 2027,
    Parameters: futureGapParameters));
var futureFromGapStart = monteCarlo.Validate(new MonteCarloValidationRequest(
    StartYear: 2027,
    EndYear: 2027,
    Parameters: futureGapParameters));
AssertClose(
    futureFromBaseline.Comparisons.Single(comparison => comparison.Year == 2027).ModeledGridTotal,
    futureFromGapStart.Comparisons.Single().ModeledGridTotal,
    "Monte Carlo warm-up includes homes and aging between baseline and future start year");

var search = monteCarlo.FindBest(new MonteCarloSearchRequest(
    StartYear: 2020,
    EndYear: 2021,
    RunsPerCandidate: 10,
    Seed: 4321,
    OwnershipChangeProbabilities: [0.02, 0.04],
    MoveInZeroChildShares: [0.05],
    MoveInOneChildShares: [0.30],
    MoveInTwoChildShares: [0.60],
    MoveInThreeChildShares: [0.05],
    MoveInFourChildShares: [0.0],
    StudentExitProbabilities: [0.01, 0.02]));

if (search.Candidates.Count != 4 || search.Best.Comparisons.Count != 2)
{
    throw new InvalidOperationException("Monte Carlo parameter search did not return the expected candidate results.");
}

Console.WriteLine("All core projection and Monte Carlo checks passed.");

static void AssertPositiveTk8AndHighSchool(ProjectionYear year, string grid)
{
    var tk8 = year.GridGrades[grid]
        .Where(kvp => kvp.Key is "TK" or "K" or "1st" or "2nd" or "3rd" or "4th" or "5th" or "6th" or "7th" or "8th")
        .Sum(kvp => kvp.Value);
    var highSchool = year.GridGrades[grid]
        .Where(kvp => kvp.Key is "9th" or "10th" or "11th" or "12th")
        .Sum(kvp => kvp.Value);

    if (tk8 <= 0 || highSchool <= 0)
    {
        throw new InvalidOperationException($"{grid} should have TK-8 and 9-12 students in {year.Year}.");
    }
}

static void AssertClose(double expected, double actual, string label)
{
    if (Math.Abs(expected - actual) > 0.01)
    {
        throw new InvalidOperationException($"{label}: expected {expected:N2}, got {actual:N2}.");
    }
}

static void AssertRelativeClose(double expected, double actual, double tolerance, string label)
{
    var denominator = Math.Max(1, Math.Abs(expected));
    if (Math.Abs(expected - actual) / denominator > tolerance)
    {
        throw new InvalidOperationException($"{label}: expected {expected:N2}, got {actual:N2}.");
    }
}

static string ResolveDataRoot(string start)
{
    var directory = new DirectoryInfo(start);
    while (directory is not null)
    {
        var candidate = Path.Combine(directory.FullName, "data");
        if (File.Exists(Path.Combine(candidate, "homes.csv"))) return candidate;
        directory = directory.Parent;
    }

    throw new DirectoryNotFoundException("Could not find data directory.");
}
