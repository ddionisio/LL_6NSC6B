using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.Game {
	[ActionCategory("Game")]
	public class PlaySetPlayerPosition : FsmStateAction {		
		public FsmVector2 position;

		[Tooltip("Repeat every frame.")]
		public bool everyFrame;

		public override void Reset() {
			position = null;
			everyFrame = false;
		}

		public override void OnEnter() {
			DoSetPosition();

			if(!everyFrame) {
				Finish();
			}
		}

		public override void OnUpdate() {
			DoSetPosition();
		}

		private void DoSetPosition() {
			if(!position.IsNone)
				PlayController.instance.player.position = position.Value;
		}
	}
}