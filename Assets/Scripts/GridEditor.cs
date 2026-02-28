using System;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class GridEditor : MonoBehaviour
{
    private Grid grid;
    public List<Tilemap> tilemaps;
    public Tile tile;
    //Current Tile Index
    public int tileIndex;
    private Vector3Int lastTilePos;
    private List<Vector3Int> LastPlacedTiles = new List<Vector3Int>();
    
    private TileLayer currentLayer = TileLayer.Default;
    
    //Used for ingame tracking
    private Dictionary<TileLayer, Dictionary<Vector3Int, Tile>> tileLayers =  new Dictionary<TileLayer, Dictionary<Vector3Int, Tile>>();
    //used for save data
    public Dictionary<Vector3Int, int> tilemap = new();
    //used for loading data (associate index numbers with tiles. Needs to be populated.)
    public Dictionary<int, Tile> tileDict = new();
    
    public bool Started = false;
    
    int UILayer;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tileDict.Add(0, tile);
        UILayer = LayerMask.NameToLayer("UI");
        grid = Component.FindFirstObjectByType(typeof(Grid)) as Grid;
        tileLayers.Add(TileLayer.Default, new Dictionary<Vector3Int, Tile>());
        tileLayers.Add(TileLayer.Walls, new Dictionary<Vector3Int, Tile>());
        tileLayers.Add(TileLayer.Deco, new Dictionary<Vector3Int, Tile>());
    }

    // Update is called once per frame
    void Update()
    {
        if (Started)
        {
            Vector3 MousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int CellMousePos = grid.WorldToCell(MousePos);
            
            if (lastTilePos != CellMousePos)
            {
                if (!LastPlacedTiles.Contains(lastTilePos))
                {
                    if (tileLayers[TileLayer.Default].ContainsKey(lastTilePos))
                    {
                        tilemaps[0].SetTile(lastTilePos, tileLayers[TileLayer.Default][lastTilePos]);
                    }
                    else
                    {
                        tilemaps[0].SetTile(lastTilePos, null);
                    }
                }
                if (tileLayers[TileLayer.Default].ContainsKey(lastTilePos))
                {
                    tilemaps[0].SetTile(lastTilePos, tileLayers[TileLayer.Default][lastTilePos]);
                }
                else
                {
                    tilemaps[0].SetTile(lastTilePos, null);
                }
                lastTilePos = CellMousePos;
                tilemaps[0].SetTile(CellMousePos, tile);
                tilemaps[0].SetColor(CellMousePos, Color.gray); 
            }
            
            if (Input.GetMouseButtonDown(0) && !IsPointerOverUIElement())
            {
                if (lastTilePos == CellMousePos)
                {
                    if (!tileLayers.ContainsKey(TileLayer.Default))
                    {
                        tileLayers.Add(TileLayer.Default, new Dictionary<Vector3Int, Tile>());
                    }

                    if (this.tileLayers[TileLayer.Default].ContainsKey(lastTilePos))
                    {
                            this.tileLayers[TileLayer.Default][lastTilePos] = tile;
                            tilemap[lastTilePos] = tileIndex;
                    }
                    else
                    {
                        tileLayers[TileLayer.Default].Add(CellMousePos, tile);
                        tilemap.Add(CellMousePos, tileIndex);
                    }
                    tilemaps[0].SetTile(CellMousePos, tile);
                    LastPlacedTiles.Add(CellMousePos);
                }
            }
        }
    }

    public Dictionary<TileLayer, Dictionary<Vector3Int, Tile>> GetGridSaveData()
    {
        return tileLayers;
    }
    
    
    //Source: https://discussions.unity.com/t/how-to-detect-if-mouse-is-over-ui/821330
    //Returns 'true' if we touched or hovering on Unity UI element.
    public bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }


    //Returns 'true' if we touched or hovering on Unity UI element.
    private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == UILayer)
                return true;
        }
        return false;
    }
    
    static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }

    public void loadMap()
    {
        if (tileDict.Count ! > 0)
        {
            Debug.Log("Tile dictionary is empty");
        }
        else
        {
            Debug.Log($"TileDict entries: {tileDict.Count}.");
        }
        Debug.Log("Loading Map");
        foreach (var (vec, tid) in tilemap)
        {
            tileLayers[TileLayer.Default].Add(vec, tileDict[tid]);
            tilemaps[0].SetTile(vec, tileDict[tid]);
        }
    }
}
