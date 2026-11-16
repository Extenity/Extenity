#if ExtenityInstantiator

using Extenity.InspectorToolbox.Editor;
using UnityEditor;

namespace Extenity.GameObjectToolbox.Editor
{

	/// <summary>
	/// The inspector of <see cref="Instantiator"/>: what the component does, then its configuration and its three
	/// prefab lists, each with an explanation. Nothing can be changed on a prefab instance: an instantiator is
	/// configured in its prefab. The parent must be an object of a scene.
	/// </summary>
	[CustomEditor(typeof(Instantiator))]
	[CanEditMultipleObjects]
	public sealed class InstantiatorInspector : ObjectInspector<Instantiator>
	{
		internal const string TypeInfo =
			"Instantiator is the system that initializes subsystems of the application. The basic usage is you create Instantiator prefabs in the project and put them inside scenes. The Instantiators then initialize all subsystems as the first operation before any other scripts.\n\n" +
			"The initialization happens in Awake of Instantiator. So the Instantiator script should be configured so that it has the highest priority in Script Execution Order, before all other scripts.";

		internal const string OrderInfo =
			"You can have more than one <i>Instantiator</i> in an application to be able to instantiate subsystems when required in different stages of the application.\n\n" +
			"If there are multiple instantiators trying to be created, these instantiators will be lined up by <i>Order</i> number. Instantiation will be postponed until every other instantiator that has lesser <i>Order</i> number completes their instantiation first.\n\n" +
			"Note that there can be no gaps. All instantiators should be ordered consecutively.";

		internal const string ParentInfo =
			"The optional parent object to be set for instantiated prefabs. The instantiated objects will be put at the top level hierarchy of active scene if no parent is provided.\n\n" +
			"But note that all <i>Subsystem Objects</i> (and maybe some <i>Volatile Objects</i> and <i>Instant Objects</i>) are directly marked with DontDestroyOnLoad, so they are instantly put to the DontDestroyOnLoad scene the moment they are created.";

		internal const string SubsystemPrefabsInfo =
			"The prefabs in this list will be instantiated only once throughout the life time of the application, no matter how many times the instantiator is created in different scenes.\n\n" +
			"The singletons of the application, designed in the form of prefabs, should be instantiated this way.\n\n" +
			"Instantiation order is ensured, so a prefab of <i>The Order 2 Instantiator</i> will always be instantiated after <i>The Order 1 Instantiator</i>.\n\n" +
			"Instantiated objects will be marked with DontDestroyOnLoad, since they expected to be singletons of the application and they will be created only once.";

		internal const string VolatilePrefabsInfo =
			"The prefabs in this list will be instantiated whenever the instantiator is created. Consider instantiating the objects this way if they have a lifetime of only one scene.\n\n" +
			"An example would be defining an Ingame Instantiator with an ingame UI and a score calculator assigned in its <i>Volatile Prefabs</i>. Put the Ingame Instantiator only into game scenes. So the UI and score calculator would be instantiated every time a game scene is launched, and they will be destroyed while changing the scene.\n\n" +
			"The instantiation order of multiple instantiators is respected just like <i>Subsystem Prefabs</i>. <i>Volatile Prefabs</i> are always instantiated after <i>Subsystem Prefabs</i> of its own Instantiator.";

		internal const string VolatilePrefabsNotImplemented = "CAUTION! Not implemented yet! Will only instantiate once and will not instantiate in consecutive scenes.";

		internal const string InstantPrefabsInfo =
			"The prefabs in this list will be instantiated immediately when the Awake of this instantiator gets called.\n\n" +
			"<i>Instant Prefabs</i> instantiated before <i>Subsystem Prefabs</i> and <i>Volatile Prefabs</i> of this instantiator, but may or may not be instantiated after other previously <i>Ordered</i> instantiators and their prefabs. So the instantiation order for <i>Instant Prefabs</i> is a chaos. Any system that is instantiated via <i>Instant Prefabs</i> should not depend on/try to access other systems.\n\n" +
			"Note that the instantiation order of multiple instantiators will NOT be respected. So a prefab of <i>The Order 2 Instantiator</i> may get instantiated before a prefab of <i>The Order 1 Instantiator</i>.\n\n" +
			"You may want to consider using <i>Volatile Prefabs</i> instead, since it is basically the same but also provides <i>Ordered</i> instantiation.\n\n" +
			"(Previously known as Nonlasting Prefabs)";

		protected override void Configure(InspectorBuilder<Instantiator> b)
		{
			b.HideScriptField();
			b.Info(TypeInfo).AtTop();

			b.Group("Configuration", GroupStyle.Box, g =>
			{
				g.Info(OrderInfo);
				g.Fields(x => x.Order);
				g.Info(ParentInfo);
				g.Fields(x => x.Parent);
			});
			b.Field(x => x.Order).EnabledWhen(IsNotPrefabInstance);
			b.Field(x => x.Parent).EnabledWhen(IsNotPrefabInstance)
			 .Validate(parent => !parent || !EditorUtility.IsPersistent(parent), "The parent must be an object of a scene, not an asset.");

			b.Group("Prefabs", GroupStyle.Box, g =>
			{
				g.Info(SubsystemPrefabsInfo);
				g.Fields(x => x.SubsystemPrefabs);
				g.Info(VolatilePrefabsInfo);
				g.Info(VolatilePrefabsNotImplemented, MessageSeverity.Error);
				g.Fields(x => x.VolatilePrefabs);
				g.Info(InstantPrefabsInfo);
				g.Fields(x => x.InstantPrefabs);
			});
			b.List(x => x.SubsystemPrefabs).ExpandedByDefault().EnabledWhen(IsNotPrefabInstance);
			b.List(x => x.VolatilePrefabs).ExpandedByDefault().EnabledWhen(IsNotPrefabInstance);
			b.List(x => x.InstantPrefabs).ExpandedByDefault().EnabledWhen(IsNotPrefabInstance);
		}

		private static bool IsNotPrefabInstance(Instantiator instantiator, InspectorContext context) => !context.IsPrefabInstance;
	}

}

#endif
