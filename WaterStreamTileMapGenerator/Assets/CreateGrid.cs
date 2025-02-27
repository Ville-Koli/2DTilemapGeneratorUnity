using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CreateGrid : MonoBehaviour
{
    public InitializeTilemap initTilemap;
    public Dictionary<string, TileBase> tiles;
    public BoundsInt bounds;
    public float accConst = 0.5f;
    public int offset = 10;
    public int swap = 1;
    public int swapHead = 10;
    public int tileAmount = 300;
    public float flowerProbability = 0.1f;
    public bool gen = false;

    /**
    <summary> Function, which returns a random tile on the left side of the boundary </summary>
    <returns> A vector containing the location </returns>
    **/
    Vector3Int GetLeftSideRandom(){
        return new Vector3Int(bounds.x, UnityEngine.Random.Range(bounds.y, bounds.yMax));
    }
    /**
    <summary> Function, which returns a random tile on the right side of the boundary </summary>
    <returns> A vector containing the location </returns>
    **/
    Vector3Int GetRightSideRandom(){
        return new Vector3Int(bounds.xMax - 1, UnityEngine.Random.Range(bounds.y, bounds.yMax));
    }
    /**
    <summary> Function, which returns a random tile on the bottom side of the boundary </summary>
    <returns> A vector containing the location </returns>
    **/
    Vector3Int GetBottomSideRandom(){
        return new Vector3Int(UnityEngine.Random.Range(bounds.x, bounds.xMax), bounds.y);
    }
    /**
    <summary> Function, which returns a random tile on the top side of the boundary </summary>
    <returns> A vector containing the location </returns>
    **/
    Vector3Int GetTopSideRandom(){
        return new Vector3Int(UnityEngine.Random.Range(bounds.x, bounds.xMax), bounds.yMax - 1);
    }
    /**
    <summary> Function, which returns a random array of tile locations from left, bottom, right and top sides </summary>
    <returns> array containing random tiles from left, bottom, right, top sides </returns>
    **/

    Vector3Int[] GetFourRandomTiles(){
        Vector3Int[] tileLocs = new Vector3Int[]{GetLeftSideRandom(), GetBottomSideRandom(), GetRightSideRandom(), GetTopSideRandom()};
        return tileLocs;
    }
    /**
    <summary> Function, which makes the river to a tilemap </summary>
    <param name="tilemap"> tilemap, for which the river is constructed on </params>
    <param name="water_points"> locations of which river will accelerate towards </params>
    <param name="iterations"> how many water nodes will river contain </params>
    **/
    void MakeRiver(Tilemap tilemap, Vector3Int[] water_points, int iterations){
        int counter = 0;
        List<Vector3Int> river_tiles = new List<Vector3Int>();
        Vector3 start_point = water_points[counter];
        Vector3 vel = ((Vector3)water_points[UnityEngine.Random.Range(0, water_points.Length)] - start_point).normalized;
        Vector3 acc = ((Vector3)water_points[UnityEngine.Random.Range(0, water_points.Length)] - start_point).normalized * accConst;
        for(int i = 0; i < iterations; ++i){
            try{
                if(i % swap == 0){
                    acc = (water_points[counter] - start_point).normalized * accConst;
                    counter = (counter + 1) % water_points.Length;
                    vel = vel.normalized;
                }
                if(i % swapHead == 0){
                    start_point = river_tiles[UnityEngine.Random.Range(0, river_tiles.Count)];
                    vel = ((Vector3)water_points[UnityEngine.Random.Range(0, water_points.Length)] - start_point).normalized;
                }
                start_point += vel;
                vel += acc;
                vel = vel.normalized;
                river_tiles.Add(new Vector3Int((int)start_point.x, (int)start_point.y));
                tilemap.SetTile(river_tiles[^1], tiles["water_0"]);
            }catch{

            }
        }
    }
    /**
    <summary> Function, which adds grass base and the river to the referenced tilemap in the editor </summary>
    **/
    void GenerateBaseMap(){
        initTilemap.Start();
        Tilemap tilemap = initTilemap.GetTilemap();
        tiles = initTilemap.GetTileDict();
        for(int i = bounds.y - offset; i < bounds.yMax + offset; ++i){
            for(int j = bounds.x - offset; j < bounds.xMax + offset; ++j){
                tilemap.SetTile(new Vector3Int(j, i), tiles["grass_0"]);
            }
        }
        Vector3Int[] water_tiles = GetFourRandomTiles();
        foreach(var loc in water_tiles){
            tilemap.SetTile(loc, tiles["water_0"]);
        }
        MakeRiver(tilemap, water_tiles, tileAmount);
    }
    /**
    <summary> Function, which checks whether cTile is the same as tile or dark version of the tile </summary>
    <param name="tile"> tile, which we are checking </params>
    <param name="cTile"> current tile </params>
    <param name="dark"> string, which is the id for the dark version of the tile </params>
    <param name="sameTile"> current amount of same tiles, which is incremented by one if tile is same as cTile and decremented by one if not </params> 
    **/
    void CheckSurroundingTile(TileBase tile, TileBase cTile, string dark, ref int sameTile){
        if(tile != null && (tile.name == cTile.name || tile.name == dark)){ ++sameTile; } else if(tile != null) { --sameTile; }
    }
    /**
    <summary> Function, which checks all surrounding tiles and gives a value depending on surrounding tiles </summary>
    <param name="tile"> tile, which we are checking </params>
    <param name="cTile"> current tile </params>
    <param name="dark"> string, which is the id for the dark version of the tile </params>
    <param name="loc"> location of cTile in vector form </params> 
    <returns> amount of same tiles surrounding the current tile subtracted amount of not same tiles</returns>
    **/
    int IsSurrondingSame(Tilemap tilemap, TileBase cTile, string dark, Vector3Int loc){
        int sameTile = 0;
        TileBase tile;
        tile = tilemap.GetTile(loc + new Vector3Int(0, 1));
        CheckSurroundingTile(tile, cTile, dark, ref sameTile);
        tile = tilemap.GetTile(loc + new Vector3Int(0, -1));
        CheckSurroundingTile(tile, cTile, dark, ref sameTile);
        tile = tilemap.GetTile(loc + new Vector3Int(1, 0));
        CheckSurroundingTile(tile, cTile, dark, ref sameTile);
        tile = tilemap.GetTile(loc + new Vector3Int(-1, 0));
        CheckSurroundingTile(tile, cTile, dark, ref sameTile);
        return sameTile;
    }
    /**
    <summary> Function, which gives dark version of the tile  </summary>
    <param name="name"> tile name </params>
    <returns> dark version of the tile name </returns>
    **/
    string GetDark(string name){
        return name == "grass_0" ? "dark_grass_0" : (name == "water_0" ? "dark_water_0" : (name == "dark_grass_0" ? "dark_grass_0" : "sand_0"));
    }
    /**
    <summary> Function which details the map by making tiles to their darker version depending on the surrounding amount of same tiles.
    Function also replaces dark grass into flowers depending on flower probability </summary>
    **/
    void DetailTheMap(){
        initTilemap.Start();
        Tilemap tilemap = initTilemap.GetTilemap();
        tiles = initTilemap.GetTileDict();
        // turn tiles into darker versions
        for(int y = bounds.y - offset; y < bounds.yMax + offset; ++y){
            for(int x = bounds.x - offset; x < bounds.xMax + offset; ++x){
                TileBase tile = tilemap.GetTile(new Vector3Int(x, y));
                // check surrounding tiles
                if(IsSurrondingSame(tilemap, tile, GetDark(tile.name), new Vector3Int(x, y)) >= 3){
                    if(tile.name == "grass_0"){tilemap.SetTile(new Vector3Int(x, y), tiles["dark_grass_0"]);}
                    if(tile.name == "water_0"){tilemap.SetTile(new Vector3Int(x, y), tiles["dark_water_0"]);}
                }else if(tile.name == "water_0"){
                    Vector3Int new_loc = new Vector3Int(x + UnityEngine.Random.Range(-1, 2), y + UnityEngine.Random.Range(-1, 2));
                    TileBase ntile = tilemap.GetTile(new_loc);
                    if(ntile != null && ntile.name != "water_0" && ntile.name != "dark_water_0")
                        tilemap.SetTile(new_loc, tiles["sand_0"]);
                }
            }
        }
        // change tiles to flowers!
        for(int y = bounds.y - offset; y < bounds.yMax + offset; ++y){
            for(int x = bounds.x - offset; x < bounds.xMax + offset; ++x){
                TileBase tile = tilemap.GetTile(new Vector3Int(x, y));
                if(tile != null && tile.name == "dark_grass_0"){
                    float p = UnityEngine.Random.Range(0, 1f);
                    if(p <= flowerProbability){
                        tilemap.SetTile(new Vector3Int(x, y), tiles["flower_0"]);
                    }
                }
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateBaseMap();
        DetailTheMap();
    }

    // Update is called once per frame
    void Update()
    {
        if(gen){
            GenerateBaseMap();
            DetailTheMap();
            gen = false;
        }
    }
}
