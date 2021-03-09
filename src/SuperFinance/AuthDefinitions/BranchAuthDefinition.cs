using System.Threading.Tasks;
using ASKSource.AuthDefinitions;
using ASPSecurityKit.Authorization;
using ASPSecurityKit.AuthProviders;
using SuperFinance.DataModels;
using SuperFinance.ViewModels;

namespace SuperFinance.AuthDefinitions
{
	public class BranchAuthDefinition : AuthDefinitionBase
	{
		public BranchAuthDefinition(DemoDbContext dbContext, IEntityIdAuthorizer entityIdAuthorizer)
			: base(dbContext, entityIdAuthorizer)
		{
		}

		public async Task<AuthResult> AddStaffAsync(AddStaffModel model, string permissionCode)
		{
			var result = await this.entityIdAuthorizer.IsAuthorizedAsync("EditUser", new { model.Username });
			if (result.IsSuccess)
			{
				return await this.entityIdAuthorizer.IsAuthorizedAsync(permissionCode, new { model.BranchId });
			}

			return result;
		}
	}
}