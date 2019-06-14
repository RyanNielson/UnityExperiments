using UnityEngine;

public class Noise
{
    public static float[,] Generate(int width, int height, float scale, float xOffset = 0, float yOffset = 0)
    {
        float[,] noise = new float[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                noise[x, y] = Mathf.PerlinNoise((x + xOffset) / (float)width * scale, (y + yOffset) / (float)height * scale);
            }
        }

        return noise;
    }
}

