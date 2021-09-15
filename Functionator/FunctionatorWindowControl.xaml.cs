using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;

namespace Functionator
{
    public partial class FunctionatorWindowControl
    {
        public static readonly DependencyProperty FuncNameProperty = DependencyProperty.Register(
            nameof(FuncName), typeof(string), typeof(FunctionatorWindowControl),
            new PropertyMetadata(default(string), OnFuncNameChanged));

        public static readonly DependencyProperty ChildrenProperty = DependencyProperty.Register(
            nameof(Children), typeof(IEnumerable<IEnumerable<string>>), typeof(FunctionatorWindowControl),
            new PropertyMetadata(default(IEnumerable<IEnumerable<string>>)));

        public static readonly DependencyProperty ParentsProperty = DependencyProperty.Register(
            nameof(Parents), typeof(IEnumerable<IEnumerable<string>>), typeof(FunctionatorWindowControl),
            new PropertyMetadata(default(IEnumerable<IEnumerable<string>>)));

        private readonly Analyzer.Analyzer _analyzer;
        
        public FunctionatorWindowControl()
        {
            InitializeComponent();
            _analyzer = new Analyzer.Analyzer();
        }

        public IEnumerable<IEnumerable<string>> Parents
        {
            get => (IEnumerable<IEnumerable<string>>) GetValue(ParentsProperty);
            set => SetValue(ParentsProperty, value);
        }

        public IEnumerable<IEnumerable<string>> Children
        {
            get => (IEnumerable<IEnumerable<string>>) GetValue(ChildrenProperty);
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
            Children = _analyzer.GetAllChildrenCombinations("BatchCalculation");

            Parents = _analyzer.GetAllParentsCombinations("GetEventDescriptions");
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
}