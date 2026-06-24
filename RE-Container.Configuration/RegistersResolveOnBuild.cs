using Microsoft.Extensions.Options;
using REContainer.Registering;
using REContainer.Resolving;

namespace REContainer.Configuration;

public class RegistersResolveOnBuild<TOptions> where TOptions : class, new()
{
	private readonly IObjectRegistry _objectRegistry;

	public RegistersResolveOnBuild(IObjectRegistry objectRegistry)
	{
		_objectRegistry = objectRegistry;
	}

	public void ValidateOnBuild()
	{
		_objectRegistry.RegisterBuildCallback(resolver => resolver.Resolve<IOptions<TOptions>>());
	}
}