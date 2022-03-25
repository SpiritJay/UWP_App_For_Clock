using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235
// Consult https://github.com/windows-toolkit/WindowsCommunityToolkit/tree/rel/7.1.0/Microsoft.Toolkit.Uwp.UI.Controls.Core/RadialProgressBar

namespace ClockApp
{
    [TemplatePart(Name = OutlineFigurePartName, Type = typeof(PathFigure))]
    [TemplatePart(Name = OutlineArcPartName, Type = typeof(ArcSegment))]
    [TemplatePart(Name = BarFigurePartName, Type = typeof(PathFigure))]
    [TemplatePart(Name = BarArcPartName, Type = typeof(ArcSegment))]
    public sealed class CustomProgressRing : Control
    {
        private const string OutlineFigurePartName = "OutlineFigurePart";
        private const string OutlineArcPartName = "OutlineArcPart";
        private const string BarFigurePartName = "BarFigurePart";
        private const string BarArcPartName = "BarArcPart";

        private PathFigure outlineFigure;
        private PathFigure barFigure;
        private ArcSegment outlineArc;
        private ArcSegment barArc;

        private bool allTemplatePartsDefined = false;

        public CustomProgressRing()
        {
            this.DefaultStyleKey = typeof(CustomProgressRing);
            SizeChanged += CustomProgressRing_SizeChanged;
        }

        /// <summary>
        /// Update the visual state of the control when its template is changed.
        /// </summary>
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            outlineFigure = GetTemplateChild(OutlineFigurePartName) as PathFigure;
            outlineArc = GetTemplateChild(OutlineArcPartName) as ArcSegment;
            barFigure = GetTemplateChild(BarFigurePartName) as PathFigure;
            barArc = GetTemplateChild(BarArcPartName) as ArcSegment;

            allTemplatePartsDefined = outlineFigure != null && outlineArc != null && barFigure != null && barArc != null;

            UpdateAll();
        }

        /// <summary>
        /// 控件尺寸改变时发生
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CustomProgressRing_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //throw new NotImplementedException();
            var ob = sender as CustomProgressRing;
            ob.UpdateAll();
        }

        /// <summary>
        /// 获取或设置线条粗细的值
        /// </summary>
        public double Thickness
        {
            get { return (double)GetValue(ThicknessProperty); }
            set { SetValue(ThicknessProperty, value); }
        }

        /// <summary>
        /// Thickness的依赖属性
        /// </summary>
        public static readonly DependencyProperty ThicknessProperty =
            DependencyProperty.Register("Thickness", typeof(double), typeof(CustomProgressRing), new PropertyMetadata(0.0, OnThicknessChanged));

        /// <summary>
        /// Thickness值改变时发生
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //throw new NotImplementedException();
            var ob = d as CustomProgressRing;
            ob.UpdateAll();
        }

        /// <summary>
        /// 获取或设置进度条未完成部分的颜色画笔
        /// </summary>
        public Brush Outline
        {
            get { return (Brush)GetValue(OutlineProperty); }
            set { SetValue(OutlineProperty, value); }
        }

        /// <summary>
        /// Outline的依赖属性
        /// </summary>
        public static readonly DependencyProperty OutlineProperty =
            DependencyProperty.Register("Outline", typeof(Brush), typeof(CustomProgressRing), new PropertyMetadata(new SolidColorBrush(Colors.Gray)));

        /// <summary>
        /// 获取或设置已完成的值
        /// </summary>
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set
            {
                StartValueChangedStoryboard(Value, Math.Min(Math.Max(value, MinValue), MaxValue));
                SetValue(ValueProperty, Math.Min(Math.Max(value, MinValue), MaxValue));
            }
        }

        /// <summary>
        /// Value的依赖属性
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(CustomProgressRing), new PropertyMetadata(0.0, OnValueChanged));

        /// <summary>
        /// Value值改变时发生
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ob = d as CustomProgressRing;
            ob.UpdateSegment();
        }

        private void StartValueChangedStoryboard(double oldValue, double newValue)
        {
            var animation = new DoubleAnimation
            {
                EnableDependentAnimation = true,
                Duration = TimeSpan.FromSeconds(1),
                From = oldValue,
                To = newValue
            };

            Storyboard.SetTargetProperty(animation, nameof(Value));
            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            storyboard.FillBehavior = FillBehavior.Stop;
            Storyboard.SetTarget(storyboard.Children[0] as DoubleAnimation, this);
            storyboard.Begin();
        }

        /// <summary>
        /// 获取或设置最小值
        /// </summary>
        public double MinValue
        {
            get { return (double)GetValue(MinValueProperty); }
            set
            {
                SetValue(MinValueProperty, Math.Min(MaxValue, value));
                Value = Math.Max(Value, value);
            }
        }

        /// <summary>
        /// MinValue的依赖属性
        /// </summary>
        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(double), typeof(CustomProgressRing), new PropertyMetadata(0.0, OnMinValueChanged));

        /// <summary>
        /// MinValue值改变时发生
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnMinValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ob = d as CustomProgressRing;
            ob.UpdateSegment();
        }

        /// <summary>
        /// 获取或设置最大值
        /// </summary>
        public double MaxValue
        {
            get { return (double)GetValue(MaxValueProperty); }
            set
            {
                SetValue(MaxValueProperty, Math.Max(MinValue, value));
                Value = Math.Min(Value, MaxValue);
            }
        }

        /// <summary>
        /// MaxValue的依赖属性
        /// </summary>
        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(double), typeof(CustomProgressRing), new PropertyMetadata(100.0, OnMaxValueChanged));

        /// <summary>
        /// MaxValue值改变时发生
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnMaxValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ob = d as CustomProgressRing;
            ob.UpdateSegment();
        }

        /// <summary>
        /// 线段开始端点形状
        /// </summary>
        public PenLineCap StartLineCap
        {
            get { return (PenLineCap)GetValue(StartLineCapProperty); }
            set { SetValue(StartLineCapProperty, value); }
        }

        /// <summary>
        /// StartLineCap的依赖属性
        /// </summary>
        public static readonly DependencyProperty StartLineCapProperty =
            DependencyProperty.Register("StartLineCap", typeof(PenLineCap), typeof(CustomProgressRing), new PropertyMetadata(PenLineCap.Round, OnStartLineCapChanged));

        /// <summary>
        /// StartLineCap值改变时发生
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnStartLineCapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        /// <summary>
        /// 线段结束端点形状
        /// </summary>
        public PenLineCap EndLineCap
        {
            get { return (PenLineCap)GetValue(EndLineCapProperty); }
            set { SetValue(EndLineCapProperty, value); }
        }

        /// <summary>
        /// EndLineCap的依赖属性
        /// </summary>
        public static readonly DependencyProperty EndLineCapProperty =
            DependencyProperty.Register("EndLineCap", typeof(PenLineCap), typeof(CustomProgressRing), new PropertyMetadata(PenLineCap.Round, OnEndLineCapChanged));

        /// <summary>
        /// EndLineCap值改变时发生
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnEndLineCapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        /// <summary>
        /// 外形发生改变时应调用此函数重绘 >> Width,Height,Thickness
        /// </summary>
        private void UpdateAll()
        {
            if (!allTemplatePartsDefined)
            {
                return;
            }

            double newStartPointX = Width / 2.0;
            double newStartPointY = Thickness / 2.0;
            barFigure.StartPoint = outlineFigure.StartPoint = new Point(newStartPointX, newStartPointY);
            outlineArc.Point = new Point(newStartPointX - 0.0001, newStartPointY);

            double newSizeX = (Width - Thickness) / 2.0;
            double newSizeY = (Height - Thickness) / 2.0;
            barArc.Size = outlineArc.Size = new Size(newSizeX, newSizeY);

            UpdateSegment();
        }

        /// <summary>
        /// 参数改变而外形不变时应调用此函数重绘 >> Value,MinValue,MaxValue
        /// </summary>
        private void UpdateSegment()
        {
            if (!allTemplatePartsDefined)
            {
                return;
            }

            double angle = (Value - MinValue) * Math.PI * 2.0 / (MaxValue - MinValue);
            barArc.IsLargeArc = angle >= Math.PI;
            double a = barArc.Size.Width;
            double b = barArc.Size.Height;
            double PointX = a * Math.Sin(angle) + Width / 2.0;
            if (angle == Math.PI * 2.0)
            {
                PointX -= 0.0001;
            }
            double PointY = -1 * b * Math.Cos(angle) + Height / 2.0;
            barArc.Point = new Point(PointX, PointY);
        }
    }
}
