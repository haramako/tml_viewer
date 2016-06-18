using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TmlViewerMain : MonoBehaviour {
	public UILabel UrlLabel;
	public TmlView View;
	public string Url;

	public UnityEngine.UI.Image UrlInputPanel;
	public UnityEngine.UI.InputField UrlInputField;

	List<string> history_ = new List<string>();

	static Tml.Layouter.CharInfo getCharacterCount(Tml.Element e, string text, int startPos, int fontSize, int width){
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
		return new Tml.Layouter.CharInfo{ CharacterCount = text.Length - startPos, TextWidth = (int)use };
	}
		
	public void Start(){
		Tml.Layouter.GetCharacterCountCallback = getCharacterCount;

		UrlInputPanel.gameObject.SetActive (false);
		Tml.Style.DefaultFontSize = 30;
		Tml.Logger.SetLogger (Debug.logger);
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
		Debug.Log (Url);

		StartCoroutine (OpenUrlCoroutine ());
	}

	IEnumerator OpenUrlCoroutine(){
		var defaultStyleUrl = new Uri(new Uri (Url), "/default.tml");

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
	}

	public void GotoUrl(string url){
		history_.Add (url);
		OpenUrl (url);
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

	public void OnTmlViewEvent(){
		var href = View.ActiveEvent.Href;
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

}
