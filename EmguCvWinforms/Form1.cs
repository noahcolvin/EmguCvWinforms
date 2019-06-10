using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using DirectShowLib;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace EmguCvWinforms
{
    public partial class Form1 : Form
    {
        private static bool _running = false;
        public Form1()
        {
            InitializeComponent();

            DetectionMethod.SelectedIndex = 0;

            var systemCameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            var capture = new VideoCapture(systemCameras.Length - 1);

            Application.Idle += (sender, args) =>
            {
                OriginalImage.Image = capture.QueryFrame();
                RunIfNotRunning(() =>
                {
                    var (form, contours) = FindFormUsingSelectedMethod(capture.QueryFrame());
                    FormImage.Image = form;
                    ContourImage.Image = contours;
                });
            };
        }

        (IImage, IImage) FindFormUsingSelectedMethod(IImage image)
        {
            switch (DetectionMethod.SelectedIndex)
            {
                case 1:
                    return FindForm2(image);
                case 2:
                    return FindForm3(image);
                case 3:
                    return FindForm4(image);
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
            CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Rgb);

            Write(image, 1);
            CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Gray);
            Write(image, 2);

            var copy = new Mat();
            CvInvoke.BilateralFilter(image, copy, 9, 75, 75);
            image = copy;
            Write(image, 3);

            CvInvoke.AdaptiveThreshold(image, image, 255, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 115, 4);
            Write(image, 4);
            CvInvoke.MedianBlur(image, image, 11);
            Write(image, 5);
            CvInvoke.CopyMakeBorder(image, image, 5, 5, 5, 5, BorderType.Constant, new MCvScalar(0, 0, 0));
            Write(image, 6);

            var edges = new Mat();
            CvInvoke.Canny(image, edges, 200, 250);
            Write(edges, 7);

            var contours = new VectorOfVectorOfPoint();
            var hierarchy = new Mat();
            CvInvoke.FindContours(edges, contours, hierarchy, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);

            var height = edges.Height;
            var width = edges.Width;
            var maxContourArea = (width - 10) * (height - 10);
            var maxAreaFound = maxContourArea * 0.3;

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
                    CvInvoke.DrawContours(imageWithContours, new VectorOfVectorOfPoint(approx), -1, new Bgr(Color.SpringGreen).MCvScalar, 2);

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

            //width = Math.Max(Math.Max(pageContour[0].X, pageContour[1].X), Math.Max(pageContour[2].X, pageContour[3].X));
            //height = Math.Max(Math.Max(pageContour[0].Y, pageContour[1].Y), Math.Max(pageContour[2].Y, pageContour[3].Y));
            width = Math.Max(pageContour[0].X - pageContour[1].X, pageContour[3].X - pageContour[2].X);
            height = Math.Max(pageContour[2].Y - pageContour[1].Y, pageContour[3].Y - pageContour[0].Y);

            // top right, top left, bottom left, bottom right
            var destRectangle = new[] { new PointF(width, 0), new PointF(0, 0), new PointF(0, height), new PointF(width, height), };
            var sourceRectangle = new[] { new PointF(pageContour[0].X, pageContour[0].Y), new PointF(pageContour[1].X, pageContour[1].Y), new PointF(pageContour[2].X, pageContour[2].Y), new PointF(pageContour[3].X, pageContour[3].Y) };

            var transform = CvInvoke.GetPerspectiveTransform(sourceRectangle, destRectangle);
            var outputImage = new Mat();
            var tr = Transform(pageContour);
            CvInvoke.WarpPerspective(originalImage, outputImage, transform, new Size(width, height));
            Write(outputImage, 8);

            return (outputImage, imageWithContours);
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

        (IImage, IImage) FindForm3(IImage image)
        {
            var i = new Image<Bgr, byte>(image.Bitmap);
            var ratio = image.Size.Height / 500.0;
            var orig = image.Clone() as IImage;
            image = i;//.Resize(0, 500, Inter.Cubic, true);

            var gray = new Mat();
            CvInvoke.CvtColor(image, gray, ColorConversion.Bgr2Gray);
            CvInvoke.GaussianBlur(gray, gray, new Size(5, 5), 0);

            var edged = new Mat();
            CvInvoke.Canny(gray, edged, 75, 200);

            var contours = new VectorOfVectorOfPoint();
            var hierarchy = new Mat();
            CvInvoke.FindContours(edged.Clone(), contours, hierarchy, RetrType.List, ChainApproxMethod.ChainApproxSimple);
            var imageWithContours = orig.Clone() as IImage;
            contours = SortedContours(contours, imageWithContours);

            VectorOfPoint screenCnt = null;//new VectorOfPoint();

            for (var j = 0; j < contours.Size; j++)
            {
                var c = contours[j];
                var peri = CvInvoke.ArcLength(c, true);
                var approx = new VectorOfPoint();
                CvInvoke.ApproxPolyDP(c, approx, 0.02 * peri, true);

                if (approx.Size == 4)
                {
                    screenCnt = approx;
                    break;
                }
            }

            if (screenCnt == null)
                return (orig, imageWithContours);

            var t = Transform(screenCnt);

            var outputImage = new Mat();
            CvInvoke.WarpPerspective(orig, outputImage, t.Item1, orig.Size);

            return (outputImage, imageWithContours);
        }

        (IImage, IImage) FindForm4(IImage image)
        {
            Mat mainMat = new Mat();
            Mat grayMat = new Mat();

            //convert texture2d to matrix
            //Utils.texture2DToMat(baseTexture, mainMat);
            //copy main matrix to grayMat
            //mainMat.copyTo(grayMat);

            //convert color to gray
            CvInvoke.CvtColor(image, grayMat, ColorConversion.Bgr2Gray);
            //blur the image
            CvInvoke.GaussianBlur(grayMat, grayMat, new Size(5, 5), 0);

            //thresholding make the image black and white
            CvInvoke.Threshold(grayMat, grayMat, 0, 255, ThresholdType.Otsu);
            //extract the edge of the image
            CvInvoke.Canny(grayMat, grayMat, 50, 50);


            //prepare for the finding contours
            var contours = new VectorOfVectorOfPoint();
            //find the contour from canny edge image
            CvInvoke.FindContours(grayMat, contours, new Mat(), RetrType.External, ChainApproxMethod.ChainApproxSimple);


            var tempTargets = new VectorOfVectorOfPoint();
            for (int i = 0; i < contours.Size; i++)
            {
                var cp = contours[i];
                var cn = new VectorOfPointF(cp.ToArray().Select(c => new PointF(c.X, c.Y)).ToArray());
                double p = CvInvoke.ArcLength(cn, true);

                var approx = new VectorOfPointF();
                //convert contour to readable polygon
                CvInvoke.ApproxPolyDP(cn, approx, 0.03 * p, true);

                // find a contour with 4 points
                if (approx.Size == 4)
                {
                    var approxPt = new VectorOfPoint();
                    //approx.convertTo(approxPt, CvType.CV_32S);

                    float maxCosine = 0;
                    for (int j = 2; j < 5; j++)
                    {
                        var v1 = new Vector2((float)(approx[j % 4].X - approx[j - 1].X), (float)(approx[j % 4].Y - approx[j - 1].Y));
                        var v2 = new Vector2((float)(approx[j - 2].X - approx[j - 1].X), (float)(approx[j - 2].Y - approx[j - 1].Y));

                        //float angle = Math.Abs(Vector2.Angle(v1, v2));
                        var angle = Math.Abs(Math.Atan2(v2.Y - v1.Y, v2.X - v1.X));
                        maxCosine = Math.Max(maxCosine, (float)angle);
                    }

                    if (maxCosine < 135f)
                    {
                        tempTargets.Push(approxPt);
                    }

                }

            }

            if (tempTargets.Size > 0)
            {
                //get the first contour
                VectorOfPoint approxPt = tempTargets[0];
                //making source mat
                //Mat srcPointsMat = Converters.vector_Point_to_Mat(approxPt.toList(), CvType.CV_32F);

                var src = new[] { new PointF(approxPt[0].X, approxPt[0].Y), new PointF(approxPt[1].X, approxPt[1].Y), new PointF(approxPt[2].X, approxPt[2].Y), new PointF(approxPt[3].X, approxPt[3].Y) };
                var dest = new[] { new PointF(0, 0), new PointF(0, 512), new PointF(512, 512), new PointF(512, 0) };

                //making destination mat
                /*List<Point> dstPoints = new List<Point>();
                dstPoints.Add(new Point(0, 0)    );
                dstPoints.Add(new Point(0, 512)  );
                dstPoints.Add(new Point(512, 512));
                dstPoints.Add(new Point(512, 0)  );*/
                //Mat dstPointsMat = Converters.vector_Point_to_Mat(dstPoints, CvType.CV_32F);

                //make perspective transform
                //Mat M = CvInvoke.GetPerspectiveTransform(srcPointsMat, dstPointsMat);
                Mat M = CvInvoke.GetPerspectiveTransform(src, dest);
                Mat warpedMat = new Mat(mainMat.Size, mainMat.Depth, 8);
                //crop and warp the image
                CvInvoke.WarpPerspective(mainMat, warpedMat, M, new Size(512, 512), Inter.Linear);
                //warpedMat.convertTo(warpedMat, CvType.CV_8UC3);

                return (warpedMat, warpedMat);

                /*//create a empty final texture
                Texture2D finalTexture = new Texture2D(warpedMat.width(), warpedMat.height(), TextureFormat.RGB24, false);
                //convert matrix to texture 2d
                Utils.matToTexture2D(warpedMat, finalTexture);

                targetRawImage.texture = finalTexture;*/

            }

            return (image, image);
        }

        (Mat, Size) Transform(VectorOfPoint vectorOfPoint)
        {
            var topLeft = vectorOfPoint[0];
            var topRight = vectorOfPoint[3];
            var bottomRight = vectorOfPoint[2];
            var bottomLeft = vectorOfPoint[1];

            var furthestLeft = Math.Min(topLeft.X, bottomLeft.X);
            var furthestRight = Math.Max(topRight.X, bottomRight.X);
            var furthestTop = Math.Min(topLeft.Y, topRight.Y);
            var furthestBottom = Math.Max(bottomLeft.Y, bottomRight.Y);

            var width = furthestRight - furthestLeft;
            var height = furthestBottom - furthestTop;

            var source = new[] { new PointF(topLeft.Y, topLeft.X), new PointF(topRight.Y, topRight.X), new PointF(bottomRight.Y, bottomRight.X), new PointF(bottomLeft.Y, bottomLeft.X) };
            var destination = new[] { new PointF(0, 0), new PointF(width - 1, 0), new PointF(width - 1, height - 1), new PointF(0, height - 1), };

            return (CvInvoke.GetPerspectiveTransform(source, destination), new Size(width, height));
        }

        VectorOfVectorOfPoint SortedContours(VectorOfVectorOfPoint contours, IImage imageWithContours)
        {
            var dict = new Dictionary<VectorOfPoint, double>();

            var count = contours.Size;
            for (var i = 0; i < count; i++)
            {
                using (var contour = contours[i])
                using (var approxContour = new VectorOfPoint())
                {
                    if (contour.Size == 4)
                    {
                        CvInvoke.DrawContours(imageWithContours, new VectorOfVectorOfPoint(contour), -1, new Bgr(Color.SpringGreen).MCvScalar, 2);
                        dict.Add(contour, CvInvoke.ContourArea(contour));
                    }
                }
            }

            var orderedDict = dict.OrderByDescending(c => c.Value);
            var sorted = new VectorOfVectorOfPoint();
            orderedDict.ToList().ForEach(x => sorted.Push(x.Key));
            return sorted;
        }

        VectorOfPoint GrabContours(VectorOfVectorOfPoint contours)
        {
            if (contours.Size == 2)
                return contours[0];
            if (contours.Size == 3)
                return contours[1];

            return null;
        }

        void Write(IImage image, int number)
        {
            //CvInvoke.Imwrite($"sample{number}.jpg", image);
        }

        Mat ResizeImage(Mat image, int height = 800)
        {
            var output = new Mat();
            var rat = (decimal)height / image.Height;
            CvInvoke.Resize(image, output, new Size((int)(image.Width * rat), height));
            return output;
        }
    }
}
