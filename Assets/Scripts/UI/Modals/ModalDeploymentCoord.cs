using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class ModalDeploymentCoord : M8.ModalController, M8.IModalPush, M8.IModalPop, M8.IModalActive {
	public const string parmIsReflection = "isReflection";

	[Header("Data")]
	public string modalCoordNumpad = "coordNumpad";

	[Header("Cell Highlight Data")]
	public float cellHighlightMoveDelay = 0.3f;
	public float cellHighlightSuccessDelay = 0.5f;
	public int cellReflectShowAfterFailCount = 2;

	[Header("Title Display")]
	public TMP_Text titleDisplay;
	[M8.Localize]
	public string titleEnterCoordTextRef; //when we just need to enter coordinates
	[M8.Localize]
	public string titleReflectCoordTextRef; //for reflect question

	[Header("Coord Display")]
	public Transform coordRoot;
	public TMP_Text coordText;
	public string coordStringFormat = "({0},  {1})";
	[M8.Localize]
	public string coordOutofBoundsTextRef;

	[Header("Coord Animation")]
	public M8.Animator.Animate coordAnimator;
	[M8.Animator.TakeSelector(animatorField = "coordAnimator")]
	public int takeCoordEnter = -1;
	[M8.Animator.TakeSelector(animatorField = "coordAnimator")]
	public int takeCoordCorrect = -1;
	[M8.Animator.TakeSelector(animatorField = "coordAnimator")]
	public int takeCoordError = -1;

	[Header("SFX")]
	[M8.SoundPlaylist]
	public string sfxMatch;
	[M8.SoundPlaylist]
	public string sfxError;

	[Header("Signal Invoke")]
	public M8.Signal signalInvokeShowReflectPoint;

	[Header("Signal Listen")]
	public SignalFloatArray signalListenCoordVerify;

	private bool mIsReflect;
	private int mErrorCount;

	private Coroutine mCoordVerifyRout;
	private int[] mCoordValues = new int[2]; //[col, row]

	private M8.GenericParams mCoordNumpadParms = new M8.GenericParams();

	void M8.IModalActive.SetActive(bool aActive) {
		if(aActive) {
			if(mIsReflect) {
				if(!string.IsNullOrEmpty(titleReflectCoordTextRef))
					LoLManager.instance.SpeakText(titleReflectCoordTextRef);
			}
			else {
				if(!string.IsNullOrEmpty(titleEnterCoordTextRef))
					LoLManager.instance.SpeakText(titleEnterCoordTextRef);
			}

			//open modal coord
			ShowCoordNumpad();
		}
	}

	void M8.IModalPop.Pop() {
		StopCoordVerify();

		if(signalListenCoordVerify) signalListenCoordVerify.callback -= OnSignalCoordVerify;
	}

	void M8.IModalPush.Push(M8.GenericParams parms) {
		mIsReflect = false;

		if(parms != null) {
			if(parms.ContainsKey(parmIsReflection))
				mIsReflect = parms.GetValue<bool>(parmIsReflection);
		}

		mCoordValues[0] = 0;
		mCoordValues[1] = 0;

		mErrorCount = 0;

		//TODO: determine title to use
		if(titleDisplay) {
			if(mIsReflect) {
				if(!string.IsNullOrEmpty(titleReflectCoordTextRef))
					titleDisplay.text = M8.Localize.Get(titleReflectCoordTextRef);
			}
			else {
				if(!string.IsNullOrEmpty(titleEnterCoordTextRef))
					titleDisplay.text = M8.Localize.Get(titleEnterCoordTextRef);
			}
		}

		coordRoot.gameObject.SetActive(false);

		if(signalListenCoordVerify) signalListenCoordVerify.callback += OnSignalCoordVerify;
	}

	void OnSignalCoordVerify(float[] coords) {
		mCoordValues[0] = Mathf.FloorToInt(coords[0]);
		mCoordValues[1] = Mathf.FloorToInt(coords[1]);

		StopCoordVerify();

		mCoordVerifyRout = StartCoroutine(DoCoordVerify(mCoordValues[0], mCoordValues[1]));
	}

	IEnumerator DoCoordVerify(int x, int y) {
		var cam = Camera.main;
		Vector3 coordUIPos;

		//close coord numpad
		var modalMgr = M8.ModalManager.main;

		modalMgr.CloseUpTo(modalCoordNumpad, true);

		while(modalMgr.isBusy || modalMgr.IsInStack(modalCoordNumpad))
			yield return null;

		//setup
		var levelGrid = PlayController.instance.levelGrid;
		var player = PlayController.instance.player;
		var playerCellIndex = player.cellIndex;

		var coordCol = levelGrid.originCol + x;
		var coordRow = levelGrid.originRow + y;

		var isValid = coordCol == playerCellIndex.col && coordRow == playerCellIndex.row;

		//walk through coordinates
		var walkWait = new WaitForSeconds(cellHighlightMoveDelay);
				
		coordText.text = string.Format(coordStringFormat, 0, 0);

		coordUIPos = cam.WorldToScreenPoint(levelGrid.GetCellPosition(levelGrid.originCol, levelGrid.originRow));
		coordRoot.position = coordUIPos;

		levelGrid.CellHighlightShow(levelGrid.originCol, levelGrid.originRow);

		coordRoot.gameObject.SetActive(true);
		if(takeCoordEnter != -1)
			yield return coordAnimator.PlayWait(takeCoordEnter);

		bool isOutofBounds = false;

		//x-axis
		int xCount = Mathf.Abs(coordCol - levelGrid.originCol);
		int xDir = x < 0 ? -1 : 1;

		var curX = 0;
		for(int i = 0; i < xCount; i++) {
			//check if out of bounds in advance
			var colCheck = levelGrid.originCol + curX + xDir;
			if(colCheck < 0 || colCheck >= levelGrid.numCol) {
				isOutofBounds = true;

				coordText.text = M8.Localize.Get(coordOutofBoundsTextRef);
				break;
			}

			curX += xDir;
			
			coordText.text = string.Format(coordStringFormat, curX, 0);

			coordUIPos = cam.WorldToScreenPoint(levelGrid.GetCellPosition(levelGrid.originCol + curX, levelGrid.originRow));
			coordRoot.position = coordUIPos;

			levelGrid.CellHighlightShow(levelGrid.originCol + curX, levelGrid.originRow);

			yield return walkWait;
		}

		//y-axis
		if(!isOutofBounds) {
			int yCount = Mathf.Abs(coordRow - levelGrid.originRow);
			int yDir = y < 0 ? -1 : 1;

			var curY = 0;
			for(int i = 0; i < yCount; i++) {
				//check if out of bounds in advance
				var rowCheck = levelGrid.originRow + curY + yDir;
				if(rowCheck < 0 || rowCheck >= levelGrid.numRow) {

					coordText.text = M8.Localize.Get(coordOutofBoundsTextRef);
					break;
				}

				curY += yDir;

				coordText.text = string.Format(coordStringFormat, curX, curY);

				coordUIPos = cam.WorldToScreenPoint(levelGrid.GetCellPosition(levelGrid.originCol + curX, levelGrid.originRow + curY));
				coordRoot.position = coordUIPos;

				levelGrid.CellHighlightShow(levelGrid.originCol + curX, levelGrid.originRow + curY);

				yield return walkWait;
			}
		}
				
		levelGrid.CellHighlightHide();

		if(isValid) {
			if(signalInvokeShowReflectPoint)
				signalInvokeShowReflectPoint.Invoke();

			//match
			if(!string.IsNullOrEmpty(sfxMatch))
				M8.SoundPlaylist.instance.Play(sfxMatch, false);

			if(takeCoordCorrect != -1)
				yield return coordAnimator.PlayWait(takeCoordCorrect);

			yield return new WaitForSeconds(cellHighlightSuccessDelay);

			mCoordVerifyRout = null;

			//move on
			Close();
		}
		else {
			//error
			if(!string.IsNullOrEmpty(sfxError))
				M8.SoundPlaylist.instance.Play(sfxError, false);

			if(takeCoordError != -1)
				yield return coordAnimator.PlayWait(takeCoordError);

			mErrorCount++;
			if(mIsReflect && mErrorCount == cellReflectShowAfterFailCount) {
				if(signalInvokeShowReflectPoint)
					signalInvokeShowReflectPoint.Invoke();
			}

			mCoordVerifyRout = null;

			//re-open coord numpad
			ShowCoordNumpad();
		}
	}

	private void ShowCoordNumpad() {
		mCoordNumpadParms[ModalCalculatorMultiNumber.parmInitValues] = mCoordValues;
		mCoordNumpadParms[ModalCalculatorMultiNumber.parmInitIndex] = 0;

		M8.ModalManager.main.Open(modalCoordNumpad, mCoordNumpadParms);
	}

	private void StopCoordVerify() {
		if(mCoordVerifyRout != null) {
			StopCoroutine(mCoordVerifyRout);
			mCoordVerifyRout = null;

			if(PlayController.isInstantiated) {
				var levelGrid = PlayController.instance.levelGrid;
				if(levelGrid)
					levelGrid.CellHighlightHide();
			}
		}
	}
}
