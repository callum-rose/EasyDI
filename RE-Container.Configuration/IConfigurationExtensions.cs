using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace REContainer.Configuration;

internal static class IConfigurationExtensions
{
	public static TOptions GetAndValidate<TOptions>(this IConfiguration configuration) where TOptions : class, new()
	{
		var options = configuration.Get<TOptions>() ?? new TOptions();

		var validator = new DataAnnotationValidateOptions<TOptions>(Options.DefaultName);
		var result = validator.Validate(Options.DefaultName, options);

		if (result.Failed)
		{
			throw new OptionsValidationException(Options.DefaultName, typeof(TOptions), result.Failures);
		}

		return options;
	}
}