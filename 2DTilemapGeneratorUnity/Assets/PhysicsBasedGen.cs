using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PhysicsBasedGen : MonoBehaviour
{
    public InitializeTilemap initTilemap;
    public Dictionary<string, TileBase> tiles;
    public GenSettings settings;
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
        return new Vector3Int(settings.bounds.x, UnityEngine.Random.Range(settings.bounds.y, settings.bounds.yMax));
    }
    /**
    <summary> Function, which returns a random tile on the right side of the boundary </summary>
    <returns> A vector containing the location </returns>
    **/
    Vector3Int GetRightSideRandom(){
        return new Vector3Int(settings.bounds.xMax - 1, UnityEngine.Random.Range(settings.bounds.y, settings.bounds.yMax));
    }
    /**
    <summary> Function, which returns a random tile on the bottom side of the boundary </summary>
    <returns> A vector containing the location </returns>
    **/
    Vector3Int GetBottomSideRandom(){
        return new Vector3Int(UnityEngine.Random.Range(settings.bounds.x, settings.bounds.xMax), settings.bounds.y);
    }
    /**
    <summary> Function, which returns a random tile on the top side of the boundary </summary>
    <returns> A vector containing the location </returns>
    **/
    Vector3Int GetTopSideRandom(){
        return new Vector3Int(UnityEngine.Random.Range(settings.bounds.x, settings.bounds.xMax), settings.bounds.yMax - 1);
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
    <param name="tilemap"> tilemap, for which the river is constructed on </param>
    <param name="water_points"> locations of which river will accelerate towards </param>
    <param name="iterations"> how many water nodes will river contain </param>
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
                //vel = vel.normalized;
                river_tiles.Add(new Vector3Int((int)start_point.x, (int)start_point.y));
                tilemap.SetTile(river_tiles[^1], initTilemap.tiles["water_0"]);
            }catch{

            }
        }
    }
    /**
    <summary> Function, which adds grass base and the river to the referenced tilemap in the editor </summary>
    **/
    void GenerateBaseMap(){
        Tilemap tilemap = initTilemap.GetTilemap();
        for(int i = settings.bounds.y - offset; i < settings.bounds.yMax + offset; ++i){
            for(int j = settings.bounds.x - offset; j < settings.bounds.xMax + offset; ++j){
                tilemap.SetTile(new Vector3Int(j, i), initTilemap.tiles["grass_0"]);
            }
        }
        Vector3Int[] water_tiles = GetFourRandomTiles();
        foreach(var loc in water_tiles){
            tilemap.SetTile(loc, initTilemap.tiles["water_0"]);
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
    public string GetDark(string name){
        switch (name)
        {
            case "grass_0":
                return "dark_grass_0";
            case "water_0":
                return "dark_water_0";
            case "dark_grass_0":
                return "dark_grass_0";
            case "mountain_0":
                return "dark_mountain_0";
            case "dark_mountain_0":
                return "snow_0";
            case "snow_0":
                return "snow_0";
            case "sand_0":
                return "dark_sand_0";
            default:
                return "sand_0";
        }
    }
    /**
    <summary> Function which details the map by making tiles to their darker version depending on the surrounding amount of same tiles.
    Function also replaces dark grass into flowers depending on flower probability </summary>
    **/
    public void DetailTheMap(bool addFlowers){
        Tilemap tilemap = initTilemap.GetTilemap();
        // turn tiles into darker versions
        for(int y = settings.bounds.y - offset; y < settings.bounds.yMax + offset; ++y){
            for(int x = settings.bounds.x - offset; x < settings.bounds.xMax + offset; ++x){
                try{
                    TileBase tile = tilemap.GetTile(new Vector3Int(x, y));
                    // check surrounding tiles
                    if(IsSurrondingSame(tilemap, tile, GetDark(tile.name), new Vector3Int(x, y)) >= 3){
                        if(tile.name == "grass_0"){tilemap.SetTile(new Vector3Int(x, y), initTilemap.tiles["dark_grass_0"]);}
                        if(tile.name == "water_0"){tilemap.SetTile(new Vector3Int(x, y), initTilemap.tiles["dark_water_0"]);}
                        if(tile.name == "mountain_0"){tilemap.SetTile(new Vector3Int(x, y), initTilemap.tiles["dark_mountain_0"]);}
                        if(tile.name == "dark_mountain_0"){tilemap.SetTile(new Vector3Int(x, y), initTilemap.tiles["snow_0"]);}
                        if(tile.name == "sand_0"){tilemap.SetTile(new Vector3Int(x, y), initTilemap.tiles["dark_sand_0"]);}
                    }else if(tile.name == "water_0"){
                        Vector3Int new_loc = new Vector3Int(x + UnityEngine.Random.Range(-1, 2), y + UnityEngine.Random.Range(-1, 2));
                        TileBase ntile = tilemap.GetTile(new_loc);
                        if(ntile != null && ntile.name != "water_0" && ntile.name != "dark_water_0")
                            tilemap.SetTile(new_loc, initTilemap.tiles["sand_0"]);
                    }
                }catch{

                }
            }
        }
        if(!addFlowers) return;
        // change tiles to flowers!
        for(int y = settings.bounds.y - offset; y < settings.bounds.yMax + offset; ++y){
            for(int x = settings.bounds.x - offset; x < settings.bounds.xMax + offset; ++x){
                TileBase tile = tilemap.GetTile(new Vector3Int(x, y));
                if(tile != null && tile.name == "dark_grass_0"){
                    float p = UnityEngine.Random.Range(0, 1f);
                    if(p <= flowerProbability){
                        tilemap.SetTile(new Vector3Int(x, y), initTilemap.tiles["flower_0"]);
                    }
                }
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initTilemap.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if(gen){
            GenerateBaseMap();
            DetailTheMap(true);
            gen = false;
        }
    }
}
