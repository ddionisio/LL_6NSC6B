using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalVictory : M8.ModalController {
    [Header("Data")]
    public int progressStartInd; //starting index to show progress

    public void Next() {
        LoLManager.instance.ApplyProgress(LoLManager.instance.curProgress + 1);
        GameData.instance.LoadLevelFromProgress();
    }
}
