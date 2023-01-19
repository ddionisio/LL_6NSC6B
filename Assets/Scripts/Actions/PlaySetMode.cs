using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.Game {
    [ActionCategory("Game")]
    public class PlaySetMode : FsmStateAction {
        public PlayController.Mode toMode;

        public override void Reset() {
            toMode = PlayController.Mode.None;
        }

        public override void OnEnter() {
            PlayController.instance.curMode = toMode;
            Finish();
        }
    }
}