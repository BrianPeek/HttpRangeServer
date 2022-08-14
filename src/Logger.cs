using System;
using System.Threading.Tasks;

namespace HttpRangeServer
{
	public static class Logger
	{
		public delegate void LogCallback(string s);
		private static LogCallback _logCallback;

		public static void SetLogCallback(LogCallback lc)
		{
			_logCallback = lc;
		}

		public static void Log(string s)
		{
			string msg = $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}: {s}{Environment.NewLine}";

			Console.Write(msg);
			_logCallback?.Invoke(msg);
		}
	}
}
