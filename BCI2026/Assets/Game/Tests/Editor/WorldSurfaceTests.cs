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
    /// <summary>Validates World Surface prefab structure and editor synchronization.</summary>
    public sealed class WorldSurfaceTests
    {
        private const string PrefabPath = "Assets/Game/Prefabs/World/WorldSurface.prefab";

        /// <summary>Verifies World Surface uses static 3D collision without 2D components.</summary>
        [Test]
        public void Prefab_UsesStaticThreeDimensionalCollision()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);

            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.GetComponent<WorldSurface>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<BoxCollider>().isTrigger, Is.False);
            Assert.That(prefab.GetComponent<Rigidbody>(), Is.Null);
            Assert.That(prefab.GetComponentsInChildren<Collider>(true), Has.Length.EqualTo(1));
        }

        /// <summary>Verifies None disables collision and every exposed edge renderer.</summary>
        [Test]
        public void None_DisablesCollisionAndEdges()
        {
            GameObject instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
            try
            {
                SetExposedEdges(instance, WorldSurfaceEdges.None);

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
        public void Size_UpdatesPhysicalAndVisualDimensions()
        {
            GameObject instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
            try
            {
                SerializedObject serialized = new SerializedObject(instance.GetComponent<WorldSurface>());
                serialized.FindProperty("size").vector2Value = new Vector2(4f, 2f);
                serialized.ApplyModifiedPropertiesWithoutUndo();
                instance.GetComponent<WorldSurface>().SendMessage("OnValidate");

                Assert.That(instance.GetComponent<BoxCollider>().size, Is.EqualTo(new Vector3(4f, 2f, 0.2f)));
                Assert.That(instance.transform.Find("Body").localScale, Is.EqualTo(new Vector3(4f, 2f, 0.2f)));
                Assert.That(instance.transform.Find("TopEdge").localScale.x, Is.EqualTo(4f));
                Assert.That(instance.transform.Find("LeftEdge").localScale.y, Is.EqualTo(2f));
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        /// <summary>Updates the exposed-edge mask on a temporary geometry instance.</summary>
        /// <param name="instance">Temporary geometry instance.</param>
        /// <param name="edges">Mask to assign.</param>
        private static void SetExposedEdges(GameObject instance, WorldSurfaceEdges edges)
        {
            SerializedObject serialized = new SerializedObject(instance.GetComponent<WorldSurface>());
            serialized.FindProperty("exposedEdges").intValue = (int)edges;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            instance.GetComponent<WorldSurface>().SendMessage("OnValidate");
        }
    }
}
