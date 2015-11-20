using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Globalization;

namespace VSIXIvson
{
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Media;
  using EnvDTE;
  using EnvDTE80;

  public class GraphControl : ContentControl
  {
    public bool scrollLast = true;
    public string intFormat = "D3";
    public bool isBuilding = false;
    private bool isDark = false;

    Brush blueSolidBrush = new SolidColorBrush(Colors.DarkBlue);
    Pen blackPen = new Pen(new SolidColorBrush(Colors.Black), 1.0);
    Brush blackBrush = new SolidColorBrush(Colors.Black);
    Brush greenSolidBrush = new SolidColorBrush(Colors.DarkGreen);
    Brush redSolidBrush = new SolidColorBrush(Colors.DarkRed);
    Brush whiteBrush = new SolidColorBrush(Colors.White);
    Pen grid = new Pen(new SolidColorBrush(Colors.LightGray), 1.0);
    Typeface fontFace = new Typeface("Segoe UI");
    double emSize = 11.0;
    public Timer timer = new Timer();

    public GraphControl()
    {
      isDark = GetThemeId().ToLower() == "1ded0138-47ce-435e-84ef-9ec1f439b749";

      blackBrush = new SolidColorBrush(isDark ? Colors.White : Colors.Black);
      Background = Brushes.Transparent;
      greenSolidBrush = new SolidColorBrush(isDark ? Colors.LightGreen : Colors.DarkGreen);
      redSolidBrush = new SolidColorBrush(isDark ? Color.FromRgb(165, 33, 33) : Colors.DarkRed);
      blueSolidBrush = new SolidColorBrush(isDark ? Color.FromRgb(50, 152, 204) : Colors.DarkBlue);
      blackPen = new Pen(new SolidColorBrush(isDark ? Colors.White : Colors.Black), 1.0);
      grid = new Pen(new SolidColorBrush(isDark ? Color.FromRgb(66, 66, 66) : Colors.LightGray), 1.0);

      Instance = this;
    }

    public static GraphControl Instance
    {
      get;
      private set;
    }


    protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
    {
      if (RenderSize.Width < 10.0 || RenderSize.Height < 10.0)
      {
        return;
      }

      DTE2 dte = (DTE2)MonitorWindowCommand.Instance.ServiceProvider.GetService(typeof(DTE));

      MonitorWindowCommand host = MonitorWindowCommand.Instance;

      FormattedText dummyText = new FormattedText("A0", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontFace, emSize, Brushes.Black);

      double rowHeight = dummyText.Height + 1;
      int linesCount = host.currentBuilds.Count + host.finishedBuilds.Count + 1;
      double totalHeight = rowHeight * linesCount;

      Height = Math.Max(totalHeight, RenderSize.Height);

      double maxStringLength = 0.0;
      long tickStep = 100000000;
      long maxTick = tickStep;
      long nowTick = DateTime.Now.Ticks;
      long t = nowTick - host.buildTime.Ticks;
      if (host.currentBuilds.Count > 0)
      {
        if (t > maxTick)
        {
          maxTick = t;
        }
      }
      int i;
      bool atLeastOneError = false;
      for (i = 0; i < host.finishedBuilds.Count; i++)
      {
        FormattedText iname = new FormattedText(host.finishedBuilds[i].name, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontFace, emSize, Brushes.Black);
        double l = iname.Width;
        t = host.finishedBuilds[i].end;
        atLeastOneError = atLeastOneError || !host.finishedBuilds[i].success;
        if (t > maxTick)
        {
          maxTick = t;
        }
        if (l > maxStringLength)
        {
          maxStringLength = l;
        }
      }
      foreach (KeyValuePair<string, DateTime> item in host.currentBuilds)
      {
        FormattedText iname = new FormattedText(item.Key, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontFace, emSize, Brushes.Black);
        double l = iname.Width;
        if (l > maxStringLength)
        {
          maxStringLength = l;
        }
      }
      if (isBuilding)
      {
        maxTick = (maxTick / tickStep + 1) * tickStep;
      }
      FormattedText iint = new FormattedText(i.ToString(intFormat) + " ", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontFace, emSize, Brushes.Black);
      maxStringLength += 5 + iint.Width;

      Brush greenGradientBrush = new LinearGradientBrush(Colors.MediumSeaGreen, Colors.DarkGreen, new Point(0, 0), new Point(0, rowHeight));
      Brush redGradientBrush = new LinearGradientBrush(Colors.IndianRed, Colors.DarkRed, new Point(0, 0), new Point(0, rowHeight));
      for (i = 0; i < host.finishedBuilds.Count; i++)
      {
        Brush solidBrush = host.finishedBuilds[i].success ? greenSolidBrush : redSolidBrush;
        Brush gradientBrush = host.finishedBuilds[i].success ? greenGradientBrush : redGradientBrush;
        DateTime span = new DateTime(host.finishedBuilds[i].end - host.finishedBuilds[i].begin);
        string time = span.ToString(MonitorWindowCommand.timeFormat);
        FormattedText itext = new FormattedText((i + 1).ToString(intFormat) + " " + host.finishedBuilds[i].name, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontFace, emSize, solidBrush);
        drawingContext.DrawText(itext, new Point(1, i * rowHeight));
        Rect r = new Rect();
        r.X = maxStringLength + (int)((host.finishedBuilds[i].begin) * (long)(RenderSize.Width - maxStringLength) / maxTick);
        r.Width = maxStringLength + (int)((host.finishedBuilds[i].end) * (long)(RenderSize.Width - maxStringLength) / maxTick) - r.X;
        if (r.Width == 0)
        {
          r.Width = 1;
        }
        r.Y = i * rowHeight + 1;
        r.Height = rowHeight - 1;
        drawingContext.DrawRectangle(gradientBrush, null, r);
        FormattedText itime = new FormattedText(time, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontFace, emSize, whiteBrush);
        double timeLen = itime.Width;
        if (r.Width > timeLen)
        {
          drawingContext.DrawText(itime, new Point(r.Right - timeLen, i * rowHeight));
        }
        drawingContext.DrawLine(grid, new Point(0, i * rowHeight), new Point(RenderSize.Width, i * rowHeight));
      }

      Brush blueGradientBrush = new LinearGradientBrush(Colors.LightBlue, Colors.DarkBlue, new Point(0, 0), new Point(0, rowHeight));
      foreach (KeyValuePair<string, DateTime> item in host.currentBuilds)
      {
        FormattedText itext = new FormattedText((i + 1).ToString(intFormat) + " " + item.Key, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontFace, emSize, blueSolidBrush);
        drawingContext.DrawText(itext, new Point(1, i * rowHeight));
        Rect r = new Rect();
        r.X = maxStringLength + (int)((item.Value.Ticks - host.buildTime.Ticks) * (long)(RenderSize.Width - maxStringLength) / maxTick);
        r.Width = maxStringLength + (int)((nowTick - host.buildTime.Ticks) * (long)(RenderSize.Width - maxStringLength) / maxTick) - r.X;
        if (r.Width == 0)
        {
          r.Width = 1;
        }
        r.Y = i * rowHeight + 1;
        r.Height = rowHeight - 1;
        drawingContext.DrawRectangle(blueGradientBrush, null, r);
        drawingContext.DrawLine(grid, new Point(0, i * rowHeight), new Point(RenderSize.Width, i * rowHeight));
        i++;
      }

      if (host.currentBuilds.Count > 0 || host.finishedBuilds.Count > 0)
      {
        string line = "";
        if (isBuilding)
        {
          line = "Building...";
        }
        else
        {
          line = "Done";
        }
        if (host.maxParallelBuilds > 0)
        {
          line += " (" + host.PercentageProcessorUse().ToString() + "% of " + host.maxParallelBuilds.ToString() + " CPUs)";
        }
        FormattedText itext = new FormattedText(line, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontFace, emSize, blackBrush);
        drawingContext.DrawText(itext, new Point(1, i * rowHeight));
      }
      drawingContext.DrawLine(grid, new Point(maxStringLength, 0), new Point(maxStringLength, i * rowHeight));
      drawingContext.DrawLine(blackPen, new Point(0, i * rowHeight), new Point(RenderSize.Width, i * rowHeight));
      if (host.allProjectsCount > 0)
        drawingContext.DrawLine(new Pen(new SolidColorBrush(atLeastOneError ? Colors.Red : Colors.Green), 1), new Point(0, (i + 1) * rowHeight - 1),
            new Point(RenderSize.Width * host.finishedBuilds.Count / host.allProjectsCount, (i + 1) * rowHeight - 1));
      DateTime dt = new DateTime(0);
      FormattedText zeroTime = new FormattedText(dt.ToString(MonitorWindowCommand.timeFormat), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontFace, emSize, Brushes.Black);
      drawingContext.DrawText(zeroTime, new Point(maxStringLength, i * rowHeight));

      dt = new DateTime(maxTick);
      string s = dt.ToString(MonitorWindowCommand.timeFormat);
      FormattedText maxTime = new FormattedText(s, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontFace, emSize, Brushes.Black);
      double m = maxTime.Width;
      drawingContext.DrawText(maxTime, new Point(RenderSize.Width - m, i * rowHeight));
    }

    private string GetThemeId()
    {
      const string CATEGORY_TEXT_GENERAL = "General";
      const string PROPERTY_NAME_CURRENT_THEME = "CurrentTheme";

      string result;

      result = Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\VisualStudio\14.0\"
         + CATEGORY_TEXT_GENERAL, PROPERTY_NAME_CURRENT_THEME, "").ToString();

      return result;
    }


  }
}
