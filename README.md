<H1 align="center">Api de Autenticação e Autorização</H1>
<p align="center">🚀 Projeto de criação de uma api de sistema de Autenticação e Autorização para referências futuras</p>



## Recursos Utilizados

* .NET 5.0
* Authentication – versão 2.2.0
* JwtBearer – versão 5.0.12

## Adicionando pacotes ao projeto

Para instalação de pacotes com versões antigas utilize dotnet no Windows powershell

```
dotnet add package Microsoft.AspNetCore.Authentication --version 2.2.0
```
```
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 5.0.12
```

## Criação de repositório

 ```
 Repositories/UserRepository.cs
 ```
 Representará um banco de dados com usuários e suas roles
```
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiAuth.Models;

namespace ApiAuth.Repositories
{
	public static class UserRepository
	{
		public static User Get(string username, string password)
		{

			var users = new List<User>();
			users.Add(new User { Id = 1, Username = "Lucas", Password = "123", Role = "manager" });
			users.Add(new User { Id = 2, Username = "Martins", Password = "321", Role = "employee" });
			return users.Where(x => x.Username.ToLower() == username.ToLower() && x.Password == x.Password).FirstOrDefault();

		}
	}
}

```

## Criação Chave privada

```
Settings.cs
```
Contém um chave privada para criação de tokens onde o servidor utiliza para descriptografar uma parte dos tokens recebidos 

```
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiAuth
{
	public static class Settings
	{
		public static string Secret = "fedaf7d8863b48e197b9287d492b708e";
	}
}
```

## Criação do Service

```
Services/TokenService.cs
```

Este service será responsável para criação de tokens, gerando um token JWT utilziando ASP.NET 5

```
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiAuth.Models;
using Microsoft.IdentityModel.Tokens;

namespace ApiAuth.Services
{
	public class TokenService
	{
		public static string GenerateToken(User user)
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			var key = Encoding.ASCII.GetBytes(Settings.Secret);
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(new Claim[]
				{
					new Claim(ClaimTypes.Name, user.Username.ToString()), //User.Identity.name
					new Claim(ClaimTypes.Role, user.Role.ToString()) //User.isInRole()
				}),
				Expires = DateTime.UtcNow.AddHours(2),
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
			};
			var token = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(token);
		}
	}
}
```

## Adicionando autenticação e autorização

```
Startup.cs
```

Informando a aplicação que iremos trabalhar com autenticação e autorização e definindo quais perfis tem acesso a determinadas ações de controladores

### Informando a aplicação que utilizaremos autenticação

<img src="https://cdn.discordapp.com/attachments/1046824853015113789/1202803916786634792/image.png?ex=65cec970&is=65bc5470&hm=d14f7d6a28a16931acc089dfa31f624bcf40f5be496c2aa755421c5a53af18be&" alt="">


### Configurando autenticação

<img src="https://cdn.discordapp.com/attachments/1046824853015113789/1202803944452263987/image.png?ex=65cec977&is=65bc5477&hm=7b9317657596061b3861458680f6624548b55d77f1de69dc4b1001b986eedc1e&" alt="">


```
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.IdentityModel.Tokens;



namespace ApiAuth
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllersWithViews();


			var key = Encoding.ASCII.GetBytes(Settings.Secret);

			services.AddAuthentication(x =>
			{
				x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			}).AddJwtBearer(x =>
			{
				x.RequireHttpsMetadata = false;
				x.SaveToken = true;
				x.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(key),
					ValidateIssuer = false,
					ValidateAudience = false
				};
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}
			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthentication();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}

```

## Autenticando

```
    Controller/LoginController.cs
```
Explorando toda autenticação e autorização no Controller

```
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ApiAuth.Models;
using ApiAuth.Repositories;
using ApiAuth.Services;
using System.Net;

namespace ApiAuth.Controllers
{

	[ApiController]
	[Route(template: "v1")]
	public class LoginController
	{
		[HttpPost]
		[Route("login")]
		public async Task<ActionResult<dynamic>> Authenticate([FromBody] User model)
		{
			// Recupera o usuário
			var user = UserRepository.Get(model.Username, model.Password);

			// Verifica se o usuário existe
			if (user == null)
				return HttpStatusCode.BadRequest;

			// Gera o Token
			var token = TokenService.GenerateToken(user);

			// Oculta a senha
			user.Password = "";

			// Retorna os dados
			return new
			{
				user = user,
				token = token
			};
		}


	}

}

```

## Controlador de Rotas

```
    Controller/HomeController.cs
```

Criação de 4 métodos explorando as autorizações e rotas

```
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;


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

```

## Tipos de acesso

### Anônimo

<img src="https://cdn.discordapp.com/attachments/1046824853015113789/1202805751026810921/image.png?ex=65cecb26&is=65bc5626&hm=694dc1d16ffa7feaad9cd30f909992256bdea54b4e5e89f3b63b3706f0019085&" alt="">

### Não autenticado 

<img src="https://cdn.discordapp.com/attachments/1046824853015113789/1202805800377262080/image.png?ex=65cecb31&is=65bc5631&hm=80ca9649b04a0fe48aa3afed5efdd1f92b29101735817ab871a6de27c77c13c9&" alt="">

### Autenticação

<img src="https://cdn.discordapp.com/attachments/1046824853015113789/1202805845063241738/image.png?ex=65cecb3c&is=65bc563c&hm=27daf47232d38b973b43b5c91ef16fd0b16e8a1f0a5bc369b6cefd0d2af1d89e&" alt="">

### Autenticado

<img src="https://cdn.discordapp.com/attachments/1046824853015113789/1202805871533625374/image.png?ex=65cecb42&is=65bc5642&hm=ff5ad08e07e8a6bb8a686628cac30fc04249294a25b3f0d6182c087f18639781&" alt="">

### Autenticado porém sem acesso a Employee

<img src="https://cdn.discordapp.com/attachments/1046824853015113789/1202805897827590155/image.png?ex=65cecb49&is=65bc5649&hm=f15c909e86254abd206b7832f668490fb3d3b89f0077d2fe3598119cdad86272&" alt="">

### Autenticado e com acesso a Manager

<img src="https://cdn.discordapp.com/attachments/1046824853015113789/1202807725533429810/image.png?ex=65ceccfc&is=65bc57fc&hm=97b5158c522e582f6be386852528b653657a794822374e1ece03114f88e07add&" alt="">

## Estrutura do Projeto

<img src="https://cdn.discordapp.com/attachments/1046824853015113789/1202806448388833350/image.png?ex=65cecbcc&is=65bc56cc&hm=fd00599f8ba29f1f5d6156221239aa42ee5136242e02eaa7cacb3547f9e2cd6c&" alt="">


