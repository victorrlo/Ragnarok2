using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteRenderOrder : MonoBehaviour
{
    private SpriteRenderer _renderer;

    private void Awake()
    {
        this.TryGetComponent<SpriteRenderer>(out _renderer);
    }

    private void LateUpdate()
    {
        _renderer.sortingOrder = Mathf.RoundToInt(-transform.position.z * 100);
    }
}
