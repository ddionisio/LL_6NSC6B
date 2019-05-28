using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.Game {
    [ActionCategory("Game")]
    public class LevelEntityMoverSetDir : ComponentAction<LevelEntityMover> {
        [RequiredField]
        [CheckForComponent(typeof(LevelEntityMover))]
        [Tooltip("The GameObject to apply the force to.")]
        public FsmOwnerDefault gameObject;

        public MoveDir dir;

        public override void Reset() {
            gameObject = null;
            dir = MoveDir.Up;
        }

        public override void OnEnter() {
            var go = Fsm.GetOwnerDefaultTarget(gameObject);
            if(UpdateCache(go))
                cachedComponent.dir = dir;

            Finish();
        }
    }
}