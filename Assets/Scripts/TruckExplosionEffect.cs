using UnityEngine;

/// <summary>
/// Attach to the TruckExplosion prefab.
/// Builds a 3-layer explosion in Awake — fire burst, rising smoke,
/// and sparks that bounce off the ground via the collision module.
/// The whole hierarchy self-destructs after 5 seconds.
/// </summary>
public class TruckExplosionEffect : MonoBehaviour
{
    void Awake()
    {
        BuildFire();
        BuildSmoke();
        BuildSparks();
        Destroy(gameObject, 5f);
    }

    // ── layer builders ────────────────────────────────────────────────────────

    void BuildFire()
    {
        ParticleSystem ps = MakeChild("Fire");

        var main = ps.main;
        main.duration        = 0.1f;
        main.loop            = false;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.6f, 1.5f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(5f, 14f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.6f, 2.2f);
        main.startColor      = new ParticleSystem.MinMaxGradient(
                                   new Color(1f, 0.45f, 0f),
                                   new Color(1f, 0.15f, 0f));
        main.gravityModifier = new ParticleSystem.MinMaxCurve(-0.15f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles    = 80;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 80) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius    = 1.5f;

        ps.Play();
    }

    void BuildSmoke()
    {
        ParticleSystem ps = MakeChild("Smoke");

        var main = ps.main;
        main.duration        = 0.1f;
        main.loop            = false;
        main.startDelay      = new ParticleSystem.MinMaxCurve(0.1f);
        main.startLifetime   = new ParticleSystem.MinMaxCurve(2.5f, 4f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(1f, 3f);
        main.startSize       = new ParticleSystem.MinMaxCurve(1.2f, 3f);
        main.startColor      = new ParticleSystem.MinMaxGradient(
                                   new Color(0.3f, 0.3f, 0.3f, 0.85f),
                                   new Color(0.12f, 0.12f, 0.12f, 0.9f));
        main.gravityModifier = new ParticleSystem.MinMaxCurve(-0.25f); // rises
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

    void BuildSparks()
    {
        ParticleSystem ps = MakeChild("Sparks");

        var main = ps.main;
        main.duration        = 0.1f;
        main.loop            = false;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.5f, 1.3f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(8f, 22f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.04f, 0.14f);
        main.startColor      = new ParticleSystem.MinMaxGradient(
                                   Color.white, new Color(1f, 1f, 0.35f));
        main.gravityModifier = new ParticleSystem.MinMaxCurve(1.2f); // falls
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles    = 60;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 60) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius    = 0.5f;

        // Bounce off ground
        var collision = ps.collision;
        collision.enabled      = true;
        collision.type         = ParticleSystemCollisionType.World;
        collision.mode         = ParticleSystemCollisionMode.Collision3D;
        collision.bounce       = new ParticleSystem.MinMaxCurve(0.4f);
        collision.dampen       = new ParticleSystem.MinMaxCurve(0.4f);
        collision.lifetimeLoss = new ParticleSystem.MinMaxCurve(0.1f);

        ps.Play();
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    ParticleSystem MakeChild(string childName)
    {
        GameObject go = new GameObject(childName);
        go.transform.SetParent(transform, false);
        return go.AddComponent<ParticleSystem>();
    }
}
