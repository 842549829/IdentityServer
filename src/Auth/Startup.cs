using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog.Extensions.Logging;

namespace Auth
{
    public class Startup
    {
        public IHostingEnvironment Environment { get; }

        public IConfiguration Configuration { get; set; }

        public Startup(IHostingEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("Auth");
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            /*
             * 证书 https://www.cnblogs.com/cgzl/p/7780559.html 
             */
            var certAbsolutePath = PathResolver.ResolveCertConfigPath(Configuration["AuthOption:X509Certificate2FileName"], Environment);
            var cert = new X509Certificate2(certAbsolutePath, Configuration["AuthOption:X509Certificate2Password"]);
            services.AddIdentityServer()
                .AddResourceOwnerValidator<ResourceOwnerPasswordValidator>()
                .AddSigningCredential(cert)
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = b =>
                        b.UseMySql(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
                }).AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = b =>
                        b.UseMySql(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
                    options.EnableTokenCleanup = true;
                });
            services.AddLogging(logBuilder =>
            {
                logBuilder.AddNLog();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            InitializeDatabase(app);
            app.UseIdentityServer();
        }

        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                /*
                 *切换到程序包管理器控制器，将默认项目设置为Auth，然后输入以下命令
                 *PersistedGrantDbContext
                 *Add-Migration -Name InitialIdentityServerConfigurationDbMigration -Context PersistedGrantDbContext -OutputDir Migrations/IdentityServer/PersistedGrantDb -Project Auth
                 *ConfigurationDbContext
                 *Add-Migration -Name InitialIdentityServerConfigurationDbMigration -Context ConfigurationDbContext -OutputDir Migrations/IdentityServer/ConfigurationDb -Project Auth
                 *命令将使用ConfigurationDbContext创建一个名为InitialIdentityServerConfigurationDbMigration 的迁移。迁移文件将输出到将默认项目设置为Auth项目的Migrations/IdentityServer/ConfigurationDb文件夹。
                */
                // 必须执行数据库迁移命令才能生成数据库表
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();
                if (!context.Clients.Any())
                {
                    foreach (var client in Config.GetClients())
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in Config.GetIdentityResources())
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    foreach (var resource in Config.GetApiResources())
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
            }
        }
    }
}
