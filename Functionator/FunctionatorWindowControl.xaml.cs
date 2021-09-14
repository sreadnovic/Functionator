using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace Functionator
{
    /// <summary>
    ///     Interaction logic for FunctionatorWindowControl.
    /// </summary>
    public partial class FunctionatorWindowControl
    {
        public static readonly DependencyProperty FuncNameProperty = DependencyProperty.Register(
            nameof(FuncName), typeof(string), typeof(FunctionatorWindowControl),
            new PropertyMetadata(default(string), OnFuncNameChanged));

        private readonly Analyzer.Analyzer _analyzer;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FunctionatorWindowControl" /> class.
        /// </summary>
        public FunctionatorWindowControl()
        {
            InitializeComponent();
            _analyzer = new Analyzer.Analyzer();
            DataContext = this;
        }

        public string FuncName
        {
            get => (string) GetValue(FuncNameProperty);
            set => SetValue(FuncNameProperty, value);
        }

        private static void OnFuncNameChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            (d as FunctionatorWindowControl).AnalyzeThis();
        }

        public void AnalyzeThis()
        {
            var counter1 = _analyzer.GetAllChildrenCombinations("BatchCalculation").Count();

            var counter2 = _analyzer.GetAllParentsCombinations("GetEventDescriptions").Count();
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