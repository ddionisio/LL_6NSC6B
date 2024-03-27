using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.Game {
    [ActionCategory("Game")]
    [Tooltip("Check current progress to see if we are at the beginning of the level.")]
    public class LevelCheckProgressBegin : FsmStateAction {
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
			var isTrue = GameData.instance.IsCurrentProgressLevelBegin();

			storeResult = isTrue;

			Fsm.Event(isTrue ? this.isTrue : isFalse);
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