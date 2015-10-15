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

    }
}
