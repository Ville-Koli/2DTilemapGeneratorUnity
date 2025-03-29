using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HillClimbingModel : MonoBehaviour
{
    public GameObject testmap;
    public GenSettings settings;
    public InitializeTilemap initTilemap;
    public GACAMapGen gaca;
    private Tilemap testTilemap;
    public int genMax = 1000;
    public string[] indexList;
    private int genCounter = 0;
    public int celluralAutomataGen = 10;
    private float[] bestFoundMapping;
    private float bestFitness = 0;
    public bool tryAgain = false;
    public bool retrain = false;
    public bool stop = false;
    public bool playAnimation = false;
    public int animationLength = 100;
    public float timeStep = 0.3f;
    private float timer = 0;
    private int animationCounter = 0;
    private int[] currentHeightmap;
    public Dictionary<string, List<int>> relData;
    public float[] wanted_ratios = new float[11]{1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000};
    Dictionary<string, List<int>> GetRelationalData(Tilemap tilemap, string[] indexList){
        Dictionary<string, List<int>> relData = new Dictionary<string, List<int>>();
        for(int x = tilemap.cellBounds.x; x < tilemap.cellBounds.xMax; x += 1){
            for(int y = tilemap.cellBounds.y; y < tilemap.cellBounds.yMax; y += 1){
                Vector3Int location = new Vector3Int(x, y);
                TileBase currentTile = tilemap.GetTile(location);
                if(currentTile != null){
                    string rel = settings.GetRelationalDataFromLocation(location, tilemap, indexList);
                    if(rel != ""){
                        int tileInd = Array.IndexOf(indexList, currentTile.name);
                        if(relData.ContainsKey(rel) && !relData[rel].Any(elem => {return elem == tileInd;})){
                            relData[rel].Add(tileInd);
                        }else{
                            relData[rel] = new List<int>(){tileInd};
                        }
                    }
                }
            }
        }
        return relData;
    }

    public float FitnessFromRelationalData(Dictionary<string, List<int>> relData, int[] map, int width, int height, string[] indexList, float[] wanted_ratios){
        float correct = 0;
        float[] ratios = new float[indexList.Length];
        for(int i = 0; i < map.Length; ++i){
            string neighbours = settings.GetRelationalDataFromLocation(map, i, width, height);
            if(relData.ContainsKey(neighbours)){
                correct += 1;
            }
            if(0 <= map[i] && map[i] < ratios.Length){
                ratios[map[i]] += 1;
            }
        }
        for(int i = 0; i < ratios.Length; ++i){
            if(math.abs((wanted_ratios[i] - ratios[i])/wanted_ratios[i]) > 0.01f){
                ratios[i] -= wanted_ratios[i] * 30;
            }else{
                ratios[i] -= wanted_ratios[i];
            }
        }
        return correct + ratios.Sum();
    }
    float[] HillClimbingAlgorithm(int generations){
        float[] randomMapping = gaca.GenerateRandomMapping();
        float fitness = gaca.CalculateFitness(randomMapping, testTilemap);
        for(int i = 0; i < generations; ++i){
            float[] newRandomMapping = gaca.MappingMutation(randomMapping);
            float newRandomMappingFitness = gaca.CalculateFitness(newRandomMapping, testTilemap);
            if(newRandomMappingFitness > fitness){
                fitness = newRandomMappingFitness;
                randomMapping = newRandomMapping;
            }
        }
        Debug.Log("hill climbing best fitness: " + fitness);
        return randomMapping;
    }
    void HillClimbingAlgorithm(){
        int width = Math.Abs(settings.bounds.xMax - settings.bounds.x);
        int height = Math.Abs(settings.bounds.yMax - settings.bounds.y);
        float[] randomMapping = gaca.MappingMutation(bestFoundMapping);
        float fitness = FitnessFromRelationalData(relData, gaca.GaCaAlgNonDraw(randomMapping, celluralAutomataGen), width, height, indexList, wanted_ratios);
        if(bestFitness < fitness){
            bestFitness = fitness;
            bestFoundMapping = randomMapping;
            settings.ResetTileMap(initTilemap.GetTilemap());
            gaca.CalculateFitnessDraw(bestFoundMapping, testTilemap);
            Debug.Log("gen: " + genCounter + " hill climbing best fitness: " + fitness);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gaca.Start();
        initTilemap.Start();
        int width = Math.Abs(settings.bounds.xMax - settings.bounds.x);
        int height = Math.Abs(settings.bounds.yMax - settings.bounds.y);
        indexList = initTilemap.tiles.Keys.ToArray();
        testTilemap = testmap.GetComponentInChildren<Tilemap>();
        bestFoundMapping = gaca.GenerateRandomMapping();
        relData = GetRelationalData(testTilemap, indexList);
        Debug.Log("COUNT: " + relData.Keys.Count);
        bestFitness = FitnessFromRelationalData(relData, gaca.GaCaAlgNonDraw(bestFoundMapping, celluralAutomataGen), width, height, indexList, wanted_ratios);
        Debug.Log("BEST FITNESS: " + bestFitness);
        //bestFoundMapping = HillClimbingAlgorithm(genMax);
    }

    // Update is called once per frame
    void Update()
    {
        if(retrain){
            if(genCounter < genMax){
                ++genCounter;
                HillClimbingAlgorithm();
            }else if(genCounter >= genMax){
                retrain = false;
                genCounter = 0;
                Debug.Log("TRAINING COMPLETE!");
            }
        }
        if(tryAgain){
            if(!playAnimation){
                currentHeightmap = gaca.GaCaAlg(bestFoundMapping, celluralAutomataGen);
                tryAgain = false;
            }else{
                if(currentHeightmap == null){
                    currentHeightmap = gaca.GaCaAlg(bestFoundMapping, celluralAutomataGen);
                }else{
                    timer += Time.deltaTime;
                    if(animationCounter < animationLength && timer > timeStep){
                        currentHeightmap = gaca.GaCaAlg(bestFoundMapping, currentHeightmap, 1);
                        ++animationCounter;
                        timer -= timeStep;
                    }else{
                        animationCounter = 0;
                    }
                }
            }
        }
    }
}
