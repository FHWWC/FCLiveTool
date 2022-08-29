using FCLiveTool.Models.VideoListModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.WebEncoders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace FCLiveTool
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
            services.AddSession();

            services.AddDbContext<WAAccountContext>(options => options.UseSqlServer(Configuration.GetConnectionString("SQLConnection")));
            /*
                         services.AddControllersWithViews(options =>
                        {
                            options.Filters.Add(typeof(PageLoadMethod));
                        });
             */
            services.Configure<WebEncoderOptions>(options => options.TextEncoderSettings=new TextEncoderSettings(UnicodeRanges.All));
            services.AddMvc();
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

            //app.UseDefaultFiles();
            app.UseStaticFiles();
            /*
                         app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
            Path.Combine(Directory.GetCurrentDirectory(), "TempM3U8")),
                RequestPath = "/tm3"
            });
             */

            app.UseSession();
            app.UseRouting();

            app.UseCors("AllowAnyCorsPolicy");

            app.UseCors(builder =>
            {
                builder.SetIsOriginAllowed(_ => true)
                      .AllowCredentials()
                      .AllowAnyHeader();
            });


            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=TVideo}/{action=VideoPage}");
            });
        }
    }
}
