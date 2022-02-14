using AutoMapper;
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
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TalkToApi.Database;
using TalkToApi.Helpers;
using TalkToApi.Helpers.Constants;
using TalkToApi.V1.Helpers.Swagger;
using TalkToApi.V1.Models;
using TalkToApi.V1.Repositories;
using TalkToApi.V1.Repositories.Contracts;

namespace TalkToApi
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
			#region AutoMapper-Config
			var config = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile(new DTOMapperProfile());
			});
			IMapper mapper = config.CreateMapper();
			services.AddSingleton(mapper);
			#endregion

			#region Repositories
			/* Repositories */
			services.AddScoped<IMensagemRepository, MensagemRespository>();
			services.AddScoped<IUsuarioRepository, UsuarioRepository>();
			services.AddScoped<ITokenRepository, TokenRepository>();
			#endregion

			services.Configure<ApiBehaviorOptions>(op => {
				op.SuppressModelStateInvalidFilter = true;
			});

			services.AddDbContext<TalkToApiContext>(cfg => {
				cfg.UseSqlite("Data Source=Database\\TalkTo.db");
			});

			//Habilitando o CORS
			services.AddCors(cfg => {
				cfg.AddDefaultPolicy(policy => {
					policy
						.WithOrigins("https://localhost:44339", "http://localhost:44339")
						.AllowAnyMethod()
						//Habilitar CORS para tds os subdominios.
						.SetIsOriginAllowedToAllowWildcardSubdomains()
						.AllowAnyHeader();
				});

				//Habilitar todos os sites, com restrição.
				cfg.AddPolicy("AnyOrigin", policy => {
					policy.AllowAnyOrigin()
					.WithMethods("GET")
					.AllowAnyHeader();
				});
			});

			services.AddMvc(cfg => {
				//Formatação para enviar e receber XML
				cfg.ReturnHttpNotAcceptable = true;
				cfg.InputFormatters.Add(new XmlSerializerInputFormatter(cfg));
				cfg.OutputFormatters.Add(new XmlSerializerOutputFormatter());

				var jsonOutputFormatter = cfg.OutputFormatters.OfType<JsonOutputFormatter>().FirstOrDefault();

				if (jsonOutputFormatter != null)
				{
					jsonOutputFormatter.SupportedMediaTypes.Add(CustomMediaType.Hateoas);
				}

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
				cfg.AddSecurityDefinition("Bearer", new ApiKeyScheme()
				{
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
					Title = "TalkTo API = V1.0",
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
				.AddEntityFrameworkStores<TalkToApiContext>()
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
			
			//Desabilite qd for usar Atributos EnableCors/DisableCors
			//app.UseCors("AnyOrigin");
			
			app.UseMvc();

			app.UseSwagger(); // /swagger/v1/swagger.json
			app.UseSwaggerUI(cfg => {
				cfg.SwaggerEndpoint("/swagger/v1.0/swagger.json", "TalkTo Api - V 1.0");
				cfg.RoutePrefix = String.Empty;
			});

		}
	}
}
