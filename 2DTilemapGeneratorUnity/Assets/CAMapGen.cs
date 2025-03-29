using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CAMapGen : MonoBehaviour
{
    public InitializeTilemap initTilemap;
    public GenSettings settings;
    public PhysicsBasedGen gc;
    public int celluralAutomataGen = 10;
    public bool gen = false;
    int CountNeighbours(int[] map, int i, int width, int height){
        int[] surroundNodes = new int[]{
        1, 0,
        -1, 0, 
        0, 1, 
        0, -1, 
        1, 1, 
       -1, 1, 
       1, -1, 
       -1, -1
       };
        int x = i % width + settings.bounds.x;
        int y = i / height + settings.bounds.y;
        int neighbours = 0;
        for(int j = 1; j < surroundNodes.Length; j += 2){
            int loc = (y + surroundNodes[j - 1])*height + x + surroundNodes[j];
            if(0 <= loc && loc < map.Length){
                if(map[loc] == 0){
                    neighbours += 1;
                }
            }else{
                neighbours += 1;
            }
                
        }
        return neighbours;
    }


    int[] GenerateAHeightMap(int generations){
        int width = Math.Abs(settings.bounds.xMax - settings.bounds.x);
        int height = Math.Abs(settings.bounds.yMax - settings.bounds.y);
        int[] heightMap = new int[width*height];
        float density = 60;
        for(int i = 0; i < heightMap.Length; ++i){
            if(UnityEngine.Random.Range(1, 101) > density){
                heightMap[i] = 1;
            }else{
                heightMap[i] = 0;
            }
                
        }
        for(int generation = 0; generation < generations; ++generation){
            int[] tempMap = new int[width*height];
            for(int i = 0; i < heightMap.Length; ++i){
                tempMap[i] = heightMap[i];
            }
            for(int i = 0; i < heightMap.Length; ++i){
                int info = CountNeighbours(tempMap, i, width, height);
                if(info > 4){
                    heightMap[i] = 0;
                }else{
                    heightMap[i] = 1;
                }
            }
        }
        return heightMap;
    }

    void GenerateMapBasedOnRandomHeightMap(){
        int[] map = GenerateAHeightMap(celluralAutomataGen);
        initTilemap.Start();
        Tilemap tilemap = initTilemap.GetTilemap();
        int width = Math.Abs(settings.bounds.xMax - settings.bounds.x);
        int height = Math.Abs(settings.bounds.yMax - settings.bounds.y);
        // turn tiles into darker versions
        for(int i = 0; i < map.Length; ++i){
            int x = i % width + settings.bounds.x;
            int y = i / height + settings.bounds.y;
            if(map[i] == 0){
                tilemap.SetTile(new Vector3Int(x, y), initTilemap.tiles["water_0"]);
            }else if(map[i] == 1){
                tilemap.SetTile(new Vector3Int(x, y), initTilemap.tiles["grass_0"]);
            }
        }
        float waterRatio = 0;
        float grassRatio = 0;
        for(int i = 0; i < map.Length; ++i){
            int x = i % width + settings.bounds.x;
            int y = i / height + settings.bounds.y;
            TileBase tile = tilemap.GetTile(new Vector3Int(x, y));
            if(tile != null && tile.name == "water_0"){
                waterRatio += 1;
            }else if(tile != null && tile.name == "grass_0"){
                grassRatio += 1;
            }
        }
        Debug.Log("W: " + waterRatio/map.Length + " G: " + grassRatio/map.Length);
        // generate dark grass areas
        gc.DetailTheMap(false);
        // add mountains
        AddLayerOnTile(map, width, height, "mountain_0", 20, 0.001f, false, false, "dark_grass_0");
        AddLayerOnTile(map, width, height, "forest_tile_0", 20, 0.001f, false, false, "dark_grass_0");
        AddLayerOnTile(map, width, height, "grass_0", 20, 0.001f, false, false, "dark_grass_0");
        // generate dark mountains
        gc.DetailTheMap(false);
        // add snow layers to mountains
        gc.DetailTheMap(false);
        gc.DetailTheMap(true);
        AddLayerOnTile(map, width, height, "grass_0", 3, 0.0001f, false, false, "dark_grass_0");
    }

    void AddLayerOnTile(int[] map, int width, int height, string tileName, int iterations, float percentage, bool applyOnAll = false, bool rememberTiles = false, params string[] applyOnTiles){
        Tilemap tilemap = initTilemap.GetTilemap();
        List<(int x, int y)> coords = new List<(int x, int y)>();
        // turn tiles into darker versions
        for(int i = 0; i < map.Length; ++i){
            int x = i % width + settings.bounds.x;
            int y = i / height + settings.bounds.y;
            Vector3Int loc = new Vector3Int(x, y);
            TileBase tile = tilemap.GetTile(loc);
            foreach(var applyTile in applyOnTiles){
                if(tile != null && tile.name == applyTile && UnityEngine.Random.Range(0f, 1f) <= percentage){
                    tilemap.SetTile(loc, initTilemap.tiles[tileName]);
                    coords.Add((loc.x, loc.y));
                }
            }
        }
        for(int j = 0; j < iterations; ++j){
            for(int i = 0; i < map.Length; ++i){
                int x, y;
                Vector3Int loc;
                if(!rememberTiles){
                    x = i % width + settings.bounds.x;
                    y = i / height + settings.bounds.y;
                }else{
                    x = coords[i % coords.Count].x;
                    y = coords[i % coords.Count].y;
                }
                loc = new Vector3Int(x, y);
                TileBase tile = tilemap.GetTile(loc);
                if(tile != null && tile.name == tileName){
                    Vector3Int newLoc = loc + new Vector3Int(UnityEngine.Random.Range(-1, 2), UnityEngine.Random.Range(-1, 2));
                    TileBase secondTile = tilemap.GetTile(newLoc);
                    foreach(var applyTile in applyOnTiles){
                        if(secondTile != null && (secondTile.name == applyTile || applyOnAll)){
                            tilemap.SetTile(newLoc, initTilemap.tiles[tileName]);
                            coords.Add((newLoc.x, newLoc.y));
                        }
                    }
                }
            }
            }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(gen){
            GenerateMapBasedOnRandomHeightMap();
            gen = false;
        }
    }
}
