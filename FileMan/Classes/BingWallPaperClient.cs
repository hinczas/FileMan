using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Raf.FileMan.Classes
{
    public class BingWallPaperClient
    {
        private string _strJSONString = "";
        private string _strRegion = "en-GB";
        private int _numOfImages = 1;
        private string _imgUrl = "";


        private async Task GetImageAsync()
        {
            string strBingImageURL = string.Format("http://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n={0}&mkt={1}", _numOfImages, _strRegion);

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
                }
            }
        }

        public async Task<string> GetDailyImage()
        {
            await GetImageAsync();
            await ParseImage();

            if (_imgUrl.Equals(""))
                return null;
            else
                return "https://www.bing.com" + _imgUrl;
        }
    }
}