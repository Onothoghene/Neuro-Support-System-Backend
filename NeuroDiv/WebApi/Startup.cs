using Application;
using Application.Interfaces;
using Asp.Versioning.ApiExplorer;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Infrastructure.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Configuration;
using System.Threading.Tasks;
using WebApi.Extensions;
using WebApi.Services;

namespace WebApi
{
    public class Startup
    {
        public IConfiguration _config { get; }
        public Startup(IConfiguration configuration)
        {
            _config = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
            {
                builder.WithOrigins("*").AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));
            services.AddApplicationLayer(_config);
            services.AddIdentityInfrastructure(_config);
            services.AddPersistenceInfrastructure(_config);
            services.AddSharedInfrastructure(_config);
            services.AddIntegrations(_config);
            services.AddSwaggerExtension();
            services.AddControllers();
            services.AddApiVersioningExtension();
            services.AddURIExtension();
            services.AddHealthChecks();
            services.AddBackgroundJobs();
            services.AddScoped<IAuthenticatedUserService, AuthenticatedUserService>();
            services.AddSingleton(Log.Logger);

            #region LARGE FILES UPLOAD
            //if using IIS
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
                options.MaxRequestBodySize = 1000000000;
            });
            //If using Kestrel
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
                options.Limits.MaxRequestBodySize = 1000000000;
            });
            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = 1000000000;
                x.MultipartBodyLengthLimit = 1000000000;
                x.MultipartHeadersLengthLimit = 1000000000;
            });

            //ALSO NOTE: copy and paste this code to the web.config file after deployment for large file uploads
            //<system.webServer>
            //<security>
            //  <requestFiltering>
            //    <requestLimits maxAllowedContentLength = "2147483648" />
            //  </requestFiltering>
            //</security >
            //</system.webServer>
            #endregion

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseSwaggerExtension(provider);

            app.UseCors("MyPolicy");
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseErrorHandlingMiddleware();
            app.UseHealthChecks();
            app.UseHangfireDashboard();




            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }


    }
}
