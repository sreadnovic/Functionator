using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
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
            nameof(Parents), typeof(IEnumerable<Analyzer.Function>), typeof(FunctionatorWindowControl),
            new PropertyMetadata(default(IEnumerable<Analyzer.Function>)));

        private readonly Analyzer.Analyzer _analyzer;
        
        public FunctionatorWindowControl()
        {
            InitializeComponent();
            _analyzer = new();
        }

        public IEnumerable<Analyzer.Function> Parents
        {
            get => (IEnumerable<Analyzer.Function>) GetValue(ParentsProperty);
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
            Children = new(_analyzer.GetChildren("BatchCalculation"));

            //ParentsTreeView.ItemsSource = Children;

            Parents = _analyzer.GetParents("GetEventDescriptions");

            //ParentsTreeView.Items.Clear();

            //List<Family> families = new List<Family>();

            //Family family1 = new Family() { Name = "The Doe's" };
            //family1.Members.Add(new FamilyMember() { Name = "John Doe", Age = 42 });
            //family1.Members.Add(new FamilyMember() { Name = "Jane Doe", Age = 39 });
            //family1.Members.Add(new FamilyMember() { Name = "Sammy Doe", Age = 13 });
            //families.Add(family1);

            //Family family2 = new Family() { Name = "The Moe's" };
            //family2.Members.Add(new FamilyMember() { Name = "Mark Moe", Age = 31 });
            //family2.Members.Add(new FamilyMember() { Name = "Norma Moe", Age = 28 });
            //families.Add(family2);

            //trvFamilies.ItemsSource = families;
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