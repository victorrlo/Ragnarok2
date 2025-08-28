using UnityEngine;

public class TargetMarker : MonoBehaviour
{
    private Transform _target;
    private Transform _cameraPivot;
    private Transform _mainCamera;
    [SerializeField] private float _bobAmplitude = 0.25f;
    [SerializeField] private float _bobFrequency = 2f;
    private float _baseHeight;
    private float _offset = 0.5f;

    private void Awake()
    {
        _mainCamera = Camera.main ? Camera.main.transform : null;
    }

    private void Start()
    {
        _cameraPivot = GameObject.FindWithTag("Pivot")?.transform;
    }

    public void AttachTo(Transform target)
    {
        _target = target;
    }

    private void LateUpdate()
    {
        if (!_target || !_mainCamera) { Destroy(gameObject); return;}

        transform.rotation = _cameraPivot.rotation;

        if (_target == null)
        {
            Destroy(gameObject);
            return;
        }

        float bob = Mathf.Sin(Time.time * _bobFrequency) * _bobAmplitude;
        Vector3 up = _mainCamera.up;

        transform.position = _target.position + up * (_offset + bob);

        transform.rotation = Quaternion.LookRotation(_mainCamera.forward, _mainCamera.up);
    }
}