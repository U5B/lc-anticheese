using UnityEngine;
using HarmonyLib;
using GameNetcodeStuff;
using Unity.Netcode;

namespace lc_anticheese.Patches
{
	[HarmonyPatch]
	internal class VotePatch
	{
		[HarmonyPatch(typeof(TimeOfDay), "__rpc_handler_543987598")]
		[HarmonyPrefix]
		private static bool SetShipLeaveEarlyServerRpcHandlerPatch(__RpcParams rpcParams)
		{
			ulong playerVoteId = rpcParams.Server.Receive.SenderClientId;
			PlayerControllerB player = null;
			foreach (PlayerControllerB p in StartOfRound.Instance.allPlayerScripts)
			{
				if (p.actualClientId == playerVoteId)
				{
					player = p;
					break;
				}
			}
			// Only the host gets to decide when to leave
			Plugin.SendWarning("" + player.playerUsername + " (" + player.playerSteamId + ") attempted to vote to leave early!");
			return false;
		}
		[HarmonyPatch(typeof(TimeOfDay), "SetShipLeaveEarlyServerRpc")]
		[HarmonyPrefix]
		private static bool SetShipLeaveEarlyServerRpcPatch(TimeOfDay __instance)
		{
			// Host doesn't call an rpc handler so it is safe to just leave if this gets called
			if (!__instance.IsServer)
			{
				return true;
			}
			__instance.SetShipLeaveEarlyClientRpc(__instance.normalizedTimeOfDay + 0.1f, 1);
			return false;
		}
	}
}