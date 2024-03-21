using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.Game {
    [ActionCategory("Game")]
    public class PlayGetPlayerPosition : FsmStateAction {
		[UIHint(UIHint.Variable)]
		public FsmVector2 output;

		[Tooltip("Repeat every frame.")]
		public bool everyFrame;

		public override void Reset() {
			output = null;
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
			output.Value = PlayController.instance.player.position;
		}
	}
}