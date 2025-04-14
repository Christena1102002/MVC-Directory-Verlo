using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using mvc.Hubs;
using mvc.Models;
using mvc.Models.Authorize;
using mvc.RepoInterfaces;
using mvc.Repositories;


namespace mvc
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(
               options =>
               {
                   options.Password.RequireDigit = false;



                   options.Password.RequireLowercase = false;
                   options.Password.RequireUppercase = false;
                   options.Password.RequireNonAlphanumeric = false;
                   options.User.RequireUniqueEmail = true;



               }

               ).AddEntityFrameworkStores<ProjectContext>();


            //register services
            builder.Services.AddDbContext<ProjectContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });
            builder.Services.AddSignalR();

            //register custom services 
            builder.Services.AddScoped<IPackageRepository, PackageRepository>();
            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
            builder.Services.AddScoped<IReviewRepository,ReviewRepository>();
            builder.Services.AddScoped<IBussinessRepository, BussinessRepository>();
            builder.Services.AddScoped<IBusinessFeaturesRepoisitory, BusinessFeaturesRepoisitory>();
            builder.Services.AddScoped<IOpeningHourRepository, OpeningHourRepository>();


            builder.Services.AddScoped<ICategoryReposiotry, CategoryRepository>();
           // builder.Services.AddScoped<IcategoryFeaturesRepository, CategoryFeatures>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStatusCodePagesWithReExecute("/Error/{0}");
            app.UseHsts();

            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapHub<Chathub>("/chathub");
            app.MapHub<ReviewHub>("/reviewhub");
            app.MapControllerRoute(
                "pay", "Payment/Success/{id}/{businessId}/{packageid}/{subscription}",
                new { controller = "Payment", action = "Success" }
                );
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Configure error handling
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
                app.UseHsts();
            }

            app.Run();
        }
    }
}
