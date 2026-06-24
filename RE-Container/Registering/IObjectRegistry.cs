using REContainer.Registering.RegistrationBuilders;

namespace REContainer.Registering;

public interface IObjectRegistry
{
	bool HasRegistrationResolvableAs<TResolvable>();

	TRegistrationBuilder Register<TRegistrationBuilder>(TRegistrationBuilder registrationBuilder)
		where TRegistrationBuilder : RegistrationBuilder;
	
	void MarkResolvableAsMany(Type type);
}