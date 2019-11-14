using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DutchTreat.Data;
using DutchTreat.Data.Entities;
using DutchTreat.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DutchTreat
{
  public class Startup
  {
    private readonly IConfiguration _config;

    public Startup(IConfiguration config)
    {
      _config = config;
    }
    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddIdentity<StoreUser, IdentityRole>(cfg =>
      {
        cfg.User.RequireUniqueEmail = true;
      }).AddEntityFrameworkStores<DutchContext>();

      services.AddAuthentication()
        .AddCookie()
        .AddJwtBearer(cfg =>
        {
          cfg.TokenValidationParameters = new TokenValidationParameters()
          {
            ValidIssuer = _config["Tokens:Issuer"],
            ValidAudience = _config["Tokens:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"]))
          };
        });

      services.AddDbContext<DutchContext>(cfg =>
      {
        cfg.UseSqlServer(_config.GetConnectionString("DutchConnectionString"));
      });
      services.AddTransient<IMailService, NullMailService>();

      services.AddTransient<DutchSeeder>();

      services.AddAutoMapper(Assembly.GetExecutingAssembly());

      services.AddScoped<IDutchRepository, DutchRepository>();

      services.AddMvc()
          .AddNewtonsoftJson(opt => opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);
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
        // Add Error Page
        app.UseExceptionHandler("/Error");
      }

      app.UseStaticFiles();
      app.UseNodeModules();

      app.UseRouting();

      app.UseAuthentication();
      app.UseAuthorization();


      app.UseEndpoints(cfg =>
      {
        cfg.MapControllerRoute("Fallback",
                  "{controller}/{action}/{id?}",
                  new { controller = "App", action = "Index" });

        cfg.MapRazorPages();
      });

      //app.Run(async context =>
      //    {
      //        await context.Response.WriteAsync("<html><body><h1>Hello World!</h1></body></html>");
      //    });
    }
  }
}
