using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Functionator.Analyzer;

public class Analyzer
{
    private List<Function> _functions;
    private Collector _collector;

    private static Analyzer _instance;

    public static Analyzer GetInstance()
    {
        if (_instance == null)
        {
            _instance = new Analyzer();
        }

        return _instance;
    }

    public Analyzer() =>
        _collector = Collector.GetInstance();
    
    public void UpdateFunctions(string projectPath)
    {
        _functions = _collector.GetAllFunctions(projectPath);

        foreach (var item in _functions.Where(item => string.IsNullOrEmpty(item.TriggerTypeString)))
        {
            item.TriggerTypeString = GetFunctionTriggerType(item.Name);
        }
    }

    private string GetFunctionTriggerType(string functionName) =>
        _functions.FirstOrDefault(x => x.Name == functionName && !string.IsNullOrEmpty(x.TriggerTypeString))?.TriggerTypeString;

    internal Function GetCallerFunction(Function function) =>
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

    private ObservableCollection<Function> GetChildren(string functionName) =>
        new(_functions.Where(x => x.Caller == functionName)
            .Select(x => new Function(x.Name, x.Caller, x.FunctionType, x.TriggerTypeString, x.FilePath, x.LineNumber) { Children = GetChildren(x.Name) }));
    
    private ObservableCollection<Function> GetChildren(string functionName, List<Function> functions) =>
        new(functions.Where(x => x.Caller == functionName)
            .Select(x => new Function(x.Name, x.Caller, x.FunctionType, x.TriggerTypeString, x.FilePath, x.LineNumber) { Children = GetChildren(x.Name, functions) }));
    
    private ObservableCollection<Function> GetParents(string functionName) =>
        new(_functions.Where(x => x.Name == functionName && !string.IsNullOrEmpty(x.Caller))
            .Select(x => new Function(x.Name, x.Caller, x.FunctionType, x.TriggerTypeString, x.FilePath, x.LineNumber) { Parents = GetParents(x.Caller) }));
    
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

        parentsInverted = parentsInverted.GroupBy(x => x.Name).Select(x => x.First()).ToList();

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
}