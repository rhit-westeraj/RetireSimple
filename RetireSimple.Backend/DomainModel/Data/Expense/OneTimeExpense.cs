namespace RetireSimple.Backend.DomainModel.Data.Expense {
	/// <summary>
	/// Represents an Expense on an investment that occurs once
	/// </summary>
	public class OneTimeExpense : ExpenseBase {
		/// <summary>
		/// The date the expense is applied to the investment
		/// </summary>
		public DateTime Date { get; set; }

		public override List<DateTime> GetExpenseDates() => new List<DateTime> { this.Date };


	}

}
