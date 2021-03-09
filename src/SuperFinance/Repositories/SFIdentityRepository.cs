using System;
using System.Linq;
using System.Threading.Tasks;
using SuperFinance.DataModels;
using SuperFinance.Security;
using ASPSecurityKit;
using Microsoft.EntityFrameworkCore;
using ASKSource.Models;
using ASKSource.DataModels;
using ASKSource.Infrastructure;
using ASKSource.Repositories;
using ASKSource.Security;

namespace SuperFinance.Repositories
{
	public class SFIdentityRepository : IdentityRepository
	{
		private readonly DemoDbContext dbContext;

		public SFIdentityRepository(DemoDbContext dbContext, IBrowser browser,
			ISecuritySettings securitySettings, ISecurityContext securityContext,
			ISecurityUtility securityUtility, ILogger logger) : base(dbContext, browser, securitySettings,
			securityContext, securityUtility, logger)
		{
			this.dbContext = dbContext;
		}

		protected override IdentityAuthDetails NewAuthDetails() => new SFIdentityAuthDetails();

		protected override IAuthDetails LoadAdditionalData(DbUserSession dbSession, IdentityAuthDetails auth)
		{
			throw new NotImplementedException("Use async");
		}

		protected override async Task<IAuthDetails> LoadAdditionalDataAsync(DbUserSession dbSession, IdentityAuthDetails auth)
		{
			var skipMFAInsideNetwork = false;
			var firewallEnabled = false;
			var entityUrn = string.Empty;

			if (dbSession.User.UserType == UserType.Staff && auth is SFIdentityAuthDetails authDetails)
			{
				var owningUserId = dbSession.User.ParentId ?? dbSession.UserId;
				var dbBank = await dbContext.Banks.FirstOrDefaultAsync(x => x.OwningUserId == owningUserId)
					.ConfigureAwait(false);

				if (dbBank != null)
				{
					authDetails.BankId = dbBank.Id;
					authDetails.MFAEnforced = dbBank.EnforceMFA;
					skipMFAInsideNetwork = dbBank.SkipMFAInsideNetwork;
					firewallEnabled = dbBank.FirewallEnabled;
					entityUrn = EntityUrn.MakeUrn(SFEntityTypes.Bank, dbBank.Id);
				}
			}
			else
			{
				firewallEnabled = dbSession.User.FirewallEnabled;
				entityUrn = EntityUrn.MakeUrn(EntityTypes.User, dbSession.User.Id);
			}

			if (firewallEnabled && !string.IsNullOrEmpty(entityUrn))
			{
				auth.FirewallIpRanges = await this.dbContext.FirewallRules
					.Where(x => x.EntityUrn == entityUrn)
					.OrderBy(p => p.Name)
					.Select(x => new FirewallIpRange
					{
						Id = x.Id,
						Name = x.Name,
						IpFrom = x.IpFrom,
						IpTo = x.IpTo
					})
					.ToListAsync<IFirewallIpRange>().ConfigureAwait(false);
			}
			else
			{
				auth.FirewallIpRanges = FirewallIpRange.RangeForWholeOfInternet();
			}

			if (skipMFAInsideNetwork)
			{
				auth.MFAWhiteListedIpRanges = auth.FirewallIpRanges;
			}

			AddLocalhostIpToFirewall(auth);

			return auth;
		}

		private void AddLocalhostIpToFirewall(IAuthDetails auth)
		{
			if (ASPSecurityKitConfiguration.IsDevelopmentEnvironment)
			{
				if (auth.FirewallIpRanges == null)
				{
					auth.FirewallIpRanges = FirewallIpRange.RangeForLocalhost();
				}
				else
				{
					auth.FirewallIpRanges.AddRange(FirewallIpRange.RangeForLocalhost());
				}
			}
		}
	}
}