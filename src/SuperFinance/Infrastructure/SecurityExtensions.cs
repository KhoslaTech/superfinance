using ASPSecurityKit;
using SuperFinance.Security;
using System;
using System.Linq;
using ASKSource.DataModels;

namespace SuperFinance.Infrastructure
{
	public static class SecurityExtensions
	{
		public static bool IsBankOwner(this IUserService<Guid, Guid, DbUser> userService) => userService.PossessesPermission(SFPermissionCodes.BankOwner);

		public static Guid[] GetEmployeeBranchIds(this IUserService<Guid, Guid, DbUser> userService)
			=> userService.GetLoadedPermissions()
				.Where(x => x.PermissionCode == SFPermissionCodes.BranchManager || x.PermissionCode == SFPermissionCodes.BranchStaff)
				.Select(x => Guid.Parse(x.EntityId.ToString())).ToArray();
	}
}
