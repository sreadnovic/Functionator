using Functionator.Analyzer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FunctionatorUnitTests
{
    [TestClass]
    public class CollectorTests
    {
        private readonly Collector _collector;
        private readonly string _functionsForTestingPathPrefix = @"D:\a\Functionator\Functionator\";

        public CollectorTests()
        {
            #if DEBUG
            _functionsForTestingPathPrefix = @"..\..\..\";
            #endif

            _collector = Collector.GetInstance();
        }

        [TestMethod]
        public void Collector_GetAllFunctions_FoundAllFunctions()
        {
            var allFunctions = _collector.GetAllFunctions($"{_functionsForTestingPathPrefix}FunctionsForTesting");

            Assert.AreEqual(15, allFunctions.Count);
        }
    }
}
