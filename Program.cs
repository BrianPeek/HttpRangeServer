namespace HttpRangeServer
{
    class Program
	{
		private static readonly HttpListenerProcessor _httpListenerProcessor = new();

		async static Task Main(string[] args)
		{
			int port = 1165;
			string path = Environment.CurrentDirectory;

			for(int i = 0; i < args.Length; i+=2)
            {
                switch (args[i])
                {
					case "--port":
						port = Convert.ToInt32(args[i+1]);
						break;

					case "--path":
						path = args[i+1];
						break;

					default:
						throw new ArgumentException("Unknown argument");
                }
            }

            Logger.Log("Starting up...");
			await _httpListenerProcessor.StartAsync(port, path);

			// wait forever
			new CancellationToken().WaitHandle.WaitOne();
		}
	}
}
