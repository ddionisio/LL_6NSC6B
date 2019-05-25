using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectSpawner : MonoBehaviour {
    public string poolGroup = "fx";
    public GameObject template;
    public int capacity = 4;

    public Transform target;

    private M8.PoolController mPool;

    public void Spawn() {
        var pos = target ? target.position : transform.position;

        //assume it is an EffectSpawnerItem and it will release itself after it is done.
        mPool.Spawn(template.name, "", null, pos, null);
    }

    void Awake() {
        mPool = M8.PoolController.CreatePool(poolGroup);
        mPool.AddType(template, capacity, capacity);
    }
}
