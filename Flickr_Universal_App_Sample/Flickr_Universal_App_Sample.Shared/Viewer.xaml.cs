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

        public Viewer()
        {
            this.InitializeComponent();
            Current = this;
            FlipView3.SelectionChanged += FlipView3_SelectionChanged;
            this.Loaded += Viewer_Loaded;
        }

        async void Viewer_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= Viewer_Loaded;
            GeoInfo geo = await App.viewModel.Data.photos.photo[FlipView3.SelectedIndex].Geo();
            if (geo.isValid)
                MapButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            else
                MapButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
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

        async void FlipView3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!ignoreSelectionChanged)
            {
                if (FlipView3.SelectedIndex != -1)
                {
                    await App.viewModel.Data.photos.photo[FlipView3.SelectedIndex].Geo();
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
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
#if WINDOWS_PHONE_APP
            Windows.Phone.UI.Input.HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
#endif
            base.OnNavigatedFrom(e);
        }


        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private async void AppBar_Opened(object sender, object e)
        {

        }

        private void MapButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MapPage));
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
