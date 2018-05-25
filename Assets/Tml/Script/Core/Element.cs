#if UNITY
using UnityEngine;
#endif
using System;
using System.Collections.Generic;
#if USE_SYSTEM_XML
using System.Xml;
#else
using Tml.XmlPolyfill;
#endif
using System.IO;
using System.Text;
using System.Reflection;
using System.Linq;

namespace Tml
{

    public partial class Document: BlockElement
    {
		public StyleSheet StyleSheet = new StyleSheet();
    }

    public enum LayoutType
    {
        Box,
        Block,
        Inline,
		InlineBlock,
        Text,
    }

    public enum PositionType
    {
        Absolute,
        Relative,
    }

    public partial class Element
    {
		static readonly char[] ClassSeprators = new char[]{' ', '\t', '\n'};

		public string Tag;
        public string Id;
        public string Class;
		public string[] Classes = new string[0];
        public string Tips;
		public string Href;
        public int[] Cols;

        public Element Parent;
        public List<Element> Children = new List<Element>();
		public List<Element> Fragments = new List<Element>();

		public LayoutType LayoutType = LayoutType.Block;
		public Style Style;
        public Style ImmediateStyle { get { return (immediateStyle_ != null ? immediateStyle_ : Style.SharedEmpty); } }

        // レイアウト済み情報（内部で使用）
        public int LayoutedX;
        public int LayoutedY;
        public int LayoutedWidth;
        public int LayoutedHeight;
        public int LayoutedInnerX { get { return LayoutedX + Style.PaddingLeft; } }
        public int LayoutedInnerY { get { return LayoutedY + Style.PaddingTop; } }
        public int LayoutedInnerWidth { get { return LayoutedWidth - Style.PaddingLeft - Style.PaddingRight; } }
        public int LayoutedInnerHeight { get { return LayoutedHeight - Style.PaddingTop - Style.PaddingBottom; } }


        Style immediateStyle_;

        public Element()
        {
        }

        //==========================================================
        // パース
        //==========================================================
        public virtual void Parse(Parser loader, XmlReader reader)
        {
            if (reader.HasAttributes)
            {
                for (int i = 0; i < reader.AttributeCount; i++)
                {
                    reader.MoveToAttribute(i);
                    ParseAttribute(reader.Name, reader.Value);
                }
                reader.MoveToElement();
            }
        }

        public virtual void ParseAttribute(string name, string value)
        {
            switch (name)
            {
                case "id":
                    Id = value;
                    break;
				case "class":
					SetClass (value);
                    break;
				case "href":
                    Href = value;
                    break;
                case "tips":
                    Tips = value;
                    break;
                case "cols":
                    Cols = value.Split(',').Select(e=>int.Parse(e)).ToArray();
                    break;
                default:
                    GetEditableImmediateStyle().SetField(name, value);
                    //Logger.Log("unknown attribute " + name + "=" + value);
                    break;
            }
        }

		public void SetClass(string value){
			Class = value;
			Classes = (Tag+" "+value).Trim ().Split (ClassSeprators, StringSplitOptions.RemoveEmptyEntries);
		}

        public Style GetEditableImmediateStyle()
        {
            if (immediateStyle_ == null)
            {
                immediateStyle_ = Style.Empty();
            }
            return immediateStyle_;
        }

        //==========================================================
        // スタイル適用
        //==========================================================

        public void ApplyStyle(StyleSheet styleSheet){
            Style = styleSheet.GetStyle(Classes);
            Style.Merge(ImmediateStyle);
			for (int i = 0; i < Children.Count; i++) {
				Children [i].ApplyStyle (styleSheet);
			}
		}

        //==========================================================
        // レイアウトの計算
        //==========================================================

		public virtual IEnumerable<Element> InlineElements(){
			return Children.SelectMany (c => c.InlineElements ());
		}

        public virtual void CalculateBlockHeight()
        {
			
        }

		public string ActualColor()
        {
            if (Style.Color != "")
            {
                return Style.Color;
            }
            else if (Parent != null)
            {
				return Parent.ActualColor();
            }
            else
            {
				return "";
            }
        }

		public int ActualFontSize(){
			if (Style.FontSize != Style.Inherit) {
				return Style.FontSize;
			} else if (Parent != null) {
				return (int)(Parent.ActualFontSize () * Style.FontScale);
			} else {
				return Style.DefaultFontSize;
			}
		}

		public float ActualLineScale(){
			if (Style.LineScale != Style.InheritFloat) {
				return Style.LineScale;
			} else if (Parent != null) {
				return Parent.ActualLineScale ();
			} else {
				return Style.DefaultLineScale;
			}
		}

		public int ActualLineHeight(){
			if (Style.LineHeight != Style.Inherit) {
				return Style.LineHeight;
			} else if (Parent != null) {
				return Parent.ActualLineHeight ();
			} else {
				return Style.Nothing; //Style.DefaultLineHeight;
			}
		}

		public int ActualComputedLineHeight(int fontSize){
			if (Style.LineHeight != Style.Inherit) {
				return Style.LineHeight;
			} else if( Style.LineScale != Style.InheritFloat ){
				return (int)(fontSize * Style.LineScale);
			} else if (Parent != null) {
				return Parent.ActualComputedLineHeight (fontSize);
			} else {
				return (int)(Style.DefaultFontSize * Style.DefaultLineScale);
			}
		}

		public string ActualHref()
		{
			if (Href != null)
			{
				return Href;
			}
			else if (Parent != null)
			{
				return Parent.ActualHref();
			}
			else
			{
				return null;
			}
        }

		public string ActualTips()
        {
            if (Tips != null)
            {
                return Tips;
            }
            else if (Parent != null)
            {
                return Parent.ActualTips();
            }
            else
            {
                return null;
            }
        }

        //==========================================================
        // デバッグ用出力
        //==========================================================
        public string Dump()
        {
            var buf = new StringBuilder();
            DumpToBuf(buf, 0);
            return buf.ToString();
        }

        public virtual void DumpToBuf(StringBuilder buf, int level)
        {
			buf.Append (' ', level * 2);
            buf.Append(this.GetType().ToString());
            buf.Append("\n");
            foreach (var e in Children)
            {
                e.DumpToBuf(buf, level + 1);
            }
        }

		public string DumpToHtml(){
			var buf = new StringBuilder ();
			buf.Append ("<html><body><style>.def{ position: absolute; } .inner{ position: relative; }</style>\n");
			DumpToHtmlBuf (buf, 0);
			buf.Append ("</body></html>\n");
			return buf.ToString ();
		}

		public virtual void DumpToHtmlBuf(StringBuilder buf, int level)
		{
			buf.Append(' ', level * 2);
			string bgStyle = null;
			if (!string.IsNullOrEmpty(Style.BackgroundImage)) {
				bgStyle = string.Format ("background-image: url({0}.png); background-size: 100% 100%;", Style.BackgroundImage);
			}
			buf.Append (string.Format (
				"<div id='{0}' class='def' style='outline: solid gray 1px; left:{1}px; top:{2}px; width:{3}px; height:{4}px; {5}; background-color: {6};'><div class='inner'>\n",
				Id,
				LayoutedX,
				LayoutedY,
				LayoutedWidth,
				LayoutedHeight,
				bgStyle,
				Style.BackgroundColor
			));
			foreach (var e in Fragments)
			{
				e.DumpToHtmlBuf(buf, level + 1);
			}
			buf.Append(' ', level * 2);
			buf.Append("</div></div>\n");
		}

		public Element FindById(string id){
			if (this.Id == id) {
				return this;
			} else {
				for (int i = 0; i < Children.Count; i++) {
					var found = Children [i].FindById (id);
					if (found != null) {
						return found;
					}
				}
				return null;
			}
		}

    }

    public partial class BoxElement : Element
    {
        public BoxElement() : base()
        {
            LayoutType = LayoutType.Box;
        }
    }

    public partial class BlockElement : Element
    {
		public BlockElement(): base(){
			LayoutType = LayoutType.Block;
		}
    }

	public partial class InlineElement : Element 
	{
		public InlineElement(): base(){
			LayoutType = LayoutType.Inline;
		}
	}

	public partial class InlineBlockElement : Element
	{
		public InlineBlockElement(): base(){
			LayoutType = LayoutType.InlineBlock;
		}
	}

	public partial class A : InlineElement
	{
	}

	public partial class Img : InlineBlockElement {
		public string Src;

		public override void ParseAttribute(string name, string value) {
			switch (name) {
			case "src":
				Src = value;
				break;
			default:
				base.ParseAttribute (name, value);
				break;
			}
		}

		public override void DumpToHtmlBuf(StringBuilder buf, int level)
		{
			buf.Append(' ', level * 2);
			buf.Append (string.Format (
				"<img id='{0}' class='def' style='outline: solid gray 1px; left:{1}px; top:{2}px; width:{3}px; height:{4}px; background-color: {6}' src='{5}'>\n",
				Id,
				LayoutedX,
				LayoutedY,
				LayoutedWidth,
				LayoutedHeight,
				Src,
				Style.BackgroundColor
			));
			buf.Append(' ', level * 2);
			buf.Append("</div>\n");
		}
	}

    public partial class Text : Element
    {
        public string Value;

        public Text() : base()
        {
			LayoutType = LayoutType.Text;
        }

		public override void DumpToBuf(StringBuilder buf, int level)
		{
			for (int i = 0; i < level * 2; i++) buf.Append(" ");
			buf.Append(Value);
			buf.Append("\n");
		}

    }

	// 行末での分解後の文字列
	public partial class TextFragment : Element
	{
		public string Value;

		public TextFragment() : base()
		{
			LayoutType = LayoutType.Text;
		}

		public override void CalculateBlockHeight()
		{
			var fontSize = ActualFontSize();
			var lineHeight = ActualComputedLineHeight (fontSize);
			if (lineHeight == Style.Nothing) {
				LayoutedHeight = (int)(fontSize * 1.5f);
			} else {
				LayoutedHeight = lineHeight;
			}
		}

		public override void DumpToHtmlBuf(StringBuilder buf, int level)
		{
			buf.Append(' ', level * 2);
			buf.Append (string.Format (
				"<div id='{0}' class='def' style='outline: solid gray 1px; left:{1}px; top:{2}px; width:{3}px; height:{4}px; font-size: {5}px; background-color: {6}'><div class='inner'>\n",
				Id,
				LayoutedX,
				LayoutedY,
				LayoutedWidth,
				LayoutedHeight,
				ActualFontSize(),
				Style.BackgroundColor
			));
			if (Tag == "a") {
				//buf.Append (string.Format ("<a href='{0}.html'>", ((A)StyleElement).Href));
			}
			buf.Append (Value);
			if (Tag == "a") {
				buf.Append ("</a>");
			}
			buf.Append(' ', level * 2);
			buf.Append("</div></div>\n");
		}

	}
}