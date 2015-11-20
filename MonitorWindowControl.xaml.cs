//------------------------------------------------------------------------------
// <copyright file="MonitorWindowControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace VSIXIvson
{
  using System.Diagnostics.CodeAnalysis;
  using System.Windows;
  using System.Windows.Controls;
  using Microsoft.VisualStudio.Shell;
  
  /// <summary>
  /// Interaction logic for MonitorWindowControl.
  /// </summary>
  public partial class MonitorWindowControl : UserControl
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="MonitorWindowControl"/> class.
    /// </summary>
    public MonitorWindowControl()
    {
      this.InitializeComponent();
    }

    public Package Package;

    private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
      if (e.ExtentHeightChange != 0)
      {
        ScrollViewer scrollViewer = sender as ScrollViewer;
        if (scrollViewer != null)
        {
          scrollViewer.ScrollToBottom();
        }
      }
    }
  }
}