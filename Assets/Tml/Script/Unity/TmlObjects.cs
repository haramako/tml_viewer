using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using UIText = UnityEngine.UI.Text;

namespace Tml {

	public partial class Element {

		public class RedrawParam
		{
			public TmlView View;
			public Document Document;
			public int Depth;
			public RectTransform Container;
		}
		
		public RectTransform obj_;
		public Graphic widget_;

		protected enum WidgetMode { None, Widget, Sprite }
		protected WidgetMode widgetMode_ = WidgetMode.None;

		WidgetMode savedWidgetMode_ = WidgetMode.None;

		protected void updateWidgetMode(){
			if (savedWidgetMode_ == widgetMode_) return;
			savedWidgetMode_ = widgetMode_;

			Object.DestroyImmediate (widget_);

			switch (savedWidgetMode_) {
			case WidgetMode.None:
				widget_ = null;
				break;
			case WidgetMode.Widget:
				break;
			case WidgetMode.Sprite:
				widget_ = obj_.gameObject.AddComponent<Image> ();
				break;
			}
		}

		public virtual void Redraw(RedrawParam p){
			var go = new GameObject ("" + Tag + ":" + Id);
			go.transform.SetParent (p.Container.transform, false);
			obj_ = go.GetComponent<RectTransform> ();
			if (obj_ == null) {
				obj_ = go.AddComponent<RectTransform> ();
			}

			if (widgetMode_ == WidgetMode.None) {
				if (!string.IsNullOrEmpty (Style.BackgroundImage)) {
					widgetMode_ = WidgetMode.Sprite;
				}
			}

			updateWidgetMode ();

			if (widget_ != null) {
				obj_.anchorMax = new Vector2 (0, 1);
				obj_.anchorMin = new Vector2 (0, 1);
				obj_.pivot = new Vector2 (0, 1);
				obj_.SetSizeWithCurrentAnchors (RectTransform.Axis.Horizontal, LayoutedWidth);
				obj_.SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, LayoutedHeight);
				//obj_.depth = p.Depth;
				if (widgetMode_ == WidgetMode.Sprite) {
					var sprite = (Image)widget_;
					p.View.GetSprite (Style.BackgroundImage).Done (spr => {
						sprite.sprite = spr;
						sprite.type = Image.Type.Sliced;
					});
				}
			}

			obj_.localPosition = new Vector3 (LayoutedX, -LayoutedY);

			var containerBackup = p.Container;
			p.Container = obj_;
			p.Depth += 10;
			for (int i = 0; i < Fragments.Count; i++) {
				Fragments [i].Redraw (p);
			}
			p.Depth -= 10;
			p.Container = containerBackup;
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

		public override void Redraw(RedrawParam p){
			base.Redraw (p);

			label_ = obj_.gameObject.AddComponent<UIText> ();
			//label_.pivot = UIWidget.Pivot.TopLeft;
			var text = Value;
			if (StyleElement.Style.TextDecoration == "underline") {
				//text = "<[u]" + text;
			}
			if (!string.IsNullOrEmpty (StyleElement.Style.Color)) {
				text = "<color=#" + StyleElement.Style.Color.Substring(1) + ">" + text + "</color>";
			}
			label_.text = text;
			label_.fontSize = StyleElement.ActualFontSize();
			label_.alignment = TextAnchor.LowerLeft;
			label_.font = p.View.DefaultFont;
			label_.verticalOverflow = VerticalWrapMode.Overflow;
			//label_.depth = p.Depth + 1;
			var rt = obj_.GetComponent<RectTransform>();
			rt.pivot = new Vector2 (0, 1);
			rt.SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, LayoutedHeight);
			rt.SetSizeWithCurrentAnchors (RectTransform.Axis.Horizontal, LayoutedWidth * 1.1f);
			//label_.SetRect (0, -LayoutedHeight, LayoutedWidth * 1.1f /* 幅が足りないことがあるので */, LayoutedHeight);

			if (StyleElement.Tag == "a") {
				var button = obj_.gameObject.AddComponent<Button> ();
                ColorBlock cb = button.colors;
                cb.highlightedColor = new Color(0.5f, 0.5f, 1.0f); // ハイライト
                button.colors = cb;
				button.onClick.AddListener (this.OnClick);
				view_ = p.View;
			}
            obj_.localPosition = new Vector3(LayoutedX, -LayoutedY);
        }

		TmlView view_;

		public void OnClick(){
			view_.OnClickElement(new TmlView.EventInfo(){ Element = this.Parent, Fragment = this, Href=((A)StyleElement).Href });
		}
	}

}
