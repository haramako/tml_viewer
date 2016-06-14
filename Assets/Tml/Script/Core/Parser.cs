using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

// TODO: &nbsp; などのエンティティに対応
// TODO: エラーに寛容に
// TODO: styleのwidth,height
// TODO: float: left or right
// TODO: background-repeat: simple slice tile
// TODO: background-color: NGUI
// TODO: 複数classに対応
// TODO: Styleクラス継承
// TODO: タグ定義

namespace Tml
{
	public class ParserException : Exception {
		public string Filename;
		public int Line;
		public int Column;

		public ParserException(string message, string filename, int line, int column) : base(message){
			Filename = filename;
			Line = line;
			Column = column;
		}
		public override string Message {
			get {
				return "" + base.Message + " in '" + Filename + "' line " + Line + " col " + Column;
			}
		}
	}

	public class Parser
	{
		class InnerError : Exception {
			public InnerError(string message) : base(message) {}
		}

		public static readonly Parser Default = new Parser();
		Dictionary<string, ConstructorInfo> TypeByTag = new Dictionary<string, ConstructorInfo>();

		string filename_;
		XmlReader reader_;
		List<string> errors_;

		public int ErrorCount { get; private set; }

		public Parser()
		{
			// 初期化
			AddTag("div", typeof(BlockElement));
			AddTag("h1", typeof(BlockElement));
			AddTag("h2", typeof(BlockElement));
			AddTag("h3", typeof(BlockElement));
			AddTag("h4", typeof(BlockElement));
			AddTag("h5", typeof(BlockElement));
			AddTag("h6", typeof(BlockElement));
			AddTag("p", typeof(BlockElement));
			AddTag ("span", typeof(InlineElement));
			AddTag ("img", typeof(Img));
			AddTag ("a", typeof(A));
		}

		public void AddTag(string tag, Type type)
		{
			var constructor = type.GetConstructor(new Type[0]);
			TypeByTag.Add(tag, constructor);
		}

		/// <summary>
		/// TMLをパースする
		/// </summary>
		/// <param name="loader"></param>
		/// <param name="reader"></param>
		public Document Parse(string src, string filename = "")
		{
			using (Stream s = new MemoryStream(Encoding.UTF8.GetBytes(src)))
			{
				return Parse(s, filename);
			}
		}

		/// <summary>
		/// TMLをパースする
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public Document Parse(Stream stream, string filename = "")
		{
			filename_ = filename;
			var setting = new XmlReaderSettings
			{
				IgnoreComments = true,
				IgnoreWhitespace = true,
				ConformanceLevel = ConformanceLevel.Fragment,
			};
			reader_ = XmlTextReader.Create(new StreamReader(stream), setting);
			var styleParser = new StyleParser ();

			Document root = new Document();
			root.Tag = "document";
			root.SetClass ("");
			Stack<Element> stack = new Stack<Element>();
			stack.Push(root);

			while (reader_.Read ()) {
				try {
					switch (reader_.NodeType) {
					case XmlNodeType.Text:
						var text = new Text () { Value = convertSpaces (reader_.Value) };
						text.Parent = stack.Peek ();
						text.Tag = "text";
						stack.Peek ().Children.Add (text);
						break;
					case XmlNodeType.Element:
						if (reader_.Name == "style") {
							reader_.Read ();
							if (reader_.NodeType != XmlNodeType.Text) {
								throw new Exception ("must be text");
							}
							styleParser.ParseStyleSheet (root.StyleSheet, reader_.Value);
							reader_.Read ();
							if (reader_.NodeType != XmlNodeType.EndElement) {
								throw new Exception ("must be close tag");
							}
						} else {

							ConstructorInfo constructor;
							if (!TypeByTag.TryGetValue (reader_.Name, out constructor)) {
								throw new InnerError ("unknown tag name " + reader_.Name);
							}
							var element = (Element)constructor.Invoke (null);

							stack.Peek ().Children.Add (element);
							element.Tag = reader_.Name;
							element.SetClass("");
							element.Parent = stack.Peek ();
							stack.Push (element);
							element.Parse (this, reader_);

							if (reader_.IsEmptyElement) {
								stack.Pop ();
							}
						}

						break;
					case XmlNodeType.EndElement:
						stack.Pop ();
						break;
					default:
						throw new InnerError ("invalid element type " + reader_.NodeType);
					}
				} catch (InnerError ex){
					var lineInfo = (IXmlLineInfo)reader_;
					ErrorCount++;
					if (errors_ == null) errors_ = new List<string> ();
					errors_.Add (filename_ + ":" + lineInfo.LineNumber + ":" + lineInfo.LinePosition + " " + ex.Message);
					throw new ParserException (ex.Message, filename_, lineInfo.LineNumber, lineInfo.LinePosition);
				}
			}

			// Logger.Log(root.Dump());

			root.ApplyStyle (root.StyleSheet);

			return root;
		}

		// 複数の空白を一つにする
		string convertSpaces(string str){
			str = str.Trim (' ', '\t', '\r', '\n');
			var b = new StringBuilder ();
			bool inSpaces = false;
			for (int i = 0; i < str.Length; i++) {
				var c = str [i];
				if (c == ' ' || c == '\t' || c == '\r' || c == '\n') {
					if (!inSpaces) {
						inSpaces = true;
						b.Append (' ');
					}
				}else{
					inSpaces = false;
					b.Append (c);
				}
			}
			return b.ToString ();
		}


	}

}

