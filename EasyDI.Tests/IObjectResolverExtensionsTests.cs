using EasyDI.Exceptions;
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
	public void TryResolveResult_WhenRegistered_ReturnsSuccessWithInstance()
	{
		var registry = ObjectRegistry.CreateRoot();
		registry.RegisterSingleton<Dependency>();
		var resolver = registry.Build();

		var result = resolver.TryResolve<Dependency>();

		Assert.That(result, Is.InstanceOf<Result<Dependency>.Success>());
		Assert.That(((Result<Dependency>.Success)result).Value, Is.TypeOf<Dependency>());
	}

	[Test]
	public void TryResolveResult_WhenConstructorThrows_ReturnsFailureCarryingException()
	{
		var registry = ObjectRegistry.CreateRoot();
		registry.RegisterSingleton<ThrowingConstructor>();
		var resolver = registry.Build();

		Result<ThrowingConstructor> result = null!;
		Assert.That(() => result = resolver.TryResolve<ThrowingConstructor>(), Throws.Nothing);

		Assert.That(result, Is.InstanceOf<Result<ThrowingConstructor>.Failure>());
		var exception = ((Result<ThrowingConstructor>.Failure)result).Exception;
		Assert.That(exception, Is.InstanceOf<ResolutionException>());
		Assert.That(GetInnermost(exception), Is.InstanceOf<InvalidOperationException>());
	}

	[Test]
	public void TryResolveResult_WhenTransitiveDependencyMissing_ReturnsFailureCarryingException()
	{
		var registry = ObjectRegistry.CreateRoot();
		registry.RegisterSingleton<NeedsMissingDependency>();
		var resolver = registry.Build();

		Result<NeedsMissingDependency> result = null!;
		Assert.That(() => result = resolver.TryResolve<NeedsMissingDependency>(), Throws.Nothing);

		Assert.That(result, Is.InstanceOf<Result<NeedsMissingDependency>.Failure>());
		Assert.That(((Result<NeedsMissingDependency>.Failure)result).Exception, Is.Not.Null);
	}

	[Test]
	public void TryResolveResult_WhenNotRegistered_ReturnsFailureCarryingResolutionException()
	{
		var resolver = ObjectRegistry.CreateRoot().Build();

		var result = resolver.TryResolve<Dependency>();

		Assert.That(result, Is.InstanceOf<Result<Dependency>.Failure>());
		Assert.That(((Result<Dependency>.Failure)result).Exception, Is.InstanceOf<ResolutionException>());
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

	private static Exception GetInnermost(Exception exception)
	{
		while (exception.InnerException is { } inner)
		{
			exception = inner;
		}

		return exception;
	}
}