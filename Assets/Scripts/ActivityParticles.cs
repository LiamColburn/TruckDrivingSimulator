using UnityEngine;

/// <summary>
/// Attach to the truck. Spawns brief world-space particle bursts for
/// trucker activities. All particles self-destroy after 2 seconds.
/// No prefab needed — systems are built fully in code.
/// </summary>
public class ActivityParticles : MonoBehaviour
{
    [Header("Burst Counts")]
    [SerializeField] private int eatCount  = 18;
    [SerializeField] private int drinkCount = 14;
    [SerializeField] private int honkCount  = 10;

    public void SpawnEatBurst()   => SpawnBurst(new Color(1f, 0.9f, 0.1f),  eatCount,   Vector3.up * 0.8f);
    public void SpawnDrinkBurst() => SpawnBurst(new Color(0.2f, 0.85f, 1f), drinkCount, Vector3.up * 0.8f);
    public void SpawnHonkPuff()   => SpawnBurst(new Color(0.9f, 0.9f, 0.9f), honkCount, Vector3.forward * 1.2f + Vector3.up * 0.4f);

    void SpawnBurst(Color color, int count, Vector3 localOffset)
    {
        GameObject go = new GameObject("ActivityBurst");
        go.transform.position = transform.position + transform.TransformDirection(localOffset);

        ParticleSystem ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startLifetime    = new ParticleSystem.MinMaxCurve(0.35f, 0.75f);
        main.startSpeed       = new ParticleSystem.MinMaxCurve(2f, 6f);
        main.startSize        = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
        main.startColor       = color;
        main.maxParticles     = count;
        main.playOnAwake      = false;
        main.stopAction       = ParticleSystemStopAction.Destroy;
        main.simulationSpace  = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius    = 0.4f;

        ps.Play();
    }
}
