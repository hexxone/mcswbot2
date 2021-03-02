using System;
using System.Net;
using BooruSharp.Booru;
using BooruSharp.Search.Post;
using Newtonsoft.Json;
using SkiaSharp;

namespace mcswbot2.Objects
{
    public class TahnosInfo
    {
        public DateTime Acquired;

        [JsonIgnore] public SKImage Bmap;

        public int RelatedMsgID;

        public SearchResult SResult;


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

        internal static TahnosInfo Get(int recurseTry = 0, int recurseTries = 5)
        {
            try
            {
                var booru = new Gelbooru();
                var result = booru.GetRandomPostAsync("mature").Result;
                if (result.FileUrl == null) throw new ArgumentNullException(nameof(result.FileUrl), "No url given!");
                var request = WebRequest.Create(result.FileUrl);
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