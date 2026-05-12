using UnityEngine;

/// <summary>
/// Attach to the TruckExplosion prefab.
/// Configures three pre-baked child particle systems (Fire, Smoke, Sparks)
/// and plays them on Awake. The whole hierarchy self-destructs after 5 s.
///
/// The child GameObjects must exist in the prefab hierarchy already (they are
/// baked into TruckExplosion.prefab). Each child's ParticleSystemRenderer
/// has Default-Particle assigned in YAML; this script also sets it via
/// Resources.GetBuiltinResource as a runtime safety-net.
/// </summary>
public class TruckExplosionEffect : MonoBehaviour
{
    void Awake()
    {
        ConfigureFire(transform.Find("Fire")?.GetComponent<ParticleSystem>());
        ConfigureSmoke(transform.Find("Smoke")?.GetComponent<ParticleSystem>());
        ConfigureSparks(transform.Find("Sparks")?.GetComponent<ParticleSystem>());
        Destroy(gameObject, 5f);
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Assign Default-Particle material to the renderer — guarantees the
    /// particles are never rendered as a purple error blob at runtime.
    /// </summary>
    static void EnsureMaterial(ParticleSystem ps)
    {
        if (ps == null) return;
        var psr = ps.GetComponent<ParticleSystemRenderer>();
        if (psr == null) return;
        var mat = Resources.GetBuiltinResource<Material>("Default-Particle.mat");
        if (mat != null) psr.material = mat;
    }

    // ── layer configurators ───────────────────────────────────────────────────

    void ConfigureFire(ParticleSystem ps)
    {
        if (ps == null) return;
        EnsureMaterial(ps);

        var main = ps.main;
        main.loop            = false;
        main.duration        = 0.1f;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.6f, 1.5f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(8f, 15f);
        main.startSize       = new ParticleSystem.MinMaxCurve(2f, 4f);
        main.startColor      = new ParticleSystem.MinMaxGradient(
                                   new Color(1f, 0.45f, 0f),
                                   new Color(1f, 0.15f, 0f));
        main.gravityModifier = new ParticleSystem.MinMaxCurve(-0.15f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles    = 80;
        main.stopAction      = ParticleSystemStopAction.Destroy;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 60) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius    = 1.5f;

        ps.Play();
    }

    void ConfigureSmoke(ParticleSystem ps)
    {
        if (ps == null) return;
        EnsureMaterial(ps);

        var main = ps.main;
        main.loop            = false;
        main.duration        = 0.1f;
        main.startDelay      = new ParticleSystem.MinMaxCurve(0.1f);
        main.startLifetime   = new ParticleSystem.MinMaxCurve(2.5f, 4f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(1f, 3f);
        main.startSize       = new ParticleSystem.MinMaxCurve(3f, 5f);
        main.startColor      = new ParticleSystem.MinMaxGradient(
                                   new Color(0.2f, 0.2f, 0.2f, 0.85f),
                                   new Color(0.12f, 0.12f, 0.12f, 0.9f));
        main.gravityModifier = new ParticleSystem.MinMaxCurve(-0.3f);   // rises
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles    = 30;
        main.stopAction      = ParticleSystemStopAction.Destroy;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 30) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius    = 1f;

        ps.Play();
    }

    void ConfigureSparks(ParticleSystem ps)
    {
        if (ps == null) return;
        EnsureMaterial(ps);

        var main = ps.main;
        main.loop            = false;
        main.duration        = 0.1f;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.5f, 0.8f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(15f, 25f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.1f, 0.2f);
        main.startColor      = new ParticleSystem.MinMaxGradient(
                                   Color.white, new Color(1f, 1f, 0.35f));
        main.gravityModifier = new ParticleSystem.MinMaxCurve(1.2f);    // falls
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles    = 60;
        main.stopAction      = ParticleSystemStopAction.Destroy;

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
