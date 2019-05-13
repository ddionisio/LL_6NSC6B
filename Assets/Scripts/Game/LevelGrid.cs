using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGrid : MonoBehaviour
{
    [Header("Grid Info")]
    public Vector2 cellSize = new Vector2(1f, 1f);
    public int originCol = 2;
    public int originRow = 2;
    public int numCol = 5;
    public int numRow = 5;
    public Transform tilesRoot;
    public Transform entitiesRoot;

    public Vector2 size { get { return new Vector2(numCol * cellSize.x, numRow * cellSize.y); } }
    public Vector2 extents { get { return new Vector2(numCol * cellSize.x * 0.5f, numRow * cellSize.y * 0.5f); } }
    public Vector2 min { get { return center - extents; } }
    public Vector2 max { get { return center + extents; } }
    public Vector2 center { get { return transform.position; } }

    public LevelTile[,] tileCells {
        get {
            if(mTileCells == null || mTileCells.GetLength(0) != numRow || mTileCells.GetLength(1) != numCol) {
                if(mTiles == null)
                    mTiles = tilesRoot ? tilesRoot.GetComponentsInChildren<LevelTile>() : GetComponentsInChildren<LevelTile>();

                mTileCells = new LevelTile[numRow, numCol];

                //go through tiles and place them
                for(int i = 0; i < mTiles.Length; i++) {
                    var tile = mTiles[i];

                    var cellInd = GetCellIndexLocal(tile.transform.localPosition);
                    if(cellInd.isValid)
                        mTileCells[cellInd.row, cellInd.col] = tile;
                }
            }

            return mTileCells;
        }
    }

    private LevelTile[,] mTileCells; //[row][col]
    private LevelTile[] mTiles; //[row][col]

    private const int entityListCapacity = 4;
    private M8.CacheList<LevelEntity>[,] mEntityCells;

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

    public LevelTile GetTile(Vector2 pos) {
        var cellInd = GetCellIndex(pos);
        if(cellInd.isValid)
            return tileCells[cellInd.row, cellInd.col];

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
