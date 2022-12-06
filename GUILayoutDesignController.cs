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

#region Layout Design Controller
[ExecuteInEditMode]
[CustomEditor(typeof(ScriptableObject))]
[CanEditMultipleObjects]
[Serializable]
public class GUILayoutDesignController : EditorWindow
{
    #region Variables

    private bool refined = false;

    #region Refined Variables

    #region GUI Variables

    #region Upper Control Variables

    #region Control Variables
    private string[] toolbarControls = { "Modify", "Clear", "Save", "Load" };
    private int toolbarSelected;
    #endregion

    #region Global Option Variables
    private GUISkin globalSkin;
    #endregion

    #endregion

    #region Column Variables

    #region Left Side Variables
    private Vector2 leftPos;
    private List<string> designNames;
    private int designChosen;
    private List<MethodInfo> designs;
    private List<List<object>> designParams;
    private List<List<MethodInfo>> designGloMs;
    private List<List<object>> designGloPs;
    #endregion

    #region Right Side Variables

    #region Positions
    private Vector2 rightPos;
    private Vector2 gloPos;
    private Vector2 paramPos;
    private Vector2 gloPpos;
    #endregion

    #region Placeholders(!GUILayout)
    private MethodInfo pHmInf;
    private List<object> pHparamsR;
    private List<object> pHparamsO;
    #endregion

    #region Placeholders(GUILayout)

    #region List
    private List<MethodInfo> pHgloMs;
    private List<string> pHgloNames;
    private List<object> pHgloPs;
    #endregion

    #region New Creations
    private MethodInfo pHgloM;
    private object pHgloP;
    #endregion

    #endregion
    
    #region Layers
    private string[] layerOuter = { "Enclosers", "Controls", "Display" };

    #region Layer0
    private string[] layerInner0;
    private List<string> lIn0L;
    private List<int> lIn0F;
    #endregion

    #region Layer1
    private string[] layerInner1;
    private List<string> lIn1L;
    private List<int> lIn1F;
    #endregion

    #region Layer2
    private string[] layerInner2;
    private List<string> lIn2L;
    private List<int> lIn2F;
    #endregion

    private string[] layoutNames = { "Width", "Height", "MinWidth", "MinHeight", "MaxWidth", "MaxHeight", "ExpandWidth", "ExpandHeight" };
    #endregion

    #region Layer/Layout Selections
    private int outerSelected;
    private int innerSelected;
    private int layoutSelected;
    #endregion

    #endregion

    #endregion

    #endregion

    #region !GUI Variables

    #region Invokers
    private List<MethodInfo> layer0Invoke;
    private List<MethodInfo> layer1Invoke;
    private List<MethodInfo> layer2Invoke;
    private List<MethodInfo> layoutsInvoke;
    #endregion

    #endregion

    #endregion

    #region Communicate With Design Window Variables
    static GUILayoutDesignWindow window;
    #endregion

    #region Legacy Variables
    //
    #region Toolbar Variables
    private string[] toolbarButtons = { "Modify", "Clear", "Save", "Load" };
    private int toolbarChosen;
    #endregion
    //
    #region Design List Variables
    private Vector2 designPos;
    private int objChosen;
    private List<string> objList;
    #endregion
    //
    #region Expanded Data Variables
    private string[] outerLayer = { "Enclosers", "Controls", "Display" };
    //
    #region Layer 0
    private string[] innerlayer0;
    private List<string> l0L;
    private List<int> l0F;
    #endregion
    //
    #region Layer 1
    private string[] innerlayer1;
    private List<string> l1L;
    private List<int> l1F;
    #endregion
    //
    #region Layer 2
    private string[] innerlayer2;
    private List<string> l2L;
    private List<int> l2F;
    #endregion
    //
    #region Layout Layer
    private string[] layouts = { "Width", "Height", "MinWidth", "MinHeight", "MaxWidth", "MaxHeight", "ExpandWidth", "ExpandHeight" };
    private List<string> layoutList;
    GUILayoutOption[] aGO;
    List<MethodInfo> lM;
    List<object> lMP;
    #endregion
    //
    #region Invokers & Other Lists
    private int outerChosen, innerChosen;
    private int layoutChosen;
    private List<MethodInfo> l0Invoke;
    private List<MethodInfo> l1Invoke;
    private List<MethodInfo> l2Invoke;
    private List<MethodInfo> layoutInvoke;
    private List<List<object>> parameters;
    private List<object> displayParams;
    private Vector2 dataPos;
    #endregion

    #endregion

    #endregion

    #endregion

    #region On Enable
    void OnEnable()
    {
        #region Refined On Enable
        if (refined)
        {
            #region Set Vars

            #region Set Positions
            leftPos = Vector2.zero;
            rightPos = Vector2.zero;
            gloPos = Vector2.zero;
            paramPos = Vector2.zero;
            gloPpos = Vector2.zero;
            #endregion

            #region Set Selections
            toolbarSelected = -1;
            designChosen = -1;
            outerSelected = -1;
            innerSelected = -1;
            layoutSelected = -1;
            #endregion

            #region Set Lists
            designParams = new List<List<object>>();
            designGloMs = new List<List<MethodInfo>>();
            designGloPs = new List<List<object>>();
            pHparamsR = new List<object>();
            pHparamsO = new List<object>();
            pHgloMs = new List<MethodInfo>();
            pHgloNames = new List<string>();
            pHgloPs = new List<object>();
            lIn0L = new List<string>();
            lIn1L = new List<string>();
            lIn2L = new List<string>();
            lIn0F = new List<int>();
            lIn1F = new List<int>();
            lIn2F = new List<int>();
            layer0Invoke = new List<MethodInfo>();
            layer1Invoke = new List<MethodInfo>();
            layer2Invoke = new List<MethodInfo>();
            layoutsInvoke = new List<MethodInfo>();
            #endregion

            #region Set Objects
            pHgloM = null;
            pHmInf = null;
            #endregion

            #region Set Layer Arrays
            layerInner0 = new string[] { "BeginHorizontal", "EndHorizontal", "BeginVertical", "EndVertical", "BeginScrollView", "EndScrollView" };
            layerInner1 = new string[] { "Button", "TextField", "TextArea", "Toggle", "Toolbar", "SelectionGrid", "HorizontalScrollbar", "VerticalScrollbar" };
            layerInner2 = new string[] { "Label", "Box", "Space", "FlexibleSpace" };
            #endregion

            #endregion

            #region Populate Lists of Invokers

            #region Populate Startup
            MethodInfo[] mInf = new GUILayout().GetType().GetMethods();
            //Debug.Log("Total number of methods:" + mInf.Length);
            pHgloNames.AddRange(layoutNames);
            layoutList.AddRange(layouts);
            lIn0L.AddRange(layerInner0);
            lIn1L.AddRange(layerInner1);
            lIn2L.AddRange(layerInner2);
            int[] g = { 4, 5, 10, 11, 18, 19 };
            lIn0F.AddRange(g);
            int[] h = { 5, 9, 13, 19, 30, 36, 38, 40 };
            lIn1F.AddRange(h);
            int[] i = { 5, 11, 12, 13 };
            lIn2F.AddRange(i);
            #endregion

            #region Add Methods by Matching Names
            for (int n = 0; n <= mInf.Length - 1; n++)
            {
                #region GUILayout
                if (layoutNames.Contains(mInf[n].Name))
                {
                    layoutsInvoke.Add(mInf[n]);
                }
                #endregion

                #region All Others
                else
                {
                    #region Enclosers
                    if (lIn0L.Contains(mInf[n].Name))
                    {
                        layer0Invoke.Add(mInf[n]);
                    }
                    #endregion

                    #region Controls
                    if (lIn1L.Contains(mInf[n].Name))
                    {
                        layer1Invoke.Add(mInf[n]);
                    }
                    #endregion

                    #region Display
                    if (lIn2L.Contains(mInf[n].Name))
                    {
                        layer2Invoke.Add(mInf[n]);
                    }
                    #endregion
                }
                #endregion
            }
            #endregion

            #region Filter Layers

            #region Layer0
            List<MethodInfo> ph0 = new List<MethodInfo>();
            for (int n = 0; n <= layer0Invoke.Count - 1; n++)
            {
                if (lIn0F.Contains(n))
                {
                    ph0.Add(layer0Invoke[n]);
                }
            }

            layer0Invoke = ph0;
            #endregion

            #region Layer1
            List<MethodInfo> ph1 = new List<MethodInfo>();
            for (int n = 0; n <= layer1Invoke.Count - 1; n++)
            {
                if (lIn1F.Contains(n))
                {
                    ph1.Add(layer1Invoke[n]);
                }
            }

            layer1Invoke = ph1;
            #endregion

            #region Layer2
            List<MethodInfo> ph2 = new List<MethodInfo>();
            for (int n = 0; n <= layer2Invoke.Count - 1; n++)
            {
                if (lIn2F.Contains(n))
                {
                    ph2.Add(layer2Invoke[n]);
                }
            }

            layer2Invoke = ph2;
            #endregion

            #endregion
            
            #endregion
        }
        #endregion

        #region Legacy On Enable
        else
        {
            #region Set Variables To Start Point
            toolbarChosen = -1;
            objChosen = -1;
            outerChosen = -1;
            innerChosen = -1;
            layoutChosen = -1;
            designPos = Vector2.zero;
            dataPos = Vector2.zero;
            parameters = new List<List<object>>();
            displayParams = null;
            objList = new List<string>();
            objList.Add("Create New GUI Object");
            #endregion

            #region Set & Verify Layer Names

            #region Set Layer Names
            //Debug.Log("Set Layer Names");
            innerlayer0 = new string[] { "BeginHorizontal", "EndHorizontal", "BeginVertical", "EndVertical", "BeginScrollView", "EndScrollView" };
            innerlayer1 = new string[] { "Button", "TextField", "TextArea", "Toggle", "Toolbar", "SelectionGrid", "HorizontalScrollbar", "VerticalScrollbar" };
            innerlayer2 = new string[] { "Label", "Box", "Space", "FlexibleSpace" };
            #endregion

            #region Verify Layer Names
            //Debug.Log("Verify Layer Names");
            string l0 = "Layer 0 Names:\n";
            for (int n = 0; n <= innerlayer0.Length - 1; n++)
            {
                l0 += innerlayer0[n] + ",";
            }
            //Debug.Log(l0);
            string l1 = "Layer 1 Names:\n";
            for (int n = 0; n <= innerlayer1.Length - 1; n++)
            {
                l1 += innerlayer1[n] + ",";
            }
            //Debug.Log(l1);
            string l2 = "Layer 2 Names:\n";
            for (int n = 0; n <= innerlayer2.Length - 1; n++)
            {
                l2 += innerlayer2[n] + ",";
            }
            //Debug.Log(l2);
            #endregion

            #endregion

            #region Populate Lists of Invokers

            #region Populate Startup Operations
            //Debug.Log("Populate Invokers");
            l0Invoke = new List<MethodInfo>();
            l1Invoke = new List<MethodInfo>();
            l2Invoke = new List<MethodInfo>();
            layoutInvoke = new List<MethodInfo>();
            MethodInfo[] mInf = new GUILayout().GetType().GetMethods();
            //Debug.Log("Total number of methods:" + mInf.Length);
            layoutList = new List<string>();
            layoutList.AddRange(layouts);
            l0L = new List<string>();
            l0L.AddRange(innerlayer0);
            l1L = new List<string>();
            l1L.AddRange(innerlayer1);
            l2L = new List<string>();
            l2L.AddRange(innerlayer2);
            l0F = new List<int>();
            int[] g = { 4, 5, 10, 11, 18, 19 };
            l0F.AddRange(g);
            l1F = new List<int>();
            int[] h = { 5, 9, 13, 19, 30, 36, 38, 40 };
            l1F.AddRange(h);
            l2F = new List<int>();
            int[] i = { 5, 11, 12, 13 };
            l2F.AddRange(i);
            lM = null;
            lMP = null;
            #endregion

            #region Add Methods By Name Match
            for (int n = 0; n <= mInf.Length - 1; n++)
            {
                #region GUILayout
                if (layoutList.Contains(mInf[n].Name))
                {
                    layoutInvoke.Add(mInf[n]);
                }
                #endregion

                #region All Others
                else
                {
                    #region Enclosers
                    if (l0L.Contains(mInf[n].Name))
                    {
                        l0Invoke.Add(mInf[n]);
                    }
                    #endregion

                    #region Controls
                    if (l1L.Contains(mInf[n].Name))
                    {
                        l1Invoke.Add(mInf[n]);
                    }
                    #endregion

                    #region Display
                    if (l2L.Contains(mInf[n].Name))
                    {
                        l2Invoke.Add(mInf[n]);
                    }
                    #endregion
                }
                #endregion
            }
            #endregion

            #region Filter Layers

            #region Filter & Debug Layer 0

            #region Filter
            List<MethodInfo> ph0 = new List<MethodInfo>();
            for (int n = 0; n <= l0Invoke.Count - 1; n++)
            {
                if (l0F.Contains(n))
                {
                    ph0.Add(l0Invoke[n]);
                }
            }

            l0Invoke = ph0;
            #endregion

            #region Debug
            if (l0Invoke != null)
            {
                if (l0Invoke.Count > 0)
                {
                    //Debug.Log("l0 Invoke Count:" + l0Invoke.Count);
                    //Debug.Log("Layer0");
                    foreach (MethodInfo m in l0Invoke)
                    {
                        string iOut0 = "[" + l0Invoke.IndexOf(m) + "]Name:" + m.Name + "\nParameters:" + "\n";
                        parameters = new List<List<object>>();
                        ParameterInfo[] parms = m.GetParameters();
                        for (int n = 0; n <= parms.Length - 1; n++)
                        {
                            iOut0 += "[" + n + "]" + parms[n].ParameterType + ",";
                        }
                        //Debug.Log(iOut0);
                    }
                }
                else
                {
                    Debug.Log("l0 Invoke !null but has no members");
                }
            }
            else
            {
                Debug.Log("l0 Invoke is null");
            }
            #endregion

            #endregion

            #region Filter & Debug Layer 1

            #region Filter
            List<MethodInfo> ph1 = new List<MethodInfo>();
            for (int n = 0; n <= l1Invoke.Count - 1; n++)
            {
                if (l1F.Contains(n))
                {
                    ph1.Add(l1Invoke[n]);
                }
            }

            l1Invoke = ph1;
            #endregion

            #region Debug
            if (l1Invoke != null)
            {
                if (l1Invoke.Count > 0)
                {
                    //Debug.Log("l1 Invoke Count:" + l1Invoke.Count);
                    //Debug.Log("Layer1");
                    foreach (MethodInfo m in l1Invoke)
                    {
                        string iOut1 = "[" + l1Invoke.IndexOf(m) + "]Name:" + m.Name + "\nParameters:" + "\n";
                        parameters = new List<List<object>>();
                        ParameterInfo[] parms = m.GetParameters();
                        for (int n = 0; n <= parms.Length - 1; n++)
                        {
                            iOut1 += "[" + n + "]" + parms[n].ParameterType + ",";
                        }
                        //Debug.Log(iOut1);
                    }
                }
                else
                {
                    Debug.Log("l1 Invoke !null but has no members");
                }
            }
            else
            {
                Debug.Log("l1 Invoke is null");
            }
            #endregion

            #endregion

            #region Filter & Debug Layer 2

            #region Filter
            List<MethodInfo> ph2 = new List<MethodInfo>();
            for (int n = 0; n <= l2Invoke.Count - 1; n++)
            {
                if (l2F.Contains(n))
                {
                    ph2.Add(l2Invoke[n]);
                }
            }

            l2Invoke = ph2;
            #endregion

            #region Debug
            if (l2Invoke != null)
            {
                if (l2Invoke.Count > 0)
                {
                    //Debug.Log("l2 Invoke Count:" + l2Invoke.Count);
                    //Debug.Log("Layer2");
                    foreach (MethodInfo m in l2Invoke)
                    {
                        string iOut2 = "[" + l2Invoke.IndexOf(m) + "]Name:" + m.Name + "\nParameters:" + "\n";
                        parameters = new List<List<object>>();
                        ParameterInfo[] parms = m.GetParameters();
                        for (int n = 0; n <= parms.Length - 1; n++)
                        {
                            iOut2 += "[" + n + "]" + parms[n].ParameterType + ",";
                        }
                        //Debug.Log(iOut2);
                    }
                }
                else
                {
                    Debug.Log("l2Invoke !null but has no members");
                }
            }
            else
            {
                Debug.Log("l2 Invoke is null");
            }
            #endregion

            #endregion

            #endregion

            #region Debug Layout Invokers
            if (layoutInvoke != null)
            {
                if (layoutInvoke.Count > 0)
                {
                    //Debug.Log("Layout Invoke Count:" + layoutInvoke.Count);
                    //Debug.Log("Layout");
                    foreach (MethodInfo m in layoutInvoke)
                    {
                        string iOut = "Name:" + m.Name + "\nParameters:" + "\n";
                        parameters = new List<List<object>>();
                        ParameterInfo[] parms = m.GetParameters();
                        for (int n = 0; n <= parms.Length - 1; n++)
                        {
                            iOut += "[" + n + "]" + parms[n].ParameterType + "\n";
                        }
                        //Debug.Log(iOut);
                    }
                }
                else
                {
                    Debug.Log("layoutInvoke !null but has no members");
                }
            }
            else
            {
                Debug.Log("layout Invoke is null");
            }
            #endregion

            #endregion
        }
        #endregion
    }
    #endregion

    #region INIT
    [MenuItem("Window/Custom/GUILayout Controller")]
    static void Init()
    {
        GUILayoutDesignController controller = (GUILayoutDesignController)GetWindow(typeof(GUILayoutDesignController));
        controller.Show();
        window = (GUILayoutDesignWindow)GetWindow(typeof(GUILayoutDesignWindow));
    }
    #endregion

    #region On GUI
     void OnGUI()
    {
        #region Refined Layout
        if (refined)
        {
            #region Prelayout Modifications
            GUI.skin.button.stretchWidth = true;
            GUI.skin.box.padding = GUI.skin.button.padding;
            #endregion

            #region Menu
            GUILayout.BeginVertical();

            #region Upper Controls

            #region Toolbar
            toolbarChosen = GUILayout.SelectionGrid(toolbarChosen, toolbarControls, toolbarControls.Length);

            #region Control Logic
            if (toolbarChosen >= 0)
            {
                Debug.Log("Toolbar button " + toolbarControls[toolbarChosen] + " selected. Functionality not yet implemented.");
                toolbarChosen = -1;
            }
            #endregion

            #endregion

            #region Global Options
            GUILayout.BeginHorizontal();

            GUILayout.Label("Global Options:");

            globalSkin = (GUISkin)EditorGUILayout.ObjectField(new GUIContent("GUISkin"), globalSkin, typeof(GUISkin), true);

            GUILayout.EndHorizontal();
            #endregion

            #endregion

            #region Columns
            GUILayout.BeginHorizontal();

            #region Left Side
            GUILayout.BeginVertical();

            #region Display Logic

            #region Create/Delete Buttons
            GUILayout.BeginHorizontal();

            #region Create Button
            if (GUILayout.Button("Create"))
            {
                Debug.Log("This will create a new design object when functionality is implemented");
            }
            #endregion

            #region Delete Button
            if (GUILayout.Button("Delete"))
            {
                Debug.Log("This will shift the list to a selection mode so that design objects can be selected and deleted");
            }
            #endregion

            GUILayout.EndHorizontal();
            #endregion

            #region List
            leftPos = GUILayout.BeginScrollView(leftPos);

            #region Display Design Objects

            #endregion

            GUILayout.EndScrollView();
            #endregion

            #endregion

            #region Control Logic

            #endregion

            GUILayout.EndVertical();
            #endregion

            #region Right Side
            GUILayout.BeginVertical();
            rightPos = GUILayout.BeginScrollView(rightPos);

            #region Non GUILayoutOption Display
            paramPos = GUILayout.BeginScrollView(paramPos);

            #region Outer Layer Display

            #endregion

            #region Inner Layer Display

            #endregion

            #region Parameters Display

            #region Required Parameters

            #endregion

            #region Break

            #endregion

            #region Optional Parameters

            #endregion

            #endregion

            #region Create/Clear Buttons

            #endregion

            GUILayout.EndScrollView();
            #endregion

            #region GUILayoutOption Display
            GUILayout.BeginHorizontal();

            #region GUILayoutOption List (Left)
            GUILayout.BeginVertical();

            #region +/- Buttons
            GUILayout.BeginHorizontal();

            #region +
            if (GUILayout.Button("+"))
            {
                Debug.Log("This will allow the creation of another GUILayoutOption Parameter");
            }
            #endregion

            #region -
            if (GUILayout.Button("-"))
            {
                Debug.Log("This will remove the most recently created GUILayoutOption Parameter");
            }
            #endregion

            GUILayout.EndHorizontal();
            #endregion

            #region List
            gloPos = GUILayout.BeginScrollView(gloPos);

            #region Display
            GUILayout.BeginVertical();

            GUILayout.EndVertical();
            #endregion

            #region Control Logic

            #endregion

            GUILayout.EndScrollView();
            #endregion

            GUILayout.EndVertical();
            #endregion

            #region GUILayoutOption Selection List (Right)
            GUILayout.BeginVertical();
            gloPpos = GUILayout.BeginScrollView(gloPpos);

            #region Select A Type

            #endregion

            #region Display Parameters

            #endregion

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            #endregion

            #region Add/Clear Buttons
            GUILayout.BeginHorizontal();

            #region Add
            if (GUILayout.Button("Add"))
            {
                Debug.Log("This will add the created instance of a GUILayoutOption to the parameter list");
            }
            #endregion

            #region Clear
            if (GUILayout.Button("Clear"))
            {
                Debug.Log("This will clear all fields for this instance of a GUILayoutOption");
            }
            #endregion

            GUILayout.EndHorizontal();
            #endregion

            GUILayout.EndHorizontal();
            #endregion

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            #endregion

            GUILayout.EndHorizontal();
            #endregion

            GUILayout.EndVertical();
            #endregion
        }
        #endregion

        #region Legacy
        else
        {
            GUI.skin.button.stretchWidth = true;
            GUI.skin.box.padding = GUI.skin.button.padding;

            #region Totally Enclosed
            GUILayout.BeginVertical();

            #region Toolbar Display

            toolbarChosen = GUILayout.Toolbar(toolbarChosen, toolbarButtons, GUILayout.Width(Screen.width - 14));

            #region Control Logic
            if (toolbarChosen >= 0)
            {
                if (toolbarChosen == 0)
                {
                    Debug.Log("Chosen to modify");
                }

                if (toolbarChosen == 1)
                {
                    Debug.Log("Chosen to clear");
                }

                if (toolbarChosen > 1)
                {
                    Debug.Log("Functionality not yet implemented");
                }
                toolbarChosen = -1;
            }
            #endregion

            GUILayout.Space(10);

            #endregion

            #region Side By Side Enclosure
            GUILayout.BeginHorizontal(GUILayout.Width(Screen.width - 10));

            #region Left Side
            GUILayout.BeginVertical(GUILayout.Width((Screen.width / 2) - 5));
            designPos = GUILayout.BeginScrollView(designPos);

            #region Display List
            if (objChosen < 0)
            {
                objChosen = GUILayout.SelectionGrid(objChosen, objList.ToArray(), 1, GUILayout.Width((Screen.width / 2) - 5));
            }
            #endregion

            #region Expand Object Chosen
            else if (objChosen >= 0)
            {
                if (GUILayout.Button(objList[objChosen]))
                {
                    objChosen = -1;
                }
            }
            #endregion

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            #endregion

            #region Right Side
            GUILayout.BeginVertical(GUILayout.Width((Screen.width / 2) - 10));

            #region No Object Selected
            if (objChosen < 0)
            {
                GUI.skin.box.stretchHeight = true;
                GUI.skin.box.stretchWidth = true;
                GUILayout.Box("Select An Object :)");
                GUI.skin.box.stretchHeight = false;
                GUI.skin.box.stretchWidth = false;
                displayParams = null;
            }
            #endregion

            #region Object Selected
            else if (objChosen >= 0)
            {
                #region Create New Object
                if (objList[objChosen] == "Create New GUI Object")
                {
                    #region Outer Layer
                    if (outerChosen < 0)
                    {
                        //Debug.Log("Outer Layer Selection");
                        outerChosen = GUILayout.SelectionGrid(outerChosen, outerLayer, outerLayer.Length, GUILayout.Width((Screen.width / 2) - 10));
                    }
                    else
                    {
                        if (GUILayout.Button(outerLayer[outerChosen]))
                        {
                            outerChosen = -1;
                            innerChosen = -1;
                        }
                    }
                    #endregion

                    #region Inner Layer
                    if (outerChosen >= 0 && innerChosen < 0)
                    {
                        //Debug.Log("Inner Layer Selection");
                        #region Layer 0
                        if (outerChosen == 0)
                        {
                            innerChosen = GUILayout.SelectionGrid(innerChosen, innerlayer0, 2, GUILayout.Width((Screen.width / 2) - 10));
                        }
                        #endregion

                        #region Layer 1
                        else if (outerChosen == 1)
                        {
                            innerChosen = GUILayout.SelectionGrid(innerChosen, innerlayer1, 2, GUILayout.Width((Screen.width / 2) - 10));
                        }
                        #endregion

                        #region Layer 2
                        else if (outerChosen == 2)
                        {
                            innerChosen = GUILayout.SelectionGrid(innerChosen, innerlayer2, 2, GUILayout.Width((Screen.width / 2) - 10));
                        }
                        #endregion

                        displayParams = null;
                    }
                    else if (outerChosen >= 0 && innerChosen >= 0)
                    {
                        #region Layer 0
                        if (outerChosen == 0)
                        {
                            if (GUILayout.Button(innerlayer0[innerChosen]))
                            {
                                innerChosen = -1;
                            }
                        }
                        #endregion

                        #region Layer 1
                        if (outerChosen == 1)
                        {
                            if (GUILayout.Button(innerlayer1[innerChosen]))
                            {
                                innerChosen = -1;
                            }
                        }
                        #endregion

                        #region Layer 2
                        if (outerChosen == 2)
                        {
                            if (GUILayout.Button(innerlayer2[innerChosen]))
                            {
                                innerChosen = -1;
                            }
                        }
                        #endregion
                    }
                    #endregion

                    #region Parameter Selection
                    if (outerChosen >= 0 && innerChosen >= 0)
                    {
                        //Debug.Log("Parameter Selection");
                        #region Obtain Parameters
                        if (displayParams == null)
                        {
                            //Debug.Log("Get Parameters");
                            displayParams = new List<object>();
                            displayParams = getParameters(outerChosen, innerChosen);
                            Repaint();
                        }
                        #endregion

                        #region Display Parameters
                        else
                        {
                            //Debug.Log("Display Parameters");

                            if (displayParams.Count <= 0)
                            {
                                //Debug.Log("Populate Parameters");
                                displayParams = getParameters(outerChosen, innerChosen);
                            }
                            else
                            {
                                #region Has Required Parameters
                                if (displayParams[0] != null)
                                {
                                    //Debug.Log("Has Required Parameters");
                                    int nB = displayParams.IndexOf(null);
                                    //Debug.Log("Req:" + nB + "\n" + "Opt:" + (displayParams.Count - (nB + 1)) + "\nNull Index:" + nB + "\n" + "Total:" + displayParams.Count);

                                    GUI.skin.textArea.stretchWidth = true;
                                    GUI.skin.textField.stretchWidth = true;
                                    GUI.skin.box.stretchWidth = true;

                                    #region Display Parameters
                                    for (int n = 0; n <= displayParams.Count - 1; n++)
                                    {
                                        #region Required Parameters
                                        if (n < nB)
                                        {
                                            #region Header
                                            if (n == 0)
                                            {
                                                //Debug.Log("RqParams");
                                                GUILayout.Box("Required Parameters");
                                            }
                                            #endregion

                                            #region Parameters
                                            GUI.skin.textArea.stretchWidth = true;
                                            GUI.skin.textField.stretchWidth = true;

                                            #region Show Required Parameters

                                            #region !Array Type
                                            if (displayParams[n].GetType() != typeof(List<>) && displayParams[n].GetType() != typeof(Array))
                                            {
                                                object o = displayParams[n];
                                                GUI.skin.box.stretchWidth = true;
                                                GUILayout.Box(o.ToString());
                                                GUI.skin.box.stretchWidth = false;

                                                #region Vector 2
                                                if (o.GetType() == typeof(Vector2))
                                                {
                                                    Vector2 v = (Vector2)o;
                                                    v = EditorGUILayout.Vector2Field("", v);
                                                    o = v;
                                                }
                                                #endregion

                                                #region Bool
                                                if (o.GetType() == typeof(bool))
                                                {
                                                    bool b = (bool)o;
                                                    b = GUILayout.Toggle(b, "");
                                                    o = b;
                                                }
                                                #endregion

                                                #region Int
                                                if (o.GetType() == typeof(int))
                                                {
                                                    int i = (int)o;
                                                    i = EditorGUILayout.IntField(i);
                                                    o = i;
                                                }
                                                #endregion

                                                #region String
                                                if (o.GetType() == typeof(string))
                                                {
                                                    string s = (string)o;
                                                    s = EditorGUILayout.TextField(s);
                                                    o = s;
                                                }
                                                #endregion

                                                #region Float
                                                if (o.GetType() == typeof(float))
                                                {
                                                    float f = (float)o;
                                                    f = EditorGUILayout.FloatField(f);
                                                    o = f;
                                                }
                                                #endregion

                                                #region GUIContent
                                                if (o.GetType() == typeof(GUIContent))
                                                {
                                                    GUILayout.BeginVertical();

                                                    GUIContent gC = (GUIContent)o;

                                                    GUILayout.BeginHorizontal();
                                                    GUILayout.Label("Image");
                                                    gC.image = (Texture2D)EditorGUILayout.ObjectField(gC.image, typeof(Texture2D), true);
                                                    GUILayout.EndHorizontal();

                                                    GUILayout.BeginHorizontal();
                                                    GUILayout.Label("Text");
                                                    gC.text = GUILayout.TextField(gC.text, GUILayout.MinWidth(50));
                                                    GUILayout.EndHorizontal();

                                                    GUILayout.BeginHorizontal();
                                                    GUILayout.Label("Tooltip");
                                                    gC.tooltip = GUILayout.TextField(gC.tooltip, GUILayout.MinWidth(50));
                                                    GUILayout.EndHorizontal();

                                                    o = gC;

                                                    GUILayout.EndVertical();
                                                }
                                                #endregion

                                                displayParams[n] = o;
                                                //Debug.Log("End of iteration(!ArrayReq)");
                                            }
                                            #endregion

                                            #region Array Type
                                            else
                                            {
                                                object o = displayParams[n];

                                                #region GUIContent

                                                #region Prepare Array
                                                List<GUIContent> lGC = (List<GUIContent>)o;

                                                if (lGC.Count <= 0)
                                                {
                                                    GUIContent gC0 = new GUIContent();
                                                    lGC.Add(gC0);
                                                }
                                                #endregion

                                                GUILayout.BeginHorizontal();

                                                #region Add/Remove Buttons
                                                GUILayout.BeginVertical();

                                                #region Add New Member Button
                                                if (GUILayout.Button("+", GUILayout.MaxWidth(50)))
                                                {
                                                    lGC.Add(new GUIContent());
                                                }
                                                #endregion

                                                #region Remove Member Button
                                                if (GUILayout.Button("-", GUILayout.MaxWidth(50)))
                                                {
                                                    if (lGC.Count > 1)
                                                    {
                                                        lGC.Remove(lGC[lGC.Count - 1]);
                                                    }
                                                    else
                                                    {
                                                        lGC[0] = new GUIContent();
                                                    }
                                                }
                                                #endregion

                                                GUILayout.EndVertical();
                                                #endregion

                                                #region Display Array Members
                                                GUILayout.BeginVertical();

                                                for (int u = 0; u <= lGC.Count - 1; u++)
                                                {
                                                    GUIContent gCon = lGC[n];

                                                    #region Display GUIContent
                                                    GUILayout.BeginVertical();

                                                    GUILayout.BeginHorizontal();
                                                    GUILayout.Label("Image");
                                                    gCon.image = (Texture2D)EditorGUILayout.ObjectField(gCon.image, typeof(Texture2D), true);
                                                    GUILayout.EndHorizontal();

                                                    GUILayout.BeginHorizontal();
                                                    GUILayout.Label("Text");
                                                    gCon.text = GUILayout.TextField(gCon.text, GUILayout.MinWidth(50));
                                                    GUILayout.EndHorizontal();

                                                    GUILayout.BeginHorizontal();
                                                    GUILayout.Label("Tooltip");
                                                    gCon.tooltip = GUILayout.TextField(gCon.tooltip, GUILayout.MinWidth(50));
                                                    GUILayout.EndHorizontal();

                                                    lGC[n] = gCon;

                                                    GUILayout.EndVertical();
                                                    #endregion

                                                }

                                                GUILayout.EndVertical();
                                                #endregion

                                                GUILayout.EndHorizontal();

                                                #region Passback To Parameter List
                                                List<GUIContent> lGUI = new List<GUIContent>();
                                                for (int u = 0; u <= lGC.Count - 1; u++)
                                                {
                                                    if (lGC[u].image != null || lGC[u].text != "" || lGC[u].tooltip != "")
                                                    {
                                                        lGUI.Add(lGC[u]);
                                                    }
                                                    else
                                                    {
                                                        lGUI.Add(null);
                                                    }
                                                }

                                                for (int u = 0; u <= lGUI.Count - 1; u++)
                                                {
                                                    if (lGUI[u] == null)
                                                    {
                                                        lGUI.Remove(lGUI[u]);
                                                    }
                                                }
                                                o = lGUI.ToArray();

                                                displayParams[n] = o;
                                                #endregion

                                                #endregion

                                                Debug.Log("End of iteration(ArrayReq)");
                                            }
                                            #endregion

                                            #endregion

                                            GUI.skin.textArea.stretchWidth = false;
                                            GUI.skin.textField.stretchWidth = false;
                                            #endregion
                                        }
                                        #endregion

                                        #region Break
                                        else if (n == nB)
                                        {
                                            //Debug.Log("Break");
                                            GUI.enabled = false;
                                            GUILayout.Button("", GUILayout.Height(4));
                                            GUI.enabled = true;
                                        }
                                        #endregion

                                        #region Optional Parameters
                                        else if (n > nB)
                                        {
                                            //Debug.Log("OPT");

                                            #region Header
                                            if (n == nB + 1)
                                            {
                                                //Debug.Log("OptParams");
                                                GUILayout.Box("Optional Parameters");
                                            }
                                            #endregion

                                            #region Parameters

                                            #region !GUILayoutOption Types
                                            if (displayParams[n].GetType() != typeof(GUILayoutOption[]))
                                            {
                                                //Debug.Log("!GLO Type");

                                                #region !Array Type
                                                if (displayParams[n].GetType() != typeof(List<>) && displayParams[n].GetType() != typeof(Array))
                                                {
                                                    object o = displayParams[n];
                                                    GUI.skin.box.stretchWidth = true;
                                                    GUILayout.Box(o.ToString());
                                                    GUI.skin.box.stretchWidth = false;

                                                    #region Vector 2
                                                    if (o.GetType() == typeof(Vector2))
                                                    {
                                                        Vector2 v = (Vector2)o;
                                                        v = EditorGUILayout.Vector2Field("", v);
                                                        o = v;
                                                    }
                                                    #endregion

                                                    #region Bool
                                                    if (o.GetType() == typeof(bool))
                                                    {
                                                        bool b = (bool)o;
                                                        b = GUILayout.Toggle(b, "");
                                                        o = b;
                                                    }
                                                    #endregion

                                                    #region Int
                                                    if (o.GetType() == typeof(int))
                                                    {
                                                        int i = (int)o;
                                                        i = EditorGUILayout.IntField(i);
                                                        o = i;
                                                    }
                                                    #endregion

                                                    #region String
                                                    if (o.GetType() == typeof(string))
                                                    {
                                                        string s = (string)o;
                                                        s = EditorGUILayout.TextField(s);
                                                        o = s;
                                                    }
                                                    #endregion

                                                    #region Float
                                                    if (o.GetType() == typeof(float))
                                                    {
                                                        float f = (float)o;
                                                        f = EditorGUILayout.FloatField(f);
                                                        o = f;
                                                    }
                                                    #endregion

                                                    #region GUIContent
                                                    if (o.GetType() == typeof(GUIContent))
                                                    {
                                                        GUILayout.BeginVertical();

                                                        GUIContent gC = (GUIContent)o;

                                                        GUILayout.BeginHorizontal();
                                                        GUILayout.Label("Image");
                                                        gC.image = (Texture2D)EditorGUILayout.ObjectField(gC.image, typeof(Texture2D), true);
                                                        GUILayout.EndHorizontal();

                                                        GUILayout.BeginHorizontal();
                                                        GUILayout.Label("Text");
                                                        gC.text = GUILayout.TextField(gC.text, GUILayout.MinWidth(50));
                                                        GUILayout.EndHorizontal();

                                                        GUILayout.BeginHorizontal();
                                                        GUILayout.Label("Tooltip");
                                                        gC.tooltip = GUILayout.TextField(gC.tooltip, GUILayout.MinWidth(50));
                                                        GUILayout.EndHorizontal();

                                                        o = gC;

                                                        GUILayout.EndVertical();
                                                    }
                                                    #endregion

                                                    displayParams[n] = o;
                                                    //Debug.Log("End of iteration(!ArrayOpts)");
                                                }
                                                #endregion

                                                #region Array Type
                                                else
                                                {
                                                    object o = displayParams[n];

                                                    #region GUIContent

                                                    #region Prepare Array
                                                    List<GUIContent> lGC = (List<GUIContent>)o;

                                                    if (lGC.Count <= 0)
                                                    {
                                                        GUIContent gC0 = new GUIContent();
                                                        lGC.Add(gC0);
                                                    }
                                                    #endregion

                                                    GUILayout.BeginHorizontal();

                                                    #region Add/Remove Buttons
                                                    GUILayout.BeginVertical();

                                                    #region Add New Member Button
                                                    if (GUILayout.Button("+", GUILayout.MaxWidth(50)))
                                                    {
                                                        lGC.Add(new GUIContent());
                                                    }
                                                    #endregion

                                                    #region Remove Member Button
                                                    if (GUILayout.Button("-", GUILayout.MaxWidth(50)))
                                                    {
                                                        if (lGC.Count > 1)
                                                        {
                                                            lGC.Remove(lGC[lGC.Count - 1]);
                                                        }
                                                        else
                                                        {
                                                            lGC[0] = new GUIContent();
                                                        }
                                                    }
                                                    #endregion

                                                    GUILayout.EndVertical();
                                                    #endregion

                                                    #region Display Array Members
                                                    GUILayout.BeginVertical();

                                                    for (int u = 0; u <= lGC.Count - 1; u++)
                                                    {
                                                        GUIContent gCon = lGC[n];

                                                        #region Display GUIContent
                                                        GUILayout.BeginVertical();

                                                        GUILayout.BeginHorizontal();
                                                        GUILayout.Label("Image");
                                                        gCon.image = (Texture2D)EditorGUILayout.ObjectField(gCon.image, typeof(Texture2D), true);
                                                        GUILayout.EndHorizontal();

                                                        GUILayout.BeginHorizontal();
                                                        GUILayout.Label("Text");
                                                        gCon.text = GUILayout.TextField(gCon.text, GUILayout.MinWidth(50));
                                                        GUILayout.EndHorizontal();

                                                        GUILayout.BeginHorizontal();
                                                        GUILayout.Label("Tooltip");
                                                        gCon.tooltip = GUILayout.TextField(gCon.tooltip, GUILayout.MinWidth(50));
                                                        GUILayout.EndHorizontal();

                                                        lGC[n] = gCon;

                                                        GUILayout.EndVertical();
                                                        #endregion

                                                    }

                                                    GUILayout.EndVertical();
                                                    #endregion

                                                    GUILayout.EndHorizontal();

                                                    #region Passback To Parameter List
                                                    List<GUIContent> lGUI = new List<GUIContent>();
                                                    for (int u = 0; u <= lGC.Count - 1; u++)
                                                    {
                                                        if (lGC[u].image != null || lGC[u].text != "" || lGC[u].tooltip != "")
                                                        {
                                                            lGUI.Add(lGC[u]);
                                                        }
                                                        else
                                                        {
                                                            lGUI.Add(null);
                                                        }
                                                    }

                                                    for (int u = 0; u <= lGUI.Count - 1; u++)
                                                    {
                                                        if (lGUI[u] == null)
                                                        {
                                                            lGUI.Remove(lGUI[u]);
                                                        }
                                                    }
                                                    o = lGUI.ToArray();

                                                    displayParams[n] = o;
                                                    #endregion

                                                    #endregion

                                                    Debug.Log("End of iteration(ArrayOpts)");
                                                }
                                                #endregion
                                            }
                                            #endregion

                                            #region GUILayoutOptions
                                            else
                                            {
                                                //Debug.Log("GLO Type");

                                                #region Define And Prepare Arrays
                                                if (lM == null || lMP == null)
                                                {
                                                    GUILayoutOption[] aGO = displayParams[n] as GUILayoutOption[];
                                                    lM = new List<MethodInfo>();
                                                    lMP = new List<object>();
                                                }
                                                #endregion

                                                GUILayout.BeginHorizontal();

                                                #region Add/Remove Buttons
                                                GUILayout.BeginVertical();

                                                #region Add Button
                                                if (GUILayout.Button("+", GUILayout.MaxWidth(50)))
                                                {
                                                    lMP.Add(new Vector2());
                                                    lM.Add(null);
                                                }
                                                #endregion

                                                #region Remove Button
                                                if (GUILayout.Button("-", GUILayout.MaxWidth(50)))
                                                {
                                                    if (lM.Count > 1)
                                                    {
                                                        lM.Remove(lM[lM.Count - 1]);
                                                        lMP.Remove(lMP[lMP.Count - 1]);
                                                    }
                                                    else
                                                    {
                                                        lM = new List<MethodInfo>();
                                                        lMP = new List<object>();
                                                    }
                                                }
                                                #endregion

                                                GUILayout.EndVertical();
                                                #endregion

                                                #region Display Array Members
                                                GUILayout.BeginVertical();
                                                Debug.Log("LMP C:" + lMP.Count);
                                                for (int u = 0; u <= lMP.Count - 1; u++)
                                                {
                                                    Debug.Log("LMP[" + u + "]");
                                                    #region Create New Layout Option
                                                    #region Select New Layout Option
                                                    if (layoutChosen < 0)
                                                    {
                                                        Debug.Log("Select New Layout Option");
                                                        layoutChosen = GUILayout.SelectionGrid(layoutChosen, layouts, 2);
                                                    }
                                                    #endregion

                                                    #region Add Parameters To Selected Layout Option
                                                    else
                                                    {
                                                        Debug.Log("Selected Layout Option");

                                                        #region Functional Back Button
                                                        if (layoutChosen >= 0)
                                                        {
                                                            if (GUILayout.Button(layouts[layoutChosen]))
                                                            {
                                                                layoutChosen = -1;
                                                                if (lMP[u] != null)
                                                                {
                                                                    lMP[u] = null;
                                                                }
                                                            }

                                                            if (lM[u] == null)
                                                            {
                                                                lM[u] = layoutInvoke[layoutChosen];
                                                            }
                                                        }
                                                        #endregion

                                                        #region Display Parameters
                                                        GUILayout.BeginHorizontal();

                                                        #region Float Type
                                                        if (layoutChosen >= 0 && layoutChosen < 6)
                                                        {
                                                            #region Set To Float
                                                            if (lMP.Count < u)
                                                            {
                                                                Debug.Log("set to float by addition");
                                                                lMP.Add(new float());
                                                            }
                                                            else if (lMP[u].GetType() != typeof(float))
                                                            {
                                                                Debug.Log("Set to float from null");
                                                                lMP[u] = new float();
                                                            }
                                                            #endregion

                                                            GUILayout.BeginVertical();
                                                            GUILayout.Label(layouts[layoutChosen]);
                                                            GUILayout.BeginHorizontal();
                                                            GUILayout.Label("Float");
                                                            lMP[u] = EditorGUILayout.FloatField((float)lMP[u], GUILayout.MinWidth(50));
                                                            GUILayout.EndHorizontal();
                                                            GUILayout.EndVertical();
                                                        }
                                                        #endregion

                                                        #region Bool Type
                                                        if (layoutChosen >= 6)
                                                        {
                                                            #region Set To Bool
                                                            if (lMP.Count < u)
                                                            {
                                                                Debug.Log("Set to bool by addition");
                                                                lMP.Add(new bool());
                                                            }
                                                            else if (lMP[u].GetType() != typeof(bool))
                                                            {
                                                                Debug.Log("Set to bool from null");
                                                                lMP[u] = new bool();
                                                            }
                                                            #endregion

                                                            GUILayout.BeginVertical();
                                                            GUILayout.Label(layouts[layoutChosen]);
                                                            GUILayout.BeginHorizontal();
                                                            GUILayout.Label("Bool");
                                                            lMP[u] = EditorGUILayout.Toggle((bool)lMP[u]);
                                                            GUILayout.EndHorizontal();
                                                            GUILayout.EndVertical();
                                                        }
                                                        #endregion

                                                        GUILayout.EndHorizontal();
                                                        #endregion
                                                    }
                                                    #endregion

                                                    #endregion

                                                    #region Display Existing Layout Option

                                                    #region Selection Button
                                                    if (GUILayout.Button(lM[u].Name))
                                                    {
                                                        Debug.Log("This would show the already chosen parameters of this Layout Option");
                                                    }
                                                    #endregion

                                                    #endregion

                                                    #region Passback To Parameter List
                                                    List<GUILayoutOption> lGO = new List<GUILayoutOption>();
                                                    lGO[u] = (GUILayoutOption)lM[u].Invoke(this, lMP.ToArray());
                                                    displayParams[n] = lGO.ToArray();
                                                    #endregion

                                                }
                                                GUILayout.EndVertical();
                                                #endregion

                                                GUILayout.EndHorizontal();
                                            }
                                            #endregion

                                            #endregion
                                        }
                                        #endregion
                                    }
                                    #endregion

                                    GUI.skin.textArea.stretchWidth = false;
                                    GUI.skin.textField.stretchWidth = false;
                                    GUI.skin.box.stretchWidth = false;
                                }
                                #endregion

                                #region Has No Required Parameters
                                else
                                {
                                    Debug.Log("Has no required parameters");
                                    for (int n = 0; n <= displayParams.Count - 1; n++)
                                    {
                                        if (displayParams[n].GetType() != typeof(GUIStyle) && displayParams[n].GetType() != typeof(GUI.ToolbarButtonSize))
                                        {
                                            GUILayout.Box("Optional Parameters");
                                            if (n > displayParams.IndexOf(null))
                                            {
                                                #region !GUILayoutOption Types

                                                Type t = displayParams[n].GetType();
                                                GUI.skin.box.stretchWidth = true;
                                                GUILayout.Box(t.ToString());
                                                GUI.skin.box.stretchWidth = false;

                                                #region Vector 2
                                                if (t == typeof(Vector2))
                                                {
                                                    displayParams[n] = EditorGUILayout.Vector2Field("", (Vector2)displayParams[n]);
                                                }
                                                #endregion

                                                #region Bool
                                                if (t == typeof(bool))
                                                {
                                                    displayParams[n] = GUILayout.Toggle((bool)displayParams[n], "");
                                                }
                                                #endregion

                                                #region Int
                                                if (t == typeof(int))
                                                {
                                                    displayParams[n] = EditorGUILayout.IntField((int)displayParams[n]);
                                                }
                                                #endregion

                                                #region String
                                                if (t == typeof(string))
                                                {
                                                    displayParams[n] = EditorGUILayout.TextField((string)displayParams[n]);
                                                }
                                                #endregion

                                                #region Float
                                                if (t == typeof(float))
                                                {
                                                    displayParams[n] = EditorGUILayout.FloatField((float)displayParams[n]);
                                                }
                                                #endregion

                                                #region GUIContent
                                                if (t == typeof(GUIContent))
                                                {
                                                    GUILayout.BeginVertical();

                                                    GUIContent gC = (GUIContent)displayParams[n];

                                                    GUILayout.BeginHorizontal();
                                                    GUILayout.Label("Image");
                                                    gC.image = (Texture2D)EditorGUILayout.ObjectField(gC.image, typeof(Texture2D), true);
                                                    GUILayout.EndHorizontal();

                                                    GUILayout.BeginHorizontal();
                                                    GUILayout.Label("Text");
                                                    gC.text = GUILayout.TextField(gC.text, GUILayout.MinWidth(50));
                                                    GUILayout.EndHorizontal();

                                                    GUILayout.BeginHorizontal();
                                                    GUILayout.Label("Tooltip");
                                                    gC.tooltip = GUILayout.TextField(gC.tooltip, GUILayout.MinWidth(50));
                                                    GUILayout.EndHorizontal();

                                                    displayParams[n] = gC;

                                                    GUILayout.EndVertical();
                                                }
                                                #endregion

                                                #endregion

                                                #region GUILayoutOptions
                                                if (t == typeof(GUILayoutOption[]))
                                                {
                                                    Debug.Log("LayoutOptions not null");
                                                    List<GUILayoutOption> lGO = displayParams[n] as List<GUILayoutOption>;
                                                    List<MethodInfo> lM = new List<MethodInfo>();
                                                    List<object> lMP = new List<object>();

                                                    #region Prepare Arrays
                                                    if (lGO.Count <= 0)
                                                    {
                                                        lGO.Add(null);
                                                    }

                                                    if (lM.Count != lGO.Count)
                                                    {
                                                        lM.Add(null);
                                                    }

                                                    if (lMP.Count != lM.Count)
                                                    {
                                                        lMP.Add(null);
                                                    }
                                                    #endregion

                                                    GUILayout.BeginHorizontal();

                                                    #region Add/Remove Buttons
                                                    GUILayout.BeginVertical();

                                                    #region Add Button
                                                    if (GUILayout.Button("+", GUILayout.MaxWidth(50)))
                                                    {
                                                        lM.Add(null);
                                                        lMP.Add(null);
                                                    }
                                                    #endregion

                                                    #region Remove Button
                                                    if (GUILayout.Button("-", GUILayout.MaxWidth(50)))
                                                    {
                                                        if (lM.Count > 1)
                                                        {
                                                            lM.Remove(lM[lM.Count - 1]);
                                                            lMP.Remove(lMP[lMP.Count - 1]);
                                                        }
                                                        else
                                                        {
                                                            lM[0] = null;
                                                            lMP[0] = null;
                                                        }
                                                    }
                                                    #endregion

                                                    GUILayout.EndVertical();
                                                    #endregion

                                                    #region Display Array Members
                                                    GUILayout.BeginVertical();
                                                    for (int u = 0; u <= lGO.Count - 1; u++)
                                                    {
                                                        #region Create New Layout Option
                                                        if (lGO[u] == null)
                                                        {
                                                            #region Select New Layout Option
                                                            if (layoutChosen < 0)
                                                            {
                                                                Debug.Log("Select New Layout Option");
                                                                layoutChosen = GUILayout.SelectionGrid(layoutChosen, layouts, 2);
                                                            }
                                                            #endregion

                                                            #region Add Parameters To Selected Layout Option
                                                            else
                                                            {
                                                                Debug.Log("Selected a LayoutOption");

                                                                #region Functional Back Button
                                                                if (layoutChosen >= 0)
                                                                {
                                                                    if (GUILayout.Button(layouts[layoutChosen]))
                                                                    {
                                                                        layoutChosen = -1;
                                                                        if (lMP[u] != null)
                                                                        {
                                                                            lMP[u] = null;
                                                                        }
                                                                    }

                                                                    if (lM[u] == null)
                                                                    {
                                                                        lM[u] = layoutInvoke[layoutChosen];
                                                                    }
                                                                }
                                                                #endregion

                                                                #region Display Parameters
                                                                GUILayout.BeginHorizontal();

                                                                #region Float Type
                                                                if (layoutChosen >= 0 && layoutChosen < 6)
                                                                {
                                                                    if (lMP[u].GetType() != typeof(float))
                                                                    {
                                                                        Debug.Log("Set to float[!REQ]");
                                                                        lMP[u] = new float();
                                                                    }
                                                                    GUILayout.BeginVertical();
                                                                    GUILayout.Label(layouts[layoutChosen]);
                                                                    GUILayout.Label("Float");
                                                                    lMP[u] = EditorGUILayout.FloatField((float)lMP[u], GUILayout.MinWidth(50));
                                                                    GUILayout.EndVertical();
                                                                }
                                                                #endregion

                                                                #region Bool Type
                                                                if (layoutChosen >= 6)
                                                                {
                                                                    if (lMP[u].GetType() != typeof(bool))
                                                                    {
                                                                        Debug.Log("Set to bool[!REQ]");
                                                                        lMP[u] = new bool();
                                                                    }
                                                                    GUILayout.BeginVertical();
                                                                    GUILayout.Label(layouts[layoutChosen]);
                                                                    GUILayout.Label("Bool");
                                                                    lMP[u] = EditorGUILayout.Toggle((bool)lMP[u]);
                                                                    GUILayout.EndVertical();
                                                                }
                                                                #endregion

                                                                GUILayout.EndHorizontal();
                                                                #endregion
                                                            }
                                                            #endregion
                                                        }
                                                        #endregion

                                                        #region Display Existing Layout Option

                                                        #region Selection Button
                                                        if (GUILayout.Button(lM[u].Name))
                                                        {
                                                            Debug.Log("This would show the already chosen parameters of this Layout Option");
                                                        }
                                                        #endregion

                                                        #endregion

                                                        #region Passback To Parameter List
                                                        lGO[u] = (GUILayoutOption)lM[u].Invoke(this, lMP.ToArray());
                                                        displayParams[n] = lGO.ToArray();
                                                        #endregion

                                                    }
                                                    GUILayout.EndVertical();
                                                    #endregion

                                                    GUILayout.EndHorizontal();
                                                }
                                                #endregion

                                                else
                                                {
                                                    Debug.Log("LayoutOptions are null");
                                                }
                                            }
                                        }
                                    }
                                }
                                #endregion
                            }
                        }
                        #endregion
                    }
                    #endregion

                    GUILayout.EndVertical();
                }

                #region Modify Existing Object
                else
                {

                }
                #endregion

                #endregion
            }
            #endregion

            #endregion

            GUILayout.EndVertical();
            #endregion

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            #endregion
        }
        #endregion
    }
    #endregion

    #region Custom Methods

    #region Get Layer Ints From Existing Object
    private int[] getLayerInts(MethodInfo invk)
    {
        int[] i = new int[2];
        i[0] = -1;
        i[1] = -1;

        #region Layer 0 Check
        for (int n = 0; n <= innerlayer0.Length - 1; n++)
        {
            if (invk.Name == innerlayer0[n])
            {
                i[0] = 0;
                i[1] = n;
                break;
            }
        }
        #endregion

        #region Layer 1 Check
        if (i[1] < 0)
        {
            for (int n = 0; n <= innerlayer1.Length - 1; n++)
            {
                if (invk.Name == innerlayer1[n])
                {
                    i[0] = 1;
                    i[1] = n;
                    break;
                }
            }
        }
        #endregion

        #region Layer 2 Check
        if (i[1] < 0)
        {
            for (int n = 0; n <= innerlayer2.Length - 1; n++)
            {
                if (invk.Name == innerlayer2[n])
                {
                    i[0] = 2;
                    i[1] = n;
                    break;
                }
            }
        }
        #endregion

        return i;
    }
    #endregion

    #region Debug Parameters Types
    public void debugParametersTypes(List<object> parms)
    {
        Debug.Log("Called Debug Parameters Types");
        for(int n = 0; n <= parms.Count - 1; n++)
        {
            Debug.Log("parameters[" + n + "], " + parms[n].GetType());
        }
    }
    #endregion

    #region Get New Instance
    public object newInstance(Type t)
    {
        object o = Activator.CreateInstance(t);
        return o;
    }
    #endregion

    #endregion

    #region Invokable Methods

    #region Get Parameters Of A Method (layer ints)
    List<object> getParameters(int outLayer, int inLayer)
    {
        List<object> pOut = new List<object>();
        //Debug.Log("called. oL:" + outLayer + ", iL:" + inLayer);
        //return format is as follows
        //has required params - [required params], null, [optional params]
        //no required params - null, [optional params]

        #region Enclosers [0]
        if (outLayer == 0)
        {
            ParameterInfo[] p = l0Invoke[inLayer].GetParameters();
            //Debug.Log("oL:0, " + "pL:" + p.Length);
            for (int n = 0; n <= p.Length - 1; n++)
            {
                //Debug.Log("////");
                //Debug.Log("[" + n + "]pT:" + p[n].ParameterType);
                object o;
                if (p[n].ParameterType != typeof(GUILayoutOption[]))
                {
                    o = newInstance(p[n].ParameterType);
                }
                else
                {
                    o = new GUILayoutOption[0];
                }
                //Debug.Log("[" + n + "]o:" + o);
                //Debug.Log("o T:" + o.GetType());
                //Debug.Log("o T TS:" + o.GetType().ToString());
                pOut.Add(o);
            }

            #region Begin Horizontal or Vertical [0, 2]
            if (inLayer == 0 || inLayer == 2)
            {
                pOut.Insert(0, null);
            }
            #endregion

            #region Begin Scrollview [4]
            if (inLayer == 4)
            {
                pOut.Insert(1, null);
            }
            #endregion
            
            #region Methods With No Parameters [1, 3, 5]
            if (inLayer == 1 || inLayer == 3 || inLayer == 5)
            {
                pOut.Insert(0, null);
            }
            #endregion
        }
        #endregion

        #region Controls [1]
        if (outLayer == 1)
        {
            ParameterInfo[] p = l1Invoke[inLayer].GetParameters();
            //Debug.Log("oL:1, " + "pL:" + p.Length);
            for (int n = 0; n <= p.Length - 1; n++)
            {
                //Debug.Log("////");
                //Debug.Log("[" + n + "]pT:" + p[n].ParameterType);
                object o;
                if (p[n].ParameterType != typeof(GUILayoutOption[]))
                {
                    o = newInstance(p[n].ParameterType);
                }
                else
                {
                    o = new GUILayoutOption[0];
                }
                //Debug.Log("[" + n + "]o:" + o);
                //Debug.Log("o T:" + o.GetType());
                //Debug.Log("o T TS:" + o.GetType().ToString());
                pOut.Add(o);
            }

            #region Button [0]
            if (inLayer == 0)
            {
                pOut.Insert(1, null);
            }
            #endregion

            #region Text Field or Area [1/2]
            if (inLayer == 1 || inLayer == 2)
            {
                pOut.Insert(1, null);
            }
            #endregion

            #region Toggle [3]
            if (inLayer == 3)
            {
                pOut.Insert(2, null);
            }
            #endregion

            #region Toolbar [4]
            if (inLayer == 4)
            {
                pOut.Insert(2, null);
            }
            #endregion

            #region Selection Grid [5]
            if (inLayer == 5)
            {
                pOut.Insert(3, null);
            }
            #endregion

            #region Horizontal or Vertical Scrollbar [6/7]
            if (inLayer == 6 || inLayer == 7)
            {
                pOut.Insert(4, null);
            }
            #endregion

            //Debug.Log("Get Parameters. C:" + pOut.Count);
        }
        #endregion

        #region Displays [2]
        if (outLayer == 2)
        {
            ParameterInfo[] p = l2Invoke[inLayer].GetParameters();
            //Debug.Log("oL:2, " + "pL:" + p.Length);
            for (int n = 0; n <= p.Length - 1; n++)
            {
                //Debug.Log("////");
                //Debug.Log("[" + n + "]pT:" + p[n].ParameterType);
                object o;
                if (p[n].ParameterType != typeof(GUILayoutOption[]))
                {
                    o = newInstance(p[n].ParameterType);
                }
                else
                {
                    o = new GUILayoutOption[0];
                }
                //Debug.Log("[" + n + "]o:" + o);
                //Debug.Log("o T:" + o.GetType());
                //Debug.Log("o T TS:" + o.GetType().ToString());
                pOut.Add(o);
            }

            #region Label [0]
            if (inLayer == 0)
            {
                pOut.Insert(1, null);
            }
            #endregion

            #region Box [1]
            if (inLayer == 1)
            {
                pOut.Insert(1, null);
            }
            #endregion

            #region Space [2]
            if (inLayer == 2)
            {
                pOut.Insert(1, null);
            }
            #endregion

            #region FlexibleSpace [3]
            if (inLayer == 3)
            {
                pOut.Insert(0, null);
            }
            #endregion
        }
        #endregion

        //Debug.Log("Get Parameters. C:" + pOut.Count);
        return pOut;
    }
    #endregion

    #region Get Parameters Of A Method (method name)
    List<object> getParameters(string name)
    {
        List<object> pOut = new List<object>();
        int outLayer = -1;
        int inLayer = -1;

        #region Get Layer Ints

        #region Layer 0
        for (int n = 0; n <= innerlayer0.Length - 1; n++)
        {
            if (innerlayer0[n] == name)
            {
                inLayer = n;
                outLayer = 0;
                break;
            }
        }
        #endregion

        #region Layer 1
        if (outLayer < 0)
        {
            for(int n = 0; n <= innerlayer1.Length - 1; n++)
            {
                if (innerlayer1[n] == name)
                {
                    inLayer = n;
                    outLayer = 1;
                }
            }
        }
        #endregion

        #region Layer 2
        if (outLayer < 0)
        {
            for (int n = 0; n <= innerlayer2.Length - 1; n++)
            {
                if (innerlayer2[n] == name)
                {
                    inLayer = n;
                    outLayer = 2;
                }
            }
        }
        #endregion

        #endregion

        //return format is as follows
        //has required params - [required params], null, [optional params]
        //no required params - null, [optional params]

        #region Enclosers [0]
        if (outLayer == 0)
        {
            #region Begin Horizontal or Vertical [0, 2]
            if (inLayer == 0 || inLayer == 2)
            {
                pOut.Add(null);
                ParameterInfo[] p = l0Invoke[inLayer].GetParameters();
                for (int n = 0; n <= p.Length - 1; n++)
                {
                    object o = p[n];
                    pOut.Add(o);
                }
            }
            #endregion

            #region Begin Scrollview [4]
            if (inLayer == 4)
            {
                ParameterInfo[] p = l0Invoke[inLayer].GetParameters();
                for (int n = 0; n <= p.Length - 1; n++)
                {
                    object o = p[n];
                    pOut.Add(o);
                }
                pOut.Insert(1, null);
            }
            #endregion

            #region Methods With No Parameters [1, 3, 5]
            if (inLayer == 1 || inLayer == 3 || inLayer == 5)
            {
                pOut.Add(null);
            }
            #endregion
        }
        #endregion

        #region Controls [1]
        if (outLayer == 1)
        {
            #region Button [0]
            if (inLayer == 0)
            {
                ParameterInfo[] p = l1Invoke[inLayer].GetParameters();
                for (int n = 0; n <= p.Length - 1; n++)
                {
                    object o = p[n];
                    pOut.Add(o);
                }
                pOut.Insert(1, null);
            }
            #endregion

            #region Text Field or Area [1/2]
            if (inLayer == 1 || inLayer == 2)
            {
                ParameterInfo[] p = l1Invoke[inLayer].GetParameters();
                for (int n = 0; n <= p.Length - 1; n++)
                {
                    object o = p[n];
                    pOut.Add(o);
                }
                pOut.Insert(1, null);
            }
            #endregion

            #region Toggle [3]
            if (inLayer == 3)
            {
                ParameterInfo[] p = l1Invoke[inLayer].GetParameters();
                for (int n = 0; n <= p.Length - 1; n++)
                {
                    object o = p[n];
                    pOut.Add(o);
                }
                pOut.Insert(2, null);
            }
            #endregion

            #region Toolbar [4]
            if (inLayer == 4)
            {
                ParameterInfo[] p = l1Invoke[inLayer].GetParameters();
                for (int n = 0; n <= p.Length - 1; n++)
                {
                    object o = p[n];
                    pOut.Add(o);
                }
                pOut.Insert(2, null);
            }
            #endregion

            #region Selection Grid [5]
            if (inLayer == 5)
            {
                ParameterInfo[] p = l1Invoke[inLayer].GetParameters();
                for (int n = 0; n <= p.Length - 1; n++)
                {
                    object o = p[n];
                    pOut.Add(o);
                }
                pOut.Insert(3, null);
            }
            #endregion

            #region Horizontal or Vertical Scrollbar [6/7]
            if (inLayer == 6 || inLayer == 7)
            {
                ParameterInfo[] p = l1Invoke[inLayer].GetParameters();
                for (int n = 0; n <= p.Length - 1; n++)
                {
                    object o = p[n];
                    pOut.Add(o);
                }
                pOut.Insert(4, null);
            }
            #endregion

        }
        #endregion

        #region Displays [2]
        if (outLayer == 2)
        {
            #region Label [0]
            if (inLayer == 0)
            {
                ParameterInfo[] p = l2Invoke[inLayer].GetParameters();
                for (int n = 0; n <= p.Length - 1; n++)
                {
                    object o = p[n];
                    pOut.Add(o);
                }
                pOut.Insert(1, null);
            }
            #endregion

            #region Box [1]
            if (inLayer == 1)
            {
                ParameterInfo[] p = l2Invoke[inLayer].GetParameters();
                for (int n = 0; n <= p.Length - 1; n++)
                {
                    object o = p[n];
                    pOut.Add(o);
                }
                pOut.Insert(1, null);
            }
            #endregion

            #region Space [2]
            if (inLayer == 2)
            {
                ParameterInfo[] p = l2Invoke[inLayer].GetParameters();
                for (int n = 0; n <= p.Length - 1; n++)
                {
                    object o = p[n];
                    pOut.Add(o);
                }
                pOut.Add(null);
            }
            #endregion

            #region FlexibleSpace [3]
            if (inLayer == 3)
            {
                pOut.Add(null);
            }
            #endregion
        }
        #endregion

        return pOut;
    }
    #endregion
    
    #endregion
}
#endregion
