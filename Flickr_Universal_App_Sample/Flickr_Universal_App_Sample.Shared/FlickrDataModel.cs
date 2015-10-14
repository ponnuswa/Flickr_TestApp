using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Flickr_Universal_App_Sample
{
//    { "photo": 
//{ "id": "21537066543", 
//    "location": { "latitude": 64.149855, "longitude": -21.931231, "accuracy": 16, "context": 0, 
//      "locality": { "_content": "Reykjavik", "place_id": "PGxy3mRYWrpnp5g", "woeid": "980389" }, 
//      "region": { "_content": "Reykjavik", "place_id": "9RNUBA9TUrpJ8bGVUw", "woeid": "20070109" }, 
//      "country": { "_content": "Iceland", "place_id": "jPn7LAVTUb5tyPnsog", "woeid": "23424845" }, 
//    "place_id": "PGxy3mRYWrpnp5g", "woeid": "980389" } }, "stat": "ok" }

    public class PlaceInfo
    {

    }
    public class FlickrData
    {
        //"photos": {...}, "stat": "ok"
        public Photos photos { get; set; }
        public string stat { get; set; }
    }

    public class Photos
    {
        //"page": 1, "pages": "4082", "perpage": 100, "total": "408186", "photo": [{...}] , "stat": "ok" }
        public int page { get; set; }
        public int pages { get; set; }
        public int perpage { get; set; }
        public string total { get; set; }
        public List<Photo> photo { get; set; }
    }
    public class Photo
    {
        //id": "21522112874", "owner": "54462299@N06", "secret": "5f95fa292b", "server": "761", "farm": 1, "title": "20151008-JJ6A9464-2.jpg", "ispublic": 1, "isfriend": 0, "isfamily": 0 
        public string id { get; set; }
        public string owner { get; set; }
        public string secret { get; set; }
        public string server { get; set; }
        public string farm { get; set; }

        public string title { get; set; }
        public string ispublic { get; set; }
        public string isfriend { get; set; }
        public string isfamily { get; set; }

        public string ThumbnailUrl
        {
            get
            {
                return UrlFormat(this, "_t", "jpg");
            }
        }

        public string OriginalUrl
        {
            get
            {
                return UrlFormat(this, "large", "jpg");
            }
        }

        public ImageSource Thumbnail
        {
            get
            {
                return new BitmapImage(new Uri(ThumbnailUrl, UriKind.Absolute));
            }
        }

        public ImageSource Original
        {
            get
            {
                return new BitmapImage(new Uri(OriginalUrl, UriKind.Absolute));
            }
        }

        private GeoInfo _geo = null;
        public async Task<GeoInfo> Geo()
        {
                if (_geo == null)
                {
                    _geo = await App.flickr.GetGeoInfo(id);
                }
                return _geo;
        }

        private const string PhotoUrlFormat = "https://farm{0}.staticflickr.com/{1}/{2}_{3}{4}.{5}";

        internal static string UrlFormat(Photo p, string size, string extension)
        {
            if (size == "_o" || size == "original")
                return UrlFormat(p.farm, p.server, p.id, p.secret, size, extension);
            else
                return UrlFormat(p.farm, p.server, p.id, p.secret, size, extension);
        }

        internal static string UrlFormat(string farm, string server, string photoid, string secret, string size, string extension)
        {
            string sizeAbbreviation;
            switch (size)
            {
                case "square":
                    sizeAbbreviation = "_s";
                    break;
                case "thumbnail":
                    sizeAbbreviation = "_t";
                    break;
                case "small":
                    sizeAbbreviation = "_m";
                    break;
                case "large":
                    sizeAbbreviation = "_b";
                    break;
                case "original":
                    sizeAbbreviation = "_o";
                    break;
                case "medium":
                    sizeAbbreviation = string.Empty;
                    break;
                default:
                    sizeAbbreviation = size;
                    break;
            }

            return UrlFormat(PhotoUrlFormat, farm, server, photoid, secret, sizeAbbreviation, extension);
        }

        private static string UrlFormat(string format, params object[] parameters)
        {
            return String.Format(System.Globalization.CultureInfo.InvariantCulture, format, parameters);
        }
    }

    public class GeoInfo
    {
        /// <summary>
        /// The latitude of this place.
        /// </summary>
        public double latitude { get; set; }

        /// <summary>
        /// The longitude of this place.
        /// </summary>
        public double longitude { get; set; }

        /// <summary>
        /// The accuracy of the location information, if this information is about a photo.
        /// </summary>
        public int accuracy { get; set; }

        public bool isValid { get; set; }

        public string locality { get; set; }
    }

    public class FlickrDataModel
    {
        public int CurrentPage { get; private set; }
        public bool IsInitialised { get; private set; }
        public FlickrData Data { get; private set; }
        public bool Locked { get; set; }

        public async Task<bool> InitialisePhotosList()
        {
            string webresponse = await App.flickr.GetPhotos();
            Data = JsonConvert.DeserializeObject<FlickrData>(webresponse);
            if (Data.stat == "ok")
            {
                CurrentPage = 1;
                IsInitialised = true;
                return true;
            }
            return false;
        }


        public bool HasMore(int currentCount)
        {
            if (Locked)
                return false;

            if (Data != null && Data.photos != null && Convert.ToInt32(Data.photos.total) == currentCount)
            {
                return false;
            }
            return true;
        }

        public async Task<List<Photo>> FetchMoreData()
        {
            CurrentPage = Convert.ToInt32(Data.photos.page);
            int totalPages = Convert.ToInt32(Data.photos.pages);
            if (CurrentPage < totalPages)
            {
                //Fetch More
                string webresponse = await App.flickr.GetPhotos(CurrentPage + 1);
                FlickrData tmp = JsonConvert.DeserializeObject<FlickrData>(webresponse);
                if (tmp.stat == "ok")
                {
                    CurrentPage = tmp.photos.page;
                    UpdateData(tmp);
                    return tmp.photos.photo;
                }
            }
            return new List<Photo>();
        }

        private void UpdateData(FlickrData tmpData)
        {
            Data.photos.page = tmpData.photos.page;
            Data.photos.pages = tmpData.photos.pages;
            Data.photos.total = tmpData.photos.total;
            Data.photos.photo.AddRange(Data.photos.photo);
        }

        public int SelectedIndex { get; set; }

        public Photo SelectedItem
        {
            get {
                return Data.photos.photo[SelectedIndex];
            }
        }

    }
}
