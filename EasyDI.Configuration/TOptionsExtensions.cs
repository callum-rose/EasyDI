using Microsoft.Extensions.Options;

namespace EasyDI.Configuration;

internal static class TOptionsExtensions
{
	public static IOptions<TOptions> ToOptions<TOptions>(this TOptions options) where TOptions : class =>
		Options.Create(options);
}