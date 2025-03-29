using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.Tilemaps;

public class GACAMapGen : MonoBehaviour
{
    public GameObject testmap;
    public GenSettings settings;
    public InitializeTilemap initTilemap;
    private Tilemap testTilemap;
    public string[] indexList;
    public int genMax = 1000;
    private int genCounter = 0;
    public int celluralAutomataGen = 10;
    private List<float[]> population;
    private List<float> fitnessList;
    private float[] bestFoundMapping;
    public bool tryAgain = false;
    public bool stop = false;

    float[] GenerateRandomFloatArray(int length){
        float[] array = new float[length];
        for(int i = 0; i < length; ++i){
            array[i] = UnityEngine.Random.Range(-1f, 1f);
        }
        return array;
    }

    public float[] GenerateRandomMapping(){
        float[] mapping = new float[indexList.Length * indexList.Length];
        int j = 0;
        float[] elem = GenerateRandomFloatArray(indexList.Length);
        for(int i = 0; i < mapping.Length; ++i, ++j){
            if(j == indexList.Length){
                elem = GenerateRandomFloatArray(indexList.Length);
                j = 0;
            }
            mapping[i] = elem[j];
        }
        return mapping;
    }

    public float CalculateFitness(float[] mapping, Tilemap tilemap){
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
                        //initTilemap.GetTilemap().SetTile(location - new Vector3Int(testTilemap.cellBounds.x, testTilemap.cellBounds.y), initTilemap.tiles[indexList[index]]);
                        if(tilename == currentTile.name){
                            correct += 1;
                        }
                        total += 1;
                    }
                }
            }
        }
        //Debug.Log(" CORRECT : " + correct + " TOTAL : " + total);
        return correct;
    }
    public float CalculateFitnessDraw(float[] mapping, Tilemap tilemap){
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
                        initTilemap.GetTilemap().SetTile(location - new Vector3Int(tilemap.cellBounds.x, tilemap.cellBounds.y), initTilemap.tiles[indexList[index]]);
                        if(tilename == currentTile.name){
                            correct += 1;
                        }
                        total += 1;
                    }
                }
            }
        }
        //Debug.Log(" CORRECT : " + correct + " TOTAL : " + total);
        return correct;
    }

    float[] MappingCrossover(float[] parentA, float[] parentB){
        float[] child = new float[indexList.Length * indexList.Length];
        int onepointcrossover = UnityEngine.Random.Range(0, indexList.Length * indexList.Length);
        for(int i = 0; i < parentA.Length; ++i){
            if(i < onepointcrossover){
                child[i] = parentA[i];
            }else{
                child[i] = parentB[i];
            }
        }
        return child;
    }
    public float[] MappingMutation(float[] parentA){
        float[] child = new float[indexList.Length * indexList.Length];;
        for(int i = 0; i < parentA.Length; ++i){
            child[i] = parentA[i];
            float probability = UnityEngine.Random.Range(0, 1f);
            if(probability > 0.6){
                child[i] = child[i] + UnityEngine.Random.Range(-0.1f, 0.1f);
            }else if(probability < 0.05){
                child[i] = UnityEngine.Random.Range(-1f, 1f);
            }
        }
        return child;
    }
    List<float[]> NextGeneration(List<float[]> previousGeneration){
        List<float[]> generation = new List<float[]>();
        // apply elitism
        for(int i = 0; i < settings.populationSize*0.1; i++){
            generation.Add(previousGeneration[i]);
        }
        int rouletteWheel = 10000;
        int parentA = 0;
        int parentB = 0;
        float currentVal;
        // apply crossover
        for(int i = 0; i < settings.populationSize*0.9; i++){
            currentVal = 0;
            while(currentVal < rouletteWheel){
                parentA = UnityEngine.Random.Range(0, previousGeneration.Count);
                currentVal += fitnessList[parentA];
            }
            currentVal = 0;
            while(currentVal < rouletteWheel){
                parentB = UnityEngine.Random.Range(0, previousGeneration.Count);
                currentVal += fitnessList[parentB];
            }
            float probability = UnityEngine.Random.Range(0, 1f);
            if(parentA < parentB){
                generation.Add(MappingCrossover(previousGeneration[parentA], previousGeneration[parentB]));
            }else{
                generation.Add(MappingCrossover(previousGeneration[parentB], previousGeneration[parentA]));
            }
            if(probability < 0.05f){
                generation[^1] = MappingMutation(generation[^1]);
            }
        }
        //Debug.Log(fitnessList.Count + " " + generation.Count + " " + previousGeneration.Count);
        for(int i = 0; i < generation.Count; ++i){
            fitnessList[i] = CalculateFitness(generation[i], testTilemap);
        }
        //Debug.Log("LIST LENGTH: " + generation.Count);
        return generation;
    }

    public int[] GaCaAlg(float[] mapping, int generations){
        int width = Math.Abs(settings.bounds.xMax - settings.bounds.x);
        int height = Math.Abs(settings.bounds.yMax - settings.bounds.y);
        int[] heightMap = settings.GenerateRandomNoiseMap(indexList.Length);
        for(int generation = 0; generation < generations; ++generation){
            int[] tempMap = new int[width*height];
            for(int i = 0; i < heightMap.Length; ++i){
                tempMap[i] = heightMap[i];
            }
            for(int i = 0; i < heightMap.Length; ++i){
                heightMap[i] = settings.GetTileFromMapAndNeighbours(mapping, i, tempMap, width, height, indexList);
            }
        }
        for(int i = 0; i < heightMap.Length; ++i){
            if(0 <= heightMap[i] && heightMap[i] < indexList.Length){
                string name = indexList[heightMap[i]];
                int x = i % width + settings.bounds.x;
                int y = i / height + settings.bounds.y;
                initTilemap.GetTilemap().SetTile(new Vector3Int(x, y), initTilemap.tiles[name]);
            }
        }
        return heightMap;
    }
    public int[] GaCaAlgNonDraw(float[] mapping, int generations){
        int width = Math.Abs(settings.bounds.xMax - settings.bounds.x);
        int height = Math.Abs(settings.bounds.yMax - settings.bounds.y);
        int[] heightMap = settings.GenerateRandomNoiseMap(indexList.Length);
        for(int generation = 0; generation < generations; ++generation){
            int[] tempMap = new int[width*height];
            for(int i = 0; i < heightMap.Length; ++i){
                tempMap[i] = heightMap[i];
            }
            for(int i = 0; i < heightMap.Length; ++i){
                heightMap[i] = settings.GetTileFromMapAndNeighbours(mapping, i, tempMap, width, height, indexList);
            }
        }
        return heightMap;
    }
    public int[] GaCaAlg(float[] mapping, int[] heightMap, int generations){
        int width = Math.Abs(settings.bounds.xMax - settings.bounds.x);
        int height = Math.Abs(settings.bounds.yMax - settings.bounds.y);
        for(int generation = 0; generation < generations; ++generation){
            int[] tempMap = new int[width*height];
            for(int i = 0; i < heightMap.Length; ++i){
                tempMap[i] = heightMap[i];
            }
            for(int i = 0; i < heightMap.Length; ++i){
                heightMap[i] = settings.GetTileFromMapAndNeighbours(mapping, i, tempMap, width, height, indexList);
            }
        }
        for(int i = 0; i < heightMap.Length; ++i){
            if(0 <= heightMap[i] && heightMap[i] < indexList.Length){
                string name = indexList[heightMap[i]];
                int x = i % width + settings.bounds.x;
                int y = i / height + settings.bounds.y;
                initTilemap.GetTilemap().SetTile(new Vector3Int(x, y), initTilemap.tiles[name]);
            }
        }
        return heightMap;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        if(testTilemap != null) return;
        initTilemap.Start();
        testTilemap = testmap.GetComponentInChildren<Tilemap>();
        indexList = initTilemap.tiles.Keys.ToArray();
        fitnessList = new List<float>();
        population = new List<float[]>();
 
        for(int i = 0; i < settings.populationSize; ++i){
            population.Add(GenerateRandomMapping());
            fitnessList.Add(CalculateFitness(population[^1], testTilemap));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!stop){
            if(genCounter < genMax){
                population = NextGeneration(population);
                Debug.Log("GEN : " + genCounter + " TOP CANDIDATE: " + fitnessList.Max());
                genCounter++;
            }else if(tryAgain){
                if(bestFoundMapping == null){
                    float maxFit = 0;
                    int maxInd = -1;
                    for(int i = 0; i < fitnessList.Count; ++i){
                        if(maxFit < fitnessList[i]){
                            maxFit = fitnessList[i];
                            maxInd = i;
                        }
                    }
                    bestFoundMapping = population[maxInd];
                }
                GaCaAlg(bestFoundMapping, celluralAutomataGen);
                tryAgain = false;
            }
        }
    }
}
