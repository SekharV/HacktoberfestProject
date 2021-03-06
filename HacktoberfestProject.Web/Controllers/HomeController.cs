﻿using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using HacktoberfestProject.Web.Models;
using HacktoberfestProject.Web.Models.DTOs;
using HacktoberfestProject.Web.Services;
using HacktoberfestProject.Web.ViewModels;

namespace HacktoberfestProject.Web.Controllers
{
	public class HomeController : Controller
	{
		private readonly IHttpContextAccessor _contextAccessor;
		private readonly ITableService _tableService;
		private const string GitHubUsernameClaimType = "urn:github:login";

		public HomeController(IHttpContextAccessor contextAccessor, ITableService tableService)
		{
			_contextAccessor = contextAccessor;
			_tableService = tableService;
		}

		public async Task<IActionResult> Index()
		{
			User user = null;
			if (_contextAccessor.HttpContext.User.Identity.IsAuthenticated)
			{
				var username = _contextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == GitHubUsernameClaimType)?.Value;
				var response = await _tableService.GetUserAsync(username);

				if (response.ServiceResponseStatus == Models.Enums.ServiceResponseStatus.Ok)
				{
					user = response.Content;
				}
			}
			return View(user);
		}

		[Authorize]
		[HttpGet]
		public IActionResult Add()
		{
			return View();
		}

		[Authorize]
		[HttpPost]
		public async Task<IActionResult> Add(AddPrViewModel addPrViewModel)
		{
			if (!ModelState.IsValid)
			{
				return View("Add", addPrViewModel);
			}

			addPrViewModel.UserName = _contextAccessor.HttpContext.User
				.Claims.FirstOrDefault(c => c.Type == GitHubUsernameClaimType)?.Value;

			await _tableService.AddPrAsync(addPrViewModel.UserName, addPrViewModel.Owner,
				addPrViewModel.Repository, new PullRequest(addPrViewModel.PrNumber, addPrViewModel.PrUrl));

			return RedirectToAction("Index");
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
