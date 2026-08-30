/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEngine;
using UnityEngine.UI;

namespace Bit.UI
{
    /// <summary>Creates the reusable cyan segmented frame used by selected UI elements.</summary>
    [ExecuteAlways]
    public sealed class DashFrame : MonoBehaviour
    {
        private static readonly Color DashColor = new(0.08f, 0.74f, 0.86f, 1f);

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
            if (transform.childCount > 0) { return; }

            CreateDash(new Vector2(0.05f, 0.97f), new Vector2(0.18f, 1f));
            CreateDash(new Vector2(0.25f, 0.97f), new Vector2(0.38f, 1f));
            CreateDash(new Vector2(0.45f, 0.97f), new Vector2(0.58f, 1f));
            CreateDash(new Vector2(0.65f, 0.97f), new Vector2(0.78f, 1f));
            CreateDash(new Vector2(0.82f, 0.97f), new Vector2(0.95f, 1f));
            CreateDash(new Vector2(0.05f, 0f), new Vector2(0.18f, 0.03f));
            CreateDash(new Vector2(0.25f, 0f), new Vector2(0.38f, 0.03f));
            CreateDash(new Vector2(0.45f, 0f), new Vector2(0.58f, 0.03f));
            CreateDash(new Vector2(0.65f, 0f), new Vector2(0.78f, 0.03f));
            CreateDash(new Vector2(0.82f, 0f), new Vector2(0.95f, 0.03f));
            CreateDash(new Vector2(0f, 0.2f), new Vector2(0.03f, 0.42f));
            CreateDash(new Vector2(0f, 0.58f), new Vector2(0.03f, 0.8f));
            CreateDash(new Vector2(0.97f, 0.2f), new Vector2(1f, 0.42f));
            CreateDash(new Vector2(0.97f, 0.58f), new Vector2(1f, 0.8f));
        }

        private void CreateDash(Vector2 min, Vector2 max)
        {
            GameObject go = new("Dash");
            go.transform.SetParent(transform, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = go.AddComponent<Image>();
            image.color = DashColor;
            image.raycastTarget = false;
        }
    }
}
