using System.IO.Compression;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace HttpRangeServer
{
    internal class HttpListenerProcessor
    {
		private readonly HttpListener _listener = new();
		private readonly Dictionary<string, byte[]> _fileMap = new();
		private string _basePath;

		public async Task StartAsync(int port, string path)
		{
			try
			{
				Logger.Log($"Starting HttpListenerProcessor, serving files from {path}...");

				_basePath = path;

				// netsh http add urlacl url=http://*:PORT/ user=Everyone
				_listener.Prefixes.Add($"http://*:{port}/"); // listen on root

				_listener.Start();
				Logger.Log($"Listening on port {port}");

				while(true)
				{
					try
					{
						HttpListenerContext context = await _listener.GetContextAsync();
						HttpListenerRequest request = context.Request;
						Logger.Log($"Request received: {request.Url}");

						await ProcessRequest(context, request);
					}
					catch(Exception e)
					{
						Logger.Log(e.ToString());
					}
				}
			}
			catch(Exception ex)
			{
				Logger.Log(ex.ToString());
			}
		}

        private async Task ProcessRequest(HttpListenerContext context, HttpListenerRequest request)
        {
			byte[] bytes;

			string urlPath = HttpUtility.UrlDecode(request.Url?.AbsolutePath.TrimStart('/'));
			string filePath = Path.Combine(_basePath, urlPath);

			if(!_fileMap.ContainsKey(filePath))
			{
				if(!File.Exists(filePath))
				{
					Logger.Log($"{filePath} not found, returning 404");
					context.Response.StatusCode = 404;
				}
				else
				{
					if(urlPath.EndsWith("zip"))
					{
						Logger.Log($"Serving first file in zip archive");
                        using(FileStream zipFile = File.OpenRead(filePath))
						{
							using(var zipArchive = new ZipArchive(zipFile, ZipArchiveMode.Read))
							{
								using(var ms = new MemoryStream())
								{
                                    Stream s = zipArchive.Entries[0].Open();
									s.CopyTo(ms);
									_fileMap[filePath] = ms.ToArray();;
								}
							}
						}
					}
					else
					{
						Logger.Log($"Reading and caching {filePath}");
						_fileMap[filePath] = await File.ReadAllBytesAsync(filePath);
					}
				}
			}

			if(_fileMap.ContainsKey(filePath))
			{
				bytes = _fileMap[filePath];

				string rangeHeader = request.Headers["Range"];
				if(!string.IsNullOrEmpty(rangeHeader))
				{
					Match matches = Regex.Match(rangeHeader, @"^bytes=(\d*)-(\d*)*$", RegexOptions.Compiled);

					int start  = Convert.ToInt32(matches.Groups[1].Captures[0].Value);
					int end    = Convert.ToInt32(matches.Groups[2].Captures[0].Value);
					int length = end-start+1;

					context.Response.StatusCode = 206;
					context.Response.ContentLength64 = length;
					string range = $"bytes {start}-{end}/{bytes.Length}";
					Logger.Log($"Returning {range}");
					context.Response.Headers.Add("Content-Range", range);
					context.Response.OutputStream.Write(bytes, start, length);
				}
			}
			else
			{
				Logger.Log($"{filePath} not in map, returning 400");
				context.Response.StatusCode = 400;
			}

			context.Response.Close();
        }
    }
}
