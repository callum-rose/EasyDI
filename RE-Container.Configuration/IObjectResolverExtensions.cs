using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using REContainer.Resolving;
using REContainer.Exceptions;

namespace REContainer.Configuration;

internal static class IObjectResolverExtensions
{
	public static IConfiguration GetConfiguration(this IObjectResolver resolver)
	{
		return resolver.TryLazyResolve(new SingleInstanceQuery(typeof(IConfiguration))) switch
		{
			Success success => (IConfiguration)success.InstanceGetter.Invoke(),
			Fail fail => throw new ResolutionException(
				typeof(IConfiguration),
				fail,
				resolver.GetAllResolvableTypes()),
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	public static IOptionsFactory<TOptions> GetOptionsFactory<TOptions>(this IObjectResolver resolver)
		where TOptions : class
	{
		return resolver.TryLazyResolve(new SingleInstanceQuery(typeof(IOptionsFactory<TOptions>))) switch
		{
			Success success => (IOptionsFactory<TOptions>)success.InstanceGetter.Invoke(),
			Fail fail => throw new ResolutionException(
				typeof(IOptionsFactory<TOptions>),
				fail,
				resolver.GetAllResolvableTypes()),
			_ => throw new ArgumentOutOfRangeException()
		};
	}
}