using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TmlView : MonoBehaviour {
	public class EventInfo
	{
		public Tml.Element Element;
		public Tml.Element Fragment;
		public string Href;
	}
	public RectTransform Container;
	public Font DefaultFont;

	public Tml.Document Document;
	public Sprite[] Atlases;

	public EventInfo ActiveEvent { get; private set; }

	public UnityEngine.Events.UnityEvent OnEvent;

	Tml.Element.RedrawParam redrawParam_ = new Tml.Element.RedrawParam();

	string source_;
	public string Source { get { return source_; } set { setSource (value); } }

	public void Awake(){
	}

	void setSource(string val){
		if (Document != null) {
			GameObject.DestroyImmediate (Document.obj_.gameObject);
		}

		source_ = val;
		try {
			Document = Tml.Parser.Default.Parse (source_);
		}catch(Tml.ParserException ex){
			Tml.Logger.LogException (ex);
			return;
		}
		Document.Width = Document.LayoutedWidth = (int)Container.rect.width;
		Document.Height = Document.LayoutedHeight = 0;

		new Tml.Layouter (Document).Reflow ();

		redrawParam_.Container = Container;
		redrawParam_.Document = Document;
		redrawParam_.View = this;
		redrawParam_.Depth = 100;

		Document.Redraw (redrawParam_);

		Container.SetSizeWithCurrentAnchors (RectTransform.Axis.Horizontal, Document.LayoutedWidth);
		Container.SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, Document.LayoutedHeight);
	}

	public Sprite GetSpriteAtlas(string spriteName){
		for (int i = 0; i < Atlases.Length; i++) {
			if (Atlases [i].name == spriteName) {
				return Atlases [i];
			}
		}
		return null;
	}

	public void OnClickElement(EventInfo ev){
		ActiveEvent = ev;
		OnEvent.Invoke ();
	}

}
