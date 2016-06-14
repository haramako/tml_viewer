using UnityEngine;
using System.Collections;

public class TmlView : MonoBehaviour {
	public class EventInfo
	{
		public Tml.Element Element;
		public string Href;
	}
	public UIWidget Container;

	public Tml.Document Document;
	public UIAtlas[] Atlases;

	public EventInfo ActiveEvent { get; private set; }

	public UnityEngine.Events.UnityEvent OnEvent;

	Tml.Element.RedrawParam redrawParam_ = new Tml.Element.RedrawParam();

	string source_;
	public string Source { get { return source_; } set { setSource (value); } }

	public void Awake(){
	}

	void setSource(string val){
		if (Document != null) {
			if (Document.obj_ != null) {
				GameObject.DestroyImmediate (Document.obj_);
			}
		}

		source_ = val;
		Document = Tml.Parser.Default.Parse (source_);
		var w = Container.GetComponent<UIWidget> ();
		Document.Width = Document.LayoutedWidth = w.width;
		Document.Height = Document.LayoutedHeight = w.height;
		new Tml.Layouter (Document).Reflow ();

		redrawParam_.Container = Container.gameObject;
		redrawParam_.Document = Document;
		redrawParam_.View = this;
		redrawParam_.Depth = 100;

		Document.Redraw (redrawParam_);
	}

	public UIAtlas GetSpriteAtlas(string spriteName){
		for (int i = 0; i < Atlases.Length; i++) {
			var sprite = Atlases [i].GetSprite (spriteName);
			if (sprite != null) {
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
