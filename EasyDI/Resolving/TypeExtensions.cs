using System.Diagnostics.CodeAnalysis;

namespace EasyDI.Resolving;

internal static class TypeExtensions
{
	public static bool IsListable(this Type type, [NotNullWhen(true)] out Type? elementType)
	{
		return TypeCache.GetIsListable(type, out elementType);
	}
}