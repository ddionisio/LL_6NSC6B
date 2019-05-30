using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.Game {
    [ActionCategory("Game")]
    public class PlaySetPlayerStartCellToCurrent : FsmStateAction {
        public override void OnEnter() {
            PlayController.instance.player.SetDefaultCell(PlayController.instance.player.cellIndex);
            Finish();
        }
    }
}