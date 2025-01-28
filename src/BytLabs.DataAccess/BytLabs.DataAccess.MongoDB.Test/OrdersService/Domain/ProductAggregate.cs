using BytLabs.Domain.Audit;
using BytLabs.Domain.DomainEvents;
using BytLabs.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BytLabs.DataAccess.MongoDB.Test.OrdersService.Domain
{
    public class ProductAggregate : IAggregateRoot<Guid>
    {
        public IReadOnlyCollection<IDomainEvent> DomainEvents => throw new NotImplementedException();

        public Guid Id => throw new NotImplementedException();

        public DateTime? CreatedAt { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string? CreatedBy { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime? LastModifiedAt { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string? LastModifiedBy { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime? DeletedAt { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string? DeletedBy { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public AuditInfo AuditInfo => throw new NotImplementedException();

        public void ClearDomainEvents()
        {
            throw new NotImplementedException();
        }
    }
}
