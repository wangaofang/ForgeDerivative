using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ForgeDerivative.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ForgeDerivative
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);    

            // tell ASP.NET Core to use a Memory Cache to store the session data.
            //https://benjii.me/2016/07/using-sessions-and-httpcontext-in-aspnetcore-and-mvc-core/
            services.AddSession();
            services.AddDistributedMemoryCache(); // Adds a default in-memory implementation of IDistributedCache

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            //https://stackoverflow.com/questions/37329354/how-to-use-ihttpcontextaccessor-in-static-class-to-set-cookies
            //this would have been done by the framework any way after this method call;
            //in this case you call the BuildServiceProvider manually to be able to use it
            var serviceProvider = services.BuildServiceProvider();
            var accessor = serviceProvider.GetService<IHttpContextAccessor>();
            Credentials.SetHttpContextAccessor(accessor);
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
                app.UseHsts();
            }

            // IMPORTANT: This session call MUST go before UseMvc()
            app.UseSession();

            app.UseFileServer();
            app.UseStaticFiles();
            app.UseDefaultFiles();
            app.UseStatusCodePages();

            // app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
