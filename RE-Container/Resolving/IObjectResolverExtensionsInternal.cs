using REContainer.Exceptions;

namespace REContainer.Resolving;

internal static class IObjectResolverExtensionsInternal
{
	public static IObjectResolver WithAdditionalArguments(this IObjectResolver resolver,
		IReadOnlyList<ArgumentInfo> arguments)
	{
		resolver.Validate(arguments);

		return new ObjectResolver(resolver.LocalRegistrations,
			resolver.Parent,
			resolver.TypesResolvableAsMany,
			arguments);
	}

	public static IReadOnlyList<Registration> CloneScopedRegistrations(this IObjectResolver resolver)
	{
		return resolver.LocalRegistrations
			.Where(r => r.Lifetime is Lifetime.Scoped)
			.Select(r => r with { Provider = r.Provider.Clone() })
			.ToArray();
	}

	private static void Validate(this IObjectResolver resolver, IReadOnlyList<ArgumentInfo> arguments)
	{
		var errors = new List<string>();

		var duplicateArgumentTypes = arguments
			.GroupBy(a => a.Type)
			.Where(g => g.Count() > 1)
			.ToArray();

		foreach (var group in duplicateArgumentTypes)
		{
			errors.Add(
				$"Argument type '{group.Key.FullName}' was registered {group.Count()} times. " +
				"Each additional argument type can only be registered once");
		}

		var conflictingArguments = resolver.GetAllResolvableTypes()
			.Distinct()
			.Concat(arguments.Select(a => a.Type))
			.GroupBy(t => t)
			.Where(g => g.Count() > 1)
			.ToArray();

		foreach (var grouping in conflictingArguments)
		{
			errors.Add(
				$"Type '{grouping.Key.FullName}' is already registered in the resolver. " +
				"Cannot register it as an additional argument");
		}

		if (errors.Count > 0)
		{
			throw new RegistrationException(errors);
		}
	}
}