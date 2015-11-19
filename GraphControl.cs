using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXIvson
{
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Media;
  using EnvDTE;
  using EnvDTE80;

  public class GraphControl : ScrollViewer
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

    public GraphControl()
    {
      isDark = GetThemeId().ToLower() == "1ded0138-47ce-435e-84ef-9ec1f439b749";

      blackBrush = new SolidColorBrush(isDark ? Colors.White : Colors.Black);
      Background = new SolidColorBrush(isDark ? Colors.Black : Colors.White);
      greenSolidBrush = new SolidColorBrush(isDark ? Colors.LightGreen : Colors.DarkGreen);
      redSolidBrush = new SolidColorBrush(isDark ? Color.FromRgb(165, 33, 33) : Colors.DarkRed);
      blueSolidBrush = new SolidColorBrush(isDark ? Color.FromRgb(50, 152, 204) : Colors.DarkBlue);
      blackPen = new Pen(new SolidColorBrush(isDark ? Colors.White : Colors.Black), 1.0);
      grid = new Pen(new SolidColorBrush(isDark ? Color.FromRgb(66, 66, 66) : Colors.LightGray), 1.0);

      Instance = this;
    }

    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static GraphControl Instance
    {
      get;
      private set;
    }


    protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
    {
      if (RenderSize.Width < 10 || RenderSize.Height < 10)
      {
        return;
      }

      DTE2 dte = (DTE2)MonitorWindowCommand.Instance.ServiceProvider.GetService(typeof(DTE));

      MonitorWindowCommand host = MonitorWindowCommand.Instance;

      int rowHeight = (int)FontFamily.LineSpacing + 1;
      int linesCount = host.currentBuilds.Count + host.finishedBuilds.Count + 1;
      int totalHeight = rowHeight * linesCount;

      //AutoScrollMinSize = new Size(this.AutoScrollMinSize.Width, totalHeight);
      //if (scrollLast)
      //{
      //    int newPos = totalHeight - Size.Height;
      //    AutoScrollPosition = new Point(0, newPos);
      //}

      //Matrix mx = new Matrix(1, 0, 0, 1, AutoScrollPosition.X, AutoScrollPosition.Y);
      //e.Graphics.Transform = mx;

      //int maxStringLength = 0;
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
      //int i;
      //bool atLeastOneError = false;
      //for (i = 0; i < host.finishedBuilds.Count; i++)
      //{
      //    int l = graphicsObj.MeasureString(host.finishedBuilds[i].name, Font).ToSize().Width;
      //    t = host.finishedBuilds[i].end;
      //    atLeastOneError = atLeastOneError || !host.finishedBuilds[i].success;
      //    if (t > maxTick)
      //    {
      //        maxTick = t;
      //    }
      //    if (l > maxStringLength)
      //    {
      //        maxStringLength = l;
      //    }
      //}
      //foreach (KeyValuePair<string, DateTime> item in host.currentBuilds)
      //{
      //    int l = graphicsObj.MeasureString(item.Key, Font).ToSize().Width;
      //    if (l > maxStringLength)
      //    {
      //        maxStringLength = l;
      //    }
      //}
      //if (isBuilding)
      //{
      //    maxTick = (maxTick / tickStep + 1) * tickStep;
      //}
      //maxStringLength += 5 + graphicsObj.MeasureString(i.ToString(intFormat) + " ", Font).ToSize().Width;

      //Brush greenGradientBrush = new LinearGradientBrush(new Point(0, 0), new Point(0, rowHeight), Colors.MediumSeaGreen, Colors.DarkGreen);
      //Brush redGradientBrush = new LinearGradientBrush(new Point(0, 0), new Point(0, rowHeight), Colors.IndianRed, Colors.DarkRed);
      //for (i = 0; i < host.finishedBuilds.Count; i++)
      //{
      //    Brush solidBrush = host.finishedBuilds[i].success ? greenSolidBrush : redSolidBrush;
      //    Brush gradientBrush = host.finishedBuilds[i].success ? greenGradientBrush : redGradientBrush;
      //    DateTime span = new DateTime(host.finishedBuilds[i].end - host.finishedBuilds[i].begin);
      //    string time = span.ToString(Connect.timeFormat);
      //    graphicsObj.DrawString((i + 1).ToString(intFormat) + " " + host.finishedBuilds[i].name, Font, solidBrush, 1, i * rowHeight);
      //    Rectangle r = new Rectangle();
      //    r.X = maxStringLength + (int)((host.finishedBuilds[i].begin) * (long)(DisplayRectangle.Size.Width - maxStringLength) / maxTick);
      //    r.Width = maxStringLength + (int)((host.finishedBuilds[i].end) * (long)(DisplayRectangle.Size.Width - maxStringLength) / maxTick) - r.X;
      //    if (r.Width == 0)
      //    {
      //        r.Width = 1;
      //    }
      //    r.Y = i * rowHeight + 1;
      //    r.Height = rowHeight - 1;
      //    graphicsObj.FillRectangle(gradientBrush, r);
      //    int timeLen = graphicsObj.MeasureString(time, Font).ToSize().Width;
      //    if (r.Width > timeLen)
      //    {
      //        graphicsObj.DrawString(time, Font, whiteBrush, r.Right - timeLen, i * rowHeight);
      //    }
      //    graphicsObj.DrawLine(grid, new Point(0, i * rowHeight), new Point(DisplayRectangle.Size.Width, i * rowHeight));
      //}

      //Brush blueGradientBrush = new LinearGradientBrush(new Point(0, 0), new Point(0, rowHeight), Colors.LightBlue, Colors.DarkBlue);
      //foreach (KeyValuePair<string, DateTime> item in host.currentBuilds)
      //{
      //    graphicsObj.DrawString((i + 1).ToString(intFormat) + " " + item.Key, Font, blueSolidBrush, 1, i * rowHeight);
      //    Rectangle r = new Rectangle();
      //    r.X = maxStringLength + (int)((item.Value.Ticks - host.buildTime.Ticks) * (long)(DisplayRectangle.Size.Width - maxStringLength) / maxTick);
      //    r.Width = maxStringLength + (int)((nowTick - host.buildTime.Ticks) * (long)(DisplayRectangle.Size.Width - maxStringLength) / maxTick) - r.X;
      //    if (r.Width == 0)
      //    {
      //        r.Width = 1;
      //    }
      //    r.Y = i * rowHeight + 1;
      //    r.Height = rowHeight - 1;
      //    graphicsObj.FillRectangle(blueGradientBrush, r);
      //    graphicsObj.DrawLine(grid, new Point(0, i * rowHeight), new Point(DisplayRectangle.Size.Width, i * rowHeight));
      //    i++;
      //}

      //if (host.currentBuilds.Count > 0 || host.finishedBuilds.Count > 0)
      //{
      //    string line = "";
      //    if (isBuilding)
      //    {
      //        line = "Building...";
      //    }
      //    else
      //    {
      //        line = "Done";
      //    }
      //    if (host.maxParallelBuilds > 0)
      //    {
      //        line += " (" + host.PercentageProcessorUse().ToString() + "% of " + host.maxParallelBuilds.ToString() + " CPUs)";
      //    }
      //    graphicsObj.DrawString(line, Font, blackBrush, 1, i * rowHeight);
      //}
      //graphicsObj.DrawLine(grid, new Point(maxStringLength, 0), new Point(maxStringLength, i * rowHeight));
      //graphicsObj.DrawLine(blackPen, new Point(0, i * rowHeight), new Point(DisplayRectangle.Size.Width, i * rowHeight));
      //if (host.allProjectsCount > 0)
      //    graphicsObj.DrawLine(new Pen(atLeastOneError ? Color.Red : Color.Green, 1), new Point(0, (i + 1) * rowHeight - 1),
      //      new Point(DisplayRectangle.Size.Width * host.finishedBuilds.Count / host.allProjectsCount, (i + 1) * rowHeight - 1));
      //DateTime dt = new DateTime(0);
      //graphicsObj.DrawString(dt.ToString(Connect.timeFormat), Font, blackBrush, maxStringLength, i * rowHeight);

      //dt = new DateTime(maxTick);
      //string s = dt.ToString(Connect.timeFormat);
      //int m = graphicsObj.MeasureString(s, Font).ToSize().Width;
      //graphicsObj.DrawString(s, Font, blackBrush, DisplayRectangle.Size.Width - m, i * rowHeight);
    }

    //private void ChartsControl_Scroll(object sender, ScrollEventArgs e)
    //{
    //    if (e.ScrollOrientation == ScrollOrientation.VerticalScroll)
    //    {
    //        if (e.Type == ScrollEventType.LargeIncrement || e.Type == ScrollEventType.SmallIncrement)
    //        {
    //            if (e.NewValue == e.OldValue)
    //            {
    //                scrollLast = true;
    //            }
    //            else
    //            {
    //                scrollLast = false;
    //            }
    //        }
    //        else
    //        {
    //            scrollLast = false;
    //        }
    //    }
    //}

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
