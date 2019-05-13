using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayController : GameModeController<PlayController> {



    protected override void OnInstanceInit() {
        base.OnInstanceInit();
    }

    protected override IEnumerator Start() {
        yield return base.Start();
    }
}
