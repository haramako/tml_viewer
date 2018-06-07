using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Tml;

public class TmlDebugWindow : EditorWindow
{
    public static TmlDebugWindow Instance { get; private set; }

    Element targetElement_;

    [MenuItem("Tools/TML/デバッグウィンドウ")]
    public static void OpenWindow()
    {
        var Instance = GetWindow<TmlDebugWindow>("TMLデバッグ");
    }

    public void SetTargetElement(Element e)
    {
        targetElement_ = e;
        Repaint();
    }

    void drawLabel(string name, string val)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(name);
        EditorGUILayout.LabelField(val);
        EditorGUILayout.EndHorizontal();
    }

    void OnGUI()
    {
        if( !Application.isPlaying )
        {
            return;
        }

        if (targetElement_ != null)
        {
            var e = targetElement_;
            var s = e.Style;
            drawLabel("タグ", e.Tag);
            drawLabel("実際のサイズ", e.LayoutedWidth + "x" + e.LayoutedHeight);
            drawLabel("実際の内側サイズ", e.LayoutedInnerWidth + "x" + e.LayoutedInnerHeight);
            drawLabel("レイアウトタイプ", ""+e.LayoutType);

            drawLabel("サイズ", s.Width + "x" + s.Height);
            drawLabel("マージン(横)", s.MarginLeft + "x" + s.MarginRight);
            drawLabel("マージン(縦)", s.MarginTop + "x" + s.MarginBottom);

        }
        else
        { 
            EditorGUILayout.LabelField("None");
        }
    }

    void OnSelectionChange()
    {
        if (Selection.activeGameObject != null)
        {
            var tmlElement = Selection.activeGameObject.GetComponent<TmlElement>();
            if (tmlElement != null)
            {
                targetElement_ = tmlElement.Element;
            }
            Repaint();
        }
    }
}
