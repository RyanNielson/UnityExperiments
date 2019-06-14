using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Land : MonoBehaviour
{
    public int width = 1;
    public int height = 1;

    public float noiseScale = 1;

    public float waterDepth = 0.25f;

    public GameObject landBlock;
    public GameObject waterBlock;

    public GameObject[,] blocks;

    public Gradient waterGradient;
    public Gradient grassGradient;

    void Start()
    {
        GenerateMap();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.R))
        {
            ClearMap();
            GenerateMap();
        }
    }

    private void ClearMap()
    {
        for (int x = 0; x < blocks.GetLength(0); x++)
        {
            for (int y = 0; y < blocks.GetLength(1); y++)
            {
                GameObject block = blocks[x, y];
                Destroy(block);
            }
        }
    }
    public void GenerateMap()
    {
        blocks = new GameObject[width, height];
        float[,] noiseMap = Noise.Generate(width, height, noiseScale, Random.Range(0, 100000), Random.Range(0, 100000));

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject block = SpawnBlock(x, y, noiseMap[x, y]);
                blocks[x, y] = block;
            }
        }
    }

    private GameObject SpawnBlock(int x, int y, float noise)
    {
        if (noise < waterDepth)
        {
            // float steps = waterDepth / 3f;
            GameObject water = Instantiate(waterBlock, new Vector3(x, -.25f, y), Quaternion.identity, transform);

            // TODO: Since noise is rarely 0, maybe we want to find the min and use that as the baseline or add an offset for the deepest.
            Color color = waterGradient.Evaluate(noise / waterDepth);
            water.GetComponent<Renderer>().material.SetColor("_BaseColor", color);

            return water;
        }
        else
        {

            // Sand light rgb(248,241,107)
            // Sand med rgb(237,227,101)
            // Sand dark rgb(209,190,82)
            // Grass light rgb(142,164,42)
            // Grass med rgb(146,180,46)
            // Grass Dark rgb(128,170,48)
            // Instantiate(landBlock, new Vector3(x, .5f, y), Quaternion.identity);


            GameObject grass = Instantiate(landBlock, new Vector3(x, 0, y), Quaternion.identity, transform);
            Color color = grassGradient.Evaluate((noise - waterDepth) / (1f - waterDepth));
            grass.GetComponent<Renderer>().material.SetColor("_BaseColor", color);

            return grass;
        }
    }
}
