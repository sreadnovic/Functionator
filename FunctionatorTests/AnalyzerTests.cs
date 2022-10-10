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
            var filePath = "..\\..\\..\\..\\FunctionsForTesting\\DurableFunction.cs";
            var callerName = "DurableFunction";
            var children = _analyzer.GetChildren("DurableFunction");

            Assert.That(children.Count, Is.EqualTo(4));

            Assert.That(children[0].Name, Is.EqualTo("ActivityFunction_Hello"));
            Assert.That(children[0].FilePath, Is.EqualTo(filePath));
            Assert.That(children[0].Caller, Is.EqualTo(callerName));
            Assert.That(children[0].LineNumber, Is.EqualTo(16));
            Assert.That(children[0].TriggerTypeString, Is.EqualTo("Activity"));
            Assert.That(children[0].FunctionType, Is.EqualTo(FunctionType.GenericActivity));
            Assert.That(children[0].ChildrenCount, Is.EqualTo(0));

            Assert.That(children[1].Name, Is.EqualTo("ActivityFunction_Hello"));
            Assert.That(children[1].FilePath, Is.EqualTo(filePath));
            Assert.That(children[1].Caller, Is.EqualTo(callerName));
            Assert.That(children[1].LineNumber, Is.EqualTo(17));
            Assert.That(children[1].TriggerTypeString, Is.EqualTo("Activity"));
            Assert.That(children[1].FunctionType, Is.EqualTo(FunctionType.GenericActivity));
            Assert.That(children[1].ChildrenCount, Is.EqualTo(0));

            Assert.That(children[2].Name, Is.EqualTo("ActivityFunction_Hello"));
            Assert.That(children[2].FilePath, Is.EqualTo(filePath));
            Assert.That(children[2].Caller, Is.EqualTo(callerName));
            Assert.That(children[2].LineNumber, Is.EqualTo(18));
            Assert.That(children[2].TriggerTypeString, Is.EqualTo("Activity"));
            Assert.That(children[2].FunctionType, Is.EqualTo(FunctionType.GenericActivity));
            Assert.That(children[2].ChildrenCount, Is.EqualTo(0));

            Assert.That(children[3].Name, Is.EqualTo("AnotherDurableFunction"));
            Assert.That(children[3].FilePath, Is.EqualTo(filePath));
            Assert.That(children[3].Caller, Is.EqualTo(callerName));
            Assert.That(children[3].LineNumber, Is.EqualTo(20));
            Assert.That(children[3].TriggerTypeString, Is.EqualTo("Orchestration"));
            Assert.That(children[3].FunctionType, Is.EqualTo(FunctionType.SubOrchestrator));
            Assert.That(children[3].ChildrenCount, Is.EqualTo(3));
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

        [Test]
        public void Analyzer_GetParentsGetChildren_HasParentsHasChildren()
        {
            var functionName = "AnotherDurableFunction";

            var parents = _analyzer.GetParentsInverted(functionName);
            var children = _analyzer.GetChildren(functionName);

            Assert.That(parents.Count, Is.EqualTo(1));
            Assert.That(children.Count, Is.EqualTo(3));
        }
    }
}