using CustomersApi.DataStore;

using Jaeger;
using Jaeger.Samplers;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using OpenTracing;
using OpenTracing.Contrib.NetCore.CoreFx;
using OpenTracing.Util;

using System;
using System.Reflection;

namespace CustomersApi
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
            services.AddSingleton<ITracer>(serviceProvider =>
            {
                string serviceName = Assembly.GetEntryAssembly().GetName().Name;

                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

                var sampler = new ConstSampler(sample: true);

                var tracer = new Tracer.Builder(serviceName)
                    .WithLoggerFactory(loggerFactory)
                    .WithSampler(sampler)
                    .Build();

                GlobalTracer.Register(tracer);

                return tracer;
            });

            // Prevent endless loops when OpenTracing is tracking HTTP requests to Jaeger.
            services.Configure<HttpHandlerDiagnosticOptions>(options =>
            {
                options.IgnorePatterns.Add(request => new Uri("http://localhost:14268/api/traces").IsBaseOf(request.RequestUri));
            });

            services.AddOpenTracing();

            // Adds an InMemory-Sqlite DB to show EFCore traces.
            services
                .AddEntityFrameworkSqlite()
                .AddDbContext<CustomerDbContext>(options =>
                {
                    var connectionStringBuilder = new SqliteConnectionStringBuilder
                    {
                        DataSource = ":memory:",
                        Mode = SqliteOpenMode.Memory,
                        Cache = SqliteCacheMode.Shared
                    };
                    var connection = new SqliteConnection(connectionStringBuilder.ConnectionString);

                    // Hack: EFCore resets the DB for every connection so we keep the connection open.
                    // This is obviously just demo code :)
                    connection.Open();
                    connection.EnableExtensions(true);

                    options.UseSqlite(connection);
                });

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            BootstrapDataStore(app.ApplicationServices);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        public void BootstrapDataStore(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
                dbContext.Seed();
            }
        }
    }
}
