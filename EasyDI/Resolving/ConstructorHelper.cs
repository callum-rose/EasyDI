using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using EasyDI.Exceptions;

namespace EasyDI.Resolving;

internal static class ConstructorHelper
{
	public static bool GetMostRelevant(Type type,
		IObjectResolver resolver,
		[NotNullWhen(true)] out ConstructorInfo? constructorInfo)
	{
		constructorInfo = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
			.Where(c => c.GetParameters().All(p => resolver.CanResolve(p.ParameterType)))
			.OrderBy(c => c.GetParameters().Length)
			.LastOrDefault();
		return constructorInfo != null;
	}

	public static IReadOnlyList<ConstructorDiagnostic> GetAllConstructorDiagnostics(Type type, IObjectResolver resolver)
	{
		return type.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
			.Select(ctor =>
				new ConstructorDiagnostic(
					ctor,
					ctor.GetParameters()
						.Select(p => CreateParameterDiagnostic(p.ParameterType, resolver))
						.ToArray()))
			.ToArray();
	}

	private static ParameterDiagnostic CreateParameterDiagnostic(Type parameterType, IObjectResolver resolver)
	{
		bool canResolve = resolver.CanResolve(parameterType);
		string? reason = canResolve ? null : "not registered";
		return new ParameterDiagnostic(parameterType, canResolve, reason);
	}
}