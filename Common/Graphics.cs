﻿#if WPF
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Markup;
using System.Xml;
using System.Globalization;
using System.Linq;

#else

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Windows.Markup;
using System.Globalization;
using FluxJpeg.Core;
using FluxJpeg.Core.Encoder;

#endif
using System.ComponentModel;
using Visifire.Charts;
using Visifire.Commons;
using System.Windows.Media.Imaging;
using System.Text;
using System.Windows.Media.Effects;

namespace Visifire.Charts
{
    public enum Face3DType

    {
        FrontFace,
        BackFace,
        LeftFace,
        RightFace,
        BottomFace
    }

    public class Area3DDataPointFace
    {
        public Area3DDataPointFace(Double depth3D)
        {   
            _depth3D = depth3D;
            _frontFacePoints = new PointCollection();
        }

        /// <summary>
        /// 3D depth
        /// </summary>
        public Double Depth3d
        {
            get
            {   
                return _depth3D;
            }
        }

        /// <summary>
        /// Set or get points for front face
        /// </summary>
        public PointCollection FrontFacePoints
        {
            get
            {   
                return _frontFacePoints;
            }
            set
            {   
                _frontFacePoints = value;
            }
        }
        
        /// <summary>
        /// Calculates the points for back face
        /// </summary>
        public void CalculateBackFacePoints()
        {
            _backFacePoints = new PointCollection();

            foreach(Point point in _frontFacePoints)
                _backFacePoints.Add(new Point(point.X + _depth3D, point.Y - _depth3D));
        }

        /// <summary>
        /// Returns points present in at the back face.
        /// You must call CalculateBackFacePoints() befor you access BackFacePoints property
        /// </summary>
        public PointCollection BackFacePoints
        {
            get
            {   
                return _backFacePoints;
            }
        }

        // Returns points to draw a Face returns except the first point of the FrontFacePoints
        public PointCollection GetFacePoints()
        {   
            PointCollection collection = new PointCollection();
            
            for(int i = 1; i < _frontFacePoints.Count; i++)
                collection.Add(_frontFacePoints[i]);

            for(int i = _backFacePoints.Count -1; i >= 0; i--)
                collection.Add(_backFacePoints[i]);

            return collection;
        }

        public static PathFigure GetPathFigure(Path path)
        {   
            PathGeometry pg = path.Data as PathGeometry;
            return pg.Figures[0];
        }

        public static LineSegment GetLineSegment(Path path, Int32 index)
        {   
            PathGeometry pg = path.Data as PathGeometry;
            return (pg.Figures[0].Segments[index] as LineSegment);
        }
        
        PointCollection _frontFacePoints;
        PointCollection _backFacePoints;
        Double _depth3D;

        public DependencyObject TopFace;
        public DependencyObject LeftFace;
        public DependencyObject RightFace;
    }   

    /// <summary>
    /// Visifire.Charts.Faces class
    /// </summary>
    public class Faces
    {
        /// <summary>
        /// Initializes a new instance of Visifire.Charts.Faces class
        /// </summary>
        public Faces()
        {   
            VisualComponents = new List<FrameworkElement>();
            BorderElements = new List<Shape>();
            BevelElements = new List<FrameworkElement>();
            LightingElements = new List<FrameworkElement>();
            ShadowElements = new List<FrameworkElement>();
            Parts = new List<DependencyObject>();
        }
        
        /// <summary>
        /// Contains references to individual components of the _axisIndicatorBorderElement elements in the visual
        /// </summary>
        public List<Shape> BorderElements;

        public List<FrameworkElement> BevelElements;

        public List<FrameworkElement> LightingElements;

        public List<FrameworkElement> ShadowElements;

        /// <summary>
        /// Contains references to individual components of the elements in the visual
        /// </summary>
        public List<FrameworkElement> VisualComponents;

        /// <summary>
        /// Different parts of Visuals. Parts are used while doing partial update
        /// </summary>
        public List<DependencyObject> Parts;

        /// <summary>
        /// Visual of faces
        /// </summary>
        public FrameworkElement Visual;

        /// <summary>
        /// Label canvas reference for faces
        /// </summary>
        public Canvas LabelCanvas;

        /// <summary>
        /// DataContext
        /// </summary>
        public Object DataContext;

        internal void ClearInstanceRefs()
        {
            if (VisualComponents != null)
                foreach (FrameworkElement fe in VisualComponents)
                    fe.Tag = null;

            if (BorderElements != null)
                foreach (FrameworkElement fe in BorderElements)
                    fe.Tag = null;

            if (Parts != null)
                Parts = null;

            if (Visual != null)
                Visual.Tag = null;

            ClearFrontArea3DFaces();

            BorderElements = null;
            BevelElements = null;
            LightingElements = null;
            ShadowElements = null;
            VisualComponents = null;
            Parts = null;
            Visual = null;
            LabelCanvas = null;
            DataContext = null;
        }

        public void ClearFrontArea3DFaces()
        {
            Area3DLeftFace = null;
            Area3DLeftTopFace = null;
            Area3DRightTopFace = null;
            Area3DRightFace = null;
        }

        internal void ClearList(ref List<DependencyObject> listReference)
        {
            foreach (FrameworkElement fe in listReference)
            {
                Panel parent = fe.Parent as Panel;

                if (parent != null)
                    parent.Children.Remove(fe);
            }

            listReference.Clear();
        }

        internal void ClearList(ref List<FrameworkElement> listReference)
        {
            foreach (FrameworkElement fe in listReference)
            {
                Panel parent = fe.Parent  as Panel;
                
                if (parent != null)
                    parent.Children.Remove(fe);
            }

            listReference.Clear();
        }

        public void ClearFronta3DFaces()
        {
            Area3DLeftFace = null;
            Area3DLeftTopFace = null;
            Area3DRightTopFace = null;
            Area3DRightFace = null;
        }

        public Area3DDataPointFace Area3DLeftFace;
        public Area3DDataPointFace Area3DLeftTopFace;
        public Area3DDataPointFace Area3DRightTopFace;
        public Area3DDataPointFace Area3DRightFace;
        public LineSegment AreaFrontFaceLineSegment;
        public LineSegment AreaFrontFaceBaseLineSegment; // Zero line segment point for 1st and last DataPoint only
        public Line BevelLine;
        public DataPoint PreviousDataPoint;
        public DataPoint NextDataPoint;

        /// <summary>
        /// Front faces of area chart(It includes all broken area front faces)
        /// </summary>
        public List<Path> FrontFacePaths;
    }

    /// <summary>
    /// Visifire.Charts.ExtendedGraphics class
    /// </summary>
    internal class ExtendedGraphics
    {   
        /// <summary>
        /// Initializes a new instance of Visifire.Charts.ExtendedGraphics class
        /// </summary>
        public ExtendedGraphics()
        {
        }

        #region Static Methods

        internal static void GetBrushesForPlank(Chart chart, out Brush frontBrush, out Brush topBrush, out Brush rightBrush, Boolean zeroPlank)
        {
            List<Color> colors = new List<Color>();

            if (zeroPlank)
            {
                colors.Add(Colors.White); // #FFFFFFFF
                colors.Add(Color.FromArgb(255, 223, 223, 223)); // #FFDFDFDF
                frontBrush = Graphics.CreateLinearGradientBrush(0, new Point(0.5, 1), new Point(0.5, 0), colors, new List<double>() { 0, 1 });

            }
            else
            {
                colors.Add(Color.FromArgb(125, 144, 144, 144)); // #FF868686
                colors.Add(Color.FromArgb(255, 220, 220, 220)); // #FFD2D2D2
                colors.Add(Color.FromArgb(255, 245, 245, 245)); // #FFFFFFFF
                colors.Add(Color.FromArgb(255, 220, 220, 220)); // #FFDFDFDF
                frontBrush = Graphics.CreateLinearGradientBrush(0, new Point(0.5, 1), new Point(0.5, 0), colors, new List<double>() { 0, 1.844, 1, 0.442 });
            }

            colors = new List<Color>();
            colors.Add(Color.FromArgb(255, 155, 155, 155));  // #FF8E8787
            colors.Add(Color.FromArgb(255, 232, 232, 232));  // #FFE8E8E8

            rightBrush = Graphics.CreateLinearGradientBrush(45, new Point(0, 0.5), new Point(1, 0.5), colors, new List<double>() { 1, 0 });

            colors = new List<Color>();
            colors.Add(Color.FromArgb(255, 180, 180, 180));  // #FF8E8787
            colors.Add(Color.FromArgb(255, 240, 240, 240));  // #FFE8E8E8
            //colors.Add(Color.FromArgb(255, 232, 227, 227));  // #FFE8E3E3

            topBrush = Graphics.CreateLinearGradientBrush(0, new Point(0.5, 1), new Point(0.5, 0), colors, new List<double>() { 1, 0 });
        
        }

        /// <summary>
        /// Returns dash array for _axisIndicatorBorderElement
        /// </summary>
        /// <param name="borderStyle">BorderStyle as BorderStyles</param>
        /// <returns>DashArray as DoubleCollection</returns>
        internal static DoubleCollection GetDashArray(BorderStyles borderStyle)
        {
            return Graphics.LineStyleToStrokeDashArray(borderStyle.ToString());
        }
        
        /// <summary>
        /// Returns dash array for line
        /// </summary>
        /// <param name="lineStyle">LineStyle as LineStyles</param>
        /// <returns>DashArray as DoubleCollection</returns>
        internal static DoubleCollection GetDashArray(LineStyles lineStyle)
        {
            return Graphics.LineStyleToStrokeDashArray(lineStyle.ToString());
        }

        /// <summary>
        /// Generates a rectangle. The shape of each of the corners can be controlled and is useful for creating single sided 
        /// curved rectangles.
        /// </summary>
        private static PathGeometry GetRectanglePathGeometry(Double width, Double height, CornerRadius xRadius, CornerRadius yRadius)
        {
            // Create a path geometry object
            PathGeometry pathGeometry = new PathGeometry();

            pathGeometry.Figures = new PathFigureCollection();

            PathFigure pathFigure = new PathFigure();

            pathFigure.StartPoint = new Point(xRadius.TopLeft, 0);
            pathFigure.Segments = new PathSegmentCollection();

            // Do not change the order of the lines below
            // Segmens required to create the rectangle
            pathFigure.Segments.Add(Graphics.GetLineSegment(new Point(width - xRadius.TopRight, 0)));
            pathFigure.Segments.Add(Graphics.GetArcSegment(new Point(width, yRadius.TopRight), new Size(xRadius.TopRight, yRadius.TopRight),0, SweepDirection.Clockwise));
            pathFigure.Segments.Add(Graphics.GetLineSegment(new Point(width, height - yRadius.BottomRight)));
            pathFigure.Segments.Add(Graphics.GetArcSegment(new Point(width - xRadius.BottomRight, height), new Size(xRadius.BottomRight, yRadius.BottomRight),0, SweepDirection.Clockwise));
            pathFigure.Segments.Add(Graphics.GetLineSegment(new Point(xRadius.BottomLeft, height)));
            pathFigure.Segments.Add(Graphics.GetArcSegment(new Point(0, height - yRadius.BottomLeft), new Size(xRadius.BottomLeft, yRadius.BottomLeft), 0,SweepDirection.Clockwise));
            pathFigure.Segments.Add(Graphics.GetLineSegment(new Point(0, yRadius.TopLeft)));
            pathFigure.Segments.Add(Graphics.GetArcSegment(new Point(xRadius.TopLeft, 0), new Size(xRadius.TopLeft, yRadius.TopLeft), 0,SweepDirection.Clockwise));

            pathGeometry.Figures.Add(pathFigure);

            return pathGeometry;
        }

        /// <summary>
        /// Get the corrected corner radius
        /// </summary>
        /// <param name="radius">Radius as CornerRadius</param>
        /// <param name="limit">Limit as Double</param>
        /// <returns>CornerRadius</returns>
        private static CornerRadius GetCorrectedRadius(CornerRadius radius,Double limit)
        {
           return new CornerRadius(
                ((radius.TopLeft > limit) ? limit/2 : radius.TopLeft),
                ((radius.TopRight > limit) ? limit/2 : radius.TopRight),
                ((radius.BottomRight > limit) ? limit/2 : radius.BottomRight),
                ((radius.BottomLeft > limit) ? limit/2 : radius.BottomLeft)
                );
        }

        /// <summary>
        /// Get corner shadow gradient brush for a rectangle
        /// </summary>
        /// <param name="corner">Corner as Corners</param>
        /// <returns>Brush</returns>
        private static Brush GetCornerShadowGradientBrush(Corners corner)
        {
            RadialGradientBrush gradBrush = new RadialGradientBrush();
            gradBrush.GradientStops = new GradientStopCollection();
            gradBrush.GradientStops.Add(Graphics.GetGradientStop(Color.FromArgb(191, 0, 0, 0), 0));
            gradBrush.GradientStops.Add(Graphics.GetGradientStop(Color.FromArgb(0, 0, 0, 0), 1));
            TransformGroup tg = new TransformGroup();
            ScaleTransform st = new ScaleTransform() { ScaleX = 2, ScaleY = 2, CenterX = 0.5, CenterY = 0.5 };
            TranslateTransform tt = null;
            switch (corner)
            {
                case Corners.TopLeft:
                    tt = new TranslateTransform() { X = 0.5, Y = 0.5 };
                    break;
                case Corners.TopRight:
                    tt = new TranslateTransform() { X = -0.5, Y = 0.5 };
                    break;
                case Corners.BottomLeft:
                    tt = new TranslateTransform() { X = 0.5, Y = -0.5 };
                    break;
                case Corners.BottomRight:
                    tt = new TranslateTransform() { X = -0.5, Y = -0.5 };
                    break;
            }
            tg.Children.Add(st);
            tg.Children.Add(tt);
            gradBrush.RelativeTransform = tg;
            return gradBrush;
        }

        /// <summary>
        /// Get side shadow gradient brush for a rectangle
        /// </summary>
        /// <param name="direction">Direction as Directions</param>
        /// <returns>Brush</returns>
        private static Brush GetSideShadowGradientBrush(Directions direction)
        {
            LinearGradientBrush gradBrush = new LinearGradientBrush();
            gradBrush.GradientStops = new GradientStopCollection();
            gradBrush.GradientStops.Add(Graphics.GetGradientStop(Color.FromArgb(191, 0, 0, 0), 0));
            gradBrush.GradientStops.Add(Graphics.GetGradientStop(Color.FromArgb(0, 0, 0, 0), 1));
            switch (direction)
            {   
                case Directions.Top:
                    gradBrush.StartPoint = new Point(0.5, 1);
                    gradBrush.EndPoint = new Point(0.5, 0);
                    break;
                case Directions.Right:
                    gradBrush.StartPoint = new Point(0, 0.5);
                    gradBrush.EndPoint = new Point(1, 0.5);
                    break;
                case Directions.Left:
                    gradBrush.StartPoint = new Point(1, 0.5);
                    gradBrush.EndPoint = new Point(0, 0.5);
                    break;
                case Directions.Bottom:
                    gradBrush.StartPoint = new Point(0.5, 0);
                    gradBrush.EndPoint = new Point(0.5, 1);
                    break;
            }
            return gradBrush;
        }

        /// <summary>
        /// Clone a DoubleCollection
        /// </summary>
        /// <param name="collection">Collection as DoubleCollection</param>
        /// <returns>DoubleCollection</returns>
        public static DoubleCollection CloneCollection(DoubleCollection collection)
        {
            if (collection == null)
                return null;

            DoubleCollection newCollection = new DoubleCollection();
            foreach (Double value in collection)
                newCollection.Add(value);

            return newCollection;
        }

        /// <summary>
        /// Creates and returns a rectangle based on the given params
        /// </summary>
        /// <param name="width">Visual width</param>
        /// <param name="height">Visual height</param>
        /// <param name="strokeThickness">StrokeThickness</param>
        /// <param name="strokeDashArray">StrokeDashArray</param>
        /// <param name="stroke">Stroke</param>
        /// <param name="fill">Fill color</param>
        /// <param name="xRadius">XRadius as CornerRadius</param>
        /// <param name="yRadius">YRadius as CornerRadius</param>
        /// <returns>Canvas</returns>
        public static Rectangle Get3DRectangle(FrameworkElement tagReference, Double width, Double height, Double strokeThickness, DoubleCollection strokeDashArray, Brush stroke, Brush fill)
        {
            Rectangle rectangle = new Rectangle() { Width = width, Height = height, Tag = new ElementData() { Element = tagReference } };
            rectangle.StrokeDashCap = PenLineCap.Flat;
            rectangle.StrokeEndLineCap = PenLineCap.Flat;
            rectangle.StrokeMiterLimit = 1;
            rectangle.StrokeStartLineCap = PenLineCap.Flat;
            rectangle.StrokeLineJoin = PenLineJoin.Bevel;
            UpdateBorderOf3DRectangle(rectangle, strokeThickness, CloneCollection(strokeDashArray), stroke);
            rectangle.Fill = fill;

            return rectangle;
        }

        public static void UpdateBorderOf3DRectangle(Shape rectangle, Double strokeThickness, DoubleCollection strokeDashArray, Brush stroke)
        {   
            rectangle.StrokeDashArray = strokeDashArray != null ? strokeDashArray : strokeDashArray;
            rectangle.StrokeThickness = strokeThickness;

            if (rectangle.StrokeThickness != 0)
            {
                rectangle.Stroke = stroke;
            }
        }

        /// <summary>
        /// Creates and returns a rectangle based on the given params
        /// </summary>
        /// <param name="width">Visual width</param>
        /// <param name="height">Visual height</param>
        /// <param name="strokeThickness">StrokeThickness</param>
        /// <param name="strokeDashArray">StrokeDashArray</param>
        /// <param name="stroke">Stroke</param>
        /// <param name="fill">Fill color</param>
        /// <param name="xRadius">XRadius as CornerRadius</param>
        /// <param name="yRadius">YRadius as CornerRadius</param>
        /// <returns>Canvas</returns>
        public static Path Get2DRectangle(FrameworkElement tagReference, Double width, Double height, Double strokeThickness, DoubleCollection strokeDashArray, Brush stroke, Brush fill, CornerRadius xRadius, CornerRadius yRadius, Nullable<Boolean> isRadiusApplicable)
        {
            Boolean isPositive = false;

            DataPoint dataPoint = tagReference as DataPoint;

            if(dataPoint != null)
                isPositive = (dataPoint.InternalYValue >=0)? true : false;

            Path path = new Path() { Width = width, Height = height, Tag = new ElementData() { Element = tagReference } };
            path.StrokeDashCap = PenLineCap.Flat;
            path.StrokeEndLineCap = PenLineCap.Flat;
            path.StrokeMiterLimit = 1;
            path.StrokeStartLineCap = PenLineCap.Flat;
            path.StrokeLineJoin = PenLineJoin.Bevel;
            
            path.Data = GetBasicPathDataForColumn(width, height, isPositive, isRadiusApplicable, xRadius.TopLeft, yRadius.TopLeft);

            UpdateBorderOf2DRectangle(path as Shape, strokeThickness, CloneCollection(strokeDashArray), stroke, xRadius, yRadius, isRadiusApplicable);
            
            path.Fill = fill;

            return path;
        }


        public static Geometry GetBasicPathDataForColumn(Double width, Double height, 
            Boolean isPositive, Nullable<Boolean> isCornerRadiusAllowed, Double xRadius, Double yRadius)
        {   
            if (isCornerRadiusAllowed == true)
            {   
                PathGeometry pathGeo = new PathGeometry();
                PathFigure pathFig = new PathFigure() { StartPoint = new Point(0, height) };

                pathFig.Segments.Add(new LineSegment());
                pathFig.Segments.Add(new ArcSegment());
                pathFig.Segments.Add(new LineSegment());
                pathFig.Segments.Add(new ArcSegment());
                pathFig.Segments.Add(new LineSegment());
                pathFig.Segments.Add(new LineSegment());

                pathGeo.Figures.Add(pathFig);

                return pathGeo;
            }
            else
            {   
                RectangleGeometry recGeo = new RectangleGeometry() { Rect = new Rect(0, 0, width, height) };
                return recGeo;
            }
        }
        
        public static void UpdateBorderOf2DRectangle(Shape shape, Double strokeThickness, DoubleCollection strokeDashArray, Brush stroke, CornerRadius xRadius, CornerRadius yRadius, Nullable<Boolean> isRadiusApplicable)
        {
            Path path = shape as Path;
            Double height = path.Height;
            Double width = path.Width;

            DataPoint dataPoint = (path.Tag as ElementData).Element as DataPoint;

            if (dataPoint != null)
            {   
                 Double tempRadiusX=0;
                Double tempRadiusY=0;

                if(!dataPoint.Parent.Bevel)
                {
                    tempRadiusX = GetCorrectedRadius(xRadius, width).TopLeft;
                    tempRadiusY = GetCorrectedRadius(yRadius, height).TopLeft;
                }

                Chart chart = dataPoint.Chart as Chart;

                Boolean isPositive = dataPoint.InternalYValue >= 0 ? true : false;

                if ((Boolean)isRadiusApplicable)
                {
                    RectangleGeometry recGeo = (path.Data as RectangleGeometry);

                    PathGeometry pathGeo = (path.Data as PathGeometry);
                    PathFigure pathFig = pathGeo.Figures[0] as PathFigure;
       
                    if (recGeo != null)
                    {   
                        recGeo.Rect = new Rect(0, 0, width, height);
                    }
                    else
                    {
                        if (chart.PlotDetails.ChartOrientation == ChartOrientationType.Vertical)
                        {
                            if (isPositive)
                            {
                                pathFig.StartPoint = new Point(0, height);

                                (pathFig.Segments[0] as LineSegment).Point = new Point(0, tempRadiusY);

                                (pathFig.Segments[1] as ArcSegment).Point = new Point(tempRadiusX, 0);
                                (pathFig.Segments[1] as ArcSegment).SweepDirection = SweepDirection.Clockwise;
                                (pathFig.Segments[1] as ArcSegment).Size = new Size(tempRadiusX, tempRadiusY);

                                (pathFig.Segments[2] as LineSegment).Point = new Point(width - tempRadiusX, 0);

                                (pathFig.Segments[3] as ArcSegment).Point = new Point(width, tempRadiusY);
                                (pathFig.Segments[3] as ArcSegment).SweepDirection = SweepDirection.Clockwise;
                                (pathFig.Segments[3] as ArcSegment).Size = new Size(tempRadiusX, tempRadiusY);

                                (pathFig.Segments[4] as LineSegment).Point = new Point(width, height);
                                (pathFig.Segments[5] as LineSegment).Point = new Point(0, height);
                            }
                            else
                            {
                                pathFig.StartPoint = new Point(0, 0);

                                (pathFig.Segments[0] as LineSegment).Point = new Point(0, height - tempRadiusY);

                                (pathFig.Segments[1] as ArcSegment).Point = new Point(tempRadiusX, height);
                                (pathFig.Segments[1] as ArcSegment).SweepDirection = SweepDirection.Counterclockwise;
                                (pathFig.Segments[1] as ArcSegment).Size = new Size(tempRadiusX, tempRadiusY);

                                (pathFig.Segments[2] as LineSegment).Point = new Point(width - tempRadiusX, height);

                                (pathFig.Segments[3] as ArcSegment).Point = new Point(width, height - tempRadiusY);
                                (pathFig.Segments[3] as ArcSegment).SweepDirection = SweepDirection.Counterclockwise;
                                (pathFig.Segments[3] as ArcSegment).Size = new Size(tempRadiusX, tempRadiusY);

                                (pathFig.Segments[4] as LineSegment).Point = new Point(width, 0);
                                (pathFig.Segments[5] as LineSegment).Point = new Point(0, 0);
                            }

                        }
                        else // For BarChart
                        {
                            if (isPositive)
                            {
                                pathFig.StartPoint = new Point(0, 0);

                                (pathFig.Segments[0] as LineSegment).Point = new Point(width - tempRadiusY, 0);

                                (pathFig.Segments[1] as ArcSegment).Point = new Point(width, tempRadiusX);
                                (pathFig.Segments[1] as ArcSegment).SweepDirection = SweepDirection.Clockwise;
                                (pathFig.Segments[1] as ArcSegment).Size = new Size(tempRadiusY, tempRadiusX);

                                (pathFig.Segments[2] as LineSegment).Point = new Point(width, height - tempRadiusY);

                                (pathFig.Segments[3] as ArcSegment).Point = new Point(width - tempRadiusY, height);
                                (pathFig.Segments[3] as ArcSegment).SweepDirection = SweepDirection.Clockwise;
                                (pathFig.Segments[3] as ArcSegment).Size = new Size(tempRadiusX, tempRadiusY);

                                (pathFig.Segments[4] as LineSegment).Point = new Point(0, height);
                                (pathFig.Segments[5] as LineSegment).Point = new Point(0, 0);
                            }
                            else
                            {
                                pathFig.StartPoint = new Point(width, height);

                                (pathFig.Segments[0] as LineSegment).Point = new Point(tempRadiusY, height);

                                (pathFig.Segments[1] as ArcSegment).Point = new Point(0, height - tempRadiusX);
                                (pathFig.Segments[1] as ArcSegment).SweepDirection = SweepDirection.Clockwise;
                                (pathFig.Segments[1] as ArcSegment).Size = new Size(tempRadiusY, tempRadiusX);

                                (pathFig.Segments[2] as LineSegment).Point = new Point(0, tempRadiusX);

                                (pathFig.Segments[3] as ArcSegment).Point = new Point(tempRadiusY, 0);
                                (pathFig.Segments[3] as ArcSegment).SweepDirection = SweepDirection.Clockwise;
                                (pathFig.Segments[3] as ArcSegment).Size = new Size(tempRadiusY, tempRadiusX);

                                (pathFig.Segments[4] as LineSegment).Point = new Point(width, 0);
                                (pathFig.Segments[5] as LineSegment).Point = new Point(width, height);
                            }
                        }
                    }
                }
                else if (!(Boolean)isRadiusApplicable)
                {
                    //topRecGeo.RadiusX = 0;
                    //topRecGeo.RadiusY = 0;
                }
            }

            path.StrokeDashArray = strokeDashArray != null ? strokeDashArray : strokeDashArray;
            path.StrokeThickness = strokeThickness;

            if (path.StrokeThickness != 0)
            {
                path.Stroke = stroke;
            }
        }
        /// <summary>
        /// Creates and returns a rectangle bevel layer based on the given params
        /// </summary>
        /// <param name="width">Visual width</param>
        /// <param name="height">Visual height</param>
        /// <param name="bevelX">BevelX as Double</param>
        /// <param name="bevelY">BevelY as Double</param>
        /// <param name="topBrush">TopBrush</param>
        /// <param name="leftBrush">LeftBrush</param>
        /// <param name="rightBrush">RightBrush</param>
        /// <param name="bottomBrush">BottomBrush</param>
        /// <returns>Canvas</returns>
        public static Canvas Get2DRectangleBevel(FrameworkElement tagReference, Double width, Double height, Double bevelX,Double bevelY, Brush topBrush, Brush leftBrush, Brush rightBrush, Brush bottomBrush)
        {
            Canvas canvas = new Canvas() {IsHitTestVisible = false, Tag = new ElementData() { Element = tagReference, VisualElementName = "Bevel" } };

            canvas.Width = width;
            canvas.Height = height;

            Polygon topBevel = new Polygon() { Tag = new ElementData() { Element = tagReference, VisualElementName = "TopBevel" } };
            topBevel.Points = new PointCollection();
            topBevel.Points.Add(new Point(0, 0));
            topBevel.Points.Add(new Point(width, 0));
            topBevel.Points.Add(new Point(width - bevelX , bevelY));
            topBevel.Points.Add(new Point(bevelX, bevelY));
            topBevel.Fill = topBrush;
            canvas.Children.Add(topBevel);

            Polygon leftBevel = new Polygon() { Tag = new ElementData() { Element = tagReference, VisualElementName = "LeftBevel" } };
            leftBevel.Points = new PointCollection();
            leftBevel.Points.Add(new Point(0, 0));
            leftBevel.Points.Add(new Point(bevelX, bevelY));
            leftBevel.Points.Add(new Point(bevelX, height - bevelY));
            leftBevel.Points.Add(new Point(0, height));
            leftBevel.Fill = leftBrush;
            canvas.Children.Add(leftBevel);

            Polygon rightBevel = new Polygon() { Tag = new ElementData() { Element = tagReference, VisualElementName="RightBevel" } };
            rightBevel.Points = new PointCollection();
            rightBevel.Points.Add(new Point(width, 0));
            rightBevel.Points.Add(new Point(width, height));
            rightBevel.Points.Add(new Point(width - bevelX, height - bevelY));
            rightBevel.Points.Add(new Point(width - bevelX, bevelY));
            rightBevel.Fill = rightBrush;
            canvas.Children.Add(rightBevel);

            Polygon bottomBevel = new Polygon() { Tag = new ElementData() { Element = tagReference, VisualElementName = "BottomBevel" } };
            bottomBevel.Points = new PointCollection();
            bottomBevel.Points.Add(new Point(0, height));
            bottomBevel.Points.Add(new Point(bevelX,height - bevelY));
            bottomBevel.Points.Add(new Point(width - bevelX, height - bevelY));
            bottomBevel.Points.Add(new Point(width, height));
            bottomBevel.Fill = bottomBrush;
            canvas.Children.Add(bottomBevel);

            return canvas;
        }

        /// <summary>
        /// Creates and returns a rectangle gradient layer based on the given params
        /// </summary>
        /// <param name="width">Visual width</param>
        /// <param name="height">visual height</param>
        /// <param name="brush1">Brush1</param>
        /// <param name="brush2">Brush2</param>
        /// <param name="orientation">Orientation</param>
        /// <returns>Canvas</returns>
        public static Canvas Get2DRectangleGradiance(Double width, Double height, Brush brush1, Brush brush2, Orientation orientation)
        {
            Canvas canvas = new Canvas() { IsHitTestVisible = false, Tag = new ElementData() { VisualElementName = "LightingCanvas" } };

            canvas.Width = width;
            canvas.Height = height;

            if (orientation == Orientation.Vertical)
            {   
                Rectangle rectLeft = new Rectangle();
                rectLeft.Width = width / 2 ;
                rectLeft.Height = height;
                rectLeft.SetValue(Canvas.TopProperty, (Double)0);
                rectLeft.SetValue(Canvas.LeftProperty, (Double)0);
                rectLeft.Fill = brush1;
                canvas.Children.Add(rectLeft);

                Rectangle rectRight = new Rectangle();
                rectRight.Width = width / 2;
                rectRight.Height = height;
                rectRight.SetValue(Canvas.TopProperty, (Double)0);
                rectRight.SetValue(Canvas.LeftProperty, (Double)width / 2);
                rectRight.Fill = brush2;
                canvas.Children.Add(rectRight);
            }
            else
            {
                Rectangle rectTop = new Rectangle();
                rectTop.Width = width;
                rectTop.Height = height / 2;
                rectTop.SetValue(Canvas.TopProperty, (Double)0);
                rectTop.SetValue(Canvas.LeftProperty, (Double)0);
                rectTop.Fill = brush1;
                canvas.Children.Add(rectTop);

                Rectangle rectBottom = new Rectangle();
                rectBottom.Width = width;
                rectBottom.Height = height / 2;
                rectBottom.SetValue(Canvas.TopProperty, (Double)height / 2);
                rectBottom.SetValue(Canvas.LeftProperty, (Double)0);
                rectBottom.Fill = brush2;
                canvas.Children.Add(rectBottom);
            }

            return canvas;
        }


        /// <summary>
        /// Creates Path From Points
        /// </summary>
        /// <param name="fillColor"></param>
        /// <param name="points"></param>
        /// <returns></returns>
        public static Path GetPathFromPoints(Brush fillColor, params Point[] points)
        {
            Path path = new Path();
            PathGeometry pathGeo = new PathGeometry();
            PathFigure pathFigure = new PathFigure();
            LineSegment lineSegment;

            pathFigure.StartPoint = points[0];

            foreach (Point point in points)
            {
                lineSegment = new LineSegment() { Point = point };
                pathFigure.Segments.Add(lineSegment);
            }

            lineSegment = new LineSegment() { Point = points[0] };
            pathFigure.Segments.Add(lineSegment);

            pathGeo.Figures.Add(pathFigure);
            path.Data = pathGeo;
            path.Fill = fillColor;

            return path;
        }

#if !WP
        public static DropShadowEffect GetShadowEffect(Double direction, Double shadowDepth, Double opacity)
        {
            return new System.Windows.Media.Effects.DropShadowEffect()
            {
                Direction = direction,
                ShadowDepth = shadowDepth,
                Opacity = opacity,
#if WPF
                Color = Color.FromArgb((Byte)255, (Byte)200, (Byte)200, (Byte)200),
                RenderingBias = System.Windows.Media.Effects.RenderingBias.Quality,
                BlurRadius = 5
#else
                Color = Color.FromArgb((Byte)255, (Byte)135, (Byte)135, (Byte)135),
                BlurRadius = 6
#endif
            };
        }
#endif
        internal static PathGeometry GetShadowClip(Size clipSize, CornerRadius radius)
        {
            PathGeometry pathGeometry = new PathGeometry();
            pathGeometry.FillRule = FillRule.EvenOdd;
            pathGeometry.Figures = new PathFigureCollection();

            PathFigure pathFigure = new PathFigure();

            pathFigure.StartPoint = new Point(0, clipSize.Height - Charts.Chart.SHADOW_DEPTH);
            pathFigure.Segments = new PathSegmentCollection();

            // Do not change the order of the lines below
            pathFigure.Segments.Add(Graphics.GetLineSegment(new Point(0, clipSize.Height - Charts.Chart.SHADOW_DEPTH)));
            pathFigure.Segments.Add(Graphics.GetLineSegment(new Point(0, clipSize.Height)));
            pathFigure.Segments.Add(Graphics.GetLineSegment(new Point(clipSize.Width, clipSize.Height)));
            pathFigure.Segments.Add(Graphics.GetLineSegment(new Point(clipSize.Width, 0)));
            pathFigure.Segments.Add(Graphics.GetLineSegment(new Point(clipSize.Width - Charts.Chart.SHADOW_DEPTH, 0)));
            pathFigure.Segments.Add(Graphics.GetLineSegment(new Point(clipSize.Width - Charts.Chart.SHADOW_DEPTH, clipSize.Height - Charts.Chart.SHADOW_DEPTH - radius.BottomRight)));
            pathFigure.Segments.Add(Graphics.GetArcSegment(new Point(clipSize.Width - Charts.Chart.SHADOW_DEPTH - radius.BottomRight, clipSize.Height - Charts.Chart.SHADOW_DEPTH), new Size(radius.BottomRight, radius.BottomRight), 90, SweepDirection.Clockwise));

            pathGeometry.Figures.Add(pathFigure);

            return pathGeometry;
        }

        /// <summary>
        /// Returns PathGeometry for clipping Shadow of an element
        /// </summary>
        /// <param name="clipSize">Size clipSize</param>
        /// <returns>PathGeometry</returns>
        internal static PathGeometry GetShadowClip(Size clipSize)
        {
            PathGeometry pathGeometry = new PathGeometry();
            pathGeometry.FillRule = FillRule.EvenOdd;
            pathGeometry.Figures = new PathFigureCollection();

            PathFigure pathFigure = new PathFigure();

            pathFigure.StartPoint = new Point(0, clipSize.Height - Charts.Chart.SHADOW_DEPTH);
            pathFigure.Segments = new PathSegmentCollection();

            // Do not change the order of the lines below
            pathFigure.Segments.Add(Graphics.GetLineSegment(new Point(clipSize.Width - Charts.Chart.SHADOW_DEPTH, clipSize.Height - Charts.Chart.SHADOW_DEPTH)));
            pathFigure.Segments.Add(Graphics.GetLineSegment(new Point(clipSize.Width - Charts.Chart.SHADOW_DEPTH, 0)));
            pathFigure.Segments.Add(Graphics.GetLineSegment(new Point(clipSize.Width, 0)));
            pathFigure.Segments.Add(Graphics.GetLineSegment(new Point(clipSize.Width, clipSize.Height)));
            pathFigure.Segments.Add(Graphics.GetLineSegment(new Point(0, clipSize.Height)));
            pathFigure.Segments.Add(Graphics.GetLineSegment(new Point(0, clipSize.Height - Charts.Chart.SHADOW_DEPTH)));

            pathGeometry.Figures.Add(pathFigure);

            return pathGeometry;
        }

        /// <summary>
        /// Creates and returns a rectangle shadow based on the given params
        /// </summary>
        /// <param name="width">Visual width</param>
        /// <param name="height">Visual height</param>
        /// <param name="xRadius">XRadius as CornerRadius</param>
        /// <param name="yRadius">YRadius as CornerRadius</param>
        /// <param name="minCurvature">MinCurvature as Double</param>
        /// <returns>Grid</returns>
        public static Grid Get2DRectangleShadow(FrameworkElement tagReference, Double width, Double height, CornerRadius xRadius, CornerRadius yRadius, Double minCurvature)
        {
            CornerRadius tempXRadius = new CornerRadius(Math.Max(xRadius.TopLeft, minCurvature), Math.Max(xRadius.TopRight, minCurvature), Math.Max(xRadius.BottomRight, minCurvature), Math.Max(xRadius.BottomLeft, minCurvature));
            CornerRadius tempYRadius = new CornerRadius(Math.Max(yRadius.TopLeft, minCurvature), Math.Max(yRadius.TopRight, minCurvature), Math.Max(yRadius.BottomRight, minCurvature), Math.Max(yRadius.BottomLeft, minCurvature));

            CornerRadius radiusX = GetCorrectedRadius(tempXRadius, width/2);
            CornerRadius radiusY = GetCorrectedRadius(tempYRadius, height/2);

            Grid visual = new Grid() {IsHitTestVisible = false };
            visual.Height = height;
            visual.Width = width;

            for (Int32 index = 0; index < 3; index++)
            {
                visual.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Auto) });
                visual.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0, GridUnitType.Auto) });
            }

            Rectangle topLeft = new Rectangle() { Width = radiusX.TopLeft, Height = radiusY.TopLeft,Fill = GetCornerShadowGradientBrush(Corners.TopLeft), Tag = (tagReference != null) ? new ElementData() { Element = tagReference }: null};
            Rectangle topRight = new Rectangle() { Width = radiusX.TopRight, Height = radiusY.TopRight, Fill = GetCornerShadowGradientBrush(Corners.TopRight), Tag = (tagReference != null) ? new ElementData() { Element = tagReference } : null };
            Rectangle bottomLeft = new Rectangle() { Width = radiusX.BottomLeft, Height = radiusY.BottomLeft, Fill = GetCornerShadowGradientBrush(Corners.BottomLeft), Tag = (tagReference != null) ? new ElementData() { Element = tagReference } : null };
            Rectangle bottomRight = new Rectangle() { Width = radiusX.BottomRight, Height = radiusY.BottomRight, Fill = GetCornerShadowGradientBrush(Corners.BottomRight), Tag = (tagReference != null) ? new ElementData() { Element = tagReference } : null };
            Rectangle center = new Rectangle() { Width = width - radiusX.TopLeft - radiusX.TopRight, Height = height - radiusY.TopLeft - radiusY.BottomLeft, Fill = new SolidColorBrush(Color.FromArgb((Byte)191, (Byte)0, (Byte)0, (Byte)0)), Tag = (tagReference != null) ? new ElementData() { Element = tagReference } : null };
            Rectangle top = new Rectangle() { Width = width - radiusX.TopLeft - radiusX.TopRight, Height = Math.Max(radiusY.TopLeft, radiusY.TopRight), Fill = GetSideShadowGradientBrush(Directions.Top), Tag = (tagReference != null) ? new ElementData() { Element = tagReference } : null };
            Rectangle right = new Rectangle() { Width = Math.Max(radiusX.TopRight, radiusX.BottomRight), Height = height - radiusY.TopRight - radiusY.BottomRight, Fill = GetSideShadowGradientBrush(Directions.Right), Tag = (tagReference != null) ? new ElementData() { Element = tagReference } : null };
            Rectangle left = new Rectangle() { Width = Math.Max(radiusX.TopLeft, radiusX.BottomLeft), Height = height - radiusY.TopLeft - radiusY.BottomLeft, Fill = GetSideShadowGradientBrush(Directions.Left), Tag = (tagReference != null) ? new ElementData() { Element = tagReference } : null };
            Rectangle bottom = new Rectangle() { Width = width - radiusX.BottomLeft - radiusX.BottomRight, Height = Math.Max(radiusY.BottomLeft, radiusY.BottomRight), Fill = GetSideShadowGradientBrush(Directions.Bottom), Tag = (tagReference != null) ? new ElementData() { Element = tagReference } : null };

            topLeft.SetValue(Grid.RowProperty, (Int32)0); topLeft.SetValue(Grid.ColumnProperty, (Int32)0);
            top.SetValue(Grid.RowProperty, (Int32)0); top.SetValue(Grid.ColumnProperty, (Int32)1);
            topRight.SetValue(Grid.RowProperty, (Int32)0); topRight.SetValue(Grid.ColumnProperty, (Int32)2);
            left.SetValue(Grid.RowProperty, (Int32)1); topLeft.SetValue(Grid.ColumnProperty, (Int32)0);
            center.SetValue(Grid.RowProperty, (Int32)1); center.SetValue(Grid.ColumnProperty, (Int32)1);
            right.SetValue(Grid.RowProperty, (Int32)1); right.SetValue(Grid.ColumnProperty, (Int32)2);
            bottomLeft.SetValue(Grid.RowProperty, (Int32)2); bottomLeft.SetValue(Grid.ColumnProperty, (Int32)0);
            bottom.SetValue(Grid.RowProperty, (Int32)2); bottom.SetValue(Grid.ColumnProperty, (Int32)1);
            bottomRight.SetValue(Grid.RowProperty, (Int32)2); bottomRight.SetValue(Grid.ColumnProperty, (Int32)2);

            visual.Children.Add(topLeft);
            visual.Children.Add(top);
            visual.Children.Add(topRight);
            visual.Children.Add(left);
            visual.Children.Add(center);
            visual.Children.Add(right);
            visual.Children.Add(bottomLeft);
            visual.Children.Add(bottom);
            visual.Children.Add(bottomRight);

            return visual;
        }

        /// <summary>
        /// Creates a rectangle for 3D PlotArea right edge based on the given params
        /// </summary>
        /// <param name="tagReference"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="xRadius"></param>
        /// <param name="yRadius"></param>
        /// <param name="minCurvature"></param>
        /// <param name="brush"></param>
        /// <returns>Grid</returns>
        public static Grid GetRectangle4PlotAreaEdge(FrameworkElement tagReference, Double width, Double height, CornerRadius xRadius, CornerRadius yRadius, Double minCurvature, Brush brush)
        {
            CornerRadius tempXRadius = new CornerRadius(Math.Max(xRadius.TopLeft, minCurvature), Math.Max(xRadius.TopRight, minCurvature), Math.Max(xRadius.BottomRight, minCurvature), Math.Max(xRadius.BottomLeft, minCurvature));
            CornerRadius tempYRadius = new CornerRadius(Math.Max(yRadius.TopLeft, minCurvature), Math.Max(yRadius.TopRight, minCurvature), Math.Max(yRadius.BottomRight, minCurvature), Math.Max(yRadius.BottomLeft, minCurvature));

            CornerRadius radiusX = GetCorrectedRadius(tempXRadius, width / 2);
            CornerRadius radiusY = GetCorrectedRadius(tempYRadius, height / 2);

            Grid visual = new Grid() { IsHitTestVisible = false };
            visual.Height = height;
            visual.Width = width;

            Rectangle rect = new Rectangle()
            { 
                Width = width,
                Height = height,
                RadiusX = 5,
                RadiusY = 5,    
                Fill = new SolidColorBrush(Color.FromArgb((Byte)255,(Byte)167,(Byte)171,(Byte)171)),
                Opacity = 0.90,
                Tag = (tagReference != null) ? new ElementData() { Element = tagReference } : null
            };

            visual.Children.Add(rect);

            return visual;
        }

        private enum Corners { TopLeft, TopRight, BottomLeft, BottomRight };
        private enum Directions { Top, Left, Right, Bottom };
        #endregion
    }
}

namespace Visifire.Commons
{
    /// <summary>
    /// Visifire.Commons.Graphics class
    /// </summary>

    public class Graphics
    {
        /// <summary>
        /// Initializes a new instance of Visifire.Commons.Graphics class
        /// </summary>
        public Graphics()
        {

        }

        #region Static Methods
/*
        public static ScrollViewer CreateScrollViewerFromXAML(Double scrollBarSize)
        {
            StringBuilder xaml = new StringBuilder();
            
            xaml.Append("<ScrollViewer xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">");
			xaml.Append("<ScrollViewer.Template>");
			xaml.Append("<ControlTemplate TargetType=\"ScrollViewer\">");
						xaml.Append("<Border BorderBrush=\"{TemplateBinding BorderBrush}\" BorderThickness=\"{TemplateBinding BorderThickness}\" CornerRadius=\"2\">");
							xaml.Append("<Grid Background=\"{TemplateBinding Background}\">");
								xaml.Append("<Grid.ColumnDefinitions>");
									xaml.Append("<ColumnDefinition Width=\"*\"/>");
									xaml.Append("<ColumnDefinition Width=\"Auto\"/>");
								xaml.Append("</Grid.ColumnDefinitions>");
								xaml.Append("<Grid.RowDefinitions>");
									xaml.Append("<RowDefinition Height=\"*\"/>");
									xaml.Append("<RowDefinition Height=\"Auto\"/>");
								xaml.Append("</Grid.RowDefinitions>");
								xaml.Append("<ScrollContentPresenter x:Name=\"ScrollContentPresenter\" Cursor=\"{TemplateBinding Cursor}\" Margin=\"{TemplateBinding Padding}\" ContentTemplate=\"{TemplateBinding ContentTemplate}\"/>");
								xaml.Append("<Rectangle Fill=\"#FFE9EEF4\" Grid.Column=\"1\" Grid.Row=\"1\"/>");
								xaml.Append("<ScrollBar x:Name=\"VerticalScrollBar\" Margin=\"0,-1,-1,-1\" Width=\"18\" Visibility=\"{TemplateBinding ComputedVerticalScrollBarVisibility}\" IsTabStop=\"False\" Grid.Column=\"1\" Grid.Row=\"0\" Maximum=\"{TemplateBinding ScrollableHeight}\" Minimum=\"0\" Value=\"{TemplateBinding VerticalOffset}\" Orientation=\"Vertical\" ViewportSize=\"{TemplateBinding ViewportHeight}\"/>");
								xaml.Append("<ScrollBar x:Name=\"HorizontalScrollBar\" Height=\"18\" Margin=\"-1,0,-1,-1\" Visibility=\"{TemplateBinding ComputedHorizontalScrollBarVisibility}\" IsTabStop=\"False\" Grid.Column=\"0\" Grid.Row=\"1\" Maximum=\"{TemplateBinding ScrollableWidth}\" Minimum=\"0\" Value=\"{TemplateBinding HorizontalOffset}\" Orientation=\"Horizontal\" ViewportSize=\"{TemplateBinding ViewportWidth}\"/>");
							xaml.Append("</Grid>");
						xaml.Append("</Border>");
					xaml.Append("</ControlTemplate>");
			xaml.Append("</ScrollViewer.Template>");
		xaml.Append("</ScrollViewer>");

#if SL
           return (ScrollViewer) System.Windows.Markup.XamlReader.Load(xaml.ToString());
#else
           return (ScrollViewer)XamlReader.Load(new XmlTextReader(new System.IO.StringReader(xaml.ToString())));
#endif
        }
        */
#if SL

        /// <summary>
        /// Get image MemoryStream from WriteableBitmap
        /// </summary>
        /// <param name="bitmap">WriteableBitmap</param>
        /// <returns>MemoryStream</returns>
        public static System.IO.MemoryStream GetImageStream(WriteableBitmap bitmap, Visifire.Charts.ExportType exportTypes)
        {   
            byte[][,] raster = ReadRasterInformation(bitmap);

            if (exportTypes == ExportType.Jpg)
            {
                return EncodeRasterInformationToStream(raster, ColorSpace.RGB);
            }
            else
            {   
                int width = bitmap.PixelWidth;
                int height = bitmap.PixelHeight;

                System.IO.MemoryStream ms = new System.IO.MemoryStream();

                // http://en.wikipedia.org/wiki/BMP_file_format
                #region BMP File Header(14 bytes)

                // Magic number(2 bytes):BM
                ms.WriteByte(0x42); ms.WriteByte(0x4D);

                // Size of the BMP file in bytes(4 bytes)
                long len = bitmap.Pixels.Length * 4 + 0x36;

                ms.WriteByte((byte)len);
                ms.WriteByte((byte)(len >> 8));
                ms.WriteByte((byte)(len >> 16));
                ms.WriteByte((byte)(len >> 24));

                // Reserved(2 bytes)
                ms.WriteByte(0x00); ms.WriteByte(0x00);

                // Reserved(2 bytes)
                ms.WriteByte(0x00); ms.WriteByte(0x00);

                // Offset(4 bytes)
                ms.WriteByte(0x36); ms.WriteByte(0x00); ms.WriteByte(0x00); ms.WriteByte(0x00);

                #endregion

                // http://en.wikipedia.org/wiki/BMP_file_format
                #region Bitmap Information(40 bytes:Windows V3)

                // Size of this header(4 bytes)
                ms.WriteByte(0x28); ms.WriteByte(0x00); ms.WriteByte(0x00); ms.WriteByte(0x00);

                // Bitmap width in pixels(4 bytes)
                ms.WriteByte((byte)width); ms.WriteByte((byte)(width >> 8)); ms.WriteByte((byte)(width >> 16));
                ms.WriteByte((byte)(width >> 24));

                // Bitmap height in pixels(4 bytes)
                ms.WriteByte((byte)height); ms.WriteByte((byte)(height >> 8)); ms.WriteByte((byte)(height >> 16)); ms.WriteByte((byte)(height >> 24));

                // Bumber of color planes(2 bytes)
                ms.WriteByte(0x01); ms.WriteByte(0x00);

                // Number of bits per pixel(2 bytes)
                ms.WriteByte(0x20); ms.WriteByte(0x00);

                // Compression method(4 bytes)
                ms.WriteByte(0x00); ms.WriteByte(0x00); ms.WriteByte(0x00); ms.WriteByte(0x00);
                
                // Image size(4 bytes)
                ms.WriteByte(0x00); ms.WriteByte(0x00); ms.WriteByte(0x00); ms.WriteByte(0x00);

                // Horizontal resolution of the image(4 bytes)
                ms.WriteByte(0x00); ms.WriteByte(0x00); ms.WriteByte(0x00); ms.WriteByte(0x00);
                
                // Vertical resolution of the image(4 bytes)
                ms.WriteByte(0x00); ms.WriteByte(0x00); ms.WriteByte(0x00); ms.WriteByte(0x00);

                // Number of colors in the color palette(4 bytes)
                ms.WriteByte(0x00); ms.WriteByte(0x00); ms.WriteByte(0x00); ms.WriteByte(0x00);

                // Number of important colors(4 bytes)
                ms.WriteByte(0x00); ms.WriteByte(0x00); ms.WriteByte(0x00); ms.WriteByte(0x00);
                
                #endregion

                // Write Bitmap data
                for (int columnId = height - 1; columnId >= 0; columnId--)
                {
                    for (int rowId = 0; rowId < width; rowId++)
                    {
                        int pixel = bitmap.Pixels[width * columnId + rowId];

                        ms.WriteByte((byte)(pixel & 0xff)); //B
                        ms.WriteByte((byte)((pixel >> 8) & 0xff)); //G
                        ms.WriteByte((byte)((pixel >> 0x10) & 0xff)); //R
                        ms.WriteByte(0x00); //reserved
                    }
                }

                return new System.IO.MemoryStream(ms.GetBuffer(), true);
            }
        }

        /// <summary>
        /// Reads raster information from WriteableBitmap
        /// </summary>
        /// <param name="bitmap">WriteableBitmap</param>
        /// <returns>Array of bytes</returns>
        public static byte[][,] ReadRasterInformation(WriteableBitmap bitmap)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int bands = 3;
            byte[][,] raster = new byte[bands][,];

            for (int i = 0; i < bands; i++)
            {
                raster[i] = new byte[width, height];
            }

            for (int row = 0; row < height; row++)
            {   
                for (int column = 0; column < width; column++)
                {
                    int pixel = bitmap.Pixels[width * row + column];
                    raster[0][column, row] = (byte)(pixel >> 16);
                    raster[1][column, row] = (byte)(pixel >> 8);
                    raster[2][column, row] = (byte)pixel;
                }
            }

            return raster;
        }

        /// <summary>
        /// Encode raster information to MemoryStream
        /// </summary>
        /// <param name="raster">Raster information (Array of bytes)</param>
        /// <param name="colorSpace">ColorSpace used</param>
        /// <returns>MemoryStream</returns>
        public static System.IO.MemoryStream EncodeRasterInformationToStream(byte[][,] raster, ColorSpace colorSpace)
        {
            ColorModel model = new ColorModel { colorspace = ColorSpace.RGB };
            FluxJpeg.Core.Image img = new FluxJpeg.Core.Image(model, raster);

            //Encode the Image as a JPEG
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            FluxJpeg.Core.Encoder.JpegEncoder encoder = new FluxJpeg.Core.Encoder.JpegEncoder(img, 100, stream);
            encoder.Encode();
            
            // Back to the start
            stream.Seek(0, System.IO.SeekOrigin.Begin);

            return stream;
        }
#endif

        /// <summary>
        /// Set soruce of an image from assembly resource
        /// </summary>
        /// <param name="image">Image</param>
        /// <param name="imageResourcePath">imageResourcePath</param>
        public static void SetImageSource(System.Windows.Controls.Image image, String imageResourcePath)
        {   
            using (System.IO.Stream imageStream = typeof(Chart).Assembly.GetManifestResourceStream(imageResourcePath))
            {
                if (imageStream != null)
                {
                    BitmapImage bmp = new BitmapImage();
#if SL
                    bmp.SetSource(imageStream);
#else               
                    bmp.BeginInit();
                    bmp.StreamSource = imageStream;
                    bmp.EndInit();
#endif
                    image.Source = bmp;
                }
            }
        }

        internal static Random RAND = new Random(DateTime.Now.Millisecond);

        public static Brush GetRandomColor()
        {
            return new SolidColorBrush(Color.FromArgb((byte)255, (byte)RAND.Next(255), (byte)RAND.Next(255), (byte)RAND.Next(255)));
        }

        public static void DrawPointAt(Point point, Canvas visual, Color fillColor)
        {
            Ellipse e = new Ellipse() { Height = 4, Width = 4, Fill = new SolidColorBrush(fillColor), Stroke = new SolidColorBrush(Colors.Red), StrokeThickness = .25 };

            e.SetValue(Canvas.LeftProperty, point.X - e.Height / 2);
            e.SetValue(Canvas.TopProperty, point.Y - e.Width / 2);
            e.SetValue(Canvas.ZIndexProperty, 10001);

            visual.Children.Add(e);
        }


        internal static bool IntersectionOfTwoStraightLines(Point line1Point1, Point line1Point2,
            Point line2Point1, Point line2Point2, ref Point intersection)
        {
            // Based on the 2d line intersection method from "comp.graphics.algorithms Frequently Asked Questions"

            /*
                   (Ay-Cy)(Dx-Cx)-(Ax-Cx)(Dy-Cy)
               r = -----------------------------  (eqn 1)
                   (Bx-Ax)(Dy-Cy)-(By-Ay)(Dx-Cx)
            */

            Double q = (line1Point1.Y - line2Point1.Y) * (line2Point2.X -
          line2Point1.X) - (line1Point1.X - line2Point1.X) * (line2Point2.Y -
          line2Point1.Y);
            Double d = (line1Point2.X - line1Point1.X) * (line2Point2.Y -
          line2Point1.Y) - (line1Point2.Y - line1Point1.Y) * (line2Point2.X -
          line2Point1.X);

            if (d == 0) // parallel lines so no intersection anywhere in space (in curved space, maybe, but not here in Euclidian space.)
            {
                return false;
            }

            Double r = q / d;

            /*
                   (Ay-Cy)(Bx-Ax)-(Ax-Cx)(By-Ay)
               s = -----------------------------  (eqn 2)
                   (Bx-Ax)(Dy-Cy)-(By-Ay)(Dx-Cx)
            */

            q = (line1Point1.Y - line2Point1.Y) * (line1Point2.X - line1Point1.X) -
          (line1Point1.X - line2Point1.X) * (line1Point2.Y - line1Point1.Y);
            Double s = q / d;

            /*
                   If r>1, P is located on extension of AB
                   If r<0, P is located on extension of BA
                   If s>1, P is located on extension of CD
                   If s<0, P is located on extension of DC

                   The above basically checks if the intersection is located at an
            extrapolated
                   point outside of the line segments. To ensure the intersection is
            only within
                   the line segments then the above must all be false, ie r between 0
            and 1
                   and s between 0 and 1.
            */

            if (r < 0 || r > 1 || s < 0 || s > 1)
            {
                return false;
            }

            /*
                   Px=Ax+r(Bx-Ax)
                   Py=Ay+r(By-Ay)
            */

            intersection.X = line1Point1.X + (int)(0.5f + r * (line1Point2.X - line1Point1.X));

            intersection.Y = line1Point1.Y + (int)(0.5f + r * (line1Point2.Y - line1Point1.Y));

            return true;
        }

        internal static Point MidPointOfALine(Point point1, Point point2)
        {
            return new Point((point1.X + point2.X) / 2, (point1.Y + point2.Y) / 2);
        }

        internal static Double DistanceBetweenTwoPoints(Point point1, Point point2)
        {
            return (Math.Sqrt(Math.Pow((point1.X - point2.X), 2) + Math.Pow((point1.Y - point2.Y), 2)));
        }

        internal static Point IntersectingPointOfTwoLines(Point p1, Point p2, Point p3, Point p4)
        {   
            Double ua = ((p4.X - p3.X) * (p1.Y - p3.Y) - (p4.Y - p3.Y) * (p1.X - p3.X));
            ua /= ((p4.Y - p3.Y) * (p2.X - p1.X) - (p4.X - p3.X) * (p2.Y - p1.Y));

            Double ub = ((p2.X - p1.X) * (p1.Y - p3.Y) - (p2.Y - p1.Y) * (p1.X - p3.X));
            ub /= ((p4.Y - p3.Y) * (p2.X - p1.X) - (p4.X - p3.X) * (p2.Y - p1.Y));

            Double x = p1.X + ua * (p2.X - p1.X);
            Double y = p1.X + ub * (p2.Y - p1.Y);

            return new Point(x, y);
        }
        
        /// <summary>
        /// Calculates visual size
        /// </summary>
        /// <param name="visual">Visual as FrameworkElement</param>
        /// <returns>Visual size</returns>
        public static Size CalculateVisualSize(FrameworkElement visual)
        {
            Size retVal = new Size(0,0);

            if (visual != null)
            {   
                visual.Measure(new Size(Double.MaxValue, Double.MaxValue));
                retVal = visual.DesiredSize;
            }

            return retVal;
        }

        /// <summary>
        /// Get 3dBrush with or without lighting for bubble
        /// </summary>
        internal static Brush Get3DBrushLighting(Brush solidColorBrush, Boolean lightingEnabled)
        {   
            if (solidColorBrush.GetType().Equals(typeof(SolidColorBrush)))
            {
                Color color = (solidColorBrush as SolidColorBrush).Color;

                RadialGradientBrush rgb = new RadialGradientBrush()
                {   
                    Center = new Point(0.3, 0.3),
                    RadiusX = 0.93,
                    RadiusY = 1,
                    GradientOrigin = new Point(0.2, 0.2)
                };

                if (color == Colors.Black)
                {
                    if(lightingEnabled)
                        rgb.GradientStops.Add(new GradientStop() { Color = Colors.White, Offset = 0 });

                    rgb.GradientStops.Add(new GradientStop() { Color = Colors.Gray, Offset = 0.1 });
                    rgb.GradientStops.Add(new GradientStop() { Color = Colors.Black, Offset = 1 });
                }
                else
                {
                    Color darkerColor = Graphics.GetDarkerColor(color, 0.2);

                    if (lightingEnabled)
                        rgb.GradientStops.Add(new GradientStop() { Color = Colors.White, Offset = 0 });

                    rgb.GradientStops.Add(new GradientStop() { Color = color, Offset = 0.1 });
                    rgb.GradientStops.Add(new GradientStop() { Color = darkerColor, Offset = 1 });
                }

                return rgb;
            }
            else
                return solidColorBrush;
        }


        internal static Size CalculateAngularSize(Double radianAngle, Double actualWidth, Double actualHeight)
        {
            if (radianAngle != 0)
            {
                // length of the diagonal from top left to bottom right
                Double length = Math.Sqrt(Math.Pow(actualHeight, 2) + Math.Pow(actualWidth, 2));

                // angle made by the diagonal with respect to the horizontal
                Double beta = Math.Atan(actualHeight / actualWidth);

                // calculate the two possible height and width values using the diagonal length and angle
                Double height1 = length * Math.Sin(radianAngle + beta);
                Double height2 = length * Math.Sin(radianAngle - beta);
                Double width1 = length * Math.Cos(radianAngle + beta);
                Double width2 = length * Math.Cos(radianAngle - beta);

                // Actual height will be the maximum of the two calculated heights
                actualHeight = Math.Max(Math.Abs(height1), Math.Abs(height2));

                // Actual width will be the maximum of the two calculated widths
                actualWidth = Math.Max(Math.Abs(width1), Math.Abs(width2));
            }

            return new Size(actualWidth, actualHeight);
        }

        /// <summary>
        /// Calculates textblock size
        /// </summary>
        /// <param name="radianAngle">RadianAngle as Double</param>
        /// <param name="textBlock">TextBlock</param>
        /// <returns>TextBlock size</returns>
        internal static Size CalculateTextBlockSize(Double radianAngle, TextBlock textBlock)
        {
            Double actualHeight;
            Double actualWidth;

#if WPF
            if(!textBlock.IsMeasureValid)
                textBlock.Measure(new Size(Double.MaxValue, Double.MaxValue));
            actualHeight = textBlock.DesiredSize.Height;
            actualWidth = textBlock.DesiredSize.Width;
#else       
            actualHeight = textBlock.ActualHeight;
            actualWidth = textBlock.ActualWidth;
#endif      

            return CalculateAngularSize(radianAngle, actualWidth, actualHeight);
        }

        /// <summary>
        /// Generate double collection
        /// </summary>
        /// <param name="values">Array of double values</param>
        /// <returns>DoubleCollection</returns>
        internal static DoubleCollection GenerateDoubleCollection(params Double[] values)
        {
            DoubleCollection collection = new DoubleCollection();

            foreach (Double value in values)
                collection.Add(value);

            return collection;
        }

        /// <summary>
        /// Generate double collection
        /// </summary>
        /// <param name="values">Array of double values</param>
        /// <returns>DoubleCollection</returns>
        internal static PointCollection GeneratePointCollection(params Point[] points)
        {
            PointCollection collection = new PointCollection();

            foreach (Point point in points)
                collection.Add(point);

            return collection;
        }

        /// <summary>
        /// Creates and returns a right gradient brush
        /// </summary>
        /// <param name="alpha">Alpha as Int32</param>
        /// <returns>Brush</returns>
        internal static Brush GetRightGradianceBrush(Int32 alpha)
        {
            LinearGradientBrush gradBrush = new LinearGradientBrush();

            gradBrush.GradientStops = new GradientStopCollection();

            gradBrush.StartPoint = new Point(1, 1);
            gradBrush.EndPoint = new Point(0, 0);

            gradBrush.GradientStops.Add(Graphics.GetGradientStop(Color.FromArgb((byte)alpha, 0, 0, 0), 0));
            gradBrush.GradientStops.Add(Graphics.GetGradientStop(Color.FromArgb(0, 0, 0, 0), 0));

            return gradBrush;
        }

        /// <summary>
        /// Creates and returns a top face brush
        /// </summary>
        /// <param name="brush">Brush</param>
        /// <returns>Brush</returns>
        internal static Brush GetTopFaceBrush(Brush brush)
        {
            if (brush != null)
            {
                if (typeof(SolidColorBrush).Equals(brush.GetType()))
                {
                    SolidColorBrush solidBrush = brush as SolidColorBrush;

                    if (_3dLightingTopBrushs.ContainsKey(solidBrush.Color))
                    {
#if WPF
                        if (System.Threading.Thread.CurrentThread.IsBackground)
                        {
                            brush = GetTopFaceGradientBrush(solidBrush);

                            if (_3dLightingTopBrushs.ContainsKey(solidBrush.Color))
                                _3dLightingTopBrushs[solidBrush.Color] = brush;
                            else
                                _3dLightingTopBrushs.Add(solidBrush.Color, brush);

                            return brush;
                        }
#endif
                        return _3dLightingTopBrushs[solidBrush.Color];
                    }
                    else
                    {
                        brush = GetTopFaceGradientBrush(solidBrush);
                        _3dLightingTopBrushs.Add(solidBrush.Color, brush);

                        return brush;
                    }
                }
                else if (brush is GradientBrush)
                {
                    GradientBrush gradBrush = brush as GradientBrush;

                    List<Color> colors = new List<Color>();
                    List<Double> stops = new List<Double>();

                    foreach (GradientStop gradStop in gradBrush.GradientStops)
                    {
                        colors.Add(Graphics.GetLighterColor(gradStop.Color, 0.85));
                        stops.Add(gradStop.Offset);
                    }

                    if (brush is LinearGradientBrush)
                        return Graphics.CreateLinearGradientBrush(-45, new Point(-0.5, 1.5), new Point(0.5, 0), colors, stops);
                    else
                        return Graphics.CreateRadialGradientBrush(colors, stops);
                }
                else
                {
                    return brush;
                }
            }
            else
                return null;
        }

        private static Brush GetTopFaceGradientBrush(SolidColorBrush solidBrush)
        {
            Brush brush;
            List<Color> colors = new List<Color>();
            List<Double> stops = new List<Double>();

            colors.Add(Graphics.GetDarkerColor(solidBrush.Color, 0.85));
            stops.Add(0);

            colors.Add(Graphics.GetLighterColor(solidBrush.Color, 0.35));
            stops.Add(1);

            brush = Graphics.CreateLinearGradientBrush(-45, new Point(0, 0.5), new Point(1, 0.5), colors, stops);

            return brush;
        }

        private static Dictionary<Color, Brush> _3dLightingTopBrushs = new Dictionary<Color, Brush>();
        private static Dictionary<Color, Brush> _3dLightingRightBrushs = new Dictionary<Color, Brush>();
        private static Dictionary<Color, Brush> _3dLightingFrontBrushs = new Dictionary<Color, Brush>();

        /// <summary>
        /// Creates and returns a right face brush
        /// </summary>
        /// <param name="brush">Brush</param>
        /// <returns>Brush</returns>
        internal static Brush GetRightFaceBrush(Brush brush)
        {
            if (brush != null)
            {
                if (typeof(SolidColorBrush).Equals(brush.GetType()))
                {
                    SolidColorBrush solidBrush = brush as SolidColorBrush;

                    if (_3dLightingRightBrushs.ContainsKey(solidBrush.Color))
                    {
#if WPF
                        if (System.Threading.Thread.CurrentThread.IsBackground)
                        {
                            brush = GetRightFaceGradientBrush(solidBrush);

                            if (_3dLightingRightBrushs.ContainsKey(solidBrush.Color))
                                _3dLightingRightBrushs[solidBrush.Color] = brush;
                            else
                                _3dLightingRightBrushs.Add(solidBrush.Color, brush);

                            return brush;
                        }
#endif

                        return _3dLightingRightBrushs[solidBrush.Color];
                    }
                    else
                    {
                        brush = GetRightFaceGradientBrush(solidBrush);
                        _3dLightingRightBrushs.Add(solidBrush.Color, brush);

                        return brush;
                    }
                }
                else if (brush is GradientBrush)
                {   
                    GradientBrush gradBrush = brush as GradientBrush;

                    List<Color> colors = new List<Color>();
                    List<Double> stops = new List<Double>();

                    foreach (GradientStop gradStop in gradBrush.GradientStops)
                    {
                        colors.Add(Graphics.GetDarkerColor(gradStop.Color, 0.75));
                        stops.Add(gradStop.Offset);
                    }

                    if (brush is LinearGradientBrush)
                        return Graphics.CreateLinearGradientBrush(0, new Point(0, 1), new Point(1, 0), colors, stops);
                    else
                        return Graphics.CreateRadialGradientBrush(colors, stops);
                }
                else
                {
                    return brush;
                }
            }
            else
                return null;
        }

        private static Brush GetRightFaceGradientBrush(SolidColorBrush solidBrush)
        {
            Brush brush;
            List<Color> colors = new List<Color>();
            List<Double> stops = new List<Double>();

            colors.Add(Graphics.GetDarkerColor(solidBrush.Color, 0.35));
            stops.Add(0);

            colors.Add(Graphics.GetDarkerColor(solidBrush.Color, 0.75));
            stops.Add(1);

            brush = Graphics.CreateLinearGradientBrush(-120, new Point(0, 0.5), new Point(1, 0.5), colors, stops);

            return brush;
        }

        /// <summary>
        /// Creates and returns a Back face brush
        /// </summary>
        /// <param name="brush">Brush</param>
        /// <returns>Brush</returns>
        internal static Brush GetFrontFaceBrush(Brush brush)
        {
            if (brush != null)
            {
                if (typeof(SolidColorBrush).Equals(brush.GetType()))
                {
                    SolidColorBrush solidBrush = brush as SolidColorBrush;

                    if (_3dLightingFrontBrushs.ContainsKey(solidBrush.Color))
                    {
#if WPF
                        if (System.Threading.Thread.CurrentThread.IsBackground)
                        {
                            brush = GetFrontFaceGradientBrush(solidBrush);

                            if (_3dLightingFrontBrushs.ContainsKey(solidBrush.Color))
                                _3dLightingFrontBrushs[solidBrush.Color] = brush;
                            else
                                _3dLightingFrontBrushs.Add(solidBrush.Color, brush);

                            return brush;
                        }
#endif
                        return _3dLightingFrontBrushs[solidBrush.Color];
                    }
                    else
                    {
                        brush = GetFrontFaceGradientBrush(solidBrush);

                        _3dLightingFrontBrushs.Add(solidBrush.Color, brush);

                        return brush;
                    }
                }
                else
                {
                    return brush;
                }
            }
            else
                return null;
        }

        private static Brush GetFrontFaceGradientBrush(SolidColorBrush solidBrush)
        {
            Brush brush;

            List<Color> colors = new List<Color>();
            List<Double> stops = new List<Double>();

            colors.Add(Graphics.GetDarkerColor(solidBrush.Color, 0.65));
            stops.Add(0);

            colors.Add(Graphics.GetLighterColor(solidBrush.Color, 0.55));
            stops.Add(1);

            brush = Graphics.CreateLinearGradientBrush(-90, new Point(0, 0.5), new Point(1, 0.5), colors, stops);

            return brush;
        }

        /// <summary>
        /// Creates and returns a left gradient brush
        /// </summary>
        /// <param name="alpha">Alpha as Int32</param>
        /// <returns>Brush</returns>
        internal static Brush GetLeftGradianceBrush(Int32 alpha)
        {
            LinearGradientBrush gradBrush = new LinearGradientBrush();

            gradBrush.GradientStops = new GradientStopCollection();

            gradBrush.StartPoint = new Point(0, 1);
            gradBrush.EndPoint = new Point(1, 0);

            gradBrush.GradientStops.Add(Graphics.GetGradientStop(Color.FromArgb((byte)alpha, 0, 0, 0), 0));
            gradBrush.GradientStops.Add(Graphics.GetGradientStop(Color.FromArgb(0, 0, 0, 0), 0));

            return gradBrush;
        }

        /// <summary>
        /// Creates and returns a line segment
        /// </summary>
        /// <param name="point">Point</param>
        /// <returns>LineSegment</returns>
        public static LineSegment GetLineSegment(Point point)
        {
            LineSegment lineSegment = new LineSegment();
            lineSegment.Point = point;
            return lineSegment;
        }

        /// <summary>
        /// Creates and returns an arc segment
        /// </summary>
        /// <param name="point">Point</param>
        /// <param name="size">Size</param>
        /// <param name="rotation">Rotation as Double</param>
        /// <param name="sweep">Sweep as SweepDirection</param>
        /// <returns>ArcSegment</returns>
        public static ArcSegment GetArcSegment(Point point, Size size, Double rotation, SweepDirection sweep)
        {
            ArcSegment arcSegment = new ArcSegment();
            arcSegment.Point = point;
            arcSegment.Size = size;
            arcSegment.RotationAngle = rotation;
            arcSegment.SweepDirection = SweepDirection.Clockwise;
            return arcSegment;
        }

        /// <summary>
        /// Convert scale based on the parameters
        /// </summary>
        /// <param name="fromScaleMin">FromScaleMin</param>
        /// <param name="fromScaleMax">FromScaleMax</param>
        /// <param name="fromValue">FromValue</param>
        /// <param name="toScaleMin">ToScaleMin</param>
        /// <param name="toScaleMax">ToScaleMax</param>
        /// <returns>Double</returns>
        public static Double ConvertScale(Double fromScaleMin, Double fromScaleMax, Double fromValue, Double toScaleMin, Double toScaleMax)
        {
            return ((fromValue - fromScaleMin) * (toScaleMax - toScaleMin) / (fromScaleMax - fromScaleMin)) + toScaleMin;
        }
        
       /// <summary>
        /// Converts value to pixel position
       /// </summary>
       /// <param name="positionMin">Min position</param>
       /// <param name="positionMax">Max position</param>
       /// <param name="valueMin">Min value</param>
       /// <param name="valueMax">Max value</param>
       /// <param name="value">Value</param>
       /// <returns>Double</returns>
        public static Double ValueToPixelPosition(Double positionMin, Double positionMax, Double valueMin, Double valueMax, Double value)
        {
            return ((value - valueMin) / (valueMax - valueMin)) * (positionMax - positionMin) + positionMin;
        }

        /// <summary>
        /// Converts pixel position to value
        /// </summary>
        /// <param name="positionMin">Min position</param>
        /// <param name="positionMax">Max position</param>
        /// <param name="valueMin">Min value</param>
        /// <param name="valueMax">Max value</param>
        /// <param name="position">position</param>
        /// <returns>Double</returns>
        public static Double PixelPositionToValue(Double positionMin, Double positionMax, Double valueMin, Double valueMax, Double position)
        {
            return ((position - positionMin) / (positionMax - positionMin) * (valueMax - valueMin)) + valueMin;
        }

        /// <summary>
        /// Creates and returns gradient stop
        /// </summary>
        /// <param name="color">Color as Color</param>
        /// <param name="stop">Stop as Double</param>
        /// <returns>GradientStop</returns>
        public static GradientStop GetGradientStop(Color color, Double stop)
        {
            GradientStop gradStop = new GradientStop();
            gradStop.Color = color;
            gradStop.Offset = stop;
            return gradStop;
        }

        /// <summary>
        /// Creates and returns a linear gradient brush
        /// </summary>
        /// <param name="angle">Angle</param>
        /// <param name="dateTime">Start point</param>
        /// <param name="end">End point</param>
        /// <param name="colors">List of color</param>
        /// <param name="stops">List of Double</param>
        /// <returns>Brush</returns>
        public static Brush CreateLinearGradientBrush(Double angle, Point start, Point end, List<Color> colors, List<Double> stops)
        { 
            LinearGradientBrush brush = new LinearGradientBrush();
            if (colors.Count != stops.Count)
                throw new Exception("Colors and Stops arrays don't match");

            brush.StartPoint = start;
            brush.EndPoint = end;
            brush.GradientStops = new GradientStopCollection();

            for (Int32 i = 0; i < colors.Count; i++)
            {
                brush.GradientStops.Add(GetGradientStop(colors[i],stops[i]));
            }

            RotateTransform rt = new RotateTransform();
            rt.Angle = angle;
            rt.CenterX = 0.5;
            rt.CenterY = 0.5;
            brush.RelativeTransform = rt;

            return brush;
        }

        public static Brush CreateSolidColorBrush(Color color)
        {
            SolidColorBrush brush = new SolidColorBrush();
            brush.Color = color;
            return brush;
        }

        /// <summary>
        /// Creates and returns bevel top brush
        /// </summary>
        /// <param name="brush">Brush</param>
        /// <param name="angle">Angle</param>
        /// <returns>Brush</returns>
        internal static Brush GetBevelTopBrush(Brush brush, Double angle)
        {
            if (brush != null)
            {
                if (typeof(SolidColorBrush).Equals(brush.GetType()))
                {
                    SolidColorBrush solidBrush = brush as SolidColorBrush;
                    Double r, g, b;
                    List<Color> colors = new List<Color>();
                    List<Double> stops = new List<Double>();

                    r = ((double)solidBrush.Color.R / (double)255) * 0.9999;
                    g = ((double)solidBrush.Color.G / (double)255) * 0.9999;
                    b = ((double)solidBrush.Color.B / (double)255) * 0.9999;

                    colors.Add(Graphics.GetLighterColor(solidBrush.Color, 0.99));
                    stops.Add(0);

                    colors.Add(Graphics.GetLighterColor(solidBrush.Color, 1 - r, 1 - g, 1 - b));
                    stops.Add(0.2);

                    colors.Add(Graphics.GetLighterColor(solidBrush.Color, 1 - r, 1 - g, 1 - b));
                    stops.Add(0.6);

                    colors.Add(Graphics.GetLighterColor(solidBrush.Color, 0.99));
                    stops.Add(1);

                    return Graphics.CreateLinearGradientBrush(angle, new Point(0, 0.5), new Point(1, 0.5), colors, stops);
                }
                else
                {
                    return brush;
                }
            }
            else
                return null;
        }

        /// <summary>
        /// Creates and returns bevel side brush
        /// </summary>
        /// <param name="angle">Angle</param>
        /// <param name="brush">Brush</param>
        /// <returns>Brush</returns>
        internal static Brush GetBevelSideBrush(Double angle, Brush brush)
        {
            return Graphics.GetLightingEnabledBrush(brush, angle, "Linear", new Double[] { 0.75, 0.97});
        }

        /// <summary>
        /// Creates and returns a bevel top brush
        /// </summary>
        /// <param name="brush">Brush</param>
        /// <returns>Brush</returns>
        internal static Brush GetBevelTopBrush(Brush brush)
        {
            return GetBevelTopBrush(brush, 90);
        }

        /// <summary>
        /// Creates and returns a lighting brush
        /// </summary>
        /// <param name="brush">Brush</param>
        /// <returns>Brush</returns>
        internal static Brush GetLightingEnabledBrush3D(Brush brush)
        {
            return GetLightingEnabledBrush(brush, "Linear", new Double[] { 0.65, 0.55 });
        }

        //private static Dictionary<Color, Brush> _lightingColorCache = new Dictionary<Color, Brush>();
        
        /// <summary>
        /// Creates and returns a lighting brush
        /// </summary>
        /// <param name="brush">Brush</param>
        /// <param name="angle">Angle</param>
        /// <param name="type">Type as String</param>
        /// <param name="colorIntensies">Array of Double</param>
        /// <returns>Brush</returns>
        internal static Brush GetLightingEnabledBrush(Brush brush, Double angle, String type, Double[] colorIntensies)
        {
            if (brush != null)
            {
                SolidColorBrush solidColorBrush = brush as SolidColorBrush;

                if (solidColorBrush != null)
                {

                    //if (_lightingColorCache.ContainsKey(solidColorBrush.Color))
                    //    return _lightingColorCache[solidColorBrush.Color];
                    //else
                    //{
                        if (colorIntensies == null)
                            colorIntensies = new Double[] { 0.745, 0.99 };

                        SolidColorBrush solidBrush = brush as SolidColorBrush;

                        List<Color> colors = new List<Color>();
                        List<Double> stops = new List<Double>();

                        colors.Add(Graphics.GetDarkerColor(solidBrush.Color, colorIntensies[0]));
                        stops.Add(0);

                        colors.Add(Graphics.GetDarkerColor(solidBrush.Color, colorIntensies[1]));
                        stops.Add(1);
                                                
                        if (type == "Radial")
                            brush = Graphics.CreateRadialGradientBrush(colors, stops);
                        else
                            brush = Graphics.CreateLinearGradientBrush(angle, new Point(0, 0.5), new Point(1, 0.5), colors, stops);

                       //_lightingColorCache.Add(solidColorBrush.Color, brush);
                        return brush;
                    //}
                }
                else
                {   
                    return brush;
                }
            }
            else
            {
                return brush;
            }

        }
 
        /// <summary>
        /// Creates and returns a lighting brush
        /// </summary>
        /// <param name="brush">Brush</param>
        /// <param name="type">Type as String</param>
        /// <param name="colorIntensies">Array of Double</param>
        /// <returns>Brush</returns>
        internal static Brush GetLightingEnabledBrush(Brush brush, String type, Double[] colorIntensies)
        {
             return GetLightingEnabledBrush(brush, -90, type, colorIntensies);
        }

        
        /// <summary>
        /// Creates and returns a radial gradient brush
        /// </summary>
        /// <param name="colors">List of color</param>
        /// <param name="stops">List of Double</param>
        /// <returns>Brush</returns>
        public static Brush CreateRadialGradientBrush(List<Color> colors, List<Double> stops)
        {
            RadialGradientBrush brush = new RadialGradientBrush();
            if (colors.Count != stops.Count)
                throw new Exception("Colors and Stops arrays don't match");

            brush.GradientStops = new GradientStopCollection();

            for (Int32 i = 0; i < colors.Count; i++)
            {
                brush.GradientStops.Add(GetGradientStop(colors[i], stops[i]));
            }

            return brush;
        }

        /// <summary>
        /// Creates and returns a brush intensity
        /// </summary>
        /// <param name="brush">Brush</param>
        /// <returns>Double</returns>
        public static Double GetBrushIntensity(Brush brush)
        {
            Color color = new Color();
            Double intensity = 0;
            if (brush == null) return 1;
            if (brush.GetType().Name == "SolidColorBrush")
            {
                color = (brush as SolidColorBrush).Color;
                intensity = (Double)(color.R + color.G + color.B) / (3 * 255);
            }
            else if (brush.GetType().Name == "LinearGradientBrush" || brush.GetType().Name == "RadialGradientBrush")
            {
                foreach (GradientStop grad in (brush as GradientBrush).GradientStops)
                {
                    color = grad.Color;
                    intensity += (Double)(color.R + color.G + color.B) / (3 * 255);
                }

                intensity /= (brush as GradientBrush).GradientStops.Count;
            }
            else
            {
                intensity = 1;
            }
            return intensity;
        }

        public static KeyValuePair<Color, Double> GetColorIntensity(Color color)
        {
            Double intensity = 0;
            intensity = (Double)(color.R + color.G + color.B) / (3 * 255);
            return new KeyValuePair<Color, double>(color, intensity);
        }

        /// <summary>
        /// Creates and returns a default font color based on intensity
        /// </summary>
        /// <param name="intensity">Intensity as Double</param>
        /// <returns>Brush</returns>
        public static Brush GetDefaultFontColor(Double intensity)
        {
            Brush brush = null;
            if (intensity < 0.6)
            {
                //brush = ParseSolidColor("#EFEFEF");
                brush = AUTO_WHITE_FONT_BRUSH;
            }
            else
            {
                //brush = ParseSolidColor("#000000");
                brush = AUTO_BLACK_FONT_BRUSH;
            }
            return brush;
        }

        /// <summary>
        /// Compare brushes
        /// </summary>
        /// <param name="first">First brush</param>
        /// <param name="second">Second brush</param>
        /// <returns></returns>
        internal static bool AreBrushesEqual(Brush first, Brush second)
        {
            // If the default comparison is true, that's good enough.
            if (object.Equals(first, second))
            {
                return true;
            }

            // Do a field by field comparison if they're not the same reference
            SolidColorBrush firstSolidColorBrush = first as SolidColorBrush;
            if (firstSolidColorBrush != null)
            {
                SolidColorBrush secondSolidColorBrush = second as SolidColorBrush;
                if (secondSolidColorBrush != null)
                {
                    return object.Equals(firstSolidColorBrush.Color, secondSolidColorBrush.Color);
                }
            }

            return false;
        }

        /// <summary>
        /// Converts a color in String form to Solid Color Brush
        /// </summary>
        /// <param name="colorCode">ColorCode as String</param>
        /// <returns>Brush</returns>
        public static Brush ParseSolidColor(String colorCode)
        {
#if WPF
            return (Brush)XamlReader.Load(new XmlTextReader(new System.IO.StringReader(String.Format(System.Globalization.CultureInfo.InvariantCulture, @"<SolidColorBrush xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" Color=""{0}""></SolidColorBrush>", colorCode))));
#else
            return (Brush)XamlReader.Load(String.Format(System.Globalization.CultureInfo.InvariantCulture, @"<SolidColorBrush xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" Color=""{0}""></SolidColorBrush>", colorCode));
#endif
        }

        /// <summary>
        /// Return lighter brush
        /// </summary>
        public static Brush GetLighterBrush(Brush brush, Double intensity)
        {   
            SolidColorBrush solidColorBrush = brush as SolidColorBrush;

            if (solidColorBrush != null)
                return new SolidColorBrush(GetLighterColor(solidColorBrush.Color, intensity));
            else
                return brush;
        }
        
        /// <summary>
        /// Return darker brush
        /// </summary>
        public static Brush GetDarkerBrush(Brush brush, Double intensity)
        {   
            SolidColorBrush solidColorBrush = brush as SolidColorBrush;

            if (solidColorBrush != null)
                return new SolidColorBrush(GetDarkerColor(solidColorBrush.Color, intensity));
            else
                return brush;
        }

        /// <summary>
        /// Returns a darker shade of the color by decreasing the brightness by the given intensity value
        /// </summary>
        /// <param name="color">Color as Color</param>
        /// <param name="intensity">Intensity as Double</param>
        /// <returns></returns>
        public static Color GetDarkerColor(Color color, Double intensity)
        {
            Color darkerShade = new Color();
            intensity = (intensity < 0 || intensity > 1) ? 1 : intensity;
            darkerShade.R = (Byte)(color.R * intensity);
            darkerShade.G = (Byte)(color.G * intensity);
            darkerShade.B = (Byte)(color.B * intensity);
            darkerShade.A = color.A;
            return darkerShade;
        }

        /// <summary>
        /// Returns a lighter shade of the color by increasing the brightness by the given intensity value
        /// </summary>
        /// <param name="color">Color as Color</param>
        /// <param name="intensity">Intensity as Double</param>
        /// <returns></returns>
        public static Color GetLighterColor(Color color, Double intensity)
        {
            Color lighterShade = new Color();
            intensity = (intensity < 0 || intensity > 1) ? 1 : intensity;
            lighterShade.R = (Byte)(256 - ((256 - color.R) * intensity));
            lighterShade.G = (Byte)(256 - ((256 - color.G) * intensity));
            lighterShade.B = (Byte)(256 - ((256 - color.B) * intensity));
            lighterShade.A = color.A;
            return lighterShade;
        }

        /// <summary>
        /// Creates and returns a lighter color based on the parameters
        /// </summary>
        /// <param name="color">Color as Color</param>
        /// <param name="intensityR">IntensityR as Double</param>
        /// <param name="intensityG">IntensityG as Double</param>
        /// <param name="intensityB">IntensityB as Double</param>
        /// <returns>Color</returns>
        public static Color GetLighterColor(Color color, Double intensityR, Double intensityG, Double intensityB)
        {
            Color lighterShade = new Color();
            intensityR = (intensityR < 0 || intensityR > 1) ? 1 : intensityR;
            intensityG = (intensityG < 0 || intensityG > 1) ? 1 : intensityG;
            intensityB = (intensityB < 0 || intensityB > 1) ? 1 : intensityB;
            lighterShade.R = (Byte)(256 - ((256 - color.R) * intensityR));
            lighterShade.G = (Byte)(256 - ((256 - color.G) * intensityG));
            lighterShade.B = (Byte)(256 - ((256 - color.B) * intensityB));
            lighterShade.A = color.A;
            return lighterShade;
        }

        /// <summary>
        /// Converts LineStyle to StrokeDashArray
        /// </summary>
        /// <param name="lineStyle">lineStyle as String</param>
        /// <returns>DashArray as DoubleCollection</returns>
        public static DoubleCollection LineStyleToStrokeDashArray(String lineStyle)
        {
            DoubleCollection retVal = null;

            switch (lineStyle)
            {
                case "Solid":
                    retVal = null;
                    break;
                case "Dashed":
                    retVal = new DoubleCollection() { 4, 4, 4, 4 };
                    break;
                case "Dotted":
                    retVal = new DoubleCollection() { 1, 2, 1, 2 };
                    break;
            }

            return retVal;
        }

        /// <summary>
        /// Creates and returns a lighting brush
        /// </summary>
        /// <param name="lightingEnabled">Whether lighting is enabled</param>
        /// <returns>Brush</returns>
        public static Brush LightingBrush(Boolean lightingEnabled)
        {
            Brush brush;

            if (lightingEnabled)
            {
                String xaml = String.Format(@"<LinearGradientBrush EndPoint=""0.5,1"" StartPoint=""0.5,0"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
                                                <GradientStop Color=""#A0FFFFFF"" Offset=""0""/>
                                                <GradientStop Color=""#00FFFFFF"" Offset=""1""/>
                                          </LinearGradientBrush>");

#if WPF
                brush = (Brush)XamlReader.Load(new XmlTextReader(new System.IO.StringReader(xaml)));
#else
                brush = System.Windows.Markup.XamlReader.Load(xaml) as Brush;
#endif
            }
            else
                brush = Graphics.TRANSPARENT_BRUSH;

            return brush;
        }

        #endregion

        #region Constants


        public static Brush AUTO_WHITE_FONT_BRUSH
        {
             get
             {
#if SL
                 return _AUTO_WHITE_FONT_BRUSH;
#else
                return new SolidColorBrush(Color.FromArgb(255, 239, 239, 239));
#endif

             }
        }

        public static Brush AUTO_BLACK_FONT_BRUSH
        {
             get
             {
#if SL
                 return _AUTO_BLACK_FONT_BRUSH;
#else
                return new SolidColorBrush(Color.FromArgb(255, 51, 51, 51));
#endif

             }
        }

        public static Brush GRAY_BRUSH
        {
             get
             {
#if SL
                 return _GRAY_BRUSH;
#else
                return new SolidColorBrush(Colors.Gray);
#endif

             }
        }

        public static Brush BLACK_BRUSH
        {
             get
             {
#if SL
                 return _BLACK_BRUSH;
#else
                return new SolidColorBrush(Colors.Black);
#endif

             }
        }

        public static Brush RED_BRUSH
        {
             get
             {
#if SL
                 return _RED_BRUSH;
#else
                return new SolidColorBrush(Colors.Red);
#endif

             }
        }

        public static Brush ORANGE_BRUSH
        {
             get
             {
#if SL
                 return _ORANGE_BRUSH;
#else
                return new SolidColorBrush(Colors.Orange);
#endif

             }
        }

        
        public static Brush WHITE_BRUSH
        {
             get
             {
#if SL
                 return _WHITE_BRUSH;
#else
                return new SolidColorBrush(Colors.White);
#endif

             }
        }

        public static Brush TRANSPARENT_BRUSH
        {
             get
             {
#if SL
                 return _TRANSPARENT_BRUSH;
#else
                return new SolidColorBrush(Colors.Transparent);
#endif

             }
        }

#if SL
        private static Brush _AUTO_WHITE_FONT_BRUSH = new SolidColorBrush(Color.FromArgb(255, 239, 239, 239));
        private static Brush _AUTO_BLACK_FONT_BRUSH = new SolidColorBrush(Color.FromArgb(255, 72, 72, 72));
        private static Brush _GRAY_BRUSH = new SolidColorBrush(Colors.Gray);
        private static Brush _BLACK_BRUSH = new SolidColorBrush(Colors.Black);
        private static Brush _RED_BRUSH = new SolidColorBrush(Colors.Red);
        private static Brush _ORANGE_BRUSH = new SolidColorBrush(Colors.Orange);
        private static Brush _WHITE_BRUSH = new SolidColorBrush(Colors.White);
        private static Brush _TRANSPARENT_BRUSH = new SolidColorBrush(Colors.Transparent);
#endif

        /// <summary>
        /// Array of font sizes
        /// </summary>
        public static Double[] DefaultFontSizes = { 8, 10, 12, 14, 16, 18, 20, 24, 28, 32, 36, 40 };

        #endregion
    }
}



