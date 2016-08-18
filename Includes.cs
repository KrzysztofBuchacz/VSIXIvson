//------------------------------------------------------------------------------
// <copyright file="Includes.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
using EnvDTE80;

namespace VSIXIvson
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class Includes
  {
    /// <summary>
    /// Command ID.
    /// </summary>
    public const int CommandId = 4130;

    /// <summary>
    /// Command menu group (command set GUID).
    /// </summary>
    public static readonly Guid CommandSet = new Guid("cf5988aa-595f-4ae0-8831-d38ec20b3047");

    /// <summary>
    /// VS Package that provides this command, not null.
    /// </summary>
    private readonly Package package;

    /// <summary>
    /// Initializes a new instance of the <see cref="Includes"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    private Includes(Package package)
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
    public static Includes Instance
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
      Instance = new Includes(package);
    }

    ProjectItem GetFiles(ProjectItem item)
    {
      projectItems.Add(item);

      if (item.SubProject != null)
      {
        if (item.SubProject.ProjectItems != null)
        {
          foreach (ProjectItem sitem in item.SubProject.ProjectItems)
          {
            GetFiles(sitem);
          }
        }
      }

      if (item.ProjectItems == null)
        return item;

      var items = item.ProjectItems.GetEnumerator();
      while (items.MoveNext())
      {
        var currentItem = (ProjectItem)items.Current;
        projectItems.Add(GetFiles(currentItem));
      }

      return item;
    }

    private List<ProjectItem> projectItems = new List<ProjectItem>();

    private void AddHeader(string header, int line, ref bool opened, ProjectItem pitem)
    {
      if (!opened)
      {
        opened = true;
        Window win = pitem.Open(EnvDTE.Constants.vsViewKindCode);
        win.Activate();
      }
      DTE2 dte = (DTE2)this.ServiceProvider.GetService(typeof(DTE));
      TextSelection selection = dte.ActiveDocument.Selection as TextSelection;
      selection.GotoLine(line, false);
      selection.NewLine();
      selection.GotoLine(line, false);
      selection.Text = "#include " + header;
    }

    private void Replace(string what, string with, ref bool opened, ProjectItem pitem)
    {
      if (what == with)
        return;
      if (!opened)
      {
        opened = true;
        Window win = pitem.Open(EnvDTE.Constants.vsViewKindCode);
        win.Activate();
      }
      DTE2 dte = (DTE2)this.ServiceProvider.GetService(typeof(DTE));
      dte.Find.Action = vsFindAction.vsFindActionReplaceAll;
      dte.Find.FindWhat = what;
      dte.Find.ReplaceWith = with;
      dte.Find.Target = vsFindTarget.vsFindTargetCurrentDocument;
      dte.Find.MatchCase = true;
      dte.Find.MatchWholeWord = true;
      dte.Find.MatchInHiddenText = true;
      dte.Find.PatternSyntax = vsFindPatternSyntax.vsFindPatternSyntaxLiteral;
      dte.Find.SearchPath = "Current Document";
      dte.Find.Execute();
    }

    private string ToBackslash(string path)
    {
      path = path.Replace("/", "\\");
      return path;
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
        // get all files in solution
        EnvDTE.Projects projects = dte.Solution.Projects;
        projectItems.Clear();
        foreach (Project project in projects)
        {
          foreach (ProjectItem item in project.ProjectItems)
          {
            GetFiles(item);
          }
        }

        Dictionary<string, string> uniquePaths = new Dictionary<string, string>();
        List<ProjectItem> editableItems = new List<ProjectItem>();
        foreach (ProjectItem pitem in projectItems)
        {
          try
          {
            Property opath = pitem.Properties.Item("FullPath");
            string path = opath.Value.ToString();
            if (File.Exists(path))
            {
              FileInfo ifile = new FileInfo(path);
              if (ifile.IsReadOnly)
                continue;
              if (ifile.Extension.ToLower() == ".h" ||
                  ifile.Extension.ToLower() == ".hpp" ||
                  ifile.Extension.ToLower() == ".hd4" ||
                  ifile.Extension.ToLower() == ".c" ||
                  ifile.Extension.ToLower() == ".cpp")
              {
                if (!uniquePaths.ContainsKey(path))
                {
                  uniquePaths.Add(path, path);
                  editableItems.Add(pitem);
                }
              }
            }
          }
          catch
          {
          }
        }

        for (int k = 0; k < editableItems.Count; k++)
        {
          ProjectItem pitem  = editableItems[k];
          FileInfo docFile = new FileInfo(pitem.Properties.Item("FullPath").Value.ToString());
          bool opened = false;
          StreamReader sr = File.OpenText(docFile.FullName);
          List<string> lines = new List<string>();
          while (sr.Peek() >= 0)
          {
            lines.Add(sr.ReadLine());
          }
          sr.Close();
          bool firstHeader = true;
          for (int i = 0; i < lines.Count; i++)
          {
            string line = lines[i];
            if (line.StartsWith("#include"))
            {
              string include = line.Substring(9);
              include = include.TrimStart(new char[] { ' ', '\t' });
              // angle-braces or quotes
              bool braces = include.StartsWith("<");
              include = include.Split(new char[] { '\"', '>', '<' })[1];
              include = include.Replace("\\\\", "\\");
              string[] splitted = include.Split(new char[] { '\\', '/' });
              string includeFileName = splitted[splitted.Length - 1];

              if (firstHeader)
              {
                firstHeader = false;
                if (docFile.Name.ToLower() == "stdafx.h")
                {
                  if (includeFileName.ToLower() != "vc_pragma.h")
                  {
                    AddHeader("<vc_pragma.h>", i+1, ref opened, pitem);
                  }
                }
                if (docFile.Extension.ToLower() == ".c" ||
                    docFile.Extension.ToLower() == ".cpp")
                {
                  if (includeFileName.ToLower() != "stdafx.h" && includeFileName.ToLower() != "vc_pragma.h")
                  {
                    AddHeader("<vc_pragma.h>", i+1, ref opened, pitem);
                  }
                }
              }

              string comment = "";
              bool replaced = false;
              if (braces)
              {
                comment = line.Substring(line.IndexOf('>') + 1);
                // Rule: braces should be used for system headers only, if file exists in the same directory, then quotes should be used
                // eg. #include <soil.h> --> #include "soil.h"
                if (splitted.Length == 1 && File.Exists(docFile.DirectoryName + "\\" + includeFileName))
                {
                  FileInfo incFile = new FileInfo(docFile.DirectoryName + "\\" + includeFileName);
                  string newLine = "#include \"" + incFile.Name + "\"" + comment;
                  Replace(line, newLine, ref opened, pitem);
                  replaced = true;
                }
              }
              else // quotes
              {
                comment = line.Substring(line.IndexOf('"', line.IndexOf('"') + 1) + 1);
              }
              if (!replaced && splitted.Length == 1)
              {
                if (File.Exists(docFile.DirectoryName + "\\..\\..\\H\\" + includeFileName) &&
                   !File.Exists(docFile.DirectoryName + "\\" + includeFileName))
                {
                  FileInfo incFile = new FileInfo(docFile.DirectoryName + "\\" + includeFileName);
                  string newLine = "#include \"../../H/" + incFile.Name + "\"" + comment;
                  Replace(line, newLine, ref opened, pitem);
                  replaced = true;
                }
              }
              if (!replaced && splitted.Length == 1)
              {
                if (File.Exists(docFile.DirectoryName + "\\..\\..\\..\\H\\" + includeFileName) &&
                   !File.Exists(docFile.DirectoryName + "\\" + includeFileName))
                {
                  FileInfo incFile = new FileInfo(docFile.DirectoryName + "\\" + includeFileName);
                  string newLine = "#include \"../../../H/" + incFile.Name + "\"" + comment;
                  Replace(line, newLine, ref opened, pitem);
                  replaced = true;
                }
              }
              if (!replaced && splitted.Length == 2)
              {
                if (File.Exists(ToBackslash(docFile.DirectoryName + "\\..\\" + splitted[0] + "\\" + includeFileName)))
                {
                  FileInfo incFile = new FileInfo(docFile.DirectoryName + "\\" + includeFileName);
                  string newLine = "#include \"../" + splitted[0] + "/" + incFile.Name + "\"" + comment;
                  Replace(line, newLine, ref opened, pitem);
                  replaced = true;
                }
              }
              if (!replaced && splitted.Length == 3)
              {
                if (File.Exists(ToBackslash(docFile.DirectoryName + "\\..\\..\\" + splitted[0] + "\\" + splitted[1] + "\\" + includeFileName)))
                {
                  FileInfo incFile = new FileInfo(docFile.DirectoryName + "\\" + includeFileName);
                  string newLine = "#include \"../../" + splitted[0] + "/" + splitted[1] + "/" + incFile.Name + "\"" + comment;
                  Replace(line, newLine, ref opened, pitem);
                  replaced = true;
                }
              }
              if (!replaced && splitted.Length > 1 && line.Contains("\\"))
              {
                Replace(line, line.Replace("\\", "/"), ref opened, pitem);
                replaced = true;
              }
              if (!replaced)
              {
                if (File.Exists(docFile.DirectoryName + "\\" + include))
                {
                  FileInfo incFile = new FileInfo(docFile.DirectoryName + "\\" + include);
                  string realCaseSensitiveName = Path.GetFileName(Directory.GetFiles(Path.GetDirectoryName(incFile.FullName), Path.GetFileName(incFile.FullName))[0]);
                  if (realCaseSensitiveName != splitted[splitted.Length - 1])
                  {
                    Replace(line, line.Replace(splitted[splitted.Length - 1], realCaseSensitiveName), ref opened, pitem);
                    replaced = true;
                  }
                }
              }
            }
          }
          if (firstHeader)
          {
            if (docFile.Extension.ToLower() == ".c" ||
                docFile.Extension.ToLower() == ".cpp")
            {
              AddHeader("<vc_pragma.h>", 1, ref opened, pitem);
            }
          }
        }
      }
      catch (Exception ex)
      {
        VsShellUtilities.ShowMessageBox(this.ServiceProvider, ex.Message, "VSIXIvson", OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
      }
    }
  }
}
