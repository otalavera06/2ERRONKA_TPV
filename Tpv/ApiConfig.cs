using System;
using System.Configuration;

namespace Tpv
{
    internal static class ApiConfig
    {
        private static readonly string BaseUrlValue =
            (ConfigurationManager.AppSettings["ApiBaseUrl"] ?? "http://192.168.10.5:5093").TrimEnd('/');

        public static string BaseUrl => BaseUrlValue;
        public static string ApiBaseUrl => BaseUrlValue + "/api";

        public static Uri ApiUri(string relativePath)
        {
            var path = relativePath.StartsWith("/") ? relativePath : "/" + relativePath;
            return new Uri(ApiBaseUrl + path);
        }
    }
}
