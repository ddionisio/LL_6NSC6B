using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.Game {
    [ActionCategory("Game")]
    public class LevelEntityMoverJumpTo : ComponentAction<LevelEntityMover> {
        [RequiredField]
        [CheckForComponent(typeof(LevelEntityMover))]
        [Tooltip("The GameObject to apply the force to.")]
        public FsmOwnerDefault gameObject;

        public FsmInt col;
        public FsmInt row;

        public override void Reset() {
            gameObject = null;
            col = null;
            row = null;
        }

        public override void OnEnter() {
            var go = Fsm.GetOwnerDefaultTarget(gameObject);
            if(UpdateCache(go)) {
                cachedComponent.JumpTo(col.Value, row.Value);
            }
        }

        public override void OnUpdate() {
            //wait for jumping to finish
            if(cachedComponent.state != LevelEntityMover.State.Jumping) {
                Finish();
            }
        }
    }
}