using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ArcShare.Server
{
	/*
		//HTTP请求行
		GET / HTTP/1.1
		//请求头部
		Accept: text/html, application/xhtml+xml, image/jxr, *//*
		Accept-Encoding: gzip, deflate
		Accept-Language: zh-Hans-CN, zh-Hans; q=0.5
		Connection: Keep-Alive
		Host: localhost:4000
		User-Agent: Mozilla/5.0 (Windows NT 10.0; Trident/7.0; rv:11.0) like Gecko

		//消息体
	*/
	class HttpRequestHeader
	{
		public string Method { get; set; }
		public string RequestedUrl { get; set; }
		public string Accept { get; set; }
		public string Connection { get; set; }
		public string Host { get; set; }
		public string UserAgent { get; set; }
		//public UInt64 RangeX { get; set; }
		//public UInt64 RangeY { get; set; }
		public string Origin { get; set; }
		public string ContentType { get; set; }
		public long ContentLength { get; set; }

		//public string RequestBody { get; set; }

		public static HttpRequestHeader Create(string raw)
		{
			HttpRequestHeader request = new HttpRequestHeader();
			string[] lines = raw.Split('\n');
			request.Method = lines[0].Split(' ')[0];

			int count = 0;
			foreach (var line in lines)
			{
				count++;

				var parts = line.Split(": ");

				if (count == 1)
				{
					try
					{
						var devided = line.Split(' ');
						request.Method = devided[0];
						request.RequestedUrl = devided[1];
						continue;
					}
					catch (IndexOutOfRangeException)
					{
						//
					}

				}

				switch (parts[0])
				{
					case "Accept":
						request.Accept = parts[1];
						break;
					case "Connection":
						request.Connection = parts[1];
						break;
					case "Host":
						request.Host = parts[1];
						break;
					case "User-Agent":
						request.UserAgent = parts[1];
						break;
					case "Origin":
						request.Origin = parts[1];
						break;
					case "Content-Type":
						request.ContentType = parts[1].Replace("\r", "").Replace("\n", "");
						break;
					case "Content-Length":
						request.ContentLength = Convert.ToInt64(parts[1]);
						break;
						//case "Range":
						//Range: bytes=7340032-8388607
						//string ranges = parts[1];
						//var spl = ranges.Substring(6).Split('-');
						//request.RangeX = Convert.ToUInt64(spl[0]);
						//request.RangeY = Convert.ToUInt64(spl[1]);
						//break;
				}
			}
			return request;
		}
	}

}
