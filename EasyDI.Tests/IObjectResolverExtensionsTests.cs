using EasyDI.Registering;
using EasyDI.Resolving;

namespace Tests;

public class IObjectResolverExtensionsTests
{
	[Test]
	public void WithArgs_CreatesResolverWithProvidedArguments_CanResolveAllArgTypes()
	{
		var resolver = ObjectRegistry.CreateRoot().Build();

		var copiedResolver = resolver.WithAdditionalArguments([1.ToArgumentInfo(), "Hi".ToArgumentInfo()]);

		Assert.That(copiedResolver.CanResolve<int>(), Is.True);
		Assert.That(copiedResolver.CanResolve<string>(), Is.True);
	}

	[Test]
	public void WithArgs_WhenResolverAlreadyRegistrationOfArgumentType_ThrowsException()
	{
		var registry = ObjectRegistry.CreateRoot();
		registry.RegisterInstance(5);
		var resolver = registry.Build();

		void Act() => resolver.WithAdditionalArguments([1.ToArgumentInfo()]);

		Assert.That(Act, Throws.Exception);
	}
}