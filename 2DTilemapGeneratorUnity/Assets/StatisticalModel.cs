using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StatisticalModel : MonoBehaviour
{
    public GameObject testmap;
    public GenSettings settings;
    public InitializeTilemap initTilemap;
    private Tilemap testTilemap;
    private string[] indexList;
    public int generations = 10;
    public bool tryAgain = false;
    public int width = 100;
    public int height = 100;
    private Dictionary<string, float[]> model;
    public bool showApproximationOfTest = true;
    public bool start = false;
    /**
    <summary> Function, which calculates the tile frequencies in a specific tilemap </summary>
    <param name="tilemap"> tilemap to be used </param>
    <returns> Dictionary, where key is a tilename and float[] contains the neighbouring tile frequencies </returns>
    **/
    Dictionary<string, float[]> FrequencyAnalysis(Tilemap tilemap){
        Dictionary<string, float[]> mapping = new Dictionary<string, float[]>();
        for(int x = tilemap.cellBounds.x; x < tilemap.cellBounds.xMax; x += 1){
            for(int y = tilemap.cellBounds.y; y < tilemap.cellBounds.yMax; y += 1){
                Vector3Int location = new Vector3Int(x, y);
                TileBase currentTile = tilemap.GetTile(location);
                if(currentTile != null){
                    int[] neighbours = settings.CountNeighboursTestMap(location, tilemap, indexList);
                    if(!mapping.ContainsKey(currentTile.name)){
                        mapping.Add(currentTile.name, new float[11]);
                    }
                    for(int i = 0; i < neighbours.Length; ++i){
                        mapping[currentTile.name][i] += neighbours[i];
                    }
                }
            }
        }
        foreach(var pair in mapping){
            float sum = pair.Value.Sum();
            for(int i = 0; i < pair.Value.Length; ++i){
                pair.Value[i] = pair.Value[i]/sum;
            }
        }
        return mapping;
    }
    /**
    <summary> Function, which calculates the tile frequencies in a specific tilemap </summary>
    <param name="tilemap"> tilemap to be used </param>
    <param name="height"> tilemaps height </param>
    <param name="width"> tilemaps width </param>
    <returns> Dictionary, where key is a tilename and float[] contains the neighbouring tile frequencies in the same order as indexlist </returns>
    **/
    Dictionary<string, float[]> FrequencyAnalysis(int[] tilemap, int width, int height){
        Dictionary<string, float[]> mapping = new Dictionary<string, float[]>();
        for(int i = 0; i < tilemap.Length; ++i){
            int[] neighbours = settings.CountNeighbours(tilemap, i, width, height);
            string tile;
            if(tilemap[i] != -1){
                tile = indexList[tilemap[i]];
                for(int j = 0; j < neighbours.Length; ++j){
                    if(!mapping.ContainsKey(tile)){
                        mapping.Add(tile, new float[indexList.Length]);
                        mapping[tile][j] += neighbours[j];
                    }else{
                        mapping[tile][j] += neighbours[j];
                    }
                }
            }
        }
        foreach(var pair in mapping){
            float sum = pair.Value.Sum();
            if(sum == 0) continue;
            for(int i = 0; i < pair.Value.Length; ++i){
                pair.Value[i] = pair.Value[i]/sum;
            }
        }
        return mapping;
    }
    /**
    <summary> Function, which calculates the mean square error of two arrays of the same size  </summary>
    <param name="arrayA"> array to be used in calculations </param>
    <param name="arrayB"> array to be used in calculations </param>
    <returns> returns the MSE of two arrays </returns>
    **/
    float MeanSquareError(float[] arrayA, float[] arrayB){
        float result = 0;
        for(int i = 0; i < arrayA.Length; ++i){
            result += (arrayA[i] - arrayB[i]) * (arrayA[i] - arrayB[i]);
        }
        return result / arrayA.Length;
    }
    /**
    <summary> Function, which calculates the tile frequencies in a specific tilemap </summary>
    <param name="tilemap"> tilemap to be used </param>
    <param name="height"> tilemaps height </param>
    <param name="width"> tilemaps width </param>
    <returns> Dictionary, where key is a tilename and float[] contains the neighbouring tile frequencies in the same order as indexlist </returns>
    **/
    public float FrequencyDifference(int[] heightmapA, int[] heightmapB, int width, int height){
        float[] frequencyDifference = new float[initTilemap.tiles.Keys.Count];
        Dictionary<string, float[]> fqAnalysisA = FrequencyAnalysis(heightmapA, width, height);
        Dictionary<string, float[]> fqAnalysisB = FrequencyAnalysis(heightmapB, width, height);
        int i = 0;
        foreach(var key in fqAnalysisA.Keys){
            if(fqAnalysisB.ContainsKey(key)){
                frequencyDifference[i] = MeanSquareError(fqAnalysisA[key], fqAnalysisB[key]);
            }else{
                frequencyDifference[i] = MeanSquareError(fqAnalysisA[key], new float[initTilemap.tiles.Keys.Count]);
            }
        }
        float average = 0;
        foreach(float elem in frequencyDifference){
            average += elem;
        }
        return average / frequencyDifference.Length;
    }
    int GetTileFromMapAndNeighboursProbabilistic(Dictionary<string, float[]> mapping, Vector3Int location, Tilemap tilemap){
        float[] deduction = new float[11];
        int[] neighbours = settings.CountNeighboursTestMap(location, tilemap, indexList);
        for(int i = 0; i < neighbours.Length; ++i){
            if(neighbours[i] > 0){
                if(!mapping.ContainsKey(indexList[i])){
                    throw new System.Exception("AYO this aint right, where da fuck are you keeping that tile?!");
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
        };
        int choose = 0;
        float probability = UnityEngine.Random.Range(0, 1f);
        while(probability >= deduction[choose]){
            probability = UnityEngine.Random.Range(0, 1f);
            choose = (choose + 1) % deduction.Length;
        }
        return choose;
    }
    float CalculateFitness(Dictionary<string, float[]> mapping, Tilemap tilemap){
        int correct = 0;
        int total = 0;
        float currentxMax = tilemap.cellBounds.xMax;
        float currentyMax = tilemap.cellBounds.yMax;
        int currenty = tilemap.cellBounds.y;
        for(int x = tilemap.cellBounds.x; x < currentxMax; x += 1){
            for(int y = currenty; y < currentyMax; y += 1){
                Vector3Int location = new Vector3Int(x, y);
                TileBase currentTile = tilemap.GetTile(location);
                if(currentTile != null){
                    int index = settings.GetTileFromMapAndNeighbours(mapping, location, tilemap, indexList);
                    if(index != -1){
                        string tilename = indexList[index];
                        if(showApproximationOfTest){
                            initTilemap.GetTilemap().SetTile(location - new Vector3Int(tilemap.cellBounds.x, tilemap.cellBounds.y), initTilemap.tiles[indexList[index]]);
                        }
                        if(tilename == currentTile.name){
                            correct += 1;
                        }
                        total += 1;
                    }
                }
            }
        }
        Debug.Log(" CORRECT : " + correct + " TOTAL : " + total + " ratio: " + ((float)correct)/total);
        return correct;
    }
    void FrequencyAnalsysisBasedMapGen(Dictionary<string, float[]> fq, int width, int height){
        int x = settings.bounds.x;
        int y = settings.bounds.y;
        for(int i = 0; i < width*height; ++i){
            string name = indexList[GetTileFromMapAndNeighboursProbabilistic(fq, new Vector3Int(x, y), initTilemap.GetTilemap())];
            initTilemap.GetTilemap().SetTile(new Vector3Int(x, y), initTilemap.tiles[name]);
            if(x == width){
                x = settings.bounds.x;
                y++;
            }
            ++x;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        if(start){
            if(testTilemap != null) return;
            initTilemap.Start();
            testTilemap = testmap.GetComponentInChildren<Tilemap>();
            indexList = initTilemap.tiles.Keys.ToArray();
            model = FrequencyAnalysis(testTilemap);
            Debug.Log("Frequency analysis solution: " + CalculateFitness(FrequencyAnalysis(testTilemap), testTilemap));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(model == null && start){
            Start();
        }
        if(tryAgain && start){
            settings.ResetTileMap(initTilemap.GetTilemap());
            for(int i = 0; i < generations; ++i){
                FrequencyAnalsysisBasedMapGen(model, width, height);
            }
            tryAgain = false;
        }
    }
}
