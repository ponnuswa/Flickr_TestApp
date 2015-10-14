using System.ComponentModel;
using Windows.UI.Xaml.Controls;
using Windows.Devices.Geolocation;
using System.Collections.Generic;
using Windows.UI;

#if WINDOWS_PHONE_APP
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml;
using System;
using Windows.UI.Xaml.Media.Imaging;
#elif WINDOWS_APP
using Bing.Maps;
#endif

namespace Flickr_Universal_App_Sample
{
    public class MapView : Grid, INotifyPropertyChanged
    {
        #region Private Methods

        #if WINDOWS_APP
        private Map _map;
        private MapShapeLayer _shapeLayer;
        private MapLayer _pinLayer;
        #elif WINDOWS_PHONE_APP
        private MapControl _map;
        #endif

        private bool _disableHeading, _disablePitch;

        #endregion

        #region Constructor

        public MapView()
        {
            #if WINDOWS_APP
            _map = new Map();

            _shapeLayer = new MapShapeLayer();
            _map.ShapeLayers.Add(_shapeLayer);

            _pinLayer = new MapLayer();
            _map.Children.Add(_pinLayer);
            #elif WINDOWS_PHONE_APP
            _map = new MapControl();
            #endif
           
            this.Children.Add(_map);
            
            SetMapBindings();
        }

        #endregion

        #region Public Properties

        #if WINDOWS_APP
        public Map BaseMap { get { return _map; } }

        public MapShapeLayer ShapeLayer { get { return _shapeLayer; } }

        public UIElementCollection PinLayer { get { return _pinLayer.Children; } }
        #elif WINDOWS_PHONE_APP
        public MapControl BaseMap { get { return _map; } }

        public IList<MapElement> ShapeLayer { get { return _map.MapElements; } }

        public IList<DependencyObject> PinLayer { get { return _map.Children; } }
        #endif

        public double Zoom
        {
            get
            {
                return _map.ZoomLevel;
            }
            set
            {
                _map.ZoomLevel = value;
                OnPropertyChanged("Zoom");
            }
        }
        
        public Geopoint Center
        {
            get
            {
                #if WINDOWS_APP
                return _map.Center.ToGeopoint();
                #elif WINDOWS_PHONE_APP
                return _map.Center;
                #endif
            }
            set
            {
                #if WINDOWS_APP
                _map.Center = value.ToLocation();
                #elif WINDOWS_PHONE_APP
                _map.Center = value;
                #endif

                OnPropertyChanged("Center");
            }
        }

        public double Heading
        {
            get
            {
                return _map.Heading;
            }
            set
            {
                _map.Heading = value;
                OnPropertyChanged("Heading");
            }
        }

        public string Credentials
        {
            get
            {
                #if WINDOWS_APP
                return _map.Credentials;
                #elif WINDOWS_PHONE_APP
                return string.Empty;
                #endif
            }
            set
            {
                #if WINDOWS_APP
                if (!string.IsNullOrEmpty(value))
                {
                    _map.Credentials = value;
                }
                #endif

                OnPropertyChanged("Credentials");
            }
        }

        public string MapServiceToken
        {
            get
            {
                #if WINDOWS_APP
                return string.Empty;
                #elif WINDOWS_PHONE_APP
                return _map.MapServiceToken;
                #endif
            }
            set
            {
                #if WINDOWS_PHONE_APP
                if (!string.IsNullOrEmpty(value))
                {
                    _map.MapServiceToken = value;
                }
                #endif

                OnPropertyChanged("MapServiceToken");
            }
        }

        public bool ShowTraffic
        {
            get
            {
                #if WINDOWS_APP
                return _map.ShowTraffic;
                #elif WINDOWS_PHONE_APP
                return _map.TrafficFlowVisible;
                #endif
            }
            set
            {
                #if WINDOWS_APP
                _map.ShowTraffic = value;
                #elif WINDOWS_PHONE_APP
                _map.TrafficFlowVisible = value;
                #endif
                
                OnPropertyChanged("ShowTraffic");
            }
        }

        /// <summary>
        /// Locks the pitch of the map to 0. This is required when using BoundedImage's.
        /// </summary>
        public bool DisablePitch
        {
            get
            {
                return _disablePitch;
            }
            set
            {
                _disablePitch = value;
                OnPropertyChanged("DisablePitch");
            }
        }

        /// <summary>
        /// Locks the heading of the map to 0. This is required when using BoundedImage's.
        /// </summary>
        public bool DisableHeading
        {
            get
            {
                return _disableHeading;
            }
            set
            {
                _disableHeading = value;
                OnPropertyChanged("DisableHeading");
            }
        }
        
        public delegate void ViewChangeHandler(Geopoint center, double zoom, double heading);

        public event ViewChangeHandler ViewChanged;

        #endregion

        #region Public Methods

        public void ClearMap()
        {
            #if WINDOWS_APP
            _shapeLayer.Shapes.Clear();
            _pinLayer.Children.Clear();
            #elif WINDOWS_PHONE_APP
            _map.MapElements.Clear();
            _map.Children.Clear();
            #endif
        }

        public void AddPushpin(BasicGeoposition location, string text)
        {
            #if WINDOWS_APP
            var pin = new Pushpin()
            {
                Text = text, Width = 100
            };
            MapLayer.SetPosition(pin, location.ToLocation());
            _pinLayer.Children.Add(pin);
            #elif WINDOWS_PHONE_APP
            var pin = new Grid()
            {
                Width = 24,
                Height = 24,
                Margin = new Windows.UI.Xaml.Thickness(-12)
            };

            pin.Children.Add(new Ellipse()
            {
                Fill = new SolidColorBrush(Colors.DodgerBlue),
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 3,
                Width = 24,
                Height = 24
            });

            pin.Children.Add(new TextBlock()
            {
                Text = text,
                FontSize = 12,
                Foreground = new SolidColorBrush(Colors.White),
                HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center,
                VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center
            });

            MapControl.SetLocation(pin, new Geopoint(location));
            _map.Children.Add(pin);
            #endif
        }

        public void AddPolyline(List<BasicGeoposition> locations, Color strokeColor, double strokeThickness)
        {
            #if WINDOWS_APP
            var line = new MapPolyline()
            {
                Locations = locations.ToLocationCollection(),
                Color = strokeColor,
                Width = strokeThickness
            };

            _shapeLayer.Shapes.Add(line);
            #elif WINDOWS_PHONE_APP
            var line = new MapPolyline()
            {
                Path = new Geopath(locations),
                StrokeColor = strokeColor,
                StrokeThickness = strokeThickness
            };

            _map.MapElements.Add(line);
            #endif
        }

        public void AddPolygon(List<BasicGeoposition> locations, Color fillColor)
        {
            AddPolygon(locations, fillColor, Windows.UI.Color.FromArgb(0, 0, 0, 0), 1);
        }

        public void AddPolygon(List<BasicGeoposition> locations, Color fillColor, Color strokeColor, double strokeThickness)
        {
            #if WINDOWS_APP
            var line = new MapPolygon()
            {
                Locations = locations.ToLocationCollection(),
                FillColor = fillColor
            };

            _shapeLayer.Shapes.Add(line);
            #elif WINDOWS_PHONE_APP
            var line = new MapPolygon()
            {
                Path = new Geopath(locations),
                FillColor = fillColor,
                StrokeColor = strokeColor,
                StrokeThickness = strokeThickness
            };

            _map.MapElements.Add(line);
            #endif
        }

        public void SetMapType(string type)
        {
            #if WINDOWS_APP
            switch (type)
            {
                case "aerial":
                case "aerialWithRoads":
                    _map.MapType = MapType.Aerial;
                    break;
                case "birdseye":
                    _map.MapType = MapType.Birdseye;
                    break;
                case "road":
                case "terrain":
                default:
                    _map.MapType = MapType.Road;
                    break;
            }
            #elif WINDOWS_PHONE_APP
            switch (type)
            {
                case "aerial":
                    _map.Style = MapStyle.Aerial;
                    break;
                case "aerialWithRoads":
                case "birdseye":
                    _map.Style = MapStyle.AerialWithRoads;
                    break;
                case "terrain":
                    _map.Style = MapStyle.Terrain;
                    break;
                case "road":
                default:
                    _map.Style = MapStyle.Terrain;
                    break;
            }
            #endif
        }

        public void SetView(BasicGeoposition center, double zoom)
        {
            #if WINDOWS_APP
            _map.SetView(center.ToLocation(), zoom);
            OnPropertyChanged("Center");
            OnPropertyChanged("Zoom");
            #elif WINDOWS_PHONE_APP
            _map.Center = new Geopoint(center);
            _map.ZoomLevel = zoom;
            #endif
        }

        ~MapView()
        {
            #if WINDOWS_APP
            _map.ViewChanged -= _map_ViewChanged;
            #elif WINDOWS_PHONE_APP
            _map.ZoomLevelChanged -= _map_ZoomLevelChanged;
            _map.CenterChanged -= _map_CenterChanged;
            _map.HeadingChanged -= _map_HeadingChanged;
            _map.PitchChanged -= _map_PitchChanged;
            #endif
        }

        #endregion

        #region Private Methods

        private void SetMapBindings()
        {
            #if WINDOWS_APP
            _map.ViewChanged += _map_ViewChanged;
            #elif WINDOWS_PHONE_APP
            _map.ZoomLevelChanged += _map_ZoomLevelChanged;
            _map.CenterChanged += _map_CenterChanged;
            _map.HeadingChanged += _map_HeadingChanged;
            _map.PitchChanged += _map_PitchChanged;
            #endif
        }

        private async void _map_PitchChanged(object sender, object args)
        {
            #if WINDOWS_PHONE_APP
            //Disable pitch. Required when using BoundedImages.
            if (_disablePitch && _map.Pitch != 0)
            {
                await _map.TrySetViewAsync(_map.Center, null, null, 0);                
            }
            #endif

            CallViewChangedEvent();
        }

        private void _map_HeadingChanged(object sender, object args)
        {
            //Disable heading. Required when using BoundedImages.
            if (_disableHeading && _map.Heading != 0)
            {
                _map.Heading = 0;
            }
           
            CallViewChangedEvent();
        }

        private void _map_CenterChanged(object sender, object args)
        {
            CallViewChangedEvent();
        }

        private void _map_ZoomLevelChanged(object sender, object args)
        {
            CallViewChangedEvent();
        }

        private void _map_ViewChanged(object sender, object e)
        {
            CallViewChangedEvent();
        }

        private void CallViewChangedEvent()
        {
            if (ViewChanged != null)
            {
                #if WINDOWS_APP
                ViewChanged(_map.Center.ToGeopoint(), _map.ZoomLevel, _map.Heading);
                #elif WINDOWS_PHONE_APP
                ViewChanged(_map.Center, _map.ZoomLevel, _map.Heading);
                #endif
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        internal void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
