using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Text;
using System.Text.RegularExpressions;
using EnvDTE;
using EnvDTE80;

namespace VSIXIvson
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class FindReplaceGUIDs
  {
    /// <summary>
    /// Command ID.
    /// </summary>
    public const int CommandId = 4131;

    /// <summary>
    /// Command menu group (command set GUID).
    /// </summary>
    public static readonly Guid CommandSet = new Guid("cf5988aa-595f-4ae0-8831-d38ec20b3047");

    /// <summary>
    /// VS Package that provides this command, not null.
    /// </summary>
    private readonly Package package;

    /// <summary>
    /// Initializes a new instance of the <see cref="FindReplaceGUIDs"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    private FindReplaceGUIDs(Package package)
    {
      if (package == null)
      {
        throw new ArgumentNullException("package");
      }

      this.package = package;

      OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
      if (commandService != null)
      {
        var menuCommandID = new CommandID(CommandSet, CommandId);
        var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
        commandService.AddCommand(menuItem);
      }
    }

    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static FindReplaceGUIDs Instance
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
    public static void Initialize(Package package)
    {
      Instance = new FindReplaceGUIDs(package);
    }

    /// <summary>
    /// This function is the callback used to execute the command when the menu item is clicked.
    /// See the constructor to see how the menu item is associated with this function using
    /// OleMenuCommandService service and MenuCommand class.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event args.</param>
    private void MenuItemCallback(object sender, EventArgs e)
    {
      string title = "FindReplaceGUIDs";

      DTE2 dte = (DTE2)this.ServiceProvider.GetService(typeof(DTE));
      TextSelection selection = dte.ActiveDocument.Selection as TextSelection;

      StringBuilder selectedText = new StringBuilder();
      selectedText.Append(selection.Text);

      MatchCollection allGuids = Regex.Matches(selection.Text, "[0-9a-hA-H]+-[0-9a-hA-H]+-[0-9a-hA-H]+-[0-9a-hA-H]+-[0-9a-hA-H]+", RegexOptions.Multiline);
      foreach (Match guid in allGuids)
      {
        selectedText.Replace(guid.Value, System.Guid.NewGuid().ToString().ToUpper());
      }
      selection.Text = selectedText.ToString();

      // Show a message box to prove we were here
      VsShellUtilities.ShowMessageBox(
          this.ServiceProvider,
          allGuids.Count.ToString() + " GUIDs replaced.",
          title,
          OLEMSGICON.OLEMSGICON_INFO,
          OLEMSGBUTTON.OLEMSGBUTTON_OK,
          OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
    }
  }
}


//Public Module RegexTools
//    Sub FindReplaceGUIDsInSelection()
//        Dim ts As TextSelection = DTE.ActiveDocument.Selection
//        Dim selectedText As StringBuilder = New StringBuilder
//        selectedText.Append(ts.Text)
//        Dim allGuids As MatchCollection = Regex.Matches(ts.Text, "[0-9a-hA-H]+-[0-9a-hA-H]+-[0-9a-hA-H]+-[0-9a-hA-H]+-[0-9a-hA-H]+", RegexOptions.Multiline)
//        For Each guid As Match In allGuids
//            selectedText.Replace(guid.Value, System.Guid.NewGuid().ToString().ToUpper())
//        Next
//        ts.Text = selectedText.ToString
//        MsgBox(allGuids.Count.ToString() + " GUIDs replaced.")
//    End Sub
//End Module
