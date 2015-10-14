using System;
using System.Collections.Generic;
using System.Text;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Flickr_Universal_App_Sample
{
    /// <summary>
    /// Based on http://code.msdn.microsoft.com/windowsapps/Image-Overlays-in-Bing-a0a3471e
    /// </summary>
    public class BoundedImage : Canvas
    {
        public MapView _map;
        public BasicGeoposition _northWest, _southEast;
        private Image _img;

        public BoundedImage(MapView map, Uri imageUri, BasicGeoposition northWest, BasicGeoposition southEast)
        {
            _map = map;
            _northWest = northWest;
            _southEast = southEast;

            _img = new Image();
            _img.Stretch = Stretch.Fill;
            _img.Source = new BitmapImage(imageUri);
            this.Children.Add(_img);

            _map.ViewChanged += (center, zoom, heading) =>
            {
                UpdatePosition();
            };

            _map.SizeChanged += (s, a) =>
            {
                this.Width = _map.ActualWidth;
                this.Height = _map.ActualHeight;

                UpdatePosition();
            };

            UpdatePosition();
        }

        private void UpdatePosition()
        {
            Point topLeftPixel, bottomRightPixel;

            #if WINDOWS_APP
            _map.BaseMap.TryLocationToPixel(_northWest.ToLocation(), out topLeftPixel);
            _map.BaseMap.TryLocationToPixel(_southEast.ToLocation(), out bottomRightPixel);
            #elif WINDOWS_PHONE_APP
            _map.BaseMap.GetOffsetFromLocation(new Geopoint(_northWest), out topLeftPixel);
            _map.BaseMap.GetOffsetFromLocation(new Geopoint(_southEast), out bottomRightPixel);
            #endif

            double left = topLeftPixel.X, right = bottomRightPixel.X;

            //Compensate for world wrap of map 
            if (left > right)
            {
                double mapWidth = 256 * Math.Pow(2, _map.Zoom);

                if (right < 0)
                {
                    right += mapWidth;
                }
                else
                {
                    left -= mapWidth;
                }
            }

            Canvas.SetLeft(_img, left);
            Canvas.SetTop(_img, topLeftPixel.Y);

            _img.Width = Math.Abs(right - left);
            _img.Height = Math.Abs(bottomRightPixel.Y - topLeftPixel.Y);
        }  
    }
}
