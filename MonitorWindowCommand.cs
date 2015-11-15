//------------------------------------------------------------------------------
// <copyright file="MonitorWindowCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using System.Timers;
using System.Text;
using System.IO;
using EnvDTE;
using EnvDTE80;

namespace VSIXIvson
{
  //public struct BuildInfo
  //{
  //  public BuildInfo(string n, long b, long e, bool s)
  //  {
  //    name = n;
  //    begin = b;
  //    end = e;
  //    success = s;
  //  }
  //  public long begin;
  //  public long end;
  //  public string name;
  //  public bool success;
  //}

  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class MonitorWindowCommand
  {
    /// <summary>
    /// Command ID.
    /// </summary>
    public const int CommandId = 4129;

    /// <summary>
    /// Command menu group (command set GUID).
    /// </summary>
    public static readonly Guid CommandSet = new Guid("cf5988aa-595f-4ae0-8831-d38ec20b3047");

    /// <summary>
    /// VS Package that provides this command, not null.
    /// </summary>
    private readonly Package package;

    //public DTE2 _applicationObject;
    //public AddIn _addInInstance;
    //public DateTime buildTime;
    //public OutputWindowPane paneWindow;
    //public MonitorWindow controlWindow;
    //public EnvDTE.BuildEvents buildEvents;
    //public EnvDTE.SolutionEvents solutionEvents;
    //public Dictionary<string, DateTime> currentBuilds = new Dictionary<string, DateTime>();
    //public List<BuildInfo> finishedBuilds = new List<BuildInfo>();
    //public static string timeFormat = "HH:mm:ss";
    //public static string addinName = "VSBuildMonitor";
    //public static string commandToggle = "ToggleCPPH";
    //public static string commandFixIncludes = "FixIncludes";
    //public static string commandFindReplaceGUIDsInSelection = "FindReplaceGUIDsInSelection";
    //public string logFileName;
    //public int maxParallelBuilds = 0;
    //public int allProjectsCount = 0;
    //public int outputCounter = 0;
    //public Timer timer = new Timer();

    /// <summary>
    /// Initializes a new instance of the <see cref="MonitorWindowCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    private MonitorWindowCommand(Package package)
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
                var menuItem = new MenuCommand(this.ShowToolWindow, menuCommandID);
                commandService.AddCommand(menuItem);
            }
            //DTE2 dte = (DTE2)(package as IServiceProvider).GetService(typeof(SDTE));
            //solutionEvents = dte.Events.SolutionEvents;
            //solutionEvents.AfterClosing += new _dispSolutionEvents_AfterClosingEventHandler(solutionEvents_AfterClosing);
            //buildEvents = dte.Events.BuildEvents;
            //buildEvents.OnBuildBegin += new _dispBuildEvents_OnBuildBeginEventHandler(BuildEvents_OnBuildBegin);
            //buildEvents.OnBuildDone += new _dispBuildEvents_OnBuildDoneEventHandler(BuildEvents_OnBuildDone);
            //buildEvents.OnBuildProjConfigBegin += new _dispBuildEvents_OnBuildProjConfigBeginEventHandler(BuildEvents_OnBuildProjConfigBegin);
            //buildEvents.OnBuildProjConfigDone += new _dispBuildEvents_OnBuildProjConfigDoneEventHandler(BuildEvents_OnBuildProjConfigDone);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static MonitorWindowCommand Instance
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets the service provider from the owner package.
    /// </summary>
    public IServiceProvider ServiceProvider
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
      Instance = new MonitorWindowCommand(package);
    }

    /// <summary>
    /// Shows the tool window when the menu item is clicked.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event args.</param>
    private void ShowToolWindow(object sender, EventArgs e)
    {
      // Get the instance number 0 of this tool window. This window is single instance so this instance
      // is actually the only one.
      // The last flag is set to true so that if the tool window does not exists it will be created.
      ToolWindowPane pane = this.package.FindToolWindow(typeof(MonitorWindow), 0, true);
      //controlWindow = pane as MonitorWindow;
      if ((null == pane) || (null == pane.Frame))
      {
        throw new NotSupportedException("Cannot create tool window");
      }

      IVsWindowFrame windowFrame = (IVsWindowFrame)pane.Frame;
      Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
    }

    //void solutionEvents_AfterClosing()
    //{
    //  currentBuilds.Clear();
    //  finishedBuilds.Clear();
    //  if (paneWindow != null)
    //  {
    //    paneWindow.Clear();
    //    if (controlWindow != null)
    //    {
    //      //controlWindow.;
    //    }
    //  }
    //}

    //void BuildEvents_OnBuildProjConfigDone(string Project, string ProjectConfig, string Platform, string SolutionConfig, bool Success)
    //{
    //  string key = MakeKey(Project, ProjectConfig, Platform);
    //  if (currentBuilds.ContainsKey(key))
    //  {
    //    outputCounter++;
    //    DateTime start = new DateTime(currentBuilds[key].Ticks - buildTime.Ticks);
    //    currentBuilds.Remove(key);
    //    DateTime end = new DateTime(DateTime.Now.Ticks - buildTime.Ticks);
    //    finishedBuilds.Add(new BuildInfo(key, start.Ticks, end.Ticks, Success));
    //    TimeSpan s = end - start;
    //    DateTime t = new DateTime(s.Ticks);
    //    StringBuilder b = new StringBuilder(outputCounter.ToString("D3"));
    //    b.Append(" ");
    //    b.Append(key);
    //    int space = 50 - key.Length;
    //    if (space > 0)
    //    {
    //      b.Append(' ', space);
    //    }
    //    b.Append(" \t");
    //    b.Append(start.ToString(timeFormat));
    //    b.Append("\t");
    //    b.Append(t.ToString(timeFormat));
    //    b.Append("\n");
    //    if (paneWindow != null)
    //    {
    //      paneWindow.OutputString(b.ToString());
    //      if (controlWindow != null)
    //      {
    //        //controlWindow.Refresh();
    //      }
    //    }
    //  }
    //}

    //string MakeKey(string Project, string ProjectConfig, string Platform)
    //{
    //  FileInfo fi = new FileInfo(Project);
    //  string key = fi.Name + "|" + ProjectConfig + "|" + Platform;
    //  return key;
    //}

    //void BuildEvents_OnBuildProjConfigBegin(string Project, string ProjectConfig, string Platform, string SolutionConfig)
    //{
    //  string key = MakeKey(Project, ProjectConfig, Platform);
    //  currentBuilds[key] = DateTime.Now;
    //  if (currentBuilds.Count > maxParallelBuilds)
    //  {
    //    maxParallelBuilds = currentBuilds.Count;
    //  }
    //}

    //void BuildEvents_OnBuildDone(vsBuildScope Scope, vsBuildAction Action)
    //{
    //  timer.Enabled = false;
    //  //controlWindow.isBuilding = false;
    //  TimeSpan s = DateTime.Now - buildTime;
    //  DateTime t = new DateTime(s.Ticks);
    //  string msg = "Build Total Time: " + t.ToString(timeFormat) + ", max. number of parallel builds: " + maxParallelBuilds.ToString() + "\n";
    //  if (paneWindow != null)
    //  {
    //    _applicationObject.ToolWindows.OutputWindow.ActivePane.OutputString(msg);
    //    if (controlWindow != null)
    //    {
    //      //controlWindow.Refresh();
    //    }
    //  }
    //}

    //int GetProjectsCount(Project project)
    //{
    //  int count = 0;
    //  if (project != null)
    //  {
    //    if (project.FullName.ToLower().EndsWith(".vcxproj") ||
    //        project.FullName.ToLower().EndsWith(".csproj") ||
    //        project.FullName.ToLower().EndsWith(".vbproj"))
    //      count = 1;
    //    if (project.ProjectItems != null)
    //    {
    //      foreach (ProjectItem projectItem in project.ProjectItems)
    //      {
    //        count += GetProjectsCount(projectItem.SubProject);
    //      }
    //    }
    //  }
    //  return count;
    //}

    //void BuildEvents_OnBuildBegin(vsBuildScope Scope, vsBuildAction Action)
    //{
    //  buildTime = DateTime.Now;
    //  maxParallelBuilds = 0;
    //  allProjectsCount = 0;
    //  foreach (Project project in _applicationObject.Solution.Projects)
    //    allProjectsCount += GetProjectsCount(project);
    //  outputCounter = 0;
    //  timer.Enabled = true;
    //  timer.Interval = 1000;
    //  timer.Elapsed += new ElapsedEventHandler(timer_Tick);
    //  currentBuilds.Clear();
    //  finishedBuilds.Clear();
    //  if (paneWindow != null)
    //  {
    //    paneWindow.Clear();
    //    if (controlWindow != null)
    //    {
    //      //controlWindow.scrollLast = true;
    //      //controlWindow.isBuilding = true;
    //      //controlWindow.Refresh();
    //    }
    //  }
    //}

    //public long PercentageProcessorUse()
    //{
    //  long percentage = 0;
    //  if (maxParallelBuilds > 0)
    //  {
    //    long nowTicks = DateTime.Now.Ticks;
    //    long maxTick = 0;
    //    long totTicks = 0;
    //    foreach (BuildInfo info in finishedBuilds)
    //    {
    //      totTicks += info.end - info.begin;
    //      if (info.end > maxTick)
    //      {
    //        maxTick = info.end;
    //      }
    //    }
    //    foreach (DateTime start in currentBuilds.Values)
    //    {
    //      maxTick = nowTicks - buildTime.Ticks;
    //      totTicks += nowTicks - start.Ticks;
    //    }
    //    totTicks /= maxParallelBuilds;
    //    if (maxTick > 0)
    //    {
    //      percentage = totTicks * 100 / maxTick;
    //    }
    //  }
    //  return percentage;
    //}

    //void timer_Tick(object sender, ElapsedEventArgs e)
    //{
    //  if (paneWindow != null)
    //  {
    //    if (controlWindow != null)
    //    {
    //      //controlWindow.Refresh();
    //    }
    //  }
    //}
  }
}
