using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance { get => instance; }

    [SerializeField]
    private GameSettings gSettings;

    private ITile[,] tileBoard;
    private List<Vector2> awaitingTileActionList;

    private bool isSelectedOnce = false;

    [SerializeField]
    private Vector3 tileOffset = Vector3.zero;

    public Vector3 TileOffset { get => tileOffset; }
    public Vector3 GridScale { get => gSettings.GridHolder.transform.localScale; }
    public float ScreenRatio { get => (Screen.width / gSettings.StableResolution.x) / (Screen.height / gSettings.StableResolution.y); }
    
    private void Awake()
    {
        if(Instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            initializeGrid();
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }
    private void Update()
    {
        if (gSettings.MCapturer.getCurrentMotion().Equals(MotionType.MOVEMENT))
        {
            if (!isSelectedOnce && awaitingTileActionList.Count == 0)
            {
                isSelectedOnce = true;

                Ray ray = Camera.main.ScreenPointToRay(gSettings.MCapturer.getFirstTap());
                RaycastHit2D rayHit = Physics2D.GetRayIntersection(ray);
                if (rayHit.transform == null)
                    return;

                ITile hitTile = rayHit.transform.gameObject.GetComponent(typeof(ITile)) as ITile;
                if (hitTile != null)
                {
                    Vector2 hitTilePos = hitTile.getTilePos();
                    Vector2 flipTilePos = new Vector2(hitTilePos.x, hitTilePos.y);

                    float verticalForce = gSettings.MCapturer.getVerticalMovementForce();
                    float horizontalForce = gSettings.MCapturer.getHorizontalMovementForce();
                    ITile flipTile = null;
                    if(Mathf.Abs(verticalForce) > Mathf.Abs(horizontalForce))
                    {
                        if(verticalForce > 0)
                            flipTilePos.x -= 1;
                        else
                            flipTilePos.x += 1;
                    }
                    else
                    {
                        if (horizontalForce > 0)
                            flipTilePos.y += 1;
                        else
                            flipTilePos.y -= 1;
                    }

                    if (flipTilePos.x < 0 || flipTilePos.x >= tileBoard.GetLength(0) || flipTilePos.y < 0 || flipTilePos.y >= tileBoard.GetLength(1))
                        return;

                    flipTile = tileBoard[(int)flipTilePos.x, (int)flipTilePos.y];
                    flipTile.moveToPos(hitTilePos);
                    hitTile.moveToPos(flipTilePos);
                    tileBoard[(int)hitTilePos.x, (int)hitTilePos.y] = flipTile;
                    tileBoard[(int)flipTilePos.x, (int)flipTilePos.y] = hitTile;

                    awaitingTileActionList.Add(hitTile.getTilePos());
                    awaitingTileActionList.Add(flipTile.getTilePos());
                    StartCoroutine(chainBubbleCheck(hitTile, flipTile));
                }
            }
        }
        else
        {
            isSelectedOnce = false;
        }
    }
    private void initializeGrid()
    {
        tileBoard = new ITile[(int)gSettings.GridSize.x, (int)gSettings.GridSize.y];
        awaitingTileActionList = new List<Vector2>();

        GameObject bg = null;
        GameObject tmp = null;
        ITile tmpTile = null;

        if (tileOffset.Equals(Vector3.zero))
        {
            bg = Instantiate(gSettings.TileBG, gSettings.GridHolder.transform, false);
            Resize(bg, gSettings.GridHolder.transform);
            bg.transform.localScale = bg.transform.localScale + Vector3.one * 0.01f;

            tileOffset = bg.transform.localScale;
            bg.GetComponent<SpriteRenderer>().size = new Vector2(gSettings.GridSize.y, gSettings.GridSize.x);
        }

        List<string> tileTypes = new List<string>();
        for (int i = 0; i < gSettings.TilePrefabs.Count; i++)
        {
            tmpTile = gSettings.TilePrefabs[i].GetComponent(typeof(ITile)) as ITile;
            tileTypes.Add(tmpTile?.getTileType());
        }

        List<string> selectableTiles = new List<string>(tileTypes);
        for (int i = 0; i < gSettings.GridSize.x; i++)
        {
            for (int j = 0; j < gSettings.GridSize.y; j++)
            {
                #region Checking Previous Tiles
                string prevTileType = "";
                for (int k = j - 2; k < j && k >= 0; k++)
                {
                    tmpTile = tileBoard[i, k];
                    if (!prevTileType.Equals(""))
                    {
                        if (!prevTileType.Equals(tmpTile.getTileType()))
                            prevTileType = "";
                    }
                    else
                    {
                        prevTileType = tmpTile.getTileType();
                    }  
                }
                if (!prevTileType.Equals(""))
                {
                    selectableTiles.Remove(prevTileType);
                    prevTileType = "";
                }

                for (int k = i - 2; k < i && k >= 0; k++)
                {
                    tmpTile = tileBoard[k, j];
                    if (!prevTileType.Equals(""))
                    {
                        if (!prevTileType.Equals(tmpTile.getTileType()))
                            prevTileType = "";
                    }
                    else
                    {
                        prevTileType = tmpTile.getTileType();
                    }
                }
                if (!prevTileType.Equals(""))
                {
                    selectableTiles.Remove(prevTileType);
                    prevTileType = "";
                }

                #endregion

                int selectedTile = UnityEngine.Random.Range(0, selectableTiles.Count);
                selectedTile = tileTypes.IndexOf(selectableTiles[selectedTile]);

                tmp = Instantiate(gSettings.TilePrefabs[selectedTile], gSettings.GridHolder.transform, false);
                Resize(tmp, gSettings.GridHolder.transform);

                tmpTile = tmp.GetComponent(typeof(ITile)) as ITile;
                tmpTile.MovementDoneEvent += tileAnimationDoneListener;
                tmpTile?.spawnAtPos(new Vector2(i , j));

                tileBoard[i, j] = tmpTile;

                //Recreate selectables
                if(selectableTiles.Count < tileTypes.Count)
                    selectableTiles = new List<string>(tileTypes);
            }
        }
        #region Center Grid to the Screen
        float x = -(bg.transform.localScale.x * gSettings.GridSize.y / 2f);
        x += bg.transform.localScale.x / 2f;
        float y = bg.transform.localScale.y * gSettings.GridSize.x / 2f;
        y -= bg.transform.localScale.y / 2f;
        gSettings.GridHolder.transform.position = Vector3.Scale(gSettings.GridHolder.transform.localScale, new Vector3(x, y, 1));
        #endregion

        ITile bgTile = bg.GetComponent(typeof(ITile)) as ITile;
        bgTile?.spawnAtPos(new Vector2(gSettings.GridSize.x - 1, gSettings.GridSize.y - 1) / 2f);
    }

    private void Resize(GameObject go, Transform parent = null)
    {
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        transform.localScale = new Vector3(1, 1, 1);

        float width = sr.sprite.bounds.size.x;
        float height = sr.sprite.bounds.size.y;


        float worldScreenHeight = Camera.main.orthographicSize * 2f;
        float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

        Vector2 screenDimension = new Vector2(worldScreenWidth, worldScreenHeight);

        if(parent != null)
            screenDimension = Vector2.Scale(screenDimension, gSettings.GridHolder.transform.localScale);

        Vector3 newScale = go.transform.localScale;
        newScale.x = screenDimension.x / gSettings.GridSize.y;
        newScale.y = screenDimension.y / gSettings.GridSize.x;
        go.transform.localScale = newScale;

    }

    private void tileAnimationDoneListener (object sender, EventArgs e)
    {
        ITile s = sender as ITile;
        awaitingTileActionList.Remove(s.getTilePos());
    }
    private IEnumerator chainBubbleCheck(ITile tile1 = null, ITile tile2 = null)
    {
        yield return new WaitUntil(() => { return awaitingTileActionList.Count == 0; });
        bool isAnyBubble = false;
        while (checkForBubble())
        {
            if(!isAnyBubble)
                isAnyBubble = true;
            
            shiftGridItems();
            yield return new WaitUntil(() => { return (awaitingTileActionList.Count == 0); });
        }
        if (!isAnyBubble && tile1 != null && tile2 != null)
        {
            tile1.revertMovement();
            tile2.revertMovement();
            Vector2 tile1Pos = tile1.getTilePos();
            Vector2 tile2Pos = tile2.getTilePos();
            tileBoard[(int)tile1Pos.x, (int)tile1Pos.y] = tile1;
            tileBoard[(int)tile2Pos.x, (int)tile2Pos.y] = tile2;
        }
    }
    private bool checkForBubble()
    {
        List<ITile> bubbleList = new List<ITile>();

        for (int i = 0; i < tileBoard.GetLength(0); i++)
        {
            for (int j = 0; j < tileBoard.GetLength(1); j++)
            {
                if(i - 1 >= 0 && i + 1 < tileBoard.GetLength(0))
                {
                    if (tileBoard[i - 1, j].getTileType().Equals(tileBoard[i, j].getTileType())
                    && tileBoard[i, j].getTileType().Equals(tileBoard[i + 1, j].getTileType()))
                    {
                        tileBoard[i - 1, j].Bubble();
                        tileBoard[i, j].Bubble();
                        tileBoard[i + 1, j].Bubble();
                        bubbleList.Add(tileBoard[i - 1, j]);
                        bubbleList.Add(tileBoard[i, j]);
                        bubbleList.Add(tileBoard[i + 1, j]);
                    }
                }
                if(j - 1 >= 0 && j + 1 < tileBoard.GetLength(1))
                {
                    if (tileBoard[i, j - 1].getTileType().Equals(tileBoard[i, j].getTileType())
                        && tileBoard[i, j].getTileType().Equals(tileBoard[i, j + 1].getTileType()))
                    {
                        tileBoard[i, j - 1].Bubble();
                        tileBoard[i, j].Bubble();
                        tileBoard[i, j + 1].Bubble();
                        bubbleList.Add(tileBoard[i, j - 1]);
                        bubbleList.Add(tileBoard[i, j]);
                        bubbleList.Add(tileBoard[i, j + 1]);
                    }
                }

            }
        }
        if(bubbleList.Count > 0)
        {
            bubbleList = bubbleList.Distinct().ToList();
        }
        return (bubbleList.Count > 0);

    }
    private void shiftGridItems()
    {
        for (int i = 0; i < tileBoard.GetLength(1); i++)
        {
            List<ITile> column = new List<ITile>();
            for (int j = 0; j < tileBoard.GetLength(0); j++)
            {
                column.Add(tileBoard[j, i]);
            }
            column.RemoveAll(t => t.isBubbled());

            int startOffset = (int)tileBoard.GetLength(0) - column.Count;

            GameObject tmp;
            ITile tmpTile;
            int index = startOffset - 1;
            while(column.Count < tileBoard.GetLength(0))
            {
                int selectedTile = UnityEngine.Random.Range(0, gSettings.TilePrefabs.Count);
                tmp = Instantiate(gSettings.TilePrefabs[selectedTile], gSettings.GridHolder.transform, false);
                Resize(tmp, gSettings.GridHolder.transform);

                tmpTile = tmp.GetComponent(typeof(ITile)) as ITile;
                tmpTile.MovementDoneEvent += tileAnimationDoneListener;

                column.Insert(0, tmpTile);

                tmpTile.spawnAtPos(new Vector2(-startOffset + index, i));
                index--;
            }
            for (int j = 0; j < tileBoard.GetLength(0); j++)
            {
                tileBoard[j, i] = column[j];
                if(!tileBoard[j, i].getTilePos().Equals(new Vector2(j, i)))
                {
                    tileBoard[j, i].moveToPos(new Vector2(j, i));
                    awaitingTileActionList.Add(tileBoard[j, i].getTilePos());
                }

            }
        }
    }
    [System.Serializable]
    private class GameSettings
    {
        [Header("General Setting")]
        [SerializeField]
        private MotionCapturer mCapturer;
        [SerializeField]
        private Vector2 _stableResolution = Vector2.zero;
        [SerializeField]
        private Vector2 _gridSize = Vector2.one;

        [Header("Prefabs")]
        [SerializeField]
        private GameObject _gridHolder;
        [SerializeField]
        private GameObject tileBG;
        [SerializeField]
        private List<GameObject> tilePrefabs;

        public Vector2 StableResolution { get => _stableResolution; }
        public Vector2 GridSize { get => _gridSize; }
        public List<GameObject> TilePrefabs { get => tilePrefabs; }
        public GameObject TileBG { get => tileBG; }
        public GameObject GridHolder { get => _gridHolder; }
        public MotionCapturer MCapturer { get => mCapturer; }
    }
}
