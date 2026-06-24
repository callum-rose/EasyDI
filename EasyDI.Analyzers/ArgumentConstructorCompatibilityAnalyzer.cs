using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EasyDI.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ArgumentConstructorCompatibilityAnalyzer : DiagnosticAnalyzer
{
	public static readonly DiagnosticDescriptor ArgumentNotFoundInConstructorRule = new DiagnosticDescriptor(
		DiagnosticIds.ArgumentNotFoundInConstructor,
		"Parameter type not found in any constructor of the registered type",
		"A parameter of type '{0}' is not found in any constructor of the registered type '{1}'",
		"Usage",
		DiagnosticSeverity.Warning,
		isEnabledByDefault: true,
		description:
		"The argument type specified in WithArgument<T>() or WithArgument(Type, object) should match a parameter type in at least one constructor of the registered type.");

	private static readonly DiagnosticDescriptor ConflictingArgumentRule = new(
		DiagnosticIds.ConflictingArgument,
		"Conflicting explicit argument with registered dependency",
		"Type '{0}' is provided via WithArgument but is already registered in the container",
		"Usage",
		DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		description:
		"An explicit argument type conflicts with a registered type, which will cause a runtime exception.");

	private static readonly DiagnosticDescriptor DuplicateArgumentRule = new(
		DiagnosticIds.DuplicateArgument,
		"Duplicate explicit argument type",
		"Type '{0}' is provided multiple times via WithArgument on the same registration. If the registered type has a constructor with multiple parameters of this type, use a factory to construct it instead.",
		"Usage",
		DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		description: "The same argument type is provided multiple times, which will cause a runtime exception.");

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
		ImmutableArray.Create<DiagnosticDescriptor>()
			.Add(ArgumentNotFoundInConstructorRule)
			.Add(ConflictingArgumentRule)
			.Add(DuplicateArgumentRule);

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
	}

	private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
	{
		var methodDeclaration = (MethodDeclarationSyntax)context.Node;

		// Track registered types per registry (builder) symbol
		var registeredTypesPerRegistry = new Dictionary<ISymbol, HashSet<ITypeSymbol>>(SymbolEqualityComparer.Default);
		var registrationChains = new List<RegistrationChain>();

		foreach (var statement in methodDeclaration.DescendantNodes().OfType<ExpressionStatementSyntax>())
		{
			var expression = statement.Expression;

			// Parse chains with WithArgument (captures root symbol & registered type)
			var chain = ParseRegistrationChain(expression, context.SemanticModel);

			if (chain != null)
			{
				registrationChains.Add(chain);

				if (chain.RegisteredType != null && chain.RootSymbol != null)
				{
					if (!registeredTypesPerRegistry.TryGetValue(chain.RootSymbol, out var set))
					{
						set = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
						registeredTypesPerRegistry.Add(chain.RootSymbol, set);
					}

					set.Add(chain.RegisteredType);
				}

				continue;
			}

			// Handle pure registration statements (no WithArgument chained)
			if (IsRegistrationCall(expression, context.SemanticModel, out var registeredType, out var rootSymbol))
			{
				if (registeredType != null && rootSymbol != null)
				{
					if (!registeredTypesPerRegistry.TryGetValue(rootSymbol, out var set))
					{
						set = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
						registeredTypesPerRegistry.Add(rootSymbol, set);
					}

					set.Add(registeredType);
				}
			}
		}

		// Analyze each registration chain independently per registry symbol
		foreach (var chain in registrationChains)
		{
			if (chain.RegisteredType == null)
				continue;

			var constructors = chain.RegisteredType.GetMembers()
				.OfType<IMethodSymbol>()
				.Where(m => m.MethodKind == MethodKind.Constructor && !m.IsStatic)
				.ToList();

			// Duplicate explicit argument types in same chain
			var duplicateGroups = chain.ArgumentTypes
				.GroupBy(t => t, SymbolEqualityComparer.Default)
				.Where(g => g.Count() > 1);

			foreach (var group in duplicateGroups)
			{
				var locations = chain.ArgumentLocations
					.Where((loc, i) => SymbolEqualityComparer.Default.Equals(chain.ArgumentTypes[i], group.Key))
					.ToList();

				foreach (var location in locations.Skip(1))
				{
					context.ReportDiagnostic(Diagnostic.Create(
						DuplicateArgumentRule,
						location,
						group.Key.ToDisplayString()));
				}
			}

			// Conflicts only against types registered on the same registry symbol
			var localRegisteredTypes = chain.RootSymbol != null &&
			                           registeredTypesPerRegistry.TryGetValue(chain.RootSymbol, out var set) ?
				set :
				[];

			for (int i = 0; i < chain.ArgumentTypes.Count; i++)
			{
				var argumentType = chain.ArgumentTypes[i];
				var location = chain.ArgumentLocations[i];

				if (localRegisteredTypes.Contains(argumentType))
				{
					context.ReportDiagnostic(Diagnostic.Create(
						ConflictingArgumentRule,
						location,
						argumentType.ToDisplayString()));
				}

				var matchesAnyCtorParam = constructors.Any(ctor =>
					ctor.Parameters.Any(p => SymbolEqualityComparer.Default.Equals(p.Type, argumentType)));

				if (!matchesAnyCtorParam)
				{
					context.ReportDiagnostic(Diagnostic.Create(
						ArgumentNotFoundInConstructorRule,
						location,
						argumentType.ToDisplayString(),
						chain.RegisteredType.ToDisplayString()));
				}
			}
		}
	}

	private bool IsRegistrationCall(ExpressionSyntax expression,
		SemanticModel semanticModel,
		out ITypeSymbol registeredType,
		out ISymbol rootSymbol)
	{
		registeredType = null;
		rootSymbol = null;

		if (expression is not InvocationExpressionSyntax invocation)
			return false;

		if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
			return false;

		var methodName = memberAccess.Name.Identifier.Text;
		if (methodName is not ("RegisterSingleton" or "RegisterScoped" or "RegisterTransient" or "RegisterInstance"))
			return false;

		rootSymbol = semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol;

		// Generic form
		if (memberAccess.Name is GenericNameSyntax genericName &&
		    genericName.TypeArgumentList.Arguments.Count > 0)
		{
			var typeSyntax = genericName.TypeArgumentList.Arguments[0];
			registeredType = semanticModel.GetTypeInfo(typeSyntax).Type;
			return registeredType != null;
		}

		// typeof(T) argument
		if (invocation.ArgumentList.Arguments.Count > 0)
		{
			var firstArg = invocation.ArgumentList.Arguments[0].Expression;

			if (firstArg is TypeOfExpressionSyntax typeOfExpr)
			{
				registeredType = semanticModel.GetTypeInfo(typeOfExpr.Type).Type;
				return registeredType != null;
			}
		}

		return false;
	}

	private RegistrationChain ParseRegistrationChain(ExpressionSyntax expression, SemanticModel semanticModel)
	{
		var argumentTypes = new List<ITypeSymbol>();
		var argumentLocations = new List<Location>();
		ITypeSymbol registeredType = null;
		ISymbol rootSymbol = null;
		var current = expression;

		while (current is InvocationExpressionSyntax invocation &&
		       invocation.Expression is MemberAccessExpressionSyntax memberAccess)
		{
			var methodName = memberAccess.Name.Identifier.Text;

			if (methodName == "WithArgument")
			{
				ITypeSymbol argType = null;
				Location argLocation = null;

				// Check if WithArgument has explicit generic type argument
				if (memberAccess.Name is GenericNameSyntax genericName &&
				    genericName.TypeArgumentList.Arguments.Count > 0)
				{
					var typeSyntax = genericName.TypeArgumentList.Arguments[0];
					argType = semanticModel.GetTypeInfo(typeSyntax).Type;
					argLocation = typeSyntax.GetLocation();
				}
				// Otherwise infer from the argument expression
				else if (invocation.ArgumentList.Arguments.Count > 0)
				{
					var argExpr = invocation.ArgumentList.Arguments[0].Expression;
					argType = semanticModel.GetTypeInfo(argExpr).Type;
					argLocation = argExpr.GetLocation();
				}

				if (argType != null && argLocation != null)
				{
					argumentTypes.Add(argType);
					argumentLocations.Add(argLocation);
				}

				current = memberAccess.Expression;
				continue;
			}

			if (methodName == "As")
			{
				// Skip As() calls and continue walking the chain
				current = memberAccess.Expression;
				continue;
			}

			if (methodName is "RegisterSingleton" or "RegisterScoped" or "RegisterTransient")
			{
				rootSymbol = semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol;

				if (memberAccess.Name is GenericNameSyntax genericName &&
				    genericName.TypeArgumentList.Arguments.Count > 0)
				{
					var typeSyntax = genericName.TypeArgumentList.Arguments[0];
					registeredType = semanticModel.GetTypeInfo(typeSyntax).Type;
				}

				break;
			}

			break;
		}

		return argumentTypes.Count > 0 ?
			new RegistrationChain(registeredType, rootSymbol, argumentTypes, argumentLocations) :
			null;
	}

	private sealed class RegistrationChain
	{
		public ITypeSymbol RegisteredType { get; }
		public ISymbol RootSymbol { get; }
		public List<ITypeSymbol> ArgumentTypes { get; }
		public List<Location> ArgumentLocations { get; }

		public RegistrationChain(ITypeSymbol registeredType,
			ISymbol rootSymbol,
			List<ITypeSymbol> argumentTypes,
			List<Location> argumentLocations)
		{
			RegisteredType = registeredType;
			RootSymbol = rootSymbol;
			ArgumentTypes = argumentTypes;
			ArgumentLocations = argumentLocations;
		}
	}
}