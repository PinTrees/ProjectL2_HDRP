using System.Collections.Generic;
using UnityEngine;



public class AnimallGroup : MonoBehaviour
{
    [SerializeField] GameObject SpawnObject;

    [Range(1, 100)]
    public float BasSpawnCount;
    [Range(1, 100)]
    public float SpawnRange;
    [Range(1, 100)]
    public float WalkRange;
    public bool RandomRotation = true;

    [SerializeField] List<AnimallBase> targets = new();

    // Editor
    private TerrainCollider terrain;


    void Start()
    {
    }


    void Update()
    {
    }

    public void _EditorSpawnObject()
    {
        terrain = GameObject.FindWithTag("Terrain").GetComponent<TerrainCollider>();

        Ray ray = new Ray(new Vector3(transform.position.x, 1000, transform.position.z), Vector3.down);
        RaycastHit hit;
        if (terrain.Raycast(ray, out hit, Mathf.Infinity))
        {
            transform.position = hit.point;
        }

        for (int i = 0; i < BasSpawnCount; ++i)
        {
            var spawnAniamll = Instantiate(SpawnObject);
            var spawn2dPosition = transform.position + new Vector3(Random.Range(SpawnRange * -1f, SpawnRange * 1f), 1000, Random.Range(SpawnRange * -1f, SpawnRange * 1f));

            Ray ray_spawn = new Ray(spawn2dPosition, Vector3.down);
            RaycastHit hit_spawn;
            if (terrain.Raycast(ray_spawn, out hit_spawn, Mathf.Infinity))
            {
                spawnAniamll.transform.position = hit_spawn.point;
            }
            if (RandomRotation)
            {
                spawnAniamll.transform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0);
            }

            spawnAniamll.transform.SetParent(transform, true);
            targets.Add(spawnAniamll.GetComponent<AnimallBase>());
        }
    }

    public void _EditorRemoveObject()
    {
        targets.ForEach(e => 
        {
            if(e != null)
            {
                DestroyImmediate(e.gameObject);
            }
        });
        targets.Clear();
    }

    public void _EditorRefresh()
    {
        targets.ForEach(e =>
        {
            if (e != null)
            {
                DestroyImmediate(e.gameObject);
            }
        });
        targets.Clear();
    }
}
