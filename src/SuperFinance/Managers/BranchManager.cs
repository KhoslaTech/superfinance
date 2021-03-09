using ASPSecurityKit;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SuperFinance.DataModels;
using SuperFinance.Models;
using SuperFinance.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASKSource.DataModels;
using ASKSource.Models;
using ASKSource.Repositories;
using ASKSource.Security;
using SuperFinance.Infrastructure;
using SuperFinance.Repositories;
using SuperFinance.ViewModels;

namespace SuperFinance.Managers
{
	public interface IBranchManager
	{
		Task<List<Branch>> GetBranchesAsync(Guid? bankId, IList<Guid> branchIds);

		Task<PagedResult<Branch>> GetBranchesAsync(int startIndex, int pageSize);

		Task<Branch> AddBranchAsync(Branch branch);

		Task<Branch> EditBranchAsync(Branch branch);

		Task DeleteBranchAsync(Guid id);

		Task AddStaffAsync(AddStaffModel model);
	}

	public class BranchManager : IBranchManager
	{
		private readonly DemoDbContext dbContext;
		private readonly ILogger logger;
		private readonly ISFUserService userService;
		private readonly IUserPermitRepository permitRepository;

		public BranchManager(DemoDbContext dbContext, ISFUserService userService,
			IUserPermitRepository permitRepository, ILogger logger)
		{
			this.dbContext = dbContext;
			this.logger = logger;
			this.userService = userService;
			this.permitRepository = permitRepository;
		}

		public async Task<PagedResult<Branch>> GetBranchesAsync(int startIndex, int pageSize)
		{
			var bankId = this.userService.BankId;

			var list = await this.dbContext.Branches.AsNoTracking()
				.Select(dummy => new
				{
					Total = this.dbContext.Branches.Count(p => p.BankId == bankId),
					ThisPage = this.dbContext.Branches.Where(p => p.BankId == bankId)
						.OrderBy(p => p.Name).Skip(startIndex).Take(pageSize)
						.Select(x => new Branch
						{
							Id = x.Id,
							Name = x.Name,
							Code = x.Code,
							Address = x.Address
						})
						.ToList()
				}).FirstOrDefaultAsync().ConfigureAwait(false);

			var branches = list?.ThisPage.ToList();
			if (branches != null)
			{
				var branchIds = branches.Select(x => x.Id).ToList();
				var staffPermissionCodes = new[] { SFPermissionCodes.BranchManager, SFPermissionCodes.BranchStaff };

				var branchStaff = await this.dbContext.UserPermits.Where(x =>
					branchIds.Contains(x.EntityId) && staffPermissionCodes.Contains(x.PermissionCode)).Select(x =>
					new BranchStaff
					{
						Id = x.Id,
						Name = x.UserPermitGroup.User.Name,
						Username = x.UserPermitGroup.User.Username,
						Role = x.PermissionCode.Replace("Branch", string.Empty),
						BranchId = x.EntityId
					}).ToListAsync().ConfigureAwait(false);

				foreach (var branch in branches)
				{
					branch.Staff = branchStaff.Where(x => x.BranchId == branch.Id).ToList();
				}
			}

			return new PagedResult<Branch>(branches, startIndex, pageSize, list?.Total ?? 0);
		}

		public async Task<List<Branch>> GetBranchesAsync(Guid? bankId, IList<Guid> branchIds)
		{
			var dbBranches = dbContext.Branches.Where(p => p.BankId == bankId);

			if (branchIds != null && branchIds.Any())
			{
				dbBranches = dbBranches.Where(x => branchIds.Contains(x.Id));
			}

			return await dbBranches.OrderBy(p => p.Name)
						.Select(x => new Branch
						{
							Id = x.Id,
							Name = x.Name,
							Code = x.Code,
							Address = x.Address
						})
						.ToListAsync().ConfigureAwait(false);
		}

		public async Task<Branch> AddBranchAsync(Branch branch)
		{
			try
			{
				var dbBranch = new DbBranch
				{
					Id = Guid.NewGuid(),
					Name = branch.Name,
					Code = branch.Code,
					Address = branch.Address,
					BankId = this.userService.BankId.GetValueOrDefault()
				};

				this.dbContext.Branches.Add(dbBranch);
				await this.dbContext.SaveChangesAsync().ConfigureAwait(false);

				return new Branch
				{
					Id = dbBranch.Id,
					Name = dbBranch.Name,
					Code = dbBranch.Code,
					Address = dbBranch.Address
				};
			}
			catch (Exception ex)
			{
				await this.logger.ErrorAsync(ex).ConfigureAwait(false);

				if (ex.GetBaseException() is SqlException sqlEx &&
					sqlEx.Number.In((int)SqlErrors.KeyViolation, (int)SqlErrors.UniqueIndex))
				{
					throw new OpException(OpResult.AlreadyExists,
						string.Format(SFMessages.CannotAddDuplicateBranch, branch.Name));
				}

				throw;
			}
		}

		public async Task<Branch> EditBranchAsync(Branch branch)
		{
			var dbBranch = await this.dbContext.Branches
				.Where(m => m.Id == branch.Id)
				.SingleOrDefaultAsync()
				.ConfigureAwait(false);

			if (dbBranch == null)
			{
				await this.logger.WarnAsync("{0} doesn't exist", branch.Name).ConfigureAwait(false);
				throw new OpException(OpResult.DoNotExist);
			}

			try
			{
				dbBranch.Name = branch.Name;
				dbBranch.Code = branch.Code;
				dbBranch.Address = branch.Address;

				await this.dbContext.SaveChangesAsync().ConfigureAwait(false);

				return new Branch
				{
					Id = dbBranch.Id,
					Name = dbBranch.Name,
					Code = dbBranch.Code,
					Address = dbBranch.Address
				};
			}
			catch (Exception ex)
			{
				await this.logger.ErrorAsync(ex).ConfigureAwait(false);

				if (ex.GetBaseException() is SqlException sqlEx &&
					sqlEx.Number.In((int)SqlErrors.KeyViolation, (int)SqlErrors.UniqueIndex))
				{
					throw new OpException(OpResult.AlreadyExists,
						string.Format(SFMessages.CannotAddDuplicateBranch, branch.Name));
				}

				throw;
			}
		}

		public async Task DeleteBranchAsync(Guid id)
		{
			var dbBranch = await this.dbContext.Branches
				.Where(m => m.Id == id)
				.SingleOrDefaultAsync()
				.ConfigureAwait(false);

			if (dbBranch != null)
			{
				try
				{
					this.dbContext.Branches.Remove(dbBranch);
					await this.dbContext.SaveChangesAsync().ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					await this.logger.ErrorAsync(ex).ConfigureAwait(false);
					throw new OpException(OpResult.DBDeletionDenied, SFMessages.CannotDeleteBranch);
				}
			}
			else
			{
				throw new OpException(OpResult.DoNotExist);
			}
		}

		public async Task AddStaffAsync(AddStaffModel model)
		{
			await this.permitRepository.AddPermitAsync(model.Username,
				model.Role.Equals("Manager") ? SFPermissionCodes.BranchManager : SFPermissionCodes.BranchStaff,
				model.BranchId).ConfigureAwait(false);
		}
	}
}
