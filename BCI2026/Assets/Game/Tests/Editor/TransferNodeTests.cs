/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using Bit.Gameplay;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Bit.Tests.Editor
{
    /// <summary>Validates Transfer Node activation rules and critical prefab references.</summary>
    public sealed class TransferNodeTests
    {
        private const string PrefabPath = "Assets/Game/Prefabs/Gameplay/TransferNode.prefab";

        /// <summary>Verifies the prefab uses one 3D trigger and has no visual colliders.</summary>
        [Test]
        public void Prefab_UsesTriggerWithoutVisualColliders()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.GetComponent<TransferNode>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<TransferNodeVisual>(), Is.Null);
            Assert.That(prefab.GetComponent<BoxCollider>().isTrigger, Is.True);
            Assert.That(prefab.GetComponentInChildren<TransferNodeVisual>(), Is.Not.Null);
            Assert.That(prefab.GetComponentsInChildren<MeshCollider>(true), Is.Empty);
        }
    }
}
