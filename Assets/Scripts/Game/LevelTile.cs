using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LevelTile : MonoBehaviour {
    [Header("Flags")]
    public bool isWallN;
    public bool isWallE;
    public bool isWallS;
    public bool isWallW;

    public bool isPit;
    public bool isGoal;

    [Header("Displays")]
    public Transform displayRoot;
    public GameObject pitGO;
    public GameObject goalGO;

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
            _col = value.col;
            _row = value.row;
        }
    }

    public bool isPlaceable { get { return !(isPit || isGoal); } }

    [HideInInspector]
    [SerializeField]
    int _col = -1;
    [SerializeField]
    [HideInInspector]
    int _row = -1;

    private LevelGrid mLevelGrid;

#if UNITY_EDITOR
    void Update() {
        if(!Application.isPlaying) {
            //snap to cell if parent is level grid
            if(levelGrid) {
                cellIndex = levelGrid.GetCellIndexLocal(transform.localPosition);
                if(cellIndex.isValid)
                    transform.position = levelGrid.GetCellPosition(_col, _row);
            }
        }
    }
#endif

    void OnDrawGizmos() {
        Gizmos.color = Color.white;

        const float lineThickness = 0.15f;

        if(levelGrid) {
            var cellSize = levelGrid.cellSize;
            var hCellSize = cellSize * 0.5f;

            var pos = transform.position;
            //var min = new Vector2(pos.x - hCellSize.x, pos.y - hCellSize.y);
            //var max = new Vector2(pos.x + hCellSize.x, pos.y + hCellSize.y);

            //walls
            if(isWallN)
                Gizmos.DrawCube(new Vector3(pos.x, pos.y + hCellSize.y - lineThickness * 0.5f, 0f), new Vector3(cellSize.x, lineThickness));
            if(isWallS)
                Gizmos.DrawCube(new Vector3(pos.x, pos.y - hCellSize.y + lineThickness * 0.5f, 0f), new Vector3(cellSize.x, lineThickness));
            if(isWallE)
                Gizmos.DrawCube(new Vector3(pos.x + hCellSize.x - lineThickness * 0.5f, pos.y, 0f), new Vector3(lineThickness, cellSize.y));
            if(isWallW)
                Gizmos.DrawCube(new Vector3(pos.x - hCellSize.x + lineThickness * 0.5f, pos.y, 0f), new Vector3(lineThickness, cellSize.y));
        }
    }
}
