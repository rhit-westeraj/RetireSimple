﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetireSimple.Backend.DomainModel.Data.Investment;

namespace RetireSimple.Backend.DomainModel.Data.InvestmentVehicle {
	public abstract class InvestmentVehicleBase {
		public int InvestmentVehicleId { get; set; }

		public string InvestmentVehicleType { get; set; }

		//TODO Relational Issues to resolve
		protected List<InvestmentBase> investments;

		public InvestmentVehicleBase() {
			investments = new List<InvestmentBase>();
		}

		public abstract InvestmentModel GenerateAnalysis(Dictionary<string,string> options);
	}

	public class InvestmentVehicleBaseConfiguration : IEntityTypeConfiguration<InvestmentVehicleBase> {
		public void Configure(EntityTypeBuilder<InvestmentVehicleBase> builder) {
			builder.HasKey(i => i.InvestmentVehicleId);

			builder.HasDiscriminator(i => i.InvestmentVehicleType);
		}
	}

}