using BooruSharp.Search.Post;
using Newtonsoft.Json;
using System;
using System.Drawing;

namespace mcswbot2.Bot.Objects
{
    public class TahnosInfo
    {
        public DateTime Acquired;

        public SearchResult SResult;

        [JsonIgnore]
        public Bitmap Bmap;

        public int RelatedMsgID;


        public TahnosInfo(SearchResult result, Bitmap img)
        {
            Acquired = DateTime.Now;
            SResult = result;
            Bmap = img;
        }



        /// <summary>
        /// ...
        /// </summary>
        /// <param name="recurseTry"></param>
        /// <param name="recurseTries"></param>
        /// <returns></returns>
        internal static TahnosInfo Get(int recurseTry = 0, int recurseTries = 5)
        {
            try
            {
                var booru = new BooruSharp.Booru.Gelbooru();
                var result = booru.GetRandomImage(new[] { "" }).Result;
                if (result.fileUrl == null) throw new ArgumentNullException("No result!");
                var request = System.Net.WebRequest.Create(result.fileUrl);
                var response = request.GetResponse();
                var responseStream = response.GetResponseStream();
                return new TahnosInfo(result, new Bitmap(responseStream));
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
