using Microsoft.Extensions.Options;
using RetireSimple.Backend.DomainModel.Data;
using RetireSimple.Backend.DomainModel.Data.Investment;

namespace RetireSimple.Backend.DomainModel.Analysis {

	public class StockAS {

		//This dictionary is statically available to allow for a common set of defaults that all
		//analysis modules for the same type of investment can use. 
		public static readonly OptionsDict DefaultStockAnalysisOptions = new() {
			["AnalysisLength"] = "60",                          //Number of months to project
			["StockAnalysisExpectedGrowth"] = "0.1",            //Expected Percentage Growth of the stock

		};

		private static int GetDividendIntervalMonths(string interval) => interval switch {
			"Month" => 1,
			"Quarter" => 3,
			"Annual" => 12,
			_ => throw new ArgumentException("Invalid Dividend Interval")
		};

		/// <summary>
		/// Generates a list of monthly projected stock quantities using stock dividend distribution.
		/// First element of list is assumed to be for the current month/year.
		/// </summary>
		/// <param name="investment"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static List<decimal> ProjectStockDividend(StockInvestment investment, OptionsDict options) {
			var quantityList = new List<decimal>();
			var stockQuantity = investment.StockQuantity;
			var dividendPercent = investment.StockDividendPercent;
			var currentMonth = DateTime.Now.Month;
			var firstDividendMonth = investment.StockDividendFirstPaymentDate.Month;
			var monthInterval = GetDividendIntervalMonths(investment.StockDividendDistributionInterval);

			//quantityList.Add(stockQuantity);
			for(int i = 0; i < int.Parse(options["AnalysisLength"]); i++) {
				if((currentMonth - firstDividendMonth) % monthInterval == 0) {
					stockQuantity += stockQuantity * dividendPercent;
				}
				quantityList.Add(stockQuantity);
				currentMonth++;
				if(currentMonth > 12) {
					currentMonth = 1;
				}
			}

			return quantityList;
		}

		public static InvestmentModel MonteCarlo_NormalDist(StockInvestment investment, OptionsDict options) {
			var priceModel = MonteCarlo.MonteCarloSim_NormalDistribution(investment, options);
			//TODO Update to support other dividend types
			var dividendModel = StockAS.ProjectStockDividend(investment, options);

			priceModel.MinModelData = priceModel.MinModelData.Zip(dividendModel, (price, dividend) => price * dividend).ToList();
			priceModel.AvgModelData = priceModel.AvgModelData.Zip(dividendModel, (price, dividend) => price * dividend).ToList();
			priceModel.MaxModelData = priceModel.MaxModelData.Zip(dividendModel, (price, dividend) => price * dividend).ToList();

			return priceModel;
		}

		//TODO Move to Testing/Debugging
		public static InvestmentModel testAnalysis(StockInvestment investment, OptionsDict options) {
			var value = investment.StockPrice * investment.StockQuantity;

			//HACK I think this code is leaky, but it won't exist in final builds
			//Just PoC for Investments

			return new InvestmentModel() {
				InvestmentId = investment.InvestmentId,
				MaxModelData = new List<decimal>() {
					value,
					2*value,
					4*value
				},
				MinModelData = new List<decimal>() {
					value,
					0.5m * value,
					0.25m * value
				},
				AvgModelData = new List<decimal>() {
					value,
					value,
					2* value
				}
			};
		}


	}
}