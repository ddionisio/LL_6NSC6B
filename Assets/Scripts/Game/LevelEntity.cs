using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("")]
public class LevelEntity : MonoBehaviour {
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
                if(levelGrid)
                    levelGrid.RemoveEntity(this);

                _col = value.col;
                _row = value.row;

                if(levelGrid)
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
                if(levelGrid)
                    cellIndex = levelGrid.GetCellIndexLocal(transform.localPosition);
            }
        }
    }

    [HideInInspector]
    [SerializeField]
    protected int _col = -1;
    [SerializeField]
    [HideInInspector]
    protected int _row = -1;

    private LevelGrid mLevelGrid;
    
    /// <summary>
    /// Refresh position to current cell
    /// </summary>
    public void SnapPosition() {
        var pos = levelGrid.GetCellPosition(_col, _row);
        transform.position = new Vector3(pos.x, pos.y, zOfs);
    }

    protected virtual void Update() {
#if UNITY_EDITOR
        if(!Application.isPlaying) {
            //snap to cell if parent is level grid
            if(levelGrid) {
                var _cellIndex = levelGrid.GetCellIndexLocal(transform.localPosition);

                _col = _cellIndex.col;
                _row = _cellIndex.row;

                if(_cellIndex.isValid)
                    SnapPosition();
            }
        }
#endif
    }

    void OnDrawGizmos() {
        //this is here so this gameobject can be selected properly
        if(levelGrid) {
            Gizmos.color = Color.clear;
            Gizmos.DrawCube(transform.position, new Vector3(levelGrid.cellSize.x, levelGrid.cellSize.y, 0f));
        }
    }
}
