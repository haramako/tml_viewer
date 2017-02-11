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
				((Image)widget_).color = Color.black;
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
			obj_.localPosition = Vector3.zero;

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
					sprite.sprite = p.View.GetSpriteAtlas (Style.BackgroundImage);
					sprite.type = Image.Type.Sliced;
				}
			}

			obj_.transform.localPosition = new Vector3 (LayoutedX, -LayoutedY);

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
			sprite.sprite = p.View.GetSpriteAtlas (Src);
			sprite.type = Image.Type.Sliced;
		}
	}

	// テキスト
	public partial class TextFragment : Element {
		UIText label_;

		public override void Redraw(RedrawParam p){
			base.Redraw (p);

			var labelObj = new GameObject("TmlText");
			labelObj.transform.SetParent (obj_, false);
			label_ = labelObj.AddComponent<UIText> ();
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
			label_.alignment = TextAnchor.UpperLeft;
			label_.font = p.View.DefaultFont;
			//label_.depth = p.Depth + 1;
			var rt = labelObj.GetComponent<RectTransform>();
			rt.pivot = new Vector2 (0, 1);
			rt.SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, LayoutedHeight);
			rt.SetSizeWithCurrentAnchors (RectTransform.Axis.Horizontal, LayoutedWidth * 1.1f);
			//label_.SetRect (0, -LayoutedHeight, LayoutedWidth * 1.1f /* 幅が足りないことがあるので */, LayoutedHeight);

			if (StyleElement.Tag == "a") {
				var button = labelObj.AddComponent<Button> ();
				button.onClick.AddListener (this.OnClick);
				view_ = p.View;
			}
		}

		TmlView view_;

		public void OnClick(){
			view_.OnClickElement(new TmlView.EventInfo(){ Element = this.Parent, Fragment = this, Href=((A)StyleElement).Href });
		}
	}

}
