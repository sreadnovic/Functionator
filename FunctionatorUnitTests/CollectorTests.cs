using Functionator.Analyzer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace FunctionatorUnitTests
{
    [TestClass]
    public class CollectorTests
    {
        private readonly Collector _collector;
        private readonly string _functionsForTestingPathPrefix = @"D:\a\Functionator\Functionator\";
        private readonly List<Function> _allFunctions;

        public CollectorTests()
        {
            #if DEBUG
            _functionsForTestingPathPrefix = @"..\..\..\";
            #endif

            _collector = Collector.GetInstance();
            _allFunctions = _collector.GetAllFunctions($"{_functionsForTestingPathPrefix}FunctionsForTesting");

        }

        [TestMethod]
        public void Colelctor_GetAllFunctions_AllThere()
        {
            Assert.AreEqual(15, _allFunctions.Count);
        }

        [TestMethod]
        public void Collector_GoodbyeDurableFunction_Ok()
        {
            var functionName = "GoodbyeDurableFunction";
            var occurences = _allFunctions.Where(x => x.Name == functionName).ToArray();

            Assert.AreEqual(3, occurences.Count());

            var definition = new Function(functionName, null, FunctionType.Caller, "Orchestration",$"{_functionsForTestingPathPrefix}FunctionsForTesting\\GoodbyeDurableFunction.cs", 12);
            occurences[0].AssertFunctionProperties(definition);

            var usage1 = new Function(functionName, "GoodbyeTriggerFunction_HttpStart", FunctionType.Orchestrator, null, $"{_functionsForTestingPathPrefix}FunctionsForTesting\\GoodbyeDurableFunction.cs", 31);
            occurences[1].AssertFunctionProperties(usage1);

            var usage2 = new Function(functionName, "GreetingsDurableFunction", FunctionType.SubOrchestrator, null, $"{_functionsForTestingPathPrefix}FunctionsForTesting\\GreetingsDurableFunction.cs", 22);
            occurences[2].AssertFunctionProperties(usage2);
        }

        [TestMethod]
        public void Collector_ActivityFunction_GoodBye_Ok()
        {
            var functionName = "ActivityFunction_GoodBye";
            var occurences = _allFunctions.Where(x => x.Name == functionName).ToArray();

            Assert.AreEqual(4, occurences.Count());

            var definition = new Function(functionName, null, FunctionType.Caller, "Activity", $"{_functionsForTestingPathPrefix}FunctionsForTesting\\GoodbyeDurableFunction.cs", 21);
            occurences[3].AssertFunctionProperties(definition);

            var usage1 = new Function(functionName, "GoodbyeDurableFunction", FunctionType.GenericActivity, null, $"{_functionsForTestingPathPrefix}FunctionsForTesting\\GoodbyeDurableFunction.cs", 16);
            occurences[0].AssertFunctionProperties(usage1);

            var usage2 = new Function(functionName, "GoodbyeDurableFunction", FunctionType.GenericActivity, null, $"{_functionsForTestingPathPrefix}FunctionsForTesting\\GoodbyeDurableFunction.cs", 17);
            occurences[1].AssertFunctionProperties(usage2);

            var usage3 = new Function(functionName, "GoodbyeDurableFunction", FunctionType.GenericActivity, null, $"{_functionsForTestingPathPrefix}FunctionsForTesting\\GoodbyeDurableFunction.cs", 18);
            occurences[2].AssertFunctionProperties(usage3);
        }

        [TestMethod]
        public void Collector_GoodbyeTriggerFunction_HttpStart_Ok()
        {
            var functionName = "GoodbyeTriggerFunction_HttpStart";
            var occurences = _allFunctions.Where(x => x.Name == functionName).ToArray();

            Assert.AreEqual(1, occurences.Count());

            var definition = new Function(functionName, null, FunctionType.Caller, "Http", $"{_functionsForTestingPathPrefix}FunctionsForTesting\\GoodbyeDurableFunction.cs", 28);
            occurences[0].AssertFunctionProperties(definition);
        }

        [TestMethod]
        public void Collector_GreetingsDurableFunction_Ok()
        {
            var functionName = "GreetingsDurableFunction";
            var occurences = _allFunctions.Where(x => x.Name == functionName).ToArray();

            Assert.AreEqual(2, occurences.Count());

            var definition = new Function(functionName, null, FunctionType.Caller, "Orchestration", $"{_functionsForTestingPathPrefix}FunctionsForTesting\\GreetingsDurableFunction.cs", 14);
            occurences[0].AssertFunctionProperties(definition);

            var usage1 = new Function(functionName, "GreetingsTriggerFunction_HttpStart", FunctionType.Orchestrator, null, $"{_functionsForTestingPathPrefix}FunctionsForTesting\\GreetingsDurableFunction.cs", 39);
            occurences[1].AssertFunctionProperties(usage1);
        }

        [TestMethod]
        public void Collector_ActivityFunction_Hello_Ok()
        {
            var functionName = "ActivityFunction_Hello";
            var occurences = _allFunctions.Where(x => x.Name == functionName).ToArray();

            Assert.AreEqual(4, occurences.Count());

            var definition = new Function(functionName, null, FunctionType.Caller, "Activity", $"{_functionsForTestingPathPrefix}FunctionsForTesting\\GreetingsDurableFunction.cs", 25);
            occurences[3].AssertFunctionProperties(definition);

            var usage1 = new Function(functionName, "GreetingsDurableFunction", FunctionType.GenericActivity, null, $"{_functionsForTestingPathPrefix}FunctionsForTesting\\GreetingsDurableFunction.cs", 18);
            occurences[0].AssertFunctionProperties(usage1);

            var usage2 = new Function(functionName, "GreetingsDurableFunction", FunctionType.GenericActivity, null, $"{_functionsForTestingPathPrefix}FunctionsForTesting\\GreetingsDurableFunction.cs", 19);
            occurences[1].AssertFunctionProperties(usage2);

            var usage3 = new Function(functionName, "GreetingsDurableFunction", FunctionType.GenericActivity, null, $"{_functionsForTestingPathPrefix}FunctionsForTesting\\GreetingsDurableFunction.cs", 20);
            occurences[2].AssertFunctionProperties(usage3);
        }

        [TestMethod]
        public void Collector_GreetingsTriggerFunction_HttpStart_Ok()
        {
            var functionName = "GreetingsTriggerFunction_HttpStart";
            var occurences = _allFunctions.Where(x => x.Name == functionName).ToArray();

            Assert.AreEqual(1, occurences.Count());

            var definition = new Function(functionName, null, FunctionType.Caller, "Http", $"{_functionsForTestingPathPrefix}FunctionsForTesting\\GreetingsDurableFunction.cs", 32);
            occurences[0].AssertFunctionProperties(definition);
        }
    }
}
