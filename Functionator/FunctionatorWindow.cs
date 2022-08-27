using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;

namespace Functionator
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("3386414c-0d97-4b8f-b3f4-1db79a9527d0")]
    public class FunctionatorWindow : ToolWindowPane
    {
        private FunctionatorWindowControl _functionatorWindowControl;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionatorWindow"/> class.
        /// </summary>
        public FunctionatorWindow() : base(null)
        {
            this.Caption = "The Functionator";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            _functionatorWindowControl = new FunctionatorWindowControl();
            this.Content = _functionatorWindowControl;
        }

        public void SetFunctionName(string funtionName)
        {
            _functionatorWindowControl.FuncName = funtionName;
        }
    }
}
