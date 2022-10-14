using System.Collections.ObjectModel;
using Functionator.Analyzer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FunctionatorUnitTests
{
    [TestClass]
    public class AnalyzerTests
    {
        private readonly Analyzer _analyzer;

        public AnalyzerTests()
        {
            _analyzer = new Analyzer();
            _analyzer.UpdateFunctions(@"..\..\..\FunctionsForTesting");
        }

        [TestMethod]
        public void Analyzer_GetChildren_NoChildren()
        {
            var children = _analyzer.GetChildren("ActivityFunction_Hello");
            Assert.AreEqual(children.Count, 0);
        }

        [TestMethod]
        public void Analyzer_GetChildren_HasChildren()
        {
            var children = _analyzer.GetChildren("GreetingsDurableFunction");
            Assert.AreEqual(children.Count, 4);
        }

        [TestMethod]
        public void Analyzer_GetChildren_ChildrenAreGeneratedAsExpected()
        {
            const string filePath = "..\\..\\..\\FunctionsForTesting\\GreetingsDurableFunction.cs";
            const string callerName = "GreetingsDurableFunction";
            var children = _analyzer.GetChildren("GreetingsDurableFunction");

            var referenceFunction = new Function("ActivityFunction_Hello", callerName, FunctionType.GenericActivity, "Activity", filePath, 16);
            children[0].AssertFunctionProperties(referenceFunction);

            referenceFunction = new Function("ActivityFunction_Hello", callerName, FunctionType.GenericActivity, "Activity", filePath, 17);
            children[1].AssertFunctionProperties(referenceFunction);

            referenceFunction = new Function("ActivityFunction_Hello", callerName, FunctionType.GenericActivity, "Activity", filePath, 18);
            children[2].AssertFunctionProperties(referenceFunction);

            referenceFunction = new Function("GoodbyeDurableFunction", callerName, FunctionType.SubOrchestrator, "Orchestration", filePath, 20) { Children = new ObservableCollection<Function> { new Function(), new Function(), new Function() } };
            children[3].AssertFunctionProperties(referenceFunction);
        }

        [TestMethod]
        public void Analyzer_GetParents_NoParents()
        {
            var parents = _analyzer.GetParentsInverted("GreetingsTriggerFunction_HttpStart");
            Assert.AreEqual(parents.Count, 0);
        }

        [TestMethod]
        public void Analyzer_GetParents_HasParents()
        {
            var parents = _analyzer.GetParentsInverted("GoodbyeDurableFunction");
            Assert.AreEqual(parents.Count, 2);
        }

        [TestMethod]
        public void Analyzer_GetParents_ParentsAreGeneratedAsExpected()
        {
            const string functionName = "GoodbyeDurableFunction";

            var parents = _analyzer.GetParentsInverted(functionName);

            var referenceFunction = new Function(functionName, "GoodbyeTriggerFunction_HttpStart", FunctionType.Orchestrator, "Orchestration", "..\\..\\..\\FunctionsForTesting\\GoodbyeDurableFunction.cs", 35);
            parents[0].AssertFunctionProperties(referenceFunction);

            var referenceChild = new Function(functionName, "GreetingsDurableFunction", FunctionType.SubOrchestrator, "Orchestration", "..\\..\\..\\FunctionsForTesting\\GreetingsDurableFunction.cs", 20);

            referenceFunction = new Function("GreetingsDurableFunction", "GreetingsTriggerFunction_HttpStart", FunctionType.Orchestrator, "Orchestration", "..\\..\\..\\FunctionsForTesting\\GreetingsDurableFunction.cs", 37) { Children = new ObservableCollection<Function> { referenceChild } };
            parents[1].AssertFunctionProperties(referenceFunction);
            parents[1].Children[0].AssertFunctionProperties(referenceChild);
        }
    }
}