using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectSpawnerItemAnimator : EffectSpawnerItemBase {
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takePlay;

    protected override IEnumerator DoPlay() {
        if(animator && !string.IsNullOrEmpty(takePlay))
            yield return animator.PlayWait(takePlay);
    }
}
