using System;
using DevMeetings.Services.Notifications.Api.Consumers;
using DevMeetings.Services.Notifications.Api.Infrastructure;
using DevMeetings.Services.Notifications.Api.Infrastructure.Persistence.Repositories;
using DevMeetings.Services.Notifications.Api.Infrastructure.Services.Notifications;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using SendGrid.Extensions.DependencyInjection;

namespace DevMeetings.Services.Notifications.Api
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
            var config = new MailConfig();

            Configuration.GetSection("Notifications").Bind(config);

            Console.WriteLine(JsonConvert.SerializeObject(config));

            services.AddSingleton(sp => config);

            services.AddMongo();
            
            services.AddSendGrid(options => options.ApiKey = config.SendGridApiKey);
            services.AddTransient<INotificationService, NotificationService>();
            services.AddTransient<IMailRepository, MailRepository>();

            services.AddHostedService<UserCreatedConsumer>();

            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "DevMeetings.Services.Notifications.Api", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DevMeetings.Services.Notifications.Api v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
