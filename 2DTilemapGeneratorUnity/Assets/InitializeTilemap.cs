using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InitializeTilemap : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    private GameObject grid;
    public Dictionary<string, TileBase> tiles;
    private Tilemap tilemap;
    public Tilemap GetTilemap(){
        return tilemap;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        if(grid != null) return;
        tiles = new Dictionary<string, TileBase>();
        grid = Instantiate(prefab);
        tilemap = grid.GetComponentInChildren<Tilemap>();
        // make tiles dictionary based on tiles from tilemap
        TileBase[] tileArray = tilemap.GetTilesBlock(tilemap.cellBounds);
        for(int i = 0; i < tileArray.Length; ++i){
            if(tileArray[i] != null){
                if(!tiles.ContainsKey(tileArray[i].name)){
                    tiles.Add(tileArray[i].name, tileArray[i]);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
