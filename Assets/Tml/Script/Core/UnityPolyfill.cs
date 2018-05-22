#if !UNITY_2017_2_OR_NEWER
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
#else
using System;
using UnityEngine;
using System.Text;

namespace Tml {
	public class Logger
	{
		public static bool Enabled = false;
		static ILogger logger_;
		public static bool StopOnException = true;

		public static void SetLogger(ILogger logger){
			Enabled = true;
			logger_ = logger;
		}

		public static void Log(object text)
		{
			if (Enabled && logger_ != null) logger_.Log ("Tml", text);
		}

		public static void LogWarning(object text){
			if (Enabled && logger_ != null) logger_.LogWarning ("Tml", text);
		}

		public static void LogError(object text){
			if (Enabled && logger_ != null) logger_.LogError ("Tml", text);
		}

		public static void LogException(Exception ex){
			if (StopOnException) {
				throw ex;
			}else{
				if (Enabled && logger_ != null) logger_.LogException (ex);
			}
		}
	}
}

// StringBuilder.Clear()のpolyfill
public static class StringBuilderExtension {
	public static void Clear(this StringBuilder self){
		self.Remove(0, self.Length);
	}
}
#endif
