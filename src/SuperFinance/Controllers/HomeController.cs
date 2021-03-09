using ASKSource.Controllers;
using ASPSecurityKit.Net;
using Microsoft.AspNetCore.Mvc;

namespace SuperFinance.Controllers
{
	[AllowAnonymous]
	public class HomeController : SiteControllerBase
	{
		public ActionResult Index()
		{
			return View();
		}

		public ActionResult Contact()
		{
			return View();
		}

		public ActionResult SignUp()
		{
			return View();
		}
	}
}
