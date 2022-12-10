using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Functionator.Analyzer;

public class Collector
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
    private const string TriggerAttributeStart = "[";
    private const string TriggerAttribute = "Trigger]";
    private const string TriggerAttributeWithParam = "Trigger(";

    private static Collector _instance;

    public static Collector GetInstance()
    {
        if (_instance == null)
        {
            _instance = new Collector();
        }

        return _instance;
    }

    public List<Function> GetAllFunctions(string location) =>
        Directory.GetFiles(location, "*.cs", SearchOption.AllDirectories).Select(GetFileFunctions).SelectMany(x => x).ToList();

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
                func = GetFunction(trimmedLine, default, file, lineNumber);
                callerFunctionName = func.Name;

                fileFunctions.Add(func);
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

    private string GetCallerFunctionTriggerType(string completeLineOfCode)
    {
        var triggerAttribute = completeLineOfCode.Contains(TriggerAttribute)
            ? TriggerAttribute
            : TriggerAttributeWithParam;

        return GetTextInBetween(completeLineOfCode, TriggerAttributeStart, triggerAttribute);
    }

    private string GetTextInBetween(string fullText, string start, string end)
    {
        var startIndex = fullText.IndexOf(start, StringComparison.Ordinal) + start.Length;
        var endIndex = fullText.IndexOf(end, StringComparison.Ordinal);

        if (startIndex == -1 || endIndex == -1) return default;

        return fullText.Substring(startIndex, endIndex - startIndex).Replace("\"", string.Empty);
    }

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
        const string nameOfEnd = ")";

        functionName = functionName!.Contains(nameOf) ? GetTextInBetween(functionName, nameOf, nameOfEnd) : functionName;

        return new(functionName, callerFunctionName, functionType, default, filePath, lineNumber);
    }

    private IEnumerable<(string start, string end)> GetFunctionStringWrappers(FunctionType functionType) =>
        functionType switch
        {
            FunctionType.Caller => new() { (FunctionAttribute, FunctionAttributeEndString) },
            FunctionType.Orchestrator => new() { (StartNewAsyncCall, FunctionCallEndString), (StartNewAsyncCall, FunctionCallEndStringNoParameters) },
            FunctionType.Activity => new() { (RegularActivityAsyncCall, FunctionCallEndString) },
            FunctionType.GenericActivity => new() { (FunctionCallStartString, FunctionCallEndString) },
            FunctionType.SubOrchestrator => new() { (RegularSubOrchestratorAsyncCall, FunctionCallEndString) },
            FunctionType.GenericSubOrchestrator => new() { (FunctionCallStartString, FunctionCallEndString) },
            _ => new List<(string, string)> { (string.Empty, string.Empty) }
        };

}
