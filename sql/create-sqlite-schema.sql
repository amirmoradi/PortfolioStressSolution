CREATE TABLE IF NOT EXISTS ScenarioRuns (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    StartedUtc TEXT NOT NULL,
    CompletedUtc TEXT NOT NULL,
    DurationMilliseconds INTEGER NOT NULL,
    CalculationProfile TEXT NOT NULL,
    ScenarioCollateralFormula TEXT NOT NULL,
    RecoveryRateBase TEXT NOT NULL,
    ClampNegativeLossGivenDefaultToZero INTEGER NOT NULL,
    SourcePortfoliosPath TEXT NOT NULL,
    SourceLoansPath TEXT NOT NULL,
    SourceRatingsPath TEXT NOT NULL,
    PortfolioCount INTEGER NOT NULL,
    LoanCount INTEGER NOT NULL,
    ResultCount INTEGER NOT NULL,
    TotalOutstandingAmount NUMERIC NOT NULL,
    TotalCollateralValue NUMERIC NOT NULL,
    TotalScenarioCollateralValue NUMERIC NOT NULL,
    TotalExpectedLoss NUMERIC NOT NULL
);

CREATE TABLE IF NOT EXISTS ScenarioCountryInputs (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ScenarioRunId INTEGER NOT NULL,
    CountryCode TEXT NOT NULL,
    PercentageChange NUMERIC NOT NULL,
    CONSTRAINT FK_ScenarioCountryInputs_ScenarioRuns FOREIGN KEY (ScenarioRunId) REFERENCES ScenarioRuns(Id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_ScenarioCountryInputs_ScenarioRunId_CountryCode
    ON ScenarioCountryInputs (ScenarioRunId, CountryCode);

CREATE TABLE IF NOT EXISTS ScenarioPortfolioResults (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ScenarioRunId INTEGER NOT NULL,
    PortfolioId INTEGER NOT NULL,
    PortfolioName TEXT NOT NULL,
    CountryCode TEXT NOT NULL,
    CurrencyCode TEXT NOT NULL,
    LoanCount INTEGER NOT NULL,
    TotalOutstandingAmount NUMERIC NOT NULL,
    TotalCollateralValue NUMERIC NOT NULL,
    TotalScenarioCollateralValue NUMERIC NOT NULL,
    TotalExpectedLoss NUMERIC NOT NULL,
    CONSTRAINT FK_ScenarioPortfolioResults_ScenarioRuns FOREIGN KEY (ScenarioRunId) REFERENCES ScenarioRuns(Id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_ScenarioPortfolioResults_ScenarioRunId_PortfolioId
    ON ScenarioPortfolioResults (ScenarioRunId, PortfolioId);
