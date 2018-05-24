using System;
using System.Collections.Generic;
using System.Linq;

namespace Tml
{
	/// <summary>
    /// レイアウトを行う
	/// 
	/// 
	/// ブロック要素の場合、縦にエレメントを配置し、Widthを100%にする。
	/// 
	/// インライン要素の場合、横にエレメントを配置し、右に達したら折り返す。
    /// </summary>
	public struct Layouter {

        /// <summary>
        /// レンダリングされた文字列の幅の情報
        /// </summary>
		public struct CharInfo {
			public int CharacterCount;
			public int TextWidth;
		}

        public static CharInfo DefaultGetCharacterCountCallback(Element e, string text, int startPos, int fontSize, int width)
        {
            var c = width / fontSize;
            if (c >= text.Length - startPos) c = text.Length - startPos;
            return new CharInfo { CharacterCount = c, TextWidth = c * fontSize };
        }

#if !NET_4_6
        public delegate CharInfo GetCharacterCountCallbackType(Element e,string text,int startPos,int fontSize,int width);
		public static GetCharacterCountCallbackType GetCharacterCountCallback = DefaultGetCharacterCountCallback;
#else
		public static Func<Element,string,int,int,int,CharInfo> GetCharacterCountCallback = DefaultGetCharacterCountCallback;
#endif

        public Element Target { get; private set; }

		int currentY_;
		int currentX_;
		LayoutType mode_;

        /// <summary>
        /// Fragmentsの中で、行の始まりからのインデックス
        /// </summary>
		int lineStartIndex_;

		public Layouter(Element target){
			Target = target;
			currentX_ = 0;
			currentY_ = target.Style.PaddingTop;
			mode_ = LayoutType.Block;
			lineStartIndex_ = 0;
		}

		/// <summary>
		/// レイアウト情報を計算し直す
		/// </summary>
		public void Reflow()
		{
			Target.Fragments.Clear ();

			foreach( var e in Target.Children)
			{
				reflowElement (e);
			}

			// インラインモードなら、最後に後始末する
			if (mode_ == LayoutType.Inline) {
				newlineInline ();
			}

			Target.LayoutedHeight = currentY_ + Target.Style.PaddingBottom;
			if (Target.LayoutedHeight < Target.Height) {
				Target.LayoutedHeight = Target.Height;
			}
		}

        /// <summary>
		/// モードを変更する
		/// 
		/// モードが切り替わった際には、インラインのレイアウト情報のリセットを行う。
		/// つまり、ブロックエレメントの次にインラインエレメントを配置する場合は、次の行の左端からはじまる。
        /// </summary>
        /// <param name="newMode">New mode.</param>
		void setMode(LayoutType newMode){
			if (mode_ != newMode) {
				if (mode_ == LayoutType.Inline) {
					// インラインレイアウトの終了
					newlineInline();
				} else {
					// ブロックレイアウトの終了
					resetInline();
				}
			}
			mode_ = newMode;
		}

		void reflowElement(Element e){
			switch (e.LayoutType) {
			case LayoutType.Block:
				setMode (LayoutType.Block);
				break;
			case LayoutType.Inline:
			case LayoutType.InlineBlock:
			case LayoutType.Text:
				setMode (LayoutType.Inline);
				break;
			}

			switch (e.LayoutType) {
			case LayoutType.Block:
				reflowBlock (e);
				break;
			case LayoutType.Inline:
				reflowInline (e);
				break;
			case LayoutType.InlineBlock:
				reflowInlineBlock (e);
				break;
			case LayoutType.Text:
				reflowText (e);
				break;
			}
		}

        /// <summary>
		/// ブロック要素を配置する
        /// </summary>
        /// <param name="e">E.</param>
		void reflowBlock(Element e){
			// ブロックレイアウトの場合
			// まず、幅を決定してから、高さを計算する
			e.LayoutedWidth = Target.LayoutedInnerWidth - e.Style.MarginLeft - e.Style.MarginBottom;

			new Layouter (e).Reflow();

			e.CalculateBlockHeight ();
			e.LayoutedY = currentY_ + e.Style.MarginTop;
			e.LayoutedX = Target.Style.PaddingLeft + e.Style.MarginLeft;
			currentY_ += e.LayoutedHeight + e.Style.MarginTop + e.Style.MarginBottom;

			Target.Fragments.Add (e);
		}

        /// <summary>
        /// インライン要素を配置する
        /// </summary>
        /// <param name="e">E.</param>
		void reflowInline(Element e){
			for (int i = 0; i < e.Children.Count; i++) {
				reflowElement (e.Children [i]);
			}
		}

		void reflowInlineBlock(Element e){
			e.LayoutedWidth = e.Width;
			new Layouter (e).Reflow();

			e.CalculateBlockHeight ();

			addInlineFragment (e);
		}

        /// <summary>
		/// TextFragment用の空のスタイル
		/// 
        /// 共有しているので、変更しないように注意！
        /// </summary>
		static Style emptyStyle_ = Style.Empty().Seal();

        /// <summary>
		/// テキスト要素を配置する
        /// </summary>
		void reflowText(Element e){
			var text = (Text)e;
			// テキストの場合
			var str = text.Value;
			int cur = 0;
			var fontSize = text.Parent.ActualFontSize ();
			while (true) {
				var charInfo = GetCharacterCountCallback (text, str, cur, fontSize, Target.LayoutedInnerWidth - currentX_);
				var n = charInfo.CharacterCount;
				if (n == 0) {
					if (currentX_ == 0) {
						// １文字も入らない幅の時の特別処理
						n = 1;
						charInfo.TextWidth = (int)(fontSize);
					} else {
						newlineInline ();
						continue;
					}
				}
				if (cur + n >= str.Length) n = str.Length - cur;
				var fragment = new TextFragment ();
				fragment.Parent = e;
				fragment.Tag = "text";
				fragment.Style = emptyStyle_;
				fragment.Value = str.Substring (cur, n);
				fragment.CalculateBlockHeight ();
				fragment.LayoutedWidth = charInfo.TextWidth;
				addInlineFragment (fragment);
				cur += n;
				if (cur >= str.Length) {
					break;
				}
			}
		}

		void addInlineFragment(Element e){
			currentX_ += e.LayoutedWidth;
			if (currentX_ > Target.LayoutedInnerWidth) {
				newlineInline ();
				currentX_ += e.LayoutedWidth;
				Target.Fragments.Add (e);
			} else {
				Target.Fragments.Add (e);
			}
		}

		void resetInline(){
			lineStartIndex_ = Target.Fragments.Count;
			currentX_ = 0;
		}

		void newlineInline(){
			var x = Target.Style.PaddingLeft;
			var lineHeight = 0;
			var len = Target.Fragments.Count;
			for (int i = lineStartIndex_; i < len; i++) {
				var e = Target.Fragments [i];
				if (e.LayoutedHeight > lineHeight) {
					lineHeight = e.LayoutedHeight;
				}
			}

			var gap = 0;
			if (Target.Style.TextAlign == "center") {
				gap = (Target.LayoutedWidth - currentX_) / 2;
			} else if (Target.Style.TextAlign == "right") {
				gap = Target.LayoutedWidth - currentX_;
			}
				
			for (int i = lineStartIndex_; i < Target.Fragments.Count; i++) {
				var e = Target.Fragments [i];
				e.LayoutedY = currentY_ + lineHeight - e.LayoutedHeight;
				e.LayoutedX = x + gap;
				x += e.LayoutedWidth;
			}
			
			lineStartIndex_ = Target.Fragments.Count;
			currentX_ = 0;
			currentY_ += lineHeight;
		}

	}
}

