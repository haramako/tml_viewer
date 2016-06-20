using UnityEngine;
using System.Collections;

namespace Tml {

	public partial class Element {

		public class RedrawParam
		{
			public TmlView View;
			public Document Document;
			public int Depth;
			public GameObject Container;
		}
		
		public GameObject obj_;
		public UIWidget widget_;

		protected enum WidgetMode { None, Widget, Sprite }
		protected WidgetMode widgetMode_ = WidgetMode.None;

		WidgetMode savedWidgetMode_ = WidgetMode.None;

		protected void updateWidgetMode(){
			if (savedWidgetMode_ == widgetMode_) return;
			savedWidgetMode_ = widgetMode_;

			GameObject.DestroyImmediate (widget_);

			switch (savedWidgetMode_) {
			case WidgetMode.None:
				widget_ = null;
				break;
			case WidgetMode.Widget:
				widget_ = obj_.AddComponent<UIWidget> ();
				break;
			case WidgetMode.Sprite:
				widget_ = obj_.AddComponent<UISprite> ();
				break;
			}
		}

		public virtual void Redraw(RedrawParam p){
			obj_ = new GameObject ("" + Tag + ":" + Id);
			obj_.transform.SetParent (p.Container.transform, false);

			if (widgetMode_ == WidgetMode.None) {
				if (!string.IsNullOrEmpty (Style.BackgroundImage)) {
					widgetMode_ = WidgetMode.Sprite;
				}
			}

			updateWidgetMode ();

			if (widget_ != null) {
				widget_.pivot = UIWidget.Pivot.TopLeft;
				widget_.width = LayoutedWidth;
				widget_.height = LayoutedHeight;
				widget_.depth = p.Depth;
				if (widgetMode_ == WidgetMode.Sprite) {
					var sprite = (UISprite)widget_;
					sprite.atlas = p.View.GetSpriteAtlas (Style.BackgroundImage);
					sprite.spriteName = Style.BackgroundImage;
					sprite.type = UIBasicSprite.Type.Sliced;
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

			var sprite = (UISprite)widget_;
			sprite.atlas = p.View.GetSpriteAtlas (Src);
			sprite.spriteName = Src;
			sprite.type = UIBasicSprite.Type.Simple;
		}
	}

	// テキスト
	public partial class TextFragment : Element {
		UILabel label_;

		public override void Redraw(RedrawParam p){
			base.Redraw (p);

			var labelObj = new GameObject("TmlText");
			labelObj.transform.SetParent (obj_.transform,false);
			label_ = labelObj.AddComponent<UILabel> ();
			//label_.pivot = UIWidget.Pivot.TopLeft;
			var text = Value;
			if (StyleElement.Style.TextDecoration == "underline") {
				text = "[u]" + text;
			}
			if (!string.IsNullOrEmpty (StyleElement.Style.Color)) {
				text = "[" + StyleElement.Style.Color.Substring(1) + "]" + text;
			}
			label_.text = text;
			label_.fontSize = StyleElement.ActualFontSize();
			label_.alignment = NGUIText.Alignment.Left;
			label_.depth = p.Depth + 1;
			label_.SetRect (0, -LayoutedHeight, LayoutedWidth * 1.1f /* 幅が足りないことがあるので */, LayoutedHeight);
			label_.pivot = UIWidget.Pivot.BottomLeft;

			if (StyleElement.Tag == "a") {
				labelObj.AddComponent<BoxCollider> ();
				var button = labelObj.AddComponent<UIButton> ();
				button.onClick.Add (new EventDelegate(() => {
					p.View.OnClickElement(new TmlView.EventInfo(){ Element = this.Parent, Fragment = this, Href=((A)StyleElement).Href });
				}));
				label_.ResizeCollider ();
			}
		}
	}

}
