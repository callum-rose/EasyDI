namespace EasyDI.Registering;

public static class TypeExtensions
{
	public static bool IsAssignableFromGeneric(this Type to, Type from)
	{
		if (!to.IsGenericTypeDefinition || !from.IsGenericType)
		{
			return to.IsAssignableFrom(from);
		}

		var currentType = from;
		while (currentType != null && currentType != typeof(object))
		{
			if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == to)
			{
				return true;
			}

			currentType = currentType.BaseType;
		}

		foreach (var interfaceType in from.GetInterfaces())
		{
			if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == to)
			{
				return true;
			}
		}

		return false;
	}
}