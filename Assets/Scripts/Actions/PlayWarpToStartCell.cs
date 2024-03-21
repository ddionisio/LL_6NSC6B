using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.Game {
	[ActionCategory("Game")]
	public class PlayWarpToStartCell : FsmStateAction {
		public override void OnEnter() {
			var player = PlayController.instance.player;
			player.WarpTo(player.defaultCellIndex);

			Finish();
		}

	}
}