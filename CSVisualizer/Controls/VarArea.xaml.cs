using CSVisualizer.Classes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CSVisualizer.Modules;

namespace CSVisualizer.Controls
{
    /// <summary>
    /// VarArea.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class VarArea : UserControl
    {
        public VarArea(string name, string type, string value)
        {
            InitializeComponent();

            SetContents(name, type, value);
        }

        public VarArea(Guid objGuid, List<CSDV_VarInfo> fields)
        {
            InitializeComponent();
            
            SetContents(objGuid, fields);
        }

        public void SetContents(string name, string type, string value)
        {
            this.Name = name;
            this.Header.Text = $"{name}: {type} = {value}";

            Size s = CalculateTextArea(Header);

            this.Width = s.Width + canv.Margin.Left * 2;
            this.Height = s.Height + canv.Margin.Top * 2;
        }

        public Tuple<Point, Size> GetContentPositionSize(string fieldName)
        {
            var tb = this.FindName(fieldName) as TextBlock;
            if (tb == null)
                return null;
            return new Tuple<Point, Size>(
                new Point(Canvas.GetLeft(tb) + canv.Margin.Left, Canvas.GetTop(tb) + canv.Margin.Top),
                new Size(tb.ActualWidth, tb.ActualHeight)
                );
        }

        public void SetContents(Guid objGuid, List<CSDV_VarInfo> fields)
        {
            double minHeight = 0;
            double minWidth = 0;

            canv.Children.Clear();

            this.Header.Text = $"[{objGuid.Shorten()}]";
            canv.Children.Add(this.Header);

            Size s = CalculateTextArea(Header);
            minHeight += s.Height;
            minWidth = s.Width;

            foreach (var f in fields)
            {
                TextBlock tb = new TextBlock();
                tb.Name = f.Name;

                if (f.Value?.GetType() == typeof(Guid))
                    tb.Text = $"{f.Name}: {f.Type} = {((Guid)f.Value).Shorten()}";
                else
                    tb.Text = $"{f.Name}: {f.Type} = {f.Value}";
                    
                Canvas.SetTop(tb, minHeight);

                canv.Children.Add(tb);

                s = CalculateTextArea(tb);
                minHeight += s.Height;
                minWidth = Math.Max(s.Width, minWidth);
            }

            //rect.Width = minWidth;
            //rect.Height = minHeight;

            //this.Width = rect.Width;
            //this.Height = rect.Height;
            this.Width = minWidth + canv.Margin.Left * 2;
            this.Height = minHeight + canv.Margin.Top * 2;
        }

        private Size CalculateTextArea(TextBlock textBlock)
        {
            FormattedText ft = new FormattedText(textBlock.Text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch), textBlock.FontSize, Brushes.Black);
            return new Size(ft.Width, ft.Height);
        }
    }
}
