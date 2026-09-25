using NUnit.Framework;
using UnityEngine;
namespace WildType.Tests
{
    public sealed class LineageIdentityTests
    {
        [Test] public void DefaultsAreStableBoundedAndDoNotConsumeUnityRandom()
        {
            var before = Random.state; var names = new System.Collections.Generic.HashSet<string>();
            for (int i = 1; i <= 512; i++)
            {
                var id = CreatureId.From("917430:" + i.ToString("D3")); string name = FamilyNames.DefaultName(id);
                Assert.AreEqual(name, FamilyNames.DefaultName(CreatureId.From(id.Value)));
                Assert.That(name.Length, Is.InRange(3, 16)); Assert.AreEqual(name, FamilyNames.CleanName(name)); names.Add(name);
            }
            Assert.AreEqual(before, Random.state); Assert.That(names.Count, Is.InRange(12, 24)); Assert.AreEqual("", FamilyNames.DefaultName(default));
        }
        [Test] public void RepeatedFirstNamesRemainDistinguishedByStableIds()
        {
            var seen = new System.Collections.Generic.Dictionary<string, CreatureId>(); bool duplicate = false;
            for (int i = 1; i <= 100; i++)
            {
                var id = CreatureId.From("42:" + i.ToString("D3")); string name = FamilyNames.DefaultName(id);
                if (seen.TryGetValue(name, out var earlier))
                {
                    Assert.AreEqual(FamilyNames.Format(name, 1), FamilyNames.Format(FamilyNames.DefaultName(earlier), 1));
                    Assert.AreNotEqual(GenerationLoop.ShortId(id), GenerationLoop.ShortId(earlier)); duplicate = true; break;
                }
                seen.Add(name, id);
            }
            Assert.True(duplicate);
        }
        [TestCase(-1f)] [TestCase(0f)] [TestCase(4f)] [TestCase(float.NaN)] [TestCase(float.PositiveInfinity)]
        public void PulseEndsCleanlyAndRejectsInvalidTime(float time) => Assert.AreEqual(0, DescendantLocator.Pulse(time));
        [Test] public void PulseIsSoftBoundedAndVaries()
        {
            float maximum = 0;
            for (int i = 0; i <= 400; i++) { float amount = DescendantLocator.Pulse(i * .01f); Assert.That(amount, Is.InRange(0, .381f)); maximum = Mathf.Max(maximum, amount); }
            Assert.Greater(maximum, .3f); Assert.Less(DescendantLocator.Pulse(1.2f), .001f);
        }
        [TestCase(-.2f, .5f, 1, "Left")] [TestCase(1.2f, .5f, 1, "Right")]
        [TestCase(.5f, -.2f, 1, "Below")] [TestCase(.5f, 1.2f, 1, "Above")]
        [TestCase(.8f, .5f, -1, "Behind")]
        public void OffscreenDirectionsAreExplicit(float x, float y, float z, string expected) => Assert.AreEqual(expected, FamilyWorldCues.Bearing(new Vector3(x, y, z)));
    }
}
