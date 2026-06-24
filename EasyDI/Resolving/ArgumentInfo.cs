using System.Diagnostics.CodeAnalysis;

namespace EasyDI.Resolving;

public record ArgumentInfo(Type Type, object Value);

public static class ArgumentInfoExtensions
{
	public static ArgumentInfo ToArgumentInfo<T>(this T value) => new(typeof(T), value!);

	public static bool TryFind(this IEnumerable<ArgumentInfo> arguments,
		Type type,
		[NotNullWhen(true)] out ArgumentInfo? argument)
	{
		argument = arguments.FirstOrDefault(a => a.Type == type);
		return argument is not null;
	}
}