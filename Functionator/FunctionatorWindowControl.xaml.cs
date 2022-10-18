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

        
        public static readonly DependencyProperty AnyChildrenProperty = DependencyProperty.Register(
            nameof(AnyChildren), typeof(bool), typeof(FunctionatorWindowControl), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty AnyParentsProperty = DependencyProperty.Register(
            nameof(AnyParents), typeof(bool), typeof(FunctionatorWindowControl), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty NoChildrenProperty = DependencyProperty.Register(
            nameof(NoChildren), typeof(bool), typeof(FunctionatorWindowControl), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty NoParentsProperty = DependencyProperty.Register(
            nameof(NoParents), typeof(bool), typeof(FunctionatorWindowControl), new PropertyMetadata(default(bool)));

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

        public bool AnyChildren
        {
            get => (bool)GetValue(AnyChildrenProperty);
            set => SetValue(AnyChildrenProperty, value);
        }

        public bool AnyParents
        {
            get => (bool)GetValue(AnyParentsProperty);
            set => SetValue(AnyParentsProperty, value);
        }

        public bool NoChildren
        {
            get => (bool)GetValue(NoChildrenProperty);
            set => SetValue(NoChildrenProperty, value);
        }

        public bool NoParents
        {
            get => (bool)GetValue(NoParentsProperty);
            set => SetValue(NoParentsProperty, value);
        }

        private static async void OnFuncNameChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            await (d as FunctionatorWindowControl)!.AnalyzeThisAsync();
        }

        internal async Task AnalyzeThisAsync()
        {
            await UpdateFunctionsAsync();

            AnyChildren = default;
            AnyParents = default;
            NoChildren = true;
            NoParents = true;

            Children = _analyzer.GetChildrenHierarchy(FuncName);

            if (Children != null && Children.Any())
            {
                AnyChildren = true;
                NoChildren = false;
            }

            Parents = _analyzer.GetParentsHierarchy(FuncName);

            if (Parents != null && Parents.Any())
            {
                AnyParents = true;
                NoParents = false;
            }
        }

        private async Task UpdateFunctionsAsync()
        {
            var activeDocument = await VS.Documents.GetActiveDocumentViewAsync();

            var physicalFile = await PhysicalFile.FromFileAsync(activeDocument!.FilePath!);

            var path = physicalFile!.ContainingProject;
            
            _analyzer.UpdateFunctions(Path.GetDirectoryName(path!.FullPath));
        }

        private async void ParentsGoToUsageButton_OnClick(object sender, RoutedEventArgs e) =>
            await GoToAsync(ParentsTreeView.SelectedItem as Function);
        

        private async void ParentsGoToDefinitionButton_OnClick(object sender, RoutedEventArgs e) =>
            await GoToAsync(_analyzer.GetFunctionDefinition(ParentsTreeView.SelectedItem as Function));
        

        private async void ChildrenGoToUsageButton_OnClick(object sender, RoutedEventArgs e) =>
            await GoToAsync(ChildrenTreeView.SelectedItem as Function);

        private async void ChildrenGoToDefinitionButton_OnClick(object sender, RoutedEventArgs e) =>
            await GoToAsync(_analyzer.GetFunctionDefinition(ChildrenTreeView.SelectedItem as Function));

        private async Task GoToAsync(Function function)
        {
            if (function == null) return;

            if (string.IsNullOrEmpty(function.FilePath) || function.LineNumber == default) return;

            await VS.Documents.OpenAsync(function.FilePath);
            var ts = _dte.ActiveDocument.Selection as TextSelection;
            ts.MoveToLineAndOffset(function.LineNumber, 1);
        }
    }
}