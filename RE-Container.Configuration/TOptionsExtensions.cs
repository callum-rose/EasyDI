using Microsoft.Extensions.Options;

namespace REContainer.Configuration;

internal static class TOptionsExtensions
{
	public static IOptions<TOptions> ToOptions<TOptions>(this TOptions options) where TOptions : class =>
		Options.Create(options);
}