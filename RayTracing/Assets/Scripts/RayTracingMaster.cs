using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct Sphere
{
    public Vector3 position;
    public float radius;
    public Vector3 albedo;
    public Vector3 specular;
    public float yOffset;
    public float movementSpeed;
}

// [ExecuteInEditMode]
public class RayTracingMaster : MonoBehaviour
{
    public ComputeShader RayTracingShader;

    public Texture skybox;

    public Light DirectionalLight;

    public int maxBounces = 8;

    public Vector3 SphereRadius = new Vector2(3.0f, 8.0f);
    public uint SpheresMax = 100;
    public float SpherePlacementRadius = 100f;
    public bool animate = false;
    private ComputeBuffer sphereBuffer;

    private RenderTexture target;

    private Camera camera;

    private uint currentSample = 0;

    private Material addMaterial;

    private void Awake()
    {
        camera = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        currentSample = 0;
        SetUpScene();
    }

    private void OnDisable()
    {
        if (sphereBuffer != null)
        {
            sphereBuffer.Release();
        }
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
        RayTracingShader.SetInt("_MaxBounces", maxBounces);

        // TODO: Maybe check for transform changes and pass this along only then.
        Vector3 l = DirectionalLight.transform.forward;
        RayTracingShader.SetVector("_DirectionalLight", new Vector4(l.x, l.y, l.z, DirectionalLight.intensity));

        // Set spheres.
        RayTracingShader.SetBuffer(0, "_Spheres", sphereBuffer);

        RayTracingShader.SetBool("_animate", animate);
    }

    private void SetUpScene()
    {
        List<Sphere> spheres = new List<Sphere>();

        for (int i = 0; i < SpheresMax; i++)
        {
            Sphere sphere = new Sphere();

            sphere.radius = SphereRadius.x + Random.value * (SphereRadius.y - SphereRadius.x);
            Vector2 randomPos = Random.insideUnitCircle * SpherePlacementRadius;
            sphere.position = new Vector3(randomPos.x, sphere.radius, randomPos.y);
            sphere.yOffset = Random.Range(0f, 1f);
            sphere.movementSpeed = Random.Range(-1.0f, 2.0f);

            // Reject spheres that are intersecting others
            foreach (Sphere other in spheres)
            {
                float minDist = sphere.radius + other.radius;
                if (Vector3.SqrMagnitude(sphere.position - other.position) < minDist * minDist)
                    goto SkipSphere;
            }

            // Set albedo and specular properties.
            Color color = Random.ColorHSV();
            bool metal = Random.value < 0.5f;
            sphere.albedo = metal ? Vector3.zero : new Vector3(color.r, color.g, color.b);
            sphere.specular = metal ? new Vector3(color.r, color.g, color.b) : Vector3.one * 0.04f;

            spheres.Add(sphere);

        SkipSphere:
            continue;
        }

        sphereBuffer = new ComputeBuffer(spheres.Count, 48);
        sphereBuffer.SetData(spheres);
    }
}
