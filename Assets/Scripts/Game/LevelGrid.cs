using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGrid : MonoBehaviour {
    [System.Serializable]
    public struct CellGeneratorInfo {
        public int paletteIndex;
        public float brightnessOffset;
    }

    [Header("Grid Info")]
    public Vector2 cellSize = new Vector2(1f, 1f);
    public int originCol = 2;
    public int originRow = 2;
    public int numCol = 5;
    public int numRow = 5;
    public Transform tilesRoot;
    public Transform entitiesRoot;
    public Transform cellHighlightRoot; //use for placing tiles

    public GameObject cellDestGO;
    public Transform cellDestReticleRoot;
    public SpriteRenderer cellDestSpriteRender;

    [Header("Wall")]
    public Transform wallRoot;
    public SpriteRenderer wallLineHTemplate;
    public SpriteRenderer wallLineHBackTemplate;
    public SpriteRenderer wallLineVTemplate;
    public SpriteRenderer wallLineVBackTemplate;
    public Vector2 wallLinePosOfs;
    public Vector2 wallLineOfs;
    public Vector2 wallLineBackOfs;

    [Header("Cell Generator")]
    public LevelTile cellGenTemplate;
    public CellGeneratorInfo[] cellGenQuadrantInfos;
    public CellGeneratorInfo cellGenAxisInfo;

    public Vector2 size { get { return new Vector2(numCol * cellSize.x, numRow * cellSize.y); } }
    public Vector2 extents { get { return new Vector2(numCol * cellSize.x * 0.5f, numRow * cellSize.y * 0.5f); } }
    public Vector2 min { get { return center - extents; } }
    public Vector2 max { get { return center + extents; } }
    public Vector2 center { get { return transform.position; } }

    public LevelTile[,] tileCells {
        get {
            if(mTileCells == null || mTileCells.GetLength(0) != numRow || mTileCells.GetLength(1) != numCol)
                mTileCells = GenerateTileCells();

            return mTileCells;
        }
    }

    public LevelEntityGoal[] goals {
        get {
            if(mGoals == null)
                mGoals = entitiesRoot ? entitiesRoot.GetComponentsInChildren<LevelEntityGoal>(true) : null;

            return mGoals;
        }
    }

    private LevelTile[,] mTileCells; //[row][col]
    private LevelEntityGoal[] mGoals;

    private const int entityListCapacity = 4;
    private M8.CacheList<LevelEntity>[,] mEntityCells;

    public LevelTile[,] GenerateTileCells() {
        var tileCells = new LevelTile[numRow, numCol];
        var tiles = tilesRoot ? tilesRoot.GetComponentsInChildren<LevelTile>() : GetComponentsInChildren<LevelTile>();

        //go through tiles and place them
        for(int i = 0; i < tiles.Length; i++) {
            var tile = tiles[i];

            var cellInd = GetCellIndexLocal(tile.transform.localPosition);
            if(cellInd.isValid)
                tileCells[cellInd.row, cellInd.col] = tile;
        }

        return tileCells;
    }
        
    public M8.CacheList<LevelEntity> GetEntities(int col, int row) {
        if(mEntityCells == null)
            return null;

        if(row < 0 || row >= mEntityCells.GetLength(0) || col < 0 || col >= mEntityCells.GetLength(1))
            return null;

        return mEntityCells[row, col];
    }

    public M8.CacheList<LevelEntity> GetEntities(CellIndex cellIndex) {
        return GetEntities(cellIndex.col, cellIndex.row);
    }

    public M8.CacheList<LevelEntity> GetEntities(Vector2 pos) {
        var cellIndex = GetCellIndex(pos);
        return GetEntities(cellIndex.col, cellIndex.row);
    }

    public void AddEntity(LevelEntity ent) {
        if(!ent)
            return;

        if(mEntityCells == null)
            mEntityCells = new M8.CacheList<LevelEntity>[numRow, numCol];

        var entCellInd = ent.cellIndex;
        if(entCellInd.row >= 0 && entCellInd.row < mEntityCells.GetLength(0) && entCellInd.col >= 0 && entCellInd.col < mEntityCells.GetLength(1)) {
            var entList = mEntityCells[entCellInd.row, entCellInd.col];
            if(entList == null) {
                mEntityCells[entCellInd.row, entCellInd.col] = entList = new M8.CacheList<LevelEntity>(entityListCapacity);
                entList.Add(ent);
            }
            else if(!entList.Exists(ent))
                entList.Add(ent);
        }
    }

    public void RemoveEntity(LevelEntity ent) {
        if(!ent)
            return;

        var entList = GetEntities(ent.cellIndex);
        if(entList != null)
            entList.Remove(ent);
    }

    /// <summary>
    /// Return the quadrant number: 1,2,3,4. 0 if origin, -1 if along x-axis, -2 if along y-axis
    /// </summary>
    public QuadrantType GetQuadrant(Vector2 pos) {
        var cellInd = GetCellIndex(pos);
        return GetQuadrant(cellInd.col, cellInd.row);
    }

    /// <summary>
    /// Return the quadrant number: 1,2,3,4. 0 if origin, -1 if along x-axis, -2 if along y-axis
    /// </summary>
    public QuadrantType GetQuadrant(CellIndex cellIndex) {
        return GetQuadrant(cellIndex.col, cellIndex.row);
    }

    /// <summary>
    /// Return the quadrant number: 1,2,3,4. 0 if origin, -1 if along x-axis, -2 if along y-axis
    /// </summary>
    public QuadrantType GetQuadrant(int col, int row) {
        int _col = col - originCol;
        int _row = row - originRow;

        if(_col < 0) {
            if(_row < 0)
                return QuadrantType.Quadrant3;
            else if(_row > 0)
                return QuadrantType.Quadrant2;
            else
                return QuadrantType.AxisX;
        }
        else if(_col > 0) {
            if(_row < 0)
                return QuadrantType.Quadrant4;
            else if(_row > 0)
                return QuadrantType.Quadrant1;
            else
                return QuadrantType.AxisX;
        }
        else if(_row != 0)
            return QuadrantType.AxisY;

        return QuadrantType.Origin;
    }

    public LevelTile GetTile(Vector2 pos) {
        var cellInd = GetCellIndex(pos);        
        return GetTile(cellInd);
    }

    public LevelTile GetTile(CellIndex cellIndex) {
        if(cellIndex.row >= 0 && cellIndex.row < tileCells.GetLength(0) && cellIndex.col >= 0 && cellIndex.col < tileCells.GetLength(1))
            return tileCells[cellIndex.row, cellIndex.col];

        return null;
    }

    public LevelTile GetTile(int col, int row) {
        if(row >= 0 && row < tileCells.GetLength(0) && col >= 0 && col < tileCells.GetLength(1))
            return tileCells[row, col];
        return null;
    }

    /// <summary>
    /// Grab cell index based on given world position.
    /// </summary>
    public CellIndex GetCellIndex(Vector2 pos) {
        Vector2 lpos = transform.worldToLocalMatrix.MultiplyPoint3x4(pos);
        return GetCellIndexLocal(lpos);
    }

    /// <summary>
    /// Grab cell index based on given local position.
    /// </summary>
    public CellIndex GetCellIndexLocal(Vector2 lpos) {
        lpos += extents;

        int col = Mathf.FloorToInt(lpos.x / cellSize.x);
        if(col < 0 || col >= numCol)
            col = -1;

        int row = Mathf.FloorToInt(lpos.y / cellSize.y);
        if(row < 0 || row >= numRow)
            row = -1;

        return new CellIndex(row, col);
    }

    /// <summary>
    /// Grab center of cell position (world) based on given pos (world). Basically snaps given pos
    /// </summary>
    public Vector2 GetCellPosition(Vector2 pos) {
        return GetCellPosition(GetCellIndex(pos));
    }

    /// <summary>
    /// Grab center of cell position (world) based on given col and row.
    /// </summary>
    public Vector2 GetCellPosition(int col, int row) {
        return min + new Vector2(col * cellSize.x, row * cellSize.y) + cellSize * 0.5f;
    }

    /// <summary>
    /// Grab center of cell position (world) based on given CellIndex.
    /// </summary>
    public Vector2 GetCellPosition(CellIndex cellIndex) {
        return GetCellPosition(cellIndex.col, cellIndex.row);
    }

    void Awake() {
        if(cellHighlightRoot) cellHighlightRoot.gameObject.SetActive(false);
        if(cellDestGO) cellDestGO.SetActive(false);
    }

    void OnDrawGizmos() {
        var qClr = Color.green * 0.5f;
        var cClr = Color.green;

        //draw grid
        Gizmos.color = qClr;
                        
        for(int r = 0; r < numRow; r++) {
            if(r == originRow)
                continue;

            var curPos = new Vector2(min.x, min.y + r * cellSize.y);

            for(int c = 0; c < numCol; c++) {
                if(c != originCol) {
                    var _min = curPos;
                    var _max = _min + cellSize;

                    Gizmos.DrawLine(new Vector2(_min.x, _min.y), new Vector2(_max.x, _min.y));
                    Gizmos.DrawLine(new Vector2(_max.x, _min.y), new Vector2(_max.x, _max.y));
                    Gizmos.DrawLine(new Vector2(_max.x, _max.y), new Vector2(_min.x, _max.y));
                    Gizmos.DrawLine(new Vector2(_min.x, _max.y), new Vector2(_min.x, _min.y));
                }

                curPos.x += cellSize.x;
            }
        }

        //draw center
        Gizmos.color = cClr;
                
        for(int r = 0; r < numRow; r++) {
            var curPos = new Vector2(min.x + originCol*cellSize.x, min.y + r * cellSize.y);

            Gizmos.DrawLine(new Vector2(curPos.x, curPos.y), new Vector2(curPos.x + cellSize.x, curPos.y));
            Gizmos.DrawLine(new Vector2(curPos.x + cellSize.x, curPos.y), new Vector2(curPos.x + cellSize.x, curPos.y + cellSize.y));
            Gizmos.DrawLine(new Vector2(curPos.x + cellSize.x, curPos.y + cellSize.y), new Vector2(curPos.x, curPos.y + cellSize.y));
            Gizmos.DrawLine(new Vector2(curPos.x, curPos.y + cellSize.y), new Vector2(curPos.x, curPos.y));
        }

        for(int c = 0; c < numCol; c++) {
            var curPos = new Vector2(min.x + c * cellSize.x, min.y + originRow * cellSize.y);

            Gizmos.DrawLine(new Vector2(curPos.x, curPos.y), new Vector2(curPos.x + cellSize.x, curPos.y));
            Gizmos.DrawLine(new Vector2(curPos.x + cellSize.x, curPos.y), new Vector2(curPos.x + cellSize.x, curPos.y + cellSize.y));
            Gizmos.DrawLine(new Vector2(curPos.x + cellSize.x, curPos.y + cellSize.y), new Vector2(curPos.x, curPos.y + cellSize.y));
            Gizmos.DrawLine(new Vector2(curPos.x, curPos.y + cellSize.y), new Vector2(curPos.x, curPos.y));
        }
    }
}
