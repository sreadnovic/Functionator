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
            Assert.That(children.Count, Is.EqualTo(4));
        }

        [Test]
        public void Analyzer_GetChildren_ChildrenAreGeneratedAsExpected()
        {
            const string filePath = "..\\..\\..\\..\\FunctionsForTesting\\DurableFunction.cs";
            const string callerName = "DurableFunction";
            var children = _analyzer.GetChildren("DurableFunction");
            
            var referenceFunction = new Function("ActivityFunction_Hello", callerName, FunctionType.GenericActivity, "Activity", filePath, 16);
            children[0].AssertFunctionProperties(referenceFunction);

            referenceFunction = new("ActivityFunction_Hello", callerName, FunctionType.GenericActivity, "Activity", filePath, 17);
            children[1].AssertFunctionProperties(referenceFunction);
            
            referenceFunction = new("ActivityFunction_Hello", callerName, FunctionType.GenericActivity, "Activity", filePath, 18);
            children[2].AssertFunctionProperties(referenceFunction);

            referenceFunction = new("AnotherDurableFunction", callerName, FunctionType.SubOrchestrator, "Orchestration", filePath, 20){Children = new(){new(), new(), new()}};
            children[3].AssertFunctionProperties(referenceFunction);
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