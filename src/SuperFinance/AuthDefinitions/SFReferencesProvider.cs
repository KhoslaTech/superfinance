using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASKSource.AuthDefinitions;
using SuperFinance.DataModels;
using ASPSecurityKit;

namespace SuperFinance.AuthDefinitions
{
	public sealed class SFReferencesProvider : ReferencesProvider
	{
		private readonly DemoDbContext dbContext;

		public SFReferencesProvider(ILogger logger, DemoDbContext dbContext) : base(logger, dbContext)
		{
			this.dbContext = dbContext;
		}

		public async Task<List<IdReference>> BranchId(Guid id)
		{
			return await dbContext.Branches
				.Where(x => x.Id == id)
				.Select(x => new List<Guid> { x.Id, x.BankId })
				.ToIdReferenceListAsync();
		}

		public async Task<List<IdReference>> AccountTypeId(Guid id)
		{
			return await dbContext.AccountTypes
				.Where(x => x.Id == id)
				.Select(x => new List<Guid> { x.BankId })
				.ToIdReferenceListAsync();
		}

		public async Task<List<IdReference>> AccountId(Guid id)
		{
			return await dbContext.Accounts
				.Where(x => x.Id == id)
				.Select(x => new List<Guid> { x.Id, x.BranchId, x.Branch.BankId })
				.ToIdReferenceListAsync();
		}

		public async Task<List<IdReference>> NomineeId(Guid id)
		{
			return await dbContext.AccountNominees
				.Where(x => x.Id == id)
				.Select(x => new List<Guid> { x.AccountId, x.Account.BranchId, x.Account.Branch.BankId })
				.ToIdReferenceListAsync();
		}
	}
}
