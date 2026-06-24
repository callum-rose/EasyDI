namespace EasyDI.Resolving;

public interface IObjectResolver
{
	internal IObjectResolver Parent { get; }
	internal IReadOnlyList<Registration> LocalRegistrations { get; }
	internal IReadOnlyCollection<Type> TypesResolvableAsMany { get; }

	ResolveResult TryLazyResolve(ResolutionQuery query);
}