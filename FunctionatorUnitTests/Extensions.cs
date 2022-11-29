using Functionator.Analyzer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

internal static class Extensions
{
    internal static void AssertFunctionProperties(this Function functionToAssert, Function referenceFunction)
    {
        Assert.AreEqual(referenceFunction.Name, functionToAssert.Name);
        Assert.AreEqual(referenceFunction.FilePath, functionToAssert.FilePath);
        Assert.AreEqual(referenceFunction.Caller, functionToAssert.Caller);
        Assert.AreEqual(referenceFunction.LineNumber, functionToAssert.LineNumber);
        Assert.AreEqual(referenceFunction.TriggerTypeString, functionToAssert.TriggerTypeString);
        Assert.AreEqual(referenceFunction.FunctionType, functionToAssert.FunctionType);
        Assert.AreEqual(referenceFunction.ChildrenCount, functionToAssert.ChildrenCount);
    }
}