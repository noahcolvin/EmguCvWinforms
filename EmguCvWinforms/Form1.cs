using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using DirectShowLib;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;

namespace EmguCvWinforms
{
    public partial class Form1 : Form
    {
        private static bool _running = false;
        public Form1()
        {
            InitializeComponent();

            var systemCameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            var capture = new VideoCapture(systemCameras.Length - 1);

            Application.Idle += (sender, args) =>
            {
                OriginalImage.Image = capture.QueryFrame();
                RunIfNotRunning(() =>
                {
                    var (form, contours) = FindForm(capture.QueryFrame());
                    FormImage.Image = form;
                    ContourImage.Image = contours;
                });
            };
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
            //var image = new Mat();
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
                    CvInvoke.DrawContours(imageWithContours, new VectorOfVectorOfPoint(approx), -1, new Bgr(Color.SpringGreen).MCvScalar);

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

            width = Math.Max(Math.Max(pageContour[0].X, pageContour[1].X), Math.Max(pageContour[2].X, pageContour[3].X));
            height = Math.Max(Math.Max(pageContour[0].Y, pageContour[1].Y), Math.Max(pageContour[2].Y, pageContour[3].Y));

            var destRectangle = new[] { new PointF(0, 0), new PointF(0, height), new PointF(width, height), new PointF(width, 0) };
            var sourceRectangle = new[] { new PointF(pageContour[0].X, pageContour[0].Y), new PointF(pageContour[1].X, pageContour[1].Y), new PointF(pageContour[2].X, pageContour[2].Y), new PointF(pageContour[3].X, pageContour[3].Y) };

            var transform = CvInvoke.GetPerspectiveTransform(sourceRectangle, destRectangle);
            var outputImage = new Mat();
            CvInvoke.WarpPerspective(originalImage, outputImage, transform, new Size(width, height));
            Write(outputImage, 8);

            return (outputImage, imageWithContours);
        }

        void Write(IImage image, int number)
        {
            //CvInvoke.Imwrite($"sample{number}.jpg", image);
        }

        Mat ResizeImage(Mat image, int height = 800)
        {
            var output = new Mat();
            decimal rat = (decimal)height / image.Height;
            CvInvoke.Resize(image, output, new Size((int)(image.Width * rat), height));
            return output;
        }
    }
}
