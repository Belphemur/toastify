﻿using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using log4net;
using Toastify.View;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace Toastify.Helpers
{
    internal static class ScreenHelper
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ScreenHelper));

        private const int SCREEN_RIGHT_MARGIN = 0;
        private const int SCREEN_TOP_MARGIN = 5;

        #region Static Members

        public static Point GetDPIRatios()
        {
            var p = new Point(1.0, 1.0);
            if (ToastView.Current == null)
                return p;

            PresentationSource presentationSource = PresentationSource.FromVisual(ToastView.Current);

            if (presentationSource == null)
                logger.Error("Couldn't get PresentationSource, current ToastView has been disposed.");
            else
                p = new Point(presentationSource.CompositionTarget?.TransformToDevice.M11 ?? 1.0, presentationSource.CompositionTarget?.TransformToDevice.M22 ?? 1.0);
            return p;
        }

        public static Point GetDefaultToastPosition(double width, double height)
        {
            Rectangle screenRect = Screen.PrimaryScreen.WorkingArea;
            Point dpiRatio = GetDPIRatios();
            return new Point(screenRect.Width / dpiRatio.X - width - SCREEN_RIGHT_MARGIN,
                screenRect.Height / dpiRatio.Y - height - SCREEN_TOP_MARGIN);
        }

        /// <summary>
        ///     Calculates the offset vector needed to bring an off-screen rect inside the current working area.
        /// </summary>
        /// <param name="rect"> The off-screen rect. </param>
        /// <returns> A 2D translation vector. </returns>
        public static Vector BringRectInsideWorkingArea(Rect rect)
        {
            var vector = new Vector();
            Rect totalRect = GetTotalWorkingArea();

            if (totalRect.Contains(rect))
                return vector;

            vector.X = rect.Right > totalRect.Right
                ? totalRect.Right - rect.Right
                : rect.Left < totalRect.Left
                    ? totalRect.Left - rect.Left
                    : 0.0;
            vector.Y = rect.Bottom > totalRect.Bottom
                ? totalRect.Bottom - rect.Bottom
                : rect.Top < totalRect.Top
                    ? totalRect.Top - rect.Top
                    : 0.0;

            return vector;
        }

        #endregion

        #region GetTotalWorkingArea

        public static Rect GetTotalWorkingArea()
        {
            Screen[] screens = Screen.AllScreens;
            if (screens.Length == 0)
                return new Rect(new Size(-1.0, -1.0));

            var minLocation = new Point(double.MaxValue, double.MaxValue);
            var maxLocation = new Point(double.MinValue, double.MinValue);
            foreach (Screen screen in screens)
            {
                if (screen.WorkingArea.X < minLocation.X)
                    minLocation.X = screen.WorkingArea.X;
                if (screen.WorkingArea.Y < minLocation.Y)
                    minLocation.Y = screen.WorkingArea.Y;

                if (screen.WorkingArea.X + screen.WorkingArea.Width > maxLocation.X)
                    maxLocation.X = screen.WorkingArea.X + screen.WorkingArea.Width;
                if (screen.WorkingArea.Y + screen.WorkingArea.Height > maxLocation.Y)
                    maxLocation.Y = screen.WorkingArea.Y + screen.WorkingArea.Height;
            }

            return new Rect(minLocation, maxLocation);
        }

        public static double GetTotalWorkingAreaWidth()
        {
            return GetTotalWorkingArea().Width;
        }

        public static double GetTotalWorkingAreaHeight()
        {
            return GetTotalWorkingArea().Height;
        }

        #endregion GetTotalWorkingArea

        #region GetScreenSize / GetScreenRect

        private static double GetScaleFactor()
        {
            using (Graphics mainWindowHandle = Graphics.FromHwnd(Process.GetCurrentProcess().MainWindowHandle))
            {
                return mainWindowHandle.DpiX / 96; // 100% scale factor is 96 dpi
            }
        }

        /// <summary>
        ///     Get the rectangular area of the primary screen.
        /// </summary>
        /// <returns> The rectangular area of the primary screen. </returns>
        public static Rect GetScreenRect()
        {
            Screen screen = Screen.PrimaryScreen;
            return new Rect(
                new Point(screen.WorkingArea.X, screen.WorkingArea.Y),
                new Size(screen.WorkingArea.Width / GetScaleFactor(), screen.WorkingArea.Height / GetScaleFactor()));
        }

        /// <summary>
        ///     Get the size of the primary screen.
        /// </summary>
        /// <returns> The size of the primary screen. </returns>
        public static Size GetScreenSize()
        {
            Screen screen = Screen.PrimaryScreen;
            return new Size(screen.WorkingArea.Size.Width / GetScaleFactor(), screen.WorkingArea.Size.Height / GetScaleFactor());
        }

        /// <summary>
        ///     Get the width of the primary screen.
        /// </summary>
        /// <returns> The width of the primary screen. </returns>
        public static double GetScreenWidth()
        {
            return GetScreenSize().Width;
        }

        /// <summary>
        ///     Get the height of the primary screen.
        /// </summary>
        /// <returns> The height of the primary screen. </returns>
        public static double GetScreenHeight()
        {
            return GetScreenSize().Height;
        }

        /// <summary>
        ///     Get the rectangular area of the screen that contains the given point.
        /// </summary>
        /// <returns> The rectangular area of the screen that contains the given point. </returns>
        public static Rect GetScreenRect(Point pointOnScreen)
        {
            var point = unchecked(new System.Drawing.Point((int)pointOnScreen.X, (int)pointOnScreen.Y));
            Screen screen = Screen.FromPoint(point);
            return new Rect(
                new Point(screen.WorkingArea.X, screen.WorkingArea.Y),
                new Size(screen.WorkingArea.Width / GetScaleFactor(), screen.WorkingArea.Height / GetScaleFactor()));
        }

        /// <summary>
        ///     Get the size of the screen that contains the given point.
        /// </summary>
        /// <param name="pointOnScreen"> A point on the screen. </param>
        /// <returns> The size of the screen that contains the point or of the closest one. </returns>
        public static Size GetScreenSize(Point pointOnScreen)
        {
            var point = unchecked(new System.Drawing.Point((int)pointOnScreen.X, (int)pointOnScreen.Y));
            Screen screen = Screen.FromPoint(point);
            return new Size(screen.WorkingArea.Size.Width / GetScaleFactor(), screen.WorkingArea.Size.Height / GetScaleFactor());
        }

        /// <summary>
        ///     Get the width of the screen that contains the given point.
        /// </summary>
        /// <param name="pointOnScreen"> A point on the screen. </param>
        /// <returns> The width of the screen that contains the point or of the closest one. </returns>
        public static double GetScreenWidth(Point pointOnScreen)
        {
            return GetScreenSize(pointOnScreen).Width;
        }

        /// <summary>
        ///     Get the height of the screen that contains the given point.
        /// </summary>
        /// <param name="pointOnScreen"> A point on the screen. </param>
        /// <returns> The height of the screen that contains the point or of the closest one. </returns>
        public static double GetScreenHeight(Point pointOnScreen)
        {
            return GetScreenSize(pointOnScreen).Height;
        }

        #endregion GetScreenSize / GetScreenRect

        #region CalculateMaxToastSize

        /// <summary>
        ///     Calculates the maximum size the Toast can have, considering its current position.
        /// </summary>
        /// <returns> The maximum size. </returns>
        public static Size CalculateMaxToastSize()
        {
            Rect toastRect = ToastView.Current == null ? new Rect() : ToastView.Current.Rect;
            Rect totalRect = GetTotalWorkingArea();
            //Rect toastScreenRect = GetScreenRect(toastRect.Location);

            double maxPossibleWidth = totalRect.Right - toastRect.Left;
            double maxPossibleHeight = totalRect.Bottom - toastRect.Top;

            //if (Screen.AllScreens.Length > 1)
            //{
            //    // maxPossibleHeight
            //    if (toastRect.Right > toastScreenRect.Right)
            //    {
            //        // The toast extends to multiple screens by its width
            //        Rect extScreenRect = GetScreenRect(toastScreenRect.TopRight);

            //        do // Loop through all the screens the toast extends to, and find out the minimum height between them
            //        {
            //            double h = extScreenRect.Bottom - toastRect.Top;
            //            if (h > 0 && h < maxPossibleHeight)
            //                maxPossibleHeight = h;
            //        } while ((extScreenRect.Right - toastRect.Right - extScreenRect.Width) > 0);
            //    }

            //    // maxPossibleWidth
            //    // TODO: Constrain width to only this screen if either [TopRight + (1,0)] or [BottomRight + (1,0)] are outside of every screen
            //}

            return new Size(maxPossibleWidth, maxPossibleHeight);
        }

        /// <summary>
        ///     Calculates the maximum width the Toast can have, considering its current position.
        /// </summary>
        /// <returns> The maximum width. </returns>
        public static double CalculateMaxToastWidth()
        {
            return CalculateMaxToastSize().Width;
        }

        /// <summary>
        ///     Calculates the maximum height the Toast can have, considering its current position.
        /// </summary>
        /// <returns> The maximum widheightth. </returns>
        public static double CalculateMaxToastHeight()
        {
            return CalculateMaxToastSize().Height;
        }

        #endregion CalculateMaxToastSize
    }
}