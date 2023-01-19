using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.Game {
    [ActionCategory("Game")]
    public class PlaySetPlayerStartDir : FsmStateAction {
        public MoveDir toDir;

        public override void Reset() {
            toDir = MoveDir.Up;
        }

        public override void OnEnter() {
            PlayController.instance.player.startDir = toDir;
            Finish();
        }
    }
}