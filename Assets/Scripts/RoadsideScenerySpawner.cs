using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns detailed farmland scenery on both sides of the road and scrolls it backward
/// at traffic speed. All geometry is built from Unity primitives — no prefabs required.
///
/// ROAD CLEARANCE: SidePos() positions every object by its INNER EDGE (not centre),
/// so even wide structures never clip the road (road edge ≈ ±7.5).
///
/// Setup: attach to any empty GameObject, assign the Player Truck transform.
/// </summary>
public class RoadsideScenerySpawner : MonoBehaviour
{
    [Header("Movement — match TrafficSpawner vehicleSpeed")]
    [SerializeField] private float scrollSpeed = 20f;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnDistance   = 140f;
    [SerializeField] private float despawnDistance = -60f;
    [SerializeField] private float minSpacingZ     =  28f;
    [SerializeField] private int   maxActiveItems  =  28;
    [SerializeField] private float spawnInterval   =   1.0f;

    [Header("Road Clearance")]
    [Tooltip("Inner edge of all scenery. Keep > 8 (road edge ≈ 7.5).")]
    [SerializeField] private float roadClearance   =  9.5f;
    [Tooltip("Extra random push outward past the inner edge.")]
    [SerializeField] private float maxOutwardSpread = 18f;

    [Header("References")]
    [SerializeField] private Transform playerTruck;

    private readonly List<GameObject> _active = new List<GameObject>();
    private float _timer;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Start()
    {
        if (playerTruck == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTruck = p.transform;
        }
        for (int i = 0; i < 10; i++)
            SpawnPair(playerTruck.position.z + 12f + i * minSpacingZ);
    }

    void Update()
    {
        if (playerTruck == null) return;
        ScrollAndCull();
        _timer += Time.deltaTime;
        if (_active.Count < maxActiveItems && _timer >= spawnInterval)
        {
            _timer = 0f;
            SpawnPair(playerTruck.position.z + spawnDistance);
        }
    }

    // ── Core ──────────────────────────────────────────────────────────────────

    void ScrollAndCull()
    {
        float cull = playerTruck.position.z + despawnDistance;
        for (int i = _active.Count - 1; i >= 0; i--)
        {
            var go = _active[i];
            if (go == null) { _active.RemoveAt(i); continue; }
            go.transform.Translate(Vector3.back * scrollSpeed * Time.deltaTime, Space.World);
            if (go.transform.position.z < cull) { Destroy(go); _active.RemoveAt(i); }
        }
    }

    void SpawnPair(float z)
    {
        SpawnSide(z,  1);
        SpawnSide(z + Random.Range(-10f, 10f), -1);
    }

    void SpawnSide(float z, int side)
    {
        foreach (var go in _active)
        {
            if (go == null) continue;
            if ((int)Mathf.Sign(go.transform.position.x) == side &&
                Mathf.Abs(go.transform.position.z - z) < minSpacingZ)
                return;
        }
        var item = BuildRandom(side, z);
        if (item != null) _active.Add(item);
    }

    // ── Dispatcher ────────────────────────────────────────────────────────────

    GameObject BuildRandom(int side, float z)
    {
        switch (Random.Range(0, 8))
        {
            case 0: return BuildBarn(SidePos(side, 6f, z));
            case 1: return BuildSiloCluster(SidePos(side, 6f, z));
            case 2: return BuildFarmhouse(SidePos(side, 6f, z));
            case 3: return BuildTreeCluster(SidePos(side, 6f, z));
            case 4: return BuildCropField(SidePos(side, 9f, z));
            case 5: return BuildFenceLine(SidePos(side, 0.5f, z));
            case 6: return BuildWindmill(SidePos(side, 4f, z));
            default: return BuildWaterTower(SidePos(side, 4f, z));
        }
    }

    // Inner edge of object = roadClearance. halfWidth is the object's X half-extent.
    Vector3 SidePos(int side, float halfWidth, float z)
    {
        float cx = side * (roadClearance + halfWidth + Random.Range(0f, maxOutwardSpread));
        return new Vector3(cx, 0f, z);
    }

    // ── BARN ──────────────────────────────────────────────────────────────────

    GameObject BuildBarn(Vector3 pos)
    {
        var root = Root("Barn", pos);
        var t = root.transform;
        Color red   = Clr(.60f, .09f, .06f);
        Color dark  = Clr(.20f, .18f, .16f);
        Color stone = Clr(.44f, .40f, .36f);
        Color wood  = Clr(.22f, .12f, .06f);
        Color glass = Clr(.52f, .66f, .76f);

        // Grass + dirt yard
        P(t, Cube, V(0, .05f, 0),    V(12f, .1f, 28f), Clr(.30f, .46f, .18f));
        P(t, Cube, V(-4f, .06f, 5f), V(4f, .08f, 8f),  Clr(.38f, .28f, .18f)); // dirt apron

        // Stone foundation
        P(t, Cube, V(0, .4f, 0), V(9.6f, .8f, 13.6f), stone);

        // Main barn body
        P(t, Cube, V(0, 4.2f, 0), V(9f, 7.5f, 13f), red);

        // Roof halves (angled)
        Rot(P(t, Cube, V(0, 9.0f, 0), V(10.6f, 3.3f, 14.2f), dark), 0, 0, 45);
        // Ridge cap
        P(t, Cube, V(0, 10.9f, 0), V(1.0f, .6f, 14.4f), Clr(.14f, .12f, .10f));
        // Roof overhang ends
        P(t, Cube, V(0, 7.8f, 7.2f),  V(9.4f, .3f, 1.5f), dark);
        P(t, Cube, V(0, 7.8f, -7.2f), V(9.4f, .3f, 1.5f), dark);

        // Cupola (roof vent tower)
        P(t, Cube, V(0, 11.4f, 0),  V(2.2f, 2.0f, 2.8f), dark);
        P(t, Cube, V(0, 11.5f, .6f), V(.5f, 1.5f, .12f), glass); // cupola window
        Rot(P(t, Cube, V(0, 12.7f, 0), V(2.8f, .8f, 3.4f), Clr(.12f, .10f, .09f)), 0, 0, 45);
        // Weathervane on cupola
        P(t, Cylinder, V(0, 14.0f, 0), V(.06f, .8f, .06f), Clr(.55f, .55f, .58f));
        P(t, Cube,     V(.4f, 14.7f, 0), V(.8f, .12f, .12f), Clr(.55f, .55f, .58f));

        // Lean-to extension (back of barn)
        P(t, Cube, V(0, 2.6f, -8.5f), V(7f, 4.2f, 3.8f), red);
        Rot(P(t, Cube, V(0, 4.9f, -8.5f), V(7.6f, 1.5f, 4.5f), dark), 10, 0, 0);

        // Sliding main door + frame
        P(t, Cube, V(-1.5f, 2.6f, 6.65f), V(4.2f, 4.8f, .14f), wood);  // frame
        P(t, Cube, V(-1.5f, 2.6f, 6.60f), V(3.8f, 4.4f, .18f), Clr(.48f, .06f, .04f)); // panel
        // Door cross planks
        P(t, Cube, V(-1.5f, 2.6f, 6.72f), V(3.6f, .12f, .1f), wood);
        P(t, Cube, V(-1.5f, 2.6f, 6.72f), V(.12f, 4.2f, .1f), wood);

        // Loft door + pulley hint
        P(t, Cube, V(0, 6.2f, 6.7f),   V(2.4f, 2.8f, .26f), wood);
        P(t, Sphere, V(0, 7.8f, 6.9f), V(.4f, .4f, .4f), Clr(.48f, .44f, .38f)); // pulley

        // Side windows (x2) with frames
        foreach (float wz in new[] { 2.5f, -2.5f })
        {
            P(t, Cube, V(4.65f, 5f, wz), V(.12f, 2.0f, 2.2f), wood);   // frame
            P(t, Cube, V(4.60f, 5f, wz), V(.16f, 1.6f, 1.8f), glass);  // glass
            P(t, Cube, V(4.68f, 5f, wz), V(.06f, .1f,  1.8f), wood);   // h-divider
            P(t, Cube, V(4.68f, 5f, wz), V(.06f, 1.6f, .1f),  wood);   // v-divider
        }
        // Ventilation strip
        P(t, Cube, V(4.62f, 7.5f, 0), V(.14f, .8f, 8f), wood);

        // Hay bales — stacked group
        HayBale(t, V(5.2f, .7f,  3.5f));
        HayBale(t, V(5.2f, .7f,  1.8f));
        HayBale(t, V(5.2f, 2.0f, 2.65f));
        HayBale(t, V(3.5f, .7f,  5.0f));
        HayBale(t, V(3.5f, .7f,  3.5f));

        // Corral (3 rail fence, barn is 4th wall)
        Color post = Clr(.38f, .26f, .14f);
        Color rail = Clr(.48f, .36f, .22f);
        // Corral: rectangle from (4.5, 0, -6) to (10, 0, 6)
        // Far side (x=10)
        for (int i = 0; i < 5; i++)
        {
            float fz = Mathf.Lerp(-6f, 6f, i / 4f);
            P(t, Cube, V(10f, 1.4f, fz), V(.28f, 2.8f, .28f), post);
        }
        // Near side rail along z=-6
        for (int i = 0; i < 3; i++)
            P(t, Cube, V(6.5f + i * 1.8f, 1.4f, -6f), V(.28f, 2.8f, .28f), post);
        // Near side rail along z=+6
        for (int i = 0; i < 3; i++)
            P(t, Cube, V(6.5f + i * 1.8f, 1.4f,  6f), V(.28f, 2.8f, .28f), post);
        // Rails (3 horizontal on each exposed side)
        foreach (float ry in new[] { .6f, 1.3f, 2.0f })
        {
            P(t, Cube, V(7.25f, ry, -6f), V(5.5f, .14f, .14f), rail);
            P(t, Cube, V(7.25f, ry,  6f), V(5.5f, .14f, .14f), rail);
            P(t, Cube, V(10f,   ry,  0f), V(.14f, .14f, 12f),  rail);
        }

        // Water trough in corral
        P(t, Cube, V(8f, .4f, 0),  V(3f, .6f, 1.2f), Clr(.38f, .32f, .28f)); // trough body
        P(t, Cube, V(8f, .55f, 0), V(2.6f, .3f, .8f), Clr(.30f, .48f, .60f)); // water surface

        // Dog house near barn door
        P(t, Cube, V(5f, .7f, -5f), V(1.8f, 1.4f, 2f), Clr(.55f, .36f, .18f));
        Rot(P(t, Cube, V(5f, 1.6f, -5f), V(2.2f, .9f, 2.4f), dark), 0, 0, 45);
        P(t, Cube, V(5f, .7f, -4f), V(.7f, 1.0f, .12f), wood); // dog door

        return root;
    }

    void HayBale(Transform parent, Vector3 lp)
    {
        Rot(P(parent, Cylinder, lp, V(1.2f, .75f, 1.2f), Clr(.74f, .63f, .22f)), 90, 0, 0);
        // Twine straps
        P(parent, Cylinder, lp + V(0, 0, 0), V(1.22f, .12f, 1.22f), Clr(.55f, .48f, .16f));
    }

    // ── SILO CLUSTER ─────────────────────────────────────────────────────────

    GameObject BuildSiloCluster(Vector3 pos)
    {
        var root = Root("SiloCluster", pos);
        var t = root.transform;
        Color concrete = Clr(.78f, .76f, .70f);
        Color dome     = Clr(.36f, .50f, .32f);
        Color metal    = Clr(.52f, .55f, .58f);
        Color band     = Clr(.42f, .40f, .36f);
        Color rust     = Clr(.52f, .28f, .12f);

        P(t, Cube, V(0, .05f, 0), V(12f, .1f, 18f), Clr(.30f, .46f, .18f)); // grass

        // ── Main tall silo
        P(t, Cylinder, V(0, 8.5f, 0),  V(3.2f, 8.5f, 3.2f), concrete);
        P(t, Sphere,   V(0, 17.5f, 0), V(3.5f, 2.2f, 3.5f), dome);
        P(t, Cylinder, V(0, 19.5f, 0), V(.8f, .5f, .8f), metal); // vent cap
        foreach (float y in new[] { 4f, 7f, 10f, 13f, 16f })
            P(t, Cylinder, V(0, y, 0), V(3.5f, .18f, 3.5f), band); // hoops
        // Ladder
        P(t, Cylinder, V(1.7f, 9f, 0),  V(.08f, 9f, .08f), metal);
        P(t, Cylinder, V(1.7f, 9f, .3f), V(.08f, 9f, .08f), metal);
        for (int r = 0; r < 10; r++)
            P(t, Cube, V(1.7f, r * 1.6f + 1f, .15f), V(.44f, .08f, .3f), metal); // rungs

        // ── Medium silo
        P(t, Cylinder, V(4.8f, 6f, 0),   V(2.4f, 6f, 2.4f), concrete);
        P(t, Sphere,   V(4.8f, 12.6f, 0), V(2.6f, 1.6f, 2.6f), dome);
        P(t, Cylinder, V(4.8f, 12.8f, 0), V(.7f, .4f, .7f), metal);
        foreach (float y in new[] { 3f, 6f, 9f, 11.5f })
            P(t, Cylinder, V(4.8f, y, 0), V(2.6f, .15f, 2.6f), band);

        // ── Small silo
        P(t, Cylinder, V(-4.2f, 4.5f, 0), V(1.8f, 4.5f, 1.8f), concrete);
        P(t, Sphere,   V(-4.2f, 9.4f, 0), V(2.0f, 1.2f, 2.0f), dome);
        foreach (float y in new[] { 2.5f, 5.5f, 8f })
            P(t, Cylinder, V(-4.2f, y, 0), V(2.0f, .14f, 2.0f), band);

        // ── Auger / conveyor pipe (angled from ground to top of main silo)
        var auger = P(t, Cylinder, V(2.5f, 7f, -2f), V(.6f, 8f, .6f), rust);
        auger.transform.localRotation = Quaternion.Euler(0, 0, 28f);
        P(t, Cube, V(2.5f, .5f, -2f), V(2f, .8f, 2f), Clr(.38f, .34f, .30f)); // auger base

        // ── Square grain bin (flat-roofed)
        P(t, Cube, V(-4.5f, 1.8f, -5f), V(4f, 3.5f, 5f), Clr(.68f, .62f, .50f));
        P(t, Cube, V(-4.5f, 3.7f, -5f), V(4.4f, .3f, 5.4f), metal); // bin roof
        P(t, Cube, V(-4.5f, 1.5f, -2.3f), V(1.2f, 2.5f, .2f), Clr(.28f, .22f, .16f)); // bin door

        // ── Control shack
        P(t, Cube, V(5.5f, 1.6f, -5f),  V(3.5f, 3.2f, 4f), Clr(.62f, .58f, .50f));
        P(t, Cube, V(5.5f, 3.3f, -5f),  V(4f, .5f, 4.5f), metal); // flat roof
        P(t, Cube, V(5.5f, 1.8f, -3.0f), V(1.0f, 2.0f, .2f), Clr(.28f, .22f, .14f)); // door
        P(t, Cube, V(4.2f, 2.2f, -3.0f), V(.8f, 1.0f, .15f), Clr(.52f, .66f, .74f)); // window

        // Connecting walkway pipe between tall and medium
        P(t, Cube,     V(2.4f, 12.5f, 0), V(2.6f, .3f, .8f), metal);
        P(t, Cylinder, V(2.4f, 7f, 0),    V(.18f, 7f, .18f), metal); // vertical pipe

        // Base concrete pads
        P(t, Cylinder, V(0,    .25f, 0), V(3.8f, .25f, 3.8f), Clr(.38f, .35f, .30f));
        P(t, Cylinder, V(4.8f, .25f, 0), V(2.8f, .25f, 2.8f), Clr(.38f, .35f, .30f));

        return root;
    }

    // ── FARMHOUSE ────────────────────────────────────────────────────────────

    GameObject BuildFarmhouse(Vector3 pos)
    {
        var root = Root("Farmhouse", pos);
        var t = root.transform;
        Color wall  = Clr(.93f, .90f, .82f);
        Color roof  = Clr(.45f, .15f, .11f);
        Color trim  = Clr(.82f, .80f, .72f);
        Color glass = Clr(.52f, .68f, .80f);
        Color brick = Clr(.42f, .30f, .26f);

        // Yard + driveway
        P(t, Cube, V(0, .05f, 0),    V(12f, .10f, 24f), Clr(.32f, .50f, .20f));
        P(t, Cube, V(0, .08f,  9f),  V(3.5f, .08f, 8f), Clr(.56f, .54f, .50f));  // gravel drive
        P(t, Cube, V(0, .08f, 13f),  V(2f, .06f, 4f),   Clr(.48f, .44f, .38f));  // path to door

        // Foundation
        P(t, Cube, V(0, .45f, 0), V(8.2f, .9f, 10.2f), Clr(.50f, .46f, .42f));
        // Main walls
        P(t, Cube, V(0, 4f, 0),    V(7.8f, 6.5f, 9.8f), wall);
        P(t, Cube, V(0, 8.5f, 0),  V(7.8f, 3.0f, .4f),  wall); // gable
        // Roof
        Rot(P(t, Cube, V(0, 8.2f, 0), V(9.2f, 3.2f, 11.2f), roof), 0, 0, 45);
        P(t, Cube, V(0, 10.4f, 0), V(.8f, .5f, 11.5f), Clr(.30f, .10f, .08f)); // ridge
        // Soffits / eaves
        P(t, Cube, V(0, 7.2f,  5.1f), V(8.5f, .25f, 1.0f), trim);
        P(t, Cube, V(0, 7.2f, -5.1f), V(8.5f, .25f, 1.0f), trim);

        // Chimney
        P(t, Cube, V(2.2f, 9.0f, 2f),  V(1.0f, 4.0f, 1.0f), brick);
        P(t, Cube, V(2.2f, 11.4f, 2f), V(1.4f, .35f, 1.4f), Clr(.28f, .20f, .16f));
        // Chimney cap
        P(t, Cylinder, V(2.2f, 11.7f, 2f), V(.5f, .25f, .5f), Clr(.22f, .18f, .14f));

        // Front porch
        P(t, Cube, V(0, .7f, 6.2f), V(6.5f, .35f, 2.8f), trim);
        Rot(P(t, Cube, V(0, 4.2f, 6.3f), V(7.0f, 1.4f, 3.2f), roof), 15, 0, 0);
        foreach (float cx in new[] { -2.2f, 2.2f })
        {
            P(t, Cylinder, V(cx, 2.4f, 7.2f), V(.22f, 2.4f, .22f), trim);
            // Column base/cap
            P(t, Cube, V(cx, .8f, 7.2f), V(.4f, .2f, .4f), trim);
            P(t, Cube, V(cx, 4.1f, 7.2f), V(.4f, .2f, .4f), trim);
        }
        // Porch railing
        P(t, Cube, V(0, 2.8f, 7.5f), V(5.0f, .12f, .12f), trim);
        for (int b = 0; b < 5; b++)
            P(t, Cube, V(-2f + b * 1f, 2f, 7.5f), V(.1f, 1.6f, .1f), trim);

        // Front door + screen
        P(t, Cube, V(0, 2.0f, 5.10f), V(1.3f, 3.0f, .22f), Clr(.28f, .16f, .08f));
        P(t, Cube, V(0, 2.0f, 5.25f), V(1.1f, 2.8f, .08f), Clr(.55f, .66f, .72f)); // screen
        P(t, Sphere, V(.5f, 2.0f, 5.20f), V(.18f, .18f, .18f), Clr(.70f, .60f, .20f)); // knob
        // Door frame
        P(t, Cube, V(0, 2.0f, 5.05f), V(1.6f, 3.4f, .12f), trim);

        // Front windows with shutters
        foreach (float wx in new[] { -2.6f, 2.6f })
        {
            P(t, Cube, V(wx, 4.2f, 5.05f), V(1.7f, 2.0f, .10f), trim);   // frame
            P(t, Cube, V(wx, 4.2f, 5.10f), V(1.5f, 1.8f, .12f), glass);  // glass
            P(t, Cube, V(wx, 4.2f, 5.00f), V(.08f, 1.8f, .08f), trim);   // v-divider
            // Shutters
            P(t, Cube, V(wx - 1.1f, 4.2f, 5.05f), V(.55f, 1.8f, .08f), Clr(.32f, .18f, .10f));
            P(t, Cube, V(wx + 1.1f, 4.2f, 5.05f), V(.55f, 1.8f, .08f), Clr(.32f, .18f, .10f));
        }
        // Side windows
        foreach (float wz in new[] { 2f, -2f })
        {
            P(t, Cube, V(4.02f, 4.2f, wz), V(.12f, 1.8f, 1.6f), trim);
            P(t, Cube, V(4.00f, 4.2f, wz), V(.14f, 1.6f, 1.4f), glass);
        }

        // Detached garage (further back in yard)
        Color gWall = Clr(.86f, .83f, .75f);
        P(t, Cube, V(-4.0f, .35f, -4.5f), V(5.5f, .7f, 6.5f), Clr(.46f, .42f, .38f)); // foundation
        P(t, Cube, V(-4.0f, 2.8f, -4.5f), V(5.2f, 4.8f, 6.2f), gWall);
        Rot(P(t, Cube, V(-4.0f, 5.4f, -4.5f), V(6f, 1.8f, 7f), roof), 0, 0, 45);
        // Garage door
        P(t, Cube, V(-4.0f, 2.2f, -1.5f), V(4.0f, 4.0f, .18f), Clr(.28f, .22f, .16f));
        // Garage door panels
        foreach (float gy in new[] { 1.0f, 2.2f, 3.2f })
            P(t, Cube, V(-4.0f, gy, -1.42f), V(3.6f, .12f, .1f), Clr(.34f, .28f, .20f));
        // Side window
        P(t, Cube, V(-6.7f, 3.0f, -4f), V(.14f, 1.2f, 1.6f), glass);

        // Flower beds along front of house
        Color[] flowers = { Clr(.90f, .20f, .20f), Clr(.95f, .70f, .10f), Clr(.85f, .15f, .70f) };
        for (int f = 0; f < 6; f++)
        {
            float fz = -3.5f + f * 1.2f;
            P(t, Cube, V(4.2f, .3f, fz), V(1.0f, .4f, .9f), Clr(.30f, .22f, .12f)); // soil
            P(t, Sphere, V(4.2f, .8f, fz), V(.5f, .5f, .5f), flowers[f % 3]); // bloom
        }

        // Mailbox on post at end of driveway
        P(t, Cylinder, V(0, .8f, 13.5f), V(.1f, .8f, .1f), Clr(.30f, .28f, .26f)); // post
        P(t, Cube, V(0, 1.8f, 13.5f), V(.5f, .45f, .8f), Clr(.22f, .30f, .55f)); // box
        P(t, Sphere, V(0, 1.8f, 13.9f), V(.45f, .4f, .1f), Clr(.22f, .30f, .55f)); // door end

        // Clothesline
        P(t, Cylinder, V(-3f, 2.4f, -9f), V(.08f, 2.4f, .08f), Clr(.38f, .32f, .28f));
        P(t, Cylinder, V( 3f, 2.4f, -9f), V(.08f, 2.4f, .08f), Clr(.38f, .32f, .28f));
        P(t, Cube,     V( 0f, 4.8f, -9f), V(6.2f, .04f, .04f), Clr(.70f, .68f, .65f)); // line
        // Clothes on line
        foreach (float cx in new[] { -2f, -.5f, 1f, 2.5f })
        {
            Color cc = new Color(Random.value * .6f + .3f, Random.value * .4f + .3f, Random.value * .4f + .3f);
            P(t, Cube, V(cx, 4.2f, -9f), V(.5f, .8f, .08f), cc);
        }

        // Yard tree (back corner)
        AddTree(t, V(-4f, 0, -8f), 1.2f);

        // Front picket fence
        PicketFence(t, V(0, 0, 9f), 14f, false);

        return root;
    }

    void PicketFence(Transform parent, Vector3 lp, float length, bool acrossX)
    {
        Color c = Clr(.90f, .88f, .84f);
        int n = Mathf.RoundToInt(length / 0.9f);
        for (int i = 0; i < n; i++)
        {
            float off = (i - n * .5f) * .9f;
            Vector3 pp = lp + (acrossX ? V(off, 0, 0) : V(0, 0, off));
            P(parent, Cube, pp + V(0, 1.1f, 0), V(.2f, 1.6f, .2f), c);
            Rot(P(parent, Cube, pp + V(0, 2.1f, 0), V(.16f, .35f, .16f), c), 0, 45, 0);
        }
        var railSize = acrossX ? V(length, .12f, .12f) : V(.12f, .12f, length);
        P(parent, Cube, lp + V(0, 1.4f, 0), railSize, c);
        P(parent, Cube, lp + V(0, .5f,  0), railSize, c);
    }

    // ── TREE CLUSTER ─────────────────────────────────────────────────────────

    GameObject BuildTreeCluster(Vector3 pos)
    {
        var root = Root("TreeCluster", pos);
        var t = root.transform;

        P(t, Cube, V(0, .05f, 0), V(12f, .10f, 20f), Clr(.26f, .40f, .16f)); // ground

        // Trees (3–5)
        int count = Random.Range(3, 6);
        for (int i = 0; i < count; i++)
            AddTree(t, V(Random.Range(-5f, 5f), 0, Random.Range(-7f, 7f)), Random.Range(.8f, 1.5f));

        // Undergrowth bushes
        for (int b = 0; b < 6; b++)
        {
            float s = Random.Range(.4f, .9f);
            Color bush = Color.HSVToRGB(Random.Range(.25f, .36f), .55f, .35f);
            P(t, Sphere, V(Random.Range(-5f, 5f), s * .5f, Random.Range(-7f, 7f)), V(s * 1.8f, s, s * 1.8f), bush);
        }

        // Rocks
        for (int r = 0; r < 4; r++)
        {
            float rs = Random.Range(.2f, .6f);
            Color rock = Clr(Random.Range(.38f, .52f), Random.Range(.36f, .50f), Random.Range(.34f, .46f));
            var rockGo = P(t, Sphere, V(Random.Range(-5f, 5f), rs * .35f, Random.Range(-7f, 7f)), V(rs * 1.4f, rs, rs * 1.2f), rock);
            rockGo.transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        }

        // Fallen log
        var log = P(t, Cylinder, V(Random.Range(-3f, 3f), .3f, Random.Range(-5f, 5f)),
            V(.45f, 3.5f, .45f), Clr(.38f, .28f, .16f));
        log.transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 90f);
        // Moss on log
        P(log.transform, Sphere, V(0, .5f, 0), V(.55f, .22f, 1.0f), Clr(.22f, .38f, .18f));

        // Mushrooms at base
        for (int m = 0; m < 3; m++)
        {
            Vector3 mp = V(Random.Range(-4f, 4f), 0, Random.Range(-5f, 5f));
            P(t, Cylinder, mp + V(0, .15f, 0), V(.12f, .15f, .12f), Clr(.55f, .42f, .30f)); // stem
            P(t, Sphere,   mp + V(0, .38f, 0), V(.35f, .2f, .35f), Clr(.65f, .14f, .10f));  // cap
        }

        return root;
    }

    void AddTree(Transform parent, Vector3 lp, float s)
    {
        Color bark = Clr(.32f, .22f, .12f);
        bool pine  = Random.value > .45f;

        if (pine)
        {
            P(parent, Cylinder, lp + V(0, 1.8f * s, 0), V(.42f * s, 1.8f * s, .42f * s), bark);
            Color leaf = Clr(.16f, .36f, .14f);
            float[] radii   = { 3.2f, 2.3f, 1.6f, 0.9f };
            float[] heights = { 4.2f, 5.6f, 6.8f, 7.8f };
            for (int i = 0; i < 4; i++)
            {
                float r = radii[i] * s;
                P(parent, Sphere, lp + V(0, heights[i] * s, 0), V(r, r * .88f, r), leaf);
            }
        }
        else
        {
            P(parent, Cylinder, lp + V(0, 2.2f * s, 0), V(.52f * s, 2.2f * s, .52f * s), bark);
            // Secondary trunk fork
            P(parent, Cylinder, lp + V(.3f * s, 3.5f * s, 0), V(.28f * s, 1.2f * s, .28f * s), bark);
            Color leaf = Color.HSVToRGB(Random.Range(.26f, .38f), .60f, .40f);
            float r = 3.8f * s;
            P(parent, Sphere, lp + V(0,    6.0f * s, 0), V(r, r * .85f, r), leaf);
            P(parent, Sphere, lp + V(.6f * s, 7.6f * s, .3f * s), V(r * .65f, r * .65f, r * .65f), leaf);
            P(parent, Sphere, lp + V(-.5f * s, 7.0f * s, 0), V(r * .55f, r * .55f, r * .55f), leaf);
        }
    }

    // ── CROP FIELD ───────────────────────────────────────────────────────────

    GameObject BuildCropField(Vector3 pos)
    {
        var root = Root("CropField", pos);
        var t = root.transform;

        int kind = Random.Range(0, 3);
        Color soil     = Clr(.34f, .24f, .14f);
        Color cropCol  = kind == 0 ? Clr(.52f, .68f, .18f)   // corn
                       : kind == 1 ? Clr(.70f, .62f, .18f)   // wheat
                       :             Clr(.20f, .50f, .24f);   // soy
        float cropH = kind == 0 ? 1.6f : kind == 1 ? 0.85f : 0.55f;

        // Soil base
        P(t, Cube, V(0, .10f, 0), V(18f, .20f, 34f), soil);
        // Furrow strips
        for (int i = 0; i < 6; i++)
            P(t, Cube, V((i - 2.5f) * 2.8f, .22f, 0), V(.55f, .14f, 32f), Clr(.22f, .14f, .08f));
        // Crop rows
        for (int i = 0; i < 5; i++)
            P(t, Cube, V((i - 2f) * 3f, .3f + cropH * .5f, 0), V(1.1f, cropH, 32f), cropCol);

        // Scarecrow
        if (kind < 2) AddScarecrow(t, V(0, 0, Random.Range(-8f, 8f)));

        // Parked tractor at headland
        AddTractor(t, V(-6f, 0, 14f));

        // Irrigation wheel-line (long pipe on wheeled frames)
        P(t, Cylinder, V(0, 1.8f, -10f), V(.14f, .14f, 16f), Clr(.50f, .52f, .54f)); // main pipe
        for (int w = 0; w < 4; w++)
        {
            float wz = -14f + w * 3.5f;
            P(t, Cylinder, V(0, .9f, wz), V(.1f, .9f, .1f), Clr(.44f, .44f, .46f)); // strut
            Rot(P(t, Cylinder, V(0, .35f, wz), V(.55f, .08f, .55f), Clr(.28f, .28f, .30f)), 90, 0, 0); // wheel
        }

        // Field border fence (one side)
        for (int fp = 0; fp < 5; fp++)
        {
            float fz = -14f + fp * 7f;
            P(t, Cube, V(-9.5f, 1.0f, fz), V(.25f, 2.0f, .25f), Clr(.44f, .32f, .18f));
        }
        P(t, Cube, V(-9.5f, 1.2f, 0), V(.16f, .16f, 28f), Clr(.52f, .40f, .24f)); // top rail

        return root;
    }

    void AddScarecrow(Transform parent, Vector3 lp)
    {
        Color post  = Clr(.40f, .28f, .16f);
        Color shirt = Clr(.32f, .20f, .52f);
        Color pants = Clr(.24f, .28f, .50f);
        Color hat   = Clr(.16f, .12f, .08f);
        Color straw = Clr(.70f, .60f, .20f);

        P(parent, Cylinder, lp + V(0, 1.5f, 0), V(.14f, 1.5f, .14f), post);
        Rot(P(parent, Cylinder, lp + V(0, 2.6f, 0), V(.10f, 1.4f, .10f), post), 0, 0, 90); // crossbar
        P(parent, Cube,   lp + V(0, 2.4f, 0), V(.85f, 1.3f, .4f), shirt);
        P(parent, Cube,   lp + V(-.25f, 1.2f, 0), V(.35f, .8f, .38f), pants); // left leg
        P(parent, Cube,   lp + V( .25f, 1.2f, 0), V(.35f, .8f, .38f), pants); // right leg
        P(parent, Sphere, lp + V(0, 3.5f, 0), V(.5f, .5f, .5f), Clr(.72f, .58f, .42f)); // head
        // Hat
        P(parent, Cylinder, lp + V(0, 3.80f, 0), V(.65f, .12f, .65f), hat); // brim
        P(parent, Cylinder, lp + V(0, 4.18f, 0), V(.40f, .42f, .40f), hat); // crown
        // Straw hands
        P(parent, Sphere, lp + V(-1.0f, 2.6f, 0), V(.3f, .3f, .3f), straw);
        P(parent, Sphere, lp + V( 1.0f, 2.6f, 0), V(.3f, .3f, .3f), straw);
        // Button eyes
        P(parent, Sphere, lp + V(-.12f, 3.56f, .26f), V(.08f, .08f, .06f), Clr(.1f, .1f, .1f));
        P(parent, Sphere, lp + V( .12f, 3.56f, .26f), V(.08f, .08f, .06f), Clr(.1f, .1f, .1f));
    }

    void AddTractor(Transform parent, Vector3 lp)
    {
        Color green = Clr(.14f, .38f, .10f);
        Color dark  = Clr(.10f, .28f, .08f);
        Color black = Clr(.12f, .12f, .12f);
        Color metal = Clr(.60f, .60f, .62f);
        Color glass = Clr(.55f, .70f, .82f);
        Color yellow = Clr(.80f, .72f, .10f);

        // Main body
        P(parent, Cube, lp + V(0, 1.2f, 0), V(3.8f, 1.6f, 5.8f), green);
        // Engine hood (front)
        P(parent, Cube, lp + V(0, 2.0f, 2.4f), V(3.0f, 1.5f, 2.8f), dark);
        P(parent, Cube, lp + V(0, 2.8f, 2.4f), V(3.2f, .3f, 3.0f), metal); // hood top
        // Radiator grille
        P(parent, Cube, lp + V(0, 1.8f, 3.9f), V(2.6f, 1.8f, .2f), black);
        for (int g = 0; g < 4; g++)
            P(parent, Cube, lp + V(-1.0f + g * .7f, 1.8f, 4.0f), V(.1f, 1.6f, .15f), metal);
        // Cab
        P(parent, Cube, lp + V(0, 3.5f, -1.2f), V(2.8f, 2.2f, 3.4f), dark);
        P(parent, Cube, lp + V(0, 4.7f, -1.2f), V(3.0f, .25f, 3.6f), metal); // roof
        // Cab glass
        P(parent, Cube, lp + V(0, 3.5f, .2f),   V(2.6f, 1.8f, .1f), glass); // front
        P(parent, Cube, lp + V(0, 3.5f, -2.7f), V(2.6f, 1.6f, .1f), glass); // rear
        P(parent, Cube, lp + V(1.5f, 3.5f, -1.2f), V(.1f, 1.6f, 3.0f), glass); // side
        // Exhaust stack
        P(parent, Cylinder, lp + V(1.2f, 4.2f, 2.0f), V(.18f, 1.4f, .18f), Clr(.28f, .28f, .30f));
        P(parent, Cylinder, lp + V(1.2f, 5.6f, 2.0f), V(.28f, .2f, .28f), Clr(.22f, .22f, .24f)); // stack cap
        // Headlights
        P(parent, Sphere, lp + V( .9f, 2.2f, 4.0f), V(.3f, .3f, .2f), yellow);
        P(parent, Sphere, lp + V(-.9f, 2.2f, 4.0f), V(.3f, .3f, .2f), yellow);
        // Fender arches over rear wheels
        P(parent, Cube, lp + V( 2.2f, 2.2f, -1.5f), V(.8f, .5f, 2.2f), green);
        P(parent, Cube, lp + V(-2.2f, 2.2f, -1.5f), V(.8f, .5f, 2.2f), green);
        // Front wheels (small)
        Rot(P(parent, Cylinder, lp + V( 1.9f, .75f, 2.5f), V(.75f, .35f, .75f), black), 0, 0, 90);
        Rot(P(parent, Cylinder, lp + V(-1.9f, .75f, 2.5f), V(.75f, .35f, .75f), black), 0, 0, 90);
        // Front hubcaps
        Rot(P(parent, Cylinder, lp + V( 2.28f, .75f, 2.5f), V(.5f, .08f, .5f), metal), 0, 0, 90);
        Rot(P(parent, Cylinder, lp + V(-2.28f, .75f, 2.5f), V(.5f, .08f, .5f), metal), 0, 0, 90);
        // Rear wheels (large)
        Rot(P(parent, Cylinder, lp + V( 2.2f, 1.2f, -1.5f), V(1.5f, .45f, 1.5f), black), 0, 0, 90);
        Rot(P(parent, Cylinder, lp + V(-2.2f, 1.2f, -1.5f), V(1.5f, .45f, 1.5f), black), 0, 0, 90);
        // Rear hubcaps
        Rot(P(parent, Cylinder, lp + V( 2.68f, 1.2f, -1.5f), V(1.0f, .1f, 1.0f), metal), 0, 0, 90);
        Rot(P(parent, Cylinder, lp + V(-2.68f, 1.2f, -1.5f), V(1.0f, .1f, 1.0f), metal), 0, 0, 90);
        // Rear hitch
        P(parent, Cube, lp + V(0, .9f, -3.2f), V(2.2f, .4f, .4f), metal);
        P(parent, Cube, lp + V(0, .7f, -3.4f), V(.35f, .7f, .6f), metal);
        // Steps up to cab
        for (int s = 0; s < 3; s++)
            P(parent, Cube, lp + V(1.95f, .55f + s * .6f, .5f - s * .2f), V(.4f, .12f, .5f), metal);
    }

    // ── FENCE LINE ───────────────────────────────────────────────────────────

    GameObject BuildFenceLine(Vector3 pos)
    {
        var root = Root("FenceLine", pos);
        var t = root.transform;
        Color post = Clr(.56f, .46f, .32f);
        Color rail = Clr(.64f, .55f, .42f);
        Color wire = Clr(.55f, .55f, .52f);

        const int n = 12;
        const float sp = 3.5f;
        float half = (n - 1) * sp * .5f;

        for (int i = 0; i < n; i++)
        {
            float z = i * sp - half;
            // Post body
            P(t, Cube, V(0, 1.3f, z), V(.32f, 2.6f, .32f), post);
            // Pointed cap
            Rot(P(t, Cube, V(0, 2.78f, z), V(.24f, .42f, .24f), post), 0, 45, 0);
            // Staple hint
            P(t, Cube, V(.18f, 1.2f, z), V(.12f, .12f, .36f), wire);
        }

        // Gate section in the middle (gap between posts 5 and 6)
        float gateZ = 1.75f; // midpoint
        // Gate rails (slightly open, rotated)
        var gateA = P(t, Cube, V(1.5f, 1.5f, gateZ - 2.5f), V(.2f, .2f, 5f), rail);
        gateA.transform.localRotation = Quaternion.Euler(0, 15f, 0);
        var gateB = P(t, Cube, V(1.5f, .7f, gateZ - 2.5f), V(.2f, .2f, 5f), rail);
        gateB.transform.localRotation = Quaternion.Euler(0, 15f, 0);
        // Gate hinge post (thicker)
        P(t, Cube, V(0, 1.5f, gateZ - 4.5f), V(.45f, 3.0f, .45f), post);

        float len = (n - 1) * sp + sp;
        P(t, Cube, V(0, 1.9f, 0), V(.14f, .2f, len), rail);
        P(t, Cube, V(0, .75f, 0), V(.14f, .2f, len), rail);

        // Barbed wire strands (3 lines along top)
        for (int w = 0; w < 3; w++)
            P(t, Cube, V(.12f, 2.6f + w * .18f, 0), V(.04f, .04f, len), wire);

        // Dirt track through gate
        P(t, Cube, V(1.5f, .02f, gateZ), V(2.5f, .04f, 5f), Clr(.40f, .30f, .18f));

        return root;
    }

    // ── WINDMILL ─────────────────────────────────────────────────────────────

    GameObject BuildWindmill(Vector3 pos)
    {
        var root = Root("Windmill", pos);
        var t = root.transform;
        Color stone = Clr(.68f, .64f, .56f);
        Color wood  = Clr(.40f, .30f, .20f);
        Color sail  = Clr(.86f, .83f, .74f);
        Color cap   = Clr(.36f, .28f, .20f);
        Color metal = Clr(.50f, .52f, .54f);

        P(t, Cube, V(0, .05f, 0), V(8f, .1f, 16f), Clr(.28f, .44f, .18f)); // grass

        // Tower — two cylinders (tapered)
        P(t, Cylinder, V(0, 4.5f, 0), V(3.8f, 4.5f, 3.8f), stone);
        P(t, Cylinder, V(0, 10f, 0),  V(2.8f, 3.0f, 2.8f), stone);
        // Stone bands
        foreach (float y in new[] { 2f, 5f, 8f, 11f })
            P(t, Cylinder, V(0, y, 0), V(3.9f, .12f, 3.9f), Clr(.55f, .52f, .46f));
        // Door
        P(t, Cube, V(0, 1.4f, 2.1f), V(1.1f, 2.4f, .25f), Clr(.28f, .18f, .10f));
        P(t, Cube, V(0, 1.4f, 2.22f), V(1.15f, 2.5f, .1f), Clr(.22f, .14f, .08f)); // frame
        // Window
        P(t, Cube, V(0, 7.0f, 2.0f), V(.7f, .9f, .2f), Clr(.50f, .64f, .74f));

        // Cap / roof
        P(t, Sphere, V(0, 14.2f, 0), V(3.2f, 2.8f, 3.2f), cap);
        P(t, Cylinder, V(0, 12.8f, 0), V(3.4f, .25f, 3.4f), Clr(.28f, .22f, .16f)); // collar

        // Tail vane (orients windmill to wind — decorative)
        P(t, Cube, V(0, 13.5f, -2.5f), V(.12f, 2.5f, 3.5f), wood);

        // Blade group with WindmillRotator
        var hub = new GameObject("BladeGroup");
        hub.transform.SetParent(root.transform, false);
        hub.transform.localPosition = new Vector3(0f, 13f, 2.5f);
        hub.AddComponent<WindmillRotator>();

        // Hub axle
        Prim(hub.transform, Sphere, Vector3.zero, V(.7f, .7f, .7f), wood);
        Prim(hub.transform, Cylinder, V(0, 0, .4f), V(.4f, .4f, .4f), metal); // axle stub

        // 4 blades
        for (int b = 0; b < 4; b++)
        {
            var blade = new GameObject($"Blade{b}");
            blade.transform.SetParent(hub.transform, false);
            blade.transform.localRotation = Quaternion.Euler(0, 0, b * 90f);

            Prim(blade.transform, Cube, V(0, 2.8f, 0), V(.28f, 5.0f, .22f), wood);   // arm
            Prim(blade.transform, Cube, V(0, 3.8f, .05f), V(1.55f, 3.6f, .1f), sail); // canvas
            Prim(blade.transform, Cube, V(0, 3.8f, .12f), V(1.6f, .12f, .1f), wood);  // h-spar
            Prim(blade.transform, Cube, V(0, 3.8f, .12f), V(.12f, 3.6f, .1f), wood);  // v-spar
        }

        // Guy wires (thin cubes angled from ~8m height to ground at 4 directions)
        foreach (Vector3 dir in new[] { V(1,0,0), V(-1,0,0), V(0,0,1), V(0,0,-1) })
        {
            var wire = P(t, Cube, V(dir.x * 3f, 5f, dir.z * 3f), V(.06f, 10.5f, .06f), metal);
            wire.transform.localRotation = Quaternion.LookRotation(Vector3.up) *
                Quaternion.Euler(70f, dir.x != 0 ? 90f : 0f, 0f);
        }

        // Pump house at base
        P(t, Cube, V(-3.5f, 1.2f, -2f), V(3f, 2.4f, 3.5f), Clr(.62f, .58f, .50f));
        Rot(P(t, Cube, V(-3.5f, 2.5f, -2f), V(3.4f, 1.0f, 4.0f), Clr(.40f, .14f, .10f)), 15, 0, 0);
        P(t, Cube, V(-3.5f, 1.3f, -.28f), V(.9f, 2.0f, .2f), Clr(.26f, .16f, .08f)); // door
        // Pump mechanism (pipes)
        P(t, Cylinder, V(0, 3f, 0), V(.22f, 3f, .22f), metal);           // pump shaft
        P(t, Cube,     V(0, 5.8f, .5f), V(.22f, .22f, 1.0f), metal);   // pump arm

        // Concrete water trough
        P(t, Cube, V(3.5f, .4f, 0), V(3.5f, .8f, 1.5f), Clr(.50f, .48f, .44f)); // walls
        P(t, Cube, V(3.5f, .6f, 0), V(3.0f, .5f, 1.0f), Clr(.26f, .42f, .55f)); // water

        // Fence ring around base (8 posts in octagon)
        for (int f = 0; f < 8; f++)
        {
            float ang = f * 45f * Mathf.Deg2Rad;
            float fx = Mathf.Sin(ang) * 5.5f, fz = Mathf.Cos(ang) * 5.5f;
            P(t, Cube, V(fx, .8f, fz), V(.22f, 1.6f, .22f), Clr(.44f, .32f, .18f));
        }

        return root;
    }

    // ── WATER TOWER ──────────────────────────────────────────────────────────

    GameObject BuildWaterTower(Vector3 pos)
    {
        var root = Root("WaterTower", pos);
        var t = root.transform;
        Color leg   = Clr(.42f, .32f, .20f);
        Color tank  = Clr(.50f, .46f, .38f);
        Color band  = Clr(.32f, .30f, .28f);
        Color metal = Clr(.48f, .50f, .52f);
        Color roof  = Clr(.32f, .26f, .18f);

        P(t, Cube, V(0, .05f, 0), V(8f, .1f, 14f), Clr(.28f, .44f, .18f)); // grass

        // ── 4 legs (center y=4.5 → span 0 to 9)
        float legH = 4.5f;
        foreach (var lp in new[] { V(2f, legH, 2f), V(-2f, legH, 2f), V(2f, legH, -2f), V(-2f, legH, -2f) })
            P(t, Cylinder, lp, V(.26f, legH, .26f), leg);

        // Concrete pads at leg bases
        foreach (var lp in new[] { V(2f, .1f, 2f), V(-2f, .1f, 2f), V(2f, .1f, -2f), V(-2f, .1f, -2f) })
            P(t, Cylinder, lp, V(.7f, .1f, .7f), Clr(.44f, .42f, .38f));

        // Horizontal bracing at 3 heights
        foreach (float hy in new[] { 1.5f, 4.5f, 8f })
        {
            P(t, Cube, V( 0f, hy,  2f), V(4.2f, .14f, .14f), leg);
            P(t, Cube, V( 0f, hy, -2f), V(4.2f, .14f, .14f), leg);
            P(t, Cube, V( 2f, hy,  0f), V(.14f, .14f, 4.2f), leg);
            P(t, Cube, V(-2f, hy,  0f), V(.14f, .14f, 4.2f), leg);
        }
        // Diagonal X-bracing on two faces
        Rot(P(t, Cube, V(0f, 4.5f,  2f), V(5.2f, .10f, .10f), leg),  0, 0,  66);
        Rot(P(t, Cube, V(0f, 4.5f,  2f), V(5.2f, .10f, .10f), leg),  0, 0, -66);
        Rot(P(t, Cube, V(0f, 4.5f, -2f), V(5.2f, .10f, .10f), leg),  0, 0,  66);
        Rot(P(t, Cube, V(0f, 4.5f, -2f), V(5.2f, .10f, .10f), leg),  0, 0, -66);

        // Tank (y=9 to y=15 → center y=12, scaleY=3)
        P(t, Cylinder, V(0, 12f, 0), V(4.2f, 3f, 4.2f), tank);
        P(t, Cylinder, V(0, 9.2f, 0), V(4.6f, .25f, 4.6f), band); // base collar

        // Vertical stave strips (wooden barrel look)
        for (int s = 0; s < 10; s++)
        {
            float a = s * 36f * Mathf.Deg2Rad;
            P(t, Cube, V(Mathf.Sin(a) * 2.25f, 12f, Mathf.Cos(a) * 2.25f),
                V(.1f, 6.2f, .1f), Clr(.36f, .30f, .20f));
        }
        // Iron hoop bands
        foreach (float hy in new[] { 9.8f, 11.2f, 12.6f, 14.0f })
            P(t, Cylinder, V(0, hy, 0), V(4.4f, .14f, 4.4f), band);

        // Tiered conical roof
        P(t, Cylinder, V(0, 15.4f, 0), V(4.6f, .18f, 4.6f), band);
        P(t, Cylinder, V(0, 15.9f, 0), V(4.0f, .30f, 4.0f), roof);
        P(t, Cylinder, V(0, 16.5f, 0), V(3.0f, .30f, 3.0f), roof);
        P(t, Cylinder, V(0, 17.0f, 0), V(2.0f, .28f, 2.0f), roof);
        P(t, Cylinder, V(0, 17.5f, 0), V(1.1f, .26f, 1.1f), roof);
        P(t, Sphere,   V(0, 18.0f, 0), V(.5f,  .5f,  .5f),  metal);

        // Downpipe + elbow
        P(t, Cylinder, V(2.5f, 6.5f, 0), V(.16f, 6.5f, .16f), metal);
        P(t, Sphere,   V(2.5f, 12.6f, 0), V(.30f, .30f, .30f), metal);

        // Access ladder (two rails + rungs)
        P(t, Cylinder, V(-2.5f, 6.5f, -.3f), V(.08f, 6.5f, .08f), metal);
        P(t, Cylinder, V(-2.5f, 6.5f,  .3f), V(.08f, 6.5f, .08f), metal);
        for (int r = 0; r < 9; r++)
            P(t, Cube, V(-2.5f, r * 1.4f + .8f, 0), V(.12f, .08f, .65f), metal);

        // Storage shed at base
        P(t, Cube, V(4.5f, 1.4f, -2.5f), V(3.5f, 2.8f, 4.0f), Clr(.55f, .50f, .42f));
        Rot(P(t, Cube, V(4.5f, 2.9f, -2.5f), V(4.0f, .8f, 4.5f), Clr(.36f, .14f, .10f)), 12, 0, 0);
        P(t, Cube, V(4.5f, 1.4f, -.55f), V(1.2f, 2.2f, .2f), Clr(.26f, .18f, .10f)); // shed door

        return root;
    }

    // ── Primitive helpers ─────────────────────────────────────────────────────

    static readonly PrimitiveType Cube     = PrimitiveType.Cube;
    static readonly PrimitiveType Cylinder = PrimitiveType.Cylinder;
    static readonly PrimitiveType Sphere   = PrimitiveType.Sphere;

    static GameObject Root(string name, Vector3 wp) { var g = new GameObject(name); g.transform.position = wp; return g; }

    static GameObject P(Transform parent, PrimitiveType type, Vector3 lp, Vector3 ls, Color color)
    {
        var go = GameObject.CreatePrimitive(type);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = lp;
        go.transform.localScale    = ls;
        var col = go.GetComponent<Collider>();
        if (col != null) Object.Destroy(col);
        go.GetComponent<Renderer>().material = Mat(color);
        return go;
    }

    static GameObject Prim(Transform parent, PrimitiveType type, Vector3 lp, Vector3 ls, Color color)
        => P(parent, type, lp, ls, color);

    static void Rot(GameObject go, float x, float y, float z)
        => go.transform.localRotation = Quaternion.Euler(x, y, z);

    static Vector3 V(float x, float y, float z) => new Vector3(x, y, z);
    static Color   Clr(float r, float g, float b) => new Color(r, g, b);

    static Material Mat(Color color)
    {
        foreach (var n in new[] { "Universal Render Pipeline/Lit", "Universal Render Pipeline/Simple Lit", "Standard" })
        {
            var shader = Shader.Find(n);
            if (shader == null) continue;
            return new Material(shader) { color = color };
        }
        return null;
    }
}
