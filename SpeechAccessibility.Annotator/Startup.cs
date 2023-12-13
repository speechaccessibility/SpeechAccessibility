using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Infrastructure.Data;
using SpeechAccessibility.Annotator.Extensions;

namespace SpeechAccessibility.Annotator
{
    public class Startup
    {
        public Startup(IHostEnvironment env)
        {
            //Configuration = configuration;
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)

                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication("SpeechAccessibilityAuthentication")
                .AddCookie("SpeechAccessibilityAuthentication", options =>
                {
                    options.Cookie.Name = "SpeechAccessibilityCookie";
                    options.LoginPath = "/Auth/SignIn";
                    options.AccessDeniedPath = "/Auth/Index";

                });



            services.AddAuthorization(options =>
            {
                options.AddPolicy("SystemAdmin", policy => policy.RequireClaim(ClaimTypes.Role, "SystemAdmin"));
                options.AddPolicy("AnnotatorAdmin", policy => policy.RequireClaim(ClaimTypes.Role, "TextAnnotatorAdmin", "SLPAnnotatorAdmin", "SystemAdmin"));
                options.AddPolicy("TextAnnotatorAdmin", policy => policy.RequireClaim(ClaimTypes.Role, "TextAnnotatorAdmin", "SystemAdmin"));
                options.AddPolicy("SLPAnnotatorAdmin", policy => policy.RequireClaim(ClaimTypes.Role, "SLPAnnotatorAdmin", "SystemAdmin"));
                options.AddPolicy("TextAnnotator", policy => policy.RequireClaim(ClaimTypes.Role, "TextAnnotator", "TextAnnotatorAdmin", "SystemAdmin"));
                options.AddPolicy("SLPAnnotator", policy => policy.RequireClaim(ClaimTypes.Role, "SLPAnnotator", "SLPAnnotatorAdmin", "SystemAdmin"));
                options.AddPolicy("SLPAnnotatorAndExternalSLPAnnotator", policy => policy.RequireClaim(ClaimTypes.Role, "SLPAnnotator", "SLPAnnotatorAdmin", "ExternalSLPAnnotator", "SystemAdmin"));
                options.AddPolicy("SLPAnnotatorAndTextAnnotatorAdmin", policy => policy.RequireClaim(ClaimTypes.Role, "SLPAnnotator", "SLPAnnotatorAdmin", "TextAnnotatorAdmin", "SystemAdmin"));
                options.AddPolicy("SLPAnnotatorAndTextAnnotatorAdminAndExternalSLPAnnotator", 
                    policy => policy.RequireClaim(ClaimTypes.Role, "SLPAnnotator", "SLPAnnotatorAdmin", "TextAnnotatorAdmin", "ExternalSLPAnnotator", "SystemAdmin"));
                options.AddPolicy("AllAnnotator",
                    policy => policy.RequireClaim(ClaimTypes.Role, "TextAnnotator", "SLPAnnotator", "SystemAdmin", "TextAnnotatorAdmin", "SLPAnnotatorAdmin"));
                options.AddPolicy("ExternalSLPAnnotator", policy => policy.RequireClaim(ClaimTypes.Role, "ExternalSLPAnnotator", "SystemAdmin"));
                options.AddPolicy("AllAnnotatorAndExternalSLPAnnotator", policy => policy.RequireClaim(ClaimTypes.Role, "TextAnnotator", "SLPAnnotator", "TextAnnotatorAdmin", "SLPAnnotatorAdmin", "ExternalSLPAnnotator", "SystemAdmin"));
                options.AddPolicy("Compensator", policy => policy.RequireClaim(ClaimTypes.Role, "Compensator", "SystemAdmin"));
                options.AddPolicy("CompensatorAndAnnotatorAdmin", policy => policy.RequireClaim(ClaimTypes.Role, "Compensator", "TextAnnotatorAdmin", "SLPAnnotatorAdmin", "SystemAdmin"));


            });

            services.AddMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60);
                options.Cookie.HttpOnly = true;
            });


            services.AddDbContext<SpeechAccessibilityDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("SpeechAccessibilityAnnotator"),
                    sqlServerOptionsAction: sqlOptions => { sqlOptions.EnableRetryOnFailure(); });
            });

            services.AddDbContext<SpeechAccessibilityContributorDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("SpeechAccessibilityContributor"),
                    sqlServerOptionsAction: sqlOptions => { sqlOptions.EnableRetryOnFailure(); });
            });


            //services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IBlockRepository, BlockRepository>();
            //services.AddScoped<IBlockOfDigitalCommandRepository, BlockOfDigitalCommandRepository>();
            services.AddScoped<IBlockMasterRepository, BlockMasterRepository>();
            services.AddScoped<IBlockOfPromptsRepository, BlockOfPromptsRepository>();
            //services.AddScoped<IBlockOfDigitalCommandPromptsRepository, BlockOfDigitalCommandPromptsRepository>();
            services.AddScoped<IBlockMasterOfPromptsRepository, BlockMasterOfPromptsRepository>();
            services.AddScoped<IApprovedDeniedContributorRepository, ApprovedDeniedContributorRepository>();
            services.AddScoped<IContributorRepository, ContributorRepository>();
            services.AddScoped<IContributorViewRepository, ContributorViewRepository>();
            services.AddScoped<IContributorStatusRepository, ContributorStatusRepository>();
            services.AddScoped<IContributorSubStatusRepository, ContributorSubStatusRepository>();
            services.AddScoped<IContributorAssignedAnnotatorRepository, ContributorAssignedAnnotatorRepository>();
            services.AddScoped<IContributorAssignedBlockRepository, ContributorAssignedBlockRepository>();
            services.AddScoped<IContributorCompensationRepository, ContributorCompensationRepository>();
            services.AddScoped<IContributorCompensationHistoryRepository, ContributorCompensationHistoryRepository>();
            services.AddScoped<IContributorCompensationViewRepository, ContributorCompensationViewRepository>();
            services.AddScoped<IContributorFollowUpRepository, ContributorFollowUpRepository>();
            services.AddScoped<IEtiologyRepository, EtiologyRepository>();
            services.AddScoped<IEtiologyViewRepository, EtiologyViewRepository>();
            services.AddScoped<IPromptRepository, PromptRepository>();
            services.AddScoped<IPromptEtiologyRepository, PromptEtiologyRepository>();
            services.AddScoped<IEmailLoggingRepository, EmailLoggingRepository>();
            services.AddScoped<IRecordingRepository, RecordingRepository>();
            services.AddScoped<IRecordingRatingRepository, RecordingRatingRepository>();
            services.AddScoped<IRecordingStatusRepository, RecordingStatusRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<ISubRoleRepository, SubRoleRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserSubRoleRepository, UserSubRoleRepository>();
            services.AddScoped<IDimensionRepository, DimensionRepository>();
            services.AddScoped<IDimensionCategoryRepository, DimensionCategoryRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ISubCategoryRepository, SubCategoryRepository>();
            services.AddScoped<IViewSpeechFilesRepository, ViewSpeechFilesRepository>();
            services.AddScoped<DeleteFileAttribute>();

            services.AddControllersWithViews()
                .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);
            services.AddControllersWithViews()
                .AddNewtonsoftJson(opts => opts.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            services.AddControllers().AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            );

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
            app.UseSession();
            app.UseRouting();
            app.UseAuthentication();
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
