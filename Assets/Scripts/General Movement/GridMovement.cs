using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMovement : MonoBehaviour
{
    public Vector3Int CurrentCell { get; private set; }
    public Vector3Int? IntendedNextCell { get; private set; }
    List<Node> _currentPath;

    protected virtual void Awake()
    {
    }

    protected virtual void Start()
    {
        CurrentCell = GridManager.Instance.WorldToCell(transform.position);
        StartCoroutine(GridHelper.SnapToNearestCellCenter(this.gameObject, 0.15f));
    }
    public virtual IEnumerator FollowPath(List<Node> path,
                                        float moveSpeed = 2f, 
                                        System.Func<Vector3Int, bool> canStepInto = null, 
                                        System.Func<bool> shouldCancel = null)
    {
        Vector3Int previousCell = GridManager.Instance.WorldToCell(transform.position);

        if (path == null || path.Count == 0) yield break;
        
        foreach (Node node in path)
        {
            if (node == null) continue;
            
            var nextCell = node._gridPosition;
            IntendedNextCell = nextCell;

            while(canStepInto != null && !canStepInto(nextCell))
            {
                if (shouldCancel != null && shouldCancel())
                {
                    IntendedNextCell = null;
                    yield break;
                }
                yield return null;
            }
            
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
