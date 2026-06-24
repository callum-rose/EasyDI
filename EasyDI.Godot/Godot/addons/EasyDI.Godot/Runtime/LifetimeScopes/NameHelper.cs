using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyDI.Godot.LifetimeScopes;

internal static class NameHelper
{
	private static IReadOnlyDictionary<Type, string> LifetimeScopeTypeNames { get; } = AppDomain.CurrentDomain
		.GetAssemblies()
		.SelectMany(a => a.GetTypes())
		.Where(t => !t.IsAbstract)
		.Where(t => typeof(LifetimeScope).IsAssignableFrom(t))
		.Where(t => !t.Namespace?.Contains("Test") ?? true)
		.ToDictionary(
			t => t,
			t => t.Name.Replace("LifetimeScope", string.Empty));

	public static IReadOnlyList<string> ParentableLifetimeScopeNames { get; } =
		LifetimeScopeTypeNames.Where(kv => kv.Key != typeof(SceneLifetimeScope)).Select(kv => kv.Value).ToArray();

	public static string GetName<T>() where T : LifetimeScope
	{
		return LifetimeScopeTypeNames[typeof(T)];
	}

	public static Type GetTypeByName(string name)
	{
		return LifetimeScopeTypeNames.FirstOrDefault(kv => kv.Value == name).Key ??
		       throw new ArgumentException($"No scope type found with name '{name}'.", nameof(name));
	}
}