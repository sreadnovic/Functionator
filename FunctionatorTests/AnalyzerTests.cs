using Functionator.Analyzer;

namespace FunctionatorTests;

public class Tests
{
    private Analyzer _analyzer;

    [SetUp]
    public void Setup()
    {
        _analyzer = new();
        _analyzer.UpdateFunctions(@"..\..\..\..\FunctionsForTesting");
    }

    [Test]
    public void Analyzer_GetChildren_NoChildren()
    {
        var children = _analyzer.GetChildren("ActivityFunction_Hello");
        Assert.That(children.Count, Is.EqualTo(0));
    }

    [Test]
    public void Analyzer_GetChildren_HasChildren()
    {
        var children = _analyzer.GetChildren("GreetingsDurableFunction");
        Assert.That(children.Count, Is.EqualTo(4));
    }

    [Test]
    public void Analyzer_GetChildren_ChildrenAreGeneratedAsExpected()
    {
        const string filePath = "..\\..\\..\\..\\FunctionsForTesting\\GreetingsDurableFunction.cs";
        const string callerName = "GreetingsDurableFunction";
        var children = _analyzer.GetChildren("GreetingsDurableFunction");

        var referenceFunction = new Function("ActivityFunction_Hello", callerName, FunctionType.GenericActivity, "Activity", filePath, 16);
        children[0].AssertFunctionProperties(referenceFunction);

        referenceFunction = new("ActivityFunction_Hello", callerName, FunctionType.GenericActivity, "Activity", filePath, 17);
        children[1].AssertFunctionProperties(referenceFunction);

        referenceFunction = new("ActivityFunction_Hello", callerName, FunctionType.GenericActivity, "Activity", filePath, 18);
        children[2].AssertFunctionProperties(referenceFunction);

        referenceFunction = new("GoodbyeDurableFunction", callerName, FunctionType.SubOrchestrator, "Orchestration", filePath, 20) { Children = new() { new(), new(), new() } };
        children[3].AssertFunctionProperties(referenceFunction);
    }

    [Test]
    public void Analyzer_GetParents_NoParents()
    {
        var parents = _analyzer.GetParentsInverted("GreetingsTriggerFunction_HttpStart");
        Assert.That(parents.Count, Is.EqualTo(0));
    }

    [Test]
    public void Analyzer_GetParents_HasParents()
    {
        var parents = _analyzer.GetParentsInverted("GoodbyeDurableFunction");
        Assert.That(parents.Count, Is.EqualTo(2));
    }

    [Test]
    public void Analyzer_GetParents_ParentsAreGeneratedAsExpected()
    {
        const string functionName = "GoodbyeDurableFunction";

        var parents = _analyzer.GetParentsInverted(functionName);

        var referenceFunction = new Function(functionName, "GoodbyeTriggerFunction_HttpStart", FunctionType.Orchestrator, "Orchestration", "..\\..\\..\\..\\FunctionsForTesting\\GoodbyeDurableFunction.cs", 35);
        parents[0].AssertFunctionProperties(referenceFunction);

        var referenceChild = new Function(functionName, "GreetingsDurableFunction", FunctionType.SubOrchestrator, "Orchestration", "..\\..\\..\\..\\FunctionsForTesting\\GreetingsDurableFunction.cs", 20);

        referenceFunction = new("GreetingsDurableFunction", "GreetingsTriggerFunction_HttpStart", FunctionType.Orchestrator, "Orchestration", "..\\..\\..\\..\\FunctionsForTesting\\GreetingsDurableFunction.cs", 37) {Children = new(){referenceChild}};
        parents[1].AssertFunctionProperties(referenceFunction);
        parents[1].Children[0].AssertFunctionProperties(referenceChild);
    }
}