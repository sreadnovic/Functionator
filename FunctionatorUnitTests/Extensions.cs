using Functionator.Analyzer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

internal static class Extensions
{
    internal static void AssertFunctionProperties(this Function functionToAssert, Function referenceFunction)
    {
        Assert.AreEqual(functionToAssert.Name, referenceFunction.Name);
        Assert.AreEqual(functionToAssert.FilePath, referenceFunction.FilePath);
        Assert.AreEqual(functionToAssert.Caller, referenceFunction.Caller);
        Assert.AreEqual(functionToAssert.LineNumber, referenceFunction.LineNumber);
        Assert.AreEqual(functionToAssert.TriggerTypeString, referenceFunction.TriggerTypeString);
        Assert.AreEqual(functionToAssert.FunctionType, referenceFunction.FunctionType);
        Assert.AreEqual(functionToAssert.ChildrenCount, referenceFunction.ChildrenCount);
    }
}