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

    public Vector2 size { get { return new Vector2(numCol * cellSize.x, numRow * cellSize.y); } }
    public Vector2 extents { get { return new Vector2(numCol * cellSize.x * 0.5f, numRow * cellSize.y * 0.5f); } }
    public Vector2 min { get { return center - extents; } }
    public Vector2 max { get { return center + extents; } }
    public Vector2 center { get { return transform.position; } }

    public LevelTile[][] tileCells {
        get {
            if(mTileCells == null || mTileCells.GetLength(0) != numRow || mTileCells.GetLength(1) != numCol) {
                if(mTiles == null)
                    mTiles = GetComponentsInChildren<LevelTile>();

                mTileCells = new LevelTile[numRow][];
                for(int r = 0; r < numRow; r++)
                    mTileCells[r] = new LevelTile[numCol];

                //go through tiles and place them
                for(int i = 0; i < mTiles.Length; i++) {
                    var tile = mTiles[i];

                    int col, row;
                    if(GetCellIndexLocal(tile.transform.localPosition, out col, out row))
                        mTileCells[row][col] = tile;
                }
            }

            return mTileCells;
        }
    }

    private LevelTile[][] mTileCells; //[row][col]
    private LevelTile[] mTiles;

    public LevelTile GetTile(Vector2 pos) {
        int r, c;
        if(GetCellIndex(pos, out c, out r))
            return tileCells[r][c];

        return null;
    }

    /// <summary>
    /// Grab cell index based on given world position. Returns true if valid.
    /// </summary>
    public bool GetCellIndex(Vector2 pos, out int col, out int row) {
        Vector2 lpos = transform.worldToLocalMatrix.MultiplyPoint3x4(pos);
        return GetCellIndexLocal(lpos, out col, out row);
    }

    /// <summary>
    /// Grab cell index based on given local position. Returns true if valid.
    /// </summary>
    public bool GetCellIndexLocal(Vector2 lpos, out int col, out int row) {
        lpos += extents;

        col = Mathf.FloorToInt(lpos.x / cellSize.x);
        if(col < 0 || col >= numCol)
            col = -1;

        row = Mathf.FloorToInt(lpos.y / cellSize.y);
        if(row < 0 || row >= numRow)
            row = -1;

        return col != -1 && row != -1;
    }

    /// <summary>
    /// Grab center of cell position (world) based on given col and row.
    /// </summary>
    public Vector2 GetCellPosition(int col, int row) {
        return min + new Vector2(col * cellSize.x, row * cellSize.y) + cellSize * 0.5f;
    }

    /// <summary>
    /// Outputs index relative to origin's 
    /// </summary>
    public bool GetCellIndexFromOrigin(Vector2 pos, out int col, out int row) {
        if(GetCellIndex(pos, out col, out row)) {
            CellIndexToOrigin(ref col, ref row);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Transform given indices to be relative to origin.
    /// </summary>
    public void CellIndexToOrigin(ref int col, ref int row) {
        col -= originCol;
        row -= originRow;
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
