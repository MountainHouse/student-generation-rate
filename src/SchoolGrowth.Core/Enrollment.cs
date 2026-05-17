using System.Globalization;
using System.Text;

namespace SchoolGrowth.Core;

public sealed record HomeRow(string Density, string Neighborhood, Dictionary<int, double> HomesByYear);
public sealed record SeriesRow(string Name, Dictionary<string, double?> Values);
public sealed record ScenarioHome(string Neighborhood, string Density, int Year, double Homes);

public sealed record SimulationRequest(
    int StartYear,
    int Years,
    List<ScenarioHome> ScenarioHomes,
    double RetentionBlend = 0.75,
    double HomeYieldMultiplier = 1.0,
    Dictionary<string, double>? DensityMultipliers = null,
    Dictionary<string, double>? GradeShares = null);

public sealed record BacktestRequest(
    int StartYear,
    int EndYear,
    double RetentionBlend = 0.75,
    double HomeYieldMultiplier = 1.0,
    Dictionary<string, double>? DensityMultipliers = null,
    Dictionary<string, double>? GradeShares = null);

public sealed record ProjectionYear(
    int Year,
    Dictionary<string, double> Grades,
    Dictionary<string, double> NewStudentsByGrade,
    Dictionary<string, double> NeighborhoodStudents,
    Dictionary<string, double> GridTotals,
    Dictionary<string, Dictionary<string, double>> GridGrades,
    Dictionary<string, double> SchoolTotals,
    Dictionary<string, Dictionary<string, double>> SchoolGrades,
    double TotalStudents,
    double NewStudentsTotal);

public sealed record CalibrationResult(
    Dictionary<string, double> DensityYieldPerHome,
    Dictionary<string, double> GradeShares,
    Dictionary<string, double> RetentionRates,
    Dictionary<string, double> GridTrendRates,
    Dictionary<string, double> SchoolProxyRatios,
    double BaselineTkStudents,
    double Intercept,
    double MeanAbsoluteError,
    string Method);

public sealed record SimulationResult(
    CalibrationResult Calibration,
    IReadOnlyList<ProjectionYear> Projection,
    IReadOnlyList<ScenarioHome> ScenarioHomes);

public sealed record BacktestYearComparison(
    int Year,
    Dictionary<string, double?> ActualGridTotals,
    Dictionary<string, double> ModeledGridTotals,
    double ActualTotal,
    double ModeledTotal,
    double Error,
    double AbsolutePercentageError);

public sealed record BacktestResult(
    SimulationResult Simulation,
    IReadOnlyList<BacktestYearComparison> Comparisons,
    double MeanAbsoluteError,
    double MeanAbsolutePercentageError);

public sealed class EnrollmentData
{
    public required IReadOnlyList<HomeRow> HomeRows { get; init; }
    public required IReadOnlyList<SeriesRow> GridRows { get; init; }
    public required IReadOnlyList<SeriesRow> GradeRows { get; init; }
    public required IReadOnlyList<SeriesRow> SchoolRows { get; init; }
    public required IReadOnlyList<string> Terms { get; init; }
    public required IReadOnlyList<int> AugustYears { get; init; }
    public required IReadOnlyList<string> Neighborhoods { get; init; }
    public required IReadOnlyList<string> Densities { get; init; }

    public static EnrollmentData Load(string dataRoot)
    {
        var homes = ReadHomes(Path.Combine(dataRoot, "homes.csv"));
        var grid = ReadSeries(Path.Combine(dataRoot, "grid.csv"));
        var grades = ReadSeries(Path.Combine(dataRoot, "grade.csv"));
        var schools = ReadSeries(Path.Combine(dataRoot, "schools.csv"));
        return Create(homes, grid, grades, schools);
    }

    public static EnrollmentData LoadFromCsvText(string homesCsv, string gridCsv, string gradeCsv, string schoolsCsv)
    {
        var homes = ReadHomes(ReadCsvText(homesCsv));
        var grid = ReadSeries(ReadCsvText(gridCsv));
        var grades = ReadSeries(ReadCsvText(gradeCsv));
        var schools = ReadSeries(ReadCsvText(schoolsCsv));
        return Create(homes, grid, grades, schools);
    }

    private static EnrollmentData Create(
        IReadOnlyList<HomeRow> homes,
        IReadOnlyList<SeriesRow> grid,
        IReadOnlyList<SeriesRow> grades,
        IReadOnlyList<SeriesRow> schools)
    {
        var terms = grades.FirstOrDefault()?.Values.Keys.ToList() ?? [];
        var augustYears = terms
            .Select(TryParseAugustYear)
            .Where(year => year.HasValue)
            .Select(year => year!.Value)
            .ToList();

        return new EnrollmentData
        {
            HomeRows = homes,
            GridRows = grid,
            GradeRows = grades,
            SchoolRows = schools,
            Terms = terms,
            AugustYears = augustYears,
            Neighborhoods = homes.Select(h => h.Neighborhood)
                .Concat(grid.Select(g => g.Name))
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n)
                .ToList(),
            Densities = homes.Select(h => h.Density)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(d => d)
                .ToList()
        };
    }

    private static List<HomeRow> ReadHomes(string path)
    {
        return ReadHomes(ReadCsv(path));
    }

    private static List<HomeRow> ReadHomes(List<List<string>> rows)
    {
        if (rows.Count == 0) return [];
        var header = rows[0];
        var years = header.Skip(2).Select(h => int.Parse(h, CultureInfo.InvariantCulture)).ToList();
        var result = new List<HomeRow>();

        foreach (var row in rows.Skip(1))
        {
            if (row.Count < 2 || string.IsNullOrWhiteSpace(row[0]) || string.IsNullOrWhiteSpace(row[1])) continue;
            var byYear = new Dictionary<int, double>();
            for (var i = 0; i < years.Count; i++)
            {
                var index = i + 2;
                byYear[years[i]] = index < row.Count ? ParseDouble(row[index]) ?? 0 : 0;
            }

            result.Add(new HomeRow(row[0].Trim(), row[1].Trim(), byYear));
        }

        return result;
    }

    private static List<SeriesRow> ReadSeries(string path)
    {
        return ReadSeries(ReadCsv(path));
    }

    private static List<SeriesRow> ReadSeries(List<List<string>> rows)
    {
        if (rows.Count == 0) return [];
        var header = rows[0];
        var result = new List<SeriesRow>();

        foreach (var row in rows.Skip(1))
        {
            if (row.Count == 0 || string.IsNullOrWhiteSpace(row[0])) continue;
            var values = new Dictionary<string, double?>();
            for (var i = 1; i < header.Count; i++)
            {
                values[header[i].Trim()] = i < row.Count ? ParseDouble(row[i]) : null;
            }

            result.Add(new SeriesRow(row[0].Trim(), values));
        }

        return result;
    }

    private static int? TryParseAugustYear(string term)
    {
        if (!term.StartsWith("Aug ", StringComparison.OrdinalIgnoreCase)) return null;
        var yearText = term.Split(' ', StringSplitOptions.RemoveEmptyEntries).Last().Trim('\'', '*');
        return int.TryParse(yearText, out var yy) ? 2000 + yy : null;
    }

    private static double? ParseDouble(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return double.TryParse(value.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var number) ? number : null;
    }

    private static List<List<string>> ReadCsv(string path)
    {
        return File.ReadLines(path)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(ParseCsvLine)
            .ToList();
    }

    private static List<List<string>> ReadCsvText(string text)
    {
        return text.Split(["\r\n", "\n"], StringSplitOptions.None)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(ParseCsvLine)
            .ToList();
    }

    private static List<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        values.Add(current.ToString());
        return values;
    }
}

public sealed class EnrollmentModel
{
    private static readonly string[] GradeOrder =
    [
        "TK", "K", "1st", "2nd", "3rd", "4th", "5th", "6th", "7th", "8th",
        "9th", "10th", "11th", "12th", "Sp. Ed."
    ];
    private static readonly string[] Tk8Grades = ["TK", "K", "1st", "2nd", "3rd", "4th", "5th", "6th", "7th", "8th"];
    private static readonly string[] HighSchoolGrades = ["9th", "10th", "11th", "12th"];
    private static readonly HashSet<string> MatchingElementarySchools = new(StringComparer.OrdinalIgnoreCase)
    {
        "Altamont", "Bethany", "Cordes", "Costa", "Hansen", "Lammersville", "Questa", "Wicklund", "Pombo"
    };

    private readonly EnrollmentData data;
    private readonly Dictionary<string, double> densityYields;
    private readonly Dictionary<string, double> gradeShares;
    private readonly Dictionary<string, double> retentionRates;
    private readonly Dictionary<string, double> gridTrendRates;
    private readonly Dictionary<string, double> schoolProxyRatios;
    private readonly double baselineTkStudents;
    private readonly double intercept;
    private readonly double mae;

    public CalibrationResult Calibration => new(
        densityYields,
        gradeShares,
        retentionRates,
        gridTrendRates,
        schoolProxyRatios,
        baselineTkStudents,
        intercept,
        mae,
        "Grid-first projection using ridge-calibrated density yield, per-grid trends, school proxy ratios, and district grade shares.");

    private EnrollmentModel(
        EnrollmentData data,
        Dictionary<string, double> densityYields,
        Dictionary<string, double> gradeShares,
        Dictionary<string, double> retentionRates,
        Dictionary<string, double> gridTrendRates,
        Dictionary<string, double> schoolProxyRatios,
        double baselineTkStudents,
        double intercept,
        double mae)
    {
        this.data = data;
        this.densityYields = densityYields;
        this.gradeShares = gradeShares;
        this.retentionRates = retentionRates;
        this.gridTrendRates = gridTrendRates;
        this.schoolProxyRatios = schoolProxyRatios;
        this.baselineTkStudents = baselineTkStudents;
        this.intercept = intercept;
        this.mae = mae;
    }

    public static EnrollmentModel Calibrate(EnrollmentData data)
    {
        var densities = data.Densities.ToList();
        var observations = BuildCalibrationObservations(data, densities);
        var parameters = SolveRidge(observations.Features, observations.Targets, 10.0);
        var densityYields = densities.Select((density, index) => new
            {
                density,
                value = Math.Clamp(parameters[index], 0.02, 2.0)
            })
            .ToDictionary(x => x.density, x => x.value, StringComparer.OrdinalIgnoreCase);
        var intercept = Math.Max(0, parameters.LastOrDefault());
        var mae = observations.Targets.Count == 0
            ? 0
            : observations.Targets.Select((target, i) =>
            {
                var prediction = intercept;
                for (var j = 0; j < densities.Count; j++) prediction += observations.Features[i][j] * densityYields[densities[j]];
                return Math.Abs(target - prediction);
            }).Average();

        return new EnrollmentModel(
            data,
            densityYields,
            CalculateLatestGradeShares(data),
            CalculateRetentionRates(data),
            CalculateGridTrendRates(data),
            CalculateSchoolProxyRatios(data),
            CalculateRecentAverage(data, "TK", 3),
            intercept,
            mae);
    }

    public SimulationResult Project(SimulationRequest request)
    {
        var startYear = request.StartYear <= 0 ? data.AugustYears.LastOrDefault(2025) + 1 : request.StartYear;
        var years = Math.Clamp(request.Years <= 0 ? 10 : request.Years, 1, 30);
        var scenarioHomes = request.ScenarioHomes ?? [];
        return ProjectInternal(request with { StartYear = startYear, Years = years, ScenarioHomes = scenarioHomes }, GetLatestGridTotals(), scenarioHomes);
    }

    public BacktestResult Backtest(BacktestRequest request)
    {
        var minStartYear = data.AugustYears.Skip(1).DefaultIfEmpty(data.AugustYears.FirstOrDefault(2017)).First();
        var maxEndYear = data.AugustYears.Last();
        var startYear = Math.Clamp(request.StartYear <= 0 ? minStartYear : request.StartYear, minStartYear, maxEndYear);
        var endYear = Math.Clamp(request.EndYear <= 0 ? maxEndYear : request.EndYear, startYear, maxEndYear);
        var years = endYear - startYear + 1;
        var actualHomes = BuildActualHomeScenario(startYear, endYear);
        var simulationRequest = new SimulationRequest(
            startYear,
            years,
            actualHomes,
            request.RetentionBlend,
            request.HomeYieldMultiplier,
            request.DensityMultipliers,
            request.GradeShares);
        var simulation = ProjectInternal(simulationRequest, GetGridTotalsForAugustYear(startYear - 1), actualHomes);
        var comparisons = simulation.Projection.Select(year =>
        {
            var actual = GetNullableGridTotalsForAugustYear(year.Year);
            var actualTotal = actual.Values.Where(value => value.HasValue).Sum(value => value!.Value);
            var modeledTotal = year.GridTotals
                .Where(kvp => actual.GetValueOrDefault(kvp.Key).HasValue)
                .Sum(kvp => kvp.Value);
            var error = modeledTotal - actualTotal;
            var ape = actualTotal == 0 ? 0 : Math.Abs(error) / actualTotal;
            return new BacktestYearComparison(
                year.Year,
                actual,
                year.GridTotals,
                actualTotal,
                modeledTotal,
                error,
                ape);
        }).ToList();

        return new BacktestResult(
            simulation,
            comparisons,
            comparisons.Count == 0 ? 0 : comparisons.Average(item => Math.Abs(item.Error)),
            comparisons.Count == 0 ? 0 : comparisons.Average(item => item.AbsolutePercentageError));
    }

    private SimulationResult ProjectInternal(
        SimulationRequest request,
        Dictionary<string, double> initialGridTotals,
        IReadOnlyList<ScenarioHome> scenarioHomes)
    {
        var activeDensityYields = ApplyDensityMultipliers(request.DensityMultipliers);
        var activeGradeShares = NormalizeShares(request.GradeShares, gradeShares);
        var previousGridTotals = initialGridTotals;
        var projection = new List<ProjectionYear>();

        for (var year = request.StartYear; year < request.StartYear + request.Years; year++)
        {
            var gridNewStudents = data.GridRows.ToDictionary(
                grid => grid.Name,
                grid => CalculateGridNewStudentYield(grid.Name, year, scenarioHomes, activeDensityYields) * request.HomeYieldMultiplier,
                StringComparer.OrdinalIgnoreCase);
            var gridTotals = data.GridRows.ToDictionary(
                grid => grid.Name,
                grid =>
                {
                    var trend = WeightedGridTrend(grid.Name, request.RetentionBlend);
                    var existingStudents = previousGridTotals.GetValueOrDefault(grid.Name) * trend;
                    return Math.Max(0, existingStudents + gridNewStudents.GetValueOrDefault(grid.Name));
                },
                StringComparer.OrdinalIgnoreCase);
            var gridGrades = gridTotals.ToDictionary(
                kvp => kvp.Key,
                kvp => AllocateGridGrades(kvp.Key, kvp.Value, activeGradeShares),
                StringComparer.OrdinalIgnoreCase);
            var newByGrade = gridNewStudents.ToDictionary(
                kvp => kvp.Key,
                kvp => AllocateGridGrades(kvp.Key, kvp.Value, activeGradeShares),
                StringComparer.OrdinalIgnoreCase)
                .Values
                .Aggregate(EmptyGradeDictionary(), SumGrades);
            var grades = gridGrades.Values.Aggregate(EmptyGradeDictionary(), SumGrades);
            var schoolGrades = RollUpSchoolGrades(gridGrades);
            var schoolTotals = schoolGrades.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Values.Sum(),
                StringComparer.OrdinalIgnoreCase);

            projection.Add(new ProjectionYear(
                year,
                grades,
                newByGrade,
                gridNewStudents,
                gridTotals,
                gridGrades,
                schoolTotals,
                schoolGrades,
                grades.Values.Sum(),
                newByGrade.Values.Sum()));

            previousGridTotals = gridTotals;
        }

        return new SimulationResult(Calibration, projection, scenarioHomes);
    }

    private Dictionary<string, double> ApplyDensityMultipliers(Dictionary<string, double>? multipliers)
    {
        return densityYields.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value * Math.Clamp(multipliers?.GetValueOrDefault(kvp.Key) ?? 1.0, 0.1, 5.0),
            StringComparer.OrdinalIgnoreCase);
    }

    private Dictionary<string, double> AdvanceCohorts(Dictionary<string, double> previousGrades, double blend)
    {
        blend = Math.Clamp(blend, 0, 1);
        var retained = GradeOrder.ToDictionary(grade => grade, _ => 0.0, StringComparer.OrdinalIgnoreCase);
        retained["TK"] = baselineTkStudents;
        retained["K"] = previousGrades.GetValueOrDefault("TK") * WeightedRetention("K", blend);
        retained["1st"] = previousGrades.GetValueOrDefault("K") * WeightedRetention("1st", blend);

        for (var i = 3; i <= 13; i++)
        {
            retained[GradeOrder[i]] = previousGrades.GetValueOrDefault(GradeOrder[i - 1]) * WeightedRetention(GradeOrder[i], blend);
        }

        retained["Sp. Ed."] = previousGrades.GetValueOrDefault("Sp. Ed.") * WeightedRetention("Sp. Ed.", blend);
        return retained;
    }

    private double WeightedRetention(string grade, double blend)
    {
        var observed = retentionRates.GetValueOrDefault(grade, 1.0);
        return observed * blend + (1.0 - blend);
    }

    private double WeightedGridTrend(string grid, double blend)
    {
        var observed = gridTrendRates.GetValueOrDefault(grid, 1.0);
        blend = Math.Clamp(blend, 0, 1);
        return observed * blend + (1.0 - blend);
    }

    private Dictionary<string, double> GetLatestGridTotals()
    {
        var latestYear = data.AugustYears.Last();
        return GetGridTotalsForAugustYear(latestYear);
    }

    private Dictionary<string, double> GetGridTotalsForAugustYear(int year)
    {
        var term = $"Aug '{year % 100:00}";
        return data.GridRows.ToDictionary(
            grid => grid.Name,
            grid => grid.Values.GetValueOrDefault(term) ?? LatestNonNullValue(grid),
            StringComparer.OrdinalIgnoreCase);
    }

    private Dictionary<string, double?> GetNullableGridTotalsForAugustYear(int year)
    {
        var term = $"Aug '{year % 100:00}";
        return data.GridRows.ToDictionary(
            grid => grid.Name,
            grid => grid.Values.GetValueOrDefault(term),
            StringComparer.OrdinalIgnoreCase);
    }

    private List<ScenarioHome> BuildActualHomeScenario(int startYear, int endYear)
    {
        return data.HomeRows
            .SelectMany(row => row.HomesByYear
                .Where(kvp => kvp.Key >= startYear && kvp.Key <= endYear && kvp.Value > 0)
                .Select(kvp => new ScenarioHome(row.Neighborhood, row.Density, kvp.Key, kvp.Value)))
            .ToList();
    }

    private Dictionary<string, double> AllocateGridGrades(string grid, double total, Dictionary<string, double> activeGradeShares)
    {
        var shares = BuildGridGradeShares(grid, activeGradeShares);
        return GradeOrder.ToDictionary(
            grade => grade,
            grade => total * shares.GetValueOrDefault(grade),
            StringComparer.OrdinalIgnoreCase);
    }

    private Dictionary<string, double> BuildGridGradeShares(string grid, Dictionary<string, double> activeGradeShares)
    {
        if (grid.Equals("MHESD", StringComparison.OrdinalIgnoreCase))
        {
            return AllocateGroupShares(activeGradeShares, highSchoolShare: 1.0, tk8Share: 0.0, specialShare: 0.0);
        }

        var baseSpecialShare = activeGradeShares.GetValueOrDefault("Sp. Ed.");
        var baseTk8Share = Tk8Grades.Sum(grade => activeGradeShares.GetValueOrDefault(grade));
        var baseHighSchoolShare = HighSchoolGrades.Sum(grade => activeGradeShares.GetValueOrDefault(grade));
        var proxyTk8Share = schoolProxyRatios.GetValueOrDefault(grid, baseTk8Share);
        var tk8Share = Math.Clamp(proxyTk8Share, 0.0, 0.96);
        var specialShare = Math.Min(baseSpecialShare, Math.Max(0, 0.98 - tk8Share));
        var highSchoolShare = Math.Max(0, 1.0 - tk8Share - specialShare);

        if (!MatchingElementarySchools.Contains(grid) && !grid.Equals("Inter-Districts", StringComparison.OrdinalIgnoreCase))
        {
            tk8Share = baseTk8Share;
            highSchoolShare = baseHighSchoolShare;
            specialShare = baseSpecialShare;
        }

        return AllocateGroupShares(activeGradeShares, highSchoolShare, tk8Share, specialShare);
    }

    private static Dictionary<string, double> AllocateGroupShares(
        Dictionary<string, double> activeGradeShares,
        double highSchoolShare,
        double tk8Share,
        double specialShare)
    {
        var result = EmptyGradeDictionary();
        ApplyShareGroup(result, activeGradeShares, Tk8Grades, tk8Share);
        ApplyShareGroup(result, activeGradeShares, HighSchoolGrades, highSchoolShare);
        result["Sp. Ed."] = specialShare;
        return NormalizeShares(null, result);
    }

    private static void ApplyShareGroup(
        Dictionary<string, double> target,
        Dictionary<string, double> activeGradeShares,
        IReadOnlyList<string> grades,
        double groupShare)
    {
        var denominator = grades.Sum(grade => activeGradeShares.GetValueOrDefault(grade));
        if (denominator <= 0 || groupShare <= 0) return;
        foreach (var grade in grades)
        {
            target[grade] = groupShare * activeGradeShares.GetValueOrDefault(grade) / denominator;
        }
    }

    private Dictionary<string, Dictionary<string, double>> RollUpSchoolGrades(Dictionary<string, Dictionary<string, double>> gridGrades)
    {
        var schools = new Dictionary<string, Dictionary<string, double>>(StringComparer.OrdinalIgnoreCase)
        {
            ["MHHS"] = EmptyGradeDictionary(),
            ["Special Programs"] = EmptyGradeDictionary()
        };

        foreach (var (grid, grades) in gridGrades)
        {
            foreach (var grade in GradeOrder)
            {
                var count = grades.GetValueOrDefault(grade);
                if (count <= 0) continue;

                var school = SchoolForGridGrade(grid, grade);
                if (school is null) continue;
                if (!schools.TryGetValue(school, out var schoolGrades))
                {
                    schoolGrades = EmptyGradeDictionary();
                    schools[school] = schoolGrades;
                }

                schoolGrades[grade] += count;
            }
        }

        return schools
            .Where(kvp => kvp.Value.Values.Sum() > 0)
            .OrderBy(kvp => kvp.Key.Equals("MHHS", StringComparison.OrdinalIgnoreCase) ? "zzzz" : kvp.Key)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.OrdinalIgnoreCase);
    }

    private static string? SchoolForGridGrade(string grid, string grade)
    {
        if (HighSchoolGrades.Contains(grade, StringComparer.OrdinalIgnoreCase)) return "MHHS";
        if (grade.Equals("Sp. Ed.", StringComparison.OrdinalIgnoreCase)) return "Special Programs";
        if (grid.Equals("MHESD", StringComparison.OrdinalIgnoreCase)) return null;
        if (MatchingElementarySchools.Contains(grid)) return grid;
        if (grid.Equals("Inter-Districts", StringComparison.OrdinalIgnoreCase)) return "Inter-Districts";
        return grid;
    }

    private static Dictionary<string, double> EmptyGradeDictionary()
    {
        return GradeOrder.ToDictionary(grade => grade, _ => 0.0, StringComparer.OrdinalIgnoreCase);
    }

    private static Dictionary<string, double> SumGrades(Dictionary<string, double> left, Dictionary<string, double> right)
    {
        foreach (var grade in GradeOrder)
        {
            left[grade] = left.GetValueOrDefault(grade) + right.GetValueOrDefault(grade);
        }

        return left;
    }

    private double CalculateNewStudentYield(int projectionYear, IReadOnlyList<ScenarioHome> scenarioHomes, Dictionary<string, double> activeDensityYields)
    {
        return scenarioHomes.Sum(home =>
        {
            if (home.Homes <= 0 || home.Year > projectionYear) return 0;
            var age = projectionYear - home.Year;
            return home.Homes * activeDensityYields.GetValueOrDefault(home.Density, 0.35) * AgeFactor(age);
        });
    }

    private double CalculateNeighborhoodYield(string neighborhood, int projectionYear, IReadOnlyList<ScenarioHome> scenarioHomes, Dictionary<string, double> activeDensityYields)
    {
        return scenarioHomes
            .Where(home => home.Neighborhood.Equals(neighborhood, StringComparison.OrdinalIgnoreCase))
            .Sum(home =>
            {
                if (home.Homes <= 0 || home.Year > projectionYear) return 0;
                return home.Homes * activeDensityYields.GetValueOrDefault(home.Density, 0.35) * AgeFactor(projectionYear - home.Year);
            });
    }

    private double CalculateGridNewStudentYield(string grid, int projectionYear, IReadOnlyList<ScenarioHome> scenarioHomes, Dictionary<string, double> activeDensityYields)
    {
        return scenarioHomes
            .Where(home => home.Neighborhood.Equals(grid, StringComparison.OrdinalIgnoreCase))
            .Sum(home =>
            {
                if (home.Homes <= 0 || home.Year > projectionYear) return 0;
                return home.Homes * activeDensityYields.GetValueOrDefault(home.Density, 0.35) * AgeFactor(projectionYear - home.Year);
            });
    }

    private Dictionary<string, double> GetGradeCountsForLatestAugust()
    {
        var latestYear = data.AugustYears.Last();
        var term = $"Aug '{latestYear % 100:00}";
        return GradeOrder.ToDictionary(
            grade => grade,
            grade => data.GradeRows.FirstOrDefault(r => r.Name.Equals(grade, StringComparison.OrdinalIgnoreCase))
                ?.Values.GetValueOrDefault(term) ?? 0,
            StringComparer.OrdinalIgnoreCase);
    }

    private static (List<double[]> Features, List<double> Targets) BuildCalibrationObservations(EnrollmentData data, IReadOnlyList<string> densities)
    {
        var features = new List<double[]>();
        var targets = new List<double>();
        var homeNeighborhoods = data.HomeRows.Select(h => h.Neighborhood).Distinct(StringComparer.OrdinalIgnoreCase).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var gridRow in data.GridRows)
        {
            if (!homeNeighborhoods.Contains(gridRow.Name)) continue;

            foreach (var year in data.AugustYears)
            {
                var term = $"Aug '{year % 100:00}";
                if (!gridRow.Values.TryGetValue(term, out var target) || target is null or <= 0) continue;

                var row = new double[densities.Count + 1];
                for (var i = 0; i < densities.Count; i++)
                {
                    row[i] = data.HomeRows
                        .Where(h => h.Neighborhood.Equals(gridRow.Name, StringComparison.OrdinalIgnoreCase)
                            && h.Density.Equals(densities[i], StringComparison.OrdinalIgnoreCase))
                        .Sum(h => h.HomesByYear.Where(kvp => kvp.Key <= year)
                            .Sum(kvp => kvp.Value * AgeFactor(year - kvp.Key)));
                }

                row[^1] = 1.0;
                if (row.Take(densities.Count).Sum() <= 0) continue;
                features.Add(row);
                targets.Add(target.Value);
            }
        }

        return (features, targets);
    }

    private static double[] SolveRidge(IReadOnlyList<double[]> x, IReadOnlyList<double> y, double lambda)
    {
        if (x.Count == 0) return [0.25, 0.35, 0.45, 0.55, 0];
        var n = x[0].Length;
        var a = new double[n, n];
        var b = new double[n];

        for (var row = 0; row < x.Count; row++)
        {
            for (var i = 0; i < n; i++)
            {
                b[i] += x[row][i] * y[row];
                for (var j = 0; j < n; j++) a[i, j] += x[row][i] * x[row][j];
            }
        }

        for (var i = 0; i < n - 1; i++) a[i, i] += lambda;
        return GaussianElimination(a, b);
    }

    private static double[] GaussianElimination(double[,] a, double[] b)
    {
        var n = b.Length;
        for (var pivot = 0; pivot < n; pivot++)
        {
            var best = pivot;
            for (var row = pivot + 1; row < n; row++)
            {
                if (Math.Abs(a[row, pivot]) > Math.Abs(a[best, pivot])) best = row;
            }

            if (best != pivot)
            {
                for (var col = pivot; col < n; col++) (a[pivot, col], a[best, col]) = (a[best, col], a[pivot, col]);
                (b[pivot], b[best]) = (b[best], b[pivot]);
            }

            var divisor = Math.Abs(a[pivot, pivot]) < 1e-9 ? 1e-9 : a[pivot, pivot];
            for (var col = pivot; col < n; col++) a[pivot, col] /= divisor;
            b[pivot] /= divisor;

            for (var row = 0; row < n; row++)
            {
                if (row == pivot) continue;
                var factor = a[row, pivot];
                for (var col = pivot; col < n; col++) a[row, col] -= factor * a[pivot, col];
                b[row] -= factor * b[pivot];
            }
        }

        return b;
    }

    private static Dictionary<string, double> CalculateLatestGradeShares(EnrollmentData data)
    {
        var latestYear = data.AugustYears.Last();
        var term = $"Aug '{latestYear % 100:00}";
        var counts = GradeOrder.ToDictionary(
            grade => grade,
            grade => data.GradeRows.FirstOrDefault(r => r.Name.Equals(grade, StringComparison.OrdinalIgnoreCase))
                ?.Values.GetValueOrDefault(term) ?? 0,
            StringComparer.OrdinalIgnoreCase);
        return NormalizeShares(null, counts);
    }

    private static Dictionary<string, double> CalculateRetentionRates(EnrollmentData data)
    {
        var rates = GradeOrder.ToDictionary(grade => grade, _ => 1.0, StringComparer.OrdinalIgnoreCase);
        var augTerms = data.AugustYears.Select(year => $"Aug '{year % 100:00}").ToList();

        for (var i = 1; i < GradeOrder.Length - 1; i++)
        {
            var targetGrade = GradeOrder[i];
            var sourceGrade = i == 1 ? "TK" : GradeOrder[i - 1];
            var observed = new List<double>();

            for (var t = 1; t < augTerms.Count; t++)
            {
                var source = ValueForGrade(data, sourceGrade, augTerms[t - 1]);
                var target = ValueForGrade(data, targetGrade, augTerms[t]);
                if (source > 0 && target > 0) observed.Add(target / source);
            }

            if (observed.Count > 0) rates[targetGrade] = Math.Clamp(observed.Average(), 0.75, 1.25);
        }

        rates["TK"] = 0.25;
        rates["Sp. Ed."] = Math.Clamp(YearOverYearAverage(data, "Sp. Ed.", augTerms), 0.85, 1.15);
        return rates;
    }

    private static Dictionary<string, double> CalculateGridTrendRates(EnrollmentData data)
    {
        var augTerms = data.AugustYears.Select(year => $"Aug '{year % 100:00}").ToList();
        return data.GridRows.ToDictionary(
            grid => grid.Name,
            grid =>
            {
                var observed = new List<double>();
                for (var i = 1; i < augTerms.Count; i++)
                {
                    var previous = grid.Values.GetValueOrDefault(augTerms[i - 1]) ?? 0;
                    var current = grid.Values.GetValueOrDefault(augTerms[i]) ?? 0;
                    if (previous > 0 && current > 0) observed.Add(current / previous);
                }

                return observed.Count == 0 ? 1.0 : Math.Clamp(observed.TakeLast(4).Average(), 0.92, 1.12);
            },
            StringComparer.OrdinalIgnoreCase);
    }

    private static Dictionary<string, double> CalculateSchoolProxyRatios(EnrollmentData data)
    {
        var augTerms = data.AugustYears.Select(year => $"Aug '{year % 100:00}").Reverse().Take(4).ToList();
        var result = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        foreach (var grid in data.GridRows)
        {
            if (grid.Name.Equals("MHESD", StringComparison.OrdinalIgnoreCase)) continue;
            var school = data.SchoolRows.FirstOrDefault(row => row.Name.Equals(grid.Name, StringComparison.OrdinalIgnoreCase));
            if (school is null) continue;

            var ratios = augTerms
                .Select(term =>
                {
                    var gridValue = grid.Values.GetValueOrDefault(term) ?? 0;
                    var schoolValue = school.Values.GetValueOrDefault(term) ?? 0;
                    return gridValue > 0 && schoolValue > 0 ? schoolValue / gridValue : 0;
                })
                .Where(ratio => ratio > 0)
                .ToList();

            if (ratios.Count > 0) result[grid.Name] = Math.Clamp(ratios.Average(), 0.35, 0.96);
        }

        return result;
    }

    private static double ValueForGrade(EnrollmentData data, string grade, string term)
    {
        return data.GradeRows.FirstOrDefault(row => row.Name.Equals(grade, StringComparison.OrdinalIgnoreCase))
            ?.Values.GetValueOrDefault(term) ?? 0;
    }

    private static double YearOverYearAverage(EnrollmentData data, string grade, IReadOnlyList<string> terms)
    {
        var observed = new List<double>();
        for (var i = 1; i < terms.Count; i++)
        {
            var previous = ValueForGrade(data, grade, terms[i - 1]);
            var current = ValueForGrade(data, grade, terms[i]);
            if (previous > 0 && current > 0) observed.Add(current / previous);
        }

        return observed.Count == 0 ? 1.0 : observed.Average();
    }

    private static double CalculateRecentAverage(EnrollmentData data, string grade, int count)
    {
        var terms = data.AugustYears
            .Select(year => $"Aug '{year % 100:00}")
            .Reverse()
            .Take(count)
            .ToList();
        var values = terms
            .Select(term => ValueForGrade(data, grade, term))
            .Where(value => value > 0)
            .ToList();
        return values.Count == 0 ? 0 : values.Average();
    }

    private static double LatestNonNullValue(SeriesRow row)
    {
        return row.Values.Values.Reverse().FirstOrDefault(value => value.HasValue) ?? 0;
    }

    private static Dictionary<string, double> NormalizeShares(Dictionary<string, double>? requested, Dictionary<string, double> fallback)
    {
        var raw = GradeOrder.ToDictionary(
            grade => grade,
            grade => Math.Max(0, requested?.GetValueOrDefault(grade) ?? fallback.GetValueOrDefault(grade)),
            StringComparer.OrdinalIgnoreCase);
        var total = raw.Values.Sum();
        if (total <= 0) return GradeOrder.ToDictionary(grade => grade, _ => 1.0 / GradeOrder.Length, StringComparer.OrdinalIgnoreCase);
        return raw.ToDictionary(kvp => kvp.Key, kvp => kvp.Value / total, StringComparer.OrdinalIgnoreCase);
    }

    private static double AgeFactor(int age)
    {
        return age switch
        {
            < 0 => 0,
            0 => 0.30,
            1 => 0.60,
            2 => 0.85,
            >= 3 and <= 5 => 1.00,
            >= 6 and <= 10 => 0.90,
            >= 11 and <= 15 => 0.75,
            _ => 0.62
        };
    }
}
