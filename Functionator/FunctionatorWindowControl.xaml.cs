using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Community.VisualStudio.Toolkit;
using Functionator.Analyzer;
using Microsoft.VisualStudio.Shell;
using EnvDTE;

namespace Functionator
{
    public partial class FunctionatorWindowControl
    {
        public static readonly DependencyProperty FuncNameProperty = DependencyProperty.Register(
            nameof(FuncName), typeof(string), typeof(FunctionatorWindowControl),
            new PropertyMetadata(default(string), OnFuncNameChanged));

        public static readonly DependencyProperty ChildrenProperty = DependencyProperty.Register(
            nameof(Children), typeof(ObservableCollection<Function>), typeof(FunctionatorWindowControl),
            new PropertyMetadata(default(ObservableCollection<Function>)));

        public static readonly DependencyProperty ParentsProperty = DependencyProperty.Register(
            nameof(Parents), typeof(ObservableCollection<Function>), typeof(FunctionatorWindowControl),
            new PropertyMetadata(default(ObservableCollection<Function>)));

        private readonly Analyzer.Analyzer _analyzer;
        private readonly DTE _dte;
        
        public FunctionatorWindowControl()
        {
            InitializeComponent();
            _analyzer = new();
            _dte = (DTE)Package.GetGlobalService(typeof(DTE));
        }

        public ObservableCollection<Function> Parents
        {
            get => (ObservableCollection<Function>) GetValue(ParentsProperty);
            set => SetValue(ParentsProperty, value);
        }

        public ObservableCollection<Function> Children
        {
            get => (ObservableCollection<Function>) GetValue(ChildrenProperty);
            set => SetValue(ChildrenProperty, value);
        }

        public string FuncName
        {
            get => (string) GetValue(FuncNameProperty);
            set => SetValue(FuncNameProperty, value);
        }

        private static async void OnFuncNameChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            await (d as FunctionatorWindowControl)!.AnalyzeThisAsync();
        }

        internal async Task AnalyzeThisAsync()
        {
            //await VS.Documents.OpenAsync("C:\\Users\\JovanSredanovic\\source\\repos\\i4SEE\\i4SEE\\i4SEE.Functions\\InputApi\\DiagnosisEngineFunction.cs");
            //var ts = _dte.ActiveDocument.Selection as TextSelection;
            //ts.MoveToLineAndOffset(55, 1);

            await UpdateFunctionsAsync();

            var children = new ObservableCollection<Function>(_analyzer.GetChildren("BatchCalculation"));
            
            Children = new () { new (children.First().Caller, null, default, _analyzer.GetFunctionTriggerType(children.First().Caller)) { Children = children } };

            Parents = new ();

            foreach (var function in _analyzer.GetParentsInverted("GetEventDescriptions"))
            {
                Parents.Add(new (function.Caller, null, default, _analyzer.GetFunctionTriggerType(function.Caller)) { Children = new (){function} });
            }
            
            
        }

        private async Task UpdateFunctionsAsync()
        {
            var activeDocument = await VS.Documents.GetActiveDocumentViewAsync();

            var physicalFile = await PhysicalFile.FromFileAsync(activeDocument!.FilePath!);

            var path = physicalFile!.ContainingProject;
            
            _analyzer.UpdateFunctions(Path.GetDirectoryName(path!.FullPath));
        }
    }
}