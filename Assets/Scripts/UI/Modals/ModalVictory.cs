using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalVictory : M8.ModalController, M8.IModalPush, M8.IModalActive {
    [Header("Data")]
    public int progressStartInd = 0; //starting index to show progress
    public Transform progressRoot;

    [Header("Progress Data")]
    public string progressTakeEmpty = "empty";
    public string progressTakeFilled = "filled";
    public string progressTakeFill = "fill";

    private M8.Animator.Animate[] mProgressAnims;

    public void Next() {
        LoLManager.instance.ApplyProgress(LoLManager.instance.curProgress + 1);
        GameData.instance.LoadLevelFromProgress();
    }

    void M8.IModalActive.SetActive(bool aActive) {
        if(aActive) {
            //play fill on current progress
            var curProgress = LoLManager.instance.curProgress;
            if(curProgress < mProgressAnims.Length)
                mProgressAnims[curProgress].Play(progressTakeFill);
        }
    }

    void M8.IModalPush.Push(M8.GenericParams parms) {
        if(mProgressAnims == null) {
            mProgressAnims = progressRoot.GetComponentsInChildren<M8.Animator.Animate>();
        }

        var curProgress = LoLManager.instance.curProgress;

        //setup empty/filled
        for(int i = 0; i < mProgressAnims.Length; i++) {
            if(i < curProgress)
                mProgressAnims[i].Play(progressTakeFilled);
            else if(i >= curProgress)
                mProgressAnims[i].Play(progressTakeEmpty);
        }
    }
}
