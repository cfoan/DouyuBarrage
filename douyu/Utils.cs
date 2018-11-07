﻿using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Douyu
{
    public class Utils
    {
        private const string RegexRoomId = "\"room_id\":(\\d*),";
        private const string RegexRoomId2 = "\"online_id\":\"(\\d*)\",";
        private const string RegexRoomId3 = "onlineid=(\\d*)";

        private const string REGEX_SERVER = "%7B%22ip%22%3A%22(.*?)%22%2C%22port%22%3A%22(.*?)%22%7D%2C";

        //webGetRoom :http://www.douyu.com/specific/webMGetRoom/{roomId}
        //isRecording: http://www.douyu.com/swf_api/getRoomRecordStatus/
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
            string ip = null;
            WebRequest request = WebRequest.Create(uri);

            using (var sr = new StreamReader(request.GetResponse().GetResponseStream()))
            {
                var response = sr.ReadToEnd();
                Regex regexIp = new Regex(REGEX_SERVER, RegexOptions.IgnoreCase);

                
                var match = regexIp.Match(response);

                if (match.Success && match.Groups.Count > 1)
                {
                    ip= match.Groups[1].Value;
                }

                Regex regex = new Regex(RegexRoomId, RegexOptions.IgnoreCase);
                match = regex.Match(response);
                if (match.Success&& match.Groups.Count>1)
                {
                    return match.Groups[1].Value;
                }

                Regex regex2 = new Regex(RegexRoomId2, RegexOptions.IgnoreCase);
                match = regex2.Match(response);
                if (match.Success && match.Groups.Count > 1)
                {
                    return match.Groups[1].Value;
                }

                Regex regex3 = new Regex(RegexRoomId3, RegexOptions.IgnoreCase);
                match = regex3.Match(response);
                if (match.Success && match.Groups.Count > 1)
                {
                    return match.Groups[1].Value;
                }
            }
            return "";
        }

        //public string getServerIp()
        //{

        //}


        public static void GetRoom()
        {
            throw new NotSupportedException();
        }

        private static string logPath = AppDomain.CurrentDomain.BaseDirectory + "\\log.txt";
        private static volatile StringBuilder sb = new StringBuilder();

        public static void Dumps(string log)
        {
            using (FileStream fs = new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                if (fs.CanWrite)
                {
                    using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                    {
                        if (sb.Length > 0)
                        {
                            sw.WriteLine(sb.ToString());
                            sb.Clear();
                        }
                        sw.WriteLine(log);
                    }
                }
                else
                {
                    sb.AppendLine(log);
                }
            }
        }
    }
}
