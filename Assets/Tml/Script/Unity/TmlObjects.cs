using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

using UIText = UnityEngine.UI.Text;

namespace Tml {


	public class ElementEventListener : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Element Element;
        public TmlView View;
        public RectTransform Obj;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!string.IsNullOrEmpty(Element.ActualTips()))
            {
                View.ShowTips(Element, Obj);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!string.IsNullOrEmpty(Element.ActualTips()))
            {
                View.HideTips();
            }
        }
    }


	public partial class Document : BlockElement
	{
		public void DestroyObject()
		{
			GameObject.Destroy(obj_.gameObject);
		}
	}

	public partial class Element {

		public class RedrawParam
		{
			public TmlView View;
			public Document Document;
			public RectTransform Container;
		}
		
		protected RectTransform obj_;
		protected Graphic widget_;
		TmlView view_;

		protected enum WidgetMode { None, Widget, Sprite, Text }
		protected WidgetMode widgetMode_ = WidgetMode.None;

		WidgetMode savedWidgetMode_ = WidgetMode.None;

		protected void updateWidgetMode(){
			if (savedWidgetMode_ == widgetMode_ ) return;
			savedWidgetMode_ = widgetMode_;

			switch (savedWidgetMode_) {
			case WidgetMode.None:
				widget_ = null;
				break;
			case WidgetMode.Widget:
				break;
			case WidgetMode.Sprite:
				widget_ = obj_.gameObject.AddComponent<Image> ();
				break;
			case WidgetMode.Text:
                widget_ = null;
                break;
			}
		}

		public virtual void RedrawAfter(RedrawParam p)
		{
			// リンクの設定を行う
            if (ActualHref() != null)
            {
                var button = obj_.gameObject.AddComponent<Button>();
                ColorBlock cb = button.colors;
                cb.highlightedColor = new Color(0.5f, 0.5f, 1.0f); // ハイライト
                button.colors = cb;
                button.onClick.AddListener(this.OnClick);
            }

			// TIPSの設定を行う
			if (!string.IsNullOrEmpty(ActualTips()))
            {
                var et = obj_.gameObject.AddComponent<ElementEventListener>();
                et.Element = this;
                et.Obj = obj_;
                et.View = p.View;
            }
		}

        public virtual void Redraw(RedrawParam p){
			view_ = p.View;

			var go = new GameObject ("" + Tag + ":" + Id);
			go.transform.SetParent (p.Container.transform, false);
			obj_ = go.GetComponent<RectTransform> ();
			if (obj_ == null) {
				obj_ = go.AddComponent<RectTransform> ();
                obj_.anchorMax = new Vector2(0, 1);
                obj_.anchorMin = new Vector2(0, 1);
                obj_.pivot = new Vector2(0, 1);
            }

            if (widgetMode_ == WidgetMode.None) {
				if (!string.IsNullOrEmpty (Style.BackgroundImage)) {
					widgetMode_ = WidgetMode.Sprite;
				}
			}

			updateWidgetMode ();

			if (widget_ != null) {
				if (widgetMode_ == WidgetMode.Sprite) {
					var sprite = (Image)widget_;
					p.View.GetSprite (Style.BackgroundImage).Done (spr => {
						sprite.sprite = spr;
						sprite.type = Image.Type.Sliced;
					});
				}
			}

			var containerBackup = p.Container;
			p.Container = obj_;
			for (int i = 0; i < Fragments.Count; i++) {
				Fragments [i].Redraw (p);
				Fragments[i].RedrawAfter(p);
			}
			p.Container = containerBackup;

            obj_.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, LayoutedWidth);
            obj_.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, LayoutedHeight);
			obj_.anchoredPosition = new Vector2(LayoutedX, -LayoutedY);
        }

		public void OnClick()
        {
            view_.OnClickElement(new TmlView.EventInfo() { Element = this.Parent, Fragment = this, Href = ActualHref() });
        }
    }

	// 画像
	public partial class Img : InlineBlockElement {
		public override void Redraw(RedrawParam p){
			widgetMode_ = WidgetMode.Sprite;
			base.Redraw (p);

			var sprite = (Image)widget_;
			p.View.GetSprite (Src).Done (spr => {
				sprite.sprite = spr;
				sprite.type = Image.Type.Sliced;
			});
		}
	}

	// テキスト
	public partial class TextFragment : Element {
		UIText label_;

		public static Color ColorFromString(string s)
		{
			Color color;
			if (ColorUtility.TryParseHtmlString(s, out color))
			{
				return color;
			}
			else
			{
				Logger.LogError("Can't parse color" + s);
				return Color.white;
			}
		}

		public override void Redraw(RedrawParam p){
			widgetMode_ = WidgetMode.Text;

			base.Redraw (p);

            label_ = obj_.gameObject.AddComponent<UIText> ();
			var text = Value;
			if (Style.TextDecoration == "underline") {
				//text = "<[u]" + text;
			}
			label_.text = text;
			label_.fontSize = ActualFontSize();
			label_.alignment = TextAnchor.LowerLeft;
			label_.font = p.View.DefaultFont;
			label_.verticalOverflow = VerticalWrapMode.Overflow;
			label_.horizontalOverflow = HorizontalWrapMode.Overflow;
			var color = ActualColor();
			if (!string.IsNullOrEmpty(color))
            {
				label_.color = ColorFromString(color);
            }

        }

	}

}
