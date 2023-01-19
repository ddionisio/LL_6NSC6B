using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.Game {
    [ActionCategory("Game")]
    public class MusicTrackStop : FsmStateAction {

        public override void OnEnter() {
            if(MusicTrackController.isInstantiated)
                MusicTrackController.instance.Stop();
            Finish();
        }
    }
}
