using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaserAI : MonoBehaviour
{
    private PathFind pf;
    private Transform player;
    public TileCoordinate[,] grid;
    private List<Vector3> positions = new List<Vector3>();
    private int index = 0;
    private float speed = 7;
    private Vector3 playerPosStore;
    private bool canChange;

    // Update is called once per frame
    void Update()
    {

        if (playerPosStore != player.position && canChange)
        {
            SetTarget(player.position);
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

                if (index > 1 && !canChange)
                {
                    canChange = true;
                }

                if (index >= positions.Count)
                {
                    //SetTarget(player.position);
                    index--;
                }
            }
        }
    }

    public void FindObjects()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        pf = GameObject.FindGameObjectWithTag("Generation").GetComponent<PathFind>();
    }

    public void SetTarget(Vector3 Destination)
    {
        canChange = false;
        index = 0;

        playerPosStore = player.transform.position;

        List<TileCoordinate> tempPath = new List<TileCoordinate>();
        tempPath = pf.TilePath(grid, grid[Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y)], grid[Mathf.RoundToInt(Destination.x), Mathf.RoundToInt(Destination.y)]);

        if (tempPath != null)
        {
            ConvertPath(tempPath);
        }

    }

    private void ConvertPath(List<TileCoordinate> path)
    {
        positions.Clear();

        foreach (TileCoordinate tile in path)
        {
            positions.Add(new Vector3(tile.xCoord, tile.yCoord, 0));
        }
        positions.Add(player.transform.position);
    }

}
