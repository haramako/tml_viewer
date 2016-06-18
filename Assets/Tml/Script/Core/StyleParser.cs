using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tml
{
	public partial class StyleParser {
		StreamReader r_;
		StringBuilder sb_ = new StringBuilder();
		StyleSheet currentSheet_;
		Style currentStyle_;
		TokenType tokenType_;
		string tokenString_;
		string[] values_ = new string[8];

		[Flags]
		enum CharType {
			None = 0,
			Alphabet = 1,
			Separator = 2,
			Number = 4,
			Ident = 8,
			Symbol = 16,
			Quote = 32,
			Space = 64,
			Hex = 256,
		}

		public StyleParser() {
		}

		public void ParseStyleSheet(StyleSheet sheet, string str){
			using (var stream = new MemoryStream (Encoding.UTF8.GetBytes (str))) {
				ParseStyleSheet (sheet, stream);
			}
		}

		public void ParseStyleSheet(StyleSheet sheet, Stream stream){
			currentSheet_ = sheet;
			using (r_ = new StreamReader (stream)) {
				nextToken ();
				parseStyleSheet ();
			}
		}

		public Style ParseStyle(string str){
			using (var stream = new MemoryStream (Encoding.UTF8.GetBytes (str))) {
				using (r_ = new StreamReader (stream)) {
					currentStyle_ = new Style ();
					nextToken ();
					parseElementList ();
					expect (TokenType.EOF);
				}
			}
			return currentStyle_;
		}

		void parseStyleSheet(){
			while (tokenType_ != TokenType.EOF) {
				expect (TokenType.Identifier);
				var name = tokenString_;
				currentStyle_ = currentSheet_.FindOrCreateStyle (name);
				nextToken ();

				expect (TokenType.Symbol, "{");
				nextToken ();

				parseElementList ();

				expect (TokenType.Symbol, "}");
				nextToken ();

			}
		}

		void parseElementList(){
			while (tokenType_ == TokenType.Identifier) {
				parseElement ();
			}
		}

		int valueCount_;
		void parseElement(){
			expect (TokenType.Identifier);
			var key = tokenString_;
			nextToken ();

			expect (TokenType.Symbol, ":");
			nextToken ();

			bool finished = false;
			valueCount_ = 0;
			while (!finished) {
				switch (tokenType_) {
				case TokenType.Number:
				case TokenType.Identifier:
				case TokenType.String:
				case TokenType.Color:
					values_ [valueCount_] = tokenString_;
					valueCount_++;
					nextToken ();
					break;
				default:
					finished = true;
					break;
				}
			}

			if (valueCount_ == 0) {
				throw new Exception ("not value specified");
			}
			setField (key);

			expect (TokenType.Symbol, ";");
			nextToken ();
		}

		void setField(string key){
			switch (key) {
			case "margin":
				if (valueCount_ == 1) {
					currentStyle_.SetField ("margin-left", values_ [0]);
					currentStyle_.SetField ("margin-right", values_ [0]);
					currentStyle_.SetField ("margin-top", values_ [0]);
					currentStyle_.SetField ("margin-bottom", values_ [0]);
				} else if (valueCount_ == 2) {
					currentStyle_.SetField ("margin-top", values_ [0]);
					currentStyle_.SetField ("margin-bottom", values_ [0]);
					currentStyle_.SetField ("margin-left", values_ [1]);
					currentStyle_.SetField ("margin-right", values_ [1]);
				} else if (valueCount_ == 4) {
					currentStyle_.SetField ("margin-top", values_ [0]);
					currentStyle_.SetField ("margin-right", values_ [1]);
					currentStyle_.SetField ("margin-bottom", values_ [2]);
					currentStyle_.SetField ("margin-left", values_ [3]);
				} else {
					throw new Exception ("invalid value count");
				}
				break;

			case "padding":
				if (valueCount_ == 1) {
					currentStyle_.SetField ("padding-left", values_ [0]);
					currentStyle_.SetField ("padding-right", values_ [0]);
					currentStyle_.SetField ("padding-top", values_ [0]);
					currentStyle_.SetField ("padding-bottom", values_ [0]);
				} else if (valueCount_ == 2) {
					currentStyle_.SetField ("padding-top", values_ [0]);
					currentStyle_.SetField ("padding-bottom", values_ [0]);
					currentStyle_.SetField ("padding-left", values_ [1]);
					currentStyle_.SetField ("padding-right", values_ [1]);
				} else if (valueCount_ == 4) {
					currentStyle_.SetField ("padding-top", values_ [0]);
					currentStyle_.SetField ("padding-right", values_ [1]);
					currentStyle_.SetField ("padding-bottom", values_ [2]);
					currentStyle_.SetField ("padding-left", values_ [3]);
				} else {
					throw new Exception ("invalid value count");
				}
				break;
			default:
				if (valueCount_ == 1) {
					currentStyle_.SetField (key, values_ [0]);
				} else {
					throw new Exception ("too many values for " + key);
				}
				break;
			}
		}

		//=========================================
		// Lexser処理
		//=========================================

		// トークンの種類
		enum TokenType {
			EOF,
			Identifier,
			Number,
			String,
			Symbol,
			Color,
		}

		void expect(TokenType tt, string tokenString = null ){
			if (tokenType_ != tt) {
				throw new Exception ("expect token " + tt + " but " + tokenType_);
			}
			if (tokenString != null && tokenString != tokenString_) {
				throw new Exception ("expect token '" + tokenString + "' but '" + tokenString_ + "'");
			}
		}

		void nextToken(){
			// 空白文字を飛ばす
			var c = r_.Peek ();
			if (isSpaceChar (c)) {
				// 空白
				while (isSpaceChar (r_.Peek ())) {
					r_.Read ();
				}
				nextToken ();
			} else if (c == '/') {
				// コメント
				r_.Read ();
				if (r_.Peek () == '*') {
					// ブロックコメント
					r_.Read ();
					while (true) {
						if (r_.Peek () == '*') {
							r_.Read ();
							if (r_.Peek () == '/') {
								r_.Read ();
								break;
							}
						} else {
							r_.Read ();
						}
					}
					nextToken ();
				} else if (r_.Peek () == '/') {
					// 行コメント
					r_.Read ();
					while (r_.Peek () != '\n') {
						r_.Read ();
					}
					r_.Read ();
					nextToken ();
				} else {
					// '/'
					tokenString_ = ((char)r_.Read ()).ToString ();
					tokenType_ = TokenType.Symbol;
				}
			} else if (isNumericChar (c)) {
				// 数字
				while (isNumericChar (r_.Peek ()) || r_.Peek () == '.') {
					sb_.Append ((char)r_.Read ());
				}
				tokenType_ = TokenType.Number;
				tokenString_ = sb_.ToString ();
				sb_.Clear ();
			} else if (isIdentChar (c)) {
				// 識別子
				while (isIdentChar (r_.Peek ())) {
					sb_.Append ((char)r_.Read ());
				}
				tokenType_ = TokenType.Identifier;
				tokenString_ = sb_.ToString ();
				sb_.Clear ();
			} else if (isSymbolChar (c)) {
				// 記号
				switch (c) {
				case '#':
					r_.Read ();
					sb_.Append ((char)c);
					while (isHexChar(r_.Peek())){
						sb_.Append ((char)r_.Read ());
					}
					tokenString_ = sb_.ToString ();
					tokenType_ = TokenType.Color;
					sb_.Clear ();
					break;
				default:
					tokenString_ = ((char)r_.Read ()).ToString ();
					tokenType_ = TokenType.Symbol;
					break;
				}
			} else if (isQuoteChar (c)) {
				// 文字列
				r_.Read ();
				while (r_.Peek () != c) {
					sb_.Append ((char)r_.Read ());
				}
				tokenType_ = TokenType.String;
				tokenString_ = sb_.ToString ();
				sb_.Clear ();
				r_.Read ();
			} else if (c < 0) {
				tokenType_ = TokenType.EOF;
			} else {
				throw new Exception ("unexpected character '" + (char)c + "'");
			}
		}

		CharType charType(int c){
			if (c >= 0 && (c < 128)) {
				return CharTable [c];
			} else {
				return CharType.None;
			}
		}

		bool isIdentChar(int c){
			return c >= 0 && (c > 128 || (CharTable [c] & (CharType.Alphabet | CharType.Separator | CharType.Number)) != 0);
		}

		bool isNumericChar(int c){
			return (charType(c) & (CharType.Number)) != 0;
		}

		bool isHexChar(int c){
			return (charType(c) & (CharType.Hex)) != 0;
		}

		bool isSymbolChar(int c){
			return (charType(c) & (CharType.Symbol)) != 0;
		}

		bool isQuoteChar(int c){
			return (charType(c) & (CharType.Quote)) != 0;
		}

		bool isSpaceChar(int c){
			return (charType(c) & (CharType.Space)) != 0;
		}

		string build(){
			var s = sb_.ToString ();
			sb_.Clear ();
			return s;
		}
	}
}

