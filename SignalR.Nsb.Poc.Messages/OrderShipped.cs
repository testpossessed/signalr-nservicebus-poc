﻿using NServiceBus;
using SignalR.Nsb.Poc.Messages.Abstractions;

namespace SignalR.Nsb.Poc.Messages
{
    public class OrderShipped: OrderMessage, IEvent
    {
    }
}
