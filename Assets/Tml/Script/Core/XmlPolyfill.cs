#if !USE_SYSTEM_XML
using System;
using System.IO;
using System.Text;

/// <summary>
/// System.Xml の代替
/// 
/// Tmlで最低限必要な機能だけを実装している
/// </summary>

namespace Tml.XmlPolyfill
{
	public class XmlException : Exception {
		public XmlException(string message): base(message){
		}
	}

	public enum ConformanceLevel {
		Fragment
	}

	public class XmlReaderSettings {
		public bool IgnoreComments;
		public bool IgnoreWhitespace;
		public ConformanceLevel ConformanceLevel;
	}

	public enum XmlNodeType {
		None,
		Element,
		Attribute,
		Text,
		CDATA,
		EntityReference,
		Entity,
		ProcessingInstruction,
		Comment,
		Document,
		DocumentType,
		DocumentFragment,
		Notation,
		Whitespace,
		SignificantWhitespace,
		EndElement,
		EndEntity,
		XmlDeclaration
	}

	public interface IXmlLineInfo {
		int LineNumber { get; }
		int LinePosition { get; }
	}

	public partial class XmlReader : IXmlLineInfo {

		[Flags]
		enum CharType {
			None = 0,
			Alphabet = 1,
			Separator = 2,
			Number = 4,
			Open = 8,
			Close = 16,
			Quote = 32,
			Space = 64,
		}

		public XmlNodeType NodeType { get; private set; }
		public string Value { get; private set; }
		public string Name { get; private set; }
		public bool IsEmptyElement { get; private set; }
		public bool HasAttributes { get { return AttributeCount != 0; } }
		public int AttributeCount { get; private set; }

		public string[] attributeNames_ = new string[16];
		public string[] attributeValues_ = new string[16];
		public string[] tagStack_ = new string[32];
		public int tagDepth_ = 0;
		XmlNodeType fragmentType_;
		string ident_;
		StringBuilder sb_ = new StringBuilder();

		TextReader reader_;
		//XmlReaderSettings settings_;

		public XmlReader(StreamReader reader, XmlReaderSettings settings){
			reader_ = reader;
			//settings_ = settings;
		}

		public int LineNumber { get { return 0; } }
		public int LinePosition { get { return 0; } }

		public bool MoveToAttribute(int i){
			Name = attributeNames_ [i];
			Value = attributeValues_ [i];
			return false;
		}

		public bool MoveToElement(){
			return false;
		}

		public bool Read(){
			return parseElementContent ();
		}

		//=========================================
		// Lexer処理
		//=========================================

		int peek(){
			return reader_.Peek ();
		}

		int read(){
			return reader_.Read ();
		}

		CharType charType(int c){
			if (c >= 0 && (c < 128)) {
				return CharTable [c];
			} else {
				return CharType.None;
			}
		}

		bool isIdentChar(int c){
			return (charType(c) & (CharType.Alphabet | CharType.Number | CharType.None)) != 0;
		}

		bool isQuoteChar(int c){
			return (charType(c) & (CharType.Quote)) != 0;
		}

		bool isSpaceChar(int c){
			return (charType(c) & (CharType.Space)) != 0;
		}

		void throwUnexpectedToken(params string[] expectedTokens){
			throw new XmlException ("expect " + string.Join (" or ", expectedTokens));
		}

		//=========================================
		// Parser処理
		//=========================================

		// Parses the document content
		bool parseElementContent() {
			for (;;) {
				int c = peek ();
				// some tag
				if ( c == '<' ) {
					read ();
					switch ( c = peek() ) {
					// processing instruction
					case '?':
						read ();
						//if ( ParsePI() ) {
						//	return true;
						//}
						continue;
					case '!':
						read ();
						// comment
						if ( peek() == '-' ) {
							if ( peek() == '-' ) {
								read ();
								read ();
								//if ( ParseComment() ) {
								//	return true;
								//}
								continue;
							}
							else {
								throwUnexpectedToken("-" );
							}
						}
						// CDATA section
						else if ( c == '[' ) {
							/*
							if ( fragmentType != XmlNodeType.Document ) {
								pos++;
								if ( ps.charsUsed - pos < 6 ) {
									goto ReadData;
								}
								if ( XmlConvert.StrEqual( chars, pos, 6, "CDATA[" ) ) {
									ps.charPos = pos + 6;
									ParseCData();
									if ( fragmentType == XmlNodeType.None ) {
										fragmentType = XmlNodeType.Element;
									}
									return true; 
								}
								else {
									ThrowUnexpectedToken( pos, "CDATA[" );
								}
							}
							else {
								Throw( ps.charPos, Res.Xml_InvalidRootData );
							}
							*/
						}
						// DOCTYPE declaration
						else {
							if ( fragmentType_ == XmlNodeType.Document || fragmentType_ == XmlNodeType.None ) {
								fragmentType_ = XmlNodeType.Document;
								read ();
								/*
								if ( ParseDoctypeDecl() ) {
									return true;
								}
								*/
								continue;
							}
							else {
								/*
								if ( ParseUnexpectedToken( pos ) == "DOCTYPE" ) {
									Throw( Res.Xml_BadDTDLocation );
								}
								else {
									ThrowUnexpectedToken( pos, "<!--", "<[CDATA[" ); 
								}
								*/
							}
						}
						break;
					case '/':
						read ();
						parseEndElement ();
						return true;
					default:
						parseElement();
						return true;
					}
				}
				else if ( c == '&' ) {
					if ( fragmentType_ == XmlNodeType.Document ) {
						throw new XmlException ("invalid root data");
					}
					else {
						if ( fragmentType_ == XmlNodeType.None ) {
							fragmentType_ = XmlNodeType.Element;
						}
						/* TODO
						switch ( HandleEntityReference( false, EntityExpandType.OnlyGeneral, out i ) ) {
						case EntityType.CharacterDec:
						case EntityType.CharacterHex:
						case EntityType.CharacterNamed:
							if ( ParseText() ) {
								return true;
							}
							continue;
						default:
							chars = ps.chars;
							pos = ps.charPos;
							continue;
						}
						*/
						continue;
					}
				}
				// end of buffer
				else if ( c == -1 ) {
					if (tagDepth_ > 0) {
						throw new XmlException ("unclosed tag " + tagStack_[tagDepth_-1]);
					}
					return false;
				}
				// something else -> root level whitespaces
				else {
					if ( parseText() ) {
						return true;
					}
					continue;
				}
			}
		}

		bool parseText(){
			for (;;) {
				int c = peek ();
				if (c == '<') {
					break;
				}else if (c == '&') {
					// TODO
					break;
				} else if (c == -1) {
					break;
				} else {
					sb_.Append ((char)read());
				}
			}
			NodeType = XmlNodeType.Text;
			Value = sb_.ToString ().Trim (' ', '\n', '\r', '\t');
			sb_.Clear ();

			if (string.IsNullOrEmpty (Value)) {
				return false;
			} else {
				return true;
			}
		}

		bool parseIdent(){
			for (;;) {
				int c = peek ();
				if (isIdentChar (c)) {
					sb_.Append ((char)read ());
				} else {
					break;
				}
			}
			ident_ = sb_.ToString ();
			sb_.Clear ();
			return (ident_.Length > 0);
		}

		bool parseAttributeValue(){
			var c = peek ();
			if( isQuoteChar(c) ){
				var quote = c;
				read ();
				while( peek() != quote ) {
					sb_.Append ((char)read ());
				}
				read ();
			}else{
				while (isIdentChar(peek())) {
					sb_.Append ((char)read ());
				}
			}
			ident_ = sb_.ToString ();
			sb_.Clear ();
			return (ident_.Length > 0);
		}

		void skipSpaces(){
			for(;;){
				int c = peek();
				if( !isSpaceChar(c) ) break;
				read ();
			}
		}

		void parseElement(){
			int c;
			AttributeCount = 0;
			IsEmptyElement = false;
			NodeType = XmlNodeType.Element;

			if( ! parseIdent () ){
				throw new XmlException ("No element name");
			}
			Name = ident_;
			skipSpaces ();

			while (parseIdent ()) {
				attributeNames_ [AttributeCount] = ident_;
				if (peek () == '=') {
					read ();
					parseAttributeValue ();
					attributeValues_ [AttributeCount] = ident_;
				} else {
					attributeValues_ [AttributeCount] = null;
				}
				skipSpaces ();
				AttributeCount++;
			}

			c = peek ();
			if (c == '>') {
				read ();
				tagStack_ [tagDepth_] = Name;
				tagDepth_++;
				return;
			} else if (c == '/') {
				read ();
				c = peek ();
				if (c == '>') {
					read ();
					IsEmptyElement = true;
					return;
				} else {
					throwUnexpectedToken (">");
				}
			} else {
				throwUnexpectedToken (">", "/>");
			}
		}

		void parseEndElement(){
			for (;;) {
				int c = read ();
				if (c == '>') {
					break;
				} else if (c == -1) {
					throwUnexpectedToken ("EOF");
				} else {
					sb_.Append ((char)c);
				}
			}
			NodeType = XmlNodeType.EndElement;
			Name = sb_.ToString();
			sb_.Clear ();

			tagDepth_--;
			if (tagDepth_ < 0) {
				throw new XmlException ("too many close tag");
			} else if( tagStack_[tagDepth_] != Name ){
				throw new XmlException ("unmatch close tag, expect '" + tagStack_ [tagDepth_] + "' but '" + Name + "'");
			}
		}
	}

	public class XmlTextReader {
		public static XmlReader Create(StreamReader reader, XmlReaderSettings settings){
			return new XmlReader (reader, settings);
		}
	}

}

#endif
