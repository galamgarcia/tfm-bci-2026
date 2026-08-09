using System;
using BciGame.Core;
using BciGame.UI;
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
    }
}
