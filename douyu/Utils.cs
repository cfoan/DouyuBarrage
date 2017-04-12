using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace Douyu
{
    public class Utils
    {
        private const string RegexRoomId = "\"room_id\":(\\d*),";
        /// <summary>
        /// 获取unix时间
        /// </summary>
        /// <returns></returns>
        public static long CurrentTimestampUtc()
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((DateTime.UtcNow - epoch).TotalMilliseconds);
        }

        /// <summary>
        /// douyu房间链接获取roomid
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
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

        public static string GetRoomId2(Uri uri)
        {
            WebRequest request = WebRequest.Create(uri);
            using (var sr = new StreamReader(request.GetResponse().GetResponseStream()))
            {
                var response = sr.ReadToEnd();
                Regex regex = new Regex(RegexRoomId, RegexOptions.IgnoreCase);
                var match=regex.Match(response);
                if (match.Success&& match.Groups.Count>1)
                {
                    return match.Groups[1].Value;
                }
            }
            return "";
        }
    }
}
