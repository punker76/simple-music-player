#region File Header

// -------------------------------------------------------------------------------
// 
// This file is part of the WPFSpark project: http://wpfspark.codeplex.com/
//
// Author: Ratish Philip
// 
// WPFSpark v1.1
//
// -------------------------------------------------------------------------------

#endregion

using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SimpleMusicPlayer.Core
{
    /// <summary>
    /// Interaction logic for SprocketControl.xaml
    /// </summary>
    public class SprocketControl : Control, IDisposable
    {
        #region Constants

        private const double DEFAULT_INTERVAL = 60;
        private static readonly Color DEFAULT_TICK_COLOR = Color.FromArgb((byte)255, (byte)58, (byte)58, (byte)58);
        private const double DEFAULT_TICK_WIDTH = 3;
        private const int DEFAULT_TICK_COUNT = 12;
        private const double MINIMUM_INNER_RADIUS = 5;
        private const double MINIMUM_OUTER_RADIUS = 8;
        private readonly Size MINIMUM_CONTROL_SIZE = new Size(28, 28);
        private const double MINIMUM_PEN_WIDTH = 2;
        private const double DEFAULT_START_ANGLE = 270;
        private const double INNER_RADIUS_FACTOR = 0.175;

        private const double OUTER_RADIUS_FACTOR = 0.3125;

        // The Lower limit of the Alpha value (The spokes will be shown in 
        // alpha values ranging from 255 to m_AlphaLowerLimit)
        private const Int32 ALPHA_UPPER_LIMIT = 250;
        private const Int32 ALPHA_LOWER_LIMIT = 0;
        private const double ALPHA_TICK_PERCENTAGE_LOWER_LIMIT = 10;
        private const double DEFAULT_PROGRESS_ALPHA = 10;
        private const double DEFAULT_PROGRESS = 0.0;

        #endregion

        #region Enums

        /// <summary>
        /// Defines the Direction of Rotation
        /// </summary>
        public enum Direction
        {
            CLOCKWISE,
            ANTICLOCKWISE
        }

        #endregion

        #region Structs

        /// <summary>
        /// Stores the details of each spoke
        /// </summary>
        struct Spoke
        {
            public Point StartPoint;
            public Point EndPoint;

            public Spoke(Point pt1, Point pt2)
            {
                this.StartPoint = pt1;
                this.EndPoint = pt2;
            }
        }

        #endregion

        #region Fields

        Point centerPoint = new Point();
        double innerRadius = 0;
        double outerRadius = 0;
        double alphaChange = 0;
        double angleIncrement = 0;
        double renderStartAngle = 0;
        System.Timers.Timer renderTimer = null;
        List<Spoke> spokes = null;

        #endregion

        #region Dependency Properties

        #region Interval

        /// <summary>
        /// Interval Dependency Property
        /// </summary>
        public static readonly DependencyProperty IntervalProperty =
            DependencyProperty.Register(nameof(Interval), typeof(double), typeof(SprocketControl),
                new FrameworkPropertyMetadata(DEFAULT_INTERVAL,
                    OnIntervalChanged));

        /// <summary>
        /// Gets or sets the Interval property. This dependency property 
        /// indicates duration at which the timer for rotation should fire.
        /// </summary>
        public double Interval
        {
            get => (double)this.GetValue(IntervalProperty);
            set => this.SetValue(IntervalProperty, value);
        }

        /// <summary>
        /// Handles changes to the Interval property.
        /// </summary>
        /// <param name="d">SprocketControl</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sprocket = (SprocketControl)d;
            var oldInterval = (double)e.OldValue;
            var newInterval = sprocket.Interval;
            sprocket.OnIntervalChanged(oldInterval, newInterval);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the Interval property.
        /// </summary>
        /// <param name="oldInterval">Old Value</param>
        /// <param name="newInterval">New Value</param>
        protected virtual void OnIntervalChanged(double oldInterval, double newInterval)
        {
            if (this.renderTimer != null)
            {
                var isEnabled = this.renderTimer.Enabled;
                this.renderTimer.Enabled = false;
                this.renderTimer.Interval = newInterval;
                this.renderTimer.Enabled = isEnabled;
            }
        }

        #endregion

        #region IsIndeterminate

        /// <summary>
        /// IsIndeterminate Dependency Property
        /// </summary>
        public static readonly DependencyProperty IsIndeterminateProperty =
            DependencyProperty.Register(nameof(IsIndeterminate), typeof(bool), typeof(SprocketControl),
                new FrameworkPropertyMetadata(true,
                    (OnIsIndeterminateChanged)));

        /// <summary>
        /// Gets or sets the IsIndeterminate property. This dependency property 
        /// indicates whether the SprocketControl's progress is indeterminate or not.
        /// </summary>
        public bool IsIndeterminate
        {
            get => (bool)this.GetValue(IsIndeterminateProperty);
            set => this.SetValue(IsIndeterminateProperty, value);
        }

        /// <summary>
        /// Handles changes to the IsIndeterminate property.
        /// </summary>
        /// <param name="d">SprocketControl</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnIsIndeterminateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (SprocketControl)d;
            var oldIsIndeterminate = (bool)e.OldValue;
            var newIsIndeterminate = target.IsIndeterminate;
            target.OnIsIndeterminateChanged(oldIsIndeterminate, newIsIndeterminate);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the IsIndeterminate property.
        /// </summary>
        /// <param name="oldIsIndeterminate">Old Value</param>
        /// <param name="newIsIndeterminate">New Value</param>
        protected virtual void OnIsIndeterminateChanged(bool oldIsIndeterminate, bool newIsIndeterminate)
        {
            if (oldIsIndeterminate != newIsIndeterminate)
            {
                if ((newIsIndeterminate) && (this.IsVisible))
                {
                    // Start the renderTimer
                    this.Start();
                }
                else
                {
                    // Stop the renderTimer
                    this.Stop();
                    this.InvalidateVisual();
                }
            }
        }

        #endregion

        #region Progress

        /// <summary>
        /// Progress Dependency Property
        /// </summary>
        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register(nameof(Progress), typeof(double), typeof(SprocketControl),
                new FrameworkPropertyMetadata(DEFAULT_PROGRESS,
                    OnProgressChanged,
                    CoerceProgress));

        /// <summary>
        /// Gets or sets the Progress property. This dependency property 
        /// indicates the progress percentage.
        /// </summary>
        public double Progress
        {
            get => (double)this.GetValue(ProgressProperty);
            set => this.SetValue(ProgressProperty, value);
        }

        /// <summary>
        /// Coerces the Progress value so that it stays in the range 0-100
        /// </summary>
        /// <param name="d">SprocketControl</param>
        /// <param name="value">New Value</param>
        /// <returns>Coerced Value</returns>
        private static object CoerceProgress(DependencyObject d, object value)
        {
            var progress = (double)value;

            if (progress < 0.0)
            {
                return 0.0;
            }
            else if (progress > 100.0)
            {
                return 100.0;
            }

            return value;
        }

        /// <summary>
        /// Handles changes to the Progress property.
        /// </summary>
        /// <param name="d">SprocketControl</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sprocket = (SprocketControl)d;
            var oldProgress = (double)e.OldValue;
            var newProgress = sprocket.Progress;
            sprocket.OnProgressChanged(oldProgress, newProgress);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the Progress property.
        /// </summary>
        /// <param name="oldProgress">Old Value</param>
        /// <param name="newProgress">New Value</param>
        protected virtual void OnProgressChanged(double oldProgress, double newProgress)
        {
            this.InvalidateVisual();
        }

        #endregion

        #region Rotation

        /// <summary>
        /// Rotation Dependency Property
        /// </summary>
        public static readonly DependencyProperty RotationProperty =
            DependencyProperty.Register(nameof(Rotation), typeof(Direction), typeof(SprocketControl),
                new FrameworkPropertyMetadata(Direction.CLOCKWISE,
                    OnRotationChanged));

        /// <summary>
        /// Gets or sets the Rotation property. This dependency property 
        /// indicates the direction of Rotation of the SprocketControl.
        /// </summary>
        public Direction Rotation
        {
            get => (Direction)this.GetValue(RotationProperty);
            set => this.SetValue(RotationProperty, value);
        }

        /// <summary>
        /// Handles changes to the Rotation property.
        /// </summary>
        /// <param name="d">SprocketControl</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnRotationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sprocket = (SprocketControl)d;
            var oldRotation = (Direction)e.OldValue;
            var newRotation = sprocket.Rotation;
            sprocket.OnRotationChanged(oldRotation, newRotation);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the Rotation property.
        /// </summary>
        /// <param name="oldRotation">Old Value</param>
        /// <param name="newRotation">New Value</param>
        protected virtual void OnRotationChanged(Direction oldRotation, Direction newRotation)
        {
            // Recalculate the spoke points
            this.CalculateSpokesPoints();
        }

        #endregion

        #region StartAngle

        /// <summary>
        /// StartAngle Dependency Property
        /// </summary>
        public static readonly DependencyProperty StartAngleProperty =
            DependencyProperty.Register(nameof(StartAngle), typeof(double), typeof(SprocketControl),
                new FrameworkPropertyMetadata(DEFAULT_START_ANGLE,
                    OnStartAngleChanged));

        /// <summary>
        /// Gets or sets the StartAngle property. This dependency property 
        /// indicates the angle at which the first spoke (with max opacity) is drawn.
        /// </summary>
        public double StartAngle
        {
            get => (double)this.GetValue(StartAngleProperty);
            set => this.SetValue(StartAngleProperty, value);
        }

        /// <summary>
        /// Handles changes to the StartAngle property.
        /// </summary>
        /// <param name="d">SprocketControl</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnStartAngleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sprocket = (SprocketControl)d;
            var oldStartAngle = (double)e.OldValue;
            var newStartAngle = sprocket.StartAngle;
            sprocket.OnStartAngleChanged(oldStartAngle, newStartAngle);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the StartAngle property.
        /// </summary>
        /// <param name="oldStartAngle">Old Value</param>
        /// <param name="newStartAngle">New Value</param>
        protected virtual void OnStartAngleChanged(double oldStartAngle, double newStartAngle)
        {
            // Recalculate the spoke points
            this.CalculateSpokesPoints();
        }

        #endregion

        #region TickColor

        /// <summary>
        /// TickColor Dependency Property
        /// </summary>
        public static readonly DependencyProperty TickColorProperty =
            DependencyProperty.Register(nameof(TickColor), typeof(Color), typeof(SprocketControl),
                new FrameworkPropertyMetadata(DEFAULT_TICK_COLOR,
                    OnTickColorChanged));

        /// <summary>
        /// Gets or sets the TickColor property. This dependency property 
        /// indicates the color of the Spokes in the SprocketControl.
        /// </summary>
        public Color TickColor
        {
            get => (Color)this.GetValue(TickColorProperty);
            set => this.SetValue(TickColorProperty, value);
        }

        /// <summary>
        /// Handles changes to the TickColor property.
        /// </summary>
        /// <param name="d">SprocketControl</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnTickColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sprocket = (SprocketControl)d;
            var oldTickColor = (Color)e.OldValue;
            var newTickColor = sprocket.TickColor;
            sprocket.OnTickColorChanged(oldTickColor, newTickColor);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the TickColor property.
        /// </summary>
        /// <param name="oldTickColor">Old Value</param>
        /// <param name="newTickColor">New Value</param>
        protected virtual void OnTickColorChanged(Color oldTickColor, Color newTickColor)
        {
            this.InvalidateVisual();
        }

        #endregion

        #region TickCount

        /// <summary>
        /// TickCount Dependency Property
        /// </summary>
        public static readonly DependencyProperty TickCountProperty =
            DependencyProperty.Register(nameof(TickCount), typeof(int), typeof(SprocketControl),
                new FrameworkPropertyMetadata(DEFAULT_TICK_COUNT,
                    OnTickCountChanged,
                    CoerceTickCount));

        /// <summary>
        /// Gets or sets the TickCount property. This dependency property 
        /// indicates the number of spokes of the SprocketControl.
        /// </summary>
        public int TickCount
        {
            get => (int)this.GetValue(TickCountProperty);
            set => this.SetValue(TickCountProperty, value);
        }

        /// <summary>
        /// Coerces the TickCount value to an acceptable value
        /// </summary>
        /// <param name="d">SprocketControl</param>
        /// <param name="value">New Value</param>
        /// <returns>Coerced Value</returns>
        private static object CoerceTickCount(DependencyObject d, object value)
        {
            var count = (int)value;

            if (count <= 0)
            {
                return DEFAULT_TICK_COUNT;
            }

            return value;
        }

        /// <summary>
        /// Handles changes to the TickCount property.
        /// </summary>
        /// <param name="d">SprocketControl</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnTickCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sprocket = (SprocketControl)d;
            var oldTickCount = (int)e.OldValue;
            var newTickCount = sprocket.TickCount;
            sprocket.OnTickCountChanged(oldTickCount, newTickCount);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the TickCount property.
        /// </summary>
        /// <param name="oldTickCount">Old Value</param>
        /// <param name="newTickCount">New Value</param>
        protected virtual void OnTickCountChanged(int oldTickCount, int newTickCount)
        {
            // Recalculate the spoke points
            this.CalculateSpokesPoints();
        }

        #endregion

        #region TickStyle

        /// <summary>
        /// TickStyle Dependency Property
        /// </summary>
        public static readonly DependencyProperty TickStyleProperty =
            DependencyProperty.Register(nameof(TickStyle), typeof(PenLineCap), typeof(SprocketControl),
                new FrameworkPropertyMetadata(PenLineCap.Round, (OnTickStyleChanged)));

        /// <summary>
        /// Gets or sets the TickStyle property. This dependency property 
        /// indicates the style of the ends of each tick.
        /// </summary>
        public PenLineCap TickStyle
        {
            get => (PenLineCap)this.GetValue(TickStyleProperty);
            set => this.SetValue(TickStyleProperty, value);
        }

        /// <summary>
        /// Handles changes to the TickStyle property.
        /// </summary>
        /// <param name="d">SprocketControl</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnTickStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sprocket = (SprocketControl)d;
            var oldTickStyle = (PenLineCap)e.OldValue;
            var newTickStyle = sprocket.TickStyle;
            sprocket.OnTickStyleChanged(oldTickStyle, newTickStyle);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the TickStyle property.
        /// </summary>
        /// <param name="oldTickStyle">Old Value</param>
        /// <param name="newTickStyle">New Value</param>
        protected virtual void OnTickStyleChanged(PenLineCap oldTickStyle, PenLineCap newTickStyle)
        {
            this.InvalidateVisual();
        }

        #endregion

        #region TickWidth

        /// <summary>
        /// TickWidth Dependency Property
        /// </summary>
        public static readonly DependencyProperty TickWidthProperty =
            DependencyProperty.Register(nameof(TickWidth), typeof(double), typeof(SprocketControl),
                new FrameworkPropertyMetadata(DEFAULT_TICK_WIDTH,
                    OnTickWidthChanged,
                    CoerceTickWidth));

        /// <summary>
        /// Gets or sets the TickWidth property. This dependency property 
        /// indicates the width of each spoke in the SprocketControl.
        /// </summary>
        public double TickWidth
        {
            get => (double)this.GetValue(TickWidthProperty);
            set => this.SetValue(TickWidthProperty, value);
        }

        /// <summary>
        /// Coerces the TickWidth value so that it stays above 0.
        /// </summary>
        /// <param name="d">SprocketControl</param>
        /// <param name="value">New Value</param>
        /// <returns>Coerced Value</returns>
        private static object CoerceTickWidth(DependencyObject d, object value)
        {
            var progress = (double)value;

            if (progress < 0.0)
            {
                return DEFAULT_TICK_WIDTH;
            }

            return value;
        }

        /// <summary>
        /// Handles changes to the TickWidth property.
        /// </summary>
        /// <param name="d">SprocketControl</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnTickWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (SprocketControl)d;
            var oldTickWidth = (double)e.OldValue;
            var newTickWidth = target.TickWidth;
            target.OnTickWidthChanged(oldTickWidth, newTickWidth);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the TickWidth property.
        /// </summary>
        /// <param name="oldTickWidth">Old Value</param>
        /// <param name="newTickWidth">New Value</param>
        protected virtual void OnTickWidthChanged(double oldTickWidth, double newTickWidth)
        {
            this.InvalidateVisual();
        }

        #endregion

        #region LowestAlpha

        /// <summary>
        /// LowestAlpha Dependency Property
        /// </summary>
        public static readonly DependencyProperty LowestAlphaProperty =
            DependencyProperty.Register(nameof(LowestAlpha), typeof(Int32), typeof(SprocketControl),
                new FrameworkPropertyMetadata(ALPHA_LOWER_LIMIT,
                    OnLowestAlphaChanged,
                    CoerceLowestAlpha));

        /// <summary>
        /// Gets or sets the LowestAlpha property. This dependency property 
        /// indicates the lowest Opacity value that must be used while rendering the SprocketControl's spokes.
        /// </summary>
        public Int32 LowestAlpha
        {
            get => (Int32)this.GetValue(LowestAlphaProperty);
            set => this.SetValue(LowestAlphaProperty, value);
        }

        /// <summary>
        /// Handles changes to the LowestAlpha property.
        /// </summary>
        private static void OnLowestAlphaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sprocket = (SprocketControl)d;
            var oldLowestAlpha = (Int32)e.OldValue;
            var newLowestAlpha = sprocket.LowestAlpha;
            sprocket.OnLowestAlphaChanged(oldLowestAlpha, newLowestAlpha);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the LowestAlpha property.
        /// </summary>
        protected virtual void OnLowestAlphaChanged(Int32 oldLowestAlpha, Int32 newLowestAlpha)
        {
        }

        /// <summary>
        /// Coerces the LowestAlpha value.
        /// </summary>
        private static object CoerceLowestAlpha(DependencyObject d, object value)
        {
            var sprocket = (SprocketControl)d;
            var desiredLowestAlpha = (Int32)value;

            if (desiredLowestAlpha < ALPHA_LOWER_LIMIT)
            {
                return ALPHA_LOWER_LIMIT;
            }
            else if (desiredLowestAlpha > ALPHA_UPPER_LIMIT)
            {
                return ALPHA_UPPER_LIMIT;
            }

            return desiredLowestAlpha;
        }

        #endregion

        #region AlphaTicksPercentage

        /// <summary>
        /// AlphaTicksPercentage Dependency Property
        /// </summary>
        public static readonly DependencyProperty AlphaTicksPercentageProperty =
            DependencyProperty.Register(nameof(AlphaTicksPercentage), typeof(double), typeof(SprocketControl),
                new FrameworkPropertyMetadata(100.0,
                    OnAlphaTicksPercentageChanged,
                    CoerceAlphaTicksPercentage));

        /// <summary>
        /// Gets or sets the AlphaTicksPercentage property. This dependency property 
        /// indicates the percentage of total ticks which must be considered for step by step reduction
        /// of the alpha value. The remaining ticks remain at the LowestAlpha value.
        /// </summary>
        public double AlphaTicksPercentage
        {
            get => (double)this.GetValue(AlphaTicksPercentageProperty);
            set => this.SetValue(AlphaTicksPercentageProperty, value);
        }

        /// <summary>
        /// Handles changes to the AlphaTicksPercentage property.
        /// </summary>
        private static void OnAlphaTicksPercentageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sprocket = (SprocketControl)d;
            var oldAlphaTicksPercentage = (double)e.OldValue;
            var newAlphaTicksPercentage = sprocket.AlphaTicksPercentage;
            sprocket.OnAlphaTicksPercentageChanged(oldAlphaTicksPercentage, newAlphaTicksPercentage);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the AlphaTicksPercentage property.
        /// </summary>
        protected virtual void OnAlphaTicksPercentageChanged(double oldAlphaTicksPercentage, double newAlphaTicksPercentage)
        {
        }

        /// <summary>
        /// Coerces the AlphaTicksPercentage value.
        /// </summary>
        private static object CoerceAlphaTicksPercentage(DependencyObject d, object value)
        {
            var target = (SprocketControl)d;
            var desiredAlphaTicksPercentage = (double)value;

            if (desiredAlphaTicksPercentage > 100.0)
            {
                return 100.0;
            }

            if (desiredAlphaTicksPercentage < ALPHA_TICK_PERCENTAGE_LOWER_LIMIT)
            {
                return ALPHA_TICK_PERCENTAGE_LOWER_LIMIT;
            }

            return desiredAlphaTicksPercentage;
        }

        #endregion

        #endregion

        #region Construction

        /// <summary>
        /// Ctor
        /// </summary>
        public SprocketControl()
        {
            this.renderTimer = new System.Timers.Timer(this.Interval);
            this.renderTimer.Elapsed += this.OnRenderTimerElapsed;

            // Set the minimum size of the SprocketControl
            this.MinWidth = this.MINIMUM_CONTROL_SIZE.Width;
            this.MinWidth = this.MINIMUM_CONTROL_SIZE.Height;

            // Calculate the spoke points based on the current size
            this.CalculateSpokesPoints();

            RoutedEventHandler handler = null;
            handler = delegate
            {
                this.Loaded -= handler;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if ((this.IsIndeterminate) && (this.IsVisible))
                    {
                        this.Start();
                    }
                }));
            };

            this.Loaded += handler;

            // Event handler added to stop the timer if the control is no longer visible
            this.IsVisibleChanged += this.OnVisibilityChanged;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Start the Tick Control rotation
        /// </summary>
        private void Start()
        {
            if ((this.renderTimer != null) && (!this.renderTimer.Enabled))
            {
                this.renderTimer.Interval = this.Interval;
                this.renderTimer.Enabled = true;
            }
        }

        /// <summary>
        /// Stop the Tick Control rotation
        /// </summary>
        private void Stop()
        {
            if (this.renderTimer != null)
            {
                this.renderTimer.Enabled = false;
            }
        }

        /// <summary>
        /// Converts Degrees to Radians
        /// </summary>
        /// <param name="degrees">Degrees</param>
        /// <returns>Radians</returns>
        private double ConvertDegreesToRadians(double degrees)
        {
            return ((Math.PI / (double)180) * degrees);
        }

        /// <summary>
        /// Calculate the Spoke Points and store them
        /// </summary>
        private void CalculateSpokesPoints()
        {
            this.spokes = new List<Spoke>();

            // Calculate the angle between adjacent spokes
            this.angleIncrement = (360 / (double)this.TickCount);
            // Calculate the change in alpha between adjacent spokes
            this.alphaChange = (int)((double)(255 - this.LowestAlpha) / (double)((this.AlphaTicksPercentage / 100.0) * this.TickCount));

            // Set the start angle for rendering
            this.renderStartAngle = this.StartAngle;

            // Calculate the location around which the spokes will be drawn
            var width = (this.Width < this.Height) ? this.Width : this.Height;
            this.centerPoint = new Point(this.Width / 2, this.Height / 2);
            // Calculate the inner and outer radii of the control. The radii should not be less than the
            // Minimum values
            this.innerRadius = (int)(width * INNER_RADIUS_FACTOR);
            if (this.innerRadius < MINIMUM_INNER_RADIUS)
            {
                this.innerRadius = MINIMUM_INNER_RADIUS;
            }

            this.outerRadius = (int)(width * OUTER_RADIUS_FACTOR);
            if (this.outerRadius < MINIMUM_OUTER_RADIUS)
            {
                this.outerRadius = MINIMUM_OUTER_RADIUS;
            }

            double angle = 0;

            for (var i = 0; i < this.TickCount; i++)
            {
                var pt1 = new Point(this.innerRadius * (float)Math.Cos(this.ConvertDegreesToRadians(angle)), this.innerRadius * (float)Math.Sin(this.ConvertDegreesToRadians(angle)));
                var pt2 = new Point(this.outerRadius * (float)Math.Cos(this.ConvertDegreesToRadians(angle)), this.outerRadius * (float)Math.Sin(this.ConvertDegreesToRadians(angle)));

                // Create a spoke based on the points generated
                var spoke = new Spoke(pt1, pt2);
                // Add the spoke to the List
                this.spokes.Add(spoke);

                // If it is not it Indeterminate state, 
                // ensure that the spokes are drawn in clockwise manner
                if (!this.IsIndeterminate)
                {
                    angle += this.angleIncrement;
                }
                else
                {
                    if (this.Rotation == Direction.CLOCKWISE)
                    {
                        angle -= this.angleIncrement;
                    }
                    else if (this.Rotation == Direction.ANTICLOCKWISE)
                    {
                        angle += this.angleIncrement;
                    }
                }
            }
        }

        #endregion

        #region Overrides

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            // Calculate the spoke points based on the new size
            this.CalculateSpokesPoints();
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (this.spokes == null)
            {
                return;
            }

            var translate = new TranslateTransform(this.centerPoint.X, this.centerPoint.Y);
            dc.PushTransform(translate);
            var rotate = new RotateTransform(this.renderStartAngle);
            dc.PushTransform(rotate);

            var alpha = (byte)255;

            // Get the number of spokes that can be drawn with zero transparency
            var progressSpokes = (int)Math.Floor((this.Progress * this.TickCount) / 100.0);

            // Render the spokes
            for (var i = 0; i < this.TickCount; i++)
            {
                if (!this.IsIndeterminate)
                {
                    if (progressSpokes > 0)
                    {
                        alpha = (byte)(i < progressSpokes ? 255 : DEFAULT_PROGRESS_ALPHA);
                    }
                    else
                    {
                        alpha = (byte)DEFAULT_PROGRESS_ALPHA;
                    }
                }

                var p = new Pen(new SolidColorBrush(Color.FromArgb(alpha, this.TickColor.R, this.TickColor.G, this.TickColor.B)), this.TickWidth);
                p.StartLineCap = p.EndLineCap = this.TickStyle;
                dc.DrawLine(p, this.spokes[i].StartPoint, this.spokes[i].EndPoint);

                if (this.IsIndeterminate)
                {
                    alpha -= (byte)this.alphaChange;
                    if (alpha < this.LowestAlpha)
                    {
                        alpha = (byte)this.LowestAlpha;
                    }
                }
            }

            // Perform a reverse Rotation and Translation to obtain the original Transformation
            dc.Pop();
            dc.Pop();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Elapsed event of the renderTimer
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">EventArgs</param>
        void OnRenderTimerElapsed(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (this.Rotation == Direction.CLOCKWISE)
                {
                    this.renderStartAngle += this.angleIncrement;

                    if (this.renderStartAngle >= 360)
                    {
                        this.renderStartAngle -= 360;
                    }
                }
                else if (this.Rotation == Direction.ANTICLOCKWISE)
                {
                    this.renderStartAngle -= this.angleIncrement;

                    if (this.renderStartAngle <= -360)
                    {
                        this.renderStartAngle += 360;
                    }
                }

                // Force re-rendering of control
                this.InvalidateVisual();
            }));
        }

        /// <summary>
        /// Event handler to stop the timer if the control is no longer visible
        /// and start it when it is visible
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Needs to be handled only if the state of the progress bar is indeterminate
            if (this.IsIndeterminate)
            {
                if ((bool)e.NewValue)
                {
                    this.Start();
                }
                else
                {
                    this.Stop();
                }
            }
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Releases all resources used by an instance of the SprocketControl class.
        /// </summary>
        /// <remarks>
        /// This method calls the virtual Dispose(bool) method, passing in 'true', and then suppresses 
        /// finalization of the instance.
        /// </remarks>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged resources before an instance of the SprocketControl class is reclaimed by garbage collection.
        /// </summary>
        /// <remarks>
        /// NOTE: Leave out the finalizer altogether if this class doesn't own unmanaged resources itself, 
        /// but leave the other methods exactly as they are.
        /// This method releases unmanaged resources by calling the virtual Dispose(bool), passing in 'false'.
        /// </remarks>
        ~SprocketControl()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Releases the unmanaged resources used by an instance of the SprocketControl class and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">'true' to release both managed and unmanaged resources; 'false' to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.renderTimer != null)
                {
                    this.renderTimer.Elapsed -= this.OnRenderTimerElapsed;
                    this.renderTimer.Dispose();
                }

                this.IsVisibleChanged -= this.OnVisibilityChanged;
            }

            // free native resources if there are any.			
        }

        #endregion
    }
}