using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DirectShowLib;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Orientation = System.Windows.Forms.Orientation;

namespace EmguCvWinforms
{
    public partial class Form1 : Form
    {
        private static bool _running;
        private VideoCapture _capture;

        public Form1()
        {
            InitializeComponent();

            DetectionMethod.SelectedIndex = 0;
            Resolution.SelectedIndex = 0;
            Resolution.SelectedIndexChanged += (sender, args) =>
            {
                if (Resolution.SelectedIndex == 0)
                {
                    _capture.SetCaptureProperty(CapProp.FrameWidth, 640);
                    _capture.SetCaptureProperty(CapProp.FrameHeight, 480);
                }
                else if (Resolution.SelectedIndex == 1)
                {
                    _capture.SetCaptureProperty(CapProp.FrameWidth, 1280);
                    _capture.SetCaptureProperty(CapProp.FrameHeight, 720);
                }
                else if (Resolution.SelectedIndex == 2)
                {
                    _capture.SetCaptureProperty(CapProp.FrameWidth, 1920);
                    _capture.SetCaptureProperty(CapProp.FrameHeight, 1080);
                }
            };
            splitContainer1.SplitterDistance = (splitContainer1.Width - 2) / 2;
            ReorientSplitter();
            ResizeEnd += (sender, args) => ReorientSplitter();

            var systemCameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            _capture = new VideoCapture(captureApi: VideoCapture.API.DShow);

            Cameras.DataSource = systemCameras;
            Cameras.DisplayMember = nameof(DsDevice.Name);
            Cameras.SelectedIndexChanged += (sender, args) => { _capture = new VideoCapture(Cameras.SelectedIndex, VideoCapture.API.DShow); };

            Application.Idle += (sender, args) =>
            {
                RunIfNotRunning(() =>
                {
                    var (form, contours) = FindFormUsingSelectedMethod(_capture.QueryFrame());
                    FormImage.Image = form;
                    ContourImage.Image = contours;
                });
            };
        }

        void ReorientSplitter()
        {
            splitContainer1.Orientation = Width > Height ? Orientation.Vertical : Orientation.Horizontal;
        }

        (IImage, IImage) FindFormUsingSelectedMethod(IImage image)
        {
            switch (DetectionMethod.SelectedIndex)
            {
                case 1:
                    return FindForm2(image);
                default:
                    return FindForm(image);
            }
        }

        void RunIfNotRunning(Action a)
        {
            if (_running) return;

            _running = true;
            a();
            _running = false;
        }

        (IImage, IImage) FindForm(IImage image)
        {
            var originalImage = image.Clone() as IImage;

            using (var smallerImage = new Image<Bgr, byte>(image.Bitmap).Resize(1, Inter.Cubic))
            {
                image.Dispose();
                CvInvoke.CvtColor(smallerImage, smallerImage, ColorConversion.Bgr2Rgb);
                CvInvoke.CvtColor(smallerImage, smallerImage, ColorConversion.Bgr2Gray);

                using (var filtered = new Mat())
                {
                    CvInvoke.BilateralFilter(smallerImage, filtered, 9, 75, 75);

                    CvInvoke.AdaptiveThreshold(filtered, filtered, 255, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 115, 4);
                    CvInvoke.MedianBlur(filtered, filtered, 11);
                    CvInvoke.CopyMakeBorder(filtered, filtered, 5, 5, 5, 5, BorderType.Constant, new MCvScalar(0, 0, 0));

                    using (var edges = new Mat())
                    {
                        CvInvoke.Canny(filtered, edges, 200, 250);
                        var contours = new VectorOfVectorOfPoint();
                        var hierarchy = new Mat();
                        CvInvoke.FindContours(edges, contours, hierarchy, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);

                        var height = edges.Height;
                        var width = edges.Width;

                        var maxContourArea = (width - 50) * (height - 50);
                        var maxAreaFound = maxContourArea * 0.25;

                        var imageWithContours = originalImage.Clone() as IImage;
                        var pageContour = new VectorOfPoint();

                        for (var x = 0; x < contours.Size; x++)
                        {
                            var contour = contours[x];

                            var perimeter = CvInvoke.ArcLength(contour, true);
                            var approx = new VectorOfPoint();
                            CvInvoke.ApproxPolyDP(contour, approx, 0.03 * perimeter, true);

                            var isContourConvex = CvInvoke.IsContourConvex(approx);
                            var contourArea = CvInvoke.ContourArea(approx);
                            var approxSize = approx.Size;

                            if (approxSize == 4 && isContourConvex)
                                CvInvoke.DrawContours(imageWithContours, new VectorOfVectorOfPoint(SortContourAndScale(approx, 1)), -1, new Bgr(Color.SpringGreen).MCvScalar, 2);

                            if (approxSize == 4
                                && isContourConvex
                                && maxAreaFound < contourArea
                                && contourArea < maxContourArea)
                            {
                                maxAreaFound = contourArea;
                                pageContour = approx;
                            }
                        }

                        if (pageContour.Size != 4) return (originalImage, imageWithContours);

                        pageContour = SortContourAndScale(pageContour, 1);

                        width = Math.Max(pageContour[1].X - pageContour[0].X, pageContour[2].X - pageContour[3].X);
                        height = Math.Max(pageContour[3].Y - pageContour[0].Y, pageContour[2].Y - pageContour[1].Y);

                        // top left, top right, bottom right, bottom left
                        var destRectangle = new[] { new PointF(0, 0), new PointF(width, 0), new PointF(width, height), new PointF(0, height), };
                        var sourceRectangle = new[] { new PointF(pageContour[0].X, pageContour[0].Y), new PointF(pageContour[1].X, pageContour[1].Y), new PointF(pageContour[2].X, pageContour[2].Y), new PointF(pageContour[3].X, pageContour[3].Y) };

                        using (var transform = CvInvoke.GetPerspectiveTransform(sourceRectangle, destRectangle))
                        {
                            var outputImage = new Mat();
                            CvInvoke.WarpPerspective(originalImage, outputImage, transform, new Size(width, height));
                            originalImage.Dispose();

                            return (outputImage, imageWithContours);
                        }
                    }
                }
            }
        }

        VectorOfPoint SortContourAndScale(VectorOfPoint contour, int scaleTo)
        {
            // Sort corners to be top left, top right, bottom right, bottom left
            var cArray = contour.ToArray();

            var leftSide = cArray.OrderBy(c => c.X).Take(2).OrderBy(c => c.Y).ToList();
            var rightSide = cArray.OrderByDescending(c => c.X).Take(2).OrderBy(c => c.Y).ToList();

            var topLeft = leftSide.First();
            var topRight = rightSide.First();
            var bottomRight = rightSide.Last();
            var bottomLeft = leftSide.Last();

            return new VectorOfPoint(new[] {
                new Point(topLeft.X * scaleTo, topLeft.Y * scaleTo),
                new Point(topRight.X * scaleTo, topRight.Y * scaleTo),
                new Point(bottomRight.X * scaleTo, bottomRight.Y * scaleTo),
                new Point(bottomLeft.X * scaleTo, bottomLeft.Y * scaleTo)});
        }

        (IImage, IImage) FindForm2(IImage image)
        {
            var uimage = new UMat();
            CvInvoke.CvtColor(image, uimage, ColorConversion.Bgr2Gray);

            //use image pyr to remove noise
            var pyrDown = new UMat();
            CvInvoke.PyrDown(uimage, pyrDown);
            CvInvoke.PyrUp(pyrDown, uimage);

            var cannyThreshold = 180.0;
            var cannyThresholdLinking = 120.0;
            var cannyEdges = new UMat();
            CvInvoke.Canny(uimage, cannyEdges, cannyThreshold, cannyThresholdLinking);

            var boxList = new List<RotatedRect>(); //a box is a rotated rectangle

            using (var contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(cannyEdges, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
                var count = contours.Size;
                for (var i = 0; i < count; i++)
                {
                    using (var contour = contours[i])
                    using (var approxContour = new VectorOfPoint())
                    {
                        CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * 0.05, true);
                        if (CvInvoke.ContourArea(approxContour, false) > 250) //only consider contours with area greater than 250
                        {
                            if (approxContour.Size == 4) //The contour has 4 vertices.
                            {
                                #region determine if all the angles in the contour are within [80, 100] degree
                                var isRectangle = true;
                                var pts = approxContour.ToArray();
                                var edges = PointCollection.PolyLine(pts, true);

                                for (var j = 0; j < edges.Length; j++)
                                {
                                    var angle = Math.Abs(
                                       edges[(j + 1) % edges.Length].GetExteriorAngleDegree(edges[j]));
                                    if (angle < 80 || angle > 100)
                                    {
                                        isRectangle = false;
                                        break;
                                    }
                                }
                                #endregion

                                if (isRectangle) boxList.Add(CvInvoke.MinAreaRect(approxContour));
                            }
                        }
                    }
                }
            }

            var triangleRectangleImage = new Image<Bgr, byte>(image.Bitmap);
            foreach (var box in boxList)
                triangleRectangleImage.Draw(box, new Bgr(Color.DarkOrange), 2);

            if (boxList.Any())
            {
                var biggestBox = boxList.OrderBy(c => c.Size.Height * c.Size.Width).Last();
                if (biggestBox.Size.Width * biggestBox.Size.Height < image.Size.Width * image.Size.Height / 3.0)
                    return (image, triangleRectangleImage);

                var destRectangle = new[] { new PointF(0, 0), new PointF(0, image.Size.Height), new PointF(image.Size.Width, image.Size.Height), new PointF(image.Size.Width, 0) };
                var sourceRectangle = new[]
                {
                    new PointF(biggestBox.GetVertices()[0].X, biggestBox.GetVertices()[0].Y), new PointF(biggestBox.GetVertices()[1].X, biggestBox.GetVertices()[1].Y),
                    new PointF(biggestBox.GetVertices()[2].X, biggestBox.GetVertices()[2].Y), new PointF(biggestBox.GetVertices()[3].X, biggestBox.GetVertices()[3].Y)
                };

                var transform = CvInvoke.GetPerspectiveTransform(sourceRectangle, destRectangle);
                var outputImage = new Mat();
                CvInvoke.WarpPerspective(image, outputImage, transform, image.Size);
                return (outputImage, triangleRectangleImage);
            }

            return (image, triangleRectangleImage);
        }
    }
}
