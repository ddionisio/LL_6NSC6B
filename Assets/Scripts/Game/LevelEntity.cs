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

    public int col { get { return mCellIndex.col; } }
    public int row { get { return mCellIndex.row; } }

    public CellIndex cellIndex {
        get { return mCellIndex; }
        set {
            if(mCellIndex != value) {
                //update reference in grid
                if(levelGrid)
                    levelGrid.RemoveEntity(this);

                mCellIndex = value;

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

    protected CellIndex mCellIndex;

    private LevelGrid mLevelGrid;
        
    /// <summary>
    /// Refresh position to current cell
    /// </summary>
    public void SnapPosition() {
        var pos = levelGrid.GetCellPosition(cellIndex);
        transform.position = new Vector3(pos.x, pos.y, zOfs);
    }

    protected void RefreshCellIndex() {
        if(levelGrid)
            mCellIndex = levelGrid.GetCellIndexLocal(transform.localPosition);
    }

    protected virtual void Update() {
#if UNITY_EDITOR
        if(!Application.isPlaying) {
            //snap to cell if parent is level grid
            if(levelGrid) {
                mCellIndex = levelGrid.GetCellIndexLocal(transform.localPosition);

                //if(mCellIndex.isValid) {
                    Vector2 pos = levelGrid.GetCellPosition(mCellIndex);
                    transform.position = new Vector3(pos.x, pos.y, zOfs);
                //}
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
