using HutongGames.PlayMaker.Actions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalDeploymentQuadrant : M8.ModalController, M8.IModalPush, M8.IModalPop, M8.IModalActive {
	public const string parmIsReflection = "isReflection";

	[Header("Data")]
	public AnswerWidget[] answers;
	[M8.Localize]
	public string questionTextRef;

	[Header("Quadrant Highlight")]
	public float quadrantCorrectBrightOfs = 0.3f;
	public int quadrantCorrectPulseCount = 2;
	public float quadrantCorrectPulseDelay = 0.3f;

	public float quadrantWrongBrightOfs = -0.3f;
	public int quadrantWrongPulseCount = 2;
	public float quadrantWrongPulseDelay = 0.3f;


	private int mAnswerIndex;

	private int mQuadrantIndex;

	private Coroutine mQuadrantHighlightRout;

	void M8.IModalActive.SetActive(bool aActive) {
		if(aActive) {
			if(!string.IsNullOrEmpty(questionTextRef))
				LoLManager.instance.SpeakText(questionTextRef);

			StartCoroutine(DoShowAnswers());
		}
		else {
			for(int i = 0; i < answers.Length; i++) {
				if(answers[i])
					answers[i].active = false;
			}
		}
	}

	void M8.IModalPop.Pop() {
		for(int i = 0; i < answers.Length; i++) {
			if(answers[i])
				answers[i].clickCallback -= OnAnswerClick;
		}

		if(mQuadrantHighlightRout != null) {
			StopCoroutine(mQuadrantHighlightRout);
			mQuadrantHighlightRout = null;
		}
	}

	void M8.IModalPush.Push(M8.GenericParams parms) {
		for(int i = 0; i < answers.Length; i++) {
			if(answers[i]) {
				answers[i].clickCallback += OnAnswerClick;
				answers[i].active = false;
			}
		}

		var isReflection = false;

		if(parms != null) {
			if(parms.ContainsKey(parmIsReflection))
				isReflection = parms.GetValue<bool>(parmIsReflection);
		}

		var levelGrid = PlayController.instance.levelGrid;
		var player = PlayController.instance.player;
		var playerCellIndex = player.cellIndex;

		if(isReflection) {
			var rX = -(playerCellIndex.col - levelGrid.originCol);
			var rY = -(playerCellIndex.row - levelGrid.originRow);

			playerCellIndex.col = levelGrid.originCol + rX;
			playerCellIndex.row = levelGrid.originRow + rY;
		}

		var quadType = levelGrid.GetQuadrant(playerCellIndex);
		switch(quadType) {
			case QuadrantType.Quadrant1:
				mQuadrantIndex = 0;
				break;
			case QuadrantType.Quadrant2:
				mQuadrantIndex = 1;
				break;
			case QuadrantType.Quadrant3:
				mQuadrantIndex = 2;
				break;
			case QuadrantType.Quadrant4:
				mQuadrantIndex = 3;
				break;
			default:
				mQuadrantIndex = -1;
				break;
		}
	}

	void OnAnswerClick(AnswerWidget answerWidget) {
		mAnswerIndex = answerWidget.index;

		if(mQuadrantHighlightRout != null)
			StopCoroutine(mQuadrantHighlightRout);

		if(mAnswerIndex == mQuadrantIndex) {
			//disable all the answers
			for(int i = 0; i < answers.Length; i++) {
				if(answers[i])
					answers[i].interactable = false;
			}

			answerWidget.Correct();

			mQuadrantHighlightRout = StartCoroutine(DoQuadrantHighlight(mAnswerIndex, true));
		}
		else {
			answerWidget.Error();

			mQuadrantHighlightRout = StartCoroutine(DoQuadrantHighlight(mAnswerIndex, false));
		}
	}

	IEnumerator DoShowAnswers() {
		var wait = new WaitForSeconds(0.3f);

		for(int i = 0; i < answers.Length; i++) {
			if(answers[i]) {
				answers[i].active = true;
				yield return wait;
			}
		}
	}

	IEnumerator DoQuadrantHighlight(int quadrantInd, bool isCorrect) {

		var levelGrid = PlayController.instance.levelGrid;

		int minCol, minRow, maxCol, maxRow;

		switch(quadrantInd) {
			case 0: //quadrant 1
				minCol = levelGrid.originCol + 1;
				minRow = levelGrid.originRow + 1;
				maxCol = levelGrid.numCol - 1;
				maxRow = levelGrid.numRow - 1;
				break;
			case 1: //quadrant 2
				minCol = 0;
				minRow = levelGrid.originRow + 1;
				maxCol = levelGrid.originCol - 1;
				maxRow = levelGrid.numRow - 1;
				break;
			case 2: //quadrant 3
				minCol = 0;
				minRow = 0;
				maxCol = levelGrid.originCol - 1;
				maxRow = levelGrid.originRow - 1;
				break;
			case 3: //quadrant 4
				minCol = levelGrid.originCol + 1;
				minRow = 0;
				maxCol = levelGrid.numCol - 1;
				maxRow = levelGrid.originRow - 1;
				break;
			default:
				minCol = minRow = maxCol = maxRow = 0;
				break;
		}

		float brightOfs, pulseDelay;
		int pulseCount;

		if(isCorrect) {
			brightOfs = quadrantCorrectBrightOfs;
			pulseCount = quadrantCorrectPulseCount;
			pulseDelay = quadrantCorrectPulseDelay;
		}
		else {
			brightOfs = quadrantWrongBrightOfs;
			pulseCount = quadrantWrongPulseCount;
			pulseDelay = quadrantWrongPulseDelay;
		}

		var wait = new WaitForSeconds(pulseDelay);

		for(int i = 0; i < pulseCount; i++) {
			for(int r = minRow; r <= maxRow; r++) {
				for(int c = minCol; c <= maxCol; c++) {
					var tile = levelGrid.GetTile(c, r);
					if(tile != null)
						tile.BrightnessFade(brightOfs, pulseDelay);
				}
			}

			yield return wait;
		}

		mQuadrantHighlightRout = null;

		//if correct, pop modal
		if(isCorrect)
			Close();
	}
}
