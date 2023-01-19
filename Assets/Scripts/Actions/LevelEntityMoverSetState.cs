using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.Game {
    [ActionCategory("Game")]
    public class LevelEntityMoverSetState : ComponentAction<LevelEntityMover> {
        [RequiredField]
        [CheckForComponent(typeof(LevelEntityMover))]
        [Tooltip("The GameObject to apply the force to.")]
        public FsmOwnerDefault gameObject;

        public LevelEntityMover.State state;

        public override void Reset() {
            gameObject = null;
            state = LevelEntityMover.State.None;
        }

        public override void OnEnter() {
            var go = Fsm.GetOwnerDefaultTarget(gameObject);
            if(UpdateCache(go))
                cachedComponent.state = state;

            Finish();
        }
    }
}