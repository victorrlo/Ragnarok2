using UnityEngine;

public class TargetMarker : MonoBehaviour
{
    private Transform _target;

    public void AttachTo(Transform target)
    {
        _target = target;
    }

    private void LateUpdate()
    {
        if (_target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = _target.position + Vector3.up * 0.5f;
    }
}