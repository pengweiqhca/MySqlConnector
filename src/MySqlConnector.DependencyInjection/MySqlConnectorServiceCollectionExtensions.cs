using System.Data.Common;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension method for setting up MySqlConnector services in an <see cref="IServiceCollection" />.
/// </summary>
public static class MySqlConnectorServiceCollectionExtensions
{
	/// <summary>
	/// Registers a <see cref="MySqlDataSource" /> and a <see cref="MySqlConnection" /> in the <see cref="IServiceCollection" />.
	/// </summary>
	/// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
	/// <param name="connectionString">A MySQL connection string.</param>
	/// <param name="connectionLifetime">The lifetime with which to register the <see cref="MySqlConnection" /> in the container. Defaults to <see cref="ServiceLifetime.Transient" />.</param>
	/// <param name="dataSourceLifetime">The lifetime with which to register the <see cref="MySqlDataSource" /> service in the container. Defaults to <see cref="ServiceLifetime.Singleton" />.</param>
	/// <returns>The same service collection so that multiple calls can be chained.</returns>
	public static IServiceCollection AddMySqlDataSource(
		this IServiceCollection serviceCollection,
		string connectionString,
		ServiceLifetime connectionLifetime = ServiceLifetime.Transient,
		ServiceLifetime dataSourceLifetime = ServiceLifetime.Singleton) =>
		DoAddMySqlDataSource(serviceCollection, connectionString, dataSourceBuilderAction: null, connectionLifetime, dataSourceLifetime);

	/// <summary>
	/// Registers a <see cref="MySqlDataSource" /> and a <see cref="MySqlConnection" /> in the <see cref="IServiceCollection" />.
	/// </summary>
	/// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
	/// <param name="connectionString">A MySQL connection string.</param>
	/// <param name="dataSourceBuilderAction">An action to configure the <see cref="MySqlDataSourceBuilder" /> for further customizations of the <see cref="MySqlDataSource" />.</param>
	/// <param name="connectionLifetime">The lifetime with which to register the <see cref="MySqlConnection" /> in the container. Defaults to <see cref="ServiceLifetime.Transient" />.</param>
	/// <param name="dataSourceLifetime">The lifetime with which to register the <see cref="MySqlDataSource" /> service in the container. Defaults to <see cref="ServiceLifetime.Singleton" />.</param>
	/// <returns>The same service collection so that multiple calls can be chained.</returns>
	public static IServiceCollection AddMySqlDataSource(
		this IServiceCollection serviceCollection,
		string connectionString,
		Action<MySqlDataSourceBuilder> dataSourceBuilderAction,
		ServiceLifetime connectionLifetime = ServiceLifetime.Transient,
		ServiceLifetime dataSourceLifetime = ServiceLifetime.Singleton) =>
		DoAddMySqlDataSource(serviceCollection, connectionString, dataSourceBuilderAction, connectionLifetime, dataSourceLifetime);

	private static IServiceCollection DoAddMySqlDataSource(
		this IServiceCollection serviceCollection,
		string connectionString,
		Action<MySqlDataSourceBuilder>? dataSourceBuilderAction,
		ServiceLifetime connectionLifetime,
		ServiceLifetime dataSourceLifetime)
	{
		serviceCollection.TryAdd(
			new ServiceDescriptor(
				typeof(MySqlDataSource),
				x =>
				{
					var dataSourceBuilder = new MySqlDataSourceBuilder(connectionString)
						.UseLoggerFactory(x.GetService<ILoggerFactory>());
					dataSourceBuilderAction?.Invoke(dataSourceBuilder);
					return dataSourceBuilder.Build();
				},
				dataSourceLifetime));

		serviceCollection.TryAdd(new ServiceDescriptor(typeof(MySqlConnection), x => x.GetRequiredService<MySqlDataSource>().CreateConnection(), connectionLifetime));

#if NET7_0_OR_GREATER
		serviceCollection.TryAdd(new ServiceDescriptor(typeof(DbDataSource), x => x.GetRequiredService<MySqlDataSource>(), dataSourceLifetime));
#endif

		serviceCollection.TryAdd(new ServiceDescriptor(typeof(DbConnection), x => x.GetRequiredService<MySqlConnection>(), connectionLifetime));

		return serviceCollection;
	}
}
