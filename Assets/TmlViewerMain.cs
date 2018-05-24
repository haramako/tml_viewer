using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class TmlViewerMain : MonoBehaviour, ILogHandler {
	public InputField UrlInput;
	public TmlView View;
	public string Url;
	public ScrollRect ScrollView;
	public RectTransform TopPanel;

	public Image UrlInputPanel;
	public InputField UrlInputField;

	List<string> history_ = new List<string>();

	public void Start(){

        Tml.Layouter.GetCharacterCountCallback = View.GetCharacterCount;

		UrlInputPanel.gameObject.SetActive (false);
		Tml.Style.DefaultFontSize = 30;
		Tml.Logger.SetLogger (new Logger (this));

		var homeUrl = PlayerPrefs.GetString ("HomeUrl");
        if (!string.IsNullOrEmpty (homeUrl)) {
			Url = homeUrl;
		}
        View.BaseUrl = new Uri (homeUrl);
		GotoUrl (Url);
	}

	public void OpenUrl(string url){
		var baseUri = new Uri (Url);
		var newUri = new Uri (baseUri, url);

		Url = newUri.ToString();
		UrlInput.text = Url;
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
        Debug.Log(Url);

		if (!string.IsNullOrEmpty (www.error)) {
			Debug.Log ("error:" + www.error);
			yield break;
		}

		View.SetSource( www.text, defaultStyle);

		yield return null; // 1フレーム待つ

        ScrollView.verticalNormalizedPosition = 1.0f;
	}

	public void GotoUrl(string url){
		history_.Add (url);
		OpenUrl (url);
	}

	// log handler

	public void LogFormat (LogType logType, UnityEngine.Object context, string format, params object[] args)
	{
        if (LogText != null)
        {
            LogText.text += string.Format(format, args) + "\n";
        }
		Debug.unityLogger.LogFormat (logType, context, format, args);
	}

	public void LogException (Exception exception, UnityEngine.Object context)
	{
        if (LogText != null)
        {
            LogText.text += exception.Message + "\n";
        }
		Debug.unityLogger.LogException (exception, context);
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
		UrlInputPanel.gameObject.SetActive (!UrlInputPanel.gameObject.activeSelf);
		UrlInputField.text = Url;
	}


	// events

	public void OnTmlViewEvent(){
		StartCoroutine (OnTmlViewEventCoroutine());
	}

	public IEnumerator OnTmlViewEventCoroutine(){
		var e = View.ActiveEvent;

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
