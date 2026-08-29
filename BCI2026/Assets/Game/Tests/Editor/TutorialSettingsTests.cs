/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System;
using BciGame.Core;
using BciGame.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace BciGame.Tests.Editor
{
    /// <summary>Validates configured tutorial text and feedback colors.</summary>
    public sealed class TutorialSettingsTests
    {
        /// <summary>Verifies that every tutorial text identifier has configured content.</summary>
        [Test]
        public void GetText_ReturnsContentForEveryConfiguredTextId()
        {
            TutorialSettings settings = Resources.Load<TutorialSettings>("TutorialSettings");
            Assert.That(settings, Is.Not.Null, "TutorialSettings must be available in Resources.");
            foreach (TutorialTextId id in Enum.GetValues(typeof(TutorialTextId)))
            {
                if (id == TutorialTextId.None) { continue; }
                Assert.That(settings.GetText(id), Is.Not.Empty, $"Missing text for {id}.");
            }
        }

        /// <summary>Verifies that every mental-state level has its expected feedback color.</summary>
        [Test]
        public void GetColor_ReturnsColorForEveryMentalStateLevel()
        {
            TutorialSettings settings = Resources.Load<TutorialSettings>("TutorialSettings");
            Assert.That(settings, Is.Not.Null, "TutorialSettings must be available in Resources.");
            Assert.That(settings.GetColor(MentalStateLevel.None), Is.EqualTo(Color.white));
            Assert.That(settings.GetColor(MentalStateLevel.Low), Is.EqualTo(new Color(0.04f, 0.52f, 1f)));
            Assert.That(settings.GetColor(MentalStateLevel.Medium), Is.EqualTo(new Color(0.86f, 0.24f, 0.24f)));
            Assert.That(settings.GetColor(MentalStateLevel.High), Is.EqualTo(new Color(0.86f, 0.24f, 0.24f)));
        }
    }
}
