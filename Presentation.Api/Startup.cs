using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Auth0.AuthenticationApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Infrastructure.Data.Context;
using Infrastructure.IoC;
using MediatR;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Api.API.Helpers;
using Api.Authentication;
using Application.Helpers;
using Domain.Interfaces;
using Domain.Multitenant;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;

namespace Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", false)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
            IsDevelopment = env.IsDevelopment();
        }

        public static IConfiguration Configuration { get; set; }
        private bool IsDevelopment { get; set; } = false;


        // This method gets called by the runtime. Use this method to add services to RacePlanner.
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("LocusBasedDbConnection");
            var origins = Configuration["Auth0:AllowWithOrigins"].Split(";");


            // CORS config start
            services.AddCors(options =>
            {
                options.AddPolicy(name: "SiteCorsPolicy",
                    policy =>
                    {
                        policy
                            .WithOrigins(
                            origins
                            //"http://127.0.0.1","https://127.0.0.1",
                            //"http://127.0.0.1:4000","https://127.0.0.1:4001",
                            //"http://localhost","https://localhost",
                            //"http://localhost:4000","https://localhost:4001",
                            //"http://app.locusbase.no","https://app.locusbase.no",
                            //"http://app.locusbase.no:4000","https://app.locusbase.no:4001",
                            //"http://locusbase.net:4000","https://locusbase.net:4001"
                            ) /* list of environments that will access this api */
                            //.AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                            //.AllowCredentials();
                    });
            });
            // CORS config end

            // Configure forwarded headers
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                //options.KnownProxies.Add(IPAddress.Parse("127.0.10.1"));
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            // Authentication config start
            string domain = $"https://{Configuration["Auth0:Domain"]}/";
            string authority = $"https://{Configuration["Auth0:Domain"]}/";
            string audience = Configuration["Auth0:Audience"];
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.Audience = audience;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = ClaimTypes.NameIdentifier
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        // Grab the raw value of the token, and store it as a claim so we can retrieve it again later in the request pipeline
                        // Have a look at the ValuesController.UserInformation() method to see how to retrieve it and use it to retrieve the
                        // user's information from the /userinfo endpoint
                        if (context.SecurityToken is JwtSecurityToken token)
                        {
                            if (context.Principal.Identity is ClaimsIdentity identity)
                            {
                                identity.AddClaim(new Claim("access_token", token.RawData));
                            }
                        }

                        return Task.CompletedTask;
                    }
                };
            });
            // Authentication config end

            // Policy config start
            services.AddAuthorization(options =>
            {
                foreach (var scope in Scopes.scopes)
                {
                    options.AddPolicy(scope, policy =>
                        policy.Requirements.Add(new HasScopeRequirement(scope, authority)));
                }
            });
            services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();
            services.AddSingleton(x =>
                new AuthenticationApiClient(new Uri($"https://{Configuration["Auth0:Domain"]}/")));
            // Policy config end

            services.AddAutoMapper(typeof(Startup));

            bool.TryParse(Configuration["useMySql"], out bool useMySql);
            if (useMySql)
            {
                services.AddDbContext<LocusBaseDbContext>(options =>
                {
                    options.UseMySql(
                        connectionString, ServerVersion.AutoDetect(connectionString),
                        x => x.UseNetTopologySuite());
                    if (IsDevelopment)
                        options.EnableSensitiveDataLogging();
                }, ServiceLifetime.Transient);
            }
            else
            {
                services.AddDbContext<LocusBaseDbContext>(options =>
                {
                    options.UseSqlServer(
                        connectionString,
                        x => x.UseNetTopologySuite());
                    if (IsDevelopment)
                        options.EnableSensitiveDataLogging();
                }, ServiceLifetime.Transient);
            }

            services.AddMultiTenancy()
                .WithResolutionStrategy<DomainResolutionStrategy>()
                .WithStore<TenantStore>();

            // Includes support for Razor Pages and controllers.
            services.AddMvc(options => options.EnableEndpointRouting = false);

            services.AddControllers(options =>
                    options.Filters.Add(new HttpResponseExceptionFilter())
                )
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver
                    { NamingStrategy = new SnakeCaseNamingStrategy() };
                    //options.SerializerSettings.Culture = new CultureInfo("nb_no");
                    options.SerializerSettings.DateTimeZoneHandling =
                        DateTimeZoneHandling
                            .Utc; // this should be set if you always expect UTC dates in method bodies, if not, you can use RoundTrip instead.
                    options.SerializerSettings.Formatting = Formatting.None;
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                });

            services.AddSwaggerGen(setup =>
            {
                setup.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "RacePlanner API",
                    Description = "Race Backend Web API",
                });

                // Include 'SecurityScheme' to use JWT Authentication
                var jwtSecurityScheme = new OpenApiSecurityScheme
                {
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Name = "JWT Authentication",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",

                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };

                setup.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

                setup.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {jwtSecurityScheme, Array.Empty<string>()}
                });
            });

            services.AddMediatR(typeof(Startup));

            var timeZone = Configuration["TimeZone"];
            services.AddSingleton(timeZone);
            services.AddSingleton<AttachmentCreatedDateResolver>();

            services.AddScoped<IDbInitializer, DbInitializer>();

            RegisterServices(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                // app.UseDeveloperExceptionPage();
                app.UseExceptionHandler("/error-local-development");
            }
            else
            {
                app.UseExceptionHandler("/Error");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();

                // Access to XMLHttpRequest at 'http://localhost:8000/api/signs?
                //   within_radius={latitude:59.050476,longitude:10.023207,radius:1250}&page=1'
                // from origin 'http://localhost:8080' has been blocked by CORS policy:
                // Response to preflight request doesn't pass access control check:
                // Redirect is not allowed for a preflight request.
                app.UseHttpsRedirection();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Race Backend Web Api v1"));

            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors("SiteCorsPolicy");
            //app.UseCors(options => options.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

            //app.UseForwardedHeaders(new ForwardedHeadersOptions
            //{
            //    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            //});
            app.UseForwardedHeaders();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMultiTenancy();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                //endpoints.MapRazorPages();
            });

            app.UseMvc();

            var scopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                var dbInitializer = scope.ServiceProvider.GetService<IDbInitializer>();
                dbInitializer.Initialize();
                dbInitializer.SeedData();
            }
        }

        private static void RegisterServices(IServiceCollection services)
        {
            DependencySignGroup.RegisterServices(services);
        }
    }
}