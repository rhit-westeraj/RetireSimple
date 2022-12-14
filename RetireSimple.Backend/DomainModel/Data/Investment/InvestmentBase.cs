using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetireSimple.Backend.DomainModel.Data.Expense;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RetireSimple.Backend.DomainModel.Data.Investment {
	//
	public delegate InvestmentModel AnalysisDelegate<T>(T investment, OptionsDict options) where T : InvestmentBase;


	[Table("Investments")]
	public abstract class InvestmentBase {

		/// <summary>
		/// Primary Key of the Investment Table
		/// </summary>
		public int InvestmentId { get; set; }

		/// <summary>
		/// Name of the investment (e.g. "My 401k")
		/// </summary>
		public string InvestmentName { get; set; } = "";

		/// <summary>
		/// Discriminator Field for the Investment Table to differentiate investment Types. 
		/// Check the discriminator configuration in <see cref="InvestmentBaseConfiguration"/> for valid discriminator values.
		/// </summary>
		public string InvestmentType { get; set; }


		/// <summary>
		/// This is the easiest way to store data while maintaining type safety.<br/>
		/// It's recommended to create getter/setter methods for properties you expect to exist in this map
		/// </summary>
		public OptionsDict InvestmentData { get; set; } = new OptionsDict();

		/// <summary>
		/// Overrides to pass to the analysis module when invoking analysis. <br/>
		/// Check the summary of the specified analysis module for the possible analysis keys it expects. It is also important to note that the analysis module may not use all of the keys you pass in.
		/// </summary>
		public OptionsDict AnalysisOptionsOverrides { get; set; } = new OptionsDict();

		/// <summary>
		/// The type of analysis module to use when invoking analysis. <br/>
		/// This is set/updated in the <see cref="ResolveAnalysisDelegate(string)"/> method during EF object conversion so that the correct analysis module is used in runtime<br/>
		/// </summary>
		public string? AnalysisType { get; set; }

		/// <summary>
		/// List of expense objects associated with this investment. <br/>
		/// This is not passed during JSON Serialization and should be accessed through EF.
		/// </summary>
		[JsonIgnore]
		public List<ExpenseBase> Expenses { get; set; } = new List<ExpenseBase>();

		/// <summary>
		/// List of transfers objects associated with this investment that pull value from the investment. <br/>
		/// This is not passed during JSON Serialization and should be accessed through EF.
		/// </summary>
		[JsonIgnore]
		public List<InvestmentTransfer> TransfersFrom { get; set; } = new List<InvestmentTransfer>();

		/// <summary>
		/// List of transfers objects associated with this investment that add value to the investment. <br/>
		/// This is not passed during JSON Serialization and should be accessed through EF.
		/// </summary>
		[JsonIgnore]
		public List<InvestmentTransfer> TransfersTo { get; set; } = new List<InvestmentTransfer>();

		/// <summary>
		/// Value of the last time the object was updated. Can be used with <see cref="InvestmentModel.LastUpdated"/> to determine if analysis needs to be run again.
		/// </summary>
		public DateTime? LastAnalysis { get; set; }

		/// <summary>
		/// The created <see cref="InvestmentModel"/> for this investment based on the <see cref="AnalysisDelegate{T}"/> run. If analysis has not been run prevously, this will be null.
		/// </summary>
		[JsonIgnore]
		public InvestmentModel? InvestmentModel { get; set; }

		/// <summary>
		/// The ID of the <see cref="RetireSimple.Backend.DomainModel.User.Portfolio"/> that contains this investement object.
		/// </summary>
		public int PortfolioId { get; set; }

		/// <summary>
		/// Method to assign the correct analysis module to the investment's type-specifc analysis module property. Strings are currently hard coded in the specific investment type.<br/>
		/// </summary>
		/// <param name="analysisType"></param>
		public abstract void ResolveAnalysisDelegate(string analysisType);

		/// <summary>
		/// Stub for invoking an associated analysis module. <br/>
		/// There is a bunch of things you can do before actually invoking the delegate, such as applying overrides or other logic. <br/>
		/// </summary>
		/// <param name="options">A set of externally defined options that should be used as an override</param>
		/// <returns>Specifc model generated by the analysis module</returns>
		public abstract InvestmentModel InvokeAnalysis(OptionsDict options);

	}

	public class InvestmentBaseConfiguration : IEntityTypeConfiguration<InvestmentBase> {
		static JsonSerializerOptions options = new JsonSerializerOptions {
			AllowTrailingCommas = true,
			DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
			IncludeFields = true
		};

		public void Configure(EntityTypeBuilder<InvestmentBase> builder) {
			builder.HasKey(i => i.InvestmentId);

			//TODO Allow Cascade of models as those should not exist if the investment still exists
			builder.HasOne(i => i.InvestmentModel)
					.WithOne(i => i.Investment)
					.OnDelete(DeleteBehavior.Restrict);

			builder.HasDiscriminator(i => i.InvestmentType)
					.HasValue<StockInvestment>("StockInvestment")
					.HasValue<BondInvestment>("BondInvestment")
					.HasValue<FixedInvestment>("FixedInvestment")
					.HasValue<CashInvestment>("CashInvestment")
					.HasValue<SocialSecurityInvestment>("SocialSecurityInvestment")
					.HasValue<AnnuityInvestment>("AnnuityInvestment")
					.HasValue<PensionInvestment>("PensionInvestment");
			
#pragma warning disable CS8604 // Possible null reference argument.
			builder.Property(i => i.InvestmentData)
				.HasConversion(
					v => JsonSerializer.Serialize(v, options),
					v => JsonSerializer.Deserialize<OptionsDict>(v, options) ?? new OptionsDict()
				)
				.Metadata.SetValueComparer(new ValueComparer<OptionsDict>(
					(c1, c2) => c1.SequenceEqual(c2),
					c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
					c => c.ToDictionary(entry => entry.Key, entry => entry.Value)
				));

			builder.Property(i => i.AnalysisOptionsOverrides)
				.HasConversion(
					v => JsonSerializer.Serialize(v, options),
					v => JsonSerializer.Deserialize<OptionsDict>(v, options) ?? new OptionsDict()
				)
				.Metadata.SetValueComparer(new ValueComparer<OptionsDict>(
					(c1, c2) => c1.SequenceEqual(c2),
					c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
					c => c.ToDictionary(entry => entry.Key, entry => entry.Value)
				));
#pragma warning restore CS8604 // Possible null reference argument.
			
			builder.Property(i => i.InvestmentName)
				.HasDefaultValue("");

			builder.Property(i => i.AnalysisType)
					.HasColumnName("AnalysisType");

			builder.Property(i => i.LastAnalysis)
					.HasColumnType("datetime2(7)");


		}
	}
}