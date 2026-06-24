using REContainer.Registering.RegistrationBuilders;
using REContainer.Resolving;

namespace REContainer.Registering;

public static class RegistrationBuilderExtensions
{
	public static RegistrationBuilder As<TResolvable>(this RegistrationBuilder registrationBuilder) 
		where TResolvable : class
	{
		return registrationBuilder.As(typeof(TResolvable));
	}

	public static RegistrationBuilder As(this RegistrationBuilder registrationBuilder,
		params IEnumerable<Type> resolvableTypes)
	{
		registrationBuilder.DeclaredCertainResolvableTypes.UnionWith(resolvableTypes);
		return registrationBuilder;
	}

	public static RegistrationBuilder As(this RegistrationBuilder registrationBuilder, Type resolvableType)
	{
		registrationBuilder.DeclaredCertainResolvableTypes.Add(resolvableType);
		return registrationBuilder;
	}
	
	public static RegistrationBuilder TryAs<TResolvable>(this RegistrationBuilder registrationBuilder)
	 		where TResolvable : class
	{
		return registrationBuilder.TryAs(typeof(TResolvable));
	}
	
	public static RegistrationBuilder TryAs(this RegistrationBuilder registrationBuilder, Type MaybeResolvableType)
	{
		registrationBuilder.DeclaredPotentiallyResolvableTypes.Add(MaybeResolvableType);
		return registrationBuilder;
	}

	public static RegistrationBuilder WithArgument<TArgument>(this RegistrationBuilder registrationBuilder,
		TArgument argument)
	{
		return registrationBuilder.WithArgument(typeof(TArgument), argument);
	}

	public static RegistrationBuilder WithArgument(this RegistrationBuilder registrationBuilder,
		Type type,
		object argument)
	{
		registrationBuilder.Arguments.Add(new ArgumentInfo(type, argument));
		return registrationBuilder;
	}
}