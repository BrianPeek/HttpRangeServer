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
				Logger.Log("Starting HttpListenerProcessor...");

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
						Logger.Log($"Request received: {request.Url} from {request.RemoteEndPoint?.Address}");

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
			Logger.Log($"Reading file {filePath}");

			if(!File.Exists(filePath))
			{
				context.Response.StatusCode = 404;
				context.Response.Close();
				return;
			}

			if(!_fileMap.ContainsKey(filePath))
			{
				_fileMap[filePath] = await File.ReadAllBytesAsync(filePath);
			}

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
				context.Response.Headers.Add("Content-Range", range);
				context.Response.OutputStream.Write(bytes, start, length);
			}
			else
			{
				context.Response.StatusCode = 400;
				context.Response.Close();

			}
        }
    }
}
