using NSubstitute;
using REContainer.Registering;
using REContainer.Registering.RegistrationBuilders;
using REContainer.Resolving;
using REContainer.Resolving.InstanceProviders;

namespace Tests;

public class RegistrationBuilderTests
{
	private class TestRegistrationBuilder : RegistrationBuilder
	{
		private readonly IInstanceProvider _instanceProvider;

		public TestRegistrationBuilder(Type implementationType,
			Lifetime lifetime,
			HashSet<Type> declaredCertainResolvableTypes,
			IInstanceProvider instanceProvider) : base(implementationType, lifetime)
		{
			DeclaredCertainResolvableTypes = declaredCertainResolvableTypes;
			_instanceProvider = instanceProvider;
		}

		protected override IInstanceProvider CreateInstanceProvider()
		{
			return _instanceProvider;
		}
	}

	private interface ITestService;
	private class TestService : ITestService;

	[Test]
	public void WhenInterfaceTypesSpecified_Build_ReturnsRegistrationWithGivenInterfaceTypes()
	{
		var instanceProvider = Substitute.For<IInstanceProvider>();
		var builder = new TestRegistrationBuilder(typeof(TestService),
			Lifetime.Singleton,
			[typeof(ITestService)],
			instanceProvider);

		var registration = builder.Build();

        Assert.Multiple(() =>
        {
            Assert.That(registration.ImplementationType, Is.EqualTo(typeof(TestService)));
            Assert.That(registration.Lifetime, Is.EqualTo(Lifetime.Singleton));
            Assert.That(registration.ResolvableTypes, Is.EquivalentTo(new[] { typeof(ITestService) }));
            Assert.That(registration.Provider, Is.SameAs(instanceProvider));
        });
    }
	
	[Test]
	public void WhenInterfaceTypesUnspecified_Build_ReturnsRegistrationWithImplementationTypeAsInterface()
	{
		var instanceProvider = Substitute.For<IInstanceProvider>();
		var builder = new TestRegistrationBuilder(typeof(TestService),
			Lifetime.Transient,
			[],
			instanceProvider);

		var registration = builder.Build();

        Assert.Multiple(() =>
        {
            Assert.That(registration.ImplementationType, Is.EqualTo(typeof(TestService)));
            Assert.That(registration.Lifetime, Is.EqualTo(Lifetime.Transient));
            Assert.That(registration.ResolvableTypes, Is.EquivalentTo(new[] { typeof(TestService) }));
            Assert.That(registration.Provider, Is.SameAs(instanceProvider));
        });
    }
}