using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Windows;
using Functionator.Analyzer;

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
        
        public FunctionatorWindowControl()
        {
            InitializeComponent();
            _analyzer = new();
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

        private static void OnFuncNameChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            (d as FunctionatorWindowControl)?.AnalyzeThis();
        }

        public void AnalyzeThis()
        {
            var children = new ObservableCollection<Function>(_analyzer.GetChildren("BatchCalculation"));
            
            Children = new () { new (children.First().Caller, null, default, default) { Children = children } };

            Parents = new ();

            foreach (var function in _analyzer.GetParentsInverted("GetEventDescriptions"))
            {
                Parents.Add(new (function.Caller, null, default, default) { Children = new (){function} });
            }
        }

        /// <summary>
        ///     Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter",
            Justification = "Default event handler naming pattern")]
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                string.Format(CultureInfo.CurrentUICulture, "Invoked '{0}'", ToString()),
                "FunctionatorWindow");
        }
    }

    public class Family
    {
        public Family()
        {
            this.Members = new ObservableCollection<FamilyMember>();
        }

        public string Name { get; set; }

        public ObservableCollection<FamilyMember> Members { get; set; }
    }

    public class FamilyMember
    {
        public string Name { get; set; }

        public int Age { get; set; }
    }
}