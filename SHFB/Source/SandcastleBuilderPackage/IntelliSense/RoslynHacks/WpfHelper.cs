namespace SandcastleBuilder.Package.IntelliSense.RoslynHacks
{
    using System.Windows.Forms;
    using Point = System.Windows.Point;
    using Rect = System.Windows.Rect;

    internal static class WpfHelper
    {
        private static double? _deviceScaleX;
        private static double? _deviceScaleY;

        public static double DeviceScaleX
        {
            get
            {
                if (_deviceScaleX == null)
                {
                    LoadDeviceScaling();
                }

                return _deviceScaleX.Value;
            }
        }

        public static double DeviceScaleY
        {
            get
            {
                if (_deviceScaleY == null)
                {
                    LoadDeviceScaling();
                }

                return _deviceScaleY.Value;
            }
        }

        public static Rect GetScreenRect(Point screenCoordinates)
        {
            Screen screen = Screen.FromPoint(new System.Drawing.Point((int)screenCoordinates.X, (int)screenCoordinates.Y));
            var workingArea = screen.WorkingArea;
            return new Rect(new Point(workingArea.Left, workingArea.Top), new Point(workingArea.Right, workingArea.Bottom));
        }

        private static void LoadDeviceScaling()
        {
            using (var control = new Control())
            {
                using (var graphics = control.CreateGraphics())
                {
                    _deviceScaleX = 96.0 / graphics.DpiX;
                    _deviceScaleY = 96.0 / graphics.DpiY;
                }
            }
        }
    }
}
