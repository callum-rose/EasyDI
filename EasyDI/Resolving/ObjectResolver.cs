using System.Collections;

namespace EasyDI.Resolving;

internal class ObjectResolver : IObjectResolver
{
	public IObjectResolver Parent { get; }
	public IReadOnlyList<Registration> LocalRegistrations { get; }
	public IReadOnlyCollection<Type> TypesResolvableAsMany { get; }

	private readonly IReadOnlyList<ArgumentInfo> _additionalArguments;

	public ObjectResolver(IReadOnlyList<Registration> registrations,
		IObjectResolver parent,
		IReadOnlyCollection<Type> typesResolvableAsMany,
		IReadOnlyList<ArgumentInfo> additionalArguments)
	{
		LocalRegistrations = registrations;
		Parent = parent;
		TypesResolvableAsMany = typesResolvableAsMany;
		_additionalArguments = additionalArguments;
	}

	public ResolveResult TryLazyResolve(ResolutionQuery query)
	{
		switch (query)
		{
			case SingleInstanceQuery { FromChildResolver: false } single
				when _additionalArguments.LastOrDefault(a => a.Type == single.Type) is { } argument:
				return Success.Create(() => argument.Value);

			case SingleInstanceQuery { Type: var type } when type == typeof(IObjectResolver):
				return Success.Create(() => this);

			case SingleInstanceQuery single when LocalRegistrations.TryFind(single.Type, out var registration):
				return ShouldNotBeResolvedAsMany(single.Type) ?
					Success.Create(() => CreateInstance(this, registration, single.Type, single.DependencyChain)) :
					Fail.Create(FailureReason.MarkedAsMany, query.DependencyChain);

			case SingleInstanceQuery single
				when Parent.TryLazyResolve(single with { FromChildResolver = true }) is Success success:
				return success;

			case ManyInstancesQuery many when ShouldNotBeResolvedAsMany(many.ElementType):
				return Fail.Create(FailureReason.NotMarkedAsMany, query.DependencyChain);

			case ManyInstancesQuery many:
				return Success.Create(() =>
					CreateListOfInstances(this, LocalRegistrations, many.ElementType, many.DependencyChain));

			default:
				return Fail.Create(FailureReason.NotRegistered, query.DependencyChain);
		}
	}

	private bool ShouldNotBeResolvedAsMany(Type type) => !TypesResolvableAsMany.Contains(type);

	private static object CreateListOfInstances(
		IObjectResolver resolver,
		IReadOnlyList<Registration> registrations,
		Type elementType,
		IReadOnlyList<Type> dependencyChain)
	{
		Type listType = typeof(List<>).MakeGenericType(elementType);
		var instancesList = (IList)Activator.CreateInstance(listType)!;

		foreach (var registration in registrations.Where(r => r.Matches(elementType)))
		{
			var instance = CreateInstance(resolver, registration, elementType, dependencyChain);
			instancesList.Add(instance);
		}

		return instancesList;
	}

	private static object CreateInstance(
		IObjectResolver objectResolver,
		Registration registration,
		Type requestedType,
		IReadOnlyList<Type> dependencyChain)
	{
		Type implementationType = registration.ImplementationType.IsGenericTypeDefinition ?
			registration.ImplementationType.MakeGenericType(requestedType.GenericTypeArguments) :
			registration.ImplementationType;

		return registration.Provider.GetInstance(
			objectResolver,
			implementationType,
			dependencyChain);
	}
}