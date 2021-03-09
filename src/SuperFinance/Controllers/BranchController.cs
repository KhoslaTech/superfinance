using ASPSecurityKit;
using ASPSecurityKit.Net;
using Microsoft.AspNetCore.Mvc;
using SuperFinance.Managers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ASKSource;
using ASKSource.Controllers;
using ASKSource.DataModels;
using ASKSource.Models;
using ASPSecurityKit.Authorization;
using SuperFinance.Models;
using SuperFinance.ViewModels;

namespace SuperFinance.Controllers
{
	[AuthPermission("ManageBranch")]
	public class BranchController : ServiceControllerBase
	{
		private readonly IBranchManager branchManager;

		public BranchController(IUserService<Guid, Guid, DbUser> userService, IAppContext appContext,
			INetSecuritySettings securitySettings, ISecurityUtility securityUtility, ILogger logger,
			IBranchManager branchManager) :
			base(userService, appContext, securitySettings, securityUtility, logger)
		{
			this.branchManager = branchManager;
		}

		[PossessesPermissionCode]
		public ActionResult Index()
		{
			return View();
		}

		[HttpPost]
		[PossessesPermissionCode]
		public async Task<ActionResult> List(int jtStartIndex, int jtPageSize)
		{
			return await SecureJsonAction(async () =>
			{
				var branches = await this.branchManager.GetBranchesAsync(jtStartIndex, jtPageSize);
				return Json(ApiResponse.List(branches.Records, branches.TotalCount));
			});
		}

		[HttpPost, ValidateAntiForgeryToken]
		[PossessesPermissionCode]
		public async Task<ActionResult> Add(Branch model)
		{
			return await SecureJsonAction(async () =>
			{
				if (ModelState.IsValid)
				{
					return Json(ApiResponse.Single(await this.branchManager.AddBranchAsync(model)));
				}

				throw new OpException(OpResult.InvalidInput);
			});
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> Edit(Branch model)
		{
			return await SecureJsonAction(async () =>
			{
				if (ModelState.IsValid)
				{
					return Json(ApiResponse.Single(await this.branchManager.EditBranchAsync(model)));
				}

				throw new OpException(OpResult.InvalidInput);
			});
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> Delete([Authorize("BranchId")] Guid id)
		{
			return await SecureJsonAction(async () =>
			{
				await this.branchManager.DeleteBranchAsync(id);
				return Json(ApiResponse.Success());
			});
		}

		public ActionResult AddStaff([Authorize("BranchId")] Guid id)
		{
			var model = new AddStaffModel
			{
				BranchId = id
			};

			return View(PopulateAddStaffModel(model));
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> AddStaff(AddStaffModel model)
		{
			if (ModelState.IsValid)
			{
				try
				{
					await this.branchManager.AddStaffAsync(model);
					return RedirectToAction("Index");
				}
				catch (OpException ex)
				{
					ModelState.AddModelError(string.Empty, ex.Message);
				}
			}

			return View(PopulateAddStaffModel(model));
		}

		private AddStaffModel PopulateAddStaffModel(AddStaffModel model)
		{
			model.Roles = new List<string> { "Manager", "Staff" };
			return model;
		}
	}
}
