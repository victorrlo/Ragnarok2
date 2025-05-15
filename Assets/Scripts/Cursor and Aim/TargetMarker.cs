using UnityEngine;

public class TargetMarker : MonoBehaviour
{
    private Transform _target;
    [SerializeField] private float _floatAmplitude = 0.25f;
    [SerializeField] private float _floatFrequency = 2f;
    private float _baseHeight;
    private Vector3 _offset = Vector3.up * 0.5f;

    public void AttachTo(Transform target)
    {
        _target = target;
        _baseHeight = _offset.y;
    }

    private void LateUpdate()
    {
        if (_target == null)
        {
            Destroy(gameObject);
            return;
        }

        float floatOffset = Mathf.Sin(Time.time * _floatFrequency) * _floatAmplitude;
        Vector3 basePos = _target.position + Vector3.up * _baseHeight;
        transform.position = basePos + Vector3.up * floatOffset;
        // transform.position = _target.position + Vector3.up * 0.5f;

        transform.forward = Camera.main.transform.forward; // to keep turned to camera
    }
}