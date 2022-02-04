using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.IdentityModel.Tokens;
using MinhasTarefasAPI.Database;
using MinhasTarefasAPI.V1.Helpers.Swagger;
using MinhasTarefasAPI.V1.Models;
using MinhasTarefasAPI.V1.Repositories;
using MinhasTarefasAPI.V1.Repositories.Contracts;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
			services.AddScoped<ITokenRepository, TokenRepository>();

			services.AddMvc(config => {
				//Formatação para enviar e receber XML
				config.ReturnHttpNotAcceptable = true;
				config.InputFormatters.Add(new XmlSerializerInputFormatter(config));
				config.OutputFormatters.Add(new XmlSerializerOutputFormatter());
			})
			.SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
				//Ignore reference looping handling	
			.AddJsonOptions(opt => 
					opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
			);

			//Versionamneto e Swagger
			services.AddApiVersioning(cfg => {
				cfg.ReportApiVersions = true;
				//cfg.ApiVersionReader = new HeaderApiVersionReader("api-version");
				cfg.AssumeDefaultVersionWhenUnspecified = true;
				cfg.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
			});

			services.AddSwaggerGen(cfg => {
				cfg.AddSecurityDefinition("Bearer", new ApiKeyScheme(){
					In = "header",
					Type = "apiKey",
					Description = "Add o JSON Web Token(JWT) para autenticar.",
					Name = "Authorization"
				});

				var security = new Dictionary<string, IEnumerable<string>>(){
					{ "Bearer", new string[] { } }
				};
				cfg.AddSecurityRequirement(security);

				cfg.ResolveConflictingActions(apiDescription => apiDescription.First());				
				cfg.SwaggerDoc("v1.0", new Swashbuckle.AspNetCore.Swagger.Info()
				{
					Title = "MinhasTarefas API = V1.0",
					Version = "v1.0"
				});
				var CaminhoProjeto = PlatformServices.Default.Application.ApplicationBasePath;
				//Para ativar o xml no Swagger MinhasTarefas > Propriedades > Compilar > Saida > Marcao checkbox do xml
				var NomeProejto = $"{PlatformServices.Default.Application.ApplicationName}.xml";
				var CaminhoArquivoXMLComentario = Path.Combine(CaminhoProjeto, NomeProejto);

				cfg.IncludeXmlComments(CaminhoArquivoXMLComentario);

				cfg.DocInclusionPredicate((docName, apiDesc) =>
				{
					var actionApiVersionModel = apiDesc.ActionDescriptor?.GetApiVersion();
					// significaria que esta ação não é versionada e deve ser incluída em todos os lugares
					if (actionApiVersionModel == null)
					{
						return true;
					}
					if (actionApiVersionModel.DeclaredApiVersions.Any())
					{
						return actionApiVersionModel.DeclaredApiVersions.Any(v => $"v{v.ToString()}" == docName);
					}
					return actionApiVersionModel.ImplementedApiVersions.Any(v => $"v{v.ToString()}" == docName);
				});

				cfg.OperationFilter<ApiVersionOperationFilter>();

			});

			//Configurando Identity pra usar como serviço
			services.AddIdentity<ApplicationUser, IdentityRole>()
				.AddEntityFrameworkStores<MinhasTarefasContext>()
				.AddDefaultTokenProviders();

			services.AddAuthentication(opt => {
				opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
			}).AddJwtBearer(opt => {
				opt.TokenValidationParameters = new TokenValidationParameters()
				{
					ValidateIssuer = false,
					ValidateAudience = false,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("chave-api-jwt-minhas-tarefas"))
				};
			});

			services.AddAuthorization(auth => {
				auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
											 .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
											 .RequireAuthenticatedUser()
											 .Build()
				);
			});

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

			app.UseSwagger(); // /swagger/v1/swagger.json
			app.UseSwaggerUI(cfg => {
				cfg.SwaggerEndpoint("/swagger/v1.0/swagger.json", "MinhasTarefasAPI - V 1.0");
				cfg.RoutePrefix = String.Empty;
			});
		}
	}
}
