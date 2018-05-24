using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using RSG;
using System;

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
	public Sprite[] DefaultSprites;

	public EventInfo ActiveEvent { get; private set; }

	public UnityEngine.Events.UnityEvent OnEvent;

	Canvas cachedCanvas_;

	public string Source { get; private set; }
    public string DefaultSource { get; private set; }

	RectTransform root_;

    public Uri BaseUrl;
	Dictionary<string,Sprite> sprites_ = new Dictionary<string,Sprite>();

	public Tml.Layouter.CharInfo GetCharacterCount(Tml.Element e, string text, int startPos, int fontSize, int width){
		var font = DefaultFont;

		// MEMO: UnityUIのTextが使うフォントサイズは、計算しないと求められないため
		var actualFontSize = (int)(fontSize * cachedCanvas_.scaleFactor);

		int prev = 0;
		float rest = width;
		float use = 0;
		int i;
		CharacterInfo ci;

		font.RequestCharactersInTexture (text, actualFontSize);
		for (i = startPos; i < text.Length; i++) {
			if (font.GetCharacterInfo (text [i], out ci, actualFontSize)) {
				if (use + ci.advance >= rest ) {
					return new Tml.Layouter.CharInfo{ CharacterCount = i - startPos, TextWidth = (int)use };
				}
				use += (float)ci.advance * fontSize / actualFontSize;
				prev = text [i];
			}
		}

		return new Tml.Layouter.CharInfo{ CharacterCount = text.Length - startPos, TextWidth = (int)use };
	}

	public void Awake(){
		cachedCanvas_ = gameObject.GetComponentInParent<Canvas> ();
		foreach (var sprite in DefaultSprites) {
			sprites_ [sprite.name] = sprite;
		}
	}

	public void SetSource(string src, string defaultSource)
    {
		if (Document != null) {
			Document.DestroyObject();
		}

        DefaultSource = defaultSource;
		Source = src;
        Document = RenderToContainer(this, Container, src, DefaultSource);
	}

    public static Tml.Document RenderToContainer(TmlView view, RectTransform container, string src, string defaultSrc)
    {
        Tml.Document doc;
        try
        {
            doc = Tml.Parser.Default.Parse(defaultSrc + src);
        }
        catch (Tml.ParserException ex)
        {
            Tml.Logger.LogException(ex);
            return null;
        }
        doc.Width = doc.LayoutedWidth = (int)container.rect.width;
        doc.Height = doc.LayoutedHeight = 0;

        new Tml.Layouter(doc).Reflow();

        var param = new Tml.Element.RedrawParam();
        param.Container = container;
        param.Document = doc;
        param.View = view;

        doc.Redraw(param);
		doc.RedrawAfter(param);

        container.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, doc.LayoutedWidth);
        container.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, doc.LayoutedHeight);

		return doc;
    }

    public Sprite GetSpriteAtlas(string spriteName){
		for (int i = 0; i < DefaultSprites.Length; i++) {
			if (DefaultSprites [i].name == spriteName) {
				return DefaultSprites [i];
			}
		}
		return null;
	}

	public IPromise<Sprite> GetSprite(string spriteName){
		Sprite result;
		if( sprites_.TryGetValue(spriteName, out result)){
			return Promise<Sprite>.Resolved(result);
		}else{
			var url = new Uri (BaseUrl, spriteName).ToString ();
			return PromiseEx.StartWWW(url).Then(www=>{
				var tex = www.texture;
				if( tex != null ){
					var sprite = Sprite.Create(tex, new Rect(0,0,tex.width,tex.height), new Vector2(tex.width/2,tex.height/2), 100, 0, SpriteMeshType.FullRect);
					sprites_[spriteName] = sprite;
					return sprite;
				}else{
					return null;
				}
			});
		}
	}

	public void OnClickElement(EventInfo ev){
		ActiveEvent = ev;
		OnEvent.Invoke ();
	}

    Tips tips_ = new Tips();

    public void ShowTips(Tml.Element e, RectTransform obj)
    {
		tips_.Show(this, e, obj);
    }

    public void HideTips()
    {
		tips_.Hide();
    }

}
