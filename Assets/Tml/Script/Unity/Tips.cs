using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using RSG;
using System;

public class Tips {

	GameObject tips_;
   
    public void Show(TmlView view, Tml.Element e, RectTransform obj)
    {
        if( tips_ != null)
        {
            return;
        }

        tips_ = new GameObject();
        var rt = tips_.AddComponent<RectTransform>();
        rt.anchorMax = new Vector2(0, 1);
        rt.anchorMin = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        tips_.transform.SetParent(view.transform, false);
        rt.sizeDelta = new Vector2(300, 100);

        TmlView.RenderToContainer(view, rt, e.Tips, view.DefaultSource);

        var pos = new Vector3(obj.rect.xMax, obj.rect.yMax) - new Vector3(rt.rect.xMin, obj.rect.yMax);
        rt.position = obj.localToWorldMatrix.MultiplyPoint(pos);
    }

    public void Hide()
    {
        if (tips_ == null)
        {
            return;
        }

        GameObject.Destroy(tips_);
        tips_ = null;
    }

}
