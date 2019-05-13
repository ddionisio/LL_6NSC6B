using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LevelEntity : MonoBehaviour, M8.IPoolSpawn, M8.IPoolSpawnComplete, M8.IPoolDespawn {
    [Header("Data")]
    public float zOfs = -0.01f;

    public LevelGrid levelGrid {
        get {
            if(!mLevelGrid)
                mLevelGrid = transform.parent ? GetComponentInParent<LevelGrid>() : null;
            return mLevelGrid;
        }
    }

    public int col { get { return _col; } }
    public int row { get { return _row; } }

    public CellIndex cellIndex {
        get { return new CellIndex(_row, _col); }
        set {
            if(_col != value.col || _row != value.row) {
                //update reference in grid
                levelGrid.RemoveEntity(this);

                _col = value.col;
                _row = value.row;

                levelGrid.AddEntity(this);
            }
        }
    }

    public Vector2 position {
        get { return transform.position; }
        set {
            Vector2 curPos = transform.position;
            if(curPos != value) {
                transform.position = new Vector3(value.x, value.y, zOfs);

                //update row and col
                if(levelGrid) {
                    cellIndex = levelGrid.GetCellIndexLocal(transform.localPosition);
                }
            }
        }
    }

    public M8.PoolDataController poolData {
        get {
            if(!mPoolDat)
                mPoolDat = GetComponent<M8.PoolDataController>();
            return mPoolDat;
        }
    }

    [HideInInspector]
    [SerializeField]
    int _col = -1;
    [SerializeField]
    [HideInInspector]
    int _row = -1;

    private LevelGrid mLevelGrid;
    private M8.PoolDataController mPoolDat;

    protected virtual void OnDestroy() {
        if(!poolData) {
            if(levelGrid)
                levelGrid.RemoveEntity(this);
        }
    }

    protected virtual void Start() {
        if(!poolData) {
            if(levelGrid)
                levelGrid.AddEntity(this);
        }
    }

    protected virtual void Update() {
#if UNITY_EDITOR
        if(!Application.isPlaying) {
            //snap to cell if parent is level grid
            if(levelGrid) {
                var _cellIndex = levelGrid.GetCellIndexLocal(transform.localPosition);                
                if(_cellIndex.isValid)
                    transform.position = levelGrid.GetCellPosition(_col, _row);

                _col = _cellIndex.col;
                _row = _cellIndex.row;
            }
        }
#endif
    }

    protected virtual void Spawned(M8.GenericParams parms) { }
    protected virtual void SpawnCompleted() { }
    protected virtual void Despawned() { }

    void M8.IPoolSpawn.OnSpawned(M8.GenericParams parms) {
        Spawned(parms);
    }

    void M8.IPoolSpawnComplete.OnSpawnComplete() {
        if(levelGrid) {
            cellIndex = levelGrid.GetCellIndexLocal(transform.localPosition);
            levelGrid.AddEntity(this);
        }

        SpawnCompleted();
    }

    void M8.IPoolDespawn.OnDespawned() {
        Despawned();

        if(levelGrid)
            levelGrid.RemoveEntity(this);
    }
}
