using Unity.VisualScripting;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Camera _mainCamera;
    private float smoothSpeed = 0.125f;
    private Vector3 _cameraPosition;
    private Transform _player;
    private Vector3 _playerPosition;

    private void Start()
    {
        _mainCamera = Camera.main;
        _cameraPosition = _mainCamera.transform.position;
        _player = GameObject.FindWithTag("Player")?.transform;
        _playerPosition = _player.transform.position;
    }

    void LateUpdate()
    {
        _playerPosition = _player.transform.position;
        _cameraPosition = _playerPosition;
        _mainCamera.transform.position = Vector3.Lerp(_cameraPosition, _playerPosition, smoothSpeed);
    }
}
