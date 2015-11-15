//------------------------------------------------------------------------------
// <copyright file="SwitchCPPH.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
using EnvDTE80;

namespace VSIXIvson
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class SwitchCPPH
  {
    /// <summary>
    /// Command ID.
    /// </summary>
    public const int CommandId = 0x0100;

    /// <summary>
    /// Command menu group (command set GUID).
    /// </summary>
    public static readonly Guid CommandSet = new Guid("cf5988aa-595f-4ae0-8831-d38ec20b3047");

    /// <summary>
    /// VS Package that provides this command, not null.
    /// </summary>
    private readonly Package package;

    /// <summary>
    /// Initializes a new instance of the <see cref="SwitchCPPH"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    private SwitchCPPH(Package package)
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
    public static SwitchCPPH Instance
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
      Instance = new SwitchCPPH(package);
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
      try
      {
        DTE2 dte = (DTE2)this.ServiceProvider.GetService(typeof(DTE));

        string fullName = dte.ActiveDocument.FullName.ToLower();
        FileInfo fi = new FileInfo(fullName);
        if (fi.Extension == ".cpp" || fi.Extension == ".c")
        {
          string newName = fullName.Replace(fi.Extension, ".h");
          if (File.Exists(newName))
          {
            dte.ItemOperations.OpenFile(newName);
          }
          else
          {
            newName = fullName.Replace(fi.Extension, ".hpp");
            if (File.Exists(newName))
              dte.ItemOperations.OpenFile(newName);
          }
        }
        else if (fi.Extension == ".hpp" || fi.Extension == ".h")
        {
          string newName = fullName.Replace(fi.Extension, ".cpp");
          if (File.Exists(newName))
          {
            dte.ItemOperations.OpenFile(newName);
          }
          else
          {
            newName = fullName.Replace(fi.Extension, ".c");
            if (File.Exists(newName))
              dte.ItemOperations.OpenFile(newName);
          }
        }
      }
      catch (Exception)
      {
        //VsShellUtilities.ShowMessageBox(
        //    this.ServiceProvider,
        //    message,
        //    title,
        //    OLEMSGICON.OLEMSGICON_INFO,
        //    OLEMSGBUTTON.OLEMSGBUTTON_OK,
        //    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
      }
    }
  }
}
