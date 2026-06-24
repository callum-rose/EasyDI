using Godot;

namespace EasyDI.Godot;

[GlobalClass]
public partial class EasyDISettings : Resource
{
	[Export] private PackedScene applicationScopePrefab = null!;
	[Export] private PackedScene? sessionLifetimeScope;
	[Export] private PackedScene? gameLifetimeScope;
	
	private const string PathToContainerSettings= "res://EasyDI Settings.tres";

	public static PackedScene ApplicationScopePrefab => Instance.applicationScopePrefab;
	public static PackedScene? SessionLifetimeScope => Instance.sessionLifetimeScope;
	public static PackedScene? GameLifetimeScope => Instance.gameLifetimeScope;

	private static EasyDISettings Instance
	{
		get
		{
			if (_instance != null)
			{
				return _instance;
			}

			_instance = GD.Load<EasyDISettings>(PathToContainerSettings);

			if (_instance == null)
			{
				throw new System.Exception(
					"EasyDISettings asset not found in Root folder. Please create one via Resources");
			}

			return _instance;
		}
	}

	private static EasyDISettings? _instance;
}