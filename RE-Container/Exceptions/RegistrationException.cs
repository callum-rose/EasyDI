using System.Text;

namespace REContainer.Exceptions;

/// <summary>
/// Exception thrown when there are errors during the registration or validation phase.
/// Can contain multiple accumulated validation errors.
/// </summary>
internal class RegistrationException : ContainerException
{
	/// <summary>
	/// List of all validation errors found during registration.
	/// </summary>
	public IReadOnlyList<string> ValidationErrors { get; }

	public RegistrationException(string message) : base(message)
	{
		ValidationErrors = [message];
	}

	public RegistrationException(IReadOnlyList<string> validationErrors)
		: base(FormatMessage(validationErrors))
	{
		ValidationErrors = validationErrors;
	}

	private static string FormatMessage(IReadOnlyList<string> errors)
	{
		if (errors.Count == 0)
		{
			return "Registration validation failed but no errors were provided.";
		}

		if (errors.Count == 1)
		{
			return $"Registration validation failed: {errors[0]}";
		}

		var sb = new StringBuilder();
		sb.Append($"Registration validation failed with {errors.Count} error(s):");

		foreach (var error in errors)
		{
			sb.Append($"\n  - {error}");
		}

		return sb.ToString();
	}
}

