using System.Globalization;
using Microsoft.VisualBasic.FileIO;
using PortfolioStress.Web.Application.Interfaces;
using PortfolioStress.Web.Domain.SourceData;

namespace PortfolioStress.Web.Infrastructure.Csv;

/// <summary>
/// Reads source CSV files used by the scenario calculation.
///
/// idea: I avoided adding a third-party CSV library to keep the exercise self-contained,
/// so this uses TextFieldParser which is available in the framework.
/// The files are small so performance here is not critical.
/// </summary>
public class ScenarioSourceDataReader : IScenarioSourceDataReader
{
    public IReadOnlyDictionary<int, PortfolioRecord> ReadPortfolios(string path)
    {
        var results = new Dictionary<int, PortfolioRecord>();

        using var parser = CreateParser(path);
        var headerMap = ReadHeaderMap(parser);

        while (!parser.EndOfData)
        {
            var fields = parser.ReadFields();
            if (fields is null || fields.Length == 0)
            {
                continue;
            }

            var record = new PortfolioRecord(
                PortfolioId: GetInt32(fields, headerMap, "Port_ID"),
                PortfolioName: GetString(fields, headerMap, "Port_Name"),
                CountryCode: GetString(fields, headerMap, "Port_Country").ToUpperInvariant(),
                CurrencyCode: GetString(fields, headerMap, "Port_CCY").ToUpperInvariant());

            results.Add(record.PortfolioId, record);
        }

        return results;
    }

    public IReadOnlyDictionary<string, RatingRecord> ReadRatings(string path)
    {
        var results = new Dictionary<string, RatingRecord>(StringComparer.OrdinalIgnoreCase);

        using var parser = CreateParser(path);
        var headerMap = ReadHeaderMap(parser);

        while (!parser.EndOfData)
        {
            var fields = parser.ReadFields();
            if (fields is null || fields.Length == 0)
            {
                continue;
            }

            var rating = GetString(fields, headerMap, "Rating").ToUpperInvariant();

            // CSV stores percentage so convert to decimal (i.e.. 5 → 0.05)
            var probabilityOfDefault =
                GetDecimal(fields, headerMap, "ProbablilityOfDefault") / 100m;

            results[rating] = new RatingRecord(rating, probabilityOfDefault);
        }

        return results;
    }

    public IEnumerable<LoanRecord> ReadLoans(string path)
    {
        using var parser = CreateParser(path);
        var headerMap = ReadHeaderMap(parser);

        while (!parser.EndOfData)
        {
            var fields = parser.ReadFields();
            if (fields is null || fields.Length == 0)
            {
                continue;
            }

            yield return new LoanRecord(
                LoanId: GetInt64(fields, headerMap, "Loan_ID"),
                PortfolioId: GetInt32(fields, headerMap, "Port_ID"),
                OriginalLoanAmount: GetDecimal(fields, headerMap, "OriginalLoanAmount"),
                OutstandingAmount: GetDecimal(fields, headerMap, "OutstandingAmount"),
                CollateralValue: GetDecimal(fields, headerMap, "CollateralValue"),
                CreditRating: GetString(fields, headerMap, "CreditRating").ToUpperInvariant());
        }
    }

    private static TextFieldParser CreateParser(string path)
    {
        // The main reason we are using TextFieldParser is to avoid to any external dependency for this exercise. but there are other ways that can be done as well. 
        var parser = new TextFieldParser(path)
        {
            TextFieldType = FieldType.Delimited,
            HasFieldsEnclosedInQuotes = true,
            TrimWhiteSpace = true
        };

        parser.SetDelimiters(",");
        return parser;
    }

    private static IReadOnlyDictionary<string, int> ReadHeaderMap(TextFieldParser parser)
    {
        var headers = parser.ReadFields();

        if (headers is null || headers.Length == 0)
        {
            throw new InvalidOperationException(
                "CSV file does not contain a header row.");
        }

        // Build lookup so column order does not matter
        return headers
            .Select((header, index) => new { Header = header, Index = index })
            .ToDictionary(
                x => x.Header,
                x => x.Index,
                StringComparer.OrdinalIgnoreCase);
    }

    private static string GetString(string[] fields, IReadOnlyDictionary<string, int> headerMap, string header)
    {
        return fields[GetRequiredIndex(headerMap, header)].Trim();
    }

    private static int GetInt32(string[] fields, IReadOnlyDictionary<string, int> headerMap, string header)
    {
        return int.Parse(
            GetString(fields, headerMap, header),
            CultureInfo.InvariantCulture);
    }

    private static long GetInt64(string[] fields, IReadOnlyDictionary<string, int> headerMap, string header)
    {
        return long.Parse(
            GetString(fields, headerMap, header),
            CultureInfo.InvariantCulture);
    }

    private static decimal GetDecimal(string[] fields, IReadOnlyDictionary<string, int> headerMap, string header)
    {
        return decimal.Parse(
            GetString(fields, headerMap, header),
            NumberStyles.Number,
            CultureInfo.InvariantCulture);
    }

    private static int GetRequiredIndex(IReadOnlyDictionary<string, int> headerMap, string header)
    {
        if (!headerMap.TryGetValue(header, out var index))
        {
            throw new InvalidOperationException(
                $"CSV header '{header}' was not found.");
        }

        return index;
    }
}