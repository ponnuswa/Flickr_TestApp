using System;
using System.Collections.Generic;
using System.Text;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace Flickr_Universal_App_Sample
{
    public sealed class Utility
    {
        public static string Md5_Sign(string data)
        {
            HashAlgorithmProvider alg = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            IBuffer buffUtf8Msg = CryptographicBuffer.ConvertStringToBinary(data, BinaryStringEncoding.Utf8);
            IBuffer buffhash = alg.HashData(buffUtf8Msg);
            if (buffhash.Length != alg.HashLength)
                throw new Exception("Md5 Hash Error");
            return CryptographicBuffer.EncodeToHexString(buffhash);
        }

        public static string AuthCalculateSignature(string secretKey, Dictionary<string, string> parameters)
        {
            SortedDictionary<string, string> orderedParams = new SortedDictionary<string, string>();
            foreach (KeyValuePair<string, string> pair in parameters) { orderedParams.Add(pair.Key, pair.Value); }
            string baseString = secretKey + BuildStringForHashing(orderedParams);
            return Md5_Sign(baseString);
        }

        public static string BuildString(Dictionary<string, string> parameters)
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> pair in parameters)
            {
                sb.Append(pair.Key);
                sb.Append("=");
                sb.Append(pair.Value);
                sb.Append("&");
            }
            sb.Remove(sb.Length - 1, 1); // Remove &
            return sb.ToString();
        }

        public static string BuildStringForHashing(SortedDictionary<string, string> parameters)
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> pair in parameters)
            {
                sb.Append(pair.Key);
                sb.Append(pair.Value);
            }
            return sb.ToString();
        }

        //private const string PhotoUrlFormat = "https://farm{0}.staticflickr.com/{1}/{2}_{3}{4}.{5}";

        //internal static string UrlFormat(Photo p, string size, string extension)
        //{
        //    if (size == "_o" || size == "original")
        //        return UrlFormat(p.farm, p.server, p.id, p.secret, size, extension);
        //    else
        //        return UrlFormat(p.farm, p.server, p.id, p.secret, size, extension);
        //}

        //internal static string UrlFormat(string farm, string server, string photoid, string secret, string size, string extension)
        //{
        //    string sizeAbbreviation;
        //    switch (size)
        //    {
        //        case "square":
        //            sizeAbbreviation = "_s";
        //            break;
        //        case "thumbnail":
        //            sizeAbbreviation = "_t";
        //            break;
        //        case "small":
        //            sizeAbbreviation = "_m";
        //            break;
        //        case "large":
        //            sizeAbbreviation = "_b";
        //            break;
        //        case "original":
        //            sizeAbbreviation = "_o";
        //            break;
        //        case "medium":
        //            sizeAbbreviation = string.Empty;
        //            break;
        //        default:
        //            sizeAbbreviation = size;
        //            break;
        //    }

        //    return UrlFormat(PhotoUrlFormat, farm, server, photoid, secret, sizeAbbreviation, extension);
        //}

        //private static string UrlFormat(string format, params object[] parameters)
        //{
        //    return String.Format(System.Globalization.CultureInfo.InvariantCulture, format, parameters);
        //}
    }
}
