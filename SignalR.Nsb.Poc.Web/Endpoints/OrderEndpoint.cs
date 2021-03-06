﻿using System;
using System.Threading.Tasks;
using NServiceBus;
using SignalR.Nsb.Poc.Messages;
using SignalR.Nsb.Poc.NServiceBus.Abstractions;
using SignalR.Nsb.Poc.Web.Abstractions;

namespace SignalR.Nsb.Poc.Web.Endpoints
{
    public class OrderEndpoint: IOrderEndpoint
    {
        private readonly IEndpointInstanceBuilder _endpointInstanceBuilder;
        private readonly IOrderHubMessageDispatcher _orderHubMessageDispatcher;
        private IEndpointInstance _endpointInstance;

        public OrderEndpoint(IEndpointInstanceBuilder endpointInstanceBuilder,
            IOrderHubMessageDispatcher orderHubMessageDispatcher)
        {
            _endpointInstanceBuilder = endpointInstanceBuilder;
            _orderHubMessageDispatcher = orderHubMessageDispatcher;
            Initialise().GetAwaiter().GetResult();
        }

        private async Task Initialise()
        {
            var startableEndpoint = await _endpointInstanceBuilder.Create("SignalR.Nsb.Poc.Web.Order")
                .WithTransport<RabbitMQTransport>(tc =>
                {
                    var routing = tc.Routing();

                    routing.RouteToEndpoint(typeof(PlaceOrder), "SignalR.Nsb.Poc.Sales");

                    tc.UseConventionalRoutingTopology();

                    // swap these lines to switch between running in containers or running locally
                    // not to run locally you wil need to install RabbitMQ
                    //tc.ConnectionString("host=localhost");
                    tc.ConnectionString("host=rabbitmq;username=admin;password=password");
                })
                .WithRegisteredComponents(c => { c.RegisterSingleton(_orderHubMessageDispatcher); })
                .Build();

            _endpointInstance = await startableEndpoint.Start();
        }

        public async Task SendLocal<T>(T message) where T : ICommand
        {
            await _endpointInstance.SendLocal(message);
        }

        public async Task Send<T>(T message) where T : ICommand
        {
            await _endpointInstance.Send(message);
        }
    }
}
