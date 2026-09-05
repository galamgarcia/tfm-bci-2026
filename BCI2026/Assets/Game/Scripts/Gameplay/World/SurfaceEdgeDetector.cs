/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System.Collections.Generic;
using UnityEngine;

namespace Bit.Gameplay
{
    /// <summary>Calculates exposed edge intervals for the anchored surface on this GameObject.</summary>
    [AddComponentMenu("Bit/Gameplay/Surface Edge Detector")]
    public sealed class SurfaceEdgeDetector : MonoBehaviour
    {
        /// <summary>Contains the visible intervals and edge mask for one surface.</summary>
        public readonly struct Layout
        {
            // Visible intervals on the top edge.
            public readonly List<SurfaceSegment> Top;
            // Visible intervals on the bottom edge.
            public readonly List<SurfaceSegment> Bottom;
            // Visible intervals on the left edge.
            public readonly List<SurfaceSegment> Left;
            // Visible intervals on the right edge.
            public readonly List<SurfaceSegment> Right;
            // Sides with at least one visible interval.
            public readonly WorldSurfaceEdges Mask;

            /// <summary>Creates a calculated edge layout.</summary>
            /// <param name="top">Visible top intervals.</param>
            /// <param name="bottom">Visible bottom intervals.</param>
            /// <param name="left">Visible left intervals.</param>
            /// <param name="right">Visible right intervals.</param>
            /// <param name="mask">Sides with at least one visible interval.</param>
            public Layout(List<SurfaceSegment> top, List<SurfaceSegment> bottom, List<SurfaceSegment> left, List<SurfaceSegment> right, WorldSurfaceEdges mask)
            {
                Top = top;
                Bottom = bottom;
                Left = left;
                Right = right;
                Mask = mask;
            }
        }

        // Anchored surface whose concrete type defines the comparison group.
        private AnchoredSurface _source;

        private void Awake()
        {
            _source = GetComponent<AnchoredSurface>();
        }

        /// <summary>Calculates visible intervals using automatic neighbors or manual edge flags.</summary>
        /// <param name="isAutomatic">Indicates if neighboring surfaces should be detected.</param>
        /// <param name="exposedEdges">Manual edge mask used when detection is disabled.</param>
        /// <returns>The calculated visible intervals and mask.</returns>
        public Layout Calculate(bool isAutomatic, WorldSurfaceEdges exposedEdges)
        {
            _source ??= GetComponent<AnchoredSurface>();
            AnchoredSurface[] surfaces = isAutomatic ? FindObjectsByType<AnchoredSurface>(FindObjectsInactive.Include, FindObjectsSortMode.None) : null;
            List<SurfaceSegment> top = CalculateVisibleSegments(SurfaceEdge.Top, surfaces, exposedEdges);
            List<SurfaceSegment> bottom = CalculateVisibleSegments(SurfaceEdge.Bottom, surfaces, exposedEdges);
            List<SurfaceSegment> left = CalculateVisibleSegments(SurfaceEdge.Left, surfaces, exposedEdges);
            List<SurfaceSegment> right = CalculateVisibleSegments(SurfaceEdge.Right, surfaces, exposedEdges);
            WorldSurfaceEdges mask = GetEdgeMask(top, bottom, left, right);
            return new Layout(top, bottom, left, right, mask);
        }

        /// <summary>Calculates the visible intervals for one edge.</summary>
        /// <param name="side">Edge to calculate.</param>
        /// <param name="surfaces">Compatible loaded surfaces, or null for manual mode.</param>
        /// <param name="exposedEdges">Manual edge mask.</param>
        /// <returns>The visible intervals for the selected edge.</returns>
        private List<SurfaceSegment> CalculateVisibleSegments(SurfaceEdge side, AnchoredSurface[] surfaces, WorldSurfaceEdges exposedEdges)
        {
            float length = GetSizeBySide(side);
            List<SurfaceSegment> covered = new List<SurfaceSegment>();
            if (surfaces != null)
            {
                Rect own = _source.GetWorldRect();
                foreach (AnchoredSurface surface in surfaces)
                {
                    if (surface == null) { continue; }
                    if (surface != _source && surface.GetType() == _source.GetType())
                    {
                        AddCoveredSegment(covered, own, surface.GetWorldRect(), side);
                    }
                }
            }
            else if ((exposedEdges & GetEdgeFlag(side)) == 0)
            {
                return new List<SurfaceSegment>();
            }

            return GetUncoveredSegments(covered, length);
        }

        /// <summary>Gets the size along the given edge.</summary>
        /// <param name="side">Edge to check.</param>
        /// <returns>Size of the edge.</returns>
        private float GetSizeBySide(SurfaceEdge side)
        {
            return (side == SurfaceEdge.Top || side == SurfaceEdge.Bottom) ? _source.GetSize().x : _source.GetSize().y;
        }

        /// <summary>Checks if the edge is horizontal.</summary>
        /// <param name="side">Edge to inspect.</param>
        /// <returns>True if the edge is horizontal.</returns>
        private static bool IsHorizontal(SurfaceEdge side)
        {
            return side == SurfaceEdge.Top || side == SurfaceEdge.Bottom;
        }

        /// <summary>Gets the coordinate of an edge in a world rectangle.</summary>
        /// <param name="rect">Surface rectangle.</param>
        /// <param name="side">Edge to inspect.</param>
        /// <returns>The selected edge coordinate.</returns>
        private static float GetEdgeCoordinate(Rect rect, SurfaceEdge side)
        {
            return side switch
            {
                SurfaceEdge.Top     => rect.yMax,
                SurfaceEdge.Bottom  => rect.yMin,
                SurfaceEdge.Left    => rect.xMin,
                _                   => rect.xMax
            };
        }

        /// <summary>Adds the interval covered by a neighboring surface.</summary>
        /// <param name="covered">Covered intervals to extend.</param>
        /// <param name="own">Current surface rectangle.</param>
        /// <param name="other">Neighboring surface rectangle.</param>
        /// <param name="side">Edge being inspected.</param>
        private static void AddCoveredSegment(List<SurfaceSegment> covered, Rect own, Rect other, SurfaceEdge side)
        {
            const float tolerance = 0.001f;
            if (!TouchesEdge(own, other, side, tolerance)) { return; }
            if (!TryGetOverlap(own, other, side, tolerance, out float start, out float end)) { return; }
            covered.Add(CreateCoveredSegment(own, side, start, end));
        }

        /// <summary>Checks if another surface touches or crosses the given edge.</summary>
        private static bool TouchesEdge(Rect own, Rect other, SurfaceEdge side, float tolerance)
        {
            float edge = GetEdgeCoordinate(own, side);
            float otherEdge = GetEdgeCoordinate(other, GetOppositeSide(side));
            float otherStart = GetAcrossStart(other, side);
            float otherEnd = GetAcrossEnd(other, side);

            bool touches = Mathf.Abs(otherEdge - edge) <= tolerance;
            bool crosses = otherStart < edge - tolerance && otherEnd > edge + tolerance;
            return touches || crosses;
        }

        /// <summary>Gets the overlapping interval between two surfaces along an edge.</summary>
        private static bool TryGetOverlap(Rect own, Rect other, SurfaceEdge side, float tolerance, out float overlapStart, out float overlapEnd)
        {
            overlapStart = Mathf.Max(GetAlongStart(own, side), GetAlongStart(other, side));
            overlapEnd = Mathf.Min(GetAlongEnd(own, side), GetAlongEnd(other, side));
            return overlapEnd - overlapStart > tolerance;
        }

        /// <summary>Creates a covered segment relative to the current surface.</summary>
        private static SurfaceSegment CreateCoveredSegment(Rect own, SurfaceEdge side, float overlapStart, float overlapEnd)
        {
            float ownStart = GetAlongStart(own, side);
            float ownEnd = GetAlongEnd(own, side);

            return IsHorizontal(side)
                ? new SurfaceSegment(overlapStart - ownStart, overlapEnd - ownStart)
                : new SurfaceSegment(ownEnd - overlapEnd, ownEnd - overlapStart);
        }
        
        /// <summary>Gets the start coordinate along an edge.</summary>
        private static float GetAlongStart(Rect rect, SurfaceEdge side)
        {
            return IsHorizontal(side) ? rect.xMin : rect.yMin;
        }

        /// <summary>Gets the end coordinate along an edge.</summary>
        private static float GetAlongEnd(Rect rect, SurfaceEdge side)
        {
            return IsHorizontal(side) ? rect.xMax : rect.yMax;
        }

        /// <summary>Gets the start coordinate across an edge.</summary>
        private static float GetAcrossStart(Rect rect, SurfaceEdge side)
        {
            return IsHorizontal(side) ? rect.yMin : rect.xMin;
        }

        /// <summary>Gets the end coordinate across an edge.</summary>
        private static float GetAcrossEnd(Rect rect, SurfaceEdge side)
        {
            return IsHorizontal(side) ? rect.yMax : rect.xMax;
        }

        /// <summary>Gets the edge facing the selected edge on a neighboring surface.</summary>
        /// <param name="side">Current edge.</param>
        /// <returns>The opposite edge.</returns>
        private static SurfaceEdge GetOppositeSide(SurfaceEdge side)
        {
            return side switch
            {
                SurfaceEdge.Top     => SurfaceEdge.Bottom,
                SurfaceEdge.Bottom  => SurfaceEdge.Top,
                SurfaceEdge.Left    => SurfaceEdge.Right,
                _                   => SurfaceEdge.Left
            };
        }

        /// <summary>Subtracts covered intervals from the complete edge interval.</summary>
        /// <param name="covered">Intervals hidden by neighbors.</param>
        /// <param name="length">Complete edge length.</param>
        /// <returns>The uncovered intervals.</returns>
        private static List<SurfaceSegment> GetUncoveredSegments(List<SurfaceSegment> covered, float length)
        {
            const float tolerance = 0.001f;
            SortByStart(covered);
            List<SurfaceSegment> result = new List<SurfaceSegment>();
            float coveredUntil = 0f;
            foreach (SurfaceSegment segment in covered)
            {
                SurfaceSegment clampedSegment = ClampSegment(segment, length);
                AddUncoveredSegment(result, coveredUntil, clampedSegment.Start, tolerance);
                coveredUntil = Mathf.Max(coveredUntil, clampedSegment.End);
            }
            AddUncoveredSegment(result, coveredUntil, length, tolerance);
            return result;
        }

        /// <summary>Sorts segments by their start position.</summary>
        private static void SortByStart(List<SurfaceSegment> segments)
        {
            segments.Sort((a, b) => a.Start.CompareTo(b.Start));
        }

        /// <summary>Clamps a segment to the edge length.</summary>
        private static SurfaceSegment ClampSegment(SurfaceSegment segment, float length)
        {
            return new SurfaceSegment(Mathf.Clamp(segment.Start, 0f, length), Mathf.Clamp(segment.End, 0f, length));
        }

        /// <summary>Adds a gap if there is enough space between two positions.</summary>
        private static void AddUncoveredSegment(List<SurfaceSegment> segments, float start, float end, float tolerance)
        {
            if (end - start > tolerance)
            {
                segments.Add(new SurfaceSegment(start, end));
            }
        }

        /// <summary>Converts visible interval lists into an exposed-edge mask.</summary>
        /// <param name="top">Visible top intervals.</param>
        /// <param name="bottom">Visible bottom intervals.</param>
        /// <param name="left">Visible left intervals.</param>
        /// <param name="right">Visible right intervals.</param>
        /// <returns>The mask containing edges with visible intervals.</returns>
        private static WorldSurfaceEdges GetEdgeMask(List<SurfaceSegment> top, List<SurfaceSegment> bottom,
            List<SurfaceSegment> left, List<SurfaceSegment> right)
        {
            WorldSurfaceEdges mask = WorldSurfaceEdges.None;
            if (top.Count > 0)      { mask |= WorldSurfaceEdges.Top;    }
            if (bottom.Count > 0)   { mask |= WorldSurfaceEdges.Bottom; }
            if (left.Count > 0)     { mask |= WorldSurfaceEdges.Left;   }
            if (right.Count > 0)    { mask |= WorldSurfaceEdges.Right;  }
            return mask;
        }

        /// <summary>Gets the flags value corresponding to one edge.</summary>
        /// <param name="side">Edge to convert.</param>
        /// <returns>The matching edge flag.</returns>
        private static WorldSurfaceEdges GetEdgeFlag(SurfaceEdge side)
        {
            return side switch
            {
                SurfaceEdge.Top     => WorldSurfaceEdges.Top,
                SurfaceEdge.Bottom  => WorldSurfaceEdges.Bottom,
                SurfaceEdge.Left    => WorldSurfaceEdges.Left,
                _                   => WorldSurfaceEdges.Right
            };
        }
    }
}
