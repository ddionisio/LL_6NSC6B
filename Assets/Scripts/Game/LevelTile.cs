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

    [Header("Tile")]
    public Sprite[] tileSpriteVariants;
    public SpriteRenderer tileSpriteRender;

    [Header("Displays")]
    public Transform displayRoot;

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

    public M8.SpriteColorFromPalette tileSpritePalette {
        get {
            if(!mTileSpritePalette) {
                if(tileSpriteRender)
                    mTileSpritePalette = tileSpriteRender.GetComponent<M8.SpriteColorFromPalette>();
            }

            return mTileSpritePalette;
        }
    }

    [HideInInspector]
    [SerializeField]
    int _col = -1;
    [SerializeField]
    [HideInInspector]
    int _row = -1;

    private struct BrightFadeInfo {
        public float brightOfs;
        public float delay;
        public float lastTime;
    }

    private const int brightCapacity = 4;
    private M8.CacheList<BrightFadeInfo> mBrightFades = new M8.CacheList<BrightFadeInfo>(brightCapacity);

    private LevelGrid mLevelGrid;
    private M8.SpriteColorFromPalette mTileSpritePalette;

    private float mTileDefaultBrightness;
    private Coroutine mTileBrightFadeRout;

    public void BrightnessFade(float brightOfs, float delay) {
        if(mBrightFades.IsFull) {
            //remove oldest
            int oldestInd = 0;
            for(int i = 1; i < mBrightFades.Count; i++) {
                if(mBrightFades[i].lastTime < mBrightFades[oldestInd].lastTime)
                    oldestInd = i;
            }

            mBrightFades.RemoveAt(oldestInd);
        }

        mBrightFades.Add(new BrightFadeInfo { brightOfs=brightOfs, delay=delay, lastTime=Time.time });

        if(mTileBrightFadeRout == null)
            mTileBrightFadeRout = StartCoroutine(DoBrightnessFade());
    }

    public void BrightnessReset() {
        if(tileSpritePalette)
            tileSpritePalette.brightness = mTileDefaultBrightness;

        mBrightFades.Clear();

        if(mTileBrightFadeRout != null) {
            StopCoroutine(mTileBrightFadeRout);
            mTileBrightFadeRout = null;
        }
    }

    void OnDisable() {
        if(Application.isPlaying)
            BrightnessReset();
    }

    void Awake() {
        if(tileSpriteRender && tileSpriteVariants.Length > 0) {
            tileSpriteRender.sprite = tileSpriteVariants[Random.Range(0, tileSpriteVariants.Length)];
        }

        if(tileSpritePalette)
            mTileDefaultBrightness = tileSpritePalette.brightness;
    }

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

    IEnumerator DoBrightnessFade() {
        while(mBrightFades.Count > 0) {
            var brightness = mTileDefaultBrightness;

            for(int i = mBrightFades.Count - 1; i >= 0; i--) {
                var brightFade = mBrightFades[i];

                float curTime = Time.time - brightFade.lastTime;
                if(curTime < brightFade.delay) {
                    float t = Mathf.Clamp01(curTime / brightFade.delay);
                    brightness += brightFade.brightOfs * (1.0f - t);
                }
                else
                    mBrightFades.RemoveAt(i);
            }

            tileSpritePalette.brightness = brightness;

            yield return null;
        }

        mTileBrightFadeRout = null;
    }

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

            //this is here so this gameobject can be selected properly
            Gizmos.color = Color.clear;
            Gizmos.DrawCube(transform.position, new Vector3(levelGrid.cellSize.x, levelGrid.cellSize.y, 0f));
        }
    }
}
