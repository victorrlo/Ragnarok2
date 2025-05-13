using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMovement : MonoBehaviour
{
    [SerializeField] protected Grid _grid;

    protected virtual void Awake()
    {
        _grid = FindFirstObjectByType<Grid>();
    }
    protected virtual IEnumerator FollowPath(List<Node> path, float moveSpeed = 1f)
    {
        Vector3Int previousCell = _grid.WorldToCell(transform.position);

        foreach (Node node in path)
        {
            Vector3 destinationWorld = _grid.GetCellCenterWorld(node._gridPosition);
            Vector3 flatDestination = new Vector3(destinationWorld.x, 0, destinationWorld.z);

            while (Vector3.Distance(transform.position, flatDestination) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, flatDestination, moveSpeed*Time.deltaTime);
                yield return null;
            }

            transform.position = flatDestination;

            Vector3Int newCell = node._gridPosition;

            if (newCell != previousCell)
            {
                OnStep(previousCell, newCell);
                previousCell = newCell;
            }
        }

        OnPathComplete(previousCell);
    }

    protected virtual void OnStep(Vector3Int from, Vector3Int to) {}
    protected virtual void OnPathComplete(Vector3Int finalCell) {}
}
