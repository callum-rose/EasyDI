using REContainer.Registering.RegistrationBuilders;
using REContainer.Resolving;
using REContainer.Exceptions;

namespace REContainer.Registering;

public class ObjectRegistry : IObjectRegistry
{
	private readonly IObjectResolver _parent;
	private readonly IReadOnlyList<Registration> _existingRegistrations;
	private readonly List<RegistrationBuilder> _registrationBuilders = [];
	private readonly HashSet<Type> _typesResolvableAsMany = [typeof(ResolverBuiltCallback)];

	public static ObjectRegistry CreateRoot()
	{
		return new ObjectRegistry(NullObjectResolver.Instance, [], []);
	}

	public static ObjectRegistry CreateChild(IObjectResolver parent)
	{
		return new ObjectRegistry(parent, parent.CloneScopedRegistrations(), []);
	}

	private ObjectRegistry(IObjectResolver parent,
		IReadOnlyList<Registration> existingRegistrations,
		IReadOnlyCollection<Type> existingTypesResolvableAsMany)
	{
		_parent = parent;
		_existingRegistrations = existingRegistrations;
		_typesResolvableAsMany.UnionWith(existingTypesResolvableAsMany);
	}

	public bool HasRegistrationResolvableAs<TResolvable>()
	{
		return _existingRegistrations.Any(r => r.ResolvableTypes.Contains(typeof(TResolvable))) ||
		       _registrationBuilders.Any(rb => rb.AllValidResolvableTypes.Contains(typeof(TResolvable))) ||
		       _parent.CanResolve<TResolvable>();
	}

	public TRegistrationBuilder Register<TRegistrationBuilder>(TRegistrationBuilder registrationBuilder)
		where TRegistrationBuilder : RegistrationBuilder
	{
		_registrationBuilders.Add(registrationBuilder);
		return registrationBuilder;
	}

	public void MarkResolvableAsMany(Type type)
	{
		_typesResolvableAsMany.Add(type);
	}

	public IObjectResolver Build()
	{
		Validate();

		var resolver = new ObjectResolver(
			[.._existingRegistrations, .._registrationBuilders.Select(r => r.Build())],
			_parent,
			_typesResolvableAsMany,
			[]);

		foreach (var callback in resolver.ResolveOrFallback<IEnumerable<ResolverBuiltCallback>>([])!)
		{
			try
			{
				callback.Invoke(resolver);
			}
			catch (Exception e)
			{
				throw new ResolverBuildCallbackException(
					$"An exception occurred while executing a resolver built callback: {e.Message}",
					e);
			}
		}

		return resolver;
	}

	private void Validate()
	{
		var errors = new List<string>();

		// Validate type compatibility for each registration
		foreach (var registrationBuilder in _registrationBuilders)
		{
			var incompatibleTypes = registrationBuilder.CertainResolvableTypes
				.Where(t => !t.IsAssignableFromGeneric(registrationBuilder.ImplementationType))
				.ToList();

			if (incompatibleTypes.Count > 0)
			{
				errors.Add(
					$"Implementation type '{registrationBuilder.ImplementationType.FullName}' " +
					$"is not assignable to resolvable type(s): {string.Join(", ", incompatibleTypes.Select(t => t.FullName ?? t.Name))}");
			}
		}

		foreach (Type type in _registrationBuilders.SelectMany(rb => rb.AllValidResolvableTypes).Distinct())
		{
			bool parentCanResolveSolo = _parent.CanResolve(type);
			bool parentCanResolveMany = _parent.CanResolveMany(type);
			bool typeExpectedAsMany = _typesResolvableAsMany.Contains(type);

			if (parentCanResolveSolo)
			{
				errors.Add(
					$"Type '{type.FullName}' is already registered in a parent registry and cannot be re-registered");
			}

			if (parentCanResolveMany && !typeExpectedAsMany)
			{
				errors.Add(
					$"Type '{type.FullName}' is already registered as 'many' in a parent registry. " +
					$"Cannot register it as a single instance in a child registry. Use {nameof(IObjectRegistry)}.{nameof(MarkResolvableAsMany)}()");
			}
		}

		var undeclaredTypesRegisteredMoreThanOnce = _registrationBuilders
			.SelectMany(rb => rb.AllValidResolvableTypes)
			.GroupBy(t => t)
			.Where(g => g.Count() > 1)
			.Where(g => !_typesResolvableAsMany.Contains(g.Key))
			.ToArray();

		foreach (var group in undeclaredTypesRegisteredMoreThanOnce)
		{
			errors.Add(
				$"Type '{group.Key.FullName}' was registered {group.Count()} times. " +
				$"Use {nameof(IObjectRegistry)}.{nameof(MarkResolvableAsMany)}() to allow multiple registrations for a type");
		}

		if (errors.Count > 0)
		{
			throw new RegistrationException(errors);
		}
	}
}