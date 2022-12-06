#region Libraries
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Verruckt.Operations;
using Verruckt.Editor;
#endregion

#region Main Class
[ExecuteInEditMode]
[CustomEditor(typeof(ScriptableObject))]
[CanEditMultipleObjects]
public class AsyncSocketServer : EditorWindow
{
    #region Variables
    Socket server;
    List<AsyncSocketUnit> units;
    VrrktSystemData vSys;
    Vector2 pos;
    public List<string> console;
    bool serverOn, serverListen;
    string[] commands = { "Start Server", "Shut Down Server" };
    string[] states = { "Request ID", "Receive ID", "Accept/Deny", "Request Install Request", "Receive Install Request", "Send GID Table", "ACK", "Send Card", "ACK",
        "Send Cards Done", "ACK", "Send Image", "ACK", "Send Images Done", "ACK", "Send Closing" };
    int commandSelected;
    public float sT, totTime;
    #endregion

    #region INIT
    [MenuItem("Window/Custom/vrrktSocketServer")]
    static void Init()
    {
        AsyncSocketServer vS = (AsyncSocketServer)GetWindow(typeof(AsyncSocketServer));
        vS.maxSize = new Vector2(Screen.width, Screen.height);
        EditorUtility.SetDirty(vS);
        vS.Show();
    }
    #endregion

    #region OnEnable
    private void OnEnable()
    {
        states = new string[] {
            "Request ID", "Receive ID", "Accept/Deny", 
        "Send Card Database", "ACK", "Send Image Database", "ACK", "Send Closing" };
        pos = Vector2.zero;
        console = new List<string>();
        units = new List<AsyncSocketUnit>();
        for (int n = 0; n <= 7; n++)
        {
            units.Add(new AsyncSocketUnit(this));
        }
        serverOn = false;
        serverListen = false;
        commandSelected = -1;

        load("C:/Users/robzo/Desktop/VrrktCentralDatabase/Resources/VrrktSystemData.vrrktDs", typeof(VrrktSystemData));
    }
    #endregion

    #region GUI
    private void OnGUI()
    {
        GUI.skin.button.stretchWidth = true;
        if (vSys != null)
        {
            #region Connection Display Header

            #region Main Header
            GUI.skin.box.stretchWidth = true;
            GUILayout.BeginVertical();

            #region Unit Displays
            GUILayout.BeginHorizontal();
            for (int n = 0; n <= units.Count - 1; n++)
            {
                GUILayout.BeginVertical();

                #region Connected Toggle
                if (units[n].client != null)
                {
                    GUILayout.Toggle(units[n].client.Connected, units[n].id, GUILayout.Width((Screen.width / 8) - 10));
                }
                else
                {
                    GUILayout.Toggle(false, units[n].id, GUILayout.Width((Screen.width / 8) - 10));
                }
                #endregion

                #region State Value & Label
                GUILayout.Box("State: " + units[n].state, GUILayout.Width((Screen.width / 8) - 10), GUILayout.Height(50f));

                if (units[n].state < states.Length)
                {
                    GUILayout.Box(states[units[n].state], GUILayout.Width((Screen.width / 8) - 10), GUILayout.Height(50f));
                }
                #endregion

                #region cP
                GUILayout.Box("cP: " + units[n].cP, GUILayout.Width((Screen.width / 8) - 10), GUILayout.Height(50f));
                #endregion
                
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            #endregion

            #region Total Connections
            if (units != null)
            {
                int q = 0;
                for (int n = 0; n <= units.Count - 1; n++)
                {
                    if (units[n].client != null)
                    {
                        if (units[n].client.Connected)
                        {
                            q++;
                        }
                    }
                }

                GUILayout.Box("Clients Connected: " + q);
            }
            #endregion

            GUILayout.EndVertical();
            GUI.skin.box.stretchWidth = false;
            #endregion

            #region Handle Time Tracking
            GUI.skin.box.stretchWidth = true;
            GUILayout.BeginHorizontal();

            for (int n = 0; n <= units.Count - 1; n++)
            {
                string v = "";
                if (units[n].totTime > 0)
                {
                    if (units[n].totTime / 60 >= 1)
                    {
                        if (units[n].totTime / 3600 >= 1)
                        {
                            v = ((int)units[n].totTime / 3600) + "h, " + ((int)units[n].totTime % 3600) + "m, " + ((int)units[n].totTime % 60) + "s";
                        }
                        else
                        {
                            v = ((int)units[n].totTime / 60) + "m, " + ((int)units[n].totTime % 60) + "s";
                        }
                    }
                    else
                    {
                        v = ((int)units[n].totTime) + "s";
                    }
                    GUILayout.Box("Time Running: " + v, GUILayout.Width((Screen.width / 8) - 10));
                }
            }

            GUILayout.EndHorizontal();
            #endregion
            
            #endregion

            #region Server Console

            #region Console Display
            pos = GUILayout.BeginScrollView(pos);

            for (int n = 0; n <= console.Count - 1; n++)
            {
                GUILayout.Label(console[n]);
            }

            GUILayout.EndScrollView();
            #endregion

            #region Clear Console Button
            if (GUILayout.Button("Clear Console", GUILayout.Width(Screen.width - 10)))
            {
                console = new List<string>();
            }
            #endregion

            #endregion

            #region Server Control Station
            GUILayout.Space(10);

            #region Accept Clients
            GUILayout.BeginHorizontal();

            if (server != null)
            {
                if (GUILayout.Button("Accept Available Clients"))
                {
                    acceptClients();
                }
            }

            GUILayout.EndHorizontal();
            #endregion

            #region Main Controls
            commandSelected = GUILayout.SelectionGrid(commandSelected, commands, commands.Length, GUILayout.Width(Screen.width - 10));

            #region Control Logic
            if (commandSelected != -1)
            {
                #region Start Server
                if (commandSelected == 0)
                {
                    startServer();
                }
                #endregion

                #region Shut Down Server
                if (commandSelected == 1)
                {
                    closeServer();
                }
                #endregion

                commandSelected = -1;
            }
            #endregion

            #endregion

            GUILayout.Space(40);
            #endregion
        }
    }
    #endregion

    #region Start/Close Server Functions

    #region Async Start Server
    void startServer()
    {
        if (!serverOn)
        {
            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse("192.168.0.46"), 11111);
            server = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(ipe);
            server.Listen(8);
            serverOn = true;
            console.Add("Started server listening");
        }
    }
    #endregion

    #region Async Close Server
    void closeServer()
    {
        if (serverOn)
        {
            server.Close();
            serverOn = false;
            console.Add("Closed server");
        }
    }
    #endregion

    #endregion

    #region Accept Clients Functions

    #region Accept Clients
    void acceptClients()
    {
        if (serverOn)
        {
            server.BeginAccept(new AsyncCallback(AcceptClientCallback), server);
            if (!console.Contains("Started accept clients"))
            {
                console.Add("Started accept clients");
            }
        }
    }
    #endregion

    #region Accept Client Callback
    public void AcceptClientCallback(IAsyncResult ar)
    {
        server = (Socket)ar.AsyncState;
        Socket cl = server.EndAccept(ar);

        if (cl.Connected)
        {
            for(int n = 0; n <= units.Count - 1; n++)
            {
                if (units[n].client == null)
                {
                    units[n].beginOperation(cl);
                    break;
                }
            }
            console.Add("Connected to a client.");
        }
    }
    #endregion

    #endregion

    #region VrrktFormatter Methods

    #region Load Method
    void load(string path, Type t)
    {
        int i = -1;

        #region Specify Type

        #region Card 0
        if (t == typeof(VrrktCard))
        {
            i = 0;
        }
        #endregion

        #region Image 1
        if (t == typeof(VrrktCardImage))
        {
            i = 1;
        }
        #endregion

        #region System 2
        if (t == typeof(VrrktSystemData))
        {
            i = 2;
            //Debug.Log("Loading VrrktSystemData!");
        }
        #endregion

        #region Operations 3
        if (t == typeof(VrrktOperationsData))
        {
            i = 3;
        }
        #endregion

        #region User 4
        if (t == typeof(VrrktUser))
        {
            i = 4;
        }
        #endregion

        #region Report 5 XXXXXXXXXXXXXXXXXXXXX

        #endregion

        #region GID Table 6
        if (t == typeof(vrrktGIDTable))
        {
            i = 6;
        }
        #endregion

        #endregion

        FileStream f = File.Open(path, FileMode.Open, FileAccess.ReadWrite);
        VrrktCoroutine.start(VrrktFormatter.deserialize(f, this, path, i));
    }
    #endregion

    #region Save Method
    void save(string path, object o)
    {
        FileStream f = File.Create(path);
        VrrktCoroutine.start(VrrktFormatter.serialize(f, o, this));
    }
    #endregion

    #region Object Passback
    public void appPassback(int t, int tL, object o, int i)
    {
        #region Handle Object Passback
        if (o != null)
        {
            #region Card 0
            if (i == 0)
            {

            }
            #endregion

            #region Image 1
            if (i == 1)
            {

            }
            #endregion

            #region System 2
            if (i == 2)
            {
                vSys = (VrrktSystemData)o;
            }
            #endregion

            #region GID Table 6
            if (i == 6)
            {

            }
            #endregion
        }
        #endregion
    }
    #endregion

    #region Save Passback
    public void vrrktPassback(int t, int tL, int sT, int sTL, bool isDone, object re, Type ty)
    {

    }
    #endregion

    #endregion

    #region Unit Resource Functions

    #region Get VrrktSystemData
    public VrrktSystemData getVsys()
    {
        if (vSys != null)
        {
            return vSys;
        }
        else
        {
            return null;
        }
    }
    #endregion

    #endregion
}
#endregion
