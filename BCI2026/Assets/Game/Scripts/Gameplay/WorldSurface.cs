/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEngine;

namespace Bit.Gameplay
{
    /// <summary>Represents a reusable black physical level piece with explicitly exposed cyan edges.</summary>
    [ExecuteAlways]
    [RequireComponent(typeof(BoxCollider))]
    public sealed class WorldSurface : MonoBehaviour
    {
        [Header("Geometry")]
        [Tooltip("Width and height of the physical piece in world units.")]
        [SerializeField] private Vector2 size = new Vector2(2f, 0.2f);
        [Tooltip("Depth of the physical piece along the Z axis.")]
        [SerializeField, Min(0.01f)] private float depth = 0.2f;
        [Tooltip("Thickness of every cyan exposed edge in world units.")]
        [SerializeField, Min(0.001f)] private float edgeThickness = 0.02f;
        [Header("Exposed Edges")]
        [Tooltip("Boundaries that meet empty or playable space. None creates a black interior piece without collision.")]
        [SerializeField] private WorldSurfaceEdges exposedEdges = WorldSurfaceEdges.Top;
        [Header("References")]
        [Tooltip("Black physical mass renderer transform.")]
        [SerializeField] private Transform block;
        [Tooltip("Renderer for the top exposed boundary.")]
        [SerializeField] private Renderer topEdge;
        [Tooltip("Renderer for the bottom exposed boundary.")]
        [SerializeField] private Renderer bottomEdge;
        [Tooltip("Renderer for the left exposed boundary.")]
        [SerializeField] private Renderer leftEdge;
        [Tooltip("Renderer for the right exposed boundary.")]
        [SerializeField] private Renderer rightEdge;

        // Collider representing the complete physical mass when this piece is exposed.
        private BoxCollider _collider;

        private void OnEnable()
        {
            ApplyGeometry();
        }

        private void OnValidate()
        {
            ApplyGeometry();
        }

        /// <summary>Updates the block, collider and edge renderers from the current Inspector values.</summary>
        private void ApplyGeometry()
        {
            size.x = Mathf.Max(0.01f, size.x);
            size.y = Mathf.Max(0.01f, size.y);
            depth = Mathf.Max(0.01f, depth);
            edgeThickness = Mathf.Max(0.001f, edgeThickness);

            if (block != null)
            {
                block.localPosition = Vector3.zero;
                block.localScale = new Vector3(size.x, size.y, depth);
            }

            _collider ??= GetComponent<BoxCollider>();
            _collider.size = new Vector3(size.x, size.y, depth);
            _collider.center = Vector3.zero;
            _collider.isTrigger = false;
            _collider.enabled = exposedEdges != WorldSurfaceEdges.None;

            float edgeDepth = -(depth * 0.5f + 0.001f);
            SetEdge(topEdge, WorldSurfaceEdges.Top, new Vector3(0f, size.y * 0.5f, edgeDepth), new Vector3(size.x, edgeThickness, 1f));
            SetEdge(bottomEdge, WorldSurfaceEdges.Bottom, new Vector3(0f, -size.y * 0.5f, edgeDepth), new Vector3(size.x, edgeThickness, 1f));
            SetEdge(leftEdge, WorldSurfaceEdges.Left, new Vector3(-size.x * 0.5f, 0f, edgeDepth), new Vector3(edgeThickness, size.y, 1f));
            SetEdge(rightEdge, WorldSurfaceEdges.Right, new Vector3(size.x * 0.5f, 0f, edgeDepth), new Vector3(edgeThickness, size.y, 1f));
        }

        /// <summary>Synchronizes one edge renderer with the current geometry dimensions.</summary>
        /// <param name="edge">Renderer representing the edge.</param>
        /// <param name="type">Boundary represented by the renderer.</param>
        /// <param name="position">Local position of the boundary.</param>
        /// <param name="scale">Local scale preserving the configured thickness.</param>
        private void SetEdge(Renderer edge, WorldSurfaceEdges type, Vector3 position, Vector3 scale)
        {
            if (edge == null) { return; }
            edge.transform.localPosition = position;
            edge.transform.localScale = scale;
            edge.gameObject.SetActive((exposedEdges & type) != 0);
        }
    }
}
