using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.Game {
    [ActionCategory("Game")]
    public class LevelGridCheckEntityOnCell : FsmStateAction {
        public FsmString entityName;
        public FsmInt col;
        public FsmInt row;

        [UIHint(UIHint.Variable)]
        public FsmBool storeResult;

        public FsmEvent isTrue;
        public FsmEvent isFalse;

        public FsmBool everyFrame;

        public override void Reset() {
            entityName = null;
            col = null;
            row = null;

            storeResult = null;
            isTrue = null;
            isFalse = null;

            everyFrame = false;
        }

        public override void OnEnter() {
            DoCheck();

            if(!everyFrame.Value)
                Finish();
        }

        public override void OnUpdate() {
            DoCheck();
        }

        void DoCheck() {
            bool isMatch = false;

            var cellIndex = new CellIndex(row.Value, col.Value);

            var ents = PlayController.instance.levelGrid.GetEntities(cellIndex);
            if(ents != null) {
                for(int i = 0; i < ents.Count; i++) {
                    if(ents[i].name == entityName.Value) {
                        isMatch = true;
                        break;
                    }
                }
            }

            storeResult = isMatch;

            Fsm.Event(isMatch ? isTrue : isFalse);
        }

        public override string ErrorCheck() {
            if(everyFrame.Value &&
                FsmEvent.IsNullOrEmpty(isTrue) &&
                FsmEvent.IsNullOrEmpty(isFalse))
                return "Action sends no events!";
            return "";
        }
    }
}