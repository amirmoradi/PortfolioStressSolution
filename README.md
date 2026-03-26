# Portfolio Stress Solution

This is my solution for the exercise using ASP.NET Core MVC, SQLite, and a streaming CSV calculation pipeline.

## What this solution does

- Lets a user enter a percentage change for each requested country.
- Reads `portfolios.csv`, `loans.csv`, and `ratings.csv` from local disk.
- Aggregates results by portfolio.
- Saves each run with:
  - timestamp metadata
  - selected calculation policy
  - country inputs
  - execution duration
  - aggregated portfolio results
- Shows a run history page and a run details page.

## Why this architecture

The exercise is small enough to fit in one web project, but I still separated it internally into simple layers:

- `Domain` contains persisted entities and source-data records.
- `Application` contains models, interfaces, and calculation services.
- `Infrastructure` contains CSV parsing and EF Core persistence.
- `Controllers / Views / ViewModels` are used only for the UI.

This keeps the calculation logic testable and avoids mixing UI code with business logic.

## Important ambiguity in the Exercise Description

The task details has two places that are not very clear:

1. **Scenario collateral value** is written as  
   `Collateral Value * Percentage Change Entered by User`.

   With an input like `-5.12`, a direct calculation gives a negative value,
   so it is not clear if the percentage should be divided by 100 first???

2. **Recovery rate denominator** is written as `Loan Amount`,  
   but the data contains both `OriginalLoanAmount` and `OutstandingAmount`.

Because the requirements are not fully explicit, I made the calculation behaviour configurable.
The UI lets you choose between two calculation policies:

- **Exercise wording**
  - `ScenarioCollateralValue = CollateralValue * (PercentageChange / 100)`
  - `RecoveryRate = ScenarioCollateralValue / OriginalLoanAmount`
  - `LGD = 1 - RR` (without lower clamp)

- **Market shock**
  - `ScenarioCollateralValue = CollateralValue * (1 + PercentageChange / 100)`
  - `RecoveryRate = ScenarioCollateralValue / OutstandingAmount`
  - negative LGD is clamped to zero

If only one interpretation is required, the dropdown can be removed easily.

## Performance characteristics

The implementation is written to stay efficient:

- portfolios and ratings are loaded once into dictionaries
- loans are read from disk one by one
- results are aggregated per portfolio in memory

Because of this, the large loans file can be processed in a single pass
without loading everything into memory.

## Project structure

```text
PortfolioStressSolution/
  src/PortfolioStress.Web/
  tests/PortfolioStress.Tests/
  sql/create-sqlite-schema.sql
  README.md
```


## NOTE: 
The application will create App_Data/portfolio-stress.db automatically on first run.

## CSV files

The CSV files are included under:

- `src/PortfolioStress.Web/Data/portfolios.csv`
- `src/PortfolioStress.Web/Data/loans.csv`
- `src/PortfolioStress.Web/Data/ratings.csv`

Paths can be changed in appsettings.json.

## Database choice

I used SQLite because the Task Description allows a file-based relational database, and it makes the solution easy to run locally.

Schema script:
`sql/create-sqlite-schema.sql`

A blank database file is also included: 
`src/PortfolioStress.Web/App_Data/portfolio-stress.db`.

## Testing

Tests focus on the calculation logic and aggregation:

- loan calculation for both policies
- portfolio totals
- validation for missing country inputs


## Included example outputs

Using the supplied CSV files and sample inputs from the Exercise Descriptoin,
I included example results:

- `docs/sample-results-market-shock.csv`
- `docs/sample-results-exercise-wording.csv`
- `docs/sample-results.md`

## Notes

The main areas I focused on in this solution are:

- keeping the calculation logic separate and testable
- handling unclear parts of the specification explicitly
- keeping the calculation fast for large loan files
- storing full history for each run
- simple UI to enter inputs and view results

The structure could be simplified if needed,
but I kept the layers separate to make the code easier to test and more maintainable.
