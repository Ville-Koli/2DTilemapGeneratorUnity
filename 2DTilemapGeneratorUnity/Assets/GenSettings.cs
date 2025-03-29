using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GenSettings : MonoBehaviour
{
    public BoundsInt bounds;
    public int populationSize;
    public int[] GenerateRandomNoiseMap(int range){
        int width = Math.Abs(bounds.xMax - bounds.x);
        int height = Math.Abs(bounds.yMax - bounds.y);
        int[] heightMap = new int[width*height];
        for(int i = 0; i < heightMap.Length; ++i){
            heightMap[i] = UnityEngine.Random.Range(0, range);
        }
        return heightMap;
    }

    int GetIndexOfMaxElem(float[] arr){
        float max = 0;
        int mind = -1;
        for(int i = 0; i < arr.Length; ++i){
            if(max < arr[i]){
                max = arr[i];
                mind = i;
            }
        }
        return mind;
    }
    public int[] CountNeighbours(int[] map, int i, int width, int height){
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
        int x = i % width + bounds.x;
        int y = i / height + bounds.y;
        int[] neighbours = new int[11];
        for(int j = 1; j < surroundNodes.Length; j += 2){
            int loc = (y + surroundNodes[j - 1])*height + x + surroundNodes[j];
            if(0 <= loc && loc < map.Length && 0 <= map[loc] && map[loc] < neighbours.Length){
                neighbours[map[loc]] += 1;
            }else{
                if(0 <= i && i <= map.Length && 0 <= map[i] && map[i] < neighbours.Length){
                    neighbours[map[i]] += 1;
                }
            }
                
        }
        return neighbours;
    }
    public int[] CountNeighboursTestMap(Vector3Int location, Tilemap tilemap, string[] indexList){
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
        int[] neighbours = new int[indexList.Length];
        for(int j = 1; j < surroundNodes.Length; j += 2){
            Vector3Int newLoc = new Vector3Int(location.x + surroundNodes[j], location.y + surroundNodes[j - 1]);
            TileBase tile = tilemap.GetTile(newLoc);
            if(tile != null){
                int index = Array.IndexOf(indexList, tile.name);
                if(0 <= index && index < neighbours.Length){
                    neighbours[index] += 1;
                }
            }   
        }
        return neighbours;
    }
    public string GetRelationalDataFromLocation(int[] map, int i, int width, int height){
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
        int x = i % width + bounds.x;
        int y = i / height + bounds.y;
        string neighbours = "";
        for(int j = 1; j < surroundNodes.Length; j += 2){
            int loc = x + surroundNodes[j] + (y + surroundNodes[j-1]) * height;
            if(0 <= loc && loc < map.Length){
                int index = map[loc];
                neighbours += index;
            }   
        }
        return neighbours;
    }

    public string GetRelationalDataFromLocation(Vector3Int location, Tilemap tilemap, string[] indexList){
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
        string neighbours = "";
        for(int j = 1; j < surroundNodes.Length; j += 2){
            Vector3Int newLoc = new Vector3Int(location.x + surroundNodes[j], location.y + surroundNodes[j - 1]);
            TileBase tile = tilemap.GetTile(newLoc);
            if(tile != null){
                int index = Array.IndexOf(indexList, tile.name);
                neighbours += index;
            }   
        }
        return neighbours;
    }
    public int GetTileFromMapAndNeighbours(Dictionary<string, float[]> mapping, Vector3Int location, Tilemap tilemap, string[] indexList){
        float[] deduction = new float[indexList.Length];
        int[] neighbours = CountNeighboursTestMap(location, tilemap, indexList);
        for(int i = 0; i < neighbours.Length; ++i){
            if(neighbours[i] > 0){
                if(!mapping.ContainsKey(indexList[i])){
                    throw new Exception("AYO this aint right, where da fuck are you keeping that tile?!");
                }
                float[] deltaDeduction = mapping[indexList[i]];
                for(int j = 0; j < deltaDeduction.Length; ++j){
                    deduction[j] += deltaDeduction[j] * neighbours[i];
                }
            }
        }
        float sum = deduction.Sum();
        for(int i = 0; i < deduction.Length; ++i){
            deduction[i] = deduction[i] / sum;
        }
        return GetIndexOfMaxElem(deduction);
    }
    public int GetTileFromMapAndNeighbours(float[] mapping, Vector3Int location, Tilemap tilemap, string[] indexList){
        float[] deduction = new float[indexList.Length];
        int[] neighbours = CountNeighboursTestMap(location, tilemap, indexList);
        for(int i = 0; i < neighbours.Length; ++i){
            if(neighbours[i] > 0){
                float deltaDeduction = i * indexList.Length;
                for(int j = 0; j < indexList.Length; ++j){
                    deduction[j] += mapping[(int)deltaDeduction + j] * neighbours[i];
                }
            }
        }
        return GetIndexOfMaxElem(deduction);
    }
    public int GetTileFromMapAndNeighbours(float[] mapping, int location, int[] tilemap, int width, int height, string[] indexList){
        float[] deduction = new float[indexList.Length];
        int[] neighbours = CountNeighbours(tilemap, location, width, height);
        for(int i = 0; i < neighbours.Length; ++i){
            if(neighbours[i] > 0){
                float deltaDeduction = i * indexList.Length;
                for(int j = 0; j < indexList.Length; ++j){
                    deduction[j] += mapping[(int)deltaDeduction + j] * neighbours[i];
                }
            }
        }
        return GetIndexOfMaxElem(deduction);
    }
    public int GetTileFromMapAndNeighbours(Dictionary<string, float[]> mapping, int location, int[] tilemap, int width, int height, string[] indexList){
        float[] deduction = new float[indexList.Length];
        int[] neighbours = CountNeighbours(tilemap, location, width, height);
        for(int i = 0; i < neighbours.Length; ++i){
            if(neighbours[i] > 0){
                if(!mapping.ContainsKey(indexList[i])){
                    throw new Exception("AYO this aint right, where da fuck are you keeping that tile?!");
                }
                float[] deltaDeduction = mapping[indexList[i]];
                for(int j = 0; j < deltaDeduction.Length; ++j){
                    deduction[j] += deltaDeduction[j] * neighbours[i];
                }
            }
        }
        float sum = deduction.Sum();
        for(int i = 0; i < deduction.Length; ++i){
            deduction[i] = deduction[i] / sum;
        }
        return GetIndexOfMaxElem(deduction);
    }

    public void ResetTileMap(Tilemap tilemap){
        for(int x = tilemap.cellBounds.x; x < tilemap.cellBounds.xMax; x += 1){
            for(int y = tilemap.cellBounds.y; y < tilemap.cellBounds.yMax; y += 1){
                Vector3Int location = new Vector3Int(x, y);
                tilemap.SetTile(location, null);
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
        
    }
}
