using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [ExecuteInEditMode]
public class RayTracingMaster : MonoBehaviour
{
    public ComputeShader RayTracingShader;

    public Texture skybox;

    private RenderTexture target;

    private Camera camera;

    private uint currentSample = 0;

    private Material addMaterial;

    private void Awake()
    {
        camera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (transform.hasChanged)
        {
            currentSample = 0;
            transform.hasChanged = false;
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        SetShaderProperties();
        Render(destination);
    }

    private void Render(RenderTexture destination)
    {
        InitRenderTexture();

        // Set the target of the computer shader.
        RayTracingShader.SetTexture(0, "Result", target);

        // One thread group per 8 pixels.
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8f);

        // Execute the compute shader.
        RayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        // if (addMaterial == null)
        // {
        //     addMaterial = new Material(Shader.Find("Hidden/AddShader"));
        // }
        // addMaterial.SetFloat("_Sample", currentSample);

        // Draw the result to the screen.
        // Graphics.Blit(target, destination, addMaterial);
        Graphics.Blit(target, destination);
        currentSample++;
    }

    private void InitRenderTexture()
    {
        if (target == null || target.width != Screen.width || target.height != Screen.height)
        {
            currentSample = 0;
            if (target != null)
            {
                target.Release();
            }

            target = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            target.enableRandomWrite = true;
            target.Create();
        }
    }

    private void SetShaderProperties()
    {
        // Unity already has a functional camera system so we just need a few matrics for the shader.
        RayTracingShader.SetMatrix("_CameraToWorld", camera.cameraToWorldMatrix);
        RayTracingShader.SetMatrix("_CameraInverseProjection", camera.projectionMatrix.inverse);
        RayTracingShader.SetTexture(0, "_SkyboxTexture", skybox);
        // RayTracingShader.SetVector("_PixelOffset", new Vector2(Random.value, Random.value));
        RayTracingShader.SetFloat("_t", Time.realtimeSinceStartup);
    }
}
