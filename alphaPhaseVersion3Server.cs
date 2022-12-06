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
#endregion

#region Main Class
namespace Verruckt
{
    [ExecuteInEditMode]
    [CustomEditor(typeof(ScriptableObject))]
    [CanEditMultipleObjects]
    public class alphaPhaseVersion3Server : EditorWindow
    {
        #region Variables

        #region Sync Variables
        TcpListener server;
        List<Socket> sClients;
        VrrktSystemData vSys;
        List<vrrktGIDTable> tables;
        List<VrrktCard> cards;
        List<VrrktCardImage> images;
        List<Stream> streams;
        VrrktOperationsData vOps;
        VrrktUser vUser;
        Vector2 consolePos;
        IEnumerator listener;
        List<IEnumerator> connectors;
        List<bool> connections;
        List<string> serverConsole, paths, sIns;
        List<int> commStates, clientID, unitCount, commP, ns;
        List<byte[]> datIn;
        List<Vector2> phaseNstep;
        string[] commands = { "Start Server", "Shut Down Server" };
        string[] states = { "Request ID", "Receive ID", "Accept/Deny", "Receive Install Request", "Send GID Table", "Receive Acknowledge", "Send Cards",
        "Send Cards Completed", "Receive Acknowledge", "Send Images", "Send Images Completed", "Receive Acknowledge", "Send 'Closing'" };
        int commandSelected;
        float xT, yT, delay;
        public float sT, totTime;
        #endregion

        #region Async Variables
        bool serverRunning;
        #endregion

        #endregion

        #region Init Function
        [MenuItem("Window/Custom/vrrktServer2")]
        static void Init()
        {
            alphaPhaseVersion3Server vS = (alphaPhaseVersion3Server)GetWindow(typeof(alphaPhaseVersion3Server));
            vS.maxSize = new Vector2(Screen.width, Screen.height);
            EditorUtility.SetDirty(vS);
            vS.Show();
        }
        #endregion

        #region OnEnable Function
        private void OnEnable()
        {
            AssetDatabase.Refresh();
            
            commandSelected = -1;

            #region Assorted Variable SetPoints Set
            connections = new List<bool>();
            serverConsole = new List<string>();
            consolePos = Vector2.zero;
            commStates = new List<int>();
            phaseNstep = new List<Vector2>();
            clientID = new List<int>();
            unitCount = new List<int>();
            commP = new List<int>();
            ns = new List<int>();
            paths = new List<string>();
            sIns = new List<string>();
            streams = new List<Stream>();
            datIn = new List<byte[]>();
            sT = 0;
            totTime = 0;
            xT = 0;
            yT = 0;
            delay = 0;
            for (int n = 0; n <= 7; n++)
            {
                commStates.Add(0);
                connections.Add(false);
                phaseNstep.Add(Vector2.down);
                clientID.Add(-1);
                unitCount.Add(0);
                commP.Add(0);
                ns.Add(0);
                paths.Add("");
                sIns.Add("");
                streams.Add(new MemoryStream());
                datIn.Add(null);
            }
            #endregion

            #region Async Variable Setpoints
            serverRunning = false;
            #endregion

            load("C:/Users/robzo/Desktop/VrrktCentralDatabase/Resources/VrrktSystemData.vrrktDs", typeof(VrrktSystemData));
        }
        #endregion

        #region GUI Function
        private void OnGUI()
        {
            GUI.skin.button.stretchWidth = true;
            if (vSys != null)
            {
                #region Connection Display Header

                #region Main Header
                GUILayout.BeginHorizontal();
                GUI.skin.box.stretchWidth = true;
                for (int n = 0; n <= vSys.userIDs.Count - 1; n++)
                {
                    GUILayout.BeginVertical();

                    GUILayout.Toggle(connections[n], vSys.userIDs[n]);
                    GUILayout.Box("CS: " + commStates[n], GUILayout.MinWidth(10f));
                    if (commStates[n] < states.Length)
                    {
                        GUILayout.Box(states[commStates[n]], GUILayout.MinWidth(10f));
                    }
                    GUILayout.Box("CP: " + commP[n], GUILayout.MinWidth(10f));
                    if (commStates[n] == 6)
                    {
                        GUILayout.Box("Cards: " + (unitCount[n] + 1), GUILayout.MinWidth(10f));
                    }
                    else if (commStates[n] == 9)
                    {
                        GUILayout.Box("Images: " + (unitCount[n] + 1), GUILayout.MinWidth(10f));
                    }

                    GUILayout.EndVertical();
                }

                if (sClients != null)
                {
                    GUILayout.Box("Clients Connected: " + sClients.Count);
                }
                GUI.skin.box.stretchWidth = false;
                GUILayout.EndHorizontal();
                #endregion

                #region Handle Time Tracking
                string s = "";
                if (totTime / 60 >= 1)
                {
                    if (totTime / 3600 >= 1)
                    {
                        s = ((int)totTime / 3600) + "h, " + ((int)totTime % 3600) + "m, " + ((int)totTime % 60) + "s";
                    }
                    else
                    {
                        s = ((int)totTime / 60) + "m, " + ((int)totTime % 60) + "s";
                    }
                }
                else
                {
                    s = ((int)totTime) + "s";
                }
                #endregion

                #region Display Time & Rate
                if (totTime > 0)
                {
                    GUI.skin.box.stretchWidth = true;
                    GUILayout.Box("Time Running: " + s);
                    if (commStates[0] == 6)
                    {
                        GUILayout.Box("Card Rate: " + ((int)(unitCount[0] / totTime)) + " cards/sec");
                    }
                    else if (commStates[0] == 9)
                    {
                        GUILayout.Box("Image Rate: " + ((int)(unitCount[0] / totTime)) + " imgs/sec");
                    }
                    GUI.skin.box.stretchWidth = false;
                }
                #endregion

                #endregion

                #region Server Console

                #region Console Display
                consolePos = GUILayout.BeginScrollView(consolePos);

                for (int n = 0; n <= serverConsole.Count - 1; n++)
                {
                    GUILayout.Label(serverConsole[n]);
                }

                GUILayout.EndScrollView();
                #endregion

                #region Clear Console Button
                if (GUILayout.Button("Clear Console", GUILayout.Width(Screen.width - 10)))
                {
                    serverConsole = new List<string>();
                }
                #endregion

                #endregion

                #region Server Control Station
                GUILayout.Space(10);

                GUILayout.BeginHorizontal();

                #region Async Start
                if (GUILayout.Button("Async Start Server"))
                {
                    serverStart();
                }
                #endregion

                if (server != null)
                {
                    if (server.Pending())
                    {
                        if (GUILayout.Button("Accept Available Clients"))
                        {
                            acceptClients();
                        }
                    }
                }

                GUILayout.EndHorizontal();

                commandSelected = GUILayout.SelectionGrid(commandSelected, commands, commands.Length, GUILayout.Width(Screen.width - 10));

                #region Control Logic
                if (commandSelected != -1)
                {
                    #region Start Server
                    if (commandSelected == 0)
                    {
                        if (listener != null)
                        {
                            VrrktCoroutine.destruct(listener);
                        }
                        listener = startServer();

                        VrrktCoroutine.start(listener);

                        if (serverConsole == null)
                        {
                            serverConsole = new List<string>();
                        }
                    }
                    #endregion

                    #region Shut Down Server
                    if (commandSelected == 1)
                    {
                        if (listener != null)
                        {
                            VrrktCoroutine.destruct(listener);
                        }

                        if (serverConsole == null)
                        {
                            serverConsole = new List<string>();
                        }
                        serverConsole.Add("Stopped Server.");
                    }
                    #endregion

                    commandSelected = -1;
                }
                #endregion

                GUILayout.Space(40);
                #endregion
            }
        }
        #endregion

        #region Async Start Server
        async Task serverStart()
        {
            if (server == null)
            {
                IPEndPoint ipe = new IPEndPoint(IPAddress.Parse("192.168.0.46"), 11111);
                server = new TcpListener(ipe);
            }
            if (!serverRunning)
            {
                sClients = new List<Socket>();
                server.Start();
                server.Server.Listen(8);
                serverConsole.Add("Starting Server");
                serverRunning = true;
            }
        }
        #endregion

        #region Async Connect Clients
        async Task acceptClients()
        {
            if (sClients.Count < 8)
            {
                serverConsole.Add("Unidentified Clients available");

                server.BeginAcceptSocket(new AsyncCallback(connectClients), server);
            }
        }
        #endregion

        #region Connect Clients Callback
        void connectClients(IAsyncResult ar)
        {
            server = (TcpListener)ar.AsyncState;
            Socket client = server.EndAcceptSocket(ar);
            sClients.Add(client);
            serverConsole.Add("Successfully accepted socket connection!");

            acceptClients();
        }
        #endregion

        #region Start Server ENUM
        IEnumerator startServer()
        {
            connectors = new List<IEnumerator>();

            #region Start Server
            if (server == null)
            {
                IPEndPoint ipe = new IPEndPoint(IPAddress.Parse("192.168.0.46"), 11111);
                server = new TcpListener(ipe);
                server.Server.Blocking = false;
                server.Server.SendBufferSize = 100000;
                //server.Server.NoDelay = true;
            }
            sClients = new List<Socket>();
            server.Start();
            serverConsole.Add("Starting Server");
            #endregion

            #region Connect to Inbound Clients
            while (sClients.Count < 8)
            {
                if (server != null)
                {
                    if (server.Pending())
                    {
                        serverConsole.Add("Unidentified Clients available");
                        Socket client = null;
                        try
                        {
                            client = server.AcceptSocket();
                            sClients.Add(client);
                        }
                        catch(Exception e)
                        {
                            serverConsole.Add("Exception occurred in adding client");
                        }

                        if (sClients.Count > 0)
                        {
                            serverConsole.Add("Successfully accepted socket connection!");
                            connectors.Add(consolidatedServerOps(client));
                            //connectors.Add(serverOps(client));
                            serverConsole.Add("Started Server Operations for client with index [" + sClients.IndexOf(client) + "]");
                            VrrktCoroutine.start(consolidatedServerOps(client));
                        }

                        yield return true;
                    }
                    else
                    {
                        yield return true;
                        if (!serverConsole.Contains("Can accept more clients"))
                        {
                            serverConsole.Add("Can accept more clients");
                        }
                        yield return true;
                    }
                }
            }
            
            serverConsole.Add("All clients connected");
            yield return false;
            #endregion

            yield return false;
        }
        #endregion
        
        #region Consolidated Server ENUM
        IEnumerator consolidatedServerOps(Socket client)
        {
            #region Startup Operations

            #region Set Variables
            bool loading = false;
            int z = -1;
            int i = sClients.IndexOf(client);
            commStates[i] = 0;
            commP[i] = 0;
            #endregion

            #region Load vSys
            while(vSys == null)
            {
                if (!loading)
                {
                    load("C:/Users/robzo/Desktop/VrrktCentralDatabase/Resources/VrrktSystemData.vrrktDs", typeof(VrrktSystemData));
                    loading = true;
                }
                yield return true;
            }
            #endregion

            #endregion

            #region Request ID [0]
            while (commStates[i] == 0)
            {
                if (commP[i] == 0)
                {
                    serverConsole.Add("Requesting ID 0");
                    VrrktCoroutine.start(writeToSocket(Encoding.Default.GetBytes("RM"), i));
                    commP[i] = -1;
                }
                else
                {
                    if (commP[i] == 1)
                    {
                        commP[i] = 0;
                        commStates[i]++;
                    }
                }

                Repaint();
                yield return true;
            }
            #endregion

            #region Receive ID [1]
            while (commStates[i] == 1)
            {
                if (commP[i] == 0)
                {
                    serverConsole.Add("Receiving ID 1");
                    VrrktCoroutine.start(readFromSocket(i));
                    commP[i] = -1;
                }
                else
                {
                    if (commP[i] == 1)
                    {
                        commP[i] = 0;
                        commStates[i]++;
                    }
                }

                Repaint();
                yield return true;
            }
            #endregion

            #region Accept/Deny(Get User Index) [2]
            while (commStates[i] == 2)
            {
                if (commP[i] == 0)
                {
                    if (clientID[i] != -1)
                    {
                        z = clientID[i];
                    }
                    serverConsole.Add("z: " + z);

                    #region Accept
                    if (z >= 0)
                    {
                        serverConsole.Add("Accept ID 2");
                        VrrktCoroutine.start(writeToSocket(Encoding.Default.GetBytes("K"), i));
                    }
                    #endregion

                    #region Deny
                    else
                    {
                        serverConsole.Add("Deny ID 2");
                        VrrktCoroutine.start(writeToSocket(Encoding.Default.GetBytes("O"), i));
                    }
                    #endregion

                    commP[i] = -1;
                }
                else
                {
                    if (commP[i] == 1)
                    {
                        commP[i] = 0;
                        commStates[i]++;
                    }
                }

                Repaint();
                yield return true;
            }
            #endregion

            #region Receive Install Request [3]
            while (commStates[i] == 3)
            {
                if (commP[i] == 0)
                {
                    serverConsole.Add("Receive Install Request 3");
                    VrrktCoroutine.start(readFromSocket(i));
                    commP[i] = -1;
                }
                else
                {
                    if (commP[i] == 1)
                    {
                        commP[i] = 0;
                        commStates[i]++;
                    }
                }

                Repaint();
                yield return true;
            }
            #endregion

            #region Send GID Table [4]
            while (commStates[i] == 4)
            {
                if (commP[i] == 0)
                {
                    serverConsole.Add("Send GID Table 4");
                    VrrktCoroutine.start(fileToSocket(i, z));
                    commP[i] = -1;
                }
                else
                {
                    if (commP[i] == 1)
                    {
                        commP[i] = 0;
                        commStates[i]++;
                    }
                }

                Repaint();
                yield return true;
            }
            #endregion

            #region Receive Acknowledged [5]
            while (commStates[i] == 5)
            {
                if (commP[i] == 0)
                {
                    serverConsole.Add("Receive Acknowledged 5");
                    VrrktCoroutine.start(readFromSocket(i));
                    commP[i] = -1;
                }
                else
                {
                    if (commP[i] == 1)
                    {
                        if (Encoding.Default.GetString(datIn[i]).Contains("K"))
                        {
                            unitCount[i] = 0;
                            commP[i] = 0;
                            commStates[i]++;
                        }
                        else
                        {
                            serverConsole.Add("Error in message");
                        }
                    }
                }

                Repaint();
                yield return true;
            }
            #endregion

            #region Send Cards [6]
            while (commStates[i] == 6)
            {
                if (commP[i] == 0)
                {
                    serverConsole.Add("Send Cards 6");
                    VrrktCoroutine.start(fileToSocket(i, z));
                    commP[i] = -1;
                    sT = Time.time;
                }
                else
                {
                    if (commP[i] == 1)
                    {
                        commP[i] = 0;
                        commStates[i]++;
                    }
                }

                Repaint();
                yield return true;
            }
            #endregion

            #region Send Cards Complete [7]
            while (commStates[i] == 7)
            {
                if (commP[i] == 0)
                {
                    serverConsole.Add("Cards Completed 7");
                    VrrktCoroutine.start(writeToSocket(Encoding.Default.GetBytes("D"), i));
                    commP[i] = -1;
                }
                else
                {
                    if (commP[i] == 1)
                    {
                        unitCount[i] = 0;
                        commP[i] = 0;
                        commStates[i]++;
                    }
                }

                Repaint();
                yield return true;
            }
            #endregion

            #region Receive Acknowledged [8]
            while (commStates[i] == 8)
            {
                if (commP[i] == 0)
                {
                    serverConsole.Add("Receive Acknowledged 8");
                    VrrktCoroutine.start(readFromSocket(i));
                    commP[i] = -1;
                }
                else
                {
                    if (commP[i] == 1)
                    {
                        if (Encoding.Default.GetString(datIn[i]).Contains("K"))
                        {
                            commP[i] = 0;
                            commStates[i]++;
                        }
                        else
                        {
                            serverConsole.Add("Error in message");
                        }
                    }
                }

                Repaint();
                yield return true;
            }
            #endregion

            #region Send Images [9]
            while (commStates[i] == 9)
            {
                if (commP[i] == 0)
                {
                    VrrktCoroutine.start(fileToSocket(i, z));
                    commP[i] = -1;
                }
                else
                {
                    if (commP[i] == 1)
                    {
                        commP[i] = 0;
                        commStates[i]++;
                    }
                }

                Repaint();
                yield return true;
            }
            #endregion

            #region Send Images Done [10]
            while (commStates[i] == 10)
            {
                if (commP[i] == 0)
                {
                    VrrktCoroutine.start(writeToSocket(Encoding.Default.GetBytes("D"), i));
                    commP[i] = -1;
                }
                else
                {
                    if (commP[i] == 1)
                    {
                        commP[i] = 0;
                        commStates[i]++;
                    }
                }

                Repaint();
                yield return true;
            }
            #endregion

            #region Receive Acknowledged [11]
            while (commStates[i] == 11)
            {
                if (commP[i] == 0)
                {
                    VrrktCoroutine.start(readFromSocket(i));
                    commP[i] = -1;
                }
                else
                {
                    if (commP[i] == 1)
                    {
                        if (Encoding.Default.GetString(datIn[i]).Contains("K"))
                        {
                            commP[i] = 0;
                            commStates[i]++;
                        }
                        else
                        {
                            serverConsole.Add("Error in message");
                        }
                    }
                }

                Repaint();
                yield return true;
            }
            #endregion

            #region Send Closing Notice [12]
            while (commStates[i] == 12)
            {
                if (commP[i] == 0)
                {
                    VrrktCoroutine.start(writeToSocket(Encoding.Default.GetBytes("X"), i));
                    commP[i] = -1;
                }
                else
                {
                    if (commP[i] == 1)
                    {
                        commP[i] = 0;
                        commStates[i]++;
                    }
                }

                Repaint();
                yield return true;
            }
            #endregion

            #region End
            serverConsole.Add("Server Operations Ended");
            Repaint();
            yield return false;
            #endregion
        }
        #endregion

        #region Consolidated Read/Write Methods

        #region Write Message To Socket
        IEnumerator writeToSocket(byte[] b, int i)
        {
            byte[] o = null;
            ns[i] = 0;

            #region Write To Stream
            while(ns[i] < b.Length)
            {
                while (ns[i] < b.Length)
                {
                    for (int u = 0; u <= 999; u++)
                    {
                        if (ns[i] < b.Length)
                        {
                            sClients[i].Send(new byte[1] { b[ns[i]] });
                            ns[i]++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    yield return true;
                }
            }
            #endregion

            #region Add Ending
            o = Encoding.Default.GetBytes("!");

            for(int n = 0; n <= o.Length - 1; n++)
            {
                sClients[i].Send(new byte[1] { o[n] });
            }
            #endregion

            commP[i] = 1;
            yield return false;
        }
        #endregion

        #region Write File To Socket
        IEnumerator fileToSocket(int i, int z)
        {
            #region Startup Operations
            paths[i] = "";
            ns[i] = 0;
            FileStream f = null;
            #endregion

            #region GID Table [4]
            if (commStates[i] == 4)
            {
                #region Handle Null List
                if (tables == null)
                {
                    tables = new List<vrrktGIDTable>();
                    for (int n = 0; n <= 7; n++)
                    {
                        tables.Add(null);
                    }
                }
                #endregion

                #region Load Client Specific Table
                paths[i] = "C:/Users/robzo/Desktop/VrrktCentralDatabase/Resources/" + z + ".vrrktGt";
                load(paths[i], typeof(vrrktGIDTable));
                yield return true;

                while (tables[z] == null)
                {
                    yield return true;
                }
                #endregion

                #region Call Socket Write Method
                VrrktComms.ConstructMessage(new Vector3Int(1, -1, 0) , sClients[i], tables[z], this, i);
                #endregion

                #region Wait For Callback
                while (commP[i] == -1)
                {
                    yield return true;
                }
                #endregion

                commP[i] = 1;
            }
            #endregion

            #region Card [6]
            if (commStates[i] == 6)
            {
                while (commP[i] != 1)
                {
                    #region Timekeeping
                    if (unitCount[i] > 0)
                    {
                        #region Delay Calculation
                        if (unitCount[i] > 0 && unitCount[i] % 1000 > 0)
                        {
                            delay = .01f;
                        }
                        else if (unitCount[i] > 0 && unitCount[i] % 1000 == 0)
                        {
                            delay = .25f;
                        }
                        #endregion

                        yT = Time.time;
                        totTime = yT - sT;
                    }
                    #endregion

                    #region Iteration
                    if (unitCount[i] == 0 || yT - xT >= delay)
                    {
                        #region Create Stream
                        f = File.Open("C:/Users/robzo/Desktop/VrrktCentralDatabase/Database/" + tables[z].table[unitCount[i]] + ".vrrktCd", FileMode.Open);
                        //serverConsole.Add("Created stream. C: " + f.Length);
                        #endregion

                        #region Write Stream To Socket
                        ns[i] = 0;
                        while (ns[i] < f.Length)
                        {
                            for (int n = 0; n <= 99; n++)
                            {
                                if (ns[i] < f.Length)
                                {
                                    byte[] b = new byte[1];
                                    b[0] = (byte)f.ReadByte();
                                    sClients[i].Send(b);
                                    ns[i]++;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            yield return true;
                        }
                        sClients[i].Send(Encoding.Default.GetBytes("~!"));
                        f.Close();
                        //serverConsole.Add("Wrote stream to socket");
                        #endregion

                        #region Iteration Control
                        if (unitCount[i] < tables[z].table.Count)
                        {
                            unitCount[i]++;
                        }
                        else
                        {
                            commP[i] = 1;
                            serverConsole.Add("Ended Card Transferrence");
                        }
                        xT = Time.time;
                        #endregion
                    }
                    #endregion

                    yield return true;
                }
            }
            #endregion

            #region Image [9]
            if (commStates[i] == 9)
            {
                while (unitCount[i] < tables[z].table.Count - 1)
                {
                    #region Timekeeping
                    if (unitCount[i] > 0)
                    {
                        #region Delay Calculation
                        if (unitCount[i] > 0 && unitCount[i] % 1000 > 0)
                        {
                            delay = .025f;
                        }
                        else if (unitCount[i] > 0 && unitCount[i] % 1000 == 0)
                        {
                            delay = .5f;
                        }
                        #endregion

                        yT = Time.time;
                    }
                    #endregion

                    if (unitCount[i] == 0 || yT - xT >= delay)
                    {
                        #region Create Stream
                        f = File.Open("C:/Users/robzo/Desktop/VrrktCentralDatabase/Database/" + tables[z].table[unitCount[i]] + ".vrrktCi", FileMode.Open);
                        //serverConsole.Add("Created stream. C: " + f.Length);
                        #endregion

                        #region Write Stream To Socket
                        ns[i] = 0;
                        while (ns[i] < f.Length)
                        {
                            for (int n = 0; n <= 99; n++)
                            {
                                if (ns[i] < f.Length)
                                {
                                    byte[] b = new byte[1];
                                    b[0] = (byte)f.ReadByte();
                                    sClients[i].Send(b);
                                    ns[i]++;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            yield return true;
                        }
                        sClients[i].Send(Encoding.Default.GetBytes("~!"));
                        f.Close();
                        //serverConsole.Add("Wrote stream to socket");
                        #endregion

                        #region Iteration Control
                        if (unitCount[i] < tables[z].table.Count - 1)
                        {
                            unitCount[i]++;
                        }
                        else
                        {
                            commP[i] = 1;
                            serverConsole.Add("Ended Card Transferrence");
                        }
                        xT = Time.time;
                        #endregion
                    }

                    yield return true;
                }
            }
            #endregion

            yield return false;
        }
        #endregion
        
        #region Read From Socket
        IEnumerator readFromSocket(int i)
        {
            #region Startup Operations
            int x, y;
            datIn[i] = null;
            string p = "";
            List<byte> dataIn = new List<byte>();
            bool eOS = false;
            byte[] bO;
            bool rS = false;
            #endregion

            #region Receive Data
            while (!eOS)
            {
                if (sClients[i].Available > 0)
                {
                    rS = true;
                    bO = new byte[1];

                    try
                    {
                        sClients[i].Receive(bO);
                    }
                    catch (Exception e)
                    {
                        rS = false;
                    }

                    if (rS)
                    {
                        if (Encoding.Default.GetString(bO) == "!")
                        {
                            eOS = true;
                        }
                        else
                        {
                            dataIn.Add(bO[0]);
                        }
                    }
                }
                yield return true;
            }
            #endregion

            #region Parse Messages
            if (commStates[i] == 1 || commStates[i] == 3)
            {
                #region Parse ID [1]
                if (commStates[i] == 1)
                {
                    bO = new byte[2];
                    bO[0] = dataIn[0];
                    bO[1] = dataIn[1];

                    if (Encoding.Default.GetString(bO) == "M~")
                    {
                        bO = new byte[1];
                        bO[0] = dataIn[dataIn.Count - 1];
                        yield return true;

                        if (Encoding.Default.GetString(bO) == "~")
                        {
                            string w = "";
                            dataIn.RemoveRange(0, 2);
                            dataIn.RemoveRange(dataIn.Count - 1, 1);

                            bO = dataIn.ToArray();
                            w = Encoding.Default.GetString(bO);
                            yield return true;

                            if (w.Length == 2)
                            {
                                serverConsole.Add("Trimmed: " + w);
                                for (int s = 0; s <= vSys.userIDs.Count - 1; s++)
                                {
                                    if (vSys.userIDs[s].Contains(w))
                                    {
                                        connections[s] = true;
                                        serverConsole.Add("Client identified as " + vSys.userIDs[s]);
                                        clientID[i] = s;
                                        serverConsole.Add("Set ClientID[" + i + "] = " + s);
                                        commP[i] = 1;
                                        break;
                                    }
                                }
                                yield return true;
                            }
                            else
                            {
                                serverConsole.Add("Error in trimmed length. M: " + w);
                            }
                        }
                        else
                        {
                            serverConsole.Add("Error in message p2. M: " + Encoding.Default.GetString(bO));
                        }
                    }
                    else
                    {
                        serverConsole.Add("Error in message p1. M: " + Encoding.Default.GetString(bO));
                    }
                }
                #endregion

                #region Parse Install Request [3]
                if (commStates[i] == 3)
                {
                    string w = Encoding.Default.GetString(dataIn.ToArray());

                    if (w.Substring(0, 2) == "S~")
                    {
                        if (w.Substring(w.Length - 1, 1) == "~")
                        {
                            x = -1;
                            y = -1;
                            while (phaseNstep[i] == Vector2.down)
                            {
                                while (x < 0)
                                {
                                    w = w.Substring(w.IndexOf("~") + 1);
                                    if (!int.TryParse(w.Substring(0, w.IndexOf("~")), out x))
                                    {

                                        serverConsole.Add("Failed Step parse");
                                    }
                                    yield return true;
                                }

                                while (y < 0)
                                {
                                    w = w.Substring(w.IndexOf("~") + 1);

                                    if (!int.TryParse(w.Substring(0, w.IndexOf("~")), out y))
                                    {
                                        serverConsole.Add("Failed Step parse");
                                    }

                                    yield return true;
                                }

                                phaseNstep[i] = new Vector2(x, y);
                                commP[i] = 1;
                                yield return true;
                            }
                        }
                        else
                        {
                            serverConsole.Add("error in p2. M: " + w);
                        }
                    }
                    else
                    {
                        serverConsole.Add("error in p1. M: " + w);
                    }
                }
                #endregion
            }
            #endregion

            #region Unparsed Messages
            else
            {
                datIn[i] = dataIn.ToArray();
                commP[i] = 1;
            }
            #endregion

            yield return false;
        }
        #endregion

        #endregion
        
        #region File Write Callback
        public void fileWriteCallback(int i)
        {
            if (commStates[i] != 6 && commStates[i] != 9)
            {
                commP[i] = 1;
            }
            //serverConsole.Add("Finished writing file to socket for i[" + i + "], t[" + commStates[i] + "]");
        }
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
                    VrrktCard c = (VrrktCard)o;
                    if (cards == null)
                    {
                        cards = new List<VrrktCard>();
                    }

                    for(int n = 0; n <= tables.Count - 1; n++)
                    {
                        if (tables[n] != null)
                        {
                            if (tables[n].table.Contains(c.GID))
                            {
                                cards[n] = c;
                                break;
                            }
                        }
                    }
                }
                #endregion

                #region System 2
                if (i == 2)
                {
                    vSys = (VrrktSystemData)o;
                }
                #endregion

                #region Operations 3
                if (i == 3)
                {
                    vOps = (VrrktOperationsData)o;
                }
                #endregion

                #region User 4
                if (i == 4)
                {
                    vUser = (VrrktUser)o;
                }
                #endregion

                #region GID Table 6
                if (i == 6)
                {
                    vrrktGIDTable gT = (vrrktGIDTable)o;
                    if (tables == null)
                    {
                        tables = new List<vrrktGIDTable>();
                        
                    }

                    for(int n = 0; n <= vSys.userIDs.Count - 1; n++)
                    {
                        if (vSys.userIDs[n] == gT.userID)
                        {
                            tables[n] = gT;
                            //serverConsole.Add("Added GIDTable for user: " + vSys.userIDs[n]);
                            break;
                        }
                    }
                }
                #endregion
            }
            #endregion
        }
        #endregion

        #region Save Passback
        public void vrrktPassback(int t, int tL, int sT, int sTL, bool isDone, object re, Type ty)
        {
            for(int n = 0; n <= commP.Count - 1; n++)
            {
                if (commP[n] == -3)
                {
                    commP[n] = -1;
                    break;
                }
            }
        }
        #endregion

        #endregion

        #region Legacy Methods

        #region Write Message To Stream
        IEnumerator writeToStream(byte[] b, int i)
        {
            //serverConsole.Add("Begin writing to stream");
            ns[i] = 0;

            #region Write Array To Stream
            while (ns[i] < b.Length)
            {
                for (int u = 0; u <= 999; u++)
                {
                    if (ns[i] < b.Length)
                    {
                        sClients[i].Send(new byte[1] { b[ns[i]] });
                        ns[i]++;
                    }
                    else
                    {
                        break;
                    }
                }
                yield return true;
            }
            #endregion

            if (commStates[i] != 2 && commStates[i] != 3 && commStates[i] != 5)
            {
                //serverConsole.Add("Wrote message to stream. Length = " + n + ". Decoded message is " + Encoding.Default.GetString(b));
            }
            commP[i] = 1;
            yield return false;
        }
        #endregion

        #region Write File To Stream
        IEnumerator writeToStream(Vector3Int v, int i)
        {
            #region Startup Operations
            int z = 0;
            paths[i] = "";
            ns[i] = 0;
            FileStream f = null;
            #endregion

            #region GID Table
            if (commStates[i] == 2)
            {
                #region Handle Null List
                if (tables == null)
                {
                    tables = new List<vrrktGIDTable>();
                    for (int n = 0; n <= 7; n++)
                    {
                        tables.Add(null);
                    }
                }
                #endregion

                #region Identify Client
                for (int u = 0; u <= clientID.Count - 1; u++)
                {
                    if (clientID[u] == i)
                    {
                        z = u;
                        break;
                    }
                }
                #endregion

                #region Load Client Specific Table
                paths[i] = "C:/Users/robzo/Desktop/VrrktCentralDatabase/Resources/" + z + ".vrrktGt";
                load(paths[i], typeof(vrrktGIDTable));
                yield return true;

                while (tables[z] == null)
                {
                    yield return true;
                }
                #endregion

                #region Call Socket Write Method
                VrrktComms.ConstructMessage(v, sClients[i], tables[z], this, i);
                #endregion

                #region Wait For Callback
                while (commP[i] == -1)
                {
                    yield return true;
                }
                #endregion

                if (commStates[i] == 100)
                {
                    #region Create Client Specific vSys
                    VrrktSystemData vS = new VrrktSystemData();
                    vS.userIDs = vSys.userIDs;
                    vS.GIDtable = tables[z].table;
                    vS.background = vSys.background;
                    vS.x = vSys.x;
                    vS.y = vSys.y;
                    yield return true;
                    #endregion

                    #region Save Client Specific vSys
                    paths[i] = "C:/Users/robzo/Desktop/VrrktCentralDatabase/Resources/" + vSys.userIDs[z] + ".vrrktDs";
                    save(paths[i], vS);
                    commP[i] = -3;

                    while (commP[i] == -3)
                    {
                        yield return true;
                    }
                    #endregion

                    #region Create Stream
                    while (f == null)
                    {
                        try
                        {
                            f = File.Open("C:/Users/robzo/Desktop/VrrktCentralDatabase/Resources/" + vSys.userIDs[z] + ".vrrktDs", FileMode.Open);
                        }
                        catch (Exception e)
                        {

                        }
                        yield return true;
                    }
                    #endregion

                    #region Write Stream To Socket
                    ns[i] = 0;
                    while (ns[i] < f.Length)
                    {
                        for (int n = 0; n <= 99; n++)
                        {
                            if (ns[i] < f.Length)
                            {
                                byte[] b = new byte[1];
                                b[0] = (byte)f.ReadByte();
                                sClients[i].Send(b);
                                ns[i]++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        yield return true;
                    }
                    sClients[i].Send(Encoding.Default.GetBytes("~!"));
                    f.Close();
                    yield return true;
                    //serverConsole.Add("Wrote stream to socket");
                    #endregion
                }
            }
            #endregion

            #region Card
            if (commStates[i] == 3)
            {
                #region Identify Client
                for (int u = 0; u <= clientID.Count - 1; u++)
                {
                    if (clientID[u] == i)
                    {
                        z = u;
                        break;
                    }
                }
                //serverConsole.Add("Identify client");
                #endregion

                #region Create Stream
                f = File.Open("C:/Users/robzo/Desktop/VrrktCentralDatabase/Database/" + tables[z].table[unitCount[i]] + ".vrrktCd", FileMode.Open);
                //serverConsole.Add("Created stream. C: " + f.Length);
                #endregion

                #region Write Stream To Socket
                ns[i] = 0;
                while (ns[i] < f.Length)
                {
                    for (int n = 0; n <= 99; n++)
                    {
                        if (ns[i] < f.Length)
                        {
                            byte[] b = new byte[1];
                            b[0] = (byte)f.ReadByte();
                            sClients[i].Send(b);
                            ns[i]++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    yield return true;
                }
                sClients[i].Send(Encoding.Default.GetBytes("~!"));
                f.Close();
                //serverConsole.Add("Wrote stream to socket");
                #endregion

                #region Iteration Control
                if (unitCount[i] < tables[z].table.Count - 1)
                {
                    unitCount[i]++;
                    commP[i] = 0;
                }
                else
                {
                    commP[i] = 1;
                    serverConsole.Add("Ended Card Transferrence");
                }
                xT = Time.time;
                #endregion
            }
            #endregion

            #region Image
            if (commStates[i] == 5)
            {
                #region Identify Client
                for (int u = 0; u <= clientID.Count - 1; u++)
                {
                    if (clientID[u] == i)
                    {
                        z = u;
                        break;
                    }
                }
                //serverConsole.Add("Identify client");
                #endregion

                #region Create Stream
                f = File.Open("C:/Users/robzo/Desktop/VrrktCentralDatabase/Database/" + tables[z].table[unitCount[i]] + ".vrrktCi", FileMode.Open);
                //serverConsole.Add("Created stream. C: " + f.Length);
                #endregion

                #region Write Stream To Socket
                ns[i] = 0;
                while (ns[i] < f.Length)
                {
                    for (int n = 0; n <= 99; n++)
                    {
                        if (ns[i] < f.Length)
                        {
                            byte[] b = new byte[1];
                            b[0] = (byte)f.ReadByte();
                            sClients[i].Send(b);
                            ns[i]++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    yield return true;
                }
                sClients[i].Send(Encoding.Default.GetBytes("~!"));
                f.Close();
                //serverConsole.Add("Wrote stream to socket");
                #endregion

                #region Iteration Control

                if (commStates[i] == 100)
                {
                    if (unitCount[i] < tables[z].table.Count)
                    {
                        unitCount[i]++;
                        commP[i] = 0;
                    }
                    else
                    {
                        commP[i] = 1;
                    }
                }
                xT = Time.time;
                #endregion
            }
            #endregion

            yield return false;
        }
        #endregion

        #region Intermediate Stream ENUM
        IEnumerator intermediateStream(int i)
        {
            while (commStates[i] == 3 && commP[i] != 1)
            {
                for (int n = 0; n <= 999; n++)
                {
                    if (commStates[i] == 3 && commP[i] != 1)
                    {
                        if (streams[i].Length > 0 && streams[i].CanRead)
                        {
                            byte[] b = new byte[1];
                            b[0] = (byte)streams[i].ReadByte();
                            sClients[i].Send(b);
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                yield return true;
            }

            yield return false;
        }
        #endregion

        #region Await Response
        IEnumerator awaitResponse(int i)
        {
            #region Startup Operations
            sIns[i] = "";
            string p = "";
            List<byte> dataIn = new List<byte>();
            bool eOS = false;
            #endregion

            #region Handle Data
            while (!eOS)
            {
                if (sClients[i].Available > 0)
                {
                    byte[] bO;
                    sClients[i].Receive(bO = new byte[1]);

                    for (int u = 0; u <= bO.Length - 1; u++)
                    {
                        dataIn.Add(bO[u]);
                    }

                    #region Check For End
                    bO = new byte[1];
                    bO[0] = dataIn[dataIn.Count - 1];
                    if (Encoding.Default.GetString(bO) == "!")
                    {
                        //serverConsole.Add("Found end of message");
                        dataIn.Remove(dataIn[dataIn.Count - 1]);
                        eOS = true;
                    }
                    #endregion
                }
                yield return true;
            }
            #endregion

            #region Message Verbose Statement
            //serverConsole.Add("Message length is " + dataIn.Count);
            //serverConsole.Add("Message decoded is " + Encoding.Default.GetString(dataIn.ToArray()));
            yield return true;
            #endregion

            #region Examine Message

            sIns[i] = Encoding.Default.GetString(dataIn.ToArray());

            #region ID Requested [0]
            if (commStates[i] == 0)
            {
                if (sIns[i][0] != 'M' || sIns[i][1] != '~')
                {
                    serverConsole.Add("Message was not as expected. Message is: " + sIns[i]);
                    yield return false;
                }
                //serverConsole.Add("First 2 Characters are correct");
                yield return true;


                if (sIns[i].LastIndexOf('~') == sIns[i].Length - 1)
                {
                    p = sIns[i].Substring(2, sIns[i].Length - 3);
                    //serverConsole.Add("Last character is correct");
                    //serverConsole.Add("Parsed length is " + p.Length + ", parsed is [" + p + "]");
                    yield return true;

                    if (p.Length == 2)
                    {
                        for (int x = 0; x <= vSys.userIDs.Count - 1; x++)
                        {
                            if (vSys.userIDs[x].Contains(p))
                            {
                                connections[x] = true;
                                serverConsole.Add("Client identified as " + vSys.userIDs[x]);
                                clientID[x] = i;
                                break;
                            }
                        }
                    }
                    else
                    {
                        serverConsole.Add("Message was not as expected by length. Message is: " + sIns[i]);
                        yield return false;
                    }
                }
                else
                {
                    serverConsole.Add("Message was not as expected. Message is: " + sIns[i]);
                    yield return false;
                }
            }
            #endregion

            #region Installation Phase & Step [1]
            if (commStates[i] == 1)
            {
                int x = -1;
                int y = -1;
                string pa = "";

                if (sIns[i][0] != 'S' || sIns[i][1] != '~')
                {
                    serverConsole.Add("Message was not as expected. Message is: " + sIns[i]);
                    yield return false;
                }
                //serverConsole.Add("First 2 Characters are correct");
                yield return true;

                pa = sIns[i].Substring(2);
                pa = pa.Substring(0, pa.IndexOf('~'));
                if (!int.TryParse(pa, out x))
                {
                    serverConsole.Add("Phase parsing failed. Remainder is " + pa);
                    yield return false;
                }
                //serverConsole.Add("Phase parsing was successful. Phase is " + x + ", after first parse remainder is " + pa);
                yield return true;

                if (pa.Contains("~"))
                {
                    pa = pa.Substring(pa.IndexOf('~') + 1);
                    pa = pa.Substring(0, pa.IndexOf('~'));
                }

                if (!int.TryParse(pa, out y))
                {
                    serverConsole.Add("Step parsing failed. Remainder is " + pa);
                    yield return false;
                }

                Vector2 v = new Vector2(x, y);
                phaseNstep[i] = v;
                //serverConsole.Add("Parsed Phase & Step Successfully! Phase:" + x + ", Step:" + y);
                yield return true;
            }
            #endregion

            #region Received GID Table [2]
            if (commStates[i] == 2)
            {
                if (sIns[i][0] != 'K')
                {
                    serverConsole.Add("Message was not as expected. Message is: " + sIns[i]);
                    yield return false;
                }
            }
            #endregion

            #region Received Card [3]
            if (commStates[i] == 3)
            {
                if (sIns[i][0] != 'K')
                {
                    serverConsole.Add("Message was not as expected. Message is: " + sIns[i]);
                    yield return false;
                }
            }
            #endregion

            #region Finished Receiving Cards [4]
            if (commStates[i] == 4)
            {
                if (sIns[i][0] != 'K')
                {
                    serverConsole.Add("Message was not as expected. Message is: " + sIns[i]);
                    yield return false;
                }
            }
            #endregion

            #region Received Image [5]
            if (commStates[i] == 5)
            {
                if (sIns[i][0] != 'K')
                {
                    serverConsole.Add("Message was not as expected. Message is: " + sIns[i]);
                    yield return false;
                }
            }

            #endregion

            #region Finished Receiving Images [6]
            if (commStates[i] == 6)
            {
                if (sIns[i][0] != 'K')
                {
                    serverConsole.Add("Message was not as expected. Message is: " + sIns[i]);
                    yield return false;
                }
            }

            #endregion

            #endregion

            #region End
            serverConsole.Add("Ended reading message");
            commP[i] = 2;
            yield return false;
            #endregion
        }
        #endregion

        #region Server Operations ENUM
        IEnumerator serverOps(Socket client)
        {
            #region Startup Operations
            int idx = sClients.IndexOf(client);
            commStates[idx] = 0;
            //serverConsole.Add("Server Operations Startup with commstate[" + commStates[idx] + "]");
            #endregion

            #region Request Identification [0]
            while (commStates[idx] == 0)
            {
                if (commP[idx] == 0)
                {
                    //serverConsole.Add("Writing for commstates 0");
                    byte[] bO = VrrktComms.ConstructMessage(new Vector3Int(0, 3, -1));
                    VrrktCoroutine.start(writeToStream(bO, idx));
                    commP[idx] = -1;
                }
                else
                {
                    if (commP[idx] == 1)
                    {
                        //serverConsole.Add("Reading for commstates 0");
                        VrrktCoroutine.start(awaitResponse(idx));
                        commP[idx] = -1;
                    }
                    else
                    {
                        if (commP[idx] == 2)
                        {
                            serverConsole.Add("Ended commstates 0");
                            commStates[idx]++;
                            commP[idx] = 0;
                        }
                    }
                }
                Repaint();
                yield return true;
            }
            #endregion

            #region Receive Installation Request [1]
            while (commStates[idx] == 1)
            {
                if (commP[idx] == 0)
                {
                    //serverConsole.Add("Writing for commstates 1");
                    byte[] bO = VrrktComms.ConstructMessage(new Vector3Int(1, -1, -1));
                    VrrktCoroutine.start(writeToStream(bO, idx));
                    commP[idx] = -1;
                }
                else
                {
                    if (commP[idx] == 1)
                    {
                        //serverConsole.Add("Reading for commstates 1");
                        VrrktCoroutine.start(awaitResponse(idx));
                        commP[idx] = -1;
                    }
                    else
                    {
                        if (commP[idx] == 2)
                        {
                            serverConsole.Add("Ended commstates 1");
                            commStates[idx]++;
                            commP[idx] = 0;
                        }
                    }
                }
                Repaint();
                yield return true;
            }
            #endregion

            #region Send GID Table [2]
            while (commStates[idx] == 2)
            {
                if (commP[idx] == 0)
                {
                    //serverConsole.Add("Writing for commstates 2");
                    VrrktCoroutine.start(writeToStream(new Vector3Int(1, -1, 0), idx));
                    commP[idx] = -1;
                }
                else
                {
                    if (commP[idx] == 1)
                    {
                        //serverConsole.Add("Reading for commstates 2");
                        VrrktCoroutine.start(awaitResponse(idx));
                        commP[idx] = -1;
                    }
                    else
                    {
                        if (commP[idx] == 2)
                        {
                            serverConsole.Add("Ended commstates 2");
                            commStates[idx]++;
                            unitCount[idx] = tables[5].table.Count - 100;
                            commP[idx] = 0;
                            delay = .025f;
                            sT = 0;
                            totTime = 0;
                        }
                    }
                }
                Repaint();
                yield return true;
            }
            #endregion

            #region Send Cards [3]
            while (commStates[idx] == 3)
            {
                if (commP[idx] == 0)
                {
                    if (unitCount[idx] > 0 && unitCount[idx] % 1000 > 0)
                    {
                        delay = .01f;
                        //delay = 0;
                    }
                    else if (unitCount[idx] > 0 && unitCount[idx] % 1000 == 0)
                    {
                        delay = .25f;
                    }

                    yT = Time.time;

                    if (unitCount[idx] == 0)
                    {
                        sT = Time.time;
                    }
                    totTime = yT - sT;

                    if (unitCount[idx] == 0 || yT - xT >= delay)
                    {
                        commP[idx] = -1;
                        VrrktCoroutine.start(writeToStream(new Vector3Int(-1, -1, 1), idx));
                    }
                }
                else if (commP[idx] == 1)
                {
                    commStates[idx]++;
                    unitCount[idx] = 0;
                    commP[idx] = 0;
                    serverConsole.Add("Ended commstates 3");
                }
                Repaint();
                yield return true;
            }
            #endregion

            #region Complete Cards Sent [4]
            while (commStates[idx] == 4)
            {
                if (commP[idx] == 0)
                {
                    serverConsole.Add("Writing for commstates 4");
                    byte[] bO = VrrktComms.ConstructMessage(new Vector3Int(4, -1, -1));
                    VrrktCoroutine.start(writeToStream(bO, idx));
                    commP[idx] = -1;
                }
                else if (commP[idx] == 1)
                {
                    VrrktCoroutine.start(awaitResponse(idx));
                    serverConsole.Add("Reading for commstates 4");
                    commP[idx] = -1;
                }
                else if (commP[idx] == 2)
                {
                    commStates[idx]++;
                    unitCount[idx] = 0;
                    commP[idx] = 0;
                    serverConsole.Add("Ended commstates 4");
                }
                yield return true;
            }
            #endregion

            #region Send Images [5]
            while (commStates[idx] == 5)
            {
                if (commP[idx] == 0)
                {
                    if (unitCount[idx] > 0 && unitCount[idx] % 1000 > 0)
                    {
                        delay = .025f;
                    }
                    else if (unitCount[idx] > 0 && unitCount[idx] % 1000 == 0)
                    {
                        delay = .5f;
                    }

                    yT = Time.time;
                    if (unitCount[idx] == 0 || yT - xT >= delay)
                    {
                        commP[idx] = -1;
                        VrrktCoroutine.start(writeToStream(new Vector3Int(-1, -1, 2), idx));
                    }
                }
                else if (commP[idx] == 1)
                {
                    commStates[idx]++;
                    unitCount[idx] = 0;
                    commP[idx] = 0;
                    serverConsole.Add("Ended commstates 5");
                }
                Repaint();
                yield return true;
            }
            #endregion

            #region Complete Images Sent [6]
            while (commStates[idx] == 6)
            {
                if (commP[idx] == 0)
                {
                    byte[] bO = VrrktComms.ConstructMessage(new Vector3Int(4, -1, -1));
                    VrrktCoroutine.start(writeToStream(bO, idx));
                    serverConsole.Add("Writing for commstates 6");
                    commP[idx] = -1;
                }
                else if (commP[idx] == 1)
                {
                    VrrktCoroutine.start(awaitResponse(idx));
                    serverConsole.Add("Reading for commstates 6");
                    commP[idx] = -1;
                }
                else if (commP[idx] == 2)
                {
                    commStates[idx]++;
                    serverConsole.Add("Ended commstates 6");
                }
                yield return true;
            }
            #endregion

            #region Finish Communications [7]
            while (commStates[idx] == 7)
            {
                if (commP[idx] == 0)
                {
                    byte[] bO = VrrktComms.ConstructMessage(new Vector3Int(3, -1, -1));
                    VrrktCoroutine.start(writeToStream(bO, idx));
                    serverConsole.Add("Writing for commstates 7");
                    commP[idx] = -1;
                    yield return true;
                }
                else if (commP[idx] == 1)
                {
                    VrrktCoroutine.start(awaitResponse(idx));
                    serverConsole.Add("Reading for commstates 7");
                    commP[idx] = -1;
                    yield return true;
                }
                else if (commP[idx] == 2)
                {
                    commStates[idx]++;
                    serverConsole.Add("Ended commstates 7");
                    yield return true;
                }
            }
            #endregion

            #region End
            serverConsole.Add("Server Operations Ended");
            Repaint();
            yield return false;
            #endregion
        }
        #endregion

        #endregion
    }
}
#endregion
