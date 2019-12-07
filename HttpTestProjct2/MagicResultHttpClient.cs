using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HttpTestProjct2
{
    public class MagicResultHttpClient : IMagicResultHttpClient
    {
        private HttpClient HTTPClient;
        private string SpecifiedBaseURL;

        public MagicResultHttpClient(HttpClient _httpClient, string _specifiedBaseURL)
        {
            this.HTTPClient = _httpClient;
            this.SpecifiedBaseURL = _specifiedBaseURL;
        }

        public Task<MagicNumber> FetchMagicNumber()
        {
            if (this.HTTPClient == null)
            {
                throw new ArgumentNullException();
            }

            return this.HTTPClient.GetStringAsync(this.SpecifiedBaseURL)
                .ContinueWith(_ => {
                    var magicNumber = JsonConvert.DeserializeObject<MagicNumber>(_.Result);
                    return magicNumber;
                    }
                );
        }
    }
}
