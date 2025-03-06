using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AimBehaviour : MonoBehaviour
{
    private Camera _mainCamera;
    [SerializeField] private LayerMask _aimLayerMask;
    [SerializeField] private Grid _grid; 
    private float _gridSize;

    private void Start()
    {
        _mainCamera = Camera.main;
        _gridSize = _grid.cellSize.x;
    }

    public void ChangeAimPosition(InputAction.CallbackContext context)
    {
        Vector2 mousePos = context.ReadValue<Vector2>();

        // Criamos um raio da câmera a partir da posição do mouse
        Ray ray = _mainCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;

        

        // Definimos o layer onde a mira pode ficar (exemplo: chão)
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, _aimLayerMask))
        {
            // Colocamos a mira na posição exata onde o raycast bateu
            Vector3 closestPosition = GetClosestCell(hit.point);
            Vector3 snappedPosition = SnapToGrid(closestPosition);
            transform.position = snappedPosition;
        }
    }

    private Vector3 SnapToGrid(Vector3 position)
    {
        float snappedX = Mathf.Round(position.x / _gridSize) * _gridSize;
        float snappedZ = Mathf.Round(position.z / _gridSize) * _gridSize;
        return new Vector3(snappedX +0.5f , 0, snappedZ +0.5f);
    }

    private Vector3 GetClosestCell(Vector3 position)
    {
        float x1 = Mathf.Round(position.x / _gridSize) * _gridSize;
        float x2 = Mathf.Ceil(position.x / _gridSize) * _gridSize;

        float z1 = Mathf.Round(position.z / _gridSize) * _gridSize;
        float z2 = Mathf.Ceil(position.z / _gridSize) * _gridSize;

        Vector3[] candidates = new Vector3[]
        {
            new Vector3(x1, 0, z1),
            new Vector3(x1, 0, z2),
            new Vector3(x2, 0, z1),
            new Vector3(x2, 0, z2),
        };

        Vector3 closestPoint = candidates[0];
        float minDistance = Vector3.Distance(position, closestPoint);

        for (int i = 1; i < candidates.Length; i++)
        {
            float distance = Vector3.Distance(position, candidates[i]);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestPoint = candidates[i];
            }
        }

        return closestPoint;
    }
}
