using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NSubstitute;
using EasyDI.Configuration;
using EasyDI.Exceptions;
using EasyDI.Registering;
using EasyDI.Resolving;

namespace Tests;

public class ConfigurationTests
{
	public class MyOptions
	{
		[Required]
		public string? Text { get; set; }

		[System.ComponentModel.DataAnnotations.Range(0, 10)]
		public int Number { get; set; }
	}

	[Test]
	public void RegisterRootConfiguration_WhenCalled_ReturnsRegisteredConfiguration()
	{
		var configuration = Substitute.For<IConfiguration>();

		var registry = ObjectRegistry.CreateRoot();
		registry.RegisterRootConfiguration(configuration);
		var resolver = registry.Build();

		var resolvedConfiguration = resolver.Resolve<IConfiguration>();

		Assert.That(resolvedConfiguration, Is.EqualTo(configuration));
	}

	[Test]
	public void Resolve_IOptions_ReturnsConfiguredOptionsWithCorrectValues()
	{
		var configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				{ "MyOptions:Text", "Hello" },
				{ "MyOptions:Number", "5" }
			})
			.Build();

		var registry = ObjectRegistry.CreateRoot();

		registry.RegisterRootConfiguration(configuration);
		registry.RegisterOptions<MyOptions>("MyOptions");

		var resolver = registry.Build();

		var options = resolver.Resolve<IOptions<MyOptions>>();

		Assert.That(options, Is.Not.Null);
		Assert.That(options.Value.Text, Is.EqualTo("Hello"));
		Assert.That(options.Value.Number, Is.EqualTo(5));
	}
	
	[Test]
    public void Resolve_IOptions_WhenNoRegisterRootConfiguration_ThrowsException()
	{
		var registry = ObjectRegistry.CreateRoot();

		// registry.RegisterRootConfiguration(configuration);
		registry.RegisterOptions<MyOptions>("MyOptions");

		var resolver = registry.Build();

		void Act() => resolver.Resolve<IOptions<MyOptions>>();
		
		Assert.That(Act, Throws.Exception);
	}

	[Test]
	public void CanResolve_WithOptionsType_ReturnsFalse()
	{
		var configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				{ "MyOptions:Text", "Hello" },
				{ "MyOptions:Number", "5" }
			})
			.Build();

		var registry = ObjectRegistry.CreateRoot();

		registry.RegisterRootConfiguration(configuration);
		registry.RegisterOptions<MyOptions>("MyOptions");

		var resolver = registry.Build();

		var canResolve = resolver.CanResolve<MyOptions>();

		Assert.That(canResolve, Is.False);
	}

	[Test]
	public void Resolve_IOptionsWithInvalidConfiguration_ThrowsException()
	{
		var configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				// { "MyOptions:Text", "Hello" },
				{ "MyOptions:Number", "-1" }
			})
			.Build();

		var registry = ObjectRegistry.CreateRoot();

		registry.RegisterRootConfiguration(configuration);
		registry.RegisterOptions<MyOptions>("MyOptions");

		var resolver = registry.Build();

		void Act() => resolver.Resolve<IOptions<MyOptions>>();

		var ex = Assert.Throws<FactoryException>(Act);
		Assert.That(ex.InnerException, Is.TypeOf<OptionsValidationException>());
	}

	[Test]
	public void Build_WithValidateOnBuildAndInvalidConfiguration_ThrowsException()
	{
		var configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				//{ "MyOptions:Text", "Hello" },
				{ "MyOptions:Number", "-1" }
			})
			.Build();

		var registry = ObjectRegistry.CreateRoot();

		registry.RegisterRootConfiguration(configuration);
		registry.RegisterOptions<MyOptions>("MyOptions").ValidateOnBuild();

		void Act() => registry.Build();

		Assert.Throws<ResolverBuildCallbackException>(Act);
	}

	[Test]
	public void Resolve_IOptionsSnapshot_ReturnsConfiguredOptionsWithCorrectValues()
	{
		var configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				{ "MyOptions:Text", "Hello" },
				{ "MyOptions:Number", "5" }
			})
			.Build();

		var registry = ObjectRegistry.CreateRoot();

		registry.RegisterRootConfiguration(configuration);
		registry.RegisterOptions<MyOptions>("MyOptions");

		var resolver = registry.Build();

		var options = resolver.Resolve<IOptionsSnapshot<MyOptions>>();

		Assert.That(options, Is.Not.Null);
		Assert.That(options.Value.Text, Is.EqualTo("Hello"));
		Assert.That(options.Value.Number, Is.EqualTo(5));
	}

	[Test]
	public void Resolve_IOptionsMonitor_ReturnsConfiguredOptionsWithCorrectValues()
	{
		var configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				{ "MyOptions:Text", "Hello" },
				{ "MyOptions:Number", "5" }
			})
			.Build();

		var registry = ObjectRegistry.CreateRoot();

		registry.RegisterRootConfiguration(configuration);
		registry.RegisterOptions<MyOptions>("MyOptions");

		var resolver = registry.Build();

		var options = resolver.Resolve<IOptionsMonitor<MyOptions>>();

		Assert.That(options, Is.Not.Null);
		Assert.That(options.CurrentValue.Text, Is.EqualTo("Hello"));
		Assert.That(options.CurrentValue.Number, Is.EqualTo(5));
	}
}