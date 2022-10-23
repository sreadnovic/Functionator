using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Functionator.Analyzer
{
    public class Analyzer
    {
        private const string FunctionAttribute = "[FunctionName(";
        private const string FunctionAttributeInterpolated = "[FunctionName($\"";
        private const string StartNewAsyncCall = "StartNewAsync(";
        private const string GenericActivityAsyncCall = "CallActivityAsync<";
        private const string RegularActivityAsyncCall = "CallActivityAsync(";
        private const string GenericSubOrchestratorAsyncCall = "CallSubOrchestratorAsync<";
        private const string RegularSubOrchestratorAsyncCall = "CallSubOrchestratorAsync(";
        private const string FunctionCallStartString = ">(";
        private const string FunctionCallEndString = ",";
        private const string FunctionCallEndStringNoParameters = ");";
        private const string FunctionAttributeEndString = ")]";
        private const string TriggerAttribute = "Trigger]";
        private const string TriggerAttributeWithParam = "Trigger(";

        private List<Function> _functions;

        public void UpdateFunctions(string projectPath)
        {
            _functions = GetAllFunctions(projectPath);

            foreach (var item in _functions.Where(item => string.IsNullOrEmpty(item.TriggerTypeString)))
            {
                item.TriggerTypeString = GetFunctionTriggerType(item.Name);
            }
        }

        private List<Function> GetAllFunctions(string location) =>
            Directory.GetFiles(location, "*.cs", SearchOption.AllDirectories).Select(GetFileFunctions).SelectMany(x => x).ToList();

        internal Function GetFunctionDefinition(Function function) =>
            _functions.FirstOrDefault(x => x.Name == function.Name && x.FunctionType == FunctionType.Caller);

        public ObservableCollection<Function> GetChildrenHierarchy(string functionName)
        {
            var res = new ObservableCollection<Function>();

            var children = GetChildren(functionName);

            if (children != null && children.Any())
            {
                res = new () { new (children.First().Caller, null, default, GetFunctionTriggerType(children.First().Caller), children.First().FilePath, children.First().LineNumber) { Children = children } };
            }

            return res;
        }

        private ObservableCollection<Function> GetChildren(string functionName)
        {
            return new(_functions.Where(x => x.Caller == functionName)
                .Select(x => new Function(x.Name, x.Caller, x.FunctionType, x.TriggerTypeString, x.FilePath, x.LineNumber) { Children = GetChildren(x.Name) }));
        }

        private ObservableCollection<Function> GetChildren(string functionName, List<Function> functions)
        {
            return new(functions.Where(x => x.Caller == functionName)
                .Select(x => new Function(x.Name, x.Caller, x.FunctionType, x.TriggerTypeString, x.FilePath, x.LineNumber) { Children = GetChildren(x.Name, functions) }));
        }

        private ObservableCollection<Function> GetParents(string functionName)
        {
            return new(_functions.Where(x => x.Name == functionName && !string.IsNullOrEmpty(x.Caller))
                .Select(x => new Function(x.Name, x.Caller, x.FunctionType, x.TriggerTypeString, x.FilePath, x.LineNumber) { Parents = GetParents(x.Caller) }));
        }

        public ObservableCollection<Function> GetParentsHierarchy(string functionName)
        {
            var parents = GetParents(functionName);
            List<Function> disassembledParentsHierarchy = default;
            DisassembleHierarchy(parents, ref disassembledParentsHierarchy);

            var topmostParents = GetTopmostParents(parents).Distinct().ToList();

            var parentsInverted = new List<Function>();

            foreach (var parent in topmostParents)
            {
                parentsInverted.AddRange(GetChildren(parent.Name, disassembledParentsHierarchy));
            }
            
            var res = new ObservableCollection<Function>();

            foreach (var item in parentsInverted)
            {
                res.Add(new(item.Caller, null, default, GetFunctionTriggerType(item.Caller), item.FilePath, item.LineNumber) { Children = new() { item } });
            }

            return res;
        }

        private IEnumerable<Function> GetTopmostParents(IEnumerable<Function> hierarchy)
        {
            foreach (var function in hierarchy)
            {
                if (!function.Parents.Any()) yield return _functions.FirstOrDefault(x => x.Name == function.Caller);

                foreach (var item in GetTopmostParents(function.Parents.ToList()))
                {
                    yield return item;
                }
            }
        }
        
        private void DisassembleHierarchy(IEnumerable<Function> hierarchy, ref List<Function> res)
        {
            res ??= new();

            if (hierarchy == null) return;

            foreach (var item in hierarchy)
            {
                DisassembleHierarchy(item.Parents, ref res);
                res.Add(item);
            }
        }
        
        private string GetFunctionTriggerType(string functionName) =>
            _functions.FirstOrDefault(x => x.Name == functionName && !string.IsNullOrEmpty(x.TriggerTypeString))?.TriggerTypeString;
        
        private List<Function> GetFileFunctions(string file)
        {
            var fileFunctions = new List<Function>();
            string completeLineOfCode = default;

            string callerFunctionName = default;
            var lineNumber = 0;
            foreach (var line in File.ReadAllLines(file))
            {
                lineNumber++;
                var trimmedLine = line.Trim();

                if (string.IsNullOrEmpty(trimmedLine)) continue;

                Function func;
                
                if (GetFunctionType(trimmedLine) == FunctionType.Caller)
                {
                    completeLineOfCode += trimmedLine;
                    func = GetFunction(completeLineOfCode, default, file, lineNumber);
                    callerFunctionName = func.Name;

                    fileFunctions.Add(func);
                    completeLineOfCode = default;
                }
                else if (trimmedLine.Contains(TriggerAttribute) || trimmedLine.Contains(TriggerAttributeWithParam))
                {
                    var triggerType = GetCallerFunctionTriggerType(trimmedLine);
                    fileFunctions.Last(x => x.FunctionType == FunctionType.Caller).TriggerTypeString = triggerType;
                }
                else
                {
                    if (!IsValidLine(trimmedLine) && string.IsNullOrEmpty(completeLineOfCode)) continue;

                    completeLineOfCode += trimmedLine;

                    if (!completeLineOfCode.EndsWith(";")) continue;

                    func = GetFunction(completeLineOfCode, callerFunctionName, file, lineNumber);

                    fileFunctions.Add(func);
                    completeLineOfCode = default;
                }
            }

            return fileFunctions;
        }

        private Function GetFunction(string completeLineOfCode, string callerFunctionName, string filePath, int lineNumber)
        {
            var functionType = GetFunctionType(completeLineOfCode);
            var wrappers = GetFunctionStringWrappers(functionType);
            
            string functionName = null;

            foreach (var wrapper in wrappers)
            {
                functionName = GetTextInBetween(completeLineOfCode, wrapper.start, wrapper.end);
                if (functionName != null) break;
            }

            const string nameOf = "nameof(";

            functionName = functionName!.Contains(nameOf) ? GetTextInBetween(functionName, nameOf, ")") : functionName;
            
            return new (functionName, callerFunctionName, functionType, default, filePath, lineNumber);
        }

        private string GetTextInBetween(string fullText, string start, string end)
        {
            var startIndex = fullText.IndexOf(start, StringComparison.Ordinal) + start.Length;
            var endIndex = fullText.IndexOf(end, StringComparison.Ordinal);

            if (startIndex == -1 || endIndex == -1) return default;

            return fullText.Substring(startIndex, endIndex - startIndex).Replace("\"", string.Empty);
        }

        private string GetCallerFunctionTriggerType(string completeLineOfCode)
        {
            var triggerAttribute = completeLineOfCode.Contains(TriggerAttribute)
                ? TriggerAttribute
                : TriggerAttributeWithParam;

            return GetTextInBetween(completeLineOfCode, "[", triggerAttribute);
        }

        private IEnumerable<(string start, string end)> GetFunctionStringWrappers(FunctionType functionType) =>
            functionType switch
            {
                FunctionType.Caller => new() { (FunctionAttribute, FunctionAttributeEndString) } ,
                FunctionType.Orchestrator => new() { (StartNewAsyncCall, FunctionCallEndString), (StartNewAsyncCall, FunctionCallEndStringNoParameters) },
                FunctionType.Activity => new() { (RegularActivityAsyncCall, FunctionCallEndString) },
                FunctionType.GenericActivity => new() { (FunctionCallStartString, FunctionCallEndString) },
                FunctionType.SubOrchestrator => new() { (RegularSubOrchestratorAsyncCall, FunctionCallEndString) },
                FunctionType.GenericSubOrchestrator => new() { (FunctionCallStartString, FunctionCallEndString) },
                _ => new List<(string, string)> { (string.Empty, string.Empty)}
            };

        private FunctionType GetFunctionType(string completeLineOfCode) =>
            completeLineOfCode switch
            {
                _ when completeLineOfCode.Contains(FunctionAttribute) => FunctionType.Caller,
                _ when completeLineOfCode.Contains(FunctionAttributeInterpolated) => FunctionType.Caller,
                _ when completeLineOfCode.Contains(StartNewAsyncCall) => FunctionType.Orchestrator,
                _ when completeLineOfCode.Contains(GenericActivityAsyncCall) => FunctionType.GenericActivity,
                _ when completeLineOfCode.Contains(RegularActivityAsyncCall) => FunctionType.Activity,
                _ when completeLineOfCode.Contains(GenericSubOrchestratorAsyncCall) => FunctionType.GenericSubOrchestrator,
                _ when completeLineOfCode.Contains(RegularSubOrchestratorAsyncCall) => FunctionType.SubOrchestrator,
                _ => FunctionType.Unknown
            };

        private static bool IsValidLine(string line) =>
            line.Contains(FunctionAttribute)
            || line.Contains(StartNewAsyncCall)
            || line.Contains(GenericActivityAsyncCall)
            || line.Contains(RegularActivityAsyncCall)
            || line.Contains(GenericSubOrchestratorAsyncCall)
            || line.Contains(RegularSubOrchestratorAsyncCall);
    }
}