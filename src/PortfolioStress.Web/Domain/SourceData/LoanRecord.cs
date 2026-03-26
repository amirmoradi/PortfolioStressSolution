namespace PortfolioStress.Web.Domain.SourceData;

public record LoanRecord(long LoanId, int PortfolioId, decimal OriginalLoanAmount, decimal OutstandingAmount, decimal CollateralValue, string CreditRating);
