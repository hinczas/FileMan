using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace Raf.FileMan.Classes
{
    public class PixabayWallPaperClient
    {
        private string _strJSONString = "";
        private string _strRegion = "en-GB";
        private int _numOfImages = 1;
        private string _imgUrl = "";
        private string _cpRight = "";
        private string _cpRightUrl = "";

        private string _apiKey = WebConfigurationManager.AppSettings["PB_API_KEY"];


        private async Task GetImageAsync()
        {
            string strBingImageURL = string.Format("https://pixabay.com/api/?key={0}&min_width={1}&image_type=photo&dafesearch=true&per_page=3&editors_choice=true", _apiKey, _strRegion);

            HttpClient client = new HttpClient();

            // Using an Async call makes sure the app is responsive during the time the response is fetched.
            // GetAsync sends an Async GET request to the Specified URI.
            HttpResponseMessage response = await client.GetAsync(new Uri(strBingImageURL));

            // Content property get or sets the content of a HTTP response message. 
            // ReadAsStringAsync is a method of the HttpContent which asynchronously 
            // reads the content of the HTTP Response and returns as a string.
            _strJSONString = await response.Content.ReadAsStringAsync();
        }

        private async Task ParseImage()
        {
            if (_strJSONString!=null)
            {
                JObject jResults = JObject.Parse(_strJSONString);
                foreach (var image in jResults["images"])
                {
                    _imgUrl = (string)image["url"];
                    _cpRight = (string)image["copyright"];
                    _cpRightUrl = (string)image["copyrightlink"];
                }
            }
        }

        public async Task<string[]> GetDailyImage()
        {
            string[] result = new string[3];

            await GetImageAsync();
            await ParseImage();

            if (_imgUrl.Equals(""))
                return null;
            else
            {
                result[0] = _imgUrl;
                result[1] = _cpRight;
                result[2] = _cpRightUrl;

                return result;
            }
        }
    }
}