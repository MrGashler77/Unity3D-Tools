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
public class vrrktEditorVersion2 : EditorWindow
{
    #region Variables
    FileStream f;

    #region Display Variables
    private Vector2 mainPos;
    private int pX, pY;

    #region Resources Info Variables
    VrrktOperationsData vOps;
    VrrktSystemData vSys;
    VrrktReport vRep;
    VrrktUser vUser;
    #endregion

    #region Notes Variables

    #endregion

    #region User Db Info
    List<VrrktUserDatabase> vUDb;
    #endregion

    #endregion

    #region Formatter Variables
    private VrrktFormatter vFormatter;
    private IEnumerator serialize, deserialize;
    private int tck, tLim, sTck, sTLim, pathChosen, fChosen, dChosen, deserControl, serControl, iAdd;
    private string customPath, directory, fileChosen;
    private string[] directories, files;
    private List<object> desReturned;
    private List<Type> desType, serType;
    private int serChosen;
    #endregion

    #region Page Specific Variables

    #region Main Page Variables [0,0]
    private string[] mainButtons = { "Container Operations", "Central Database Access", "Construct User Files", "Self Installation Operations", "Log View", "Custom Serialization Operations" };
    private int mainChosen;
    #endregion

    #region Container Operations Variables [1,0]
    private int contChosen;
    private int cDbPhase;
    private int dbUserCount;
    private int endC;

    #region Database Creation Variables
    string[] fNames;
    List<Vector2Int> ranges;
    List<vrrktGIDTable> tables;
    List<int> currents;
    List<int> limits;
    List<VrrktUserDatabase> lDbc;
    List<VrrktUserImageDatabase> lDbi;
    List<v3DatabaseCreator> dbCreators;
    string step;
    int current;
    #endregion

    #region Main Page [1, 0]
    private string[] cont1Buttons = { "Create GID Tables", "Create Card Databases", "Create Image Databases" };
    #endregion
    
    #endregion

    #region Central Database Access Variables [2,X]
    private string centralDatabasePathBase = "C:/Users/robzo/Desktop/VrrktCentralDatabase/Database/";
    private pullerDataContainer pullerContainer;
    private List<VrrktPullerUnit> pullers;
    private int totalProg;
    private int totalBlank;
    private bool totalDone;
    private DateTime startTime;
    private int repullTotal;
    private int clearProg;
    private List<int> bPL;
    private int bPpH;

    #region Database Main Page Variables [2,0]
    private string[] dB0Buttons = { "Create", "Access", "Integrity Check", "Duplicate Filter", "Clear", "Back" };
    private int dB0Chosen;
    #endregion

    #region Database View Selection Page Variables [2,1]
    private string[] dB1Buttons = { "View A Specific Card", "View A Random Card", "Back" };
    private int dB1Chosen;
    #endregion

    #region Database View Specific Card Input Page Variables [2,2]
    //variables to provide wide search parameters for a specific card
    private string[] dB2Buttons = { };
    private int dB2Chosen;
    #endregion

    #region View Card Page Variables [2,3]
    //display card from database
    private VrrktCard card;
    private VrrktCardImage cardImage;
    private bool displayCard;
    private string[] displayHeaders = { "GID", "Name", "Mana Cost", "Converted Mana Cost", "Types", "Card Text", "Flavor Text", "P/T", "Loyalty", "Color Indicator", "Set", "Rarity", "Artist" };
    private string[] c1Info;
    private string[] c2Info;
    private string[] dB3Buttons = { "Ok", "Home" };
    private int dB3Chosen;
    private Vector2[] dB3Pos;
    #endregion

    #region Puller Operation Selection Page Variables [2,4]
    private string[] dB4Buttons = { "Start Puller", "Resume Puller", "Run Image Puller", "Back" };
    private int dB4Chosen;
    #endregion

    #region Provide Puller Parameters Page Variables [2, 5] 
    private int pullerLimit;
    private int pullerUnits;
    private string[] dB5Buttons = { "Start From Beginning", "Resume Current Progress", "Back" };
    private int dB5Chosen;
    #endregion

    #region Resume Puller Display Parameters Page Variables [2,6]
    //Display parameters of the pullers that have already been running
    private string[] dB6Buttons = { "Resume", "Back" };
    private int dB6Chosen;
    #endregion

    #region Database Puller Running Page Variables [2, 7]
    private string[] pullerProgressButtons = { "Stop", "Save" };
    private int pullerProgressChosen;
    private Vector2 progressPos;
    #endregion

    #region Image Puller Operations Page [2, 8]
    private string[] dB8Buttons = { "Start Fresh", "Resume", "Back" };
    private int dB8Chosen;
    private List<int> iList;
    private List<int> iPullidx;
    private List<List<int>> iPullList;
    private List<List<Texture2D>> iPullImgs;
    private List<Vector2> iPullPos;
    #endregion

    #region Image Puller Start Display Page [2, 9]
    private string[] dB9Buttons = { "Begin", "Clear", "Back" };
    private int dB9Chosen;
    private int imgPullerLimit;
    private int imgPullersCount;
    #endregion

    #region Image Puller Resume Display Page [2, 10]
    private string[] dB10Buttons = { "Resume", "Back" };
    private int dB10Chosen;
    #endregion

    #region Image Puller Progress Page [2, 11]
    private bool stop;
    #endregion

    #region Clear Database Directory Progress Page [2, 12]
    private bool clearCards;
    private bool clearImages;
    private bool clearRunning;
    private string[] clearFiles;
    private int clearProgress;
    #endregion

    #region Single Puller Page [2, 13]
    private string[] db13Buttons = { "Card", "Image", "Back" };
    private int db13Chosen;
    #endregion

    #region Integrity Check Parameters Page [2, 14]
    private string[] db14Buttons = { "Run", "Single", "Clear", "Back" };
    private int db14Chosen;
    private int integrityCount;
    private int integrityLimit;
    #endregion

    #endregion

    #region Construct User Files Variables [3,X]
    private int uConChosen;

    #region Main Page Variables [3, 0]
    private string[] uConButtons0 = { "Resources", "Database" };
    #endregion

    #region System Resources Selection Variables [3, 1]
    private string[] uConButtons1 = { "VrrktSystemData", "VrrktOperations", "VrrktUser" };
    #endregion

    #region Construct VrrktSystemData Variables [3, 2]
    private string[] uConButtons2 = { "Save", "Clear" };
    private string uConEntry;
    private List<string> uIDs;
    private Texture2D bG;
    #endregion

    #region Database Subdivision Variables [3, 5]
    private string[] uConButtons5 = { "Gather GIDs", "Calculate Ranges", "Create GID Tables", "Save GID Tables" };
    private string[] gidFiles;
    private List<Vector2Int> userRanges;
    private List<vrrktGIDTable> gidTables;
    private int userCount;
    private int phase;
    private bool phaseDone;
    private bool phaseRunning;
    private int phaseProgress;
    private int usr;
    #endregion

    #endregion

    #region Self Installation Operations Variables [4,0]
    private int sIChosen;

    #region Main Page [4, 0]
    private string[] sIButtons0 = { "Clear Cards & Images", "Clear All" };
    #endregion

    #region Progress Page [4, 1]
    private int prog;
    private int progLim;
    #endregion

    #endregion

    #region Log View Variables [5,0]

    #endregion

    #region Custom Serialization Variables [6,X]

    #region Custom Serialization Main Page Variables [6,0]
    private string[] customSerButtons = { "Create Test Datasets", "Back" };
    private int customSerChosen;
    #endregion

    #region Select Serialization Type Page Variables [6,1]
    private string[] serTypeButtons = { "VrrktOperationsData", " VrrktSystemData", "VrrktReport", "VrrktUser", "VrrktCard" };
    private int serTypeChosen;
    private string[] serTypeControls = { "Create", "Load", "Back" };
    private int serControlChosen;
    private Type serT;
    #endregion

    #region Serialization Create Page Variables [6,2]
    private string[] createButtons = { "Save", "Clear", "Back" };
    private int createSelection;
    private Vector2 createPos;
    #endregion

    #region Serialization Load Page Variables [6,3]
    private string[] loadDirs;
    private int loadDirChosen;
    private string[] loadFiles;
    private int loadFilesChosen;
    private string[] loadFileNames;
    private int loadFileChosen;
    private string loadPath;
    private string[] loadFileButtons = { "Load & View", "Delete", "Back" };
    private int loadControlChosen;
    private int countTracker;
    private int dBINT;
    private Vector2 loadPos;
    private Vector2 loadDirPos;
    private Vector2 loadFilePos;
    #endregion

    #region Serialization View Loaded File Page Variables [6, 4]
    private string[] loadViewButtons = { "Ok", "Modify", "Delete", "Back" };
    private int loadViewChosen;
    private Vector2 loadViewPos;

    #region Vrrkt Card Display Variables
    private string[] cDataHeaders = { "Name:", "Mana Cost:", "Converted Mana Cost:", "Types:", "Card Text:", "Flavor Text:", "P/T:", "Loyalty:", "Color Indicator:" };
    private int c1GID;
    private List<string> c1Data;
    private string set;
    private string rarity;
    private string artist;
    private List<string> c1Rules;
    private Texture2D c1Txtr;
    private Vector2 c1RPos;
    private bool cSplit;
    private int c2GID;
    private List<string> c2Data;
    private List<string> c2Rules;
    private Texture2D c2Txtr;
    private Vector2 c2RPos;
    #endregion

    #endregion

    #region Create Test Dataset Variables
    private string testPathBase = "C:/Users/robzo/Desktop/vrrktTestFiles/";
    private Type[] serTypes = { typeof(VrrktOperationsData), typeof(VrrktSystemData), typeof(VrrktReport), typeof(VrrktUser), typeof(VrrktCard) };
    private object testSerObj;
    private string fileType, dataSetType;
    private VrrktOperationsData vOpsTest;
    private VrrktSystemData vSysTest;
    private VrrktReport vRepTest;
    private VrrktUser vUserTest;
    private VrrktCard vCardTest;
    private PropertyInfo[] props;
    #endregion

    #endregion

    #endregion

    #endregion

    #region Editor Startup Methods

    #region Enable
    private void OnEnable()
    {
        Application.runInBackground = true;
        //Debug.Log("Enable");
        AssetDatabase.Refresh();
        sIButtons0 = new string[] { "Clear Cards & Images", "Clear All" };

        stop = false;

        #region Set Variables For Main Page
        mainButtons = new string[] { "Container Operations", "Central Database Access", "Construct User Files", "Self Installation Operations", "Log View", "Custom Serialization Operations" };
        pX = 0;
        pY = 0;
        mainPos = Vector2.zero;
        mainChosen = -1;
        #endregion

        #region Set Central Database Variables
        if (centralDatabasePathBase != "C:/Users/robzo/Desktop/VrrktCentralDatabase/Database/")
        {
            centralDatabasePathBase = "C:/Users/robzo/Desktop/VrrktCentralDatabase/Database/";
        }
        dB0Chosen = -1;
        dB0Buttons = new string[] { "Create", "Access", "Integrity Check", "Duplicate Filter", "Clear All Images", "Clear", "Back" };
        dB1Chosen = -1;
        dB2Chosen = -1;
        dB3Chosen = -1;
        dB4Chosen = -1;
        dB4Buttons = new string[] { "Start Puller", "Resume Puller", "Run Image Puller", "Back" };
        dB5Chosen = -1;
        dB6Chosen = -1;
        dB8Chosen = -1;
        dB9Chosen = -1;
        dB10Chosen = -1;
        db14Buttons = new string[] { "Run", "Single", "Clear", "Back" };
        db14Chosen = -1;
        pullerLimit = 0;
        pullerUnits = 0;
        #endregion

        #region Set Formatter Variables
        vFormatter = new VrrktFormatter();
        serialize = null;
        deserialize = null;
        tck = -1;
        tLim = -1;
        sTck = -1;
        sTLim = -1;
        pathChosen = -1;
        fChosen = -1;
        dChosen = -1;
        deserControl = -1;
        serControl = -1;
        iAdd = -1;
        customPath = "";
        directory = "";
        fileChosen = "";
        directories = new string[0];
        files = new string[0];
        desReturned = new List<object>();
        desType = new List<Type>();
        serType = new List<Type>();
        serChosen = -1;
        #endregion

        #region Set Custom Serialization Testing Variables
        customSerChosen = -1;
        serTypeChosen = -1;
        serControlChosen = -1;
        createSelection = -1;
        loadFileChosen = -1;
        loadControlChosen = -1;
        countTracker = -1;
        loadViewChosen = -1;
        createPos = Vector2.zero;
        loadPos = Vector2.zero;
        loadViewPos = Vector2.zero;
        #endregion
    }
    #endregion

    #region INIT
    [MenuItem("Window/Custom/VrrktEditor V2")]
    static void Init()
    {
        Debug.Log("Init");
        vrrktEditorVersion2 vE = (vrrktEditorVersion2)GetWindow(typeof(vrrktEditorVersion2));
        vE.maxSize = new Vector2(Screen.width, Screen.height);
        EditorUtility.SetDirty(vE);
        vE.Show();
    }
    #endregion

    #endregion

    #region GUI
    private void OnGUI()
    {
        mainPos = GUILayout.BeginScrollView(mainPos);
        GUI.skin.button.fixedHeight = 40;

        #region Persistent Info & Controls

        #region Page Header
        GUILayout.BeginVertical();

        #region Page Index Box
        GUILayout.BeginHorizontal();
        GUI.skin.box.stretchWidth = true;

        GUILayout.Box("Page[" + pX + "," + pY + "]");
        GUILayout.Box(VrrktCoroutine.getCountRunning() + "");

        GUI.skin.box.stretchWidth = false;
        GUILayout.EndHorizontal();
        #endregion

        #region Global Controls
        GUILayout.BeginHorizontal();

        #region Stop All Coroutines Button
        if (GUILayout.Button("Stop All Coroutines"))
        {
            VrrktCoroutine.totalWipe();
        }
        #endregion

        #region Reload Editor Button
        if (GUILayout.Button("Reload Window"))
        {
            Init();
            OnEnable();
        }
        #endregion

        GUILayout.EndHorizontal();
        #endregion

        GUILayout.EndVertical();
        #endregion

        GUI.skin.box.alignment = TextAnchor.MiddleCenter;

        #region Resources Info Window
        GUILayout.BeginHorizontal(GUILayout.Width(Screen.width - 10));

        GUI.skin.box.stretchWidth = true;
        GUI.skin.box.stretchHeight = true;

        #region VrrktOperations
        if (vOps != null)
        {

        }
        else
        {
            GUILayout.Box("VrrktOperations does not exist yet", GUILayout.Height(Screen.height / 8));
        }
        #endregion

        #region VrrktSystemData
        if (vSys != null)
        {

        }
        else
        {
            GUILayout.Box("VrrktSystemData does not exist yet", GUILayout.Height(Screen.height / 8));
        }
        #endregion

        #region VrrktReport
        if (vRep != null)
        {

        }
        else
        {
            GUILayout.Box("VrrktReport does not exist yet", GUILayout.Height(Screen.height / 8));
        }
        #endregion

        #region VrrktUser
        if (vUser != null)
        {

        }
        else
        {
            GUILayout.Box("VrrktUser does not exist yet", GUILayout.Height(Screen.height / 8));
        }
        #endregion

        GUI.skin.box.stretchHeight = false;
        GUI.skin.box.stretchWidth = false;
        GUILayout.EndHorizontal();
        #endregion

        #region Notes Window
        GUILayout.Box("This function is not yet implemented. Please be patient", GUILayout.Height(Screen.height / 8), GUILayout.Width(Screen.width - 10));
        #endregion

        GUI.skin.box.alignment = TextAnchor.UpperCenter;

        #endregion

        #region Page Controls

        #region Main Page [0,0]
        if (pX == 0)
        {
            mainChosen = GUILayout.SelectionGrid(mainChosen, mainButtons, 3, GUILayout.Height(Screen.height / 4), GUILayout.Width(Screen.width - 10));

            #region Selection Logic
            if (mainChosen != -1)
            {
                //Debug.Log("Button " + mainButtonChosen + " hit");
                #region Container Operations [0]
                if (mainChosen == 0)
                {
                    contChosen = -1;
                    pX = 1;
                    pY = 0;
                }
                #endregion

                #region Central Database Access [1]
                if (mainChosen == 1)
                {
                    pX = 2;
                    pY = 0;
                }
                #endregion

                #region Construct User Files [2]
                if (mainChosen == 2)
                {
                    uConChosen = -1;
                    pX = 3;
                    pY = 0;
                }
                #endregion

                #region Self Installation Operations [3]
                if (mainChosen == 3)
                {
                    sIChosen = -1;
                    pX = 4;
                    pY = 0;
                }
                #endregion

                #region Custom Serialization [5]
                else if (mainChosen == 5)
                {
                    pX = 6;
                    pY = 0;
                }
                #endregion

                mainChosen = -1;
            }
            #endregion
        }
        #endregion

        #region Container Operations [1,0]
        if (pX == 1)
        {
            #region Main Page [1, 0]
            if (pY == 0)
            {
                #region Control Display
                contChosen = GUILayout.SelectionGrid(contChosen, cont1Buttons, cont1Buttons.Length);
                #endregion

                #region Control Logic
                if (contChosen != -1)
                {
                    cDbPhase = 0;

                    #region Create GID Tables
                    if (contChosen == 0)
                    {
                        dbUserCount = 0;
                        pY = 1;
                    }
                    #endregion

                    #region Create Card Databases
                    if (contChosen == 1)
                    {
                        cDbPhase = 0;
                        pY = 2;
                    }
                    #endregion

                    #region Create Image Databases
                    if (contChosen == 2)
                    {
                        cDbPhase = 0;
                        pY = 3;
                    }
                    #endregion
                    
                    contChosen = -1;
                }
                #endregion
            }
            #endregion

            #region Create GID Tables Page [1, 1]
            if (pY == 1)
            {
                #region Begin [0]
                if (cDbPhase == 0)
                {
                    #region User Count Entry
                    dbUserCount = EditorGUILayout.IntField(dbUserCount);
                    #endregion

                    #region Begin Button
                    if (GUILayout.Button("Begin Creation"))
                    {
                        if (dbUserCount > 0)
                        {
                            tables = new List<vrrktGIDTable>();
                            for (int n = 0; n <= dbUserCount - 1; n++)
                            {
                                tables.Add(new vrrktGIDTable());
                            }
                            //Debug.Log("Begin Creation. T C: " + tables.Count);
                            gidTableCreateControl(dbUserCount);
                            cDbPhase = 1;
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
                if (cDbPhase == 1)
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

            #region Create Card Databases Page [1, 2]
            if (pY == 2)
            {
                #region Begin [0]
                if (cDbPhase == 0)
                {
                    #region Begin Button
                    if (GUILayout.Button("Begin Creation"))
                    {
                        //cardDatabaseControl();
                        implementCreators(true, 8);
                        cDbPhase = 1;
                    }
                    #endregion
                }
                #endregion

                #region Running [1]
                if (cDbPhase == 1)
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

                            for(int n = 0; n <= dbCreators.Count - 1; n++)
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
                                cDbPhase = 0;
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

            #region Create Image Databases Page [1, 3]
            if (pY == 3)
            {
                #region Begin [0]
                if (cDbPhase == 0)
                {
                    #region Begin Button
                    if (GUILayout.Button("Begin Creation"))
                    {
                        implementCreators(false, 8);
                        //imageDatabaseControl();
                        cDbPhase = 1;
                    }
                    #endregion
                }
                #endregion

                #region Running [1]
                if (cDbPhase == 1)
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
                                cDbPhase = 0;
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

        #region Central Database Access [2,0-19]
        if (pX == 2)
        {
            #region Main Page [2, 0]
            if (pY == 0)
            {
                #region Controls Display
                dB0Chosen = GUILayout.SelectionGrid(dB0Chosen, dB0Buttons, dB0Buttons.Length / 2);

                #region Control Logic
                if (dB0Chosen != -1)
                {
                    #region Create
                    if (dB0Chosen == 0)
                    {
                        pY = 4;
                    }
                    #endregion

                    #region Access
                    if (dB0Chosen == 1)
                    {
                        pY = 1;
                    }
                    #endregion

                    #region Integrity Check
                    if (dB0Chosen == 2)
                    {
                        integrityCount = 0;
                        integrityLimit = 0;
                        db13Chosen = -1;
                        pY = 13;
                    }
                    #endregion

                    #region Duplicate Filter
                    if (dB0Chosen == 3)
                    {
                        db14Buttons = new string[] { "Run", "Clear", "Back" };
                        integrityCount = 0;
                        integrityLimit = 0;
                        db14Chosen = -1;
                        pY = 16;
                    }
                    #endregion

                    #region Clear All Images
                    if (dB0Chosen == 4)
                    {
                        totalProg = 0;
                        clearProg = 0;
                        startTime = DateTime.Now;
                        clearImagesDatabase();
                        pY = 18;
                    }
                    #endregion

                    #region Clear
                    if (dB0Chosen == 5)
                    {
                        clearCards = false;
                        clearImages = false;
                        clearRunning = false;
                        clearFiles = new string[0];
                        clearProgress = 0;
                        pY = 12;
                    }
                    #endregion

                    #region Back
                    if (dB0Chosen == dB0Buttons.Length - 1)
                    {
                        backButton();
                    }
                    #endregion

                    dB0Chosen = -1;
                }
                #endregion

                #endregion
            }
            #endregion

            #region Access Page [2, 1]
            if (pY == 1)
            {
                #region Controls Display

                dB1Chosen = GUILayout.SelectionGrid(dB1Chosen, dB1Buttons, dB1Buttons.Length);

                #region Control Logic
                if (dB1Chosen != -1)
                {
                    #region Specific
                    if (dB1Chosen == 0)
                    {
                        pY = 2;
                    }
                    #endregion

                    #region Random
                    if (dB1Chosen == 1)
                    {
                        pY = 3;
                    }
                    #endregion

                    #region Back
                    if (dB1Chosen == dB1Buttons.Length - 1)
                    {
                        backButton();
                    }
                    #endregion

                    dB1Chosen = -1;
                }
                #endregion

                #endregion
            }
            #endregion

            #region Specific Card Access Page [2, 2] XXXX
            if (pY == 2)
            {
                // Insert logic here to display searchable parameter layouts.

                #region Control Display

                dB2Chosen = GUILayout.SelectionGrid(dB2Chosen, dB2Buttons, dB2Buttons.Length / 2);

                #region Control Logic
                if (dB2Chosen != -1)
                {
                    #region Search

                    #endregion

                    #region Clear

                    #endregion

                    #region Back

                    #endregion
                }
                #endregion

                #endregion
            }
            #endregion

            #region Card Display Page [2, 3] XXXXXXXXXXXXX
            if (pY == 3)
            {

                #region Controls Display

                dB3Chosen = GUILayout.SelectionGrid(dB3Chosen, dB3Buttons, dB3Buttons.Length);

                #region Control Logic
                if (dB3Chosen != -1)
                {
                    #region Back To Results
                    if (dB3Chosen == 0)
                    {
                        //switch back to results display
                    }
                    #endregion

                    #region Back To Search
                    if (dB3Chosen == 1)
                    {
                        pY = 2;
                    }
                    #endregion

                    dB3Chosen = -1;
                }
                #endregion

                #endregion
            }
            #endregion

            #region Puller Operations Page [2, 4]
            if (pY == 4)
            {
                #region Controls Display

                dB4Chosen = GUILayout.SelectionGrid(dB4Chosen, dB4Buttons, dB4Buttons.Length);

                #region Control Logic
                if (dB4Chosen != -1)
                {
                    #region Start Puller
                    if (dB4Chosen == 0)
                    {
                        pullerLimit = 0;
                        dB5Chosen = -1;
                        pY = 5;
                    }
                    #endregion

                    #region Resume Puller
                    if (dB4Chosen == 1)
                    {
                        pullerLimit = 0;
                        dB6Chosen = -1;
                        pY = 6;
                    }
                    #endregion

                    #region Image Puller
                    if (dB4Chosen == 2)
                    {
                        dB8Chosen = -1;
                        pY = 8;
                    }
                    #endregion

                    #region Back
                    if (dB4Chosen == dB4Buttons.Length)
                    {
                        backButton();
                    }
                    #endregion

                    dB4Chosen = -1;
                }
                #endregion

                #endregion
            }
            #endregion

            #region Start Puller Params Page [2, 5]
            if (pY == 5)
            {
                #region Input Display
                GUILayout.BeginHorizontal();

                #region Limit
                GUILayout.Box("What GID should the system end on?");
                pullerLimit = EditorGUILayout.IntField(pullerLimit);
                #endregion

                #region Units
                GUILayout.Box("How many pullers should the load be divided amongst?");
                pullerUnits = EditorGUILayout.IntField(pullerUnits);
                #endregion

                GUILayout.EndHorizontal();
                #endregion

                #region Controls Display

                dB5Chosen = GUILayout.SelectionGrid(dB5Chosen, dB5Buttons, dB5Buttons.Length);

                #region Control Logic
                if (dB5Chosen != -1)
                {
                    #region Begin
                    if (dB5Chosen == 0)
                    {
                        if (pullerLimit > 0 && pullerUnits > 0)
                        {
                            pY = 7;
                            progressPos = Vector2.zero;
                            pullerProgressChosen = -1;
                            pullerCreationControl(pullerLimit, pullerUnits);
                            //createAndBegin(pullerLimit, pullerUnits);
                        }
                        else
                        {
                            Debug.Log("Limit and # of units must be non zero!");
                        }
                    }
                    #endregion

                    #region Clear
                    if (dB5Chosen == 1)
                    {
                        pullerLimit = 0;
                        pullerUnits = 0;
                    }
                    #endregion

                    #region Back
                    if (dB5Chosen == dB5Buttons.Length - 1)
                    {
                        backButton();
                    }
                    #endregion

                    dB5Chosen = -1;
                }
                #endregion

                #endregion
            }
            #endregion
            
            #region Puller Progress Page [2, 7]
            if (pY == 7)
            {
                #region !Completed
                if (!totalDone)
                {
                    progressPos = GUILayout.BeginScrollView(progressPos);

                    #region Progress Displays
                    GUILayout.BeginVertical();
                    GUI.skin.box.stretchWidth = true;
                    getTotalProgress();
                    getTotalBlanks();

                    #region Time Tracker
                    GUILayout.BeginHorizontal();

                    GUILayout.Box("Start:" + startTime, GUILayout.Width((Screen.width / 3) - 5));
                    GUILayout.Box("Current:" + DateTime.Now, GUILayout.Width((Screen.width / 3) - 5));
                    GUILayout.Box("Minutes Elapsed:" + (short)DateTime.Now.Subtract(startTime).TotalMinutes, GUILayout.Width((Screen.width / 3) - 5));

                    GUILayout.EndHorizontal();
                    #endregion

                    #region Global

                    #region Totals & Blanks
                    GUILayout.BeginHorizontal();

                    GUILayout.Box("Total/Limit: [" + totalProg + "/" + pullerLimit + "]", GUILayout.Width((Screen.width / 3) - 5));
                    GUILayout.Box("Cards/Limit: [" + (totalProg - totalBlank) + "/" + pullerLimit + "]", GUILayout.Width((Screen.width / 3) - 5));
                    GUILayout.Box("Blank/Limit: [" + totalBlank + "/" + pullerLimit + "]", GUILayout.Width((Screen.width / 3) - 5));

                    GUILayout.EndHorizontal();

                    #endregion

                    #region Rates & Percentages
                    GUILayout.BeginHorizontal();

                    GUILayout.Box("Blank %: " + ((double)totalBlank / pullerLimit) * 100, GUILayout.Width((Screen.width / 3) - 5));
                    GUILayout.Box("Card %: " + ((double)(totalProg - totalBlank) / pullerLimit) * 100, GUILayout.Width((Screen.width / 3) - 5));
                    GUILayout.Box("Cards/Minute:" + (int)(totalProg / DateTime.Now.Subtract(startTime).TotalMinutes), GUILayout.Width((Screen.width / 3) - 5));

                    GUILayout.EndHorizontal();
                    #endregion

                    #endregion

                    #region Unit Progress

                    #region Sections Of 5
                    GUILayout.BeginVertical();
                    for (int n = 0; n <= pullers.Count - 1; n += 5)
                    {
                        GUILayout.BeginHorizontal();
                        for(int w = 0; w <= 4; w++)
                        {
                            if (n + w < pullers.Count - 1)
                            {
                                GUILayout.BeginVertical();
                                GUILayout.Box("C: " + (pullers[n + w].current - pullers[n + w].lower));
                                GUILayout.Box("Blanks C: " + pullers[n + w].blanks.Count);
                                GUILayout.Box("Stage/Step: " + pullers[n + w].stage + "/" + pullers[n + w].step);
                                GUILayout.EndVertical();
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();
                    #endregion

                    #endregion

                    GUI.skin.box.stretchWidth = false;
                    GUILayout.EndVertical();
                    #endregion

                    #region Control Display

                    pullerProgressChosen = GUILayout.SelectionGrid(pullerProgressChosen, pullerProgressButtons, pullerProgressButtons.Length);

                    #region Control Logic
                    if (pullerProgressChosen != -1)
                    {
                        #region Stop
                        if (pullerProgressChosen == 0)
                        {
                            stopOps();

                            pullerProgressChosen = -1;
                        }
                        #endregion

                        #region Save
                        if (pullerProgressChosen == 1)
                        {
                            pullerContainer = new pullerDataContainer(pullers);
                            EditorUtility.SetDirty(pullerContainer);
                            pullerProgressChosen = -1;
                        }
                        #endregion
                    }
                    #endregion

                    #endregion

                    GUILayout.EndScrollView();
                }
                #endregion

                #region Completed
                else if (totalDone)
                {
                    GUILayout.BeginHorizontal();

                    #region Information Display
                    for (int n = 0; n <= pullers.Count - 1; n++)
                    {
                        GUILayout.BeginVertical();

                        #region Bounds & Range Display
                        GUILayout.BeginHorizontal();

                        #region Upper
                        GUILayout.Box("Upper Limit: " + pullers[n].upper);
                        #endregion

                        #region Lower
                        GUILayout.Box("Lower Limit: " + pullers[n].upper);
                        #endregion

                        #region Range
                        GUILayout.Box("Range: " + pullers[n].range);
                        #endregion

                        GUILayout.EndHorizontal();
                        #endregion

                        #region Blank Count & Blank Ratio
                        GUILayout.BeginHorizontal();

                        #region Blank Count
                        GUILayout.Box("Blank Count: " + pullers[n].blanks.Count);
                        #endregion

                        #region Ratio
                        GUILayout.Box("Blank %:" + ((float)pullers[n].blanks.Count / (float)pullers[n].range) * 100);
                        #endregion

                        GUILayout.EndHorizontal();
                        #endregion

                        GUILayout.EndVertical();
                    }
                    #endregion

                    GUILayout.EndHorizontal();

                    #region Control Display
                    if (GUILayout.Button("Save & Go Home"))
                    {
                        pullerContainer = new pullerDataContainer(pullers);
                        EditorUtility.SetDirty(pullerContainer);
                        pY = 0;
                    }
                    #endregion
                }
                #endregion
            }
            #endregion

            #region Image Puller Operations Page [2, 8]
            if (pY == 8)
            {
                #region Control Display

                dB8Chosen = GUILayout.SelectionGrid(dB8Chosen, dB8Buttons, dB8Buttons.Length);

                #region Control Logic
                if (dB8Chosen != -1)
                {
                    #region Start Fresh
                    if (dB8Chosen == 0)
                    {
                        dB9Chosen = -1;
                        imgPullersCount = 0;
                        imgPullerLimit = 0;
                        bPL = new List<int>();
                        bPpH = 0;

                        pY = 9;
                    }
                    #endregion
                    
                    #region Back
                    if (dB8Chosen == dB8Buttons.Length - 1)
                    {
                        backButton();
                    }
                    #endregion

                    dB8Chosen = -1;
                }
                #endregion

                #endregion
            }
            #endregion

            #region Image Puller Start Display Page [2, 9]
            if (pY == 9)
            {
                #region Input Display
                GUILayout.BeginVertical();

                #region Set Limit
                GUILayout.BeginHorizontal();

                GUILayout.Box("What GID limit should it look for?");
                imgPullerLimit = EditorGUILayout.IntField(imgPullerLimit);

                GUILayout.EndHorizontal();
                #endregion

                #region Puller Input
                GUILayout.BeginHorizontal();

                GUILayout.Box("How many pullers to use?");
                imgPullersCount = EditorGUILayout.IntField(imgPullersCount, GUILayout.Height(30f));

                GUILayout.EndHorizontal();
                #endregion

                GUILayout.EndVertical();
                #endregion

                #region Bypass Display

                #region Add/Remove
                GUILayout.BeginHorizontal();

                #region Add Button
                if (GUILayout.Button("Add"))
                {
                    if (bPpH > 0)
                    {
                        if (bPL != null)
                        {
                            bPL.Add(bPpH);
                        }
                        else
                        {
                            bPL = new List<int>();
                            bPL.Add(bPpH);
                        }
                        bPpH = 0;
                    }
                    else
                    {
                        Debug.Log("Cannot bypass an index that cannot be utilized. Please try again.");
                    }
                }
                #endregion

                #region Remove Button
                if (GUILayout.Button("Remove"))
                {
                    if (bPL != null)
                    {
                        if (bPL.Count > 0)
                        {
                            bPL.Remove(bPL[bPL.Count - 1]);
                        }
                        else
                        {
                            Debug.Log("Cannot remove something from an empty container. Please try again");
                        }
                    }
                    else
                    {
                        bPL = new List<int>();
                        Debug.Log("Cannot remove something from an empty container. Please try again");
                    }
                }
                #endregion

                GUILayout.EndHorizontal();
                #endregion

                #region Placeholder & List
                bPpH = EditorGUILayout.IntField(bPpH, GUILayout.MinWidth(50f));

                #region List
                if (bPL != null)
                {
                    GUILayout.BeginVertical();

                    for (int n = 0; n <= bPL.Count - 1; n++)
                    {
                        GUILayout.Box(bPL[n] + "", GUILayout.MinWidth(50f));
                    }

                    GUILayout.EndVertical();
                }
                #endregion

                #endregion

                #endregion

                #region Control Display

                dB9Chosen = GUILayout.SelectionGrid(dB9Chosen, dB9Buttons, dB9Buttons.Length);

                #region Control Logic
                if (dB9Chosen != -1)
                {
                    #region Begin
                    if (dB9Chosen == 0)
                    {
                        #region No Bypass
                        if (bPL == null || bPL.Count == 0)
                        {
                            if (imgPullerLimit > 0)
                            {
                                imagePullerCreationControl(imgPullerLimit, imgPullersCount);
                            }
                            else
                            {
                                Debug.Log("Must set a limit greater than 0!");
                            }
                        }
                        #endregion

                        #region Bypass
                        else
                        {
                            if (imgPullerLimit > 0)
                            {
                                if (bPpH > 0)
                                {
                                    bPL.Add(bPpH);
                                    bPpH = 0;
                                }

                                imagePullerCreationControl(imgPullerLimit, imgPullersCount);
                            }
                            else
                            {
                                Debug.Log("Must set a limit greater than 0!");
                            }
                        }
                        #endregion

                        pY = 11;
                    }
                    #endregion

                    #region Clear
                    if (dB9Chosen == 1)
                    {
                        imgPullerLimit = 0;
                        imgPullersCount = 0;
                        bPL = new List<int>();
                        bPpH = 0;
                    }
                    #endregion

                    #region Back
                    if (dB9Chosen == dB9Buttons.Length - 1)
                    {
                        //Debug.Log("dB9 Back");
                        imgPullersCount = 0;
                        backButton();
                    }
                    #endregion

                    dB9Chosen = -1;
                }
                #endregion

                #endregion
            }
            #endregion
            
            #region Image Puller Progress Page [2, 11]
            if (pY == 11)
            {
                #region Image Progress Display
                if (pullers != null && pullers.Count == imgPullersCount)
                {
                    GUILayout.BeginVertical(GUILayout.Width(Screen.width - 10));
                    getTotalProgress();

                    #region Global

                    #region Time Tracker
                    GUILayout.BeginHorizontal();

                    GUI.skin.box.stretchWidth = true;
                    GUILayout.Box("Start:" + startTime, GUILayout.Width((Screen.width / 3) - 5));
                    GUILayout.Box("Current:" + DateTime.Now, GUILayout.Width((Screen.width / 3) - 5));
                    GUILayout.Box(("Minutes Elapsed:" + (int)DateTime.Now.Subtract(startTime).TotalMinutes), GUILayout.Width((Screen.width / 3) - 5));
                    GUI.skin.box.stretchWidth = false;

                    GUILayout.EndHorizontal();
                    #endregion

                    #region Progress & Rates
                    GUILayout.BeginHorizontal();

                    GUILayout.Box("Total/Limit: [" + totalProg + "/" + imgPullerLimit + "]", GUILayout.Width((Screen.width / 3) - 5));
                    GUILayout.Box("Completion %: " + ((double)totalProg / imgPullerLimit) * 100, GUILayout.Width((Screen.width / 3) - 5));
                    GUILayout.Box("Rate: " + (int)(totalProg / DateTime.Now.Subtract(startTime).TotalMinutes), GUILayout.Width((Screen.width / 3) - 5));

                    GUILayout.EndHorizontal();
                    #endregion

                    #endregion

                    #region Images Display
                    for (int n = 0; n <= pullers.Count - 1; n += 5)
                    {
                        GUILayout.BeginHorizontal();
                        GUI.skin.box.stretchWidth = true;

                        for(int w = 0; w <= 4; w++)
                        {
                            if (n + w <= pullers.Count - 1)
                            {
                                if (pullers[n + w].current < pullers[n + w].upper)
                                {
                                    GUILayout.BeginVertical();

                                    GUILayout.Box("C: " + (pullers[n + w].current - pullers[n + w].lower), GUILayout.Width((Screen.width / 5) - 5));
                                    GUILayout.Box("GID1:" + pullers[n].i1GID + ", GID2:" + pullers[n].i2GID, GUILayout.Width((Screen.width / 5) - 5));
                                    GUILayout.Box("Step:" + pullers[n].step, GUILayout.Width((Screen.width / 5) - 5));

                                    GUILayout.EndVertical();
                                }
                            }
                        }

                        GUILayout.EndHorizontal();
                        GUI.skin.box.stretchWidth = false;
                    }

                    #endregion

                    GUILayout.EndVertical();
                }
                else
                {
                    GUILayout.Box("Please wait....", GUILayout.Width((Screen.width / 5) - 5));
                }
                #endregion

                #region Control Display
                if (pullers != null && pullers.Count == imgPullersCount)
                {
                    if (GUILayout.Button("Stop"))
                    {
                        stop = true;
                        pY = 8;
                    }
                }
                #endregion
            }
            #endregion

            #region Clear Database Directory Progress Page [2, 12]
            if (pY == 12)
            {
                #region !Running
                if (!clearRunning)
                {
                    #region Options

                    #region Clear Cards
                    GUILayout.BeginHorizontal();

                    GUILayout.Box("Clear all cards?");
                    clearCards = GUILayout.Toggle(clearCards, "");

                    GUILayout.EndHorizontal();
                    #endregion

                    #region Clear Images
                    GUILayout.BeginHorizontal();

                    GUILayout.Box("Clear all images?");
                    clearImages = GUILayout.Toggle(clearImages, "");

                    GUILayout.EndHorizontal();
                    #endregion

                    #endregion

                    #region Controls
                    GUILayout.BeginHorizontal();

                    #region Begin
                    if (GUILayout.Button("Begin"))
                    {
                        VrrktCoroutine.start(clearDatabase(), this);
                        clearRunning = true;
                    }
                    #endregion

                    #region Home
                    if (GUILayout.Button("Home"))
                    {
                        VrrktCoroutine.totalWipe();
                        Init();
                        OnEnable();
                    }
                    #endregion

                    GUILayout.EndHorizontal();
                    #endregion
                }
                #endregion

                #region Running
                else
                {
                    #region Progress
                    GUILayout.BeginHorizontal();

                    GUILayout.Box(clearProgress + " out of " + clearFiles.Length + " deleted.");

                    GUILayout.EndHorizontal();
                    #endregion

                    #region Controls
                    if (GUILayout.Button("Stop"))
                    {
                        VrrktCoroutine.totalWipe();
                        Init();
                        OnEnable();
                    }
                    #endregion
                }
                #endregion
            }
            #endregion

            #region Integrity Check Operations Page [2, 13]
            if (pY == 13)
            {
                #region Controls Display

                db13Chosen = GUILayout.SelectionGrid(db13Chosen, db13Buttons, db13Buttons.Length);

                #region Control Logic
                if (db13Chosen != -1)
                {
                    #region Card
                    if (db13Chosen == 0)
                    {
                        pY = 14;
                    }
                    #endregion

                    #region Image
                    if (db13Chosen == 1)
                    {
                        pY = 19;
                    }
                    #endregion

                    #region Back
                    if (db13Chosen == db13Buttons.Length - 1)
                    {
                        backButton();
                    }
                    #endregion

                    db13Chosen = -1;
                }
                #endregion

                #endregion
            }
            #endregion

            #region Integrity Check Parameters Page [2, 14]
            if (pY == 14)
            {
                #region Input Display

                #region Limit Input
                GUILayout.BeginHorizontal();

                GUILayout.Box("Provide a limit:");
                integrityLimit = EditorGUILayout.IntField(integrityLimit);

                GUILayout.EndHorizontal();
                #endregion

                #region Count Input
                GUILayout.BeginHorizontal();

                GUILayout.Box("How many units to perform this operation?");
                integrityCount = EditorGUILayout.IntField(integrityCount);

                GUILayout.EndHorizontal();
                #endregion

                #endregion

                #region Control Display

                db14Chosen = GUILayout.SelectionGrid(db14Chosen, db14Buttons, db14Buttons.Length);

                #region Control Logic
                if (db14Chosen != -1)
                {
                    #region Run
                    if (db14Chosen == 0)
                    {
                        if (integrityCount > 0 && integrityLimit > 0)
                        {
                            repullTotal = 0;
                            integrityControl(integrityLimit, integrityCount);
                            //VrrktCoroutine.start(createIntegrityPullers(integrityLimit, integrityCount), this);
                            pY = 15;
                        }
                        else
                        {
                            integrityCount = 0;
                            integrityLimit = 0;
                            Debug.Log("Limit & Count must be greater than 0!");
                        }
                    }
                    #endregion

                    #region Single
                    if (db14Chosen == 1)
                    {
                        if (integrityLimit > 0)
                        {
                            repullTotal = 0;
                            //VrrktCoroutine.start(createIntegrityPullers(integrityLimit));
                            pY = 15;
                        }
                        else
                        {
                            integrityCount = 0;
                            integrityLimit = 0;
                            Debug.Log("Limit must be greater than 0!");
                        }
                    }
                    #endregion

                    #region Clear
                    if (db14Chosen == 2)
                    {
                        integrityLimit = 0;
                        integrityCount = 0;
                    }
                    #endregion

                    #region Back
                    if (db14Chosen == db14Buttons.Length - 1)
                    {
                        integrityCount = 0;
                        integrityLimit = 0;

                        backButton();
                    }
                    #endregion

                    db14Chosen = -1;
                }
                #endregion

                #endregion
            }
            #endregion

            #region Database Integrity Progress [2, 15]
            if (pY == 15)
            {
                #region Ready
                if (pullers != null && pullers.Count == integrityCount)
                {
                    #region Display
                    GUILayout.BeginVertical(GUILayout.Width(Screen.width - 10));
                    getTotalProgress();
                    getTotalRepulled();

                    #region Time Tracker
                    GUILayout.BeginHorizontal();

                    GUI.skin.box.stretchWidth = true;
                    GUILayout.Box("Start:" + startTime, GUILayout.Width((Screen.width / 3) - 5));
                    GUILayout.Box("Current:" + DateTime.Now, GUILayout.Width((Screen.width / 3) - 5));
                    GUILayout.Box(("Minutes Elapsed:" + (int)DateTime.Now.Subtract(startTime).TotalMinutes), GUILayout.Width((Screen.width / 3) - 5));
                    GUI.skin.box.stretchWidth = false;

                    GUILayout.EndHorizontal();
                    #endregion

                    #region Global

                    #region Integers
                    GUILayout.BeginHorizontal();

                    GUILayout.Box("Total/Limit: [" + totalProg + "/" + integrityLimit + "]", GUILayout.Width((Screen.width / 2) - 5));
                    GUILayout.Box("Total repulled: " + repullTotal, GUILayout.Width((Screen.width / 2) - 5));

                    GUILayout.EndHorizontal();
                    #endregion

                    #region Percentages & Rates
                    GUILayout.BeginHorizontal();

                    GUILayout.Box("Completion %: " + ((double)totalProg / integrityLimit) * 100, GUILayout.Width((Screen.width / 2) - 5));
                    GUILayout.Box("Rate: " + (int)(totalProg / DateTime.Now.Subtract(startTime).TotalMinutes), GUILayout.Width((Screen.width / 2) - 5));

                    GUILayout.EndHorizontal();
                    #endregion

                    #endregion

                    #region Unit Display
                    for (int n = 0; n <= pullers.Count - 1; n += 5)
                    {
                        GUI.skin.box.stretchWidth = true;
                        GUILayout.BeginHorizontal();

                        #region 0
                        if (n <= pullers.Count - 1)
                        {
                            if (pullers[n].current <= pullers[n].upper)
                            {
                                GUILayout.BeginVertical();
                                GUILayout.Box("[" + n + "]", GUILayout.Width((Screen.width / 5) - 5));
                                GUILayout.Box("L/C/U: [" + pullers[n].lower + "/" + pullers[n].current + "/" + pullers[n].upper + "]", GUILayout.Width((Screen.width / 5) - 5));
                                GUILayout.Box("Repulled: " + pullers[n].repulled, GUILayout.Width((Screen.width / 5) - 5));
                                GUILayout.Box("Step: " + pullers[n].step, GUILayout.Width((Screen.width / 5) - 5));
                                GUILayout.EndVertical();
                            }
                            else
                            {
                                GUILayout.Box("Completed.", GUILayout.Width((Screen.width / 5) - 5));
                            }
                        }
                        #endregion

                        #region 1
                        if (n + 1 <= pullers.Count - 1)
                        {
                            if (pullers[n + 1].current <= pullers[n + 1].upper)
                            {
                                GUILayout.BeginVertical();
                                GUILayout.Box("[" + (n + 1) + "]", GUILayout.Width((Screen.width / 5) - 5));
                                GUILayout.Box("L/C/U: [" + pullers[n + 1].lower + "/" + pullers[n + 1].current + "/" + pullers[n + 1].upper + "]", GUILayout.Width((Screen.width / 5) - 5));
                                GUILayout.Box("Repulled: " + pullers[n + 1].repulled, GUILayout.Width((Screen.width / 5) - 5));
                                GUILayout.Box("Step: " + pullers[n + 1].step, GUILayout.Width((Screen.width / 5) - 5));
                                GUILayout.EndVertical();
                            }
                            else
                            {
                                GUILayout.Box("Completed.", GUILayout.Width((Screen.width / 5) - 5));
                            }
                        }
                        #endregion

                        #region 2
                        if (n + 2 <= pullers.Count - 1)
                        {
                            if (pullers[n + 2].current <= pullers[n + 2].upper)
                            {
                                GUILayout.BeginVertical();
                                GUILayout.Box("[" + (n + 2) + "]", GUILayout.Width((Screen.width / 5) - 5));
                                GUILayout.Box("L/C/U: [" + pullers[n + 2].lower + "/" + pullers[n + 2].current + "/" + pullers[n + 2].upper + "]", GUILayout.Width((Screen.width / 5) - 5));
                                GUILayout.Box("Repulled: " + pullers[n + 2].repulled, GUILayout.Width((Screen.width / 5) - 5));
                                GUILayout.Box("Step: " + pullers[n + 2].step, GUILayout.Width((Screen.width / 5) - 5));
                                GUILayout.EndVertical();
                            }
                            else
                            {
                                GUILayout.Box("Completed.", GUILayout.Width((Screen.width / 5) - 5));
                            }
                        }
                        #endregion

                        #region 3
                        if (n + 3 <= pullers.Count - 1)
                        {
                            if (pullers[n + 3].current <= pullers[n + 3].upper)
                            {
                                GUILayout.BeginVertical();
                                GUILayout.Box("[" + (n + 3) + "]", GUILayout.Width((Screen.width / 5) - 5));
                                GUILayout.Box("L/C/U: [" + pullers[n + 3].lower + "/" + pullers[n + 3].current + "/" + pullers[n + 3].upper + "]", GUILayout.Width((Screen.width / 5) - 5));
                                GUILayout.Box("Repulled: " + pullers[n + 3].repulled, GUILayout.Width((Screen.width / 5) - 5));
                                GUILayout.Box("Step: " + pullers[n + 3].step, GUILayout.Width((Screen.width / 5) - 5));
                                GUILayout.EndVertical();
                            }
                            else
                            {
                                GUILayout.Box("Completed.", GUILayout.Width((Screen.width / 5) - 5));
                            }
                        }
                        #endregion

                        #region 4
                        if (n + 4 <= pullers.Count - 1)
                        {
                            if (pullers[n + 4].current <= pullers[n + 4].upper)
                            {
                                GUILayout.BeginVertical();
                                GUILayout.Box("[" + (n + 4) + "]", GUILayout.Width((Screen.width / 5) - 5));
                                GUILayout.Box("L/C/U: [" + pullers[n + 4].lower + "/" + pullers[n + 4].current + "/" + pullers[n + 4].upper + "]", GUILayout.Width((Screen.width / 5) - 5));
                                GUILayout.Box("Repulled: " + pullers[n + 4].repulled, GUILayout.Width((Screen.width / 5) - 5));
                                GUILayout.Box("Step: " + pullers[n + 4].step, GUILayout.Width((Screen.width / 5) - 5));
                                GUILayout.EndVertical();
                            }
                            else
                            {
                                GUILayout.Box("Completed.", GUILayout.Width((Screen.width / 5) - 5));
                            }
                        }
                        #endregion

                        GUILayout.EndHorizontal();
                        GUI.skin.box.stretchWidth = false;
                    }
                    #endregion

                    GUILayout.EndVertical();
                    #endregion

                    #region Stop
                    if (GUILayout.Button("Stop"))
                    {
                        VrrktCoroutine.totalWipe();
                        Init();
                        OnEnable();
                    }
                    #endregion
                }
                #endregion

                #region !Ready
                else
                {
                    GUI.skin.box.stretchWidth = true;
                    GUILayout.Box("Please wait....");
                    GUI.skin.box.stretchWidth = false;
                }
                #endregion
            }
            #endregion

            #region Duplicate Filter Parameters Page [2, 16]
            if (pY == 16)
            {
                #region Input Display

                #region Limit Input
                GUILayout.BeginHorizontal();

                GUILayout.Box("Provide a limit:");
                integrityLimit = EditorGUILayout.IntField(integrityLimit);

                GUILayout.EndHorizontal();
                #endregion

                #region Count Input
                GUILayout.BeginHorizontal();

                GUILayout.Box("How many units to perform this operation?");
                integrityCount = EditorGUILayout.IntField(integrityCount);

                GUILayout.EndHorizontal();
                #endregion

                #endregion

                #region Control Display

                db14Chosen = GUILayout.SelectionGrid(db14Chosen, db14Buttons, db14Buttons.Length);

                #region Control Logic
                if (db14Chosen != -1)
                {
                    #region Run
                    if (db14Chosen == 0)
                    {
                        if (integrityCount > 0 && integrityLimit > 0)
                        {
                            duplicateFilterControl(integrityLimit, integrityCount);
                            //VrrktCoroutine.start(createDuplicateFilters(integrityLimit, integrityCount), this);
                            pY = 17;
                        }
                        else
                        {
                            integrityCount = 0;
                            integrityLimit = 0;
                            Debug.Log("Limit & Count must be greater than 0!");
                        }
                    }
                    #endregion

                    #region Clear
                    if (db14Chosen == 2)
                    {
                        integrityLimit = 0;
                        integrityCount = 0;
                    }
                    #endregion

                    #region Back
                    if (db14Chosen == db14Buttons.Length - 1)
                    {
                        integrityCount = 0;
                        integrityLimit = 0;

                        backButton();
                    }
                    #endregion

                    db14Chosen = -1;
                }
                #endregion

                #endregion
            }
            #endregion

            #region Duplicate Filter Progress [2, 17]
            if (pY == 17)
            {
                #region Ready
                if (pullers != null && pullers.Count == integrityCount)
                {
                    #region Display
                    GUILayout.BeginVertical(GUILayout.Width(Screen.width - 10));
                    getTotalProgress();
                    getTotalRepulled();

                    #region Time Tracker
                    GUILayout.BeginHorizontal();

                    GUI.skin.box.stretchWidth = true;
                    GUILayout.Box("Start:" + startTime, GUILayout.Width((Screen.width / 3) - 5));
                    GUILayout.Box("Current:" + DateTime.Now, GUILayout.Width((Screen.width / 3) - 5));
                    GUILayout.Box(("Minutes Elapsed:" + (int)DateTime.Now.Subtract(startTime).TotalMinutes), GUILayout.Width((Screen.width / 3) - 5));
                    GUI.skin.box.stretchWidth = false;

                    GUILayout.EndHorizontal();
                    #endregion

                    #region Global

                    #region Integers
                    GUILayout.BeginHorizontal();

                    GUILayout.Box("Total/Limit: [" + totalProg + "/" + integrityLimit + "]", GUILayout.Width((Screen.width / 2) - 5));
                    GUILayout.Box("Total Deleted: " + repullTotal, GUILayout.Width((Screen.width / 2) - 5));

                    GUILayout.EndHorizontal();
                    #endregion

                    #region Percentages & Rates
                    GUILayout.BeginHorizontal();

                    GUILayout.Box("Completion %: " + ((double)totalProg / integrityLimit) * 100, GUILayout.Width((Screen.width / 2) - 5));
                    GUILayout.Box("Rate: " + (int)(totalProg / DateTime.Now.Subtract(startTime).TotalMinutes), GUILayout.Width((Screen.width / 2) - 5));

                    GUILayout.EndHorizontal();
                    #endregion

                    #endregion

                    #region Unit Display
                    for (int n = 0; n <= pullers.Count - 1; n += 5)
                    {
                        GUI.skin.box.stretchWidth = true;
                        GUILayout.BeginHorizontal();

                        for(int w = 0; w <= 4; w++)
                        {
                            if (n + w <= pullers.Count - 1)
                            {
                                if (pullers[n + w].current <= pullers[n + w].upper)
                                {
                                    GUILayout.BeginVertical();
                                    GUILayout.Box("[" + (n + w) + "]", GUILayout.Width((Screen.width / 5) - 5));
                                    GUILayout.Box("C: " + (pullers[n + w].current - pullers[n + w].lower), GUILayout.Width((Screen.width / 5) - 5));
                                    GUILayout.Box("Deleted: " + pullers[n + w].repulled, GUILayout.Width((Screen.width / 5) - 5));
                                    GUILayout.Box("Step: " + pullers[n + w].step, GUILayout.Width((Screen.width / 5) - 5));
                                    GUILayout.EndVertical();
                                }
                            }
                        }

                        GUILayout.EndHorizontal();
                        GUI.skin.box.stretchWidth = false;
                    }
                    #endregion

                    GUILayout.EndVertical();
                    #endregion

                    #region Stop
                    if (GUILayout.Button("Stop"))
                    {
                        VrrktCoroutine.totalWipe();
                        Init();
                        OnEnable();
                    }
                    #endregion
                }
                #endregion

                #region !Ready
                else
                {
                    GUI.skin.box.stretchWidth = true;
                    GUILayout.Box("Please wait....");
                    GUI.skin.box.stretchWidth = false;
                }
                #endregion
            }
            #endregion

            #region Delete All Images Progress [2, 18]
            if (pY == 18)
            {
                #region Progress Display

                #region Time Tracker
                GUILayout.BeginHorizontal();

                GUI.skin.box.stretchWidth = true;
                GUILayout.Box("Start:" + startTime, GUILayout.Width((Screen.width / 3) - 5));
                GUILayout.Box("Current:" + DateTime.Now, GUILayout.Width((Screen.width / 3) - 5));
                GUILayout.Box(("Minutes Elapsed:" + (int)DateTime.Now.Subtract(startTime).TotalMinutes), GUILayout.Width((Screen.width / 3) - 5));
                GUI.skin.box.stretchWidth = false;

                GUILayout.EndHorizontal();
                #endregion

                #region Progress & Percentage

                GUILayout.Box("Total/Limit: [" + clearProg + "/" + totalProg + "]", GUILayout.Width(Screen.width - 5));
                GUILayout.Box("Completion %: " + ((double)clearProg / totalProg) * 100, GUILayout.Width(Screen.width - 5));

                #endregion

                #endregion

                #region Stop
                if (GUILayout.Button("Stop"))
                {
                    VrrktCoroutine.totalWipe();
                    Init();
                    OnEnable();
                }
                #endregion
            }
            #endregion

            #region Image Integrity Check Parameters Page [2, 19]
            if (pY == 19)
            {
                #region Input Display

                #region Limit Input
                GUILayout.BeginHorizontal();

                GUILayout.Box("Provide a limit:");
                integrityLimit = EditorGUILayout.IntField(integrityLimit);

                GUILayout.EndHorizontal();
                #endregion

                #region Count Input
                GUILayout.BeginHorizontal();

                GUILayout.Box("How many units to perform this operation?");
                integrityCount = EditorGUILayout.IntField(integrityCount);

                GUILayout.EndHorizontal();
                #endregion

                #endregion

                #region Control Display

                db14Chosen = GUILayout.SelectionGrid(db14Chosen, db14Buttons, db14Buttons.Length);

                #region Control Logic
                if (db14Chosen != -1)
                {
                    #region Run
                    if (db14Chosen == 0)
                    {
                        if (integrityCount > 0 && integrityLimit > 0)
                        {
                            repullTotal = 0;
                            imageIntegrityControl(integrityLimit, integrityCount);
                            //VrrktCoroutine.start(createImageIntegrityPullers(integrityLimit, integrityCount), this);
                            pY = 15;
                        }
                        else
                        {
                            integrityCount = 0;
                            integrityLimit = 0;
                            Debug.Log("Limit & Count must be greater than 0!");
                        }
                    }
                    #endregion

                    #region Single
                    if (db14Chosen == 1)
                    {
                        if (integrityLimit > 0)
                        {
                            repullTotal = 0;
                            //VrrktCoroutine.start(createImageIntegrityPullers(integrityLimit));
                            pY = 15;
                        }
                        else
                        {
                            integrityCount = 0;
                            integrityLimit = 0;
                            Debug.Log("Limit must be greater than 0!");
                        }
                    }
                    #endregion

                    #region Clear
                    if (db14Chosen == 2)
                    {
                        integrityLimit = 0;
                        integrityCount = 0;
                    }
                    #endregion

                    #region Back
                    if (db14Chosen == db14Buttons.Length - 1)
                    {
                        integrityCount = 0;
                        integrityLimit = 0;

                        backButton();
                    }
                    #endregion

                    db14Chosen = -1;
                }
                #endregion

                #endregion
            }
            #endregion
        }
        #endregion

        #region Construct User Files [3,0-5]
        if (pX == 3)
        {
            #region Main Page [3, 0]
            if (pY == 0)
            {
                uConChosen = GUILayout.SelectionGrid(uConChosen, uConButtons0, 2);

                #region Control Logic
                if (uConChosen != -1)
                {
                    #region Resources
                    if (uConChosen == 0)
                    {
                        pY = 1;
                    }
                    #endregion

                    #region Database
                    if (uConChosen == 1)
                    {
                        gidFiles = new string[0];
                        userRanges = new List<Vector2Int>();
                        gidTables = new List<vrrktGIDTable>();
                        userCount = 0;
                        phase = 0;
                        phaseDone = false;
                        phaseRunning = false;
                        pY = 5;
                    }
                    #endregion

                    uConChosen = -1;
                }
                #endregion
            }
            #endregion

            #region Resource Selection Page [3, 1]
            if (pY == 1)
            {
                uConChosen = GUILayout.SelectionGrid(uConChosen, uConButtons1, 3);

                #region Control Logic
                if (uConChosen != -1)
                {
                    #region VrrktSystemData
                    if (uConChosen == 0)
                    {
                        uIDs = new List<string>();
                        bG = null;
                        uConEntry = "";
                        pY = 2;
                    }
                    #endregion

                    #region VrrktOperations
                    if (uConChosen == 1)
                    {
                        pY = 3;
                    }
                    #endregion

                    #region VrrktUser
                    if (uConChosen == 2)
                    {
                        pY = 4;
                    }
                    #endregion

                    uConChosen = -1;
                }
                #endregion
            }
            #endregion

            #region Construct VrrktSystemData [3, 2]
            if (pY == 2)
            {
                #region Input Area
                GUILayout.BeginHorizontal();

                #region User ID Display

                #region Display
                GUILayout.BeginVertical();

                GUILayout.Box("User IDs", GUILayout.MinWidth(Screen.width / 5));

                for (int n = 0; n <= uIDs.Count - 1; n++)
                {
                    GUILayout.Box(uIDs[n], GUILayout.MinWidth(Screen.width / 5));
                }

                GUILayout.EndVertical();
                #endregion

                #region Controls
                GUILayout.BeginHorizontal();

                #region Entry
                uConEntry = GUILayout.TextField(uConEntry, GUILayout.MinWidth(Screen.width / 5));
                #endregion

                #region Add
                if (GUILayout.Button("+"))
                {
                    if (uConEntry != "")
                    {
                        uIDs.Add(uConEntry);
                        uConEntry = "";
                    }
                }
                #endregion

                #region Remove
                if (GUILayout.Button("-"))
                {
                    if (uIDs.Count > 0)
                    {
                        uIDs.Remove(uIDs[uIDs.Count - 1]);
                    }
                }
                #endregion

                GUILayout.EndHorizontal();
                #endregion

                #endregion

                #region Background Display
                GUILayout.BeginVertical();

                #region Entry Display
                GUILayout.BeginHorizontal();

                #region Texture Display
                GUILayout.Box(bG, GUILayout.Width(Screen.width / 4), GUILayout.Height(Screen.width / 4));
                #endregion

                #region Dimension Display
                GUILayout.BeginVertical();

                if (bG != null)
                {
                    GUILayout.Box("X: " + bG.width, GUILayout.Width(Screen.width / 8));

                    GUILayout.Box("Y: " + bG.height, GUILayout.Width(Screen.width / 8));
                }
                else
                {
                    GUILayout.Box("X: ", GUILayout.Width(Screen.width / 8));

                    GUILayout.Box("Y: ", GUILayout.Width(Screen.width / 8));
                }

                GUILayout.EndVertical();
                #endregion

                GUILayout.EndHorizontal();
                #endregion

                #region Entry Box
                bG = (Texture2D)EditorGUILayout.ObjectField(bG, typeof(Texture), true, GUILayout.Width(Screen.width / 5));
                #endregion

                GUILayout.EndVertical();
                #endregion

                GUILayout.EndHorizontal();
                #endregion

                #region Control Display
                uConChosen = GUILayout.SelectionGrid(uConChosen, uConButtons2, 2);

                #region Control Logic
                if (uConChosen != -1)
                {
                    #region Save
                    if (uConChosen == 0)
                    {
                        VrrktSystemData vS = new VrrktSystemData();
                        vS.background = bG;
                        vS.x = bG.width;
                        vS.y = bG.height;
                        vS.userIDs = uIDs;
                        vrrktSave("C:/Users/robzo/Desktop/VrrktCentralDatabase/Resources/VrrktSystemData.vrrktDs", vS);
                        uIDs = new List<string>();
                        bG = null;
                        pY = 0;
                    }
                    #endregion

                    #region Clear
                    if (uConChosen == 1)
                    {
                        uIDs = new List<string>();
                        bG = null;
                    }
                    #endregion

                    uConChosen = -1;
                }
                #endregion

                #endregion
            }
            #endregion

            #region Construct VrrktOperations [3, 3]
            if (pY == 3)
            {
                if (GUILayout.Button("Begin"))
                {
                    VrrktOperationsData vO = new VrrktOperationsData();
                    vrrktSave("C:/Users/robzo/Desktop/VrrktCentralDatabase/Resources/VrrktOperations.vrrktDo", vO);
                    pY = 0;
                }
            }
            #endregion

            #region Construct VrrktUser [3, 4]
            if (pY == 4)
            {
                if (GUILayout.Button("Begin"))
                {
                    VrrktUser vU = new VrrktUser();
                    vrrktSave("C:/Users/robzo/Desktop/VrrktCentralDatabase/Resources/VrrktUser.vrrktU", vU);
                    pY = 0;
                }
            }
            #endregion
            
        }
        #endregion

        #region Self Installation Operations [4,0-1]
        if (pX == 4)
        {
            #region Main Page [4, 0]
            if (pY == 0)
            {
                sIChosen = GUILayout.SelectionGrid(sIChosen, sIButtons0, 2);

                #region Control Logic
                if (sIChosen != -1)
                {
                    #region Clear Cards & Images
                    if (sIChosen == 0)
                    {
                        VrrktCoroutine.start(clearInstallDatabase());
                        pY = 1;
                    }
                    #endregion

                    #region Clear All
                    if (sIChosen == 1)
                    {
                        Debug.Log("Not yet operational");
                    }
                    #endregion

                    sIChosen = -1;
                }
                #endregion
            }
            #endregion

            #region Progress Page [4, 1]
            if (pY == 1)
            {
                #region Progress Display
                GUILayout.Box("Progress: " + prog + ", Progress Limit: " + progLim, GUILayout.Width(Screen.width - 10), GUILayout.Height(Screen.height / 5));
                #endregion

                #region End Of Operation Handling
                if (progLim > 0 && prog == progLim)
                {
                    Init();
                    OnEnable();
                }
                #endregion
            }
            #endregion
        }
        #endregion
        
        #region Custom Serialization Operations [6,0-4]
        if (pX == 6)
        {
            #region Main Page [6, 0]
            if (pY == 0)
            {
                customSerChosen = GUILayout.SelectionGrid(customSerChosen, customSerButtons, 2);

                #region Selection Logic
                if (customSerChosen >= 0)
                {
                    //Debug.Log("Main Page selection = " + customSerChosen + " which = " + customSerButtons[customSerChosen]);

                    #region Test Datasets
                    if (customSerChosen == 0)
                    {
                        pY = 1;
                        customSerChosen = -1;
                    }
                    #endregion

                    #region Back Button
                    else
                    {
                        backButton();
                        customSerChosen = -1;
                    }
                    #endregion
                }
                #endregion
            }
            #endregion

            #region Custom Datasets Operation Selection Page [6, 1]
            if (pY == 1)
            {
                #region Selection Line
                serTypeChosen = GUILayout.SelectionGrid(serTypeChosen, serTypeButtons, serTypeButtons.Length);
                #endregion

                #region Control Line
                serControlChosen = GUILayout.SelectionGrid(serControlChosen, serTypeControls, serTypeControls.Length);
                #endregion

                #region Control Logic
                if (serControlChosen >= 0)
                {
                    Debug.Log("Operation Selection Page control selection = " + serControlChosen + " & type selection = " + serTypeChosen);
                    //Debug.Log("Operation Selection Page selection = " + serControlChosen + " which => T" + serTypeButtons[serTypeChosen] + ", B" + serTypeControls[serControlChosen]);

                    #region !Back Button
                    if (serControlChosen < 2)
                    {
                        #region Create Logic
                        if (serControlChosen == 0)
                        {
                            Debug.Log("Create");

                            #region Has Selected Type
                            if (serTypeChosen >= 0)
                            {
                                Debug.Log("Has Selected Type:" + serTypeButtons[serTypeChosen]);

                                #region vOps
                                if (serTypeChosen == 0)
                                {
                                    vOpsTest = new VrrktOperationsData();
                                    testSerObj = vOpsTest;
                                }
                                #endregion

                                #region vSys
                                if (serTypeChosen == 1)
                                {
                                    vSysTest = new VrrktSystemData();
                                    testSerObj = vSysTest;
                                }
                                #endregion

                                #region vRep
                                if (serTypeChosen == 2)
                                {
                                    vRepTest = new VrrktReport();
                                    testSerObj = vRepTest;
                                }
                                #endregion

                                #region vUser
                                if (serTypeChosen == 3)
                                {
                                    vUserTest = new VrrktUser();
                                    testSerObj = vUserTest;
                                }
                                #endregion

                                #region vCard
                                if (serTypeChosen == 4)
                                {
                                    vCardTest = new VrrktCard();
                                    testSerObj = vCardTest;
                                }
                                #endregion

                                props = createGetParameters(testSerObj.GetType());
                                Debug.Log("Properties Count:" + props.Length);
                                pY = 2;
                                serTypeChosen = -1;
                                serControlChosen = -1;
                            }
                            #endregion

                            #region Has Not Selected Type
                            else
                            {
                                Debug.Log("Must select a type to create");
                                serControlChosen = -1;
                                serTypeChosen = -1;
                            }
                            #endregion
                        }
                        #endregion

                        #region Load Logic
                        else if (serControlChosen == 1)
                        {
                            Debug.Log("Chose to load.");

                            loadFileChosen = -1;
                            loadFilesChosen = -1;
                            loadDirChosen = -1;
                            loadPath = "C:/";
                            loadDirs = Directory.GetDirectories(loadPath);
                            loadFiles = Directory.GetFiles(loadPath);

                            for (int n = 0; n <= loadDirs.Length - 1; n++)
                            {
                                if (loadDirs[n].Contains(loadPath))
                                {
                                    loadDirs[n] = loadDirs[n].Replace(loadPath, "");
                                }
                            }

                            for (int n = 0; n <= loadFiles.Length - 1; n++)
                            {
                                if (loadFiles[n].Contains(loadPath))
                                {
                                    loadFiles[n] = loadFiles[n].Replace(loadPath, "");
                                }
                            }

                            loadDirPos = Vector2.zero;
                            loadFilePos = Vector2.zero;
                            serControlChosen = -1;
                            serTypeChosen = -1;
                            pY = 3;
                        }
                        #endregion
                    }
                    #endregion

                    #region Back Button
                    else if (serControlChosen == 2)
                    {
                        backButton();
                        serControlChosen = -1;
                    }
                    #endregion
                }
                #endregion
            }
            #endregion

            #region Custom Datasets Creation Page [6, 2]
            if (pY == 2)
            {
                #region Display Logic

                #region Begin Area
                EditorGUILayout.BeginFoldoutHeaderGroup(true, testSerObj.GetType().Name);

                #endregion

                #region Area Members
                EditorGUILayout.BeginVertical();
                for (int n = 0; n <= props.Length - 1; n++)
                {
                    EditorGUILayout.BeginHorizontal();

                    #region Property Type Logic

                    #region List
                    if (props[n].PropertyType == typeof(List<string>) || props[n].PropertyType == typeof(List<int>))
                    {
                        #region List<string>
                        if (props[n].PropertyType == typeof(List<string>))
                        {
                            if (((List<string>)props[n].GetValue(testSerObj)).Count <= 0)
                            {
                                ((List<string>)props[n].GetValue(testSerObj)).Add("");
                            }

                            #region Property Info Display & List Controls
                            EditorGUILayout.BeginHorizontal();

                            EditorGUILayout.BeginVertical();

                            #region Type/Name
                            EditorGUILayout.BeginHorizontal();

                            GUILayout.Box("List<string>");
                            GUILayout.Box(props[n].Name);

                            EditorGUILayout.EndHorizontal();
                            #endregion

                            #region +/- Buttons
                            EditorGUILayout.BeginHorizontal();

                            #region + Button
                            if (GUILayout.Button("+", GUILayout.MaxWidth(30)))
                            {
                                ((List<string>)props[n].GetValue(testSerObj)).Add("");
                            }
                            #endregion

                            #region - Button
                            if (GUILayout.Button("-", GUILayout.MaxWidth(30)))
                            {
                                if (((List<string>)props[n].GetValue(testSerObj)).Count > 1)
                                {
                                    ((List<string>)props[n].GetValue(testSerObj)).Remove(((List<string>)props[n].GetValue(testSerObj))[((List<string>)props[n].GetValue(testSerObj)).Count - 1]);
                                }
                                else
                                {
                                    ((List<string>)props[n].GetValue(testSerObj))[0] = "";
                                }
                            }
                            #endregion

                            EditorGUILayout.EndHorizontal();
                            #endregion

                            EditorGUILayout.EndVertical();

                            #region List Display
                            EditorGUILayout.BeginVertical();

                            for (int u = 0; u <= ((List<string>)props[n].GetValue(testSerObj)).Count - 1; u++)
                            {
                                ((List<string>)props[n].GetValue(testSerObj))[u] = EditorGUILayout.TextField(((List<string>)props[n].GetValue(testSerObj))[u], GUILayout.MinWidth(50));
                            }

                            EditorGUILayout.EndVertical();
                            #endregion

                            EditorGUILayout.EndHorizontal();

                            #region Reassign Property Value
                            props[n].SetValue(testSerObj, ((List<string>)props[n].GetValue(testSerObj)));
                            #endregion

                            #endregion
                        }
                        #endregion

                        #region List<int>
                        else if (props[n].PropertyType == typeof(List<int>))
                        {
                            if (((List<int>)props[n].GetValue(testSerObj)).Count <= 0)
                            {
                                ((List<int>)props[n].GetValue(testSerObj)).Add(0);
                            }

                            #region Property Info Display & List Controls
                            EditorGUILayout.BeginHorizontal();

                            EditorGUILayout.BeginVertical();

                            #region Type/Name
                            EditorGUILayout.BeginHorizontal();

                            GUILayout.Box("List<int>");
                            GUILayout.Box(props[n].Name);

                            EditorGUILayout.EndHorizontal();
                            #endregion

                            #region +/- Buttons
                            EditorGUILayout.BeginHorizontal();

                            #region + Button
                            if (GUILayout.Button("+", GUILayout.MaxWidth(30)))
                            {
                                ((List<int>)props[n].GetValue(testSerObj)).Add(0);
                            }
                            #endregion

                            #region - Button
                            if (GUILayout.Button("-", GUILayout.MaxWidth(30)))
                            {
                                if (((List<int>)props[n].GetValue(testSerObj)).Count > 1)
                                {
                                    ((List<int>)props[n].GetValue(testSerObj)).Remove(((List<int>)props[n].GetValue(testSerObj))[((List<int>)props[n].GetValue(testSerObj)).Count - 1]);
                                }
                                else
                                {
                                    ((List<int>)props[n].GetValue(testSerObj))[0] = 0;
                                }
                            }
                            #endregion

                            EditorGUILayout.EndHorizontal();
                            #endregion

                            EditorGUILayout.EndVertical();

                            #region List Display
                            EditorGUILayout.BeginVertical();

                            for (int u = 0; u <= ((List<int>)props[n].GetValue(testSerObj)).Count - 1; u++)
                            {
                                ((List<int>)props[n].GetValue(testSerObj))[u] = EditorGUILayout.IntField(((List<int>)props[n].GetValue(testSerObj))[u]);
                            }

                            EditorGUILayout.EndVertical();
                            #endregion

                            EditorGUILayout.EndHorizontal();

                            #region Reassign Property Values
                            props[n].SetValue(testSerObj, ((List<int>)props[n].GetValue(testSerObj)));
                            #endregion

                            #endregion
                        }
                        #endregion
                    }
                    #endregion

                    #region !List
                    else
                    {
                        #region Property Info Display
                        GUILayout.Box(props[n].PropertyType.Name);

                        GUILayout.Box(props[n].Name);
                        #endregion

                        #region String
                        if (props[n].PropertyType == typeof(string))
                        {
                            string s = (string)props[n].GetValue(testSerObj);
                            props[n].SetValue(testSerObj, EditorGUILayout.TextField((string)props[n].GetValue(testSerObj), GUILayout.MinWidth(50)));
                        }
                        #endregion

                        #region Bool
                        if (props[n].PropertyType == typeof(bool))
                        {
                            props[n].SetValue(testSerObj, EditorGUILayout.Toggle((bool)props[n].GetValue(testSerObj)));

                        }
                        #endregion

                        #region Int
                        if (props[n].PropertyType == typeof(int))
                        {
                            props[n].SetValue(testSerObj, EditorGUILayout.IntField((int)props[n].GetValue(testSerObj), GUILayout.MinWidth(50)));
                        }
                        #endregion

                        #region Float
                        if (props[n].PropertyType == typeof(float))
                        {
                            props[n].SetValue(testSerObj, EditorGUILayout.FloatField((float)props[n].GetValue(testSerObj), GUILayout.MinWidth(50)));
                        }
                        #endregion
                    }
                    #endregion

                    #endregion

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
                #endregion

                #region End Area
                EditorGUILayout.EndFoldoutHeaderGroup();
                #endregion

                #endregion

                #region Control Display
                createSelection = GUILayout.SelectionGrid(createSelection, createButtons, createButtons.Length);

                #region Control Logic
                if (createSelection >= 0)
                {
                    #region Save
                    if (createSelection == 0)
                    {
                        string savePath = testPathBase + testSerObj.GetType().Name;
                        savePath = appendFileType(testSerObj, savePath);
                        vrrktSave(savePath, testSerObj);
                        createSelection = -1;
                        pY = 1;
                        props = new PropertyInfo[0];
                    }
                    #endregion

                    #region Clear
                    else if (createSelection == 1)
                    {
                        for (int n = 0; n <= props.Length - 1; n++)
                        {
                            #region List
                            if (props[n].GetType() == typeof(List<string>) || props[n].GetType() == typeof(List<int>))
                            {
                                #region List<string>
                                if (props[n].GetType() == typeof(List<string>))
                                {
                                    List<string> lS = new List<string>();
                                    props[n].SetValue(testSerObj, lS);
                                }
                                #endregion

                                #region List<int>
                                else if (props[n].GetType() == typeof(List<int>))
                                {
                                    List<int> lI = new List<int>();
                                    props[n].SetValue(testSerObj, lI);
                                }
                                #endregion
                            }
                            #endregion

                            #region !List
                            else
                            {
                                #region String
                                if (props[n].GetType() == typeof(string))
                                {
                                    props[n].SetValue(testSerObj, "");
                                }
                                #endregion

                                #region Bool
                                if (props[n].GetType() == typeof(bool))
                                {
                                    props[n].SetValue(testSerObj, false);
                                }
                                #endregion

                                #region Number Type
                                if (props[n].GetType() == typeof(float) || props[n].GetType() == typeof(int))
                                {
                                    props[n].SetValue(testSerObj, 0);
                                }
                                #endregion
                            }
                            #endregion
                        }
                        createSelection = -1;
                    }
                    #endregion

                    #region Back
                    else if (createSelection == 2)
                    {
                        backButton();
                        createSelection = -1;
                    }
                    #endregion
                }
                #endregion

                #endregion
            }
            #endregion

            #region Custom Datasets Loading Page [6, 3]
            if (pY == 3)
            {
                #region Directory Selection Display
                if (countTracker < 0)
                {
                    #region Directory Contents Display

                    #region !Database Display
                    if (loadPath != "C:/Users/robzo/Desktop/VrrktCentralDatabase/Database/")
                    {
                        #region Directory Display
                        GUI.skin.box.stretchWidth = true;
                        GUILayout.Box("Directories");
                        GUI.skin.box.stretchWidth = false;

                        loadDirPos = GUILayout.BeginScrollView(loadDirPos, GUILayout.Width(Screen.width * .95f));

                        loadDirChosen = GUILayout.SelectionGrid(loadDirChosen, loadDirs, 5, GUILayout.Width(Screen.width * .9f));

                        GUILayout.EndScrollView();
                        #endregion

                        #region Files Display
                        GUI.skin.box.stretchWidth = true;
                        GUILayout.Box("Files");
                        GUI.skin.box.stretchWidth = false;

                        loadFilePos = GUILayout.BeginScrollView(loadFilePos, GUILayout.Width(Screen.width * .95f));

                        loadFilesChosen = GUILayout.SelectionGrid(loadFilesChosen, loadFiles, 5, GUILayout.Width(Screen.width * .9f));

                        GUILayout.EndScrollView();
                        #endregion
                    }
                    #endregion

                    #region Database Display
                    else
                    {
                        GUILayout.BeginHorizontal();

                        GUILayout.Box("Input a GID");
                        dBINT = EditorGUILayout.IntField(dBINT, GUILayout.MinWidth(50f));

                        GUILayout.EndHorizontal();
                    }
                    #endregion

                    #region Path Chosen Display
                    GUI.skin.box.stretchWidth = true;
                    GUILayout.Box(loadPath);
                    GUI.skin.box.stretchWidth = false;
                    #endregion

                    #region Move Up Directory Button
                    if (GUILayout.Button("Move Up One Level"))
                    {
                        if (loadPath != "C:/")
                        {
                            loadPath = loadPath.Substring(0, loadPath.LastIndexOf("/"));
                            loadPath = loadPath.Substring(0, loadPath.LastIndexOf("/"));
                            loadPath += loadDirs[loadDirChosen] + "/";
                            loadDirs = Directory.GetDirectories(loadPath);
                            loadFiles = Directory.GetFiles(loadPath);

                            for (int n = 0; n <= loadDirs.Length - 1; n++)
                            {
                                if (loadDirs[n].Contains(loadPath))
                                {
                                    loadDirs[n] = loadDirs[n].Replace(loadPath, "");
                                }

                                if (loadDirs[n].IndexOf("/") == 0)
                                {
                                    loadDirs[n] = loadDirs[n].Substring(1);
                                }

                            }

                            for (int n = 0; n <= loadFiles.Length - 1; n++)
                            {
                                if (loadFiles[n].Contains(loadPath))
                                {
                                    loadFiles[n] = loadFiles[n].Replace(loadPath, "");
                                }

                                if (loadFiles[n].IndexOf("/") == 0)
                                {
                                    loadFiles[n] = loadFiles[n].Substring(1);
                                }
                            }
                            loadDirChosen = -1;
                            loadFilesChosen = -1;
                        }
                    }
                    #endregion

                    #endregion

                    #region Controls Display

                    loadControlChosen = GUILayout.SelectionGrid(loadControlChosen, loadFileButtons, 3);

                    #region Control Logic
                    if (loadControlChosen >= 0)
                    {
                        #region Load & View
                        if (loadControlChosen == 0)
                        {
                            #region !Database
                            if (loadPath != "C:/Users/robzo/Desktop/VrrktCentralDatabase/Database/")
                            {
                                #region Has Selected A File
                                if (loadFiles[loadFilesChosen].Contains(".vrrkt"))
                                {
                                    #region Set Tracker
                                    if (desReturned == null || desReturned.Count == 0)
                                    {
                                        countTracker = 0;
                                    }
                                    else
                                    {
                                        countTracker = desReturned.Count;
                                    }
                                    #endregion

                                    #region Begin Load
                                    loadPath += loadFiles[loadFilesChosen];
                                    Debug.Log("path:" + loadPath);
                                    vrrktLoad(loadPath);
                                    #endregion

                                    loadControlChosen = -1;
                                    loadFilesChosen = -1;
                                    loadDirChosen = -1;
                                    loadDirPos = Vector2.zero;
                                    loadFilePos = Vector2.zero;
                                    loadFiles = new string[0];
                                    loadDirs = new string[0];
                                    loadPath = "C:/";
                                }
                                #endregion

                                #region Has Not Selected A File
                                else
                                {
                                    Debug.Log("You must select a file of Vrrkt Type!");
                                    loadControlChosen = -1;
                                }
                                #endregion
                            }
                            #endregion

                            #region Database
                            else
                            {
                                if (dBINT > 0)
                                {
                                    if (File.Exists(loadPath + dBINT + ".vrrktCd"))
                                    {
                                        desReturned = new List<object>();
                                        vrrktLoad(loadPath + dBINT + ".vrrktCd");
                                        dBINT = 0;
                                        loadControlChosen = -1;
                                        loadPath = "C:/";
                                        countTracker = 0;
                                    }
                                    else
                                    {
                                        Debug.Log("This GID does not exist.");
                                        loadControlChosen = -1;
                                        loadFilesChosen = -1;
                                    }
                                }
                                else
                                {
                                    Debug.Log("GID must be greater than 0!");
                                }
                            }
                            #endregion
                        }
                        #endregion

                        #region Delete
                        if (loadControlChosen == 1)
                        {
                            List<string> filt = new List<string>();
                            for (int n = 0; n <= loadFileNames.Length - 1; n++)
                            {
                                if (loadFileNames[n] != loadFileNames[loadFileChosen])
                                {
                                    filt.Add(loadFileNames[n]);
                                }
                            }
                            File.Delete(testPathBase + loadFileNames[loadFileChosen]);
                            loadFileNames = filt.ToArray();
                            loadControlChosen = -1;
                            loadFileChosen = -1;
                        }
                        #endregion

                        #region Back
                        if (loadControlChosen == 2)
                        {
                            loadControlChosen = -1;
                            loadFileChosen = -1;
                            backButton();
                        }
                        #endregion
                    }
                    #endregion

                    #endregion

                    #region Upper Control Logic

                    #region Set New Directory
                    if (loadDirChosen != -1)
                    {
                        loadPath += loadDirs[loadDirChosen] + "/";
                        loadDirs = Directory.GetDirectories(loadPath);
                        loadFiles = Directory.GetFiles(loadPath);

                        for (int n = 0; n <= loadDirs.Length - 1; n++)
                        {
                            if (loadDirs[n].Contains(loadPath))
                            {
                                loadDirs[n] = loadDirs[n].Replace(loadPath, "");
                            }

                            if (loadDirs[n].IndexOf("/") == 0)
                            {
                                loadDirs[n] = loadDirs[n].Substring(1);
                            }

                        }

                        for (int n = 0; n <= loadFiles.Length - 1; n++)
                        {
                            if (loadFiles[n].Contains(loadPath))
                            {
                                loadFiles[n] = loadFiles[n].Replace(loadPath, "");
                            }

                            if (loadFiles[n].IndexOf("/") == 0)
                            {
                                loadFiles[n] = loadFiles[n].Substring(1);
                            }
                        }
                        loadDirChosen = -1;
                        loadFilesChosen = -1;
                    }
                    #endregion

                    #endregion
                }
                #endregion

                #region Await Loading Completion Display
                else
                {
                    #region Await Message
                    if (desReturned == null || desReturned.Count == countTracker)
                    {
                        GUILayout.Box("Please wait while the selected file is loaded");
                    }
                    #endregion

                    #region Proceeding Loading
                    else
                    {
                        if (countTracker < desReturned.Count)
                        {
                            testSerObj = desReturned[countTracker];
                            if (testSerObj.GetType() != typeof(VrrktCard))
                            {
                                props = testSerObj.GetType().GetProperties();
                            }
                            else
                            {
                                VrrktCard vC = (VrrktCard)testSerObj;
                                c1GID = vC.GID;
                                c1Data = vC.c1DAT;
                                set = vC.set;
                                rarity = vC.rarity;
                                artist = vC.artist;
                                c1Rules = vC.c1Rulings;
                                c1RPos = Vector2.zero;
                                c2RPos = Vector2.zero;
                                cSplit = vC.split;
                                if (cSplit)
                                {
                                    c2Data = vC.c2DAT;
                                    c2Rules = vC.c2Rulings;
                                }
                                else
                                {
                                    c2GID = -1;
                                    c2Data = new List<string>();
                                    c2Rules = new List<string>();
                                    c2Txtr = new Texture2D(0, 0);
                                }
                            }
                            loadViewPos = Vector2.zero;
                            loadControlChosen = -1;
                            loadFileChosen = -1;
                            countTracker = -1;
                            pY = 4;
                        }
                    }
                    #endregion
                }
                #endregion
            }
            #endregion

            #region Custom Datasets View Loaded File Page [6, 4]
            if (pY == 4)
            {
                #region Display Logic

                #region !Vrrkt Card Type
                if (testSerObj.GetType() != typeof(VrrktCard))
                {
                    #region Begin Area
                    EditorGUILayout.BeginFoldoutHeaderGroup(true, testSerObj.GetType().Name);
                    #endregion

                    #region Area Members
                    EditorGUILayout.BeginVertical();
                    for (int n = 0; n <= props.Length - 1; n++)
                    {
                        EditorGUILayout.BeginHorizontal();

                        #region Property Type Logic

                        #region List
                        if (props[n].PropertyType == typeof(List<string>) || props[n].PropertyType == typeof(List<int>))
                        {
                            #region List<string>
                            if (props[n].PropertyType == typeof(List<string>))
                            {
                                #region Property Info Display & List Controls
                                EditorGUILayout.BeginHorizontal();

                                EditorGUILayout.BeginVertical();

                                #region Type/Name
                                EditorGUILayout.BeginHorizontal();

                                GUILayout.Box("List<string>");
                                GUILayout.Box(props[n].Name);

                                EditorGUILayout.EndHorizontal();
                                #endregion

                                EditorGUILayout.EndVertical();

                                #region List Display
                                EditorGUILayout.BeginVertical();

                                for (int u = 0; u <= ((List<string>)props[n].GetValue(testSerObj)).Count - 1; u++)
                                {
                                    GUILayout.Box(((List<string>)props[n].GetValue(testSerObj))[u], GUILayout.MinWidth(50));
                                }

                                EditorGUILayout.EndVertical();
                                #endregion

                                EditorGUILayout.EndHorizontal();

                                #endregion
                            }
                            #endregion

                            #region List<int>
                            else if (props[n].PropertyType == typeof(List<int>))
                            {
                                #region Property Info Display & List Controls
                                EditorGUILayout.BeginHorizontal();

                                EditorGUILayout.BeginVertical();

                                #region Type/Name
                                EditorGUILayout.BeginHorizontal();

                                GUILayout.Box("List<int>");
                                GUILayout.Box(props[n].Name);

                                EditorGUILayout.EndHorizontal();
                                #endregion

                                EditorGUILayout.EndVertical();

                                #region List Display
                                EditorGUILayout.BeginVertical();

                                for (int u = 0; u <= ((List<int>)props[n].GetValue(testSerObj)).Count - 1; u++)
                                {
                                    GUILayout.Box(((List<int>)props[n].GetValue(testSerObj))[u] + "");
                                }

                                EditorGUILayout.EndVertical();
                                #endregion

                                EditorGUILayout.EndHorizontal();
                                #endregion
                            }
                            #endregion
                        }
                        #endregion

                        #region !List
                        else
                        {
                            #region Property Info Display
                            GUILayout.Box(props[n].PropertyType.Name);

                            GUILayout.Box(props[n].Name);
                            #endregion

                            #region String
                            if (props[n].PropertyType == typeof(string))
                            {
                                GUILayout.Box((string)props[n].GetValue(testSerObj));
                            }
                            #endregion

                            #region Bool
                            if (props[n].PropertyType == typeof(bool))
                            {
                                GUILayout.Toggle((bool)props[n].GetValue(testSerObj), "");
                            }
                            #endregion

                            #region Int
                            if (props[n].PropertyType == typeof(int))
                            {
                                GUILayout.Box((int)props[n].GetValue(testSerObj) + "", GUILayout.MinWidth(50));
                            }
                            #endregion

                            #region Float
                            if (props[n].PropertyType == typeof(float))
                            {
                                GUILayout.Box((float)props[n].GetValue(testSerObj) + "", GUILayout.MinWidth(50));
                            }
                            #endregion
                        }
                        #endregion

                        #endregion

                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.EndVertical();
                    #endregion

                    #region End Area
                    EditorGUILayout.EndFoldoutHeaderGroup();
                    #endregion
                }
                #endregion

                #region Vrrkt Card Type
                else
                {
                    loadViewPos = GUILayout.BeginScrollView(loadViewPos, GUILayout.Width(Screen.width * .95f));
                    GUILayout.BeginVertical();

                    #region Card 1
                    GUILayout.BeginHorizontal();

                    #region Image & Rules Section
                    GUILayout.BeginVertical();

                    #region Image
                    GUILayout.Box(c1Txtr, GUILayout.MinWidth(50f), GUILayout.MinHeight(50f));
                    #endregion

                    #region Rules
                    c1RPos = GUILayout.BeginScrollView(c1RPos);
                    GUILayout.BeginVertical();

                    for (int n = 0; n <= c1Rules.Count - 1; n++)
                    {
                        GUILayout.TextArea(c1Rules[n]);
                    }

                    GUILayout.EndVertical();
                    GUILayout.EndScrollView();
                    #endregion

                    GUILayout.EndVertical();
                    #endregion

                    #region Card Data Section
                    GUILayout.BeginVertical();

                    #region GID
                    GUILayout.BeginHorizontal();

                    GUILayout.Box("GID:");
                    GUILayout.Box(c1GID + "");

                    GUILayout.EndHorizontal();
                    #endregion

                    #region DAT
                    for (int n = 0; n <= c1Data.Count - 1; n++)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Box(cDataHeaders[n]);
                        if (n != 4 && n != 5)
                        {
                            GUILayout.TextField(c1Data[n]);
                        }
                        else
                        {
                            GUILayout.TextArea(c1Data[n]);
                        }
                        GUILayout.EndHorizontal();
                    }
                    #endregion

                    #region Unit DAT
                    GUILayout.BeginHorizontal();

                    #region Set
                    GUILayout.Box("Set:");
                    GUILayout.Box(set);
                    #endregion

                    #region Rarity
                    GUILayout.Box("Rarity:");
                    GUILayout.Box(rarity);
                    #endregion

                    #region Artist
                    GUILayout.Box("Artist:");
                    GUILayout.Box(artist);
                    #endregion

                    GUILayout.EndHorizontal();
                    #endregion

                    GUILayout.EndVertical();
                    #endregion

                    GUILayout.EndHorizontal();
                    #endregion

                    #region Split Break
                    if (cSplit)
                    {
                        GUILayout.Box("", GUILayout.Width(Screen.width * .95f), GUILayout.Height(20f));
                    }
                    #endregion

                    #region Card 2
                    if (cSplit)
                    {
                        GUILayout.BeginHorizontal();

                        #region Image & Rules Section
                        GUILayout.BeginVertical();

                        #region Image
                        GUILayout.Box(c2Txtr, GUILayout.MinWidth(50f), GUILayout.MinHeight(50f));
                        #endregion

                        #region Rules
                        c2RPos = GUILayout.BeginScrollView(c2RPos);
                        GUILayout.BeginVertical();

                        for (int n = 0; n <= c2Rules.Count - 1; n++)
                        {
                            GUILayout.TextArea(c2Rules[n]);
                        }

                        GUILayout.EndVertical();
                        GUILayout.EndScrollView();
                        #endregion

                        GUILayout.EndVertical();
                        #endregion

                        #region Card Data Section
                        GUILayout.BeginVertical();

                        #region GID
                        GUILayout.BeginHorizontal();

                        GUILayout.Box("GID:");
                        GUILayout.Box(c2GID + "");

                        GUILayout.EndHorizontal();
                        #endregion

                        #region DAT
                        for (int n = 0; n <= c2Data.Count - 1; n++)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Box(cDataHeaders[n]);
                            if (n != 4 && n != 5)
                            {
                                GUILayout.TextField(c2Data[n]);
                            }
                            else
                            {
                                GUILayout.TextArea(c2Data[n]);
                            }
                            GUILayout.EndHorizontal();
                        }
                        #endregion

                        #region Unit DAT
                        GUILayout.BeginHorizontal();

                        #region Set
                        GUILayout.Box("Set:");
                        GUILayout.Box(set);
                        #endregion

                        #region Rarity
                        GUILayout.Box("Rarity:");
                        GUILayout.Box(rarity);
                        #endregion

                        #region Artist
                        GUILayout.Box("Artist:");
                        GUILayout.Box(artist);
                        #endregion

                        GUILayout.EndHorizontal();
                        #endregion

                        GUILayout.EndVertical();
                        #endregion

                        GUILayout.EndHorizontal();
                    }
                    #endregion

                    GUILayout.EndVertical();
                    GUILayout.EndScrollView();
                }
                #endregion

                #endregion

                #region Controls Display

                loadViewChosen = GUILayout.SelectionGrid(loadViewChosen, loadViewButtons, 4);

                #region Control Logic
                if (loadViewChosen >= 0)
                {
                    #region Ok
                    if (loadViewChosen == 0)
                    {
                        setAllSerVarsNew();
                    }
                    #endregion

                    #region Modify
                    if (loadViewChosen == 1)
                    {
                        Debug.Log("Not yet implemented");
                        loadViewChosen = -1;
                    }
                    #endregion

                    #region Delete
                    if (loadViewChosen == 2)
                    {
                        Debug.Log("Not yet implemented");
                        loadViewChosen = -1;
                    }
                    #endregion

                    #region Back
                    if (loadViewChosen == 3)
                    {
                        loadViewChosen = -1;
                        backButton();
                    }
                    #endregion
                }
                #endregion

                #endregion
            }
            #endregion
        }
        #endregion

        #endregion

        GUILayout.EndScrollView();
    }
    #endregion

    #region Custom Methods

    #region Button or Page Methods

    #region Back Button Method
    private void backButton()
    {
        OnEnable();
    }
    #endregion

    #region Set All Serialization Vars New
    void setAllSerVarsNew()
    {
        #region Selection Ints
        customSerChosen = -1;
        serTypeChosen = -1;
        serControlChosen = -1;
        createSelection = -1;
        loadFileChosen = -1;
        loadControlChosen = -1;
        countTracker = -1;
        loadViewChosen = -1;
        #endregion

        #region Position Vectors
        createPos = Vector2.zero;
        loadPos = Vector2.zero;
        loadViewPos = Vector2.zero;
        #endregion

        #region Other Vars
        serT = null;
        loadFileNames = null;
        testSerObj = null;
        #endregion

        pY = 0;
    }
    #endregion

    #endregion

    #region Get Params (Create) Method
    PropertyInfo[] createGetParameters(Type t)
    {
        PropertyInfo[] pOut = vrrktStaticDefinitions.getVrrktProperties(t);

        return pOut;
    }
    #endregion

    #region Get File Type
    string getFileType(Type t)
    {
        string fType = "";
        var fI = t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        for (int n = 0; n <= fI.Length - 1; n++)
        {
            if (fI[n].IsLiteral && !fI[n].IsInitOnly)
            {
                if (fI[n].Name == "fileType")
                {
                    fType = (string)fI[n].GetRawConstantValue();
                    break;
                }
            }
        }

        if (fType != "")
        {
            Debug.Log("Found file type property");
            return fType;
        }
        else
        {
            Debug.Log("Unable to find file type property");
            return "";
        }
    }
    #endregion

    #region Append File Type
    string appendFileType(object o, string basePath)
    {
        string outPath = "";
        var fI = o.GetType().GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        Debug.Log("Append. T:" + o.GetType() + " fIC:" + fI.Length);
        for (int n = 0; n <= fI.Length - 1; n++)
        {
            Debug.Log("Append. fI[" + n + "] N:" + fI[n].Name);
            if (fI[n].IsLiteral && !fI[n].IsInitOnly)
            {
                Debug.Log("[" + n + "] is constant");
                if (fI[n].Name == "fileType")
                {
                    Debug.Log("found fileType by name match");
                    outPath = basePath + fI[n].GetRawConstantValue();
                    break;
                }
                else if (fI[n].GetRawConstantValue().GetType() == typeof(string))
                {
                    string s = fI[n].GetRawConstantValue() as string;
                    if (s.Contains(".vrrkt"))
                    {
                        Debug.Log("found fileType by contains check");
                        outPath = basePath + fI[n].GetRawConstantValue();
                        break;
                    }
                }
            }
        }

        Debug.Log("outPath: " + outPath);

        if (outPath != "")
        {
            return outPath;
        }
        else
        {
            Debug.Log("Unable to append file type");
            return "";
        }
    }
    #endregion

    #region Get Files Of Type
    string[] getFilesFromDirectory(string dir, Type t)
    {
        string[] fInf = new string[0];

        #region Handle Non Existent Directory
        if (!Directory.Exists(dir))
        {
            Debug.Log("Provided directory does not exist!");
            return null;
        }
        #endregion

        #region Directory Exists
        else
        {
            #region Get Files

            fInf = Directory.GetFiles(dir);
            List<string> filt = new List<string>();
            string fT = "";

            #region Type Provided
            if (t != null)
            {
                fT = getFileType(t);
            }
            #endregion

            #region Iteration
            for (int n = 0; n <= fInf.Length - 1; n++)
            {
                #region Get Files of Type
                if (t != null)
                {
                    if (fInf[n].Contains(fT))
                    {
                        filt.Add(fInf[n]);
                    }
                }
                #endregion

                #region Get All Vrrkt Files
                else
                {
                    if (fInf[n].Contains(".vrrkt"))
                    {
                        filt.Add(fInf[n]);
                    }
                }
                #endregion
            }
            #endregion

            #endregion

            #region Trim Names
            for (int n = 0; n <= filt.Count - 1; n++)
            {
                if (filt[n].Contains("/Desktop/"))
                {
                    filt[n] = filt[n].Replace(testPathBase, "");
                }
            }
            #endregion

            return filt.ToArray();
        }
        #endregion
    }
    #endregion

    #endregion

    #region VrrktFormatter Functions

    #region Load Function
    void vrrktLoad(string path)
    {
        if (deserialize == null || !deserialize.MoveNext())
        {
            deserialize = VrrktFormatter.deserialize(File.OpenRead(path), this, path);
            VrrktCoroutine.start(deserialize);
        }
        else
        {
            Debug.Log("Must wait for currently running deserialization to finish");
        }
    }

    void vrrktLoad(string path, bool img)
    {
        VrrktCoroutine.start(VrrktFormatter.deserialize(File.OpenRead(path), this, path, true));
    }
    #endregion

    #region Save Function
    void vrrktSave(string path, object toSave)
    {
        //Debug.Log("isVrrkt=" + isVrrkt);
        if (toSave.GetType().GetInterfaces().Contains(typeof(vrrktSerializable)) || toSave.GetType().GetInterfaces().Contains(typeof(vrrktDatabaseSerializable)))
        {
            serialize = VrrktFormatter.serialize(File.Create(path), toSave, this);
            VrrktCoroutine.start(serialize);
            Debug.Log("Started Serializing " + toSave.GetType().Name);
        }
        else
        {
            Debug.Log("Cannot serialize " + toSave.GetType().Name + " because it is not marked vrrktSerializable");
        }
    }
    #endregion

    #region Passback Function
    public void vrrktPassback(int tick, int tickLim, int subTick, int subTickLim, bool isDone, object returned, Type t)
    {
        if (t != null && returned != null)
        {
            if (desReturned != null && desReturned.Count > 0)
            {
                desReturned.Add(returned);
                desType.Add(t);
            }
            else
            {
                desReturned = new List<object>();
                desType = new List<Type>();
                desReturned.Add(returned);
                desType.Add(t);
            }
        }

        tck = tick;
        tLim = tickLim;
        sTck = subTick;
        sTLim = subTickLim;

        //Debug.Log("t-" + tck + "\ntL-" + tLim + "\nsT-" + sTck + "\nsTL-" + subTickLim);

        #region Completion
        if (isDone)
        {
            //Debug.Log("Serializer has completed");
            deserialize = null;
            serialize = null;
            tck = 0;
            tLim = 0;
            sTck = 0;
            sTLim = 0;
            serChosen = -1;
            pathChosen = -1;
            customPath = "";

            if (pY == 2)
            {
                createSelection = -1;
            }
        }
        #endregion

        Repaint();
    }
    #endregion

    #endregion

    #region Async Database Methods

    #region Card Integrity Puller Control (Range)
    async void integrityControl(int lim, int operators)
    {
        pullers = new List<VrrktPullerUnit>();
        int u = 0;

        #region Set Ranges & Create Units
        while (u < operators)
        {
            await Task.Run(() =>
            {
                if (u < operators - 1)
                {
                    pullers.Add(new VrrktPullerUnit(Mathf.RoundToInt(u * (lim / operators)), Mathf.RoundToInt((u + 1) * (lim / operators)) - 1));
                }
                else
                {
                    pullers.Add(new VrrktPullerUnit(Mathf.RoundToInt(u * (lim / operators)), lim));
                }
                u++;
            });
        }
        #endregion


        u = 0;
        passDictionaries();
        while (u < operators)
        {
            VrrktCoroutine.start(pullers[u].cardIntegrity(this));
            //pullers[u].integrityCheckControl(this);
            u++;
        }
        startTime = DateTime.Now;
    }
    #endregion

    #region Card Puller Remote Control ENUM
    IEnumerator cardPullerRemote(VrrktPullerUnit u)
    {
        u.startCardCycle(this);

        while (u.current <= u.upper)
        {
            if (!u.running)
            {
                u.running = true;
                VrrktCoroutine.start(u.cardCycleComputation(u.stage, u.step));
                yield return true;
            }
            else
            {
                if (u.done)
                {
                    u.done = false;
                    u.running = false;
                    u.step++;
                    yield return true;
                }
            }

            yield return true;
        }

        Debug.Log("Finished card cycle");
        yield return false;
    }
    #endregion

    #region Image Integrity Puller Control (Range)
    async void imageIntegrityControl(int lim, int operators)
    {
        pullers = new List<VrrktPullerUnit>();
        int u = 0;

        #region Set Ranges & Create Units
        while (u < operators)
        {
            await Task.Run(() =>
            {
                if (u < operators - 1)
                {
                    pullers.Add(new VrrktPullerUnit(Mathf.RoundToInt(u * (lim / operators)), Mathf.RoundToInt((u + 1) * (lim / operators)) - 1));
                }
                else
                {
                    pullers.Add(new VrrktPullerUnit(Mathf.RoundToInt(u * (lim / operators)), lim));
                }
                u++;
            });
        }
        #endregion


        u = 0;
        passDictionaries();
        while (u < operators)
        {
            VrrktCoroutine.start(pullers[u].imageIntegrity(this));
            u++;
        }
        startTime = DateTime.Now;
    }
    #endregion

    #region Duplicate Filter Control
    async void duplicateFilterControl(int lim, int operators)
    {
        pullers = new List<VrrktPullerUnit>();
        int u = 0;

        #region Set Ranges & Create Units
        while (u < operators)
        {
            await Task.Run(() =>
            {
                if (u < operators - 1)
                {
                    pullers.Add(new VrrktPullerUnit(Mathf.RoundToInt(u * (lim / operators)), Mathf.RoundToInt((u + 1) * (lim / operators)) - 1));
                }
                else
                {
                    pullers.Add(new VrrktPullerUnit(Mathf.RoundToInt(u * (lim / operators)), lim));
                }
                u++;
            });
        }
        #endregion


        u = 0;
        passDictionaries();
        while (u < operators)
        {
            pullers[u].duplicateFilterControl(this);
            u++;
        }
        startTime = DateTime.Now;
    }
    #endregion

    #region Card Puller Creation Control
    async void pullerCreationControl(int lim, int operators)
    {
        pullers = new List<VrrktPullerUnit>();
        int u = 0;

        #region Set Ranges & Create Units
        while (u < operators)
        {
            await Task.Run(() =>
            {
                if (u < operators - 1)
                {
                    pullers.Add(new VrrktPullerUnit(Mathf.RoundToInt(u * (lim / operators)), Mathf.RoundToInt((u + 1) * (lim / operators)) - 1));
                }
                else
                {
                    pullers.Add(new VrrktPullerUnit(Mathf.RoundToInt(u * (lim / operators)), lim));
                }
                u++;
            });
        }
        #endregion


        u = 0;
        passDictionaries();
        while (u < operators)
        {
            VrrktCoroutine.start(cardPullerRemote(pullers[u]));
            //VrrktCoroutine.start(pullers[u].cardCycle(this));
            //pullers[u].asyncCardCycle(this);
            u++;
        }
        startTime = DateTime.Now;
    }
    #endregion

    #region Image Puller Creation Control
    async void imagePullerCreationControl(int lim, int operators)
    {

        //Debug.Log("Creating Image Pullers");
        pullers = new List<VrrktPullerUnit>();
        int u = 0;

        #region Set Ranges & Create Units
        while (u < operators)
        {
            await Task.Run(() =>
            {
                if (u < operators - 1)
                {
                    pullers.Add(new VrrktPullerUnit(Mathf.RoundToInt(u * (lim / operators)), Mathf.RoundToInt((u + 1) * (lim / operators)) - 1));
                }
                else
                {
                    pullers.Add(new VrrktPullerUnit(Mathf.RoundToInt(u * (lim / operators)), lim));
                }
                u++;
            });
        }
        #endregion


        u = 0;
        passDictionaries();
        while (u < operators)
        {
            VrrktCoroutine.start(pullers[u].combinedENUMImage(this));
            u++;
        }
        startTime = DateTime.Now;
    }
    #endregion

    #region Get Total Repulled
    void getTotalRepulled()
    {
        repullTotal = 0;

        for (int n = 0; n <= pullers.Count - 1; n++)
        {
            repullTotal += pullers[n].repulled;
        }
    }
    #endregion

    #region Clear Database ENUM
    IEnumerator clearDatabase()
    {
        clearFiles = Directory.GetFiles("C:/Users/robzo/Desktop/VrrktCentralDatabase/Database/");

        while (clearProgress < clearFiles.Length)
        {
            if (clearCards && clearFiles[clearProgress].Contains(".vrrktCd"))
            {
                File.Delete(clearFiles[clearProgress]);
                clearProgress++;
            }
            else if (clearImages && clearFiles[clearProgress].Contains(".vrrktCi"))
            {
                File.Delete(clearFiles[clearProgress]);
                clearProgress++;
            }

            if (clearProgress < clearFiles.Length)
            {
                if (clearCards && clearFiles[clearProgress].Contains(".vrrktCd"))
                {
                    File.Delete(clearFiles[clearProgress]);
                    clearProgress++;
                }
                else if (clearImages && clearFiles[clearProgress].Contains(".vrrktCi"))
                {
                    File.Delete(clearFiles[clearProgress]);
                    clearProgress++;
                }
            }

            if (clearProgress < clearFiles.Length)
            {
                if (clearCards && clearFiles[clearProgress].Contains(".vrrktCd"))
                {
                    File.Delete(clearFiles[clearProgress]);
                    clearProgress++;
                }
                else if (clearImages && clearFiles[clearProgress].Contains(".vrrktCi"))
                {
                    File.Delete(clearFiles[clearProgress]);
                    clearProgress++;
                }
            }

            if (clearProgress < clearFiles.Length)
            {
                if (clearCards && clearFiles[clearProgress].Contains(".vrrktCd"))
                {
                    File.Delete(clearFiles[clearProgress]);
                    clearProgress++;
                }
                else if (clearImages && clearFiles[clearProgress].Contains(".vrrktCi"))
                {
                    File.Delete(clearFiles[clearProgress]);
                    clearProgress++;
                }
            }

            if (clearProgress < clearFiles.Length)
            {
                if (clearCards && clearFiles[clearProgress].Contains(".vrrktCd"))
                {
                    File.Delete(clearFiles[clearProgress]);
                    clearProgress++;
                }
                else if (clearImages && clearFiles[clearProgress].Contains(".vrrktCi"))
                {
                    File.Delete(clearFiles[clearProgress]);
                    clearProgress++;
                }
            }
            yield return true;
        }

        Debug.Log("Completed!");
        yield return false;
    }
    #endregion

    #region Clear All Images
    async void clearImagesDatabase()
    {
        string[] iF = null;

        #region Get All Image Files
        await Task.Run(() =>
        {
            iF = Directory.GetFiles(centralDatabasePathBase, "*.vrrktCi");
            totalProg = iF.Length;
            clearProg = 0;
        });
        #endregion

        #region Clear Image Files
        while (clearProg < iF.Length)
        {
            await Task.Run(() =>
            {
                for (int n = 0; n <= 99; n++)
                {
                    if (clearProg < iF.Length)
                    {
                        if (iF[clearProg].Contains(".vrrktCi"))
                        {
                            File.Delete(iF[clearProg]);
                        }

                        clearProg++;
                    }
                    else
                    {
                        break;
                    }
                }
            });
            Repaint();
        }
        #endregion

        Init();
        OnEnable();
    }
    #endregion

    #region Stop Operations
    void stopOps()
    {
        Debug.Log("VED stop ops");
        for (int n = 0; n <= pullers.Count - 1; n++)
        {
            pullers[n].Stop();
        }
    }
    #endregion

    #region Get Total Progress
    void getTotalProgress()
    {
        totalProg = 0;
        for (int n = 0; n <= pullers.Count - 1; n++)
        {
            totalProg += (pullers[n].current - pullers[n].lower) + 1;
        }
    }
    #endregion

    #region Get Total Blanks
    void getTotalBlanks()
    {
        totalBlank = 0;
        for (int n = 0; n <= pullers.Count - 1; n++)
        {
            totalBlank += pullers[n].blanks.Count;
        }
    }
    #endregion

    #region Pass Dictionaries to Pullers
    void passDictionaries()
    {
        //Debug.Log("Pass Dictionaries");

        Dictionary<string, string> dict;

        if (pullerContainer == null)
        {
            pullerContainer = new pullerDataContainer();
        }

        for (int u = 1; u <= 3; u++)
        {
            dict = pullerContainer.createDictionary(u);

            for (int n = 0; n <= pullers.Count - 1; n++)
            {
                if (u == 1)
                {
                    pullers[n].colorDict = dict;
                }

                if (u == 2)
                {
                    pullers[n].typeDict = dict;
                }

                if (u == 3)
                {
                    pullers[n].lastDict = dict;
                }
            }
        }
    }
    #endregion

    #endregion
    
    #region GID Table Creation Methods

    #region Control
    async void gidTableCreateControl(int i)
    {
        #region Startup Operations
        Debug.Log("Startup");
        step = "Startup Operations";
        fNames = null;
        ranges = new List<Vector2Int>();
        currents = new List<int>();
        #endregion

        #region Handle Null vSys
        step = "Handle null vSys";
        if (vSys == null)
        {
            FileStream f = File.OpenRead("C:/Users/robzo/Desktop/VrrktCentralDatabase/Resources/VrrktSystemData.vrrktDs");
            vSys = new VrrktSystemData();
            vSys.readFromStream(f);
            f.Close();
            f.Dispose();
            Debug.Log("Loaded vSys");
        }
        Repaint();
        #endregion

        #region Get Files
        Debug.Log("Get Files");
        step = "Get Files";
        await getFiles();
        Repaint();
        #endregion

        #region Calculate Ranges
        Debug.Log("Calculate Ranges");
        Debug.Log("T C: " + tables.Count + ", vSys U C: " + vSys.userIDs.Count);
        step = "Calculate Ranges";
        await calculateRanges(i);
        Repaint();
        #endregion

        #region Trim Names
        Debug.Log("Trim Names");
        step = "Trim Names";
        current = 0;
        await trimNames();
        Repaint();
        #endregion

        #region Integrity Check
        Debug.Log("Integrity Check");
        step = "Integrity Check";
        current = 0;
        await integrityCheck();
        Repaint();
        #endregion

        #region Populate Tables
        Debug.Log("Populate Tables");
        step = "Populate Tables";
        await populateTables();
        Repaint();
        #endregion

        #region Save Tables
        Debug.Log("Save Tables");
        step = "Save Tables";
        await saveTables();
        Repaint();
        #endregion

        cDbPhase = 0;
        pY = 0;
    }
    #endregion

    #region Get Files Method
    async Task getFiles()
    {
        await Task.Run(() =>
        {
            fNames = Directory.GetFiles("C:/Users/robzo/Desktop/VrrktCentralDatabase/Database/", "*.vrrktCd");
        });
        Repaint();
    }
    #endregion

    #region Calculate Ranges Method
    async Task calculateRanges(int i)
    {
        int c = 0;
        int q = 0;
        int l = 0;
        int up = 0;
        c = fNames.Length;
        q = c / i;

        await Task.Run(() =>
        {
            for (int n = 0; n <= i - 1; n++)
            {
                tables[n].userID = vSys.userIDs[n];
            }
        });

        await Task.Run(() =>
        {
            for (int n = 0; n <= i - 1; n++)
            {
                l = q * n;

                if (n < i - 1)
                {
                    up = (q * (n + 1)) - 1;
                }
                else
                {
                    up = c - 1;
                }
                ranges.Add(new Vector2Int(l, up));
                currents.Add(0);
            }
        });
    }
    #endregion

    #region Trim Names Method
    async Task trimNames()
    {
        while (current < fNames.Length)
        {
            await Task.Run(() =>
            {
                for (int n = 0; n <= 10000; n++)
                {
                    if (current < fNames.Length)
                    {
                        //fNames[current] = fNames[current].Substring(fNames[current].IndexOf("/Database/") + 10, fNames[current].LastIndexOf(".vrrkt"));

                        fNames[current] = fNames[current].Substring(fNames[current].LastIndexOf("/") + 1);
                        fNames[current] = fNames[current].Substring(0, fNames[current].IndexOf("."));
                        //Debug.Log(fNames[current]);
                        current++;
                    }
                    else
                    {
                        break;
                    }
                }
            });
            Repaint();
        }

        //await Task.Delay(1000000000);
    }
    #endregion

    #region Integrity Check Method
    async Task integrityCheck()
    {
        int q = -1;
        while (current < fNames.Length)
        {
            await Task.Run(() =>
            {
                for (int n = 0; n <= 10000; n++)
                {
                    if (current < fNames.Length)
                    {
                        if (!int.TryParse(fNames[current], out q))
                        {
                            //Debug.Log("Failed Integrity Verification. S: " + names[current]);
                        }
                        current++;
                    }
                    else
                    {
                        break;
                    }
                }
            });
            Repaint();
        }
    }
    #endregion

    #region Table Population Method
    async Task populateTables()
    {
        int c = 0;
        while (c < currents.Count)
        {
            await Task.Run(() =>
            {
                for (int x = 0; x <= 10000; x++)
                {
                    if (c < currents.Count)
                    {
                        for (int n = 0; n <= currents.Count - 1; n++)
                        {
                            if (currents[n] < (ranges[n].y - ranges[n].x))
                            {
                                if (c < currents.Count)
                                {
                                    try
                                    {
                                        if (currents[n] + ranges[n].x - 1 < fNames.Length)
                                        {
                                            if (currents[n] + ranges[n].x - 1 >= 0)
                                            {
                                                tables[n].table.Add(int.Parse(fNames[(currents[n] + ranges[n].x) - 1]));
                                            }
                                            else
                                            {
                                                tables[n].table.Add(int.Parse(fNames[(currents[n] + ranges[n].x)]));
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.Log("Exception. n: " + n + ", t C: " + tables.Count + ", sum: " + (currents[n] + ranges[n].x - 1) + ", fN C: " + fNames.Length);
                                    }
                                    currents[n]++;

                                    if (currents[n] >= (ranges[n].y - ranges[n].x))
                                    {
                                        c++;
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            });
            Repaint();
        }
    }
    #endregion

    #region Save Tables Method
    async Task saveTables()
    {
        FileStream f = null;
        int c = 0;
        while (c < tables.Count)
        {
            f = File.Create("C:/Users/robzo/Desktop/VrrktCentralDatabase/Resources/" + c + ".vrrktGt");
            await Task.Run(() => tables[c].writeToStream(f));
            f.Close();
            f.Dispose();
            c++;
            Repaint();
        }
    }
    #endregion

    #endregion

    #region Implement Database Creators
     void implementCreators(bool b, int i)
    {
        dbCreators = new List<v3DatabaseCreator>(i);
        for (int n = 0; n <= i - 1; n++)
        {
            dbCreators.Add(new v3DatabaseCreator());
        }

        #region Card
        if (b)
        {
            for (int n = 0; n <= i - 1; n++)
            {
                dbCreators[n].startCardDatabase(n, this);
            }
        }
        #endregion

        #region Image
        else
        {
            for (int n = 0; n <= i - 1; n++)
            {
                dbCreators[n].startImageDatabase(n, this);
            }
        }
        #endregion
    }
    #endregion

    #region Card Database Creation Methods

    #region Control
    async void cardDatabaseControl()
    {
        #region Handle Null vSys
        step = "Handle null vSys";
        //Debug.Log(step);
        if (vSys == null)
        {
            FileStream e = File.OpenRead("C:/Users/robzo/Desktop/VrrktCentralDatabase/Resources/VrrktSystemData.vrrktDs");
            vSys = new VrrktSystemData();
            vSys.readFromStream(e);
            e.Close();
            e.Dispose();
            //Debug.Log("Loaded vSys");
        }
        Repaint();
        #endregion

        #region Set Variables
        step = "Set Variables";
        int c = 0;
        string[] tbls = Directory.GetFiles("C:/Users/robzo/Desktop/VrrktCentralDatabase/Resources/", "*.vrrktGt");
        tables = new List<vrrktGIDTable>(tbls.Length);
        currents = new List<int>(tbls.Length);
        limits = new List<int>(tbls.Length);
        lDbc = new List<VrrktUserDatabase>(tbls.Length);
        for (int n = 0; n <= tbls.Length - 1; n++)
        {
            currents.Add(0);
            limits.Add(0);
            lDbc.Add(new VrrktUserDatabase());
            lDbc[n].user = vSys.userIDs[n];
        }
        Repaint();
        #endregion


        while (c < tbls.Length)
        {
            await cardDatabaseOperation(c);

            c++;
        }

        cDbPhase = 0;
        pY = 0;
    }
    #endregion

    #region Operation
    async Task cardDatabaseOperation(int c)
    {
        vrrktGIDTable table;

        #region Load Table
        f = File.Open("C:/Users/robzo/Desktop/VrrktCentralDatabase/Resources/" + c + ".vrrktGt", FileMode.Open);
        table = new vrrktGIDTable();
        await Task.Run(() => table.readFromStream(f));
        f.Close();
        f.Dispose();
        f = null;
        tables.Add(table);
        //Debug.Log("Table[" + n + "] C: " + table.table.Count);
        Repaint();
        #endregion

        #region Populate Database
        lDbc.Add(new VrrktUserDatabase());
        lDbc[c].user = vSys.userIDs[c];
        step = "Populate Card Database";
        currents[c] = 0;
        limits[c] = tables[c].table.Count;
        VrrktCard card = null;
        while (currents[c] < limits[c])
        {
            f = File.Open("C:/Users/robzo/Desktop/VrrktCentralDatabase/Database/" + tables[c].table[currents[c]] + ".vrrktCd", FileMode.Open);
            card = new VrrktCard();
            await Task.Run(() =>
            {
                card.readFromStream(f);
            });
            f.Close();
            f.Dispose();
            f = null;
            lDbc[c].setCard(card);
            card = null;
            currents[c]++;
            Repaint();
        }
        #endregion

        #region Save Database
        step = "Save Database[" + c + "] T C: " + tables[c].table.Count + ", DB C: " + lDbc[c].database.Count;
        Debug.Log(step);
        int l = 0;
        currents[c] = 0;
        limits[c] = 1;
        while (currents[c] < limits[c])
        {
            for (int x = 0; x <= 10000; x++)
            {
                if (currents[c] < limits[c])
                {
                    f = File.Create("C:/Users/robzo/Desktop/VrrktCentralDatabase/Database/" + lDbc[c].user + ".vrrktcDb");
                    l = lDbc[c].beginWriteToStream(f);

                    await Task.Run(() =>
                    {
                        for (int z = 0; z <= l; z++)
                        {
                            lDbc[c].writeToStream(z);
                        }
                        f.Close();
                        f.Dispose();
                    });
                    currents[c]++;
                }
                Repaint();
            }
        }
        #endregion
    }
    #endregion

    #region Load Tables
    async Task loadTables()
    {
        step = "Load GID Table";
        Debug.Log(step);
        FileStream f;
        vrrktGIDTable table;
        string[] tbls = Directory.GetFiles("C:/Users/robzo/Desktop/VrrktCentralDatabase/Resources/", "*.vrrktGt");
        //Debug.Log("tbls C: " + tbls.Length);

        tables = new List<vrrktGIDTable>();
        for (int n = 0; n <= tbls.Length - 1; n++)
        {
            tables.Add(new vrrktGIDTable());
        }

        for (int n = 0; n <= tbls.Length - 1; n++)
        {
            f = File.Open("C:/Users/robzo/Desktop/VrrktCentralDatabase/Resources/" + n + ".vrrktGt", FileMode.Open);
            table = new vrrktGIDTable();
            await Task.Run(() => table.readFromStream(f));
            f.Close();
            f.Dispose();
            tables[n] = table;
            //Debug.Log("Table[" + n + "] C: " + table.table.Count);
            Repaint();
        }
    }
    #endregion
    
    #region Populate Databases
    async Task populateCardDatabases()
    {
        step = "Populate Card Databases";
        Debug.Log(step);
        FileStream f;
        int w = 0;
        VrrktCard card;
        while (w < tables.Count)
        {
            for (int x = 0; x <= 100; x++)
            {
                if (w < tables.Count)
                {
                    await Task.Run(() =>
                    {
                        if (currents[w] < limits[w])
                        {
                            f = File.OpenRead("C:/Users/robzo/Desktop/VrrktCentralDatabase/Database/" + tables[w].table[currents[w]] + ".vrrktCd");
                            card = new VrrktCard();
                            card.readFromStream(f);
                            f.Close();
                            f.Dispose();
                            lDbc[w].setCard(card);
                            card = null;
                            currents[w]++;

                            if (currents[w] >= limits[w])
                            {
                                Debug.Log("DBc[" + w + "] C: " + lDbc[w].database.Count);
                                w++;
                            }
                        }
                    });
                }
                else
                {
                    break;
                }
            }
            Repaint();
        }
    }
    #endregion

    #region Save Databases
    async Task saveCardDatabases()
    {
        step = "Save Database";
        Debug.Log(step);
        FileStream f;
        int c = 0;
        int l = 0;
        for (int n = 0; n <= tables.Count - 1; n++)
        {
            currents[n] = 0;
            limits[n] = 1;
        }

        while (c < tables.Count)
        {
            for (int x = 0; x <= 10000; x++)
            {
                if (c < tables.Count)
                {
                    for (int n = 0; n <= tables.Count - 1; n++)
                    {
                        if (currents[n] < limits[n])
                        {
                            f = File.Create("C:/Users/robzo/Desktop/VrrktCentralDatabase/Database/" + lDbc[n].user + ".vrrktcDb");
                            l = lDbc[n].beginWriteToStream(f);

                            await Task.Run(() =>
                            {
                                for (int z = 0; z <= l; z++)
                                {
                                    lDbc[n].writeToStream(z);
                                }
                                f.Close();
                                f.Dispose();
                            });
                            currents[n]++;

                            if (currents[n] >= limits[n])
                            {
                                c++;
                            }
                        }
                        Repaint();
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }
    #endregion

    #endregion

    #region Image Database Creation Methods

    #region Control
    async void imageDatabaseControl()
    {
        #region Handle Null vSys
        step = "Handle null vSys";
        Debug.Log(step);
        if (vSys == null)
        {
            FileStream f = File.OpenRead("C:/Users/robzo/Desktop/VrrktCentralDatabase/Resources/VrrktSystemData.vrrktDs");
            vSys = new VrrktSystemData();
            vSys.readFromStream(f);
            f.Close();
            f.Dispose();
            Debug.Log("Loaded vSys");
        }
        Repaint();
        #endregion

        #region Load Tables
        await loadTables();
        Repaint();
        #endregion

        #region Set Variables
        step = "Set Variables";
        Debug.Log(step);
        currents = new List<int>();
        limits = new List<int>();
        lDbi = new List<VrrktUserImageDatabase>();
        for (int n = 0; n <= tables.Count - 1; n++)
        {
            currents.Add(0);
            limits.Add(tables[n].table.Count);
            lDbi.Add(new VrrktUserImageDatabase());
            lDbi[n].user = vSys.userIDs[n];
        }
        Repaint();
        #endregion

        #region Populate Databases
        await populateImageDatabases();
        Repaint();
        #endregion

        #region Save Databases
        await saveImageDatabases();
        Repaint();
        #endregion

        cDbPhase = 0;
        pY = 0;
    }
    #endregion
    
    #region Populate Databases
    async Task populateImageDatabases()
    {
        step = "Populate Image Databases";
        Debug.Log(step);
        FileStream f;
        int c = 0;
        VrrktCardImage image;
        while (c < tables.Count)
        {
            for (int x = 0; x <= 10000; x++)
            {
                if (c < tables.Count)
                {
                    if (currents[c] < limits[c])
                    {
                        await Task.Run(() =>
                        {
                            f = File.OpenRead("C:/Users/robzo/Desktop/VrrktCentralDatabase/Database/" + tables[c].table[currents[c]] + ".vrrktCi");
                            image = new VrrktCardImage();
                            image.readFromStream(f);
                            f.Close();
                            f.Dispose();
                            lDbi[c].setImage(image);
                            image = null;
                            currents[c]++;
                        });

                        if (currents[c] >= limits[c])
                        {
                            Debug.Log("DBi[" + c + "] C: " + lDbi[c].database.Count);
                            c++;
                        }
                    }
                    Repaint();
                }
                else
                {
                    break;
                }
            }
        }
    }
    #endregion

    #region Save Databases
    async Task saveImageDatabases()
    {
        step = "Save Database";
        Debug.Log(step);
        FileStream f;
        int c = 0;
        int l = 0;
        for (int n = 0; n <= tables.Count - 1; n++)
        {
            currents[n] = 0;
            limits[n] = 1;
        }

        while (c < tables.Count)
        {
            for (int x = 0; x <= 10000; x++)
            {
                if (c < tables.Count)
                {
                    for (int n = 0; n <= tables.Count - 1; n++)
                    {
                        if (currents[n] < limits[n])
                        {
                            f = File.Create("C:/Users/robzo/Desktop/VrrktCentralDatabase/Database/" + lDbi[n].user + ".vrrktiDb");
                            l = lDbi[n].beginWriteToStream(f);

                            await Task.Run(() =>
                            {
                                for (int z = 0; z <= l; z++)
                                {
                                    lDbi[n].writeToStream(z);
                                }
                                f.Close();
                                f.Dispose();
                            });
                            currents[n]++;

                            if (currents[n] >= limits[n])
                            {
                                c++;
                            }
                        }
                        Repaint();
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }
    #endregion

    #endregion
    
    #region Clear Cards & Images ENUM
    IEnumerator clearInstallDatabase()
    {
        Debug.Log("Began Install Clear");

        #region Startup Operations
        string p = Application.persistentDataPath + "/Database/";
        string[] cards = Directory.GetFiles(Application.persistentDataPath + "/Database/", "*.vrrktCd");
        Debug.Log("Cards C: " + cards.Length + ", path: " + p);
        yield return true;

        p = Application.persistentDataPath + "/Database/Images/";
        string[] imgs = Directory.GetFiles(Application.persistentDataPath + "/Database/Images/", "*.vrrktCi");
        Debug.Log("Images C: " + imgs.Length + ", path: " + p);
        yield return true;

        prog = 0;
        progLim = cards.Length + imgs.Length;
        yield return true;
        #endregion

        #region Deletion Iterator

        #region Delete Cards
        while (prog < cards.Length)
        {
            for(int n = prog; n <= prog + 100; n++)
            {
                File.Delete(cards[prog]);
                prog++;

                if (prog >= cards.Length)
                {
                    break;
                }
            }

            Repaint();
            yield return true;
        }
        #endregion

        #region Delete Images
        while (prog < progLim)
        {
            for (int n = prog; n <= prog + 100; n++)
            {
                File.Delete(imgs[prog - cards.Length]);
                prog++;

                if (prog >= progLim)
                {
                    break;
                }
            }

            Repaint();
            yield return true;
        }
        #endregion

        #endregion

        yield return false;
    }
    #endregion
}
#endregion
