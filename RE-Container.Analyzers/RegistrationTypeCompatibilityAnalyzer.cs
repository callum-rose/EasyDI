using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace REContainer.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RegistrationTypeCompatibilityAnalyzer : DiagnosticAnalyzer
{
	public static readonly DiagnosticDescriptor TypeNotAssignableRule = new(
		DiagnosticIds.TypeNotAssignable,
		"Type in registration method is not assignable to type in 'As' method",
		"Type '{1}' is not assignable to type '{0}'",
		"Usage",
		DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		description:
		"The type specified in the registration method (RegisterSingleton<T>(), RegisterScoped<T>(), or RegisterTransient<T>()) must be assignable to the type specified in As<T>() or As(typeof(T)).");

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
		ImmutableArray.Create(TypeNotAssignableRule);

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
	}

	private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
	{
		var invocationExpr = (InvocationExpressionSyntax)context.Node;

		// Look for chained method calls like RegisterSingleton<A>().As<IA>() or RegisterSingleton<A>().As(typeof(IA))
		if (invocationExpr.Expression is MemberAccessExpressionSyntax memberAccess &&
		    IsAsMethod(memberAccess.Name))
		{
			// Find the RegisterSingleton call in the chain by looking at the target expression
			var registerCall = FindRegisterCall(memberAccess.Expression);

			if (registerCall != null)
			{
				var semanticModel = context.SemanticModel;

				// Get type arguments
				var registerTypeArg = GetFirstGenericTypeArgument(registerCall, semanticModel);

				if (registerTypeArg != null)
				{
					// Handle single type argument (As<T>() or As(typeof(T)))
					var singleAsTypeArg = GetAsTypeArgument(invocationExpr, semanticModel);

					if (singleAsTypeArg != null)
					{
						CheckTypeCompatibility(context,
							registerCall,
							registerTypeArg,
							singleAsTypeArg,
							GetAsTypeArgumentLocation(invocationExpr));
					}
					else
					{
						// Handle multiple type arguments (As(typeof(T1), typeof(T2), ...))
						var multipleAsTypeArgs = GetAsTypeArguments(invocationExpr, semanticModel);

						foreach (var (typeSymbol, location) in multipleAsTypeArgs)
						{
							CheckTypeCompatibility(context, registerCall, registerTypeArg, typeSymbol, location);
						}
					}
				}
			}
		}
	}

	private static void CheckTypeCompatibility(SyntaxNodeAnalysisContext context,
		InvocationExpressionSyntax registerCall,
		ITypeSymbol registerTypeArg,
		ITypeSymbol asTypeArg,
		Location location)
	{
		var semanticModel = context.SemanticModel;

		// Check if asTypeArg is assignable from registerTypeArg
		var conversion = semanticModel.Compilation.HasImplicitConversion(registerTypeArg, asTypeArg);

		if (!conversion)
		{
			var registerMethodName = GetRegisterMethodName(registerCall);
			var diagnostic = Diagnostic.Create(
				TypeNotAssignableRule,
				location,
				asTypeArg.Name,
				registerTypeArg.Name,
				registerMethodName);

			context.ReportDiagnostic(diagnostic);
		}
	}

	private static (ITypeSymbol TypeSymbol, Location Location)[] GetAsTypeArguments(
		InvocationExpressionSyntax invocation,
		SemanticModel semanticModel)
	{
		var results = new List<(ITypeSymbol, Location)>();

		// Check for As(typeof(T1), typeof(T2), ...)
		foreach (var argument in invocation.ArgumentList.Arguments)
		{
			if (argument.Expression is TypeOfExpressionSyntax typeOfExpr)
			{
				var typeSymbol = semanticModel.GetTypeInfo(typeOfExpr.Type).Type;

				if (typeSymbol != null)
				{
					results.Add((typeSymbol, typeOfExpr.GetLocation()));
				}
			}
			// Handle array expressions like new[] { typeof(T1), typeof(T2) }
			else if (argument.Expression is ArrayCreationExpressionSyntax arrayExpr &&
			         arrayExpr.Initializer != null)
			{
				foreach (var expression in arrayExpr.Initializer.Expressions)
				{
					if (expression is TypeOfExpressionSyntax typeOfInArray)
					{
						var typeSymbol = semanticModel.GetTypeInfo(typeOfInArray.Type).Type;

						if (typeSymbol != null)
						{
							results.Add((typeSymbol, typeOfInArray.GetLocation()));
						}
					}
				}
			}
			// Handle implicit array expressions like new[] { typeof(T1), typeof(T2) }
			else if (argument.Expression is ImplicitArrayCreationExpressionSyntax implicitArrayExpr)
			{
				foreach (var expression in implicitArrayExpr.Initializer.Expressions)
				{
					if (expression is TypeOfExpressionSyntax typeOfInImplicitArray)
					{
						var typeSymbol = semanticModel.GetTypeInfo(typeOfInImplicitArray.Type).Type;

						if (typeSymbol != null)
						{
							results.Add((typeSymbol, typeOfInImplicitArray.GetLocation()));
						}
					}
				}
			}
		}

		return results.ToArray();
	}

	private static bool IsAsMethod(SimpleNameSyntax name)
	{
		return name.Identifier.ValueText == "As";
	}

	private static Location GetAsTypeArgumentLocation(InvocationExpressionSyntax invocation)
	{
		if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
		{
			// Check for generic As<T>()
			if (memberAccess.Name is GenericNameSyntax genericName &&
			    genericName.TypeArgumentList.Arguments.Count > 0)
			{
				return genericName.TypeArgumentList.Arguments[0].GetLocation();
			}
			// Check for As(typeof(T))
			else if (invocation.ArgumentList.Arguments.Count > 0)
			{
				var firstArg = invocation.ArgumentList.Arguments[0];

				if (firstArg.Expression is TypeOfExpressionSyntax typeOfExpr)
				{
					return typeOfExpr.GetLocation();
				}
			}
		}

		// Fallback to the As method location if we can't find the type argument
		if (invocation.Expression is MemberAccessExpressionSyntax fallbackMemberAccess)
		{
			return fallbackMemberAccess.Name.GetLocation();
		}

		return invocation.GetLocation();
	}

	private static ITypeSymbol GetAsTypeArgument(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
	{
		if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
		{
			// Check for generic As<T>()
			if (memberAccess.Name is GenericNameSyntax genericName)
			{
				return GetFirstGenericTypeArgument(genericName, semanticModel);
			}
			// Check for As(typeof(T)) - single argument only
			else if (invocation.ArgumentList.Arguments.Count == 1)
			{
				var firstArg = invocation.ArgumentList.Arguments[0];

				if (firstArg.Expression is TypeOfExpressionSyntax typeOfExpr)
				{
					return semanticModel.GetTypeInfo(typeOfExpr.Type).Type;
				}
			}
		}

		return null;
	}

	private static InvocationExpressionSyntax FindRegisterCall(SyntaxNode node)
	{
		// For RegisterSingleton<A>().As<string>(), the node is the RegisterSingleton<A>() invocation
		if (node is InvocationExpressionSyntax directInvocation &&
		    IsRegisterMethod(directInvocation))
		{
			return directInvocation;
		}

		// Handle more complex chains by walking the syntax tree
		var current = node;

		while (current != null)
		{
			switch (current)
			{
				case InvocationExpressionSyntax invocation when IsRegisterMethod(invocation):
					return invocation;
				case MemberAccessExpressionSyntax memberAccess:
					current = memberAccess.Expression;
					break;
				case InvocationExpressionSyntax inv:
					current = inv.Expression;
					break;
				default:
					return null;
			}
		}

		return null;
	}

	private static bool IsRegisterMethod(InvocationExpressionSyntax invocation)
	{
		if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
		{
			// Handle explicit generic syntax: RegisterSingleton<T>()
			if (memberAccess.Name is GenericNameSyntax genericName)
			{
				var methodName = genericName.Identifier.ValueText;
				return methodName == "RegisterSingleton" ||
				       methodName == "RegisterScoped" ||
				       methodName == "RegisterTransient";
			}

			// Handle implicit generic syntax: RegisterSingleton(...)
			if (memberAccess.Name is IdentifierNameSyntax identifierName)
			{
				var methodName = identifierName.Identifier.ValueText;
				return methodName == "RegisterSingleton" ||
				       methodName == "RegisterScoped" ||
				       methodName == "RegisterTransient";
			}
		}

		return false;
	}

	private static ITypeSymbol GetFirstGenericTypeArgument(GenericNameSyntax genericName,
		SemanticModel semanticModel)
	{
		if (genericName.TypeArgumentList.Arguments.Count > 0)
		{
			var typeArg = genericName.TypeArgumentList.Arguments[0];
			return semanticModel.GetTypeInfo(typeArg).Type;
		}

		return null;
	}

	private static ITypeSymbol GetFirstGenericTypeArgument(InvocationExpressionSyntax invocation,
		SemanticModel semanticModel)
	{
		if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
		    memberAccess.Name is GenericNameSyntax genericName)
		{
			return GetFirstGenericTypeArgument(genericName, semanticModel);
		}

		// Handle implicit generic type from lambda expression
		if (invocation.Expression is MemberAccessExpressionSyntax memberAccessNonGeneric)
		{
			var methodSymbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

			if (methodSymbol?.TypeArguments.Length > 0)
			{
				return methodSymbol.TypeArguments[0];
			}
		}

		return null;
	}

	private static string GetRegisterMethodName(InvocationExpressionSyntax invocation)
	{
		if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
		    memberAccess.Name is GenericNameSyntax genericName)
		{
			return genericName.Identifier.ValueText;
		}

		return "Register";
	}
}