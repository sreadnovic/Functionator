using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Functionator.Analyzer
{
    internal class Analyzer
    {

        private List<Function> GetFunctions()
        {
            const string functionAttribute = "[FunctionName(";
            const string startNewAsyncCall = "StartNewAsync(\"";
            const string genericActivityAsyncCall = "CallActivityAsync<";
            const string regularActivityAsyncCall = "CallActivityAsync(\"";
            const string genericSubOrchestratorAsyncCall = "CallSubOrchestratorAsync<";
            const string regularSubOrchestratorAsyncCall = "CallSubOrchestratorAsync(\"";

            var allFiles = Directory
                .GetFiles("c:\\Users\\JovanSredanovic\\source\\repos\\i4SEE\\", "*", SearchOption.AllDirectories)
                .Where(x => x.EndsWith(".cs")).ToList();
            Console.WriteLine(allFiles.Count());

            var stopWatch = Stopwatch.StartNew();

            var functions = new List<Function>();

            string function = null;

            string completeLineOfCode = string.Empty;

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

                    if (completeLineOfCode.Contains(functionAttribute))
                    {
                        function = completeLineOfCode.Substring(functionAttribute.Length + 1,
                            completeLineOfCode.IndexOf("\")]") - (functionAttribute.Length + 1));
                        functions.Add(new Function(function, null, FunctionType.Trigger));
                    }
                    else if (completeLineOfCode.Contains(startNewAsyncCall))
                    {
                        functions.Add(new Function(completeLineOfCode.Substring(completeLineOfCode.IndexOf(startNewAsyncCall) + startNewAsyncCall.Length, completeLineOfCode.IndexOf("\",") -
                            (completeLineOfCode.IndexOf(startNewAsyncCall) + startNewAsyncCall.Length)), function, FunctionType.Orchestrator));
                    }
                    else if (completeLineOfCode.Contains(genericActivityAsyncCall))
                    {
                        functions.Add(new Function(completeLineOfCode.Substring(completeLineOfCode.IndexOf(">(\"") + 3, completeLineOfCode.IndexOf("\",") -
                            completeLineOfCode.IndexOf(">(\"") - 3), function, FunctionType.Activity));
                    }
                    else if (completeLineOfCode.Contains(regularActivityAsyncCall))
                    {
                        functions.Add(new Function(completeLineOfCode.Substring(completeLineOfCode.IndexOf(regularActivityAsyncCall) + regularActivityAsyncCall.Length, completeLineOfCode.IndexOf("\",") -
                            (completeLineOfCode.IndexOf(regularActivityAsyncCall) + regularActivityAsyncCall.Length)), function, FunctionType.Activity));
                    }
                    else if (completeLineOfCode.Contains(genericSubOrchestratorAsyncCall))
                    {
                        functions.Add(new Function(completeLineOfCode.Substring(completeLineOfCode.IndexOf(">(\"") + 3, completeLineOfCode.IndexOf("\",") -
                            completeLineOfCode.IndexOf(">(\"") - 3), function, FunctionType.SubOrchestrator));
                    }
                    else if (completeLineOfCode.Contains(regularSubOrchestratorAsyncCall))
                    {
                        functions.Add(new Function(completeLineOfCode.Substring(completeLineOfCode.IndexOf(regularSubOrchestratorAsyncCall) + regularSubOrchestratorAsyncCall.Length, completeLineOfCode.IndexOf("\",") -
                            (completeLineOfCode.IndexOf(regularSubOrchestratorAsyncCall) + regularSubOrchestratorAsyncCall.Length)), function, FunctionType.SubOrchestrator));
                    }

                    completeLineOfCode = string.Empty;
                }
            }

            return functions;
        }

        internal IEnumerable<IEnumerable<string>> GetAllChildrenCombinations(List<Function> functions, string functionName)
        {
            var res = new List<List<string>>();

            var children = new List<Function>();
            GetChildren(functionName, functions, children);

            foreach (var r in children.Where(x => x.Name.All(char.IsLetterOrDigit) && x.IsOnBottom))
            {
                r.IsOnBottom = children.FirstOrDefault(x => x.Caller == r.Name) == null;

                foreach (var combination in GetAllParentsCombinations(children, r.Name))
                {
                    res.Add(combination.ToList());
                }
            }

            return res;
        }

        internal IEnumerable<IEnumerable<string>> GetAllParentsCombinations(List<Function> functions, string functionName)
        {
            var parents = new List<Function>();
            GetParents(functionName, functions, parents);

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

        private void GetChildren(string functionName, IEnumerable<Function> functions, List<Function> res)
        {
            //res ??= new List<Function>();

            if (res == null) res = new List<Function>();

            var children = functions.Where(x => x.Caller == functionName);

            res.AddRange(children);

            foreach (var child in children) GetChildren(child.Name, functions, res);
        }

        private void GetParents(string functionName, IEnumerable<Function> functions, List<Function> res)
        {
            //res ??= new List<Function>();
            if (res == null) res = new List<Function>();

            var parents = functions.Where(x => x.Name == functionName && !string.IsNullOrEmpty(x.Caller));

            res.AddRange(parents);

            foreach (var parent in parents) GetParents(parent.Caller, functions, res);
        }

        private bool IsValidLine(string line)
        {
            const string functionAttribute = "[FunctionName(";
            const string startNewAsyncCall = "StartNewAsync(\"";
            const string genericActivityAsyncCall = "CallActivityAsync<";
            const string regularActivityAsyncCall = "CallActivityAsync(\"";
            const string genericSubOrchestratorAsyncCall = "CallSubOrchestratorAsync<";
            const string regularSubOrchestratorAsyncCall = "CallSubOrchestratorAsync(\"";

            return line.Contains(functionAttribute)
                   || line.Contains(startNewAsyncCall)
                   || line.Contains(genericActivityAsyncCall)
                   || line.Contains(regularActivityAsyncCall)
                   || line.Contains(genericSubOrchestratorAsyncCall)
                   || line.Contains(regularSubOrchestratorAsyncCall);
        }
    }
}