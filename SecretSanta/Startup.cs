//#define SIMULATE

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SecretSanta.DataAccess;
using SecretSanta.DependencyWrappers;
using SecretSanta.Matching;
using SecretSanta.Users;

namespace SecretSanta {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddMvc();
            services.AddTransient<ICreateSecretMatch, CreateSecretMatch>();
            services.AddTransient<IRandomWrapper, RandomWrapper>();
            services.AddTransient<ISessionManager, SessionManager>();
            services.AddTransient<IPageModelBuilder, PageModelBuilder>();

            //testing
#if SIMULATE
            services.AddTransient<IDataAccessor, DataAccessorSimulated>();

#else
            //prod
            string sqlConnectionString = "User ID=santa;Password=santa;Host=santapostgres;Port=5432;Database=santa;Pooling=true;";
            services.AddDbContext<DomainModelPostgreSqlContext>(
                options => options.UseNpgsql(sqlConnectionString)
            );

            services.AddTransient<IDataAccessor, DataAccessorPostgreSql>();
#endif
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes => {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
