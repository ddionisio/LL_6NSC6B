using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.Game {
    [ActionCategory("Game")]
    public class PlayGetPlayerPosition : FsmStateAction {
		[UIHint(UIHint.Variable)]
		public FsmVector2 output;

		public FsmBool isReflect;

		[Tooltip("Repeat every frame.")]
		public bool everyFrame;

		public override void Reset() {
			output = null;
			isReflect = null;
			everyFrame = false;
		}

		public override void OnEnter() {
			DoGetPosition();

			if(!everyFrame) {
				Finish();
			}
		}

		public override void OnUpdate() {
			DoGetPosition();
		}

		private void DoGetPosition() {
			var player = PlayController.instance.player;

			if(isReflect.Value) {
				var levelGrid = PlayController.instance.levelGrid;
				var playerCellIndex = player.cellIndex;

				var rX = -(playerCellIndex.col - levelGrid.originCol);
				var rY = -(playerCellIndex.row - levelGrid.originRow);

				playerCellIndex.col = levelGrid.originCol + rX;
				playerCellIndex.row = levelGrid.originRow + rY;

				output.Value = levelGrid.GetCellPosition(playerCellIndex);
			}
			else {
				output.Value = player.position;
			}
		}
	}
}