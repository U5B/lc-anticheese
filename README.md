# lc_anticheese
For LC hosts that have issues with people pulling the lever too early.

## Features
- Only the host can land the ship on a planet.
- Only the host can vote to leave early (when dead).
- Only the host can leave from the Company building (unless dead).
- 75% of players must be present on or near the ship to leave (unless you are the host).

## Conflicts
This mod patches the following methods.
- StartMatchLever.__rpc_handler_2406447821 (PlayLeverPullEffectsServerRpc)
- StartOfRound.__rpc_handler_1089447320 (StartGameServerRpc)
- StartOfRound.__rpc_handler_2028434619 (EndGameServerRpc)
- TimeOfDay.__rpc_handler_543987598
- TimeOfDay.SetShipLeaveEarlyServerRpc