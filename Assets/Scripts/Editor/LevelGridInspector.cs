using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelGrid))]
public class LevelGridInspector : Editor {
    
    private BoxCollider2D mBoxColl;
    private LevelTile[,] mTileCells;

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        var dat = target as LevelGrid;

        if(GUI.changed) {
            //update box collider
            if(!mBoxColl)
                mBoxColl = dat.GetComponent<BoxCollider2D>();
            if(mBoxColl) {
                mBoxColl.offset = Vector2.zero;
                mBoxColl.size = new Vector2(dat.cellSize.x * dat.numCol, dat.cellSize.y * dat.numRow);
            }

            //update grid cells
            for(int i = 0; i < dat.gridTiles.Length; i++)
                dat.gridTiles[i].Apply(dat);
        }

        M8.EditorExt.Utility.DrawSeparator();

        //editor only stuff

        GUI.enabled = !Application.isPlaying;

        if(GUILayout.Button("Generate Walls")) {
            if(dat.wallRoot) {
                if(mTileCells == null)
                    mTileCells = dat.GenerateTileCells();

                //clear out walls
                while(dat.wallRoot.childCount > 0)
                    DestroyImmediate(dat.wallRoot.GetChild(0).gameObject);
                //for(int i = 0; i < dat.wallRoot.childCount; i++)
                //DestroyImmediate(dat.wallRoot.GetChild(i).gameObject);

                //generate horizontals
                for(int r = 0; r < mTileCells.GetLength(0); r++) {
                    int sCol = -1;
                    int eCol = -1;

                    //bottom
                    for(int c = 0; c < mTileCells.GetLength(1); c++) {
                        bool isWall = false;

                        var cell = mTileCells[r, c];
                        if(cell != null && cell.isWallS)
                            isWall = true;
                        else if(r > 0) { //check cell below
                            cell = mTileCells[r - 1, c];
                            if(cell != null && cell.isWallN)
                                isWall = true;
                        }

                        if(isWall) {
                            if(sCol == -1) {
                                sCol = eCol = c;
                            }
                            else
                                eCol = c;
                        }
                        else if(sCol != -1) {
                            ApplyWallHorizontal(dat, sCol, eCol, r, false, true);
                            ApplyWallHorizontal(dat, sCol, eCol, r, true, true);
                            sCol = -1;
                        }
                    }

                    if(sCol != -1) {
                        ApplyWallHorizontal(dat, sCol, eCol, r, false, true);
                        ApplyWallHorizontal(dat, sCol, eCol, r, true, true);
                    }

                    //top
                    if(r == mTileCells.GetLength(0) - 1) {
                        sCol = -1;
                        for(int c = 0; c < mTileCells.GetLength(1); c++) {
                            var cell = mTileCells[r, c];
                            if(cell != null) {
                                if(cell.isWallN) {
                                    if(sCol == -1) {
                                        sCol = eCol = c;
                                    }
                                    else
                                        eCol = c;
                                }
                                else if(sCol != -1) {
                                    ApplyWallHorizontal(dat, sCol, eCol, r, false, false);
                                    ApplyWallHorizontal(dat, sCol, eCol, r, true, false);
                                    sCol = -1;
                                }
                            }
                        }

                        if(sCol != -1) {
                            ApplyWallHorizontal(dat, sCol, eCol, r, false, false);
                            ApplyWallHorizontal(dat, sCol, eCol, r, true, false);
                        }
                    }
                }

                //generate verticals
                for(int c = 0; c < mTileCells.GetLength(1); c++) {
                    int sRow = -1;
                    int eRow = -1;

                    //left
                    for(int r = 0; r < mTileCells.GetLength(0); r++) {
                        bool isWall = false;

                        var cell = mTileCells[r, c];
                        if(cell != null && cell.isWallW)
                            isWall = true;
                        else if(c > 0) { //check cell left
                            cell = mTileCells[r, c - 1];
                            if(cell != null && cell.isWallE)
                                isWall = true;
                        }

                        if(isWall) {
                            if(sRow == -1) {
                                sRow = eRow = r;
                            }
                            else
                                eRow = r;
                        }
                        else if(sRow != -1) {
                            ApplyWallVertical(dat, sRow, eRow, c, false, true);
                            ApplyWallVertical(dat, sRow, eRow, c, true, true);
                            sRow = -1;
                        }
                    }

                    if(sRow != -1) {
                        ApplyWallVertical(dat, sRow, eRow, c, false, true);
                        ApplyWallVertical(dat, sRow, eRow, c, true, true);
                    }

                    //right
                    if(c == mTileCells.GetLength(1) - 1) {
                        sRow = -1;
                        for(int r = 0; r < mTileCells.GetLength(0); r++) {
                            var cell = mTileCells[r, c];
                            if(cell != null) {
                                if(cell.isWallE) {
                                    if(sRow == -1) {
                                        sRow = eRow = r;
                                    }
                                    else
                                        eRow = r;
                                }
                                else if(sRow != -1) {
                                    ApplyWallVertical(dat, sRow, eRow, c, false, false);
                                    ApplyWallVertical(dat, sRow, eRow, c, true, false);
                                    sRow = -1;
                                }
                            }
                        }

                        if(sRow != -1) {
                            ApplyWallVertical(dat, sRow, eRow, c, false, false);
                            ApplyWallVertical(dat, sRow, eRow, c, true, false);
                        }
                    }
                }
            }
        }

        bool isApplyCells = false, isCreateCell = false;

        if(GUILayout.Button("Generate Cells")) {
            isApplyCells = true;
            isCreateCell = true;
        }

        if(GUILayout.Button("Refresh Cells")) {
            isApplyCells = true;
        }

        if(isApplyCells) {
            if(mTileCells == null || mTileCells.GetLength(0) != dat.numRow || mTileCells.GetLength(1) != dat.numCol)
                mTileCells = dat.GenerateTileCells();

            for(int r = 0; r < dat.numRow; r++) {
                for(int c = 0; c < dat.numCol; c++) {
                    var cell = new CellIndex(r, c);

                    var quadType = dat.GetQuadrant(cell);

                    LevelGrid.CellGeneratorInfo cellGenInfo;
                    switch(quadType) {
                        case QuadrantType.Quadrant1:
                        case QuadrantType.Quadrant2:
                        case QuadrantType.Quadrant3:
                        case QuadrantType.Quadrant4:
                            int quadIndex = (int)quadType - 1;
                            if(dat.cellGenQuadrantInfos != null && dat.cellGenQuadrantInfos.Length > 0) {
                                //clamp
                                if(quadIndex < 0)
                                    quadIndex = 0;
                                else if(quadIndex >= dat.cellGenQuadrantInfos.Length)
                                    quadIndex = dat.cellGenQuadrantInfos.Length - 1;

                                cellGenInfo = dat.cellGenQuadrantInfos[quadIndex];
                            }
                            else
                                cellGenInfo = dat.cellGenAxisInfo;
                            break;

                        default:
                            cellGenInfo = dat.cellGenAxisInfo;
                            break;
                    }

                    //check existing tile
                    var tile = mTileCells[r, c];
                    if(!tile && isCreateCell) {
                        //generate
                        tile = Instantiate(dat.cellGenTemplate, dat.GetCellPosition(cell), Quaternion.identity, dat.tilesRoot);
                        mTileCells[r, c] = tile;
                    }

                    if(tile) {
                        tile.name = string.Format("{0:D2}:{1:D2}", c, r);
                        tile.transform.SetAsLastSibling();

                        //apply display
                        var spritePalette = tile.tileSpritePalette;
                        if(spritePalette) {
                            spritePalette.index = cellGenInfo.paletteIndex;
                            spritePalette.brightness = 1f + cellGenInfo.brightnessOffset;
                        }
                    }
                }
            }
        }
    }

    private void ApplyWallHorizontal(LevelGrid levelGrid, int sCol, int eCol, int row, bool isBack, bool isBottom) {
        SpriteRenderer sprRenderTemplate;
        Vector2 ofs;

        if(isBack) {
            sprRenderTemplate = levelGrid.wallLineHBackTemplate;
            ofs = levelGrid.wallLineBackOfs;
        }
        else {
            sprRenderTemplate = levelGrid.wallLineHTemplate;
            ofs = levelGrid.wallLineOfs;
        }

        var sPos = levelGrid.GetCellPosition(sCol, row);
        sPos.x -= levelGrid.cellSize.x * 0.5f + ofs.x;

        if(isBottom)
            sPos.y -= levelGrid.cellSize.y * 0.5f;
        else
            sPos.y += levelGrid.cellSize.y * 0.5f;

        var ePos = levelGrid.GetCellPosition(eCol, row);
        ePos.x += levelGrid.cellSize.x * 0.5f + ofs.x;

        ePos.y = sPos.y;

        var sprRender = Instantiate(sprRenderTemplate, levelGrid.wallRoot);
        sprRender.size = new Vector2(ePos.x - sPos.x, sprRender.size.y);

        sprRender.transform.position = new Vector2(Mathf.Lerp(sPos.x, ePos.x, 0.5f), sPos.y) + levelGrid.wallLinePosOfs;
    }

    private void ApplyWallVertical(LevelGrid levelGrid, int sRow, int eRow, int col, bool isBack, bool isLeft) {
        SpriteRenderer sprRenderTemplate;
        Vector2 ofs;

        if(isBack) {
            sprRenderTemplate = levelGrid.wallLineVBackTemplate;
            ofs = levelGrid.wallLineBackOfs;
        }
        else {
            sprRenderTemplate = levelGrid.wallLineVTemplate;
            ofs = levelGrid.wallLineOfs;
        }

        var sPos = levelGrid.GetCellPosition(col, sRow);
        sPos.y -= levelGrid.cellSize.y * 0.5f + ofs.y;

        if(isLeft)
            sPos.x -= levelGrid.cellSize.x * 0.5f;
        else
            sPos.x += levelGrid.cellSize.x * 0.5f;

        var ePos = levelGrid.GetCellPosition(col, eRow);
        ePos.y += levelGrid.cellSize.y * 0.5f + ofs.y;

        ePos.x = sPos.x;

        var sprRender = Instantiate(sprRenderTemplate, levelGrid.wallRoot);
        sprRender.size = new Vector2(sprRender.size.x, ePos.y - sPos.y);

        sprRender.transform.position = new Vector2(sPos.x, Mathf.Lerp(sPos.y, ePos.y, 0.5f)) + levelGrid.wallLinePosOfs;
    }
}
