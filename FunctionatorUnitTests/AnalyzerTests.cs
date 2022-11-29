using System.Collections.ObjectModel;
using System.Linq;
using Functionator.Analyzer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FunctionatorUnitTests
{
    [TestClass]
    public class AnalyzerTests
    {
        private readonly Analyzer _analyzer;

        private readonly string _functionsForTestingPathPrefix = @"D:\a\Functionator\Functionator\";
        

        public AnalyzerTests()
        {
            #if DEBUG
            _functionsForTestingPathPrefix = @"..\..\..\";
            #endif

            _analyzer = new Analyzer();
            _analyzer.UpdateFunctions($"{_functionsForTestingPathPrefix}FunctionsForTesting");
        }

        [TestMethod]
        public void Analyzer_GetChildren_NoChildren()
        {
            var children = _analyzer.GetChildrenHierarchy("ActivityFunction_Hello");
            Assert.AreEqual(0, children.Count);
        }

        [TestMethod]
        public void Analyzer_GetChildren_HasChildren()
        {
            var children = _analyzer.GetChildrenHierarchy("GreetingsDurableFunction");
            Assert.AreEqual(1, children.Count);
            Assert.AreEqual(4, children[0].Children.Count);
        }

        [TestMethod]
        public void Analyzer_GetChildren_ChildrenAreGeneratedAsExpected()
        {
            var filePath = $"{_functionsForTestingPathPrefix}FunctionsForTesting\\GreetingsDurableFunction.cs";
            const string callerName = "GreetingsDurableFunction";
            var children = _analyzer.GetChildrenHierarchy("GreetingsDurableFunction");

            var referenceFunction = new Function("ActivityFunction_Hello", callerName, FunctionType.GenericActivity, "Activity", filePath, 18);
            children[0].Children[0].AssertFunctionProperties(referenceFunction);

            referenceFunction = new Function("ActivityFunction_Hello", callerName, FunctionType.GenericActivity, "Activity", filePath, 19);
            children[0].Children[1].AssertFunctionProperties(referenceFunction);

            referenceFunction = new Function("ActivityFunction_Hello", callerName, FunctionType.GenericActivity, "Activity", filePath, 20);
            children[0].Children[2].AssertFunctionProperties(referenceFunction);

            referenceFunction = new Function("GoodbyeDurableFunction", callerName, FunctionType.SubOrchestrator, "Orchestration", filePath, 22) { Children = new ObservableCollection<Function> { new Function(), new Function(), new Function() } };
            children[0].Children[3].AssertFunctionProperties(referenceFunction);
        }

        [TestMethod]
        public void Analyzer_GetParents_NoParents()
        {
            var parents = _analyzer.GetParentsHierarchy("GreetingsTriggerFunction_HttpStart");
            Assert.AreEqual(0, parents.Count);
        }

        [TestMethod]
        public void Analyzer_GetParents_HasParents()
        {
            var parents = _analyzer.GetParentsHierarchy("ActivityFunction_Hello");
            Assert.AreEqual(1, parents.Count);
            Assert.AreEqual(1, parents[0].Children.Count);
            Assert.AreEqual(3, parents[0].Children[0].Children.Count);
        }

        [TestMethod]
        public void Analyzer_GetParents_ParentsAreGeneratedAsExpected()
        {
            const string functionName = "GoodbyeDurableFunction";

            var parents = _analyzer.GetParentsHierarchy(functionName);

            var referenceFunction = new Function(functionName, "GoodbyeTriggerFunction_HttpStart", FunctionType.Orchestrator, "Orchestration", $"{_functionsForTestingPathPrefix}FunctionsForTesting\\GoodbyeDurableFunction.cs", 31);
            parents[0].Children[0].AssertFunctionProperties(referenceFunction);

            var referenceChild = new Function(functionName, "GreetingsDurableFunction", FunctionType.SubOrchestrator, "Orchestration", $"{_functionsForTestingPathPrefix}FunctionsForTesting\\GreetingsDurableFunction.cs", 22);

            referenceFunction = new Function("GreetingsDurableFunction", "GreetingsTriggerFunction_HttpStart", FunctionType.Orchestrator, "Orchestration", $"{_functionsForTestingPathPrefix}FunctionsForTesting\\GreetingsDurableFunction.cs", 39) { Children = new ObservableCollection<Function> { referenceChild } };
            parents[1].Children[0].AssertFunctionProperties(referenceFunction);
            parents[1].Children[0].Children[0].AssertFunctionProperties(referenceChild);
        }
        
        // TODO: Process all use cases of function calls e.g. StartNewAsync with/without additional params
    }
}