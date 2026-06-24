using Microsoft.Extensions.Options;
using EasyDI.Registering;
using EasyDI.Resolving;

namespace EasyDI.Configuration;

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