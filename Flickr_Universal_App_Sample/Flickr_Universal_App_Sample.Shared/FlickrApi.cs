using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Linq;
using Newtonsoft.Json;
using System.Xml;
using System.IO;

namespace Flickr_Universal_App_Sample
{
    public sealed class FlickrApi
    {
        private const string _consumerkey = "408ab0f2f19de2a38e0162275be1a6d5";
        private const string _consumersecret = "bee720d2e636371c";
        private const string _restserviceurl = "http://flickr.com/services/rest/";
        private string _frob;
        private string _authtoken;
        private string userid;

        public bool IsInitialised { get; private set; }

        public FlickrApi()
        {
        }


        public async Task<bool> Initialise()
        {
            await auth_getfrob();
            IsInitialised = true;
            return true;
        }

        public string GetAuthenticationLink()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("perms", "read");
            parameters.Add("frob", _frob);
            parameters.Add("api_key", _consumerkey);

            string sig = Utility.AuthCalculateSignature(_consumersecret, parameters);
            parameters.Add("api_sig", sig);

#if WINDOWS_PHONE_APP
            return "https://m.flickr.com/services/auth/" + "?" + Utility.BuildString(parameters);
            //?mobile=1&api_key=408ab0f2f19de2a38e0162275be1a6d5&perms=read&api_sig=f68b251ec306ff21c59baa35ddf5e794";
            //return "https://m.flickr.com/auth-72157659761175285";
#else
            return "http://flickr.com/services/auth/" + "?" + Utility.BuildString(parameters);
#endif
        }

        public async Task<bool> GetToken()
        {
            if (_frob == null)
            {
                return false;
            }
            await auth_gettoken();
            return true;
        }

        public async Task<GeoInfo> GetGeoInfo(string photoId)
        {
            //https://api.flickr.com/services/rest/?method=flickr.photos.geo.getLocation&api_key=dba3355dc8c8dd0fa6b7a45ce2a44db3&photo_id=22128905835&format=json&nojsoncallback=1&auth_token=72157659832560651-376ab699108fe73b&api_sig=9f500b727d3e2585b397d96f966fbef0
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("method", "flickr.photos.geo.getLocation");
            parameters.Add("photo_id", photoId);
            parameters.Add("api_key", _consumerkey);
            //parameters.Add("format", "json");
            //parameters.Add("nojsoncallback", "1");
            parameters.Add("auth_token", _authtoken);

            string sig = Utility.AuthCalculateSignature(_consumersecret, parameters);
            parameters.Add("api_sig", sig);

            string webrequest = _restserviceurl + "?" + Utility.BuildString(parameters);
            HttpClient proxy = new HttpClient();
            HttpResponseMessage response = await proxy.GetAsync(webrequest);
            string webresponse = await response.Content.ReadAsStringAsync();

            XDocument doc = XDocument.Parse(webresponse);

            var rsp = doc.Descendants(XName.Get("rsp")).FirstOrDefault();
            GeoInfo data = new GeoInfo();
            if ((string)rsp.Attribute("stat") == "ok")
            {
                var element = doc.Descendants(XName.Get("location")).FirstOrDefault();
                data.latitude = Convert.ToDouble((string)element.Attribute("latitude"));
                data.longitude = Convert.ToDouble((string)element.Attribute("longitude"));
                data.accuracy = Convert.ToInt32((string)element.Attribute("accuracy"));
                data.locality = doc.Descendants(XName.Get("locality")).FirstOrDefault().Value;

                data.isValid = true;
            }
            return data;
        }

        public async Task<string> GetPhotos(int pageNo = 1)
        {
            //URL: https://api.flickr.com/services/rest/?method=flickr.photos.search&api_key=90dacc9763c5a7b89675da609b8b3258&user_id=135202826%40N06&format=json&nojsoncallback=1&auth_token=72157659740576056-106402cd9111edc9&api_sig=5b2286c75e179d807038f69c4d241d0f
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("method", "flickr.photos.search");
            parameters.Add("api_key", _consumerkey);
            //parameters.Add("user_id", userid);
            parameters.Add("format", "json");
            parameters.Add("nojsoncallback", "1");
            parameters.Add("auth_token", _authtoken);
            if (pageNo > 1)
            {
                parameters.Add("page", pageNo.ToString());
            }

            string sig = Utility.AuthCalculateSignature(_consumersecret, parameters);
            parameters.Add("api_sig", sig);

            string webrequest = _restserviceurl + "?" + Utility.BuildString(parameters);
            HttpClient proxy = new HttpClient();
            HttpResponseMessage response = await proxy.GetAsync(webrequest);
            string webresponse = await response.Content.ReadAsStringAsync();
            return webresponse;
        }

        #region PRIVATE_FUNCTIONS
        private async Task<bool> auth_getfrob()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("method", "flickr.auth.getFrob");
            parameters.Add("api_key", _consumerkey);

            string sig = Utility.AuthCalculateSignature(_consumersecret, parameters);
            parameters.Add("api_sig", sig);

            string webrequest = _restserviceurl + "?" + Utility.BuildString(parameters);
            HttpClient proxy = new HttpClient();
            HttpResponseMessage response = await proxy.GetAsync(webrequest);
            string webresponse = await response.Content.ReadAsStringAsync();
            XDocument doc = XDocument.Parse(webresponse);
            var rsp = doc.Descendants(XName.Get("rsp")).FirstOrDefault();
            if ((string)rsp.Attribute("stat") == "ok")
            {
                _frob = doc.Descendants(XName.Get("frob")).FirstOrDefault().Value;
                return true;
            }
            return false;
        }

        private async Task<bool> auth_gettoken()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("method", "flickr.auth.getToken");
            parameters.Add("api_key", _consumerkey);
            parameters.Add("frob", _frob);

            string sig = Utility.AuthCalculateSignature(_consumersecret, parameters);
            parameters.Add("api_sig", sig);

            string webrequest = _restserviceurl + "?" + Utility.BuildString(parameters);
            HttpClient proxy = new HttpClient();
            HttpResponseMessage response = await proxy.GetAsync(webrequest);
            string webresponse = await response.Content.ReadAsStringAsync();
            XDocument doc = XDocument.Parse(webresponse);
            _authtoken = doc.Descendants(XName.Get("token")).FirstOrDefault().Value;
            var element = doc.Descendants(XName.Get("user")).FirstOrDefault();
            userid = (string)element.Attribute("nsid");
            return true;
        }
        #endregion

    }
}
