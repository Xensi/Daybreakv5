using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class LevelGenerator : MonoBehaviour
{
    public Texture2D map;
    public Texture2D placementAllowanceMap;
    public ColorToPrefab[] colorMappings;
    public Transform Bottomleftsquare;
    public Board board;
    public GameObject placementPrefab;
    public Color placementColor;
    public List<GameObject> placementTilesList;

    public List<Texture2D> mapList;
    public List<Texture2D> placementAllowanceMapList;
    public List<Terrain> terrainList;
    //public Color ignoreColor;
    public List<GameObject> tileList;

    // Start is called before the first frame update
    void Start()
    {
        //GenerateLevel();
    }
    public void DestroyLevel(string level)
    {
        /*foreach (var terrain in terrainList)
        {
            terrain.gameObject.SetActive(false);
        }*/
        foreach (var tile in tileList)
        {
            Destroy(tile);
        }
        string sceneMap = level + "Map";
        SceneManager.UnloadSceneAsync(sceneMap);
    }
    public void SelectLevel(string strLevel)
    {
        foreach (var varmap in mapList)
        {
           //Debug.Log(varmap.ToString());
            if (varmap.ToString() == strLevel + " (UnityEngine.Texture2D)")
            {
                map = varmap;
            }
        }
        foreach (var varplacementmap in placementAllowanceMapList)
        {
           //Debug.Log(varplacementmap.ToString());
            if (varplacementmap.ToString() == strLevel + "Placement (UnityEngine.Texture2D)")
            {
                placementAllowanceMap = varplacementmap;
            }
        }

        string sceneMap = strLevel + "Map";
        SceneManager.LoadScene(sceneMap, LoadSceneMode.Additive);
    }

    public void FindBoard()
    {
        if (board == null)
        {
            board = GameObject.FindGameObjectWithTag("Board").GetComponent<Board>();
            //board.BOARD_SIZE = map.width;
        }
    }

    public void GenerateLevel() //later on we can make this take a variable to generate different maps
    {//i also need this to generate map data that can be read by units, and not just visuals

        transform.localScale = new Vector3(1f, 1, 1f);
        transform.position = Vector3.zero;
        for (int x = 0; x < map.width; x++)
        {
            for (int y = 0; y < map.height; y++)
            {
                GenerateTile(x, y);
                CheckPlacement(x, y);
            }
        }
        transform.localScale = new Vector3(1.5f, 1, 1.5f);
        transform.position = Bottomleftsquare.position;

        Vector3 temp = new Vector3 (0, .22f, 0);
        transform.position += temp;
    }
    void CheckPlacement(int x, int y)
    {
        Color pixelColor = placementAllowanceMap.GetPixel(x, y);

        if (pixelColor.a == 0) //ignore fully transparent pixels
        {
            if (board != null)
            {
                board.placementAllowedGrid[x, y] = 0;
            }
        }
        else //pixels that are colored are allowed placement
        {

            if (placementColor.Equals(pixelColor))
            {

                if (board != null)
                {
                    board.placementAllowedGrid[x, y] = 1;
                }
                Vector3 position = new Vector3(x, 0, y);
                var obj = Instantiate(placementPrefab, position, Quaternion.identity, transform);
                tileList.Add(obj);
                placementTilesList.Add(obj);
            }
        }
    }
    void GenerateTile(int x, int y)
    {
        Color pixelColor = map.GetPixel(x, y);

        if (pixelColor.a == 0) //ignore fully transparent pixels
        {
            return;
        }

        foreach (ColorToPrefab colorMapping in colorMappings)
        {
            if (colorMapping.color.Equals(pixelColor))
            {
                Vector3 position = new Vector3(x, 0, y);
                var obj = Instantiate(colorMapping.prefab, position, Quaternion.identity, transform);
                tileList.Add(obj);
                if (board != null)
                {
                    //Debug.Log(x + " " + y);
                    board.terrainGrid[x, y] = colorMapping.tileType;
                    //Debug.Log(board.terrainGrid[x, y]);
                }

                var tileData = obj.GetComponent(typeof(TileData)) as TileData;
                tileData.x = x; //this will allow tile to remember its position on the grid
                tileData.y = y;

            }
        }
    }
}
