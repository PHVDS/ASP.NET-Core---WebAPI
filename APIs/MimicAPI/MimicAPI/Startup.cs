using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MimicAPI.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MimicAPI.Versao1.Repositories;
using MimicAPI.Versao1.Repositories.Contracts;
using AutoMapper;
using MimicAPI.Helpers;
using Microsoft.AspNetCore.Mvc.Versioning;
using MimicAPI.Helpers.Swagger;
using Microsoft.Extensions.PlatformAbstractions;
using System.IO;

namespace MimicAPI
{
	public class Startup
	{
		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
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

			//configurando banco de dados
			services.AddDbContext<MimicContext>( opt => {
				opt.UseSqlite("Data Source=Database\\Mimic.db");
			});
			services.AddMvc();
			services.AddScoped<IPalavraRepository, PalavraRepository>();
			services.AddApiVersioning(cfg => { 
				cfg.ReportApiVersions = true;
				//cfg.ApiVersionReader = new HeaderApiVersionReader("api-version");
				cfg.AssumeDefaultVersionWhenUnspecified = true;
				cfg.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
			});

			services.AddSwaggerGen(cfg => {
				cfg.ResolveConflictingActions(apiDescription => apiDescription.First());
				cfg.SwaggerDoc("v2.0", new Swashbuckle.AspNetCore.Swagger.Info()
				{
					Title = "MimicAPI = V2.0",
					Version = "v2.0"
				});
				cfg.SwaggerDoc("v1.1", new Swashbuckle.AspNetCore.Swagger.Info()
				{
					Title = "MimicAPI = V1.1",
					Version = "v1.1"
				});
				cfg.SwaggerDoc("v1.0", new Swashbuckle.AspNetCore.Swagger.Info()
				{
					Title = "MimicAPI = V1.0",
					Version = "v1.0"
				});
				var CaminhoProjeto = PlatformServices.Default.Application.ApplicationBasePath;
				//Para ativar o xml no Swagger MimicAPI > Propriedades > Compilar > Saida > Marcao checkbox do xml
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

		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			app.UseStatusCodePages();

			app.UseMvc();

			app.UseSwagger(); // /swagger/v1/swagger.json
			app.UseSwaggerUI(cfg => {
				cfg.SwaggerEndpoint("/swagger/v2.0/swagger.json", "MimicAPI - V 2.0");
				cfg.SwaggerEndpoint("/swagger/v1.1/swagger.json", "MimicAPI - V 1.1");
				cfg.SwaggerEndpoint("/swagger/v1.0/swagger.json", "MimicAPI - V 1.0");
				cfg.RoutePrefix = String.Empty;
			});
		}
	}
}
