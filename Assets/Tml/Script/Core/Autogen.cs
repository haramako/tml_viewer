// This file is auto-generated by codegen.rb
namespace Tml {
	public partial class Style
	{
        public int MarginLeft;
        public int MarginRight;
        public int MarginTop;
        public int MarginBottom;
        public int PaddingLeft;
        public int PaddingRight;
        public int PaddingTop;
        public int PaddingBottom;
        public int FontSize;
        public float FontScale;
        public int LineHeight;
        public float LineScale;
        public string TextDecoration;
        public string Color;
        public string TextAlign;
        public string BackgroundImage;
        public string BackgroundColor;

		public void Merge(Style over){
            if( over.MarginLeft != Nothing ) MarginLeft = over.MarginLeft;
            if( over.MarginRight != Nothing ) MarginRight = over.MarginRight;
            if( over.MarginTop != Nothing ) MarginTop = over.MarginTop;
            if( over.MarginBottom != Nothing ) MarginBottom = over.MarginBottom;
            if( over.PaddingLeft != Nothing ) PaddingLeft = over.PaddingLeft;
            if( over.PaddingRight != Nothing ) PaddingRight = over.PaddingRight;
            if( over.PaddingTop != Nothing ) PaddingTop = over.PaddingTop;
            if( over.PaddingBottom != Nothing ) PaddingBottom = over.PaddingBottom;
            if( over.FontSize != Nothing ) FontSize = over.FontSize;
            if( over.FontScale != NothingFloat ) FontScale = over.FontScale;
            if( over.LineHeight != Nothing ) LineHeight = over.LineHeight;
            if( over.LineScale != NothingFloat ) LineScale = over.LineScale;
            if( over.TextDecoration != null ) TextDecoration = over.TextDecoration;
            if( over.Color != null ) Color = over.Color;
            if( over.TextAlign != null ) TextAlign = over.TextAlign;
            if( over.BackgroundImage != null ) BackgroundImage = over.BackgroundImage;
            if( over.BackgroundColor != null ) BackgroundColor = over.BackgroundColor;
		}

		public void Seal(){
            if( MarginLeft == Nothing ) MarginLeft = 0;
            if( MarginRight == Nothing ) MarginRight = 0;
            if( MarginTop == Nothing ) MarginTop = 0;
            if( MarginBottom == Nothing ) MarginBottom = 0;
            if( PaddingLeft == Nothing ) PaddingLeft = 0;
            if( PaddingRight == Nothing ) PaddingRight = 0;
            if( PaddingTop == Nothing ) PaddingTop = 0;
            if( PaddingBottom == Nothing ) PaddingBottom = 0;
            if( FontSize == Nothing ) FontSize = Inherit;
            if( FontScale == NothingFloat ) FontScale = 1.0f;
            if( LineHeight == Nothing ) LineHeight = Inherit;
            if( LineScale == NothingFloat ) LineScale = InheritFloat;
            if( TextDecoration == null ) TextDecoration = "";
            if( Color == null ) Color = "";
            if( TextAlign == null ) TextAlign = "";
            if( BackgroundImage == null ) BackgroundImage = "";
            if( BackgroundColor == null ) BackgroundColor = "";
		}

        public void SetField(string name, string value){
            switch(name){
            case "margin-left":
                MarginLeft = int.Parse(value);
                break;
            case "margin-right":
                MarginRight = int.Parse(value);
                break;
            case "margin-top":
                MarginTop = int.Parse(value);
                break;
            case "margin-bottom":
                MarginBottom = int.Parse(value);
                break;
            case "padding-left":
                PaddingLeft = int.Parse(value);
                break;
            case "padding-right":
                PaddingRight = int.Parse(value);
                break;
            case "padding-top":
                PaddingTop = int.Parse(value);
                break;
            case "padding-bottom":
                PaddingBottom = int.Parse(value);
                break;
            case "font-size":
                FontSize = int.Parse(value);
                break;
            case "font-scale":
                FontScale = float.Parse(value);
                break;
            case "line-height":
                LineHeight = int.Parse(value);
                break;
            case "line-scale":
                LineScale = float.Parse(value);
                break;
            case "text-decoration":
                TextDecoration = (value);
                break;
            case "color":
                Color = (value);
                break;
            case "text-align":
                TextAlign = (value);
                break;
            case "background-image":
                BackgroundImage = (value);
                break;
            case "background-color":
                BackgroundColor = (value);
                break;
            default:
				Logger.Log ("invalid field name '" + name + "'");
                break;
            }
        }

		public static Style Empty(){
			return new Style () {
                MarginLeft = Nothing,
                MarginRight = Nothing,
                MarginTop = Nothing,
                MarginBottom = Nothing,
                PaddingLeft = Nothing,
                PaddingRight = Nothing,
                PaddingTop = Nothing,
                PaddingBottom = Nothing,
                FontSize = Nothing,
                FontScale = NothingFloat,
                LineHeight = Nothing,
                LineScale = NothingFloat,
                TextDecoration = null,
                Color = null,
                TextAlign = null,
                BackgroundImage = null,
                BackgroundColor = null,
			};
		}

	}

    public partial class StyleParser {
        static CharType[] CharTable = {
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.Space, // "\t"
            CharType.Space, // "\n"
            CharType.None,
            CharType.None,
            CharType.Space, // "\r"
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.Space, // " "
            CharType.None,
            CharType.Quote, // "\""
            CharType.Symbol, // "#"
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.Quote, // "'"
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.Separator, // "-"
            CharType.None,
            CharType.None,
            CharType.Number | CharType.Hex, // "0"
            CharType.Number | CharType.Hex, // "1"
            CharType.Number | CharType.Hex, // "2"
            CharType.Number | CharType.Hex, // "3"
            CharType.Number | CharType.Hex, // "4"
            CharType.Number | CharType.Hex, // "5"
            CharType.Number | CharType.Hex, // "6"
            CharType.Number | CharType.Hex, // "7"
            CharType.Number | CharType.Hex, // "8"
            CharType.Number | CharType.Hex, // "9"
            CharType.Symbol, // ":"
            CharType.Symbol, // ";"
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.Alphabet | CharType.Hex, // "A"
            CharType.Alphabet | CharType.Hex, // "B"
            CharType.Alphabet | CharType.Hex, // "C"
            CharType.Alphabet | CharType.Hex, // "D"
            CharType.Alphabet | CharType.Hex, // "E"
            CharType.Alphabet | CharType.Hex, // "F"
            CharType.Alphabet, // "G"
            CharType.Alphabet, // "H"
            CharType.Alphabet, // "I"
            CharType.Alphabet, // "J"
            CharType.Alphabet, // "K"
            CharType.Alphabet, // "L"
            CharType.Alphabet, // "M"
            CharType.Alphabet, // "N"
            CharType.Alphabet, // "O"
            CharType.Alphabet, // "P"
            CharType.Alphabet, // "Q"
            CharType.Alphabet, // "R"
            CharType.Alphabet, // "S"
            CharType.Alphabet, // "T"
            CharType.Alphabet, // "U"
            CharType.Alphabet, // "V"
            CharType.Alphabet, // "W"
            CharType.Alphabet, // "X"
            CharType.Alphabet, // "Y"
            CharType.Alphabet, // "Z"
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.Separator, // "_"
            CharType.None,
            CharType.Alphabet | CharType.Hex, // "a"
            CharType.Alphabet | CharType.Hex, // "b"
            CharType.Alphabet | CharType.Hex, // "c"
            CharType.Alphabet | CharType.Hex, // "d"
            CharType.Alphabet | CharType.Hex, // "e"
            CharType.Alphabet | CharType.Hex, // "f"
            CharType.Alphabet, // "g"
            CharType.Alphabet, // "h"
            CharType.Alphabet, // "i"
            CharType.Alphabet, // "j"
            CharType.Alphabet, // "k"
            CharType.Alphabet, // "l"
            CharType.Alphabet, // "m"
            CharType.Alphabet, // "n"
            CharType.Alphabet, // "o"
            CharType.Alphabet, // "p"
            CharType.Alphabet, // "q"
            CharType.Alphabet, // "r"
            CharType.Alphabet, // "s"
            CharType.Alphabet, // "t"
            CharType.Alphabet, // "u"
            CharType.Alphabet, // "v"
            CharType.Alphabet, // "w"
            CharType.Alphabet, // "x"
            CharType.Alphabet, // "y"
            CharType.Alphabet, // "z"
            CharType.Symbol, // "{"
            CharType.None,
            CharType.Symbol, // "}"
            CharType.None,
            CharType.None,
        };
    }
}

#if !USE_SYSTEM_XML
namespace Tml.XmlPolyfill {
    public partial class XmlReader : IXmlLineInfo {
        static CharType[] CharTable = {
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.Space, // "\t"
            CharType.Space, // "\n"
            CharType.None,
            CharType.None,
            CharType.Space, // "\r"
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.Space, // " "
            CharType.None,
            CharType.Quote, // "\""
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.Quote, // "'"
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.Separator, // "-"
            CharType.None,
            CharType.Close, // "/"
            CharType.Number, // "0"
            CharType.Number, // "1"
            CharType.Number, // "2"
            CharType.Number, // "3"
            CharType.Number, // "4"
            CharType.Number, // "5"
            CharType.Number, // "6"
            CharType.Number, // "7"
            CharType.Number, // "8"
            CharType.Number, // "9"
            CharType.None,
            CharType.None,
            CharType.Open, // "<"
            CharType.None,
            CharType.Close, // ">"
            CharType.None,
            CharType.None,
            CharType.Alphabet, // "A"
            CharType.Alphabet, // "B"
            CharType.Alphabet, // "C"
            CharType.Alphabet, // "D"
            CharType.Alphabet, // "E"
            CharType.Alphabet, // "F"
            CharType.Alphabet, // "G"
            CharType.Alphabet, // "H"
            CharType.Alphabet, // "I"
            CharType.Alphabet, // "J"
            CharType.Alphabet, // "K"
            CharType.Alphabet, // "L"
            CharType.Alphabet, // "M"
            CharType.Alphabet, // "N"
            CharType.Alphabet, // "O"
            CharType.Alphabet, // "P"
            CharType.Alphabet, // "Q"
            CharType.Alphabet, // "R"
            CharType.Alphabet, // "S"
            CharType.Alphabet, // "T"
            CharType.Alphabet, // "U"
            CharType.Alphabet, // "V"
            CharType.Alphabet, // "W"
            CharType.Alphabet, // "X"
            CharType.Alphabet, // "Y"
            CharType.Alphabet, // "Z"
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.Separator, // "_"
            CharType.None,
            CharType.Alphabet, // "a"
            CharType.Alphabet, // "b"
            CharType.Alphabet, // "c"
            CharType.Alphabet, // "d"
            CharType.Alphabet, // "e"
            CharType.Alphabet, // "f"
            CharType.Alphabet, // "g"
            CharType.Alphabet, // "h"
            CharType.Alphabet, // "i"
            CharType.Alphabet, // "j"
            CharType.Alphabet, // "k"
            CharType.Alphabet, // "l"
            CharType.Alphabet, // "m"
            CharType.Alphabet, // "n"
            CharType.Alphabet, // "o"
            CharType.Alphabet, // "p"
            CharType.Alphabet, // "q"
            CharType.Alphabet, // "r"
            CharType.Alphabet, // "s"
            CharType.Alphabet, // "t"
            CharType.Alphabet, // "u"
            CharType.Alphabet, // "v"
            CharType.Alphabet, // "w"
            CharType.Alphabet, // "x"
            CharType.Alphabet, // "y"
            CharType.Alphabet, // "z"
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
            CharType.None,
        };
    }
}
#endif

