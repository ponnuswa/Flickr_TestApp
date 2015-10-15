using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Flickr_Universal_App_Sample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Viewer : Page
    {
        public static Viewer Current;
        private bool ignoreSelectionChanged = true;
        private DispatcherTimer dispatcherTimer;

        public Viewer()
        {
            this.InitializeComponent();
            Current = this;
            FlipView3.SelectionChanged += FlipView3_SelectionChanged;
            this.Loaded += Viewer_Loaded;
        }

        void Viewer_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= Viewer_Loaded;
            GeoInfo geo = App.viewModel.Data.photos.photo[FlipView3.SelectedIndex].Geo();
            if (geo == null)
            {
                dispatcherTimer.Start();
            }
            else
            {
                if (geo.isValid)
                    MapButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            ignoreSelectionChanged = false;
        }

#if WINDOWS_PHONE_APP
        void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            e.Handled = true;
            Windows.Phone.UI.Input.HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
            // Navigate to a page
            this.Frame.GoBack();
        }
#endif

        void FlipView3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!ignoreSelectionChanged)
            {
                if (FlipView3.SelectedIndex != -1)
                {
                    App.viewModel.SelectedIndex = FlipView3.SelectedIndex;
                    MapButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    GeoInfo geo = App.viewModel.Data.photos.photo[FlipView3.SelectedIndex].Geo();
                    if (geo == null)
                    {
                        //If flicking thro pics then dont request for geo info of the picture
                        //Start a timer
                        dispatcherTimer.Stop();
                        dispatcherTimer.Start();
                    }
                    else
                    {
                        if (geo.isValid)
                            MapButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    }
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            FlipView3.ItemTemplateSelector = new ItemSelector();
            FlipView3.ItemsSource = App.viewModel.Data.photos.photo;
            FlipView3.SelectedIndex = App.viewModel.SelectedIndex;
#if WINDOWS_PHONE_APP
            BackButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
#endif
            if (dispatcherTimer == null)
            {
                dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Tick += new EventHandler<object>(dispatcherTimer_Tick);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (dispatcherTimer != null)
            {
                dispatcherTimer.Stop();
                dispatcherTimer.Tick -= dispatcherTimer_Tick;
                dispatcherTimer = null;
            }
#if WINDOWS_PHONE_APP
            Windows.Phone.UI.Input.HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
#endif
            base.OnNavigatedFrom(e);
        }


        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private void AppBar_Opened(object sender, object e)
        {

        }

        private void MapButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MapPage));
        }

        private async void dispatcherTimer_Tick(object sender, object e)
        {
            if (dispatcherTimer != null)
            {
                dispatcherTimer.Stop();
            }
            Photo item = await App.viewModel.Data.photos.photo[FlipView3.SelectedIndex].InitiateGeoFetch();
            if (item.id == App.viewModel.Data.photos.photo[FlipView3.SelectedIndex].id)
            {
                if (item.Geo() != null && item.Geo().isValid)
                    MapButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }
    }

    public sealed class ItemSelector : DataTemplateSelector
    {
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            DataTemplate itemTemplate = Viewer.Current.Resources["ImageTemplate"] as DataTemplate;

            return itemTemplate;
        }
    }
}
