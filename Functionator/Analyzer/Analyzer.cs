﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public Analyzer() => _functions = GetAllFunctions(@"c:\Users\JovanSredanovic\source\repos\i4SEE\");

        public ObservableCollection<Function> GetChildren(string functionName)
        {
            return new(_functions.Where(x => x.Caller == functionName)
                .Select(x => new Function(x.Name, x.Caller, x.FunctionType, x.TriggerTypeString) { Children = GetChildren(x.Name) }));
        }

        public ObservableCollection<Function> GetParents(string functionName)
        {
            return new(_functions.Where(x => x.Name == functionName && !string.IsNullOrEmpty(x.Caller))
                .Select(x => new Function(x.Name, x.Caller, x.FunctionType, x.TriggerTypeString) { Parents = GetParents(x.Caller) }));
        }

        private List<Function> GetAllFunctions(string location)
        {
            var allFiles = Directory.GetFiles(location, "*", SearchOption.AllDirectories).Where(x => x.EndsWith(".cs")).ToList();

            return allFiles.Select(GetFileFunctions).SelectMany(x => x).ToList();
        }

        private List<Function> GetFileFunctions(string file)
        {
            var fileFunctions = new List<Function>();
            string completeLineOfCode = default;

            string callerFunctionName = default;
            foreach (var line in File.ReadAllLines(file))
            {
                var trimmedLine = line.Trim();

                if (!IsValidLine(trimmedLine) && string.IsNullOrEmpty(completeLineOfCode)) continue;

                completeLineOfCode += trimmedLine;

                if (!completeLineOfCode.EndsWith(";")) continue;

                var func = GetFunction(completeLineOfCode, callerFunctionName);
                if (func.FunctionType == FunctionType.Caller)
                {
                    callerFunctionName = func.Name;
                }

                fileFunctions.Add(func);

                completeLineOfCode = string.Empty;
            }

            return fileFunctions;
        }

        private Function GetFunction(string completeLineOfCode, string callerFunctionName)
        {
            string triggerType = default;
            var functionType = GetFunctionType(completeLineOfCode);
            if (functionType == FunctionType.Caller)
            {
                triggerType = GetCallerFunctionTriggerType(completeLineOfCode);
                callerFunctionName = default;
            }

            var (start, end) = GetFunctionStringWrappers(functionType);

            var functionNameStart = completeLineOfCode.IndexOf(start, StringComparison.Ordinal) + start.Length;
            var functionNameEnd = completeLineOfCode.IndexOf(end, StringComparison.Ordinal);

            var functionName = completeLineOfCode.Substring(functionNameStart, functionNameEnd - functionNameStart);// [functionNameStart..functionNameEnd];

            return new (functionName, callerFunctionName, functionType, triggerType);
        }

        private string GetCallerFunctionTriggerType(string completeLineOfCode)
        {
            var triggerAttribute = completeLineOfCode.Contains(TriggerAttribute)
                ? TriggerAttribute
                : TriggerAttributeWithParam;

            var triggerNameStart = completeLineOfCode.LastIndexOf('[',
                completeLineOfCode.IndexOf(triggerAttribute, StringComparison.Ordinal)) + 1;

            var triggerNameEnd = completeLineOfCode.IndexOf(triggerAttribute, StringComparison.Ordinal);
            return completeLineOfCode.Substring(triggerNameStart, triggerNameEnd - triggerNameStart);// [triggerNameStart..triggerNameEnd];
        }

        private (string start, string end) GetFunctionStringWrappers(FunctionType functionType) =>
            functionType switch
            {
                FunctionType.Caller => (FunctionAttribute, FunctionAttributeEndString),
                FunctionType.Orchestrator => (StartNewAsyncCall, FunctionCallEndString),
                FunctionType.Activity => (RegularActivityAsyncCall, FunctionCallEndString),
                FunctionType.GenericActivity => (FunctionCallStartString, FunctionCallEndString),
                FunctionType.SubOrchestrator => (RegularSubOrchestratorAsyncCall, FunctionCallEndString),
                FunctionType.GenericSubOrchestrator => (FunctionCallStartString, FunctionCallEndString),
                _ => (string.Empty, string.Empty)
            };

        private FunctionType GetFunctionType(string completeLineOfCode) =>
            completeLineOfCode switch
            {
                _ when completeLineOfCode.Contains(FunctionAttribute) => FunctionType.Caller,
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