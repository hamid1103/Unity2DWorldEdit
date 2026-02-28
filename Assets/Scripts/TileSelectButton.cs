using DefaultNamespace;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileSelectButton : MonoBehaviour
{
    public TileCategory tileCategory;
    public int index;
    public Tile tile;
    public GameManager gameManager;

    public void SetBrushTile()
    {
        gameManager.setTile(tile);
        gameManager.selectedTileIndex = index;
    }
}
