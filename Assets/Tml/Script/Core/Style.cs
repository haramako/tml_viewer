using System;
using System.Collections.Generic;

namespace Tml
{
	public class StyleSheet {
		Dictionary<string,Style> dict_ = new Dictionary<string,Style>();
		Dictionary<string,Style> cache_ = new Dictionary<string,Style> ();

		public Style GetStyle(params string[] names){
			Style result;
			var fullname = string.Join ("+", names);
			if (cache_.TryGetValue (fullname, out result)) {
				return result;
			}

			var newStyle = Style.Empty ();
			for (int i = 0; i < names.Length; i++) {
				if (names[i] != null && dict_.TryGetValue (names[i], out result)) {
					newStyle.Merge (result);
				}
			}
			newStyle.Seal ();

			cache_ [fullname] = newStyle;
			return newStyle;
		}

		public Style FindOrCreateStyle(string name){
			Style result;
			if (dict_.TryGetValue (name, out result)) {
				return result;
			}else{
				result = Style.Empty ();
				dict_ [name] = result;
				return result;
			}
		}
	}

	public partial class Style
	{
		public const int Nothing = -9999;
		public const int Inherit = -9998;
		public const float NothingFloat = -9999f;
		public const float InheritFloat = -9998f;
		public static int DefaultFontSize = 10;
		public static float DefaultLineScale = 1.5f;
	}


}

