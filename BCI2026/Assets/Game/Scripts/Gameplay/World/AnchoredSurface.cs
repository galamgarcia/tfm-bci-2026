/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEngine;

namespace Bit.Gameplay
{
    /// <summary>Provides shared serialized geometry for surfaces anchored at their top-left corner.</summary>
    public abstract class AnchoredSurface : MonoBehaviour
    {
        [Header("Geometry")]
        [Tooltip("Width and height of the surface in world units.")]
        [SerializeField] private Vector2 size = new Vector2(2f, 0.2f);
        [Tooltip("Depth of the surface along the Z axis.")]
        [SerializeField, Min(0.01f)] private float depth = 0.2f;

        /// <summary>Returns the configured width and height of the surface.</summary>
        /// <returns>The surface dimensions.</returns>
        public Vector2 GetSize()
        {
            return size;
        }

        /// <summary>Returns the configured depth of the surface.</summary>
        /// <returns>The surface depth.</returns>
        public float GetDepth()
        {
            return depth;
        }

        /// <summary>Applies the shared physical collider dimensions.</summary>
        /// <param name="collider">Collider receiving the shared geometry.</param>
        protected void ApplyColliderGeometry(BoxCollider collider)
        {
            if (collider == null) { return; }
            collider.size = new Vector3(size.x, size.y, depth);
            collider.center = GetLocalSurfaceCenter();
        }

        /// <summary>Returns the local center for the top-left anchored rectangle.</summary>
        /// <returns>The local center position.</returns>
        protected Vector3 GetLocalSurfaceCenter()
        {
            return new Vector3(size.x * 0.5f, -size.y * 0.5f, 0f);
        }

        /// <summary>Clamps the shared dimensions to valid physical values.</summary>
        protected virtual void NormalizeDimensions()
        {
            size.x = Mathf.Max(0.01f, size.x);
            size.y = Mathf.Max(0.01f, size.y);
            depth = Mathf.Max(0.01f, depth);
        }

        /// <summary>Gets this surface's world-space axis-aligned rectangle.</summary>
        /// <returns>The surface rectangle in world XY coordinates.</returns>
        public Rect GetWorldRect()
        {
            Vector3 topLeft = transform.TransformPoint(Vector3.zero);
            Vector3 bottomRight = transform.TransformPoint(new Vector3(size.x, -size.y, 0f));
            return Rect.MinMaxRect(Mathf.Min(topLeft.x, bottomRight.x), Mathf.Min(topLeft.y, bottomRight.y), Mathf.Max(topLeft.x, bottomRight.x), Mathf.Max(topLeft.y, bottomRight.y));
        }

    }
}
