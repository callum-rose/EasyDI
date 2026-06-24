using System.Diagnostics.CodeAnalysis;

namespace EasyDI.Resolving;

internal static class RegistrationExtensions
{
	public static bool TryFind(this IEnumerable<Registration> registrations,
		Type type,
		[NotNullWhen(true)] out Registration? registration)
	{
		registration = registrations.LastOrDefault(r => r.Matches(type));
		return registration is not null;
	}

	public static bool Matches(this Registration registration, Type type)
	{
		if (registration.ResolvableTypes.Contains(type))
		{
			return true;
		}

		if (!type.IsGenericType)
		{
			return false;
		}

		var openGenericType = type.GetGenericTypeDefinition();
		return registration.ResolvableTypes.Contains(openGenericType);
	}
}