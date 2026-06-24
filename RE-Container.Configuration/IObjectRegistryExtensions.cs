using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using REContainer.Registering;
using REContainer.Resolving;

namespace REContainer.Configuration;

public static class IObjectRegistryExtensions
{
	/// <summary>
	/// Registers the root <see cref="IConfiguration"/> instance.
	/// This will enable using <see cref="RegisterOptions{TOptions}"/>
	/// </summary>
	public static void RegisterRootConfiguration(this IObjectRegistry registry, IConfiguration configuration)
	{
		registry.RegisterInstance<IConfiguration>(configuration);
	}

	/// <summary>
	/// Registers options <see cref="IOptions{TOptions}"/>, <see cref="IOptionsSnapshot{TOptions}"/> and
	/// <see cref="IOptionsMonitor{TOptions}"/> of type <typeparamref name="TOptions"/> bound to the configuration
	/// section with the name <paramref name="sectionName"/>.
	/// The options will be validated using any data annotations on resolve or on resolver build if you chain
	/// <see cref="RegistersResolveOnBuild{TOptions}.ValidateOnBuild"/>.
	/// Ensure that the root <see cref="IConfiguration"/> instance has been registered using
	/// <see cref="RegisterRootConfiguration(IObjectRegistry, IConfiguration)"/> in the current or parent
	/// <see cref="IObjectRegistry"/>.
	/// </summary>
	public static RegistersResolveOnBuild<TOptions> RegisterOptions<TOptions>(this IObjectRegistry registry,
		string sectionName)
		where TOptions : class, new()
	{
		registry.RegisterSingleton<IOptions<TOptions>>(CreateOptions<TOptions>(sectionName));

		// Only registered for resolving IOptionsSnapshot and IOptionsMonitor. Not intended for direct use.
		registry.RegisterSingleton<IOptionsFactory<TOptions>>(CreateOptionsFactory<TOptions>(sectionName));

		registry.RegisterScoped<IOptionsSnapshot<TOptions>>(CreateOptionsSnapshot<TOptions>);

		registry.RegisterSingleton(CreateOptionsMonitor<TOptions>(sectionName))
			.As<IOptionsMonitor<TOptions>>()
			.As<IDisposable>();

		return new RegistersResolveOnBuild<TOptions>(registry);
	}

	private static Func<IObjectResolver, IOptions<TOptions>> CreateOptions<TOptions>(string sectionName)
		where TOptions : class, new()
	{
		return resolver => resolver.GetConfiguration().GetSection(sectionName).GetAndValidate<TOptions>().ToOptions();
	}

	private static Func<IObjectResolver, OptionsFactory<TOptions>> CreateOptionsFactory<TOptions>(string sectionName)
		where TOptions : class, new()
	{
		return resolver => new OptionsFactory<TOptions>(
			[CreateConfigureNamedOptions<TOptions>(resolver.GetConfiguration(), sectionName)],
			[],
			[new DataAnnotationValidateOptions<TOptions>(Options.DefaultName)]);
	}

	private static IOptionsSnapshot<TOptions> CreateOptionsSnapshot<TOptions>(
		IObjectResolver resolver)
		where TOptions : class, new()
	{
		return new OptionsManager<TOptions>(resolver.GetOptionsFactory<TOptions>());
	}

	private static Func<IObjectResolver, OptionsMonitor<TOptions>> CreateOptionsMonitor<TOptions>(string sectionName)
		where TOptions : class, new()
	{
		return resolver => new OptionsMonitor<TOptions>(
			resolver.GetOptionsFactory<TOptions>(),
			[new ConfigurationChangeTokenSource<TOptions>(sectionName, resolver.GetConfiguration())],
			new OptionsCache<TOptions>());
	}

	private static ConfigureNamedOptions<TOptions> CreateConfigureNamedOptions<TOptions>(
		IConfiguration configuration,
		string sectionName) where TOptions : class
	{
		return new ConfigureNamedOptions<TOptions>(
			Options.DefaultName,
			o => configuration.GetSection(sectionName).Bind(o));
	}
}