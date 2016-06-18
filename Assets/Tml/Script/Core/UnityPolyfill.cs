#if !UNITY
using System;

namespace Tml {
	public class Logger
	{
		public static bool Enable = false;
		public static bool StopOnException = true;

		public static void Log(object text)
		{
			if( Enable ) Console.WriteLine(text.ToString());
		}

		public static void LogWarning(object text){
			if( Enable ) Console.WriteLine(text.ToString());
		}

		public static void LogError(object text){
			if( Enable ) Console.WriteLine(text.ToString());
		}

		public static void LogException(Exception ex){
			if (StopOnException) {
				throw ex;
			} else {
				Console.WriteLine (ex.ToString ());
			}
		}
	}
}
#endif

#if UNITY
using System;

// StringBuilder.Clear()のpolyfill
public static class StringBuilderExtension {
	public static void Clear(this StringBuilder self){
		self.Remove(0, self.Length);
	}
}
#endif
