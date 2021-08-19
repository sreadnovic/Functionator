using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Functionator.Analyzer
{
    internal class Analyzer
    {
        private const string FunctionAttribute = "[FunctionName(\"";
        private const string StartNewAsyncCall = "StartNewAsync(\"";
        private const string GenericActivityAsyncCall = "CallActivityAsync<";
        private const string RegularActivityAsyncCall = "CallActivityAsync(\"";
        private const string GenericSubOrchestratorAsyncCall = "CallSubOrchestratorAsync<";
        private const string RegularSubOrchestratorAsyncCall = "CallSubOrchestratorAsync(\"";
        private const string FunctionCallStartString = ">(\"";
        private const string FunctionCallEndString = "\",";
        private const string FunctionAttributeEndString = "\")]";
        private const string TriggerAttribute = "Trigger]";
        private const string TriggerAttributeWithParam = "Trigger(";

        private List<Function> _functions;

        public Analyzer()
        {
            _functions = GetFunctions();
        }

        private List<Function> GetFunctions()
        {
            var allFiles = Directory
                .GetFiles("c:\\Users\\JovanSredanovic\\source\\repos\\i4SEE\\", "*", SearchOption.AllDirectories)
                .Where(x => x.EndsWith(".cs")).ToList();
            Console.WriteLine(allFiles.Count());
            
            var functions = new List<Function>();

            Function callerFunction = null;

            var completeLineOfCode = string.Empty;

            foreach (var file in allFiles)
            {
                foreach (var line in File.ReadAllLines(file))
                {
                    var trimmedLine = line.Trim();

                    if (!IsValidLine(trimmedLine) && string.IsNullOrEmpty(completeLineOfCode))
                    {
                        continue;
                    }

                    completeLineOfCode += trimmedLine;

                    if (!completeLineOfCode.EndsWith(";"))
                    {
                        continue;
                    }

                    var functionType = GetFunctionType(completeLineOfCode);

                    if (functionType == FunctionType.Caller)
                    {
                        callerFunction = GetCallerFunction(completeLineOfCode);
                        functions.Add(callerFunction);
                    } else
                    {
                        functions.Add(GetCalledFunction(completeLineOfCode, callerFunction.Name, functionType));
                    }

                    completeLineOfCode = string.Empty;
                }
            }

            return functions;
        }

        private static Function GetCallerFunction(string completeLineOfCode)
        {
            var functionName = completeLineOfCode.Substring(FunctionAttribute.Length,
                completeLineOfCode.IndexOf(FunctionAttributeEndString, StringComparison.Ordinal) - FunctionAttribute.Length);

            var triggerType = GetCallerFunctionTriggerType(completeLineOfCode);
            
            return new Function(functionName, null, FunctionType.Caller, triggerType);
        }

        private static string GetCallerFunctionTriggerType(string completeLineOfCode)
        {
            var triggerAttribute = completeLineOfCode.Contains(TriggerAttribute)
                ? TriggerAttribute
                : TriggerAttributeWithParam;

            var triggerNameStart = completeLineOfCode.LastIndexOf('[',
                completeLineOfCode.IndexOf(triggerAttribute, StringComparison.Ordinal)) + 1;

            var triggerNameEnd = completeLineOfCode.IndexOf(triggerAttribute, StringComparison.Ordinal);
            return completeLineOfCode.Substring(triggerNameStart, triggerNameEnd - triggerNameStart);
        }

        private static Function GetCalledFunction(string completeLineOfCode, string caller, FunctionType functionType)
        {
            var (start, end) = GetFunctionStringWrappers(functionType);

            var functionNameStart = completeLineOfCode.IndexOf(start, StringComparison.Ordinal) + start.Length;
            var functionNameEnd = completeLineOfCode.IndexOf(end, StringComparison.Ordinal) -
                                  (completeLineOfCode.IndexOf(start, StringComparison.Ordinal) + start.Length);

            var functionName = completeLineOfCode.Substring(functionNameStart, functionNameEnd);

            return new Function(functionName, caller, functionType, functionType.ToString());
        }

        private static (string start, string end) GetFunctionStringWrappers(FunctionType functionType)
        {
            if (functionType == FunctionType.Orchestrator)
            {
                return (StartNewAsyncCall, FunctionCallEndString);
            }
            else if (functionType == FunctionType.Activity)
            {
                return (RegularActivityAsyncCall, FunctionCallEndString);
            } 
            else if (functionType == FunctionType.GenericActivity)
            {
                return (FunctionCallStartString, FunctionCallEndString);
            }
            else if (functionType == FunctionType.SubOrchestrator)
            {
                return (RegularSubOrchestratorAsyncCall, FunctionCallEndString);
            }
            else if (functionType == FunctionType.GenericSubOrchestrator)
            {
                return (FunctionCallStartString, FunctionCallEndString);
            }

            return (string.Empty, string.Empty);
        }

        private static FunctionType GetFunctionType(string completeLineOfCode)
        {
            if (completeLineOfCode.Contains(FunctionAttribute))
            {
                return FunctionType.Caller;
            } else if (completeLineOfCode.Contains(StartNewAsyncCall))
            {
                return FunctionType.Orchestrator;
            }
            else if (completeLineOfCode.Contains(GenericActivityAsyncCall))
            {
                return FunctionType.GenericActivity;
            }
            else if (completeLineOfCode.Contains(RegularActivityAsyncCall))
            {
                return FunctionType.Activity;
            }
            else if (completeLineOfCode.Contains(GenericSubOrchestratorAsyncCall))
            {
                return FunctionType.GenericSubOrchestrator;
            }
            else if (completeLineOfCode.Contains(RegularSubOrchestratorAsyncCall))
            {
                return FunctionType.SubOrchestrator;
            }
            
            return FunctionType.Unknown;
        }

        internal IEnumerable<IEnumerable<string>> GetAllChildrenCombinations(string functionName)
        {
            var res = new List<List<string>>();

            var children = new List<Function>();
            GetChildren(functionName, _functions, ref children);

            foreach (var r in children.Where(x => x.Name.All(char.IsLetterOrDigit)/* && x.IsOnBottom*/))
            {
                r.IsOnBottom = children.FirstOrDefault(x => x.Caller == r.Name) == null;

                if (!r.IsOnBottom) continue;

                foreach (var combination in GetAllParentsCombinations(r.Name, children))
                {
                    res.Add(combination.ToList());
                }
            }

            return res;
        }

        internal IEnumerable<IEnumerable<string>> GetAllParentsCombinations(string functionName, List<Function> functions = null)
        {
            if (functions == null || !functions.Any())
            {
                functions = _functions;
            }

            var parents = new List<Function>();
            GetParents(functionName, ref parents, functions);

            parents.Reverse();

            var startingIndexes = new List<int>();

            foreach (var r in parents)
            {
                r.IsOnTop = parents.FirstOrDefault(x => x.Name == r.Caller) == null;
                if (r.IsOnTop)
                {
                    startingIndexes.Add(parents.IndexOf(r));
                }
            }
            
            foreach (var startingIndex in startingIndexes)
            {
                var combination = new List<string>();

                Function previous = null;

                foreach (var r in parents.Skip(startingIndex))
                {
                    if (startingIndex != parents.IndexOf(r) && r.IsOnTop) continue;

                    if (previous == null)
                    {
                        combination.Add(r.Caller);
                        combination.Add(r.Name);
                    }
                    else
                    {
                        combination.Add(r.Name != previous.Name ? r.Name : r.Caller);
                    }

                    previous = r;
                }

                yield return combination;
            }
        }

        private void GetChildren(string functionName, IEnumerable<Function> functions, ref List<Function> res)
        {
            if (res == null || !res.Any()) res = new List<Function>();

            var children = functions.Where(x => x.Caller == functionName);

            res.AddRange(children);

            foreach (var child in children) GetChildren(child.Name, functions, ref res);
        }

        private void GetParents(string functionName, ref List<Function> res, IEnumerable<Function> functions = null)
        {
            if (functions == null || !functions.Any())
            {
                functions = _functions;
            }

            if (res == null || !res.Any()) res = new List<Function>();

            var parents = functions.Where(x => x.Name == functionName && !string.IsNullOrEmpty(x.Caller));

            res.AddRange(parents);

            foreach (var parent in parents) GetParents(parent.Caller, ref res, functions);
        }

        private bool IsValidLine(string line)
        {
            return line.Contains(FunctionAttribute)
                   || line.Contains(StartNewAsyncCall)
                   || line.Contains(GenericActivityAsyncCall)
                   || line.Contains(RegularActivityAsyncCall)
                   || line.Contains(GenericSubOrchestratorAsyncCall)
                   || line.Contains(RegularSubOrchestratorAsyncCall);
        }
    }
}