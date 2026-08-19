using System;
using BciGame.Core;
using BciGame.UI;
using Game.Scripts.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace BciGame.Tests.Editor
{
    public sealed class TutorialSettingsTests
    {
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
