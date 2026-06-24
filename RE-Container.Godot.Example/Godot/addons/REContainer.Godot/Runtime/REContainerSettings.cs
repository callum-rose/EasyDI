using Godot;

namespace REContainer.Godot;

[GlobalClass]
public partial class REContainerSettings : Resource
{
	[Export] private PackedScene applicationScopePrefab = null!;
	[Export] private PackedScene? sessionLifetimeScope;
	[Export] private PackedScene? gameLifetimeScope;
	
	private const string PathToContainerSettings= "res://RE Container Settings.tres";

	public static PackedScene ApplicationScopePrefab => Instance.applicationScopePrefab;
	public static PackedScene? SessionLifetimeScope => Instance.sessionLifetimeScope;
	public static PackedScene? GameLifetimeScope => Instance.gameLifetimeScope;

	private static REContainerSettings Instance
	{
		get
		{
			if (_instance != null)
			{
				return _instance;
			}

			_instance = GD.Load<REContainerSettings>(PathToContainerSettings);

			if (_instance == null)
			{
				throw new System.Exception(
					"REContainerSettings asset not found in Root folder. Please create one via Resources");
			}

			return _instance;
		}
	}

	private static REContainerSettings? _instance;
}