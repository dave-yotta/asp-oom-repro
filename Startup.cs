using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OOMRepro
{
    public class Startup
    {
        public static readonly List<object> Store = new List<object>();
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(opt =>
            {
                opt.SetMinimumLevel(LogLevel.Trace);
                opt.AddFilter("Microsoft", LogLevel.Warning);
                opt.AddFilter("System", LogLevel.Error);
                opt.AddFilter("Engine", LogLevel.Warning);

                opt.AddConsole(c =>
                {
                    c.TimestampFormat = "[HH:mm:ss] ";
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerProvider logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            void Alloc()
            {
                Console.WriteLine($"GC Heap Limit = {GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / (1024 * 1024.0)}mb");
                while (true)
                {
                    Store.Add(Enumerable.Repeat<byte>(0x01, 1024).ToArray());
                    Console.Write($"\rAllocated {Store.Count / 1024.0}mb");
                }
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/allocate", async context =>
                {
                    Console.WriteLine($"{DateTime.UtcNow.ToString("[HH:mm:ss]: ")} Allocate Called");
                    Alloc();
                });

                endpoints.MapGet("/allocate-safe", async context =>
                {
                    Console.WriteLine($"{DateTime.UtcNow.ToString("[HH:mm:ss]: ")} Allocate With Catch Called");
                    try { Alloc();}
                    catch(OutOfMemoryException m)
                    {
                        Environment.FailFast(m.ToString());
                    }
                });

                endpoints.MapGet("/clear", async c => Store.Clear());

                endpoints.MapGet("/other", async c => { Console.WriteLine($"{DateTime.UtcNow.ToString("[HH:mm:ss]: ")} Other Called"); await c.Response.WriteAsync("Ciao"); });
            });
        }
    }
}
