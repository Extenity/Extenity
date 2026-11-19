using System;
using System.Reflection;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>
	/// Selects the property handler cache in which Unity remembers the drawers of the properties that are bound
	/// while the scope lives, and puts the earlier selection back when disposed.
	/// <para>
	/// Unity looks up which drawer draws a property once and remembers the answer in a property handler cache.
	/// Every editor owns one and empties it when the editor is destroyed. The Inspector window selects the cache
	/// of an editor when it binds that editor. Whatever is bound while no cache is selected is remembered in one
	/// global cache, which is emptied only when the Inspector window is rebuilt.
	/// </para>
	/// <para>
	/// A cache finds an answer by one number: the instance id of the object combined (xor) with a hash of the
	/// property path. Nothing else is compared. So in the global cache a property can be given the drawer that
	/// was remembered for another property of another object, long after that object was destroyed, whenever
	/// the two numbers happen to be equal. In the cache of one editor only the properties of its own targets
	/// can meet, and everything is forgotten together with the editor.
	/// </para>
	/// </summary>
	public readonly struct PropertyHandlerCacheScope : IDisposable
	{
		private readonly bool HasSelected;
		private readonly object PreviousCache;
		/// <summary>The cache that was made for this scope and is thrown away with it. Null when the cache belongs to someone else.</summary>
		private readonly IDisposable OwnCache;

		private PropertyHandlerCacheScope(object cache, IDisposable ownCache)
		{
			PreviousCache = SelectedCacheField.GetValue(null);
			SelectedCacheField.SetValue(null, cache);
			OwnCache = ownCache;
			HasSelected = true;
		}

		/// <summary>
		/// The cache that is selected now, to select it again later with <see cref="Select"/>. Null while none
		/// is selected: Unity then uses the global cache. The type of a cache is internal to Unity.
		/// </summary>
		public static object Selected => SelectedCacheField.GetValue(null);

		/// <param name="cache">What <see cref="Selected"/> returned earlier. Null selects none.</param>
		public static PropertyHandlerCacheScope Select(object cache)
		{
			if (cache != null && !SelectedCacheField.FieldType.IsInstanceOfType(cache))
				throw new ArgumentException($"A '{cache.GetType().FullName}' is not a property handler cache.", nameof(cache));
			return new PropertyHandlerCacheScope(cache, null);
		}

		/// <summary>Selects the cache of the editor. The editor must be alive.</summary>
		public static PropertyHandlerCacheScope SelectCacheOf(UnityEditor.Editor editor)
		{
			if (editor == null)
				throw new ArgumentNullException(nameof(editor));
			var cache = EditorCacheProperty.GetValue(editor)
			            ?? throw new InvalidOperationException($"The editor '{editor}' has no property handler cache.");
			return new PropertyHandlerCacheScope(cache, null);
		}

		/// <summary>
		/// Selects a new, empty cache that is thrown away when the scope is disposed. For code that binds or
		/// draws properties where no editor is at hand, for example a test with a panel or a window of its own.
		/// </summary>
		public static PropertyHandlerCacheScope SelectNew()
		{
			var cache = (IDisposable)Activator.CreateInstance(SelectedCacheField.FieldType, true);
			return new PropertyHandlerCacheScope(cache, cache);
		}

		public void Dispose()
		{
			if (!HasSelected)
				return;
			SelectedCacheField.SetValue(null, PreviousCache);
			OwnCache?.Dispose();
		}

		#region Unity internals

		private static readonly FieldInfo SelectedCacheField =
			typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.ScriptAttributeUtility", true)
				.GetField("s_CurrentCache", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
			?? throw new MissingFieldException("UnityEditor.ScriptAttributeUtility", "s_CurrentCache");

		private static readonly PropertyInfo EditorCacheProperty =
			typeof(UnityEditor.Editor).GetProperty("propertyHandlerCache", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			?? throw new MissingMemberException("UnityEditor.Editor", "propertyHandlerCache");

		#endregion
	}

}
