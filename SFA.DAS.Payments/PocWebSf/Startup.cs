using Hangfire;
using Hangfire.Console;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PocWebSf
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
            services.AddMvc();
            services.AddHangfire(x =>
            {
                x.UseSqlServerStorage(SFA.DAS.Payments.Domain.Config.Configuration.SqlServerConnectionString);
                x.UseConsole(new ConsoleOptions
                {
                    PollInterval = 300
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            GlobalConfiguration.Configuration.UseSqlServerStorage(SFA.DAS.Payments.Domain.Config.Configuration.SqlServerConnectionString);

            app.UseHangfireDashboard(options: new DashboardOptions() {Authorization = new[] {new HangFireAuthorizationFilter()}});
            app.UseHangfireServer(new BackgroundJobServerOptions {ServerName = "SingleNode", WorkerCount = 1});
        }
    }
}