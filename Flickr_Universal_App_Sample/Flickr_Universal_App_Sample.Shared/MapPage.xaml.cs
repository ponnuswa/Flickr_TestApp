using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
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
    public sealed partial class MapPage : Page
    {
        public MapPage()
        {
            this.InitializeComponent();
            this.Loaded += MapPage_Loaded;

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
#if WINDOWS_PHONE_APP
            BackButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
#endif
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
#if WINDOWS_PHONE_APP
            Windows.Phone.UI.Input.HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
#endif
            base.OnNavigatedFrom(e);
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

        async void MapPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= MapPage_Loaded;
            GeoInfo geo = await App.viewModel.SelectedItem.Geo();
            MyMap.SetView(new BasicGeoposition() { Latitude = geo.latitude, Longitude = geo.longitude }, 11);
            MyMap.AddPushpin(new BasicGeoposition() { Latitude = geo.latitude, Longitude = geo.longitude }, geo.locality);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MyMap.ClearMap();
            this.Frame.GoBack();
        }
    }
}
