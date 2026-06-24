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

	private class ThrowingConstructor
	{
		public ThrowingConstructor() => throw new InvalidOperationException("boom");
	}

	private class Dependency;
	private class NeedsMissingDependency(Dependency dependency);

	[Test]
	public void TryResolve_WhenConstructorThrows_ReturnsFalseInsteadOfThrowing()
	{
		var registry = ObjectRegistry.CreateRoot();
		registry.RegisterSingleton<ThrowingConstructor>();
		var resolver = registry.Build();

		bool result = false;
		ThrowingConstructor? instance = null;
		Assert.That(() => result = resolver.TryResolve(out instance), Throws.Nothing);
		Assert.That(result, Is.False);
		Assert.That(instance, Is.Null);
	}

	[Test]
	public void TryResolve_WhenTransitiveDependencyMissing_ReturnsFalseInsteadOfThrowing()
	{
		var registry = ObjectRegistry.CreateRoot();
		registry.RegisterSingleton<NeedsMissingDependency>();
		var resolver = registry.Build();

		bool result = false;
		NeedsMissingDependency? instance = null;
		Assert.That(() => result = resolver.TryResolve(out instance), Throws.Nothing);
		Assert.That(result, Is.False);
		Assert.That(instance, Is.Null);
	}

	[Test]
	public void ResolveOrFallback_WhenTransitiveDependencyMissing_ReturnsFallback()
	{
		var registry = ObjectRegistry.CreateRoot();
		registry.RegisterSingleton<NeedsMissingDependency>();
		var resolver = registry.Build();

		var fallback = new NeedsMissingDependency(new Dependency());
		NeedsMissingDependency? result = null;
		Assert.That(() => result = resolver.ResolveOrFallback(fallback), Throws.Nothing);
		Assert.That(result, Is.SameAs(fallback));
	}
}