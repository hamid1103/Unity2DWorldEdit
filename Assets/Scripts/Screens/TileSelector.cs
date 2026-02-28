using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class TileSelector : MonoBehaviour
{
    public GameManager manager;
    [SerializeField] public List<Tile> tiles;
    public GameObject tileHolder;
    public GameObject tileSelectorContent;
    public GridEditor gridEditor;
    private bool isOpen = false;
    private Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        var trans = tileSelectorContent.GetComponent<RectTransform>();
        var countX = 0;
        var countY = 0;
        
        //index will continue to count through categories.
        int index = 1; //Starting at index 1 because of the default blank tile
        foreach (var t in tiles)
        {
            if (countX == 3)
            {
                countX = 0;
                countY++;
                if (countY > 4) trans.sizeDelta = new Vector2(trans.rect.width, trans.rect.height + 64);
            }

            var newTile = Instantiate(tileHolder, tileSelectorContent.transform);
            newTile.GetComponent<TileSelectButton>().tile = t;
            newTile.GetComponent<TileSelectButton>().gameManager = manager;
            newTile.GetComponent<TileSelectButton>().index = index;
            
            gridEditor.tileDict.Add(index, t);
            
            newTile.transform.position = new Vector3(25 + newTile.transform.position.x + 64 * countX + 12,
                newTile.transform.position.y - 64 * countY, newTile.transform.position.z);
            countX++;
            newTile.GetComponent<Image>().sprite = t.sprite;
            index++;
        }

        trans.sizeDelta = new Vector2(trans.rect.width, trans.rect.height + 32);
    }

    public void ToggleOpen()
    {
        isOpen = !isOpen;
        animator.SetBool("Open", isOpen);
    }
}