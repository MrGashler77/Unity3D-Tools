#region Libraries
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEditor;
using System;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Collections.Specialized;
#endregion

#region Layout Design Class
[ExecuteInEditMode]
[CustomEditor(typeof(ScriptableObject))]
[CanEditMultipleObjects]
[Serializable]
public class GUILayoutDesignWindow : EditorWindow
{
    #region Variables

    private Rect designWindow;

    #region Window Display Variables
    List<MethodInfo> designMethods;
    List<List<object>> designParams;
    #endregion

    #endregion

    #region On Enable
    void OnEnable()
    {
        //Debug.Log("Enable Started");

        #region Define Window Dimensions
        //Debug.Log("Define Window Dimensions");
        designWindow = new Rect(0, 0, Screen.width, Screen.height);
        #endregion

        #region Make Design Variables Usable
        designMethods = new List<MethodInfo>();
        designParams = new List<List<object>>();
        #endregion
    }
    #endregion

    #region INIT
    [MenuItem("Window/Custom/GUILayout Design")]
    static void Init()
    {
        GUILayoutDesignWindow guilayout = (GUILayoutDesignWindow)GetWindow(typeof(GUILayoutDesignWindow));
        guilayout.maxSize = new Vector2(Screen.width, Screen.height);
        EditorUtility.SetDirty(guilayout);
        guilayout.Show();
    }
    #endregion

    #region On GUI
    void OnGUI()
    {
        #region Display Window
        BeginWindows();

        designWindow = GUILayout.Window(46, designWindow, DesignWindow, "Design Display", GUILayout.Width(Screen.width), GUILayout.Height(Screen.height));

        EndWindows();
        #endregion
    }
    #endregion

    #region Custom Methods

    #region Window Method
    private void DesignWindow(int ID)
    {
        #region Window 46
        if (ID == 46)
        {
            if (designMethods.Count <= 0 && designParams.Count <= 0)
            {
                GUI.skin.box.stretchHeight = true;
                GUI.skin.box.stretchWidth = true;
                GUILayout.Box("Test Box");
                GUI.skin.box.stretchHeight = false;
                GUI.skin.box.stretchWidth = false;
            }
            else
            {
                for(int n = 0; n <= designMethods.Count - 1; n++)
                {
                    designMethods[n].Invoke(this, designParams[n].ToArray());
                }
            }
        }
        #endregion
    }
    #endregion

    #region Receive Design Objects
    public void receiveObjects(MethodInfo mInf, List<object> parms)
    {
        designMethods.Add(mInf);
        designParams.Add(parms);
    }
    #endregion

    #endregion

}
#endregion
