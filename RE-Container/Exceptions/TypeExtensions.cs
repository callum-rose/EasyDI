namespace REContainer.Exceptions;

internal static class TypeExtensions
{
	internal static string Format(this Type type)
	{
		if (!type.IsGenericType)
		{
			return type.Name;
		}

		var genericArgs = string.Join(", ", type.GetGenericArguments().Select(Format));
		var typeName = type.Name[..type.Name.IndexOf('`')];
		return $"{typeName}<{genericArgs}>";
	}
}