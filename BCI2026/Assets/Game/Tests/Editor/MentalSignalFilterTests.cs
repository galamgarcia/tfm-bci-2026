using BciGame.Input;
using NUnit.Framework;

namespace BciGame.Tests.Editor
{
    public sealed class MentalSignalFilterTests
    {
        [Test]
        public void TryPublish_WaitsForConfiguredInterval()
        {
            MentalSignalFilter filter = new MentalSignalFilter(3f, 1f, 0.2f);

            Assert.That(filter.TryPublish(0.5f, 0f, out _), Is.True);
            Assert.That(filter.TryPublish(0.6f, 0.5f, out _), Is.False);
            Assert.That(filter.TryPublish(0.6f, 1f, out _), Is.True);
        }

        [Test]
        public void TryPublish_ExcludesAnIsolatedExtremeSample()
        {
            MentalSignalFilter filter = new MentalSignalFilter(3f, 1f, 0.2f);

            filter.TryPublish(0.5f, 0f, out _);
            filter.TryPublish(0.5f, 0.2f, out _);
            filter.TryPublish(0.5f, 0.4f, out _);
            filter.TryPublish(0.5f, 0.6f, out _);
            filter.TryPublish(0.5f, 0.8f, out _);

            Assert.That(filter.TryPublish(1f, 1f, out float value), Is.True);
            Assert.That(value, Is.EqualTo(0.5f).Within(0.001f));
        }
    }
}
