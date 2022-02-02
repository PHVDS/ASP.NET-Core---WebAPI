using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MinhasTarefasAPI.Database;
using MinhasTarefasAPI.Models;
using MinhasTarefasAPI.Repositories;
using MinhasTarefasAPI.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinhasTarefasAPI
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
			services.Configure<ApiBehaviorOptions>(op => {
				op.SuppressModelStateInvalidFilter = true;
			});

			services.AddDbContext<MinhasTarefasContext>(op => {
				op.UseSqlite("Data Source=Database\\MinasTarefas.db");
			});

			//repositories
			services.AddScoped<IUsuarioRepository, UsuarioRepository>();
			services.AddScoped<ITarefaRepository, TarefaRepository>();

			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
				//Ignore reference looping handling	
				.AddJsonOptions(opt => 
						opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
					);

			//Configurando Identity pra usar como serviço
			services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<MinhasTarefasContext>();
			
			//Redirecionando usuario para tela de login (Erro 401 Usuario n autorizado)
			services.ConfigureApplicationCookie(opt => {
				opt.Events.OnRedirectToLogin = context => {
					context.Response.StatusCode = 401;
					return Task.CompletedTask;
				};
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}
			app.UseStatusCodePages();
			app.UseAuthentication();
			app.UseHttpsRedirection();
			app.UseMvc();
		}
	}
}
