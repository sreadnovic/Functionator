using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio.TextManager.Interop;
using Task = System.Threading.Tasks.Task;

namespace Functionator
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class FunctionatorWindowCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 4129;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("10711c9f-bd4e-4fe1-9a20-b97c976ad241");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionatorWindowCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private FunctionatorWindowCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static FunctionatorWindowCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in FunctionatorWindowCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new FunctionatorWindowCommand(package, commandService);
        }

        /// <summary>
        /// Shows the tool window when the menu item is clicked.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.package.FindToolWindow(typeof(FunctionatorWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("Cannot create tool window");
            }

            TextViewSelection selection = GetSelection(ServiceProvider);

            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        struct TextViewSelection
        {
            public TextViewPosition StartPosition { get; set; }
            public TextViewPosition EndPosition { get; set; }
            public string Text { get; set; }
 
            public TextViewSelection(TextViewPosition a, TextViewPosition b, string text)
            {
                StartPosition = TextViewPosition.Min(a, b);
                EndPosition = TextViewPosition.Max(a, b);
                Text = text;
            }
        }
 
        private TextViewSelection GetSelection(IServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetService(typeof(SVsTextManager));
            var textManager = service as IVsTextManager2;
            IVsTextView view;
            int result = textManager.GetActiveView2(1, null, (uint)_VIEWFRAMETYPE.vftCodeWindow, out view);
 
            view.GetSelection(out int startLine, out int startColumn, out int endLine, out int endColumn);//end could be before beginning
            var start = new TextViewPosition(startLine, startColumn);
            var end = new TextViewPosition(endLine, endColumn);
 
            view.GetSelectedText(out string selectedText);
 
            TextViewSelection selection = new TextViewSelection(start, end, selectedText);
            return selection;
        }
 
        public struct TextViewPosition
        {
            private readonly int _column;
            private readonly int _line;
 
            public TextViewPosition(int line, int column)
            {
                _line = line;
                _column = column;
            }
 
            public int Line { get { return _line; } }
            public int Column { get { return _column; } }
 
 
            public static bool operator <(TextViewPosition a, TextViewPosition b)
            {
                if (a.Line < b.Line)
                {
                    return true;
                }
                else if (a.Line == b.Line)
                {
                    return a.Column < b.Column;
                }
                else
                {
                    return false;
                }
            }
 
            public static bool operator >(TextViewPosition a, TextViewPosition b)
            {
                if (a.Line > b.Line)
                {
                    return true;
                }
                else if (a.Line == b.Line)
                {
                    return a.Column > b.Column;
                }
                else
                {
                    return false;
                }
            }
 
            public static TextViewPosition Min(TextViewPosition a, TextViewPosition b)
            {
                return a > b ? b : a;
            }
 
            public static TextViewPosition Max(TextViewPosition a, TextViewPosition b)
            {
                return a > b ? a : b;
            }
        }
        private string GetActiveFilePath(IServiceProvider serviceProvider)
        {
            EnvDTE80.DTE2 applicationObject = serviceProvider.GetService(typeof(DTE)) as EnvDTE80.DTE2;
            return applicationObject.ActiveDocument.FullName;
        }
    }
}
