using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace REContainer.Resolving;

internal static class TypeCache
{
	private static readonly ConcurrentDictionary<Type, Type?> ListableAndElementsTypes = new();
	
	public static bool GetIsListable(Type type, [NotNullWhen(true)] out Type? elementType)
	{
		if (ListableAndElementsTypes.TryGetValue(type, out Type? cachedElementType))
		{
			elementType = cachedElementType;
			return elementType != null;
		}

		var isListable = GetIsListableInternal(type, out elementType);
		ListableAndElementsTypes[type] = elementType;
		return isListable;
	}
	
	private static bool GetIsListableInternal(Type type, [NotNullWhen(true)] out Type? elementType)
	{
		if (!type.IsGenericType)
		{
			elementType = null;
			return false;
		}

		var genericTypeDefinition = type.GetGenericTypeDefinition();

		if (genericTypeDefinition == typeof(IReadOnlyList<>) ||
		    genericTypeDefinition == typeof(IReadOnlyCollection<>) ||
		    genericTypeDefinition == typeof(IEnumerable<>))
		{
			elementType = type.GetGenericArguments()[0];
			return true;
		}
		
		elementType = null;
		return false;
	}
}