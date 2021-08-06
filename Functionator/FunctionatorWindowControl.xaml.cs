using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.Shell;
using Functionator.Analyzer;

namespace Functionator
{
    /// <summary>
    /// Interaction logic for FunctionatorWindowControl.
    /// </summary>
    public partial class FunctionatorWindowControl : UserControl
    {
        private Analyzer.Analyzer _analyzer;
        private string _functionName;

        public string FunctionName
        {
            get => _functionName;
            set => _functionName = value;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionatorWindowControl"/> class.
        /// </summary>
        public FunctionatorWindowControl()
        {
            this.InitializeComponent();
            _analyzer = new Analyzer.Analyzer();
        }

        /// <summary>
        /// Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", this.ToString()),
                "FunctionatorWindow");
        }
    }
}