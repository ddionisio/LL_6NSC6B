using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.Game {
    [ActionCategory("Game")]
    public class LevelEntityItemSelectorHideGuide : ComponentAction<LevelEntityItemGroupWidget> {
        [RequiredField]
        [CheckForComponent(typeof(LevelEntityItemGroupWidget))]
        [Tooltip("The GameObject to apply the force to.")]
        public FsmOwnerDefault gameObject;

        public override void Reset() {
            gameObject = null;
        }

        public override void OnEnter() {
            var go = Fsm.GetOwnerDefaultTarget(gameObject);
            if(UpdateCache(go)) {
                cachedComponent.DragGuideHide();
            }

            Finish();
        }
    }
}