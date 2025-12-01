using System;
using UnityEngine;

public class ItemDrop : MonoBehaviour
{
    [SerializeField] private float _jumpHeight = 0.5f;
    [SerializeField] private float _jumpDuration = 0.5f;

    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    private float _jumpTimer;
    private bool _isJumping = false;

    public void InitializeDrop(Vector3 startPos, Vector3 targetPos)
    {
        _startPosition = startPos;
        _targetPosition = targetPos;
        _jumpTimer = 0f;
        _isJumping = true;
    }

    private void Update()
    {
        if (_isJumping)
        {
            // optional: add visual/audio effects when starting jump
            // play sound, particles etc

            _jumpTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(_jumpTimer / _jumpDuration);

            float sinProgress = MathF.Sin(progress * Mathf.PI);
            float height = sinProgress * _jumpHeight;

            Vector3 horizontalPos = Vector3.Lerp(_startPosition, _targetPosition, progress);
            transform.position = horizontalPos + Vector3.up * height;

            if (progress >= 1f)
            {
                // transform.position = _targetPosition;
                _isJumping = false;
                OnLand();
            }
        }
    }

    private void StartJumpAnimation()
    {
        // optional: add visual/audio effects when starting jump
        // play sound, particles etc
    }

    private void OnLand()
    {
        // optional: add landing effects if there's time
        // play sound, small bounce, particles etc 
        // in ragnarok for rare items there's a colored aura, for example
        Debug.Log("Item landed!");
    }
}
