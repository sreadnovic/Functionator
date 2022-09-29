using Functionator.Analyzer;

namespace FunctionatorTests
{
    public class Tests
    {
        private Analyzer _analyzer;

        [SetUp]
        public void Setup()
        {
            _analyzer = new ();
            _analyzer.UpdateFunctions(@"..\..\..\..\FunctionsForTesting");
        }

        [Test]
        public void Analyzer_GetChildren_NoChildren()
        {
            var children = _analyzer.GetChildren("DurableFunction_Hello");
            Assert.That(children.Count, Is.EqualTo(0));
        }

        [Test]
        public void Analyzer_GetChildren_HasChildren()
        {
            var children = _analyzer.GetChildren("DurableFunction");
            Assert.That(children.Count, Is.EqualTo(3));
        }

        [Test]
        public void Analyzer_GetParents_NoParents()
        {
            var parents = _analyzer.GetParentsInverted("DurableFunction_HttpStart");
            Assert.That(parents.Count, Is.EqualTo(0));
        }

        [Test]
        public void Analyzer_GetParents_HasParents()
        {
            var parents = _analyzer.GetParentsInverted("DurableFunction");
            Assert.That(parents.Count, Is.EqualTo(1));
        }
    }
}