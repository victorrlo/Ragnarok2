using UnityEngine;

public class PlayerClimateManager : MonoBehaviour
{
    [SerializeField] private Transform _targetTransform;
    private float followSpeed = 5f;

    private void LateUpdate()
    {
        if (_targetTransform == null)
        {
            return;
        }

        var targetPosition = new Vector3 (_targetTransform.position.x, transform.position.y, _targetTransform.position.z - 10f);
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }

    public void SetTarget(Transform targetTransform)
    {
        _targetTransform = targetTransform;
    }
}
