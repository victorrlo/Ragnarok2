using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMovement : MonoBehaviour
{
    List<Node> _currentPath;

    protected virtual void Awake()
    {
    }

    protected virtual void Start()
    {
        Vector3Int cell = GridManager.Instance.WorldToCell(transform.position);
        Vector3 center = GridManager.Instance.GetCellCenterWorld(cell);

        transform.position = new Vector3(center.x, transform.position.y, center.z);
    }
    public virtual IEnumerator FollowPath(List<Node> path, float moveSpeed = 2f)
    {
        Vector3Int previousCell = GridManager.Instance.WorldToCell(transform.position);

        if (path == null) yield return null;
        
        foreach (Node node in path)
        {
            if (node == null) continue;
            
            Vector3 destinationWorld = GridManager.Instance.GetCellCenterWorld(node._gridPosition);
            Vector3 flatDestination = new Vector3(destinationWorld.x, 0, destinationWorld.z);

            while (Vector3.Distance(transform.position, flatDestination) > 0.05f)
            {
                float currentMoveSpeed = moveSpeed*Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, flatDestination, currentMoveSpeed);
                yield return null;
            }

            transform.position = flatDestination;

            Vector3Int newCell = node._gridPosition;

            if (newCell != previousCell)
            {
                OnStep(newCell);
                previousCell = newCell;
            }
        }

        OnPathComplete(previousCell);
    }

    protected virtual void OnStep(Vector3Int newPos) {}
    protected virtual void OnPathComplete(Vector3Int finalCell) {}
    public virtual void UpdatePath(List<Node> newPath) {
        _currentPath = newPath;
    }
}
