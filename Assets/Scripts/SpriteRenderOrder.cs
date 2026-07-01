using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteRenderOrder : MonoBehaviour
{
    private const int MinSortingOrder = -30000;
    private const int MaxSortingOrder = 30000;

    private SpriteRenderer _renderer;

    [SerializeField] private bool _shouldRemainBehind = false;
    [SerializeField] private int _orderOffset = 0;
    [SerializeField] private int _behindOrderOffset = -1000;
    [SerializeField, Min(1f)] private float _precision = 10f;

    private void Awake()
    {
        TryGetComponent(out _renderer);
    }

    private void LateUpdate()
    {
        int order = Mathf.RoundToInt(-transform.position.z * _precision) + _orderOffset;

        if (_shouldRemainBehind)
        {
            order += _behindOrderOffset;
        }

        _renderer.sortingOrder = Mathf.Clamp(order, MinSortingOrder, MaxSortingOrder);
    }
}
