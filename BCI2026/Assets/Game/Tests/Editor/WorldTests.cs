/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using Bit.Gameplay;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Bit.Gameplay
{
    /// <summary>Validates world geometry, mental platforms and their reusable prefabs.</summary>
    public sealed class WorldTests
    {
        private const string PrefabPath = "Assets/Game/Prefabs/World/WorldSurface.prefab";

        /// <summary>Verifies World Surface uses static 3D collision without 2D components.</summary>
        [Test]
        public void SurfacePrefab_UsesStaticThreeDimensionalCollision()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);

            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.GetComponent<WorldSurface>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<SurfaceEdgeDetector>(), Is.Not.Null);
            Assert.That(prefab.transform.Find("Body"), Is.Not.Null);
            Assert.That(prefab.GetComponent<BoxCollider>().isTrigger, Is.False);
            Assert.That(prefab.GetComponent<Rigidbody>(), Is.Null);
            Assert.That(prefab.GetComponentsInChildren<Collider>(true), Has.Length.EqualTo(1));
        }

        /// <summary>Verifies None disables collision and every exposed edge renderer.</summary>
        [Test]
        public void SurfaceNone_DisablesCollisionAndEdges()
        {
            GameObject instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
            try
            {
                SetSurfaceAutomaticEdges(instance, false);
                SetSurfaceExposedEdges(instance, WorldSurfaceEdges.None);

                Assert.That(instance.GetComponent<BoxCollider>().enabled, Is.False);
                Assert.That(instance.transform.Find("TopEdge").gameObject.activeSelf, Is.False);
                Assert.That(instance.transform.Find("BottomEdge").gameObject.activeSelf, Is.False);
                Assert.That(instance.transform.Find("LeftEdge").gameObject.activeSelf, Is.False);
                Assert.That(instance.transform.Find("RightEdge").gameObject.activeSelf, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        /// <summary>Verifies size changes update the body, collider and edge dimensions.</summary>
        [Test]
        public void SurfaceSize_UpdatesPhysicalAndVisualDimensions()
        {
            GameObject instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
            try
            {
                SetSurfaceAutomaticEdges(instance, false);
                SerializedObject serialized = new SerializedObject(instance.GetComponent<WorldSurface>());
                serialized.FindProperty("size").vector2Value = new Vector2(4f, 2f);
                serialized.ApplyModifiedPropertiesWithoutUndo();
                instance.GetComponent<WorldSurface>().SendMessage("OnValidate");

                Assert.That(instance.GetComponent<BoxCollider>().size, Is.EqualTo(new Vector3(4f, 2f, 0.2f)));
                Assert.That(instance.transform.Find("Body").localScale, Is.EqualTo(new Vector3(4f, 2f, 0.2f)));
                Assert.That(instance.transform.Find("TopEdge").GetComponent<MeshFilter>().sharedMesh.bounds.size.x, Is.EqualTo(4f));
                Assert.That(instance.transform.Find("TopEdge").GetComponent<MeshFilter>().sharedMesh.bounds.size.y, Is.EqualTo(0.01f));
                Assert.That(instance.transform.Find("LeftEdge").gameObject.activeSelf, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        /// <summary>Verifies adjacent surfaces hide the shared boundary automatically.</summary>
        [Test]
        public void SurfaceAutomaticEdges_HideSharedBoundary()
        {
            GameObject first = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
            GameObject second = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
            try
            {
                first.transform.position = Vector3.zero;
                second.transform.position = new Vector3(2f, 0f, 0f);
                first.GetComponent<WorldSurface>().SendMessage("OnValidate");

                Assert.That(first.transform.Find("RightEdge").gameObject.activeSelf, Is.False);
                Assert.That(second.transform.Find("LeftEdge").gameObject.activeSelf, Is.False);
                Assert.That(first.GetComponent<BoxCollider>().enabled, Is.True);
                Assert.That(second.GetComponent<BoxCollider>().enabled, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
            }
        }

        /// <summary>Verifies automatic detection preserves the exposed part of a partially covered edge.</summary>
        [Test]
        public void SurfaceAutomaticEdges_PreservePartialBoundary()
        {
            GameObject first = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
            GameObject second = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
            try
            {
                SetSurfaceSize(first, new Vector2(4f, 0.2f));
                SetSurfaceSize(second, new Vector2(2f, 0.2f));
                first.transform.position = Vector3.zero;
                second.transform.position = new Vector3(2f, 0.2f, 0f);
                first.GetComponent<WorldSurface>().SendMessage("OnValidate");

                Mesh mesh = first.transform.Find("TopEdge").GetComponent<MeshFilter>().sharedMesh;
                Assert.That(mesh.bounds.size.x, Is.EqualTo(2f));
                Assert.That(first.transform.Find("TopEdge").gameObject.activeSelf, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
            }
        }

        /// <summary>Verifies distant surfaces do not hide an otherwise exposed boundary.</summary>
        [Test]
        public void SurfaceAutomaticEdges_IgnoreDistantSurface()
        {
            GameObject surface = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
            GameObject distant = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
            try
            {
                surface.transform.position = Vector3.zero;
                distant.transform.position = new Vector3(0f, 2f, 0f);
                surface.GetComponent<WorldSurface>().SendMessage("OnValidate");

                Assert.That(surface.transform.Find("TopEdge").gameObject.activeSelf, Is.True);
                Assert.That(surface.transform.Find("TopEdge").GetComponent<MeshFilter>().sharedMesh.bounds.size.x, Is.EqualTo(2f));
            }
            finally
            {
                Object.DestroyImmediate(surface);
                Object.DestroyImmediate(distant);
            }
        }

        /// <summary>Verifies a fully covered surface hides every edge and disables collision.</summary>
        [Test]
        public void SurfaceAutomaticEdges_WhenFullyCovered_DisablesVisualAndCollision()
        {
            GameObject center = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
            GameObject top = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
            GameObject bottom = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
            GameObject left = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
            GameObject right = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
            try
            {
                center.transform.position = Vector3.zero;
                top.transform.position = new Vector3(0f, 0.2f, 0f);
                bottom.transform.position = new Vector3(0f, -0.2f, 0f);
                left.transform.position = new Vector3(-2f, 0f, 0f);
                right.transform.position = new Vector3(2f, 0f, 0f);
                center.GetComponent<WorldSurface>().SendMessage("OnValidate");

                Assert.That(center.transform.Find("TopEdge").gameObject.activeSelf, Is.False);
                Assert.That(center.transform.Find("BottomEdge").gameObject.activeSelf, Is.False);
                Assert.That(center.transform.Find("LeftEdge").gameObject.activeSelf, Is.False);
                Assert.That(center.transform.Find("RightEdge").gameObject.activeSelf, Is.False);
                Assert.That(center.GetComponent<BoxCollider>().enabled, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(center);
                Object.DestroyImmediate(top);
                Object.DestroyImmediate(bottom);
                Object.DestroyImmediate(left);
                Object.DestroyImmediate(right);
            }
        }

        /// <summary>Verifies top-left anchoring grows the surface toward the right and down.</summary>
        [Test]
        public void SurfaceTopLeftAnchor_OffsetsBodyAndCollider()
        {
            GameObject instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
            try
            {
                SerializedObject serialized = new SerializedObject(instance.GetComponent<WorldSurface>());
                serialized.FindProperty("size").vector2Value = new Vector2(4f, 2f);
                serialized.ApplyModifiedPropertiesWithoutUndo();
                instance.GetComponent<WorldSurface>().SendMessage("OnValidate");

                Assert.That(instance.transform.Find("Body").localPosition, Is.EqualTo(new Vector3(2f, -1f, 0f)));
                Assert.That(instance.GetComponent<BoxCollider>().center, Is.EqualTo(new Vector3(2f, -1f, 0f)));
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        /// <summary>Verifies focus platforms activate only at high concentration.</summary>
        [Test]
        public void FocusPlatform_ActivatesOnlyWhenFocused()
        {
            Assert.That(GetMentalPlatformState(true, MentalStateLevel.High), Is.True);
            Assert.That(GetMentalPlatformState(true, MentalStateLevel.Low), Is.False);
        }

        /// <summary>Verifies unfocus platforms activate outside high concentration.</summary>
        [Test]
        public void UnfocusPlatform_ActivatesOnlyWhenUnfocused()
        {
            Assert.That(GetMentalPlatformState(false, MentalStateLevel.High), Is.False);
            Assert.That(GetMentalPlatformState(false, MentalStateLevel.Low), Is.True);
            Assert.That(GetMentalPlatformState(false, MentalStateLevel.None), Is.True);
        }

        /// <summary>Verifies the mental platform prefab uses one 3D collider and persistent visuals.</summary>
        [Test]
        public void MentalPlatformPrefab_UsesThreeDimensionalCollision()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/Gameplay/World/MentalPlatform.prefab");

            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.GetComponent<MentalPlatform>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<DeathZone>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<BoxCollider>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<BoxCollider>().isTrigger, Is.True);
            Assert.That(prefab.GetComponentsInChildren<Collider>(true), Has.Length.EqualTo(1));
            Assert.That(prefab.GetComponentsInChildren<Collider2D>(true), Is.Empty);
            Assert.That(prefab.transform.Find("Fill"), Is.Not.Null);
            Assert.That(prefab.transform.Find("TopOutline"), Is.Not.Null);
            Assert.That(prefab.transform.Find("BottomOutline"), Is.Not.Null);
            Assert.That(prefab.transform.Find("LeftOutline"), Is.Not.Null);
            Assert.That(prefab.transform.Find("RightOutline"), Is.Not.Null);
        }

        /// <summary>Verifies mental platforms use the shared top-left anchor and collider geometry.</summary>
        [Test]
        public void MentalPlatformSize_UpdatesAnchoredColliderAndFill()
        {
            const string path = "Assets/Game/Prefabs/Gameplay/World/MentalPlatform.prefab";
            GameObject instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));
            try
            {
                SerializedObject serialized = new SerializedObject(instance.GetComponent<MentalPlatform>());
                serialized.FindProperty("size").vector2Value = new Vector2(4f, 2f);
                serialized.ApplyModifiedPropertiesWithoutUndo();
                instance.GetComponent<MentalPlatform>().SendMessage("OnValidate");

                Assert.That(instance.GetComponent<BoxCollider>().size, Is.EqualTo(new Vector3(4f, 2f, 0.2f)));
                Assert.That(instance.GetComponent<BoxCollider>().center, Is.EqualTo(new Vector3(2f, -1f, 0f)));
                Assert.That(instance.transform.Find("Fill").localPosition, Is.EqualTo(new Vector3(2f, -1f, 0f)));
                Assert.That(instance.transform.Find("Fill").localScale, Is.EqualTo(new Vector3(4f, 2f, 0.2f)));
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        /// <summary>Reads the mental platform activation decision through its collider mode.</summary>
        /// <param name="requiresFocus">Whether the platform requires concentration.</param>
        /// <param name="level">Concentration level to evaluate.</param>
        /// <returns>Whether the platform collider remains a physical surface.</returns>
        private static bool GetMentalPlatformState(bool requiresFocus, MentalStateLevel level)
        {
            const string path = "Assets/Game/Prefabs/Gameplay/World/MentalPlatform.prefab";
            GameObject instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));
            try
            {
                SerializedObject serialized = new SerializedObject(instance.GetComponent<MentalPlatform>());
                serialized.FindProperty("requiresFocus").boolValue = requiresFocus;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                instance.SendMessage("OnConcentrationChanged", level);
                return !instance.GetComponent<BoxCollider>().isTrigger;
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        /// <summary>Updates the exposed-edge mask on a temporary geometry instance.</summary>
        /// <param name="instance">Temporary geometry instance.</param>
        /// <param name="edges">Mask to assign.</param>
        private static void SetSurfaceExposedEdges(GameObject instance, WorldSurfaceEdges edges)
        {
            SerializedObject serialized = new SerializedObject(instance.GetComponent<WorldSurface>());
            serialized.FindProperty("exposedEdges").intValue = (int)edges;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            instance.GetComponent<WorldSurface>().SendMessage("OnValidate");
        }

        /// <summary>Sets whether a temporary surface calculates exposed edges automatically.</summary>
        /// <param name="instance">Temporary surface instance.</param>
        /// <param name="isAutomatic">Whether automatic edge detection is enabled.</param>
        private static void SetSurfaceAutomaticEdges(GameObject instance, bool isAutomatic)
        {
            SerializedObject serialized = new SerializedObject(instance.GetComponent<WorldSurface>());
            serialized.FindProperty("autoDetectExposedEdges").boolValue = isAutomatic;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            instance.GetComponent<WorldSurface>().SendMessage("OnValidate");
        }

        /// <summary>Sets the size of a temporary surface and applies its geometry.</summary>
        /// <param name="instance">Temporary surface instance.</param>
        /// <param name="size">Width and height to assign.</param>
        private static void SetSurfaceSize(GameObject instance, Vector2 size)
        {
            SerializedObject serialized = new SerializedObject(instance.GetComponent<WorldSurface>());
            serialized.FindProperty("size").vector2Value = size;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            instance.GetComponent<WorldSurface>().SendMessage("OnValidate");
        }
    }
}
