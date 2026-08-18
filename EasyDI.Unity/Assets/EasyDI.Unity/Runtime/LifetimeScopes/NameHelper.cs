using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EasyDI.Unity.LifetimeScopes
{
	internal static class NameHelper
	{
		private static IReadOnlyDictionary<Type, string> LifetimeScopeTypeNames { get; } = AppDomain.CurrentDomain
			.GetAssemblies()
			.SelectMany(GetTypes)
			.Where(t => !t.IsAbstract)
			.Where(t => !t.IsGenericType)
			.Where(t => typeof(LifetimeScope).IsAssignableFrom(t))
			.Where(t => !t.Namespace?.Contains("Test") ?? true)
			.ToDictionary(
				t => t,
				t => t.Name.Replace("LifetimeScope", string.Empty));

		internal static IReadOnlyCollection<Type> ParentableLifetimeScopeTypes { get; } = LifetimeScopeTypeNames.Keys
			.Where(t => t != typeof(SceneLifetimeScope))
			.ToArray();

		public static IReadOnlyList<string> ParentableLifetimeScopeNames { get; } = ParentableLifetimeScopeTypes
			.Select(t => LifetimeScopeTypeNames[t])
			.ToArray();

		public static Type GetTypeByName(string name)
		{
			var matches = FindTypesByName(name);

			return matches.Length switch
			{
				1 => matches[0],
				0 => throw new ArgumentException(
					$"No lifetime scope named '{name}' exists. Known scopes: " +
					$"{string.Join(", ", ParentableLifetimeScopeNames)}.",
					nameof(name)),
				_ => throw new ArgumentException(
					$"The name '{name}' is ambiguous; it matches " +
					$"{string.Join(", ", matches.Select(t => t.FullName))}. Rename one of them so that scopes can be " +
					"told apart by name.",
					nameof(name))
			};
		}

		/// <summary>
		/// Resolves a scope name without throwing, for callers such as inspectors that must tolerate stale names.
		/// </summary>
		/// <returns>True when the name matches exactly one scope type.</returns>
		public static bool TryGetTypeByName(string name, out Type? type)
		{
			var matches = FindTypesByName(name);

			type = matches.Length == 1 ? matches[0] : null;

			return type != null;
		}

		private static Type[] FindTypesByName(string name)
		{
			return LifetimeScopeTypeNames.Where(kv => kv.Value == name).Select(kv => kv.Key).ToArray();
		}

		private static IEnumerable<Type> GetTypes(Assembly assembly)
		{
			try
			{
				return assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException exception)
			{
				// A single unloadable assembly shouldn't stop the rest of the project's scopes being discovered.
				return exception.Types.Where(t => t != null).Select(t => t!);
			}
		}
	}
}
