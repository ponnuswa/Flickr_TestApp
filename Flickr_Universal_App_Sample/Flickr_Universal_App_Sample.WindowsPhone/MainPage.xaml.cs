using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Flickr_Universal_App_Sample
{
    public class ItemsToShow : ObservableCollection<Photo>, ISupportIncrementalLoading
    {
        public FlickrDataModel baseModel;

        public ItemsToShow(FlickrDataModel model)
        {
            baseModel = model;
            foreach (var item in model.Data.photos.photo)
                this.Add(item);
        }

        public ItemsToShow(FlickrDataModel model, List<Photo> items)
        {
            baseModel = model;
            foreach (var item in items)
                this.Add(item);
        }

        public bool HasMoreItems
        {
            get
            {
                return baseModel.HasMore(this.Count);
            }
        }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            CoreDispatcher coreDispatcher = Window.Current.Dispatcher;

            return Task.Run<LoadMoreItemsResult>(async () =>
            {
                List<Photo> items = new List<Photo>();
                List<Photo> tmpList = await baseModel.FetchMoreData();

                int minVal = Convert.ToInt32(Math.Min(count, tmpList.Count));
                for (int i = 0; i < minVal; i++)
                {
                    items.Add(tmpList[i]);
                }

                await coreDispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        foreach (Photo p in items)
                        {
                            this.Add(p);
                        }
                    });

                return new LoadMoreItemsResult() { Count = count };
            }).AsAsyncOperation<LoadMoreItemsResult>();
        }
    }



    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private bool ignoreSelection = false;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

#if WINDOWS_PHONE_APP
        void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            e.Handled = true;
            Windows.Phone.UI.Input.HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
            App.Current.Exit();
            // Navigate to a page
        }
#endif

        async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= MainPage_Loaded;
            if (!App.flickr.IsInitialised)
            {
                await App.flickr.Initialise();
                LaunchBrowserForLogin();
            }
        }


        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
#if WINDOWS_PHONE_APP
            Windows.Phone.UI.Input.HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
#endif
            base.OnNavigatingFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (App.viewModel.IsInitialised)
            {
                ignoreSelection = true;
                GridViewMain.Visibility = Windows.UI.Xaml.Visibility.Visible;
                GridViewMain.ItemsSource = new ItemsToShow(App.viewModel);
                GridViewMain.SelectedIndex = App.viewModel.SelectedIndex;
                GridViewMain.ScrollIntoView(GridViewMain.SelectedItem);
            }
#if WINDOWS_PHONE_APP
            BackButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
#endif
            base.OnNavigatedTo(e);
        }
        private DispatcherTimer dispatcherTimer;

        void LaunchBrowserForLogin()
        {
            Launcher.LaunchUriAsync(new Uri(App.flickr.GetAuthenticationLink()));
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler<object>(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 20);
            dispatcherTimer.Start();
        }

        private async void dispatcherTimer_Tick(object sender, object e)
        {
            if (await App.flickr.GetToken())
            {
                GridViewMain.Visibility = Windows.UI.Xaml.Visibility.Visible;
                FetchPhotos.Visibility = Windows.UI.Xaml.Visibility.Visible;
                StatusLabel.Text = "Ready! Fetch all public photos";
            }
            else
            {
                StatusLabel.Text = "Trouble in Authentication";
            }
            dispatcherTimer.Stop();
            dispatcherTimer.Tick -= dispatcherTimer_Tick;
        }


        private async void FetchPhotos_Click(object sender, RoutedEventArgs e)
        {
            if (!App.viewModel.IsInitialised)
            {
                FetchPhotos.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                if (await App.viewModel.InitialisePhotosList())
                {
                    GridViewMain.ItemsSource = new ItemsToShow(App.viewModel);
                    StatusLabel.Text = "";
                }
                else
                {
                    StatusLabel.Text = "Trouble. Close and try again.";
                }
            }
        }

        private void GridViewMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ignoreSelection)
            {
                ignoreSelection = false;
                return;
            }
            int index = GridViewMain.SelectedIndex;
            if (index != -1)
            {
                App.viewModel.SelectedIndex = index;
                this.Frame.Navigate(typeof(Viewer));
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox s = (TextBox)sender;
            if (s.Text.Length > 0)
            {
                List<Photo> items = new List<Photo>();
                App.viewModel.Locked = true;
                foreach (var item in App.viewModel.Data.photos.photo)
                {
                    if (item.title.IndexOf(s.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        items.Add(item);
                    }
                }
                GridViewMain.ItemsSource = new ItemsToShow(App.viewModel, items);
            }
            else
            {
                App.viewModel.Locked = false;
                GridViewMain.ItemsSource = new ItemsToShow(App.viewModel);
            }

        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Visibility == Windows.UI.Xaml.Visibility.Collapsed)
            {
                SearchTextBox.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                SearchTextBox.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                SearchTextBox.Text = string.Empty;
                App.viewModel.Locked = false;
                GridViewMain.ItemsSource = new ItemsToShow(App.viewModel);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Exit();
        }

    }
}
