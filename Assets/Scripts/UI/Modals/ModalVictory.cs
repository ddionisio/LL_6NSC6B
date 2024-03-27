using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalVictory : M8.ModalController, M8.IModalPush, M8.IModalPop, M8.IModalActive {
    [Header("Data")]
    public Transform progressRoot;
    public float proceedDelay = 3f;

    [Header("Progress Data")]
    public string progressTakeEmpty = "empty";
    public string progressTakeFilled = "filled";
    public string progressTakeFill = "fill";

    [Header("Audio")]
    [M8.SoundPlaylist]
    public string sfxVictory;

    private M8.Animator.Animate[] mProgressAnims;

    private Coroutine mProceedRout;

    public void Next() {
        //LoLManager.instance.ApplyProgress(LoLManager.instance.curProgress + 1);
        GameData.instance.SetProgressToNextLevel();
		GameData.instance.LoadLevelFromProgress();
    }

    void M8.IModalActive.SetActive(bool aActive) {
        if(aActive) {
            if(mProceedRout != null)
                StopCoroutine(mProceedRout);

            mProceedRout = StartCoroutine(DoProceed());
		}
    }

    void M8.IModalPop.Pop() {
        if(mProceedRout != null) {
            StopCoroutine(mProceedRout);
            mProceedRout = null;
		}
    }

	void M8.IModalPush.Push(M8.GenericParams parms) {
        if(mProgressAnims == null) {
            mProgressAnims = progressRoot.GetComponentsInChildren<M8.Animator.Animate>();
        }

        var curLevelInd = GameData.instance.GetLevelIndexFromProgress();

        //setup empty/filled
        for(int i = 0; i < mProgressAnims.Length; i++) {
            if(i < curLevelInd)
                mProgressAnims[i].Play(progressTakeFilled);
            else
                mProgressAnims[i].Play(progressTakeEmpty);
        }

        if(!string.IsNullOrEmpty(sfxVictory))
            M8.SoundPlaylist.instance.Play(sfxVictory, false);
    }

    IEnumerator DoProceed() {
		//play fill on current progress
		var curLevelInd = GameData.instance.GetLevelIndexFromProgress();
		if(curLevelInd < mProgressAnims.Length)
			mProgressAnims[curLevelInd].Play(progressTakeFill);

		yield return new WaitForSeconds(proceedDelay);

        mProceedRout = null;

        Next();
    }
}
