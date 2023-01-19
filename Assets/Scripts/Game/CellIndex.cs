using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CellIndex : IComparer, IComparer<CellIndex> {
    public int row;
    public int col;

    public bool isValid { get { return row >= 0 && col >= 0; } }

    public void Invalidate() {
        row = col = -1;
    }

    public CellIndex(int aRow, int aCol) {
        row = aRow;
        col = aCol;
    }

    public override int GetHashCode() {
        return base.GetHashCode();
    }

    public override bool Equals(object obj) {
        if(obj is CellIndex) {
            var other = (CellIndex)obj;
            return row == other.row && col == other.col;
        }
        else
            return base.Equals(obj);
    }

    public int Compare(object x, object y) {
        return Compare((CellIndex)x, (CellIndex)y);
    }

    public int Compare(CellIndex x, CellIndex y) {
        if(x.row < y.row)
            return -1;
        else if(x.row > y.row)
            return 1;
        else if(x.row == y.row) {
            if(x.col != y.col)
                return x.col < y.col ? -1 : 1;
        }

        return 0;
    }

    public static bool operator ==(CellIndex a, CellIndex b) {
        return a.row == b.row && a.col == b.col;
    }

    public static bool operator !=(CellIndex a, CellIndex b) {
        return a.row != b.row || a.col != b.col;
    }
}
