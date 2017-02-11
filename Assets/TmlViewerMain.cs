using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class TmlViewerMain : MonoBehaviour, ILogHandler {
	public Text UrlLabel;
	public TmlView View;
	public string Url;
	public ScrollRect ScrollView;
	public GameObject ClickEffect;
	public RectTransform TopPanel;

	public Image UrlInputPanel;
	public InputField UrlInputField;

	List<string> history_ = new List<string>();

	static Tml.Layouter.CharInfo getCharacterCount(Tml.Element e, string text, int startPos, int fontSize, int width){
		/*
		NGUIText.dynamicFont = UILabel.GetDefaultFont ();
		NGUIText.fontSize = fontSize;
		NGUIText.finalSize = fontSize;
		NGUIText.Update (true);
		NGUIText.dynamicFont.RequestCharactersInTexture(text, NGUIText.finalSize, NGUIText.fontStyle);

		int prev = 0;
		float rest = width;
		float use = 0;
		for (int i = startPos; i < text.Length; i++) {
			var w = NGUIText.GetGlyphWidth (text [i], prev);
			if (use + w >= rest && i > startPos) {
				return new Tml.Layouter.CharInfo{ CharacterCount = i - startPos, TextWidth = (int)use };
			}
			use += w;
			prev = text [i];
		}
		*/
		return new Tml.Layouter.CharInfo{ CharacterCount = text.Length, TextWidth = width };
	}
		
	public void Start(){
		//ClickEffect.SetActive (false);

		Tml.Layouter.GetCharacterCountCallback = getCharacterCount;

		UrlInputPanel.gameObject.SetActive (false);
		Tml.Style.DefaultFontSize = 30;
		Tml.Logger.SetLogger (new Logger (this));

		var homeUrl = PlayerPrefs.GetString ("HomeUrl");
		if (!string.IsNullOrEmpty (homeUrl)) {
			Url = homeUrl;
		}
		GotoUrl (Url);
	}

	public void OpenUrl(string url){
		var baseUri = new Uri (Url);
		var newUri = new Uri (baseUri, url);

		Url = newUri.ToString();
		UrlLabel.text = Url;
		LogText.text = "";

		StartCoroutine (OpenUrlCoroutine ());
	}

	IEnumerator OpenUrlCoroutine(){
		var homeUrl = PlayerPrefs.GetString ("HomeUrl");
		var defaultStyleUrl = new Uri(new Uri (homeUrl), "./default.tml");

		var defaultStyle = "";
		var www0 = new WWW (defaultStyleUrl.ToString());
		yield return www0;
		if (string.IsNullOrEmpty (www0.error)) {
			defaultStyle = www0.text;
		}

		var www = new WWW (Url);
		yield return www;

		if (!string.IsNullOrEmpty (www.error)) {
			Debug.Log ("error");
			yield break;
		}

		View.Source = defaultStyle + www.text;

		// スクロール領域の調整
		//var w = View.GetComponent<UIWidget> ();
		//var col = View.GetComponent<BoxCollider> ();
		//col.center = new Vector3 (w.width / 2, - w.height / 2);
		//col.size = new Vector3 (w.width, w.height);

		yield return null; // 1フレーム待つ

		//ScrollView.ResetPosition ();
	}

	public void GotoUrl(string url){
		history_.Add (url);
		OpenUrl (url);
	}

	// クリックした時のエフェクトを作成する
	public void CreateClickEffect(Tml.Element e){
		if (e.obj_ == null) {
			return;
		}

		/*
		var effectObj = Instantiate (ClickEffect);
		var effectSprite = effectObj.GetComponent<UISprite> ();

		effectObj.SetActive (true);
		effectObj.transform.SetParent (TopPanel.transform, false);
		effectObj.transform.position = e.obj_.transform.TransformPoint (new Vector3 (e.LayoutedWidth / 2, -e.LayoutedHeight / 2));
		effectSprite.width = e.LayoutedWidth;
		effectSprite.height = e.LayoutedHeight;

		TweenScale.Begin (effectObj, 0.2f, new Vector3 (2, 2));
		TweenAlpha.Begin (effectObj, 0.2f, 0);
		GameObject.Destroy (effectObj, 0.2f);
		*/

	}

	// log handler

	public void LogFormat (LogType logType, UnityEngine.Object context, string format, params object[] args)
	{
		LogText.text += string.Format (format, args) + "\n";
		Debug.logger.LogFormat (logType, context, format, args);
	}

	public void LogException (Exception exception, UnityEngine.Object context)
	{
		Debug.Log ("hoge");
		LogText.text += exception.Message + "\n";
		Debug.logger.LogException (exception, context);
	}

	public void OnBackButtonClick(){
		if (history_.Count > 1) {
			history_.RemoveAt (history_.Count - 1);
			OpenUrl (history_ [history_.Count - 1]);
		}
	}

	public void OnForwardButtonClick(){
	}

	public void OnReloadButtonClick(){
		OpenUrl (Url);
	}

	public void OnUrlLabelClick(){
		UrlInputPanel.gameObject.SetActive (true);
		UrlInputField.text = Url;
	}


	// events

	public void OnTmlViewEvent(){
		StartCoroutine (OnTmlViewEventCoroutine());
	}

	public IEnumerator OnTmlViewEventCoroutine(){
		var e = View.ActiveEvent;

		CreateClickEffect (e.Fragment);

		yield return new WaitForSeconds (0.2f);

		var href = e.Href;
		if (!href.EndsWith (".tml")) {
			href += ".tml";
		}
		GotoUrl (href);
	}

	public void OnUrlOkClick(){
		UrlInputPanel.gameObject.SetActive (false);
		GotoUrl (UrlInputField.text);
	}

	public void OnUrlHomeClick(){
		PlayerPrefs.SetString ("HomeUrl", UrlInputField.text);
	}

	bool logVisible_ = false;
	public Image LogWindow;
	public Text LogText;

	public bool LogVisible { 
		get { return logVisible_; }
		set { 
			logVisible_ = value;
			float h;
			if (value) {
				h = 300;
			} else {
				h = 60;
			}
			LogWindow.rectTransform.sizeDelta = new Vector2 (LogWindow.rectTransform.sizeDelta.x, h);
		} 
	}
		
	public void OnLogWindowClick(){
		LogVisible = !LogVisible;
	}

}
