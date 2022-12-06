#region Libraries
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Collections.Specialized;
using Verruckt.Operations;
using Verruckt.Editor;
#endregion

#region Main Class
[ExecuteInEditMode]
[CustomEditor(typeof(ScriptableObject))]
[CanEditMultipleObjects]
public class VrrktEditorVersion3 : EditorWindow
{
    #region Variables

    #region Header Variables
    int pX, pY, coRs, tsKs;
    #endregion

    Vector2 pos;

    #region General Variables
    int pageChosen, phase, step, current, userCount;
    string[] pageButtons, fNames;
    #endregion

    List<vrrktGIDTable> tables;
    List<int> limits;
    List<int> currents;
    List<Vector2Int> ranges;
    List<VrrktUserDatabase> lDbc;
    List<v3DatabaseCreator> dbCreators;
    #endregion

    #region On Enable
    private void OnEnable()
    {
        pX = 0;
        pY = 0;
        coRs = 0;
        tsKs = 0;
        userCount = 0;
        current = 0;
        step = 0;
        phase = 0;
        pos = Vector2.zero;
        pageButtons = new string[] { "Container Operations", "Central Database", "Build User Files", "Self Install Operations", "View Logs", "File Viewer"};
        pageChosen = -1;
        tables = null;
        limits = null;
        currents = null;
        ranges = null;
        lDbc = null;
        dbCreators = null;
        fNames = null;
    }
    #endregion
    
    #region INIT
    [MenuItem("Window/Custom/VrrktEditor V3")]
    static void Init()
    {
        Debug.Log("Init");
        VrrktEditorVersion3 vE = (VrrktEditorVersion3)GetWindow(typeof(VrrktEditorVersion3));
        vE.maxSize = new Vector2(Screen.width, Screen.height);
        EditorUtility.SetDirty(vE);
        vE.Show();
    }
    #endregion

    #region GUI
    private void OnGUI()
    {
        pos = GUILayout.BeginScrollView(pos);

        #region Header
        GUI.skin.box.stretchWidth = true;
        GUI.skin.button.stretchWidth = true;

        #region Upper Header
        GUILayout.BeginHorizontal();

        GUILayout.Box("Page[" + pX + ", " + pY + "]");
        GUILayout.Box("Coroutines: " + coRs);
        GUILayout.Box("Tasks: " + tsKs);

        GUILayout.EndHorizontal();
        #endregion

        #region Lower Header
        GUILayout.BeginHorizontal();

        #region Reload Window Button
        if (GUILayout.Button("Reload Window"))
        {
            Init();
            OnEnable();
        }
        #endregion

        #region End Coroutines Button XXX
        if (GUILayout.Button("End Coroutines"))
        {

        }
        #endregion

        #region End Tasks Button XXX
        if (GUILayout.Button("End Tasks"))
        {

        }
        #endregion

        GUILayout.EndHorizontal();
        #endregion

        GUI.skin.box.stretchWidth = false;
        GUI.skin.button.stretchWidth = false;
        #endregion

        #region Info Windows XXXX

        #endregion

        #region Automation Window XXXX

        #endregion

        #region Pages

        #region Main Page [0,0]
        if (pX == 0 && pY == 0)
        {
            pageChosen = GUILayout.SelectionGrid(pageChosen, pageButtons, 3);

            #region Control Logic
            if (pageChosen != -1)
            {
                #region Container Operations
                if (pageChosen == 0)
                {
                    pX = 1;
                    pY = 0;
                    pageChosen = -1;
                    pageButtons = new string[] { "Create GID Tables", "Create Card Databases", "Create Image Databases" };
                }
                #endregion

                #region Central Database XXX
                if (pageChosen == 1)
                {

                }
                #endregion

                #region Build User Files XXX
                if (pageChosen == 2)
                {

                }
                #endregion

                #region Self Install Operations XXX
                if (pageChosen == 3)
                {

                }
                #endregion

                #region View Logs XXX
                if (pageChosen == 4)
                {

                }
                #endregion

                #region File Viewer XXX
                if (pageChosen == 5)
                {

                }
                #endregion
            }
            #endregion
        }
        #endregion

        #region Container Operations [1,0] XXXX
        if (pX == 1)
        {
            #region Main Page [1, 0]
            if (pY == 0)
            {
                #region Control Display
                pageChosen = GUILayout.SelectionGrid(pageChosen, pageButtons, pageButtons.Length);
                #endregion

                #region Control Logic
                if (pageChosen != -1)
                {
                    phase = 0;

                    #region Create GID Tables
                    if (pageChosen == 0)
                    {
                        userCount = 0;
                        pY = 1;
                    }
                    #endregion

                    #region Create Card Databases
                    if (pageChosen == 1)
                    {
                        phase = 0;
                        pY = 2;
                    }
                    #endregion

                    #region Create Image Databases
                    if (pageChosen == 2)
                    {
                        phase = 0;
                        pY = 3;
                    }
                    #endregion

                    pageChosen = -1;
                }
                #endregion
            }
            #endregion

            #region Create GID Tables Page [1, 1] XXXX
            if (pY == 1)
            {
                #region Begin [0]
                if (phase == 0)
                {
                    #region User Count Entry
                    userCount = EditorGUILayout.IntField(userCount);
                    #endregion

                    #region Begin Button
                    if (GUILayout.Button("Begin Creation"))
                    {
                        if (userCount > 0)
                        {
                            tables = new List<vrrktGIDTable>();
                            for (int n = 0; n <= userCount - 1; n++)
                            {
                                tables.Add(new vrrktGIDTable());
                            }
                            //Debug.Log("Begin Creation. T C: " + tables.Count);
                            //gidTableCreateControl(userCount);
                            phase = 1;
                        }
                        else
                        {
                            Debug.Log("User Count Must Be Greater Than 0!");
                        }
                    }
                    #endregion
                }
                #endregion

                #region Running [1]
                if (phase == 1)
                {
                    #region Progress Display
                    GUILayout.BeginVertical();

                    #region Upper Header Display
                    GUILayout.Box("Step: " + step, GUILayout.Width(Screen.width - 10));

                    if (fNames != null)
                    {
                        GUILayout.BeginHorizontal();

                        GUILayout.Box("Current: " + current, GUILayout.Width((Screen.width / 2) - 10));
                        GUILayout.Box("Limit: " + fNames.Length, GUILayout.Width((Screen.width / 2) - 10));

                        GUILayout.EndHorizontal();
                    }

                    #endregion

                    #region Unit Display
                    if (currents != null && currents.Count > 0)
                    {
                        for (int n = 0; n <= tables.Count - 1; n++)
                        {
                            GUILayout.BeginHorizontal();

                            GUILayout.Box("Unit Current: " + currents[n], GUILayout.Width((Screen.width / 2) - 10));
                            GUILayout.Box("Unit Limit: " + ((ranges[n].y - ranges[n].x) + 1), GUILayout.Width((Screen.width / 2) - 10));

                            GUILayout.EndHorizontal();
                        }
                    }
                    #endregion

                    GUILayout.EndVertical();
                    #endregion
                }
                #endregion
            }
            #endregion

            #region Create Card Databases Page [1, 2] XXXX
            if (pY == 2)
            {
                #region Begin [0]
                if (phase == 0)
                {
                    #region Begin Button
                    if (GUILayout.Button("Begin Creation"))
                    {
                        //cardDatabaseControl();
                        //implementCreators(true, 8);
                        phase = 1;
                    }
                    #endregion
                }
                #endregion

                #region Running [1]
                if (phase == 1)
                {
                    #region Progress Display
                    GUILayout.BeginVertical();

                    #region Internal Creation 
                    if (currents == null || currents.Count == 0 || currents[0] == 100000)
                    {
                        #region Upper Header
                        GUILayout.Box("Step: " + step, GUILayout.Width(Screen.width - 10));

                        if (tables != null)
                        {
                            GUILayout.BeginHorizontal();

                            for (int n = 0; n <= tables.Count - 1; n++)
                            {
                                GUILayout.Box("TC: " + tables[n].table.Count, GUILayout.Width((Screen.width / tables.Count) - 10));
                            }

                            GUILayout.EndHorizontal();
                        }
                        #endregion

                        if (currents != null && limits != null && currents.Count == limits.Count)
                        {
                            for (int n = 0; n <= currents.Count - 1; n++)
                            {
                                GUILayout.BeginHorizontal();

                                GUILayout.Box("Current: " + currents[n], GUILayout.Width((Screen.width / 3) - 10));
                                GUILayout.Box("Limit: " + limits[n], GUILayout.Width((Screen.width / 3) - 10));

                                if (lDbc != null && lDbc.Count >= n)
                                {
                                    GUILayout.Box("Db C: " + lDbc[n].database.Count, GUILayout.Width((Screen.width / 3) - 10));
                                }
                                else
                                {
                                    GUILayout.Box("Db is currently null", GUILayout.Width((Screen.width / 3) - 10));
                                }

                                GUILayout.EndHorizontal();
                            }
                        }
                    }
                    #endregion

                    #region External Creation
                    else
                    {
                        if (dbCreators != null && dbCreators.Count > 0)
                        {
                            int c = 0;

                            for (int n = 0; n <= dbCreators.Count - 1; n++)
                            {
                                GUILayout.BeginHorizontal();

                                GUILayout.Box(dbCreators[n].user, GUILayout.Width((Screen.width / 5) - 10));
                                GUILayout.Box(dbCreators[n].operation, GUILayout.Width((Screen.width / 5) - 10));
                                GUILayout.Box(dbCreators[n].step, GUILayout.Width((Screen.width / 5) - 10));
                                GUILayout.Box(dbCreators[n].current + "", GUILayout.Width((Screen.width / 5) - 10));
                                GUILayout.Box(dbCreators[n].limit + "", GUILayout.Width((Screen.width / 5) - 10));

                                GUILayout.EndHorizontal();

                                if (dbCreators[n].done)
                                {
                                    c++;
                                }
                            }

                            if (c == dbCreators.Count)
                            {
                                dbCreators = null;
                                phase = 0;
                                pY = 0;
                            }
                        }
                    }
                    #endregion

                    GUILayout.EndVertical();
                    #endregion
                }
                #endregion
            }
            #endregion

            #region Create Image Databases Page [1, 3] XXXX
            if (pY == 3)
            {
                #region Begin [0]
                if (phase == 0)
                {
                    #region Begin Button
                    if (GUILayout.Button("Begin Creation"))
                    {
                        //implementCreators(false, 8);
                        //imageDatabaseControl();
                        phase = 1;
                    }
                    #endregion
                }
                #endregion

                #region Running [1]
                if (phase == 1)
                {
                    #region Progress Display
                    GUILayout.BeginVertical();

                    #region Internal Creation 
                    if (currents == null || currents.Count == 0 || currents[0] == 100000)
                    {
                        #region Upper Header
                        GUILayout.Box("Step: " + step, GUILayout.Width(Screen.width - 10));

                        if (tables != null)
                        {
                            GUILayout.BeginHorizontal();

                            for (int n = 0; n <= tables.Count - 1; n++)
                            {
                                GUILayout.Box("TC: " + tables[n].table.Count, GUILayout.Width((Screen.width / tables.Count) - 10));
                            }

                            GUILayout.EndHorizontal();
                        }
                        #endregion

                        if (currents != null && limits != null && currents.Count == limits.Count)
                        {
                            for (int n = 0; n <= currents.Count - 1; n++)
                            {
                                GUILayout.BeginHorizontal();

                                GUILayout.Box("Current: " + currents[n], GUILayout.Width((Screen.width / 3) - 10));
                                GUILayout.Box("Limit: " + limits[n], GUILayout.Width((Screen.width / 3) - 10));

                                if (lDbc != null && lDbc.Count >= n)
                                {
                                    GUILayout.Box("Db C: " + lDbc[n].database.Count, GUILayout.Width((Screen.width / 3) - 10));
                                }
                                else
                                {
                                    GUILayout.Box("Db is currently null", GUILayout.Width((Screen.width / 3) - 10));
                                }

                                GUILayout.EndHorizontal();
                            }
                        }
                    }
                    #endregion

                    #region External Creation
                    else
                    {
                        if (dbCreators != null && dbCreators.Count > 0)
                        {
                            int c = 0;

                            for (int n = 0; n <= dbCreators.Count - 1; n++)
                            {
                                GUILayout.BeginHorizontal();

                                GUILayout.Box(dbCreators[n].user, GUILayout.Width((Screen.width / 5) - 10));
                                GUILayout.Box(dbCreators[n].operation, GUILayout.Width((Screen.width / 5) - 10));
                                GUILayout.Box(dbCreators[n].step, GUILayout.Width((Screen.width / 5) - 10));
                                GUILayout.Box(dbCreators[n].current + "", GUILayout.Width((Screen.width / 5) - 10));
                                GUILayout.Box(dbCreators[n].limit + "", GUILayout.Width((Screen.width / 5) - 10));

                                GUILayout.EndHorizontal();

                                if (dbCreators[n].done)
                                {
                                    c++;
                                }
                            }

                            if (c == dbCreators.Count)
                            {
                                dbCreators = null;
                                phase = 0;
                                pY = 0;
                            }
                        }
                    }
                    #endregion

                    GUILayout.EndVertical();
                    #endregion
                }
                #endregion
            }
            #endregion
        }
        #endregion

        #endregion

        GUILayout.EndScrollView();
    }
    #endregion
}
#endregion
