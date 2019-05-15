using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayController : GameModeController<PlayController> {    
    [Header("Data")]
    [M8.TagSelector]
    public string tagLevel = "Level";
    [M8.TagSelector]
    public string tagItemSelectorUI = "ItemSelector";

    protected override void OnInstanceInit() {
        base.OnInstanceInit();
    }

    protected override IEnumerator Start() {
        yield return base.Start();
    }
}
