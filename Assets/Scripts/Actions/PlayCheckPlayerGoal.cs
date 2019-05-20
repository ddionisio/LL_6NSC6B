using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.Game {
    [ActionCategory("Game")]
    public class PlayCheckPlayerGoal : FsmStateAction {
        [UIHint(UIHint.Variable)]
        public FsmBool storeResult;

        public FsmEvent isTrue;
        public FsmEvent isFalse;

        public FsmBool everyFrame;

        public override void Reset() {
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

            var goals = PlayController.instance.levelGrid.goals;
            if(goals != null) {
                int collectedCount = 0;
                for(int i = 0; i < goals.Length; i++) {
                    if(goals[i] && goals[i].state == LevelEntityGoal.State.Collected)
                        collectedCount++;
                }

                isMatch = collectedCount == goals.Length;
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