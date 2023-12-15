using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using BepInEx.Configuration;
using HarmonyLib;
using lc_anticheese.Patches;

namespace lc_anticheese
{
	[BepInPlugin(GUID, NAME, PluginInfo.PLUGIN_VERSION)]
	public class Plugin : BaseUnityPlugin
	{

		public const string GUID = "net.usbwire.usb.lc_anticheese";
		public const string NAME = "lc_anticheese";

		internal static ManualLogSource mls;
		private void Awake()
		{
			mls = Logger;
			// Plugin startup logic
			mls.LogInfo($"Plugin {GUID} is loaded!");
			Harmony harmony = new Harmony(GUID);
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}

		public static void SendWarning(string text)
		{
			Plugin.mls.LogWarning(text);
			HUDManager.Instance.AddToErrorLog(text, 0);
			HUDManager.Instance.AddChatMessage(text, "Lever");
		}
	}
}