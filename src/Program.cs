namespace HttpRangeServer
{
    class Program
	{
		private static readonly HttpListenerProcessor _httpListenerProcessor = new();

		async static Task Main(string[] args)
		{
			int port = 1165;
			string path = Environment.CurrentDirectory;
			bool cache = true;

			for(int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
					case "--nocache":
						cache = false;
						break;

					case "--port":
						port = Convert.ToInt32(args[i+1]);
						i++;
						break;

					case "--path":
						path = args[i+1];
						i++;
						break;

					case "-?":
					case "--help":
					default:
						Usage();
						return;
                }
            }

			if(!Directory.Exists(path))
				throw new ArgumentException($"Path not found: {path}");

			await _httpListenerProcessor.StartAsync(port, path, cache);

			// wait forever
			new CancellationToken().WaitHandle.WaitOne();
		}

		static void Usage()
		{
			Console.WriteLine("");
			Console.WriteLine("Usage: HttpRangeServer [options...]");
			Console.WriteLine("");
			Console.WriteLine("Options:");
			Console.WriteLine("  --path <path>  Path to directory of images   (default: current directory)");
			Console.WriteLine("  --port <port   TCP port to listen on         (default: 1165)");
			Console.WriteLine("  --nocache      Do not cache chunks in memory (default: caching enabled)");
			Console.WriteLine("  --help");
		}
	}
}
