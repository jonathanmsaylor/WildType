using NUnit.Framework;
namespace WildType.Tests
{
    public sealed class FamilyNamesTests
    {
        [TestCase("Dave", 1, "Dave 1")]
        [TestCase("Dave", 2, "Dave 2")]
        [TestCase("  Mary   Jane ", 12, "Mary Jane 12")]
        [TestCase("Dave", 512, "Dave 512")]
        public void NamesUseNumericBirthOrder(string name, int order, string expected) => Assert.AreEqual(expected, FamilyNames.Format(name, order));
        [Test] public void NamesAreBoundedAndCannotInjectMarkup()
        {
            Assert.AreEqual(16, FamilyNames.CleanName(new string('A', 80)).Length);
            StringAssert.DoesNotContain("<", FamilyNames.CleanName("<color=red>Dave\n\t"));
            Assert.AreEqual("", FamilyNames.CleanName(null));
            Assert.AreEqual("O'Brien", FamilyNames.CleanName("O'Brien"));
        }
    }
}
