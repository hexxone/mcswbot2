using System;
using BooruSharp.Search.Post;
using Newtonsoft.Json;
using SkiaSharp;

namespace mcswbot2.Objects
{
    public class TahnosInfo
    {
        public DateTime Acquired;

        public SearchResult SResult;

        [JsonIgnore]
        public SKImage Bmap;

        public int RelatedMsgID;


        private TahnosInfo(SearchResult result, SKImage img)
        {
            Acquired = DateTime.Now;
            SResult = result;
            Bmap = img;
        }

        [JsonConstructor]
        internal TahnosInfo(DateTime acquired, SearchResult sresult, int relatedMsgId)
        {
            Acquired = acquired;
            SResult = sresult;
            RelatedMsgID = relatedMsgId;
        }

        /// <summary>
        /// ...
        /// </summary>
        /// <param name="recurseTry"></param>
        /// <param name="recurseTries"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        internal static TahnosInfo Get(int recurseTry = 0, int recurseTries = 5)
        {
            try
            {
                var booru = new BooruSharp.Booru.Gelbooru();
                var result = booru.GetRandomPostAsync(new[] { "mature" }).Result;
                if (result.FileUrl == null) throw new ArgumentNullException(nameof(result.FileUrl), "No url given!");
                var request = System.Net.WebRequest.Create(result.FileUrl);
                var response = request.GetResponse();
                var responseStream = response.GetResponseStream();
                return new TahnosInfo(result, SKImage.FromEncodedData(responseStream));
            }
            catch (Exception ex)
            {
                Program.WriteLine("Imaging-Exception: " + ex);
                if (recurseTry < recurseTries)
                    return Get(recurseTry + 1);
            }
            return null;
        }
    }
}
