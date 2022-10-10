using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Functionator.Analyzer
{
    public class Function
    {
        public Function() { }

        public Function(string name, string caller, FunctionType functionType, string triggerTypeString, string filePath, int lineNumber)
        {
            Name = name;
            Caller = caller;
            FunctionType = functionType;
            TriggerTypeString = triggerTypeString;
            FilePath = filePath;
            LineNumber = lineNumber;
        }

        public string Name { get; set; }
        public string Caller { get; set; }
        public FunctionType FunctionType { get; set; }
        public string TriggerTypeString { get; set; }
        public ObservableCollection<Function> Children { get; set; }
        public ObservableCollection<Function> Parents { get; set; }
        public int ChildrenCount => Children?.Count ?? 0;
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
    }
}