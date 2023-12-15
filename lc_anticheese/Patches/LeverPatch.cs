using UnityEngine;
using HarmonyLib;
using GameNetcodeStuff;
using Unity.Netcode;

namespace lc_anticheese.Patches
{
	[HarmonyPatch]
	internal class LeverPatch
	{
		[HarmonyPatch(typeof(StartMatchLever), "__rpc_handler_2406447821")]
		[HarmonyPrefix]
		private static bool PlayLeverPullEffectsServerRpcHandlerPatch(__RpcParams rpcParams)
		{
			// Pulling the lever animation technically locks it up for all players, so don't if someone else other than the host pulled the lever
			return false;
		}

		// [HarmonyPatch(typeof(StartMatchLever), "PlayLeverPullEffectsServerRpc")]
		// [HarmonyPrefix]
		// private static bool PlayLeverPullEffectsServerRpcPatch(bool leverPulled)
		// {
		// 	return true;
		// }

		[HarmonyPatch(typeof(StartOfRound), "__rpc_handler_1089447320")]
		[HarmonyPrefix]
		private static bool StartGameServerRpcHandlerPatch(StartOfRound __instance, __RpcParams rpcParams)
		{
			// never allow it for other players other than the host
			ulong playerStartGameId = rpcParams.Server.Receive.SenderClientId;
			// server can sometimes send rpcs through the handler
			if (playerStartGameId == GameNetworkManager.Instance.localPlayerController.actualClientId)
			{
				return true;
			}
			PlayerControllerB player = null;
			foreach (PlayerControllerB p in StartOfRound.Instance.allPlayerScripts)
			{
				if (p.actualClientId == playerStartGameId)
				{
					player = p;
					break;
				}
			}
			if (player != null)
			{
				Plugin.SendWarning("" + player.playerUsername + " (" + player.playerSteamId + ") attempted to land the ship early!");
			}
			else
			{
				Plugin.SendWarning("A unknown player attempted to land the ship early!");
			}
			return false;
		}

		[HarmonyPatch(typeof(StartOfRound), "StartGameServerRpc")]
		[HarmonyPrefix]
		private static bool StartGameServerRpcPatch(StartOfRound __instance)
		{
			UnityEngine.Object.FindObjectOfType<StartMatchLever>().PlayLeverPullEffectsClientRpc(true);
			return true;
		}

		[HarmonyPatch(typeof(StartOfRound), "__rpc_handler_2028434619")]
		[HarmonyPrefix]
		private static bool EndGameServerRpcHandlerPatch(__RpcParams rpcParams)
		{
			ulong playerEndGameId = rpcParams.Server.Receive.SenderClientId;
			// server can sometimes send rpcs through the handler
			if (playerEndGameId == GameNetworkManager.Instance.localPlayerController.actualClientId)
			{
				return true;
			}
			PlayerControllerB player = null;
			foreach (PlayerControllerB p in StartOfRound.Instance.allPlayerScripts)
			{
				if (p.actualClientId == playerEndGameId)
				{
					player = p;
					break;
				}
			}
			if (ShouldEndGame(player))
			{
				EndGameAction(player);
				return true;
			}
			if (player != null)
			{
				Plugin.SendWarning("" + player.playerUsername + " (" + player.playerSteamId + ") attempted to start the ship early!");
			}
			else
			{
				Plugin.SendWarning("A unknown player attempted to start the ship early!");
			}
			UnityEngine.Object.FindObjectOfType<StartMatchLever>().triggerScript.interactable = true;
			return false;
		}

		[HarmonyPatch(typeof(StartOfRound), "EndGameServerRpc")]
		[HarmonyPrefix]
		private static bool EndGameServerRpcPatch(StartOfRound __instance)
		{
			UnityEngine.Object.FindObjectOfType<StartMatchLever>().PlayLeverPullEffectsClientRpc(false);
			return true;
		}

		private static void EndGameAction(PlayerControllerB player)
		{
			if (player != null)
			{
				Plugin.SendWarning("" + player.playerUsername + " (" + player.playerSteamId + ") started the ship!");
			}
			else
			{
				Plugin.SendWarning("A unknown player started the ship early!");
			}
		}

		private static bool ShouldEndGame(PlayerControllerB playerPulled)
		{
			PlayerControllerB playerHost = GameNetworkManager.Instance.localPlayerController;
			bool isHost = playerHost.actualClientId == playerPulled.actualClientId;
			if (isHost)
			{
				return true;
			}
			StartOfRound __instance = StartOfRound.Instance;
			// find actual player
			if (playerPulled == null || !playerPulled.isPlayerControlled)
			{
				Plugin.SendWarning("Player is not valid!");
				return false;
			}

			if (!playerPulled.isInHangarShipRoom || playerPulled.isPlayerDead)
			{
				Plugin.SendWarning("Player attemped to start ship while not in the ship? Potentially cheating? - " + playerPulled.playerUsername + " (" + playerPulled.playerSteamId + ")");
				return false;
			}

			// find host
			// prevent players from leaving when selling unless they are the host
			if (!playerHost.isPlayerDead && __instance.currentLevel.name == "CompanyBuildingLevel")
			{
				Plugin.SendWarning("Player attemped to start ship while selling quota and host is not dead.");
				return false;
			}

			// prevent players from leaving unless the host is dead or is near the hangar ship
			if (!playerHost.isPlayerDead && !IsNearShip(playerHost))
			{
				Plugin.SendWarning("Host is not dead and is not nearby ship.");
				return false;
			}

			int missingPlayers = 0;
			int totalValidPlayers = 0;
			for (int i = 0; i < __instance.allPlayerScripts.Length; i++)
			{
				PlayerControllerB player = __instance.allPlayerScripts[i];
				// not valid player
				if (player == null || !player.isActiveAndEnabled || !player.isPlayerControlled)
				{
					continue;
				}
				// if dead or near ship, is not missing
				totalValidPlayers++;
				if (player.isPlayerDead || IsNearShip(player))
				{
					continue;
				}
				missingPlayers++;
			}

			float missingPlayerPercent = missingPlayers / totalValidPlayers;
			if (missingPlayerPercent <= 0.25f)
			{
				return true;
			}
			Plugin.SendWarning($"Too many missing players! Players: #{missingPlayers}/${totalValidPlayers}");
			return false;
		}

		private static bool IsNearShip(PlayerControllerB player)
		{
			return Vector3.Distance(StartOfRound.Instance.shipBounds.bounds.center, player.gameplayCamera.transform.position) <= 12f;
		}
	}
}