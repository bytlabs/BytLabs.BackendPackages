using BytLabs.DataAccess.EntityFramework.Configuration;
using BytLabs.Infrastructure.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BytLabs.DataAccess.EntityFramework.Test;

public class ConfigurationTests
{
    [Fact]
    public void GIVEN_UseTransactionsFalse_WHEN_AddEntityFrameworkDatabase_THEN_Throws()
    {
        var services = new ServiceCollection();
        var config = new EfDatabaseConfiguration { UseTransactions = false };

        var act = () => services.AddEntityFrameworkDatabase<TestDbContext>(config,
            (options, conn) => options.UseSqlite(conn));

        act.Should().Throw<InfrastructureException>();
    }
}
