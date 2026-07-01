using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteRenderOrder : MonoBehaviour
{
    private SpriteRenderer _renderer;
    [SerializeField] bool _shouldRemainBehind = false;

    private void Awake()
    {
        this.TryGetComponent<SpriteRenderer>(out _renderer);
    }

    private void LateUpdate()
    {
        if (_shouldRemainBehind)
        {
            _renderer.sortingOrder = Mathf.RoundToInt(-transform.position.z * 100) + 32000;
            return;
        }

        _renderer.sortingOrder = Mathf.RoundToInt(-transform.position.z * 100);
    }
}
