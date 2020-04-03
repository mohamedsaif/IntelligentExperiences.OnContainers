using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cam.Device.Web.Repos;
using CoreLib.Abstractions;
using CoreLib.Repos;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cam.Device.Web
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

            // Adding AppSettings section
            var appSettingsSection = 
                Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);
            services.AddSingleton<IIoTHubRepo, IoTHubRepo>();
            //var settings = Configuration.Get<AppSettings>();
            var settings = Configuration.GetSection("AppSettings").Get<AppSettings>();
            var camFrameStorageConnection = settings.StorageConnection;
            var camFrameStorageContainer = settings.StorageContainer;

            services.AddSingleton<IStorageRepository>((s) =>
            {
                return new AzureBlobStorageRepository(camFrameStorageConnection, camFrameStorageContainer);
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
