using Functionator.Analyzer;

namespace FunctionatorTests;

internal static class Extensions
{
    internal static void AssertFunctionProperties(this Function functionToAssert, Function referenceFunction)
    {
        Assert.That(functionToAssert.Name, Is.EqualTo(referenceFunction.Name));
        Assert.That(functionToAssert.FilePath, Is.EqualTo(referenceFunction.FilePath));
        Assert.That(functionToAssert.Caller, Is.EqualTo(referenceFunction.Caller));
        Assert.That(functionToAssert.LineNumber, Is.EqualTo(referenceFunction.LineNumber));
        Assert.That(functionToAssert.TriggerTypeString, Is.EqualTo(referenceFunction.TriggerTypeString));
        Assert.That(functionToAssert.FunctionType, Is.EqualTo(referenceFunction.FunctionType));
        Assert.That(functionToAssert.ChildrenCount, Is.EqualTo(referenceFunction.ChildrenCount));
    }
}