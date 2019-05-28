using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.Game {
    [ActionCategory("Game")]
    public class LevelEntityMoverMove : ComponentAction<LevelEntityMover> {
        [RequiredField]
        [CheckForComponent(typeof(LevelEntityMover))]
        [Tooltip("The GameObject to apply the force to.")]
        public FsmOwnerDefault gameObject;

        public FsmInt cellCount;

        private CellIndex mLastCellIndex;
        private int mCellCount;

        public override void Reset() {
            gameObject = null;

            cellCount = null;
        }

        public override void OnEnter() {
            mCellCount = 0;

            var go = Fsm.GetOwnerDefaultTarget(gameObject);
            if(UpdateCache(go)) {
                if(cellCount.Value > 0) {
                    mLastCellIndex = cachedComponent.cellIndex;

                    cachedComponent.moveUpdateCallback += OnMoveUpdate;

                    cachedComponent.state = LevelEntityMover.State.Moving;
                }
                else
                    Finish();
            }
        }

        public override void OnExit() {
            var go = Fsm.GetOwnerDefaultTarget(gameObject);
            if(UpdateCache(go)) {
                cachedComponent.moveUpdateCallback -= OnMoveUpdate;
            }
        }

        void OnMoveUpdate() {
            if(cachedComponent.cellIndex == mLastCellIndex)
                return;

            mLastCellIndex = cachedComponent.cellIndex;

            mCellCount++;
            if(mCellCount >= cellCount.Value)
                Finish();
        }
    }
}