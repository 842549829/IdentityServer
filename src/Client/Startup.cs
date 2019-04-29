using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace Client
{
    public class Startup
    {
        /// <summary>
        /// 启动项构造
        /// </summary>
        /// <param name="configuration">配置文件</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// 配置文件
        /// </summary>
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            //加入HttpClient
            services.AddHttpClient();

            var config = Configuration.GetSection("JwtBearerOptions").Get<JwtBearerOptions>();
            services.Configure<JwtBearerOptions>(Configuration.GetSection("JwtBearerOptions"));
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(option =>
            {
                option.RequireHttpsMetadata = false;
                option.Authority = config.Authority;
                option.RequireHttpsMetadata = config.RequireHttpsMetadata;
                option.Audience = config.Audience;
            });

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(option => { option.UseCamelCasing(true); });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //添加日志
            loggerFactory.AddNLog();

            //添加权限验证
            app.UseAuthentication();

            //添加MVC框架
            app.UseMvc();
        }
    }
}
