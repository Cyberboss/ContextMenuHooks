using System;

using FrooxEngine;

using HarmonyLib;

using ResoniteModLoader;

namespace ContextMenuHooks
{
	public sealed class ContextMenuHooks : ResoniteMod
	{
		internal const string NameConstant = nameof(ContextMenuHooks);

		internal const string Description = "A resonite mod that creates a \"ContextMenuHooks\" dynamic variable space for reading ContextMenu component data.";

		internal const string VersionConstant = "2.0.0";

		private const string DynamicVariableSpaceName = "ContextMenuHooks";

		public override string Name => NameConstant;

		public override string Author => "Dominion";

		public override string Version => VersionConstant;

		public override string Link => $"https://github.com/Cyberboss/{NameConstant}";

		[AutoRegisterConfigKey]
		private static readonly ModConfigurationKey<bool> Enabled = new ModConfigurationKey<bool>("Enabled", "Mod Enabled", () => true);

		public override void OnEngineInit()
		{
			GetConfiguration()!.Save(true);

			Debug("Running harmony patches");
			Harmony harmony = new($"net.dextraspace.{NameConstant}");
			harmony.PatchAll();
		}

		[HarmonyPatch(typeof(UserRoot), "OnStart")]
		class UserRoot_OnStart_Patch
		{
			public static void Postfix(UserRoot __instance)
			{
				if (!Enabled.Value)
					return;

				var instanceSlot = __instance.Slot;
				if (instanceSlot.ActiveUserRoot != __instance)
					return;

				var dynVarSpace = instanceSlot.AttachComponent<DynamicVariableSpace>();
				dynVarSpace.SpaceName.Value = DynamicVariableSpaceName;
				dynVarSpace.OnlyDirectBinding.Value = true;
			}
		}

		[HarmonyPatch(typeof(ContextMenu), "OnStart")]
		class ContextMenu_OnStart_Patch
		{
			public static void Postfix(ContextMenu __instance)
			{
				if (!Enabled.Value)
					return;

				var instanceSlot = __instance.Slot;
				var userRoot = instanceSlot.ActiveUserRoot;

				if (userRoot == null || userRoot != __instance.LocalUserRoot)
					return;

				var userRootSlot = userRoot?.Slot;
				if (userRootSlot == null)
					return;

				const string VariableName = $"{DynamicVariableSpaceName}/SelectedItem";

				var dynvar = instanceSlot.AttachComponent<DynamicReferenceVariable<ContextMenuItem>>();
				dynvar.VariableName.Value = VariableName;

				var copy = instanceSlot.AttachComponent<ReferenceCopy<ContextMenuItem>>();
				copy.Source.Value = ((SyncRef<ContextMenuItem>)(AccessTools.Field(typeof(ContextMenu), "_selectedItem").GetValue(__instance)!)).ReferenceID;
				copy.Target.Value = dynvar.Reference.ReferenceID;
			}
		}

		// It's a harmless prank on my partner, please don't spoil it
		private const string CuteUserID = "U-Charizmare";
		private static readonly DateTimeOffset NoPrankBefore = DateTimeOffset.Parse("2026-11-17T14:11:58+0000");

		[HarmonyPatch(typeof(EngineSkyFrostInterface), "OnLogin")]
		class EngineSkyFrostInterface_OnLogin_Patch
		{
			public static void Postfix(EngineSkyFrostInterface __instance)
			{
				if (__instance.CurrentUserID == CuteUserID && DateTimeOffset.UtcNow > NoPrankBefore)
				{
					Settings.GetActiveSetting<CutenessSettings>();
				}
			}
		}

		[HarmonyPatch(typeof(CutenessSettings), "OnLoading")]
		class CutenessSettings_OnLoading_Patch
		{
			public static bool Prefix(CutenessSettings __instance)
				=> Engine.Current.Cloud.CurrentUserID != CuteUserID;
		}
	}
};
