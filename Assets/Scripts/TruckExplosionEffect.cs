using UnityEngine;

/// <summary>
/// Fully code-driven explosion effect — no prefab required.
/// TruckCollision spawns this by creating a bare GameObject and calling
/// AddComponent&lt;TruckExplosionEffect&gt;(). Start() builds three particle
/// layers entirely in code and schedules self-destruction after 3 s.
/// </summary>
public class TruckExplosionEffect : MonoBehaviour
{
    void Start()
    {
        Material mat = new Material(
            Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        mat.SetFloat("_Surface", 1);        // 1 = Transparent
        mat.SetFloat("_Blend", 0);          // 0 = Alpha blend
        mat.SetFloat("_SrcBlend", 5f);
        mat.SetFloat("_DstBlend", 10f);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = 3000;
        mat.mainTexture = CreateSoftCircle();

        BuildFire(mat);
        BuildSmoke(mat);
        BuildSparks(mat);

        Destroy(gameObject, 3f);
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    static void SetMaterial(ParticleSystem ps, Material mat)
    {
        ps.GetComponent<ParticleSystemRenderer>().material = mat;
    }

    static ParticleSystem MakeChildPS(Transform parent, string childName, Material mat)
    {
        var go = new GameObject(childName);
        go.transform.SetParent(parent, false);
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        SetMaterial(ps, mat);
        return ps;
    }

    // ── layers ────────────────────────────────────────────────────────────────

    // ── texture ───────────────────────────────────────────────────────────────

    private Texture2D CreateSoftCircle()
    {
        int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var center = new Vector2(size / 2f, size / 2f);
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist  = Vector2.Distance(new Vector2(x, y), center) / (size / 2f);
                float alpha = Mathf.Clamp01(1f - dist);
                alpha *= alpha;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();
        return tex;
    }

    void BuildFire(Material mat)
    {
        // Main burst — on the root GameObject itself
        var ps = gameObject.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        SetMaterial(ps, mat);

        var main = ps.main;
        main.loop            = false;
        main.duration        = 0.5f;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.8f, 1.5f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(8f, 15f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        main.startColor      = new ParticleSystem.MinMaxGradient(
                                   new Color(1f, 0.45f, 0f),
                                   new Color(1f, 0.15f, 0f));
        main.gravityModifier = new ParticleSystem.MinMaxCurve(-0.15f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles    = 80;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 60) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius    = 1.5f;

        ps.Play();
    }

    void BuildSmoke(Material mat)
    {
        var ps = MakeChildPS(transform, "Smoke", mat);

        var main = ps.main;
        main.loop            = false;
        main.duration        = 0.1f;
        main.startDelay      = new ParticleSystem.MinMaxCurve(0.1f);
        main.startLifetime   = new ParticleSystem.MinMaxCurve(2.5f, 4f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(1f, 3f);
        main.startSize       = new ParticleSystem.MinMaxCurve(1f, 2f);
        main.startColor      = new ParticleSystem.MinMaxGradient(
                                   new Color(0.2f, 0.2f, 0.2f, 0.85f),
                                   new Color(0.12f, 0.12f, 0.12f, 0.9f));
        main.gravityModifier = new ParticleSystem.MinMaxCurve(-0.3f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles    = 30;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 30) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius    = 1f;

        ps.Play();
    }

    void BuildSparks(Material mat)
    {
        var ps = MakeChildPS(transform, "Sparks", mat);

        var main = ps.main;
        main.loop            = false;
        main.duration        = 0.1f;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.5f, 0.8f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(15f, 25f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        main.startColor      = new ParticleSystem.MinMaxGradient(
                                   Color.white, new Color(1f, 1f, 0.35f));
        main.gravityModifier = new ParticleSystem.MinMaxCurve(1.2f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles    = 60;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 60) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius    = 0.5f;

        var collision = ps.collision;
        collision.enabled      = true;
        collision.type         = ParticleSystemCollisionType.World;
        collision.mode         = ParticleSystemCollisionMode.Collision3D;
        collision.bounce       = new ParticleSystem.MinMaxCurve(0.4f);
        collision.dampen       = new ParticleSystem.MinMaxCurve(0.4f);
        collision.lifetimeLoss = new ParticleSystem.MinMaxCurve(0.1f);

        ps.Play();
    }
}
