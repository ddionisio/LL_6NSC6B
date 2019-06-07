using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.Game {
    [ActionCategory("Game")]
    public class MusicTrackPlay : FsmStateAction {

        public override void OnEnter() {
            if(MusicTrackController.isInstantiated)
                MusicTrackController.instance.Play();
            Finish();
        }
    }
}