using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{



    [SerializeField] private AimBehaviour _aimBehaviour;
    [SerializeField] private Grid _grid;
    [SerializeField] private float _moveSpeed;
    private Coroutine _moveCoroutine;


    private void Start()
    {
        _aimBehaviour = FindObjectOfType<AimBehaviour>();
        _grid = FindObjectOfType<Grid>();
    }

    public void OnMouseClick(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        var targetGrid = _aimBehaviour.LastGridCellPosition;
        
        if (targetGrid.HasValue)
        {
            var destinationGrid = _grid.GetCellCenterWorld(targetGrid.Value);
            var newFinalPosition = new Vector3(destinationGrid.x, 0, destinationGrid.z);

            if (_moveCoroutine != null) // if already walking, stop to initiate another movement
            {
                StopCoroutine(_moveCoroutine);
            }

            _moveCoroutine = StartCoroutine(MoveToPosition(newFinalPosition));            
        }
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        float distanceThreshold = 0.1f; // minimal distance to set position to center of grid

        while(Vector3.Distance(transform.position, targetPosition) > distanceThreshold)
        {
            Debug.Log($"Current Position: {transform.position}, Target Position: {targetPosition}, Speed: {_moveSpeed}");
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, _moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition; // reach the final destination (grid center)
    }
}
