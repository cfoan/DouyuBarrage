using System;
using System.IO;
using System.Net;

namespace DouyuDanmu
{
    public class Utils
    {
        public static long CurrentTimestampUtc()
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((DateTime.UtcNow - epoch).TotalMilliseconds);
        }

        public static string GetRoomId(Uri uri)
        {
            WebRequest request = WebRequest.Create(uri);
            using (var sr = new StreamReader(request.GetResponse().GetResponseStream()))
            {
                var response=sr.ReadToEnd();
                var index0 = response.IndexOf("var $ROOM");
                var index1 = response.IndexOf(":", index0);
                var index2 = response.IndexOf(",", index1);
                var count = index2 - index1 - 1;
                if (count > 0 && index1 + 1 + count <= response.Length)
                {
                    return response.Substring(index1 + 1, index2 - index1 - 1);
                }
            }
            return "";
        }
    }
}
