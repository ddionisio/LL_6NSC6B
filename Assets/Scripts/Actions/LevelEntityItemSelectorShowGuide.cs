using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.Game {
    [ActionCategory("Game")]
    public class LevelEntityItemSelectorShowGuide : ComponentAction<LevelEntityItemGroupWidget> {
        [RequiredField]
        [CheckForComponent(typeof(LevelEntityItemGroupWidget))]
        [Tooltip("The GameObject to apply the force to.")]
        public FsmOwnerDefault gameObject;

        public FsmString itemName;
        public FsmInt col;
        public FsmInt row;

        public override void Reset() {
            gameObject = null;
            itemName = null;
            col = null;
            row = null;
        }

        public override void OnEnter() {
            var go = Fsm.GetOwnerDefaultTarget(gameObject);
            if(UpdateCache(go)) {
                var destCell = new CellIndex(row.Value, col.Value);
                cachedComponent.DragGuideShow(itemName.Value, destCell, null);
            }

            Finish();
        }
    }
}