using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("")]
public abstract class EffectSpawnerItemBase : MonoBehaviour, M8.IPoolSpawnComplete {

    private M8.PoolDataController mPoolDataCtrl;

    public void Release() {
        if(!mPoolDataCtrl)
            mPoolDataCtrl = GetComponent<M8.PoolDataController>();
        if(mPoolDataCtrl)
            mPoolDataCtrl.Release();
    }

    protected abstract IEnumerator DoPlay();

    void M8.IPoolSpawnComplete.OnSpawnComplete() {
        StartCoroutine(_Play());
    }

    IEnumerator _Play() {
        yield return DoPlay();

        Release();
    }
}
