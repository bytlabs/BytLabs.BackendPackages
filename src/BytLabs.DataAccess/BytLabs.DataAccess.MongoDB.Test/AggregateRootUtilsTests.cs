using BytLabs.DataAccess.MongoDB;
using BytLabs.DataAccess.MongoDB.Test.OrdersService.Domain;
using FluentAssertions;

namespace BytLabs.DataAccess.MongoDB.Test
{
    public class AggregateRootUtilsTests
    {
        [Fact]
        public void GIVEN_AggregateOrder_WHEN_InhertiedFromAggregateRootBase_THEN_IsInheritingShouldBeTrue()
        {
            var isEntityInheritedFromAggregateRootBase = AggregateRootUtils.IsInheritingFromAggregateRootBase<OrderAggregate, Guid>();
            var isEntityInheritingDomainEventsFromIAggregateRootInterface = AggregateRootUtils.IsImplementingIAggregateRoot<OrderAggregate, Guid>();

            isEntityInheritedFromAggregateRootBase.Should().BeTrue();
            isEntityInheritingDomainEventsFromIAggregateRootInterface.Should().BeFalse();
        }

        [Fact]
        public void GIVEN_AggregateProduct_WHEN_ImplementedIAggregateRoot_THEN_IsImplementingIAggregateRootShouldBeTrue()
        {
            var isEntityInheritedFromAggregateRootBase = AggregateRootUtils.IsInheritingFromAggregateRootBase<ProductAggregate, Guid>();
            var isEntityInheritingDomainEventsFromIAggregateRootInterface = AggregateRootUtils.IsImplementingIAggregateRoot<ProductAggregate, Guid>();

            isEntityInheritedFromAggregateRootBase.Should().BeFalse();
            isEntityInheritingDomainEventsFromIAggregateRootInterface.Should().BeTrue();
        }
    }
}
