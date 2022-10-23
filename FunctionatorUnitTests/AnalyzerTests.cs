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
            Assert.AreEqual(children.Count, 0);
        }

        [TestMethod]
        public void Analyzer_GetChildren_HasChildren()
        {
            var children = _analyzer.GetChildrenHierarchy("GreetingsDurableFunction");
            Assert.AreEqual(children.Count, 1);
            Assert.AreEqual(children[0].Children.Count, 4);
        }

        [TestMethod]
        public void Analyzer_GetChildren_ChildrenAreGeneratedAsExpected()
        {
            var filePath = $"{_functionsForTestingPathPrefix}FunctionsForTesting\\GreetingsDurableFunction.cs";
            const string callerName = "GreetingsDurableFunction";
            var children = _analyzer.GetChildrenHierarchy("GreetingsDurableFunction");

            var referenceFunction = new Function("ActivityFunction_Hello", callerName, FunctionType.GenericActivity, "Activity", filePath, 16);
            children[0].Children[0].AssertFunctionProperties(referenceFunction);

            referenceFunction = new Function("ActivityFunction_Hello", callerName, FunctionType.GenericActivity, "Activity", filePath, 17);
            children[0].Children[1].AssertFunctionProperties(referenceFunction);

            referenceFunction = new Function("ActivityFunction_Hello", callerName, FunctionType.GenericActivity, "Activity", filePath, 18);
            children[0].Children[2].AssertFunctionProperties(referenceFunction);

            referenceFunction = new Function("GoodbyeDurableFunction", callerName, FunctionType.SubOrchestrator, "Orchestration", filePath, 20) { Children = new ObservableCollection<Function> { new Function(), new Function(), new Function() } };
            children[0].Children[3].AssertFunctionProperties(referenceFunction);
        }

        [TestMethod]
        public void Analyzer_GetParents_NoParents()
        {
            var parents = _analyzer.GetParentsHierarchy("GreetingsTriggerFunction_HttpStart");
            Assert.AreEqual(parents.Count, 0);
        }

        [TestMethod]
        public void Analyzer_GetParents_HasParents()
        {
            var parents = _analyzer.GetParentsHierarchy("GoodbyeDurableFunction");
            Assert.AreEqual(parents.Count, 2);
        }

        [TestMethod]
        public void Analyzer_GetParents_ParentsAreGeneratedAsExpected()
        {
            const string functionName = "GoodbyeDurableFunction";

            var parents = _analyzer.GetParentsHierarchy(functionName);

            var referenceFunction = new Function(functionName, "GoodbyeTriggerFunction_HttpStart", FunctionType.Orchestrator, "Orchestration", $"{_functionsForTestingPathPrefix}FunctionsForTesting\\GoodbyeDurableFunction.cs", 35);
            parents[0].Children[0].AssertFunctionProperties(referenceFunction);

            var referenceChild = new Function(functionName, "GreetingsDurableFunction", FunctionType.SubOrchestrator, "Orchestration", $"{_functionsForTestingPathPrefix}FunctionsForTesting\\GreetingsDurableFunction.cs", 20);

            referenceFunction = new Function("GreetingsDurableFunction", "GreetingsTriggerFunction_HttpStart", FunctionType.Orchestrator, "Orchestration", $"{_functionsForTestingPathPrefix}FunctionsForTesting\\GreetingsDurableFunction.cs", 37) { Children = new ObservableCollection<Function> { referenceChild } };
            parents[1].Children[0].AssertFunctionProperties(referenceFunction);
            parents[1].Children[0].Children[0].AssertFunctionProperties(referenceChild);
        }

        // TODO: All function attributes in one line
        // TODO: Use constant for func name
        // TODO: Mix string with nameof
        // TODO: Process all use cases of function calls e.g. StartNewAsync with/without additional params
    }
}