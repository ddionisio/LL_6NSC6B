using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.Game {
	[ActionCategory("Game")]
	public class PlayShowGameplay : FsmStateAction {
		public override void OnEnter() {
			var levelGrid = PlayController.instance.levelGrid;

			//hide gameplay
			levelGrid.entitiesRoot.gameObject.SetActive(true);
			levelGrid.obstaclesRoot.gameObject.SetActive(true);
			levelGrid.wallRoot.gameObject.SetActive(true);

			Finish();
		}
	}
}