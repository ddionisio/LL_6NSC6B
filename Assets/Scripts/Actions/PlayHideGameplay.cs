using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.Game {
    [ActionCategory("Game")]
    public class PlayHideGameplay : FsmStateAction {
        public override void OnEnter() {
            var levelGrid = PlayController.instance.levelGrid;

            //hide gameplay
            levelGrid.entitiesRoot.gameObject.SetActive(false);
            levelGrid.obstaclesRoot.gameObject.SetActive(false);
            levelGrid.wallRoot.gameObject.SetActive(false);

            Finish();
        }
    }
}