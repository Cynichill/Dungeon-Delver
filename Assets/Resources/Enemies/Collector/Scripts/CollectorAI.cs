using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectorAI : MonoBehaviour
{
    private PathFind pf;
    private Transform player;
    public TileCoordinate[,] grid;
    private List<Vector3> positions = new List<Vector3>();
    private int index = 0;
    private float speed = 4;
    private List<GameObject> collectables = new List<GameObject>();
    private GameObject existingCollectable;

    // Update is called once per frame
    void Update()
    {

        if (existingCollectable == null)
        {
            SetTarget();
        }

        if (positions.Count != 0)
        {
            Vector3 nextTarget = positions[index];
            if (Vector3.Distance(transform.position, nextTarget) > 0.1f)
            {
                Vector3 moveDirection = (nextTarget - transform.position).normalized;
                transform.position = transform.position + moveDirection * speed * Time.deltaTime;
            }
            else
            {
                index++;

                if (index >= positions.Count)
                {
                    //SetTarget(player.position);
                    index--;
                }
            }
        }
    }

    public void FindObjects(List<GameObject> collect)
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        pf = GameObject.FindGameObjectWithTag("Generation").GetComponent<PathFind>();
        collectables = collect;
    }

    public void SetTarget()
    {
        if (collectables.Count > 0)
        {
            Vector3 Destination = new Vector3(collectables[0].transform.position.x, collectables[0].transform.position.y, collectables[0].transform.position.z);
            existingCollectable = collectables[0];
            collectables.RemoveAt(0);
            index = 0;

            List<TileCoordinate> tempPath = new List<TileCoordinate>();
            tempPath = pf.TilePath(grid, grid[Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y)], grid[Mathf.RoundToInt(Destination.x), Mathf.RoundToInt(Destination.y)]);

            if (tempPath != null)
            {
                ConvertPath(tempPath);
            }
        }

    }

    private void ConvertPath(List<TileCoordinate> path)
    {
        positions.Clear();

        foreach (TileCoordinate tile in path)
        {
            positions.Add(new Vector3(tile.xCoord, tile.yCoord, 0));
        }
    }

}
