namespace PortfolioStress.Web.Application.Models.Policy
{
    /// <summary>
    /// Defines the calculation profiles used when running a scenario.
    /// As mentioned in the Readme file, the exercise is not fully clear on how some values should be calculated,
    /// so these profiles represent two different approaches.
    /// </summary>
    public enum CalculationProfile
    {
        /// <summary>
        /// follow the exercise wording as closely as possible.
        /// </summary>
        ExerciseDetails = 1,

        /// <summary>
        /// more realistic market stress approach.
        /// </summary>
        MarketStress = 2
    }
}
