/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEngine;
using UnityEngine.UI;

namespace Bit.UI
{
    /// <summary>Builds the reusable segmented BIT logo at its prefab position.</summary>
    [ExecuteAlways]
    public sealed class Logo : MonoBehaviour
    {
        private static readonly Color Color = new(0.08f, 0.74f, 0.86f, 1f);

        [Header("Logo")]
        [Tooltip("Width and height of the segmented logo in canvas pixels.")]
        [SerializeField] private Vector2 size = new(300f, 100f);

        // Root rect used to size and position the logo segments.
        private RectTransform _rect;

        private void Awake()
        {
            Build();
        }

        private void OnEnable()
        {
            Build();
        }

        private void Build()
        {
            _rect ??= GetComponent<RectTransform>();
            if (_rect != null) { _rect.sizeDelta = size; }
            if (transform.childCount > 0) { return; }

            AddSegment(new Vector2(-108f, 0f), new Vector2(10f, 80f));
            AddSegment(new Vector2(-62f, 35f), new Vector2(70f, 10f));
            AddSegment(new Vector2(-62f, 0f), new Vector2(70f, 10f));
            AddSegment(new Vector2(-62f, -35f), new Vector2(70f, 10f));
            AddSegment(new Vector2(-20f, 18f), new Vector2(10f, 25f));
            AddSegment(new Vector2(-20f, -18f), new Vector2(10f, 25f));
            AddSegment(new Vector2(0f, 0f), new Vector2(10f, 80f));
            AddSegment(new Vector2(58f, 35f), new Vector2(70f, 10f));
            AddSegment(new Vector2(58f, 0f), new Vector2(10f, 80f));
        }

        private void AddSegment(Vector2 position, Vector2 size)
        {
            GameObject segment = new("Segment");
            segment.transform.SetParent(transform, false);
            RectTransform rect = segment.AddComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Image image = segment.AddComponent<Image>();
            image.color = Color;
            image.raycastTarget = false;
        }
    }
}
