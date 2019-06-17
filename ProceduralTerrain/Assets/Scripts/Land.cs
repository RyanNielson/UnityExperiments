using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class Land : MonoBehaviour
{
    public int width = 1;
    public int height = 1;

    public float noiseScale = 1;

    public float waterDepth = 0.25f;

    public GameObject landBlock;
    public GameObject waterBlock;

    public GameObject[,] blocks;
    public GameObject[,] spawnedTrees;

    public Gradient waterGradient;
    public Gradient grassGradient;

    public GameObject[] trees;

    [Range(0, 1f)]
    public float treeSpawnChance;

    void Start()
    {
        DOTween.SetTweensCapacity(20000, 100);
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
                GameObject tree = spawnedTrees[x, y];
                Destroy(tree);
            }
        }
    }
    public void GenerateMap()
    {
        blocks = new GameObject[width, height];
        spawnedTrees = new GameObject[width, height];
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

    // TODO: Maybe make it so that sand will only show up by water or close to other sand.
    // Otherwise we should just start with grass colours.
    private GameObject SpawnBlock(int x, int y, float noise)
    {
        if (noise < waterDepth)
        {
            GameObject water = Instantiate(waterBlock, new Vector3(x, -.25f, y), Quaternion.identity, transform);
            // water.transform.DOScaleY(0f, .5f).From().SetDelay(noise * 2);
            water.transform.DOScaleY(0f, .5f).From().SetEase(Ease.InOutElastic).SetDelay(noise * 2);
            // TODO: Since noise is rarely 0, maybe we want to find the min and use that as the baseline or add an offset for the deepest.
            Color color = waterGradient.Evaluate(noise / waterDepth);
            water.GetComponent<Renderer>().material.SetColor("_BaseColor", color);

            return water;
        }
        else
        {
            GameObject grass = Instantiate(landBlock, new Vector3(x, 0, y), Quaternion.identity, transform);
            Color color = grassGradient.Evaluate((noise - waterDepth) / (1f - waterDepth));
            grass.GetComponent<Renderer>().material.SetColor("_BaseColor", color);
            grass.transform.DOScaleY(0f, .5f).From().SetEase(Ease.InOutElastic).SetDelay(noise * 2);
            // grass.transform.DOShakeScale(0f, .5f).From().SetDelay(noise * 2);

            bool spawnTree = Random.Range(0f, 1f) <= treeSpawnChance;

            if (spawnTree)
            {
                GameObject tree = Instantiate(trees[Random.Range(0, trees.GetLength(0))], new Vector3(x, 0, y), Quaternion.identity, transform);
                spawnedTrees[x, y] = tree;
            }

            return grass;
        }
    }
}
