using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Database;
using Preprocess;
using System;
using System.Threading.Tasks;
namespace Server
{
    public class Startup
    {

        IDBOperator dbOperator = new DBOperator(VectorType.TF);
        IPreprocessable preprocessor = new TFPreprocessor();
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Task.Run(async () => 
            {
                if (await dbOperator.CheckExists())
                {
                    await dbOperator.CreateCollection();
                }
            });

            services.AddSingleton<IDBOperator>(dbOperator);
            services.AddSingleton<IPreprocessable>(preprocessor);
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
