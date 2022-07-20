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

    private void initializeGrid()
    {

        tileBoard = new ITile[(int)gSettings.GridSize.x, (int)gSettings.GridSize.y];

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

                int selectedTile = Random.Range(0, selectableTiles.Count);
                selectedTile = tileTypes.IndexOf(selectableTiles[selectedTile]);

                tmp = Instantiate(gSettings.TilePrefabs[selectedTile], gSettings.GridHolder.transform, false);
                Resize(tmp, gSettings.GridHolder.transform);

                tmpTile = tmp.GetComponent(typeof(ITile)) as ITile;
                tmpTile?.spawnAtPos(new Vector2(i, j));
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

    [System.Serializable]
    private class GameSettings
    {
        [Header("General Setting")]
        [SerializeField]
        private Vector2 _stableResolution = Vector2.zero;
        [SerializeField]
        private GameObject _gridHolder;
        [SerializeField]
        private Vector2 _gridSize = Vector2.one;

        [Header("Prefabs")]
        [SerializeField]
        private GameObject tileBG;
        [SerializeField]
        private List<GameObject> tilePrefabs;

        public Vector2 StableResolution { get => _stableResolution; }
        public Vector2 GridSize { get => _gridSize; }
        public List<GameObject> TilePrefabs { get => tilePrefabs; }
        public GameObject TileBG { get => tileBG; }
        public GameObject GridHolder { get => _gridHolder; }
    }
}
