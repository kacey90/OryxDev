using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace ByDesignServices.Core.Utilities
{
    public class ByDesignHttpClient
    {
        public ByDesignHttpClient(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public HttpClient HttpClient { get; }
    }
}
