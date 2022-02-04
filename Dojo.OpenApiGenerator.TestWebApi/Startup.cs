using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dojo.OpenApiGenerator.TestWebApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NSwag.Generation.AspNetCore;

namespace Dojo.OpenApiGenerator.TestWebApi
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
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });
            services.AddSingleton<IHelloWorldService, HelloWorldService>();

            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = Microsoft.AspNetCore.Mvc.ApiVersion.Default;
                options.ApiVersionReader = new HeaderApiVersionReader("version");
                options.ReportApiVersions = true;
            });

            services.AddVersionedApiExplorer();

            services.AddOpenApiDocument(document => ConfigureSingleVersion(document, "1.0"));
            services.AddOpenApiDocument(document => ConfigureSingleVersion(document, "2022-01-03"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var serverUrl = string.Empty;

            app.UseOpenApi(options =>
            {
                options.Path = "/api/swagger/{documentName}/swagger.json";
            });

            app.UseSwaggerUi3(x =>
            {
                x.Path = "/api/swagger";
                x.DocumentPath = "/api/swagger/{documentName}/swagger.json";

                if (!env.IsDevelopment())
                {
                    x.TransformToExternalPath = (url, _) =>
                    {
                        x.ServerUrl = serverUrl;
                        return url.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                            ? x.ServerUrl + url
                            : url;
                    };
                }
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static void ConfigureSingleVersion(
            AspNetCoreOpenApiDocumentGeneratorSettings configure,
            string version)
        {
            configure.Title = "Test WebApi Service";
            configure.DocumentName = version;
            configure.ApiGroupNames = new[] { version };

            configure.PostProcess = document =>
            {
                document.Info.Version = version;
                document.Info.Title = "API";
            };
        }
    }
}
