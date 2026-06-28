using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterVisualEffectController : MonoBehaviour
{
    [System.Serializable]
    public class SpriteOverlaySettings
    {
        public string Id = "overlay";
        public Color Color = new Color(0.25f, 0.65f, 1f, 0.55f);
        public Material Material;
        public int SortingOrderOffset = 1;
        public Vector3 LocalPositionOffset;
        public Vector3 LocalScaleMultiplier = Vector3.one;
    }

    private class SpriteOverlay
    {
        public SpriteRenderer Source;
        public SpriteRenderer Renderer;
        public Transform Transform;
        public int SortingOrderOffset;
    }

    [SerializeField] private SpriteRenderer[] _sourceRenderers;
    [SerializeField] private bool _autoFindSpriteRenderers = true;

    private readonly Dictionary<string, List<SpriteOverlay>> _activeOverlays = new();
    private readonly Dictionary<string, Coroutine> _timedEffects = new();

    private void Awake()
    {
        EnsureSourceRenderers();
    }

    private void LateUpdate()
    {
        foreach (var overlays in _activeOverlays.Values)
        {
            foreach (var overlay in overlays)
            {
                if (overlay.Source == null || overlay.Renderer == null)
                    continue;

                SyncOverlayWithSource(overlay);
            }
        }
    }

    public void PlayOverlay(SpriteOverlaySettings settings, float duration)
    {
        if (settings == null)
            return;

        ShowOverlay(settings);

        if (duration > 0f)
        {
            if (_timedEffects.TryGetValue(settings.Id, out Coroutine activeRoutine))
                StopCoroutine(activeRoutine);

            _timedEffects[settings.Id] = StartCoroutine(HideOverlayAfter(settings.Id, duration));
        }
    }

    public void ShowOverlay(SpriteOverlaySettings settings)
    {
        if (settings == null)
            return;

        EnsureSourceRenderers();

        HideOverlay(settings.Id);

        List<SpriteOverlay> overlays = new();

        foreach (SpriteRenderer sourceRenderer in _sourceRenderers)
        {
            if (sourceRenderer == null)
                continue;

            SpriteOverlay overlay = CreateOverlay(sourceRenderer, settings);
            overlays.Add(overlay);
            SyncOverlayWithSource(overlay);
        }

        if (overlays.Count > 0)
            _activeOverlays[settings.Id] = overlays;
    }

    public void HideOverlay(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return;

        if (_timedEffects.TryGetValue(id, out Coroutine activeRoutine))
        {
            StopCoroutine(activeRoutine);
            _timedEffects.Remove(id);
        }

        if (!_activeOverlays.TryGetValue(id, out List<SpriteOverlay> overlays))
            return;

        foreach (SpriteOverlay overlay in overlays)
        {
            if (overlay.Transform != null)
                Destroy(overlay.Transform.gameObject);
        }

        _activeOverlays.Remove(id);
    }

    public void HideAllOverlays()
    {
        string[] ids = new string[_activeOverlays.Keys.Count];
        _activeOverlays.Keys.CopyTo(ids, 0);

        foreach (string id in ids)
            HideOverlay(id);
    }

    private IEnumerator HideOverlayAfter(string id, float duration)
    {
        yield return new WaitForSeconds(duration);

        _timedEffects.Remove(id);
        HideOverlay(id);
    }

    private SpriteOverlay CreateOverlay(SpriteRenderer sourceRenderer, SpriteOverlaySettings settings)
    {
        GameObject overlayObject = new($"{sourceRenderer.gameObject.name}-{settings.Id}-overlay");
        overlayObject.transform.SetParent(sourceRenderer.transform, false);
        overlayObject.transform.localPosition = settings.LocalPositionOffset;
        overlayObject.transform.localRotation = Quaternion.identity;
        overlayObject.transform.localScale = settings.LocalScaleMultiplier;

        SpriteRenderer overlayRenderer = overlayObject.AddComponent<SpriteRenderer>();
        overlayRenderer.color = settings.Color;

        if (settings.Material != null)
            overlayRenderer.material = settings.Material;

        SpriteOverlay overlay = new()
        {
            Source = sourceRenderer,
            Renderer = overlayRenderer,
            Transform = overlayObject.transform,
            SortingOrderOffset = settings.SortingOrderOffset
        };

        SyncOverlayWithSource(overlay);
        return overlay;
    }

    private void SyncOverlayWithSource(SpriteOverlay overlay)
    {
        SpriteRenderer source = overlay.Source;
        SpriteRenderer target = overlay.Renderer;

        target.enabled = source.enabled && source.gameObject.activeInHierarchy;
        target.sprite = source.sprite;
        target.flipX = source.flipX;
        target.flipY = source.flipY;
        target.drawMode = source.drawMode;
        target.size = source.size;
        target.tileMode = source.tileMode;
        target.maskInteraction = source.maskInteraction;
        target.spriteSortPoint = source.spriteSortPoint;
        target.sortingLayerID = source.sortingLayerID;
        target.sortingOrder = source.sortingOrder + overlay.SortingOrderOffset;
    }

    private void EnsureSourceRenderers()
    {
        if (!_autoFindSpriteRenderers && _sourceRenderers != null && _sourceRenderers.Length > 0)
            return;

        if (_sourceRenderers != null && _sourceRenderers.Length > 0)
            return;

        _sourceRenderers = FindDefaultSourceRenderers();
    }

    private SpriteRenderer[] FindDefaultSourceRenderers()
    {
        List<SpriteRenderer> renderers = new();
        SpriteRenderer[] childRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer renderer in childRenderers)
        {
            string objectName = renderer.gameObject.name.ToLowerInvariant();

            if (objectName == "sprite")
                renderers.Add(renderer);
        }

        if (renderers.Count > 0)
            return renderers.ToArray();

        foreach (SpriteRenderer renderer in childRenderers)
        {
            string objectName = renderer.gameObject.name.ToLowerInvariant();

            if (objectName.Contains("shadow") || objectName.Contains("emote"))
                continue;

            renderers.Add(renderer);
        }

        return renderers.ToArray();
    }

    private void OnDisable()
    {
        HideAllOverlays();
    }
}
