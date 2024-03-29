﻿using ApiAuth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ApiAuth.Controllers
{
	[ApiController]
	public class HomeController : ControllerBase
	{
		[HttpGet]
		[Route("anonymous")]
		[AllowAnonymous]
		public string Anonymous() => "Anônimo";

		[HttpGet]
		[Route("authenticated")]
		[Authorize]
		public string Authenticated() => String.Format("Autenticado - {0}", User.Identity.Name);

		[HttpGet]
		[Route("employee")]
		[Authorize(Roles = "employee")]
		public string Employee() => "Funcionário";

		[HttpGet]
		[Route("manager")]
		[Authorize(Roles = "manager")]
		public string Manager() => "Gerente";
	}
}
