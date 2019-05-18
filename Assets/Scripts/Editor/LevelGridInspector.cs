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
                            ApplyWallHorizontal(dat, sCol, eCol, r, true);
                            sCol = -1;
                        }
                    }

                    if(sCol != -1)
                        ApplyWallHorizontal(dat, sCol, eCol, r, true);

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
                                    ApplyWallHorizontal(dat, sCol, eCol, r, false);
                                    sCol = -1;
                                }
                            }
                        }

                        if(sCol != -1)
                            ApplyWallHorizontal(dat, sCol, eCol, r, false);
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
                            ApplyWallVertical(dat, sRow, eRow, c, true);
                            sRow = -1;
                        }
                    }

                    if(sRow != -1)
                        ApplyWallVertical(dat, sRow, eRow, c, true);

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
                                    ApplyWallVertical(dat, sRow, eRow, c, false);
                                    sRow = -1;
                                }
                            }
                        }

                        if(sRow != -1)
                            ApplyWallVertical(dat, sRow, eRow, c, false);
                    }
                }
            }
        }
    }

    private void ApplyWallHorizontal(LevelGrid levelGrid, int sCol, int eCol, int row, bool isBottom) {
        var sPos = levelGrid.GetCellPosition(sCol, row);
        sPos.x -= levelGrid.cellSize.x * 0.5f + levelGrid.wallLineOfs.x;

        if(isBottom)
            sPos.y -= levelGrid.cellSize.y * 0.5f;
        else
            sPos.y += levelGrid.cellSize.y * 0.5f;

        var ePos = levelGrid.GetCellPosition(eCol, row);
        ePos.x += levelGrid.cellSize.x * 0.5f + levelGrid.wallLineOfs.x;

        ePos.y = sPos.y;

        var sprRender = Instantiate(levelGrid.wallLineHTemplate, levelGrid.wallRoot);
        sprRender.size = new Vector2(ePos.x - sPos.x, sprRender.size.y);

        sprRender.transform.position = new Vector2(Mathf.Lerp(sPos.x, ePos.x, 0.5f), sPos.y);
    }

    private void ApplyWallVertical(LevelGrid levelGrid, int sRow, int eRow, int col, bool isLeft) {
        var sPos = levelGrid.GetCellPosition(col, sRow);
        sPos.y -= levelGrid.cellSize.y * 0.5f + levelGrid.wallLineOfs.y;

        if(isLeft)
            sPos.x -= levelGrid.cellSize.x * 0.5f;
        else
            sPos.x += levelGrid.cellSize.x * 0.5f;

        var ePos = levelGrid.GetCellPosition(col, eRow);
        ePos.y += levelGrid.cellSize.y * 0.5f + levelGrid.wallLineOfs.y;

        ePos.x = sPos.x;

        var sprRender = Instantiate(levelGrid.wallLineVTemplate, levelGrid.wallRoot);
        sprRender.size = new Vector2(sprRender.size.x, ePos.y - sPos.y);

        sprRender.transform.position = new Vector2(sPos.x, Mathf.Lerp(sPos.y, ePos.y, 0.5f));
    }
}
