using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GenSettings : MonoBehaviour
{
    public BoundsInt bounds;
    public int mountain_iterations = 20;
    public int forest_iterations = 20;
    public int grass_first_layer_iterations = 20;
    public int grass_second_layer_iterations = 3;
    public float mountain_probability = 0.001f;
    public float forest_probability = 0.001f;
    public float grass_first_layer_probability= 0.001f;
    public float grass_second_layer_probability = 0.0001f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
