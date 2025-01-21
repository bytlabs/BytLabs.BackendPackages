using BytLabs.Domain.DomainEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BytLabs.States.Domain.Tests.Orders
{
    public class OrderAggregate : StatefulAggregateRoot<Guid>
    {
        public OrderAggregate(Guid id, string stateMachineId, string stateId) : base(id, stateMachineId, stateId)
        {

        }

        public void MarkAsShipped()
        {
            AddDomainEvent(new OrderShippedEvent());
        }
    }

    public class OrderShippedEvent : IDomainEvent
    {

    }
}
