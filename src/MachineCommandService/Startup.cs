﻿namespace MachineCommandService
{
    using System;
    using Castle.Facilities.AspNetCore;
    using Castle.Facilities.TypedFactory;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;
    using EventBus;
    using EventBus.RabbitMQ;
    using EventLogger.NLog;
    using Logging.NLog.Impl.Castle;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

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
            Configuration cfg;

            if (Environment.GetEnvironmentVariable("RUNNING_IN_CONTAINER") != null)
            {
                cfg = new Configuration
                {
                    RabbitMqBrokerName = Environment.GetEnvironmentVariable("RABBITMQ_BROKER_NAME"),
                    RabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST"),
                    RabbitMqUser = Environment.GetEnvironmentVariable("RABBITMQ_USER"),
                    RabbitMqPassword = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD"),
                };
            }
            else
            {
                var appSettingSection = Configuration.GetSection("Configuration");
                services.Configure<Configuration>(appSettingSection);

                cfg = appSettingSection.Get<Configuration>();
            }

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            var container = new WindsorContainer();
            container.AddFacility<AspNetCoreFacility>(f => f.CrossWiresInto(services));
            container.AddFacility<TypedFactoryFacility>();

            container.Install(new LogInstaller());

            container.Register(
                Component.For<IIntegrationEventHandlerFactory>().AsFactory(new IntegrationEventHandlerComponentSelector()),

                Component.For<IEventBusSubscriptionsManager>()
                         .ImplementedBy<InMemoryEventBusSubscriptionsManager>(),

                Component.For<IEventLogger>()
                         .ImplementedBy<EventLogger>(),

                Component.For<IEventBus>()
                         .ImplementedBy<EventBusRabbitMQ>()
                         .DependsOn(new { brokerName = cfg.RabbitMqBrokerName }),

                Component.For<IRabbitMQPersistentConnection>()
                         .ImplementedBy<DefaultRabbitMQPersistentConnection>()
                         .DependsOn(new
                         {
                             hostName = cfg.RabbitMqHost,
                             userName = cfg.RabbitMqUser,
                             password = cfg.RabbitMqPassword
                         }));

            services.AddWindsor(container, opts => opts.UseEntryAssembly(typeof(IRef).Assembly));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
