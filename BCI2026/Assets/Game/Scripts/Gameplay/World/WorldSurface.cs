/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System.Collections.Generic;
using UnityEngine;

namespace Bit.Gameplay
{
    /// <summary>Represents a reusable black physical level piece with explicitly exposed cyan edges.</summary>
    [ExecuteAlways]
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(SurfaceEdgeDetector))]
    public sealed class WorldSurface : AnchoredSurface
    {
        [Header("Geometry")]
        [Tooltip("Thickness of every cyan exposed edge in world units.")]
        [SerializeField, Min(0.001f)] private float edgeThickness = 0.02f;
        [Header("Automatic Edges")]
        [Tooltip("Automatically detects covered sides from other WorldSurface pieces in the scene.")]
        [SerializeField] private bool autoDetectExposedEdges = true;
        [Header("Exposed Edges")]
        [Tooltip("Boundaries that meet empty or playable space. Used manually when automatic detection is disabled.")]
        [SerializeField] private WorldSurfaceEdges exposedEdges = WorldSurfaceEdges.Top;
        [Header("Bake")]
        [Tooltip("Uses the serialized edge meshes and collision state during runtime instead of recalculating neighbors.")]
        [SerializeField] private bool isBaked;
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

        // Component that calculates exposed intervals against neighboring surfaces.
        private SurfaceEdgeDetector _edgeDetector;
        // Collider representing the complete physical mass when this piece is exposed.
        private BoxCollider _collider;
        // Prevents recursive refreshes while all scene surfaces are being synchronized.
        private static bool _isRefreshing;
        // Previous size used to detect editor changes that do not invoke validation.
        private Vector2 _lastSize;
        // Previous transform state used to detect moved surfaces in the editor.
        private Vector3 _lastPosition;
        // Procedural meshes used for the four dynamic edge renderers.
        private Mesh _topMesh;
        private Mesh _bottomMesh;
        private Mesh _leftMesh;
        private Mesh _rightMesh;

        private void OnEnable()
        {
            RefreshAll();
        }

        private void OnValidate()
        {
            RefreshAll();
        }

        private void Update()
        {
            if (!autoDetectExposedEdges || _isRefreshing) { return; }
            if (transform.hasChanged || _lastPosition != transform.position || _lastSize != GetSize())
            {
                RefreshAll();
            }
        }

        /// <summary>Updates the block, collider and edge renderers from the current Inspector values.</summary>
        private void ApplyGeometry()
        {
            NormalizeDimensions();
            if (isBaked && Application.isPlaying)
            {
                ApplyBakedGeometry();
                return;
            }

            SurfaceEdgeDetector.Layout layout = CalculateEdgeLayout();
            ApplyBodyGeometry();
            ApplyAdvancedColliderGeometry(layout.Mask != WorldSurfaceEdges.None);
            ApplyEdgeGeometry(layout);
            CacheGeometryState();
        }

        protected override void NormalizeDimensions()
        {
            base.NormalizeDimensions();
            edgeThickness = Mathf.Max(0.001f, edgeThickness);
        }

        /// <summary>Calculates the visible edge intervals and their resulting mask.</summary>
        /// <returns>The current automatic or manual edge layout.</returns>
        private SurfaceEdgeDetector.Layout CalculateEdgeLayout()
        {
            _edgeDetector ??= GetComponent<SurfaceEdgeDetector>();
            SurfaceEdgeDetector.Layout layout = _edgeDetector.Calculate(autoDetectExposedEdges, exposedEdges);
            if (autoDetectExposedEdges)
            {
                exposedEdges = layout.Mask;
            }
            return layout;
        }

        /// <summary>Applies the visual black body at the anchored surface center.</summary>
        private void ApplyBodyGeometry()
        {
            Vector3 center = GetLocalSurfaceCenter();
            if (block != null)
            {
                block.localPosition = center;
                Vector2 size = GetSize();
                block.localScale = new Vector3(size.x, size.y, GetDepth());
            }
        }

        /// <summary>Applies the physical collider dimensions and enabled state.</summary>
        /// <param name="hasVisibleEdges">Whether the surface has any exposed contour.</param>
        private void ApplyAdvancedColliderGeometry(bool hasVisibleEdges)
        {
            _collider ??= GetComponent<BoxCollider>();
            ApplyColliderGeometry(_collider);
            _collider.enabled = hasVisibleEdges;
        }

        /// <summary>Generates and applies all four procedural edge meshes.</summary>
        /// <param name="layout">Visible intervals and edge mask to apply.</param>
        private void ApplyEdgeGeometry(SurfaceEdgeDetector.Layout layout)
        {
            float edgeDepth = -(GetDepth() * 0.5f + 0.001f);
            SetEdge(topEdge, GetEdgeMesh(topEdge, ref _topMesh), layout.Top, SurfaceEdge.Top, edgeDepth);
            SetEdge(bottomEdge, GetEdgeMesh(bottomEdge, ref _bottomMesh), layout.Bottom, SurfaceEdge.Bottom, edgeDepth);
            SetEdge(leftEdge, GetEdgeMesh(leftEdge, ref _leftMesh), layout.Left, SurfaceEdge.Left, edgeDepth);
            SetEdge(rightEdge, GetEdgeMesh(rightEdge, ref _rightMesh), layout.Right, SurfaceEdge.Right, edgeDepth);
        }

        /// <summary>Stores the dimensions and transform used by the last refresh.</summary>
        private void CacheGeometryState()
        {
            _lastPosition = transform.position;
            _lastSize = GetSize();
            transform.hasChanged = false;
        }

        /// <summary>Applies serialized edge meshes without recalculating neighboring surfaces.</summary>
        private void ApplyBakedGeometry()
        {
            ApplyBodyGeometry();
            ApplyAdvancedColliderGeometry(exposedEdges != WorldSurfaceEdges.None);
            SetBakedEdge(topEdge);
            SetBakedEdge(bottomEdge);
            SetBakedEdge(leftEdge);
            SetBakedEdge(rightEdge);
        }

        /// <summary>Enables a baked edge renderer when its serialized mesh contains geometry.</summary>
        /// <param name="edge">Baked edge renderer.</param>
        private static void SetBakedEdge(Renderer edge)
        {
            MeshFilter filter = (edge == null) ? null : edge.GetComponent<MeshFilter>();
            edge?.gameObject.SetActive(filter != null && filter.sharedMesh != null && filter.sharedMesh.vertexCount > 0);
        }

        /// <summary>Refreshes all loaded surfaces from the Editor or a scene-loading tool.</summary>
        public static void RefreshAllLoadedSurfaces()
        {
            WorldSurface[] surfaces = FindSurfaces();
            if (surfaces.Length > 0) { surfaces[0].RefreshAll(); }
        }

        /// <summary>Refreshes every WorldSurface currently loaded in the active scenes.</summary>
        private void RefreshAll()
        {
            if (_isRefreshing) { return; }
            _isRefreshing = true;
            try
            {
                WorldSurface[] surfaces = FindSurfaces();
                foreach (WorldSurface surface in surfaces)
                {
                    surface.ApplyGeometry();
                }
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        /// <summary>Finds all active and inactive world surfaces in the loaded scenes.</summary>
        /// <returns>Loaded WorldSurface components.</returns>
        private static WorldSurface[] FindSurfaces()
        {
            return FindObjectsByType<WorldSurface>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        }

        /// <summary>Synchronizes one edge renderer with its visible procedural segments.</summary>
        /// <param name="edge">Renderer representing the edge.</param>
        /// <param name="mesh">Procedural mesh assigned to the edge.</param>
        /// <param name="segments">Visible intervals along the edge.</param>
        /// <param name="side">Boundary represented by the renderer.</param>
        /// <param name="depth">Local Z position of the boundary.</param>
        private void SetEdge(Renderer edge, Mesh mesh, List<SurfaceSegment> segments, SurfaceEdge side, float depth)
        {
            if (edge == null) { return; }
            edge.transform.localPosition = Vector3.zero;
            edge.transform.localScale = Vector3.one;
            if (mesh == null)
            {
                edge.gameObject.SetActive(false);
                return;
            }

            BuildEdgeMesh(mesh, segments, side, depth);
            edge.gameObject.SetActive(segments.Count > 0);
        }

        /// <summary>Gets or creates the procedural mesh assigned to one edge renderer.</summary>
        /// <param name="edge">Edge renderer whose mesh filter should receive the mesh.</param>
        /// <param name="mesh">Cached mesh field for the edge.</param>
        /// <returns>The cached mesh, or null when the edge has no mesh filter.</returns>
        private static Mesh GetEdgeMesh(Renderer edge, ref Mesh mesh)
        {
            if (edge == null || !edge.TryGetComponent(out MeshFilter filter)) { return null; }
            mesh ??= new Mesh
            {
                name = $"{edge.name}_DynamicMesh",
                hideFlags = HideFlags.HideAndDontSave
            };
            filter.sharedMesh = mesh;
            return mesh;
        }

        /// <summary>Builds quads for all exposed intervals on one edge.</summary>
        /// <param name="mesh">Mesh to rebuild.</param>
        /// <param name="segments">Exposed edge intervals.</param>
        /// <param name="side">Side represented by the mesh.</param>
        /// <param name="edgeDepth">Local Z position of the edge.</param>
        private void BuildEdgeMesh(Mesh mesh, List<SurfaceSegment> segments, SurfaceEdge side, float edgeDepth)
        {
            mesh.Clear();
            List<Vector3> vertices = new List<Vector3>(segments.Count * 4);
            List<int> triangles = new List<int>(segments.Count * 6);
            Vector2 size = GetSize();
            foreach (SurfaceSegment segment in segments)
            {
                switch (side)
                {
                    case SurfaceEdge.Top:
                        AddQuad(vertices, triangles, segment.Start, 0f, segment.End, -edgeThickness, edgeDepth);
                        break;
                    case SurfaceEdge.Bottom:
                        AddQuad(vertices, triangles, segment.Start, -size.y + edgeThickness, segment.End, -size.y, edgeDepth);
                        break;
                    case SurfaceEdge.Left:
                        AddQuad(vertices, triangles, 0f, -segment.End, edgeThickness, -segment.Start, edgeDepth);
                        break;
                    case SurfaceEdge.Right:
                        AddQuad(vertices, triangles, size.x - edgeThickness, -segment.End, size.x, -segment.Start, edgeDepth);
                        break;
                }
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
        }

        /// <summary>Adds one rectangular edge segment and its triangles to the mesh buffers.</summary>
        /// <param name="vertices">Vertex list receiving the quad.</param>
        /// <param name="triangles">Triangle index list receiving the quad.</param>
        /// <param name="left">Minimum local X.</param>
        /// <param name="bottom">Minimum local Y.</param>
        /// <param name="right">Maximum local X.</param>
        /// <param name="top">Maximum local Y.</param>
        /// <param name="z">Local Z position.</param>
        private static void AddQuad(List<Vector3> vertices, List<int> triangles, float left, float bottom, float right, float top, float z)
        {
            int index = vertices.Count;
            vertices.Add(new Vector3(left, bottom, z));
            vertices.Add(new Vector3(right, bottom, z));
            vertices.Add(new Vector3(right, top, z));
            vertices.Add(new Vector3(left, top, z));

            triangles.Add(index);
            triangles.Add(index + 2);
            triangles.Add(index + 1);
            triangles.Add(index);
            triangles.Add(index + 3);
            triangles.Add(index + 2);
        }
    }
}
