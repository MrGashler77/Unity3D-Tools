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
using Verruckt.Operations;
#endregion

#region Main Class
namespace vrrkt.Editor
{
    [ExecuteInEditMode]
    [CustomEditor(typeof(ScriptableObject))]
    [CanEditMultipleObjects]
    public class vrrktServer : EditorWindow
    {
        #region Variables
        TcpListener server;
        List<TcpClient> clients;
        SystemDATContainer sysDATC;
        SystemDAT SYSDAT;
        CommunicationsDictionary commsDictionary;
        List<imageDatabase> uIDb;
        Vector2 consolePos;
        IEnumerator listener;
        List<IEnumerator> connectors, transferers;
        List<string> serverConsole;
        List<bool> connections;
        List<byte[]> clientNames;
        List<int> portals, commStates, Requests;
        byte[] su, us;
        string[] stateNames = { "Closed", "Listening", "Identifying", "Connected", "Communicating", "Sending", "Recieving", "Finished" };
        string[] commandNames = { "Start Server", "Shut Down Server",  "Attempt Connection", "Request Data", "Send Message", "Issue Alerts", "Statistics" };
        int serverState;
        #endregion

        #region Init Function
        [MenuItem("Window/Custom/vrrktServer")]
        static void Init()
        {
            vrrktServer vS = (vrrktServer)GetWindow(typeof(vrrktServer));
            vS.maxSize = new Vector2(Screen.width, Screen.height);
            EditorUtility.SetDirty(vS);
            vS.Show();
        }
        #endregion

        #region OnEnable Function
        private void OnEnable()
        {
            AssetDatabase.Refresh();

            #region Load SYS[D/C]
            sysDATC = Resources.Load<SystemDATContainer>("SystemDATContainer");
            #endregion

            #region Load Communications Dictionary
            string[] dbGUIDs = AssetDatabase.FindAssets("CommunicationsDictionary*");
            string[] dbPaths = new string[dbGUIDs.Length];
            for (int n = 0; n <= dbGUIDs.Length - 1; n++)
            {
                dbPaths[n] = AssetDatabase.GUIDToAssetPath(dbGUIDs[n]);
                //Debug.Log(dbPaths[n]);
            }
            commsDictionary = (CommunicationsDictionary)AssetDatabase.LoadAssetAtPath(dbPaths[1], typeof(CommunicationsDictionary));
            //Debug.Log("Comms dictionary " + CommsDictionary.name);
            #endregion

            #region GET/SET Main System Files
            SYSDAT = sysDATC.getSYSDAT();
            portals = commsDictionary.getPortals();
            #endregion

            #region Set Paths

            #endregion

            #region Assorted Variable SetPoints Set
            su = commsDictionary.getDefinition(0, 7);
            us = commsDictionary.getDefinition(6, 7);
            connections = new List<bool>();
            clients = new List<TcpClient>();
            clientNames = new List<byte[]>();
            serverConsole = new List<string>();
            serverState = 0;
            consolePos = Vector2.zero;
            commStates = new List<int>();
            for (int n = 0; n <= 6; n++)
            {
                commStates.Add(0);
                connections.Add(false);
                clientNames.Add(null);
            }

            if (SYSDAT == null && sysDATC.getPopulated())
            {
                SYSDAT = sysDATC.getSYSDAT();
            }
            #endregion
        }
        #endregion

        #region GUI Function
        private void OnGUI()
        {
            #region Connection Display Header
            GUILayout.BeginHorizontal();
            GUI.skin.box.stretchWidth = true;
            for(int n = 0; n <= 6; n++)
            {
                GUI.enabled = false;
                connections[n] = GUILayout.Toggle(connections[n], SYSDAT.userIDs[n]);
                GUI.enabled = true;
                GUILayout.Box("State: " + stateNames[commStates[n]]);
            }
            GUI.skin.box.stretchWidth = false;
            GUILayout.EndHorizontal();

            GUILayout.Box("Server State is " + stateNames[serverState]);
            #endregion

            #region Server Console

            #region Console Display
            consolePos = GUILayout.BeginScrollView(consolePos);
            for (int n = 0; n <= serverConsole.Count - 1; n++)
            {
                GUILayout.Box(serverConsole[n]);
            }
            GUILayout.EndScrollView();
            #endregion

            #region Clear Console Button
            if (GUILayout.Button("Clear Console"))
            {
                serverConsole = new List<string>();
            }
            #endregion

            #endregion

            #region Server Control Station
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            for(int n = 0; n <= 6; n++)
            {
                #region Top Row of Buttons
                GUILayout.BeginVertical();
                if (GUILayout.Button(commandNames[n]))
                {
                    if (n == 0)
                    {
                        if (listener != null)
                        {
                            VrrktCoroutine.destruct(listener);
                        }
                        listener = stage1Enum(1);
                        if (commsDictionary == null)
                        {
                            Debug.Log("commsDictionary is null");
                        }

                        if (commsDictionary.getPortals() == null)
                        {
                            Debug.Log("Portals are null");
                        }
                        else
                        {
                            Debug.Log(commsDictionary.getPortals().Count);
                        }
                        server = new TcpListener(IPAddress.Parse("127.0.0.1"), commsDictionary.getPortals()[0]);
                        Debug.Log(commsDictionary.getPortals()[0]);
                        VrrktCoroutine.start(listener);
                    }
                    else if (n == 1)
                    {
                        if (listener != null)
                        {
                            VrrktCoroutine.destruct(listener);
                        }
                        listener = stage1Enum(0);
                        VrrktCoroutine.start(listener);
                    }
                }
                #endregion

                #region Bottom Row of Buttons
                if (GUILayout.Button(SYSDAT.userIDs[n]))
                {
                    Debug.Log(SYSDAT.userIDs[n]);
                }
                GUILayout.EndVertical();
                #endregion
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(40);
            #endregion
        }
        #endregion
        
        #region Listener ENUM
        IEnumerator stage1Enum(int i)
        {
            string msg;
            if (i == 1)
            {
                #region Start Server
                clients = new List<TcpClient>();
                serverState = 1;
                server.Start();
                connectors = new List<IEnumerator>();
                msg = "Starting Server";
                serverConsole.Add(msg);
                Debug.Log(msg);
                #endregion

                #region Add Server IPE to Dictionary
                if (commsDictionary.getSIP() == null)
                {
                    commsDictionary.setSIP((IPEndPoint)server.Server.RemoteEndPoint);
                    msg = "Set server IPE in dictionary due to SIP = null";
                    serverConsole.Add(msg);
                    Debug.Log(msg);
                }
                else if (commsDictionary.getSIP() != (IPEndPoint)server.Server.RemoteEndPoint)
                {
                    commsDictionary.setSIP((IPEndPoint)server.Server.RemoteEndPoint);
                    msg = "Set server IPE in dictionary due to inequality";
                    serverConsole.Add(msg);
                    Debug.Log(msg);
                }
                #endregion

                #region Connect to Inbound Clients
                while (clients.Count < 7)
                {
                    if (server != null)
                    {
                        if (server.Pending())
                        {
                            TcpClient client = server.AcceptTcpClient();
                            clients.Add(client);
                            if (commStates[clients.IndexOf(client)] < 1)
                            {
                                commStates[clients.IndexOf(client)] = 1;
                            }
                            connectors.Add(stage2Enum(client));
                            VrrktCoroutine.start(connectors[connectors.Count - 1]);
                            msg = "Adding Unidentified Client";
                            serverConsole.Add(msg);
                            Debug.Log(msg);
                            yield return true;
                        }
                        else
                        {
                            yield return new WaitForSeconds(1f);
                            msg = "Can accept more clients";
                            if (!serverConsole.Contains(msg))
                            {
                                serverConsole.Add(msg);
                                Debug.Log(msg);
                            }
                            yield return true;
                        }
                    }
                }

                msg = "All clients connected";
                serverConsole.Add(msg);
                Debug.Log(msg);
                yield return false;
                #endregion
            }
            else if (i == 0)
            {
                #region Server Shutdown
                server.Stop();
                server = null;
                serverState = 0;
                listener = null;
                connectors = null;
                transferers = null;
                msg = "Shutting server down";
                serverConsole.Add(msg);
                Debug.Log(msg);
                yield return false;
                #endregion
            }
        }
        #endregion
        
        #region Connection ENUM
        IEnumerator stage2Enum(TcpClient client)
        {
            #region Connection Startup Processes
            string msg;
            int idx = clients.IndexOf(client);
            IEnumerator s = null;
            IEnumerator r = null;
            commStates[idx] = 0;
            NetworkStream nStream = client.GetStream();

            if (client.Connected)
            {
                if (serverState < 2)
                {
                    serverState = 2;
                }
            }
            #endregion

            while (client.Connected)
            {
                #region Send Connected Response[ServerPreset(0)] (commState 1 -> 2)
                while (commStates[idx] == 1)
                {
                    List<byte[]> OORespList = commsDictionary.getPresets(true)[0].getBytes(commsDictionary);
                    OORespList[0] = su;
                    OORespList[OORespList.Count - 1] = us;
                    List<byte> OOResp = new List<byte>();
                    for(int n = 0; n <= OORespList.Count - 1; n++)
                    {
                        for(int u = 0; u <= OORespList[n].Length - 1; u++)
                        {
                            OOResp.Add(OORespList[n][u]);
                        }
                    }
                    byte[] OO = new byte[OOResp.Count];
                    s = sendByPreset(idx, nStream, OO);
                    transferers[idx] = s;
                    VrrktCoroutine.start(s);
                    commStates[idx] = 2;
                    yield return true;
                }
                #endregion

                #region Await OO Send && Request 1 (commState 2 -> 3)
                while (commStates[idx] == 2)
                {
                    #region Await Send[OO] Finished
                    while (transferers[idx] != null)
                    {
                        yield return true;
                    }
                    #endregion

                    #region Read Incoming Request
                    while (r == null)
                    {
                        if (nStream.CanRead)
                        {
                            r = ReadData(nStream, idx);
                            VrrktCoroutine.start(r);
                            commStates[idx] = 3;
                            yield return true;
                        }
                        else
                        {
                            yield return true;
                        }
                    }
                    #endregion
                }
                #endregion

                #region Add Client IPE Information to Dictionary
                if (clientNames[idx] != null)
                {
                    IPEndPoint ipe = (IPEndPoint)client.Client.RemoteEndPoint;
                    if (commsDictionary.getIPE(clientNames[idx]) == null || commsDictionary.getIPE(clientNames[idx]) != ipe)
                    {
                        commsDictionary.setIPE(clientNames[idx], ipe);
                        msg = "Set client[" + clientNames[idx] + "] ipe in dictionary";
                        serverConsole.Add(msg);
                        Debug.Log(msg);
                        EditorUtility.SetDirty(commsDictionary);
                    }
                }
                #endregion

                #region Handle Request 1 & Send Data (commState 3 -> 4)
                while(commStates[idx] == 3)
                {
                    if (Requests[idx] == 1)
                    {
                        #region Step 1. Find Path, Load UDBC & Create UDB

                        #region Startup Operations
                        byte[] fDAT = new byte[0];
                        List<byte> bDAT = new List<byte>();
                        UserDatabaseContainer udbc;
                        double fSize = 0;
                        string clientRef = commsDictionary.findReferenceFromByte(clientNames[idx]);
                        int clientIdx = -1;
                        string filePath = "";
                        for(int n = 0; n <= SYSDAT.userIDs.Count - 1; n++)
                        {
                            if (SYSDAT.userIDs[n].Contains(clientRef))
                            {
                                clientIdx = n;
                                clientRef = SYSDAT.userIDs[n];
                                break;
                            }
                        }
                        yield return true;
                        #endregion

                        #region Obtain Path & Load UDBC
                        Debug.Log("client#" + idx + " identified as user " + clientRef);
                        string[] fG = AssetDatabase.FindAssets("UserDatabaseContainer*");
                        string[] fP = new string[fG.Length];
                        for(int n = 0; n <= fG.Length - 1; n++)
                        {
                            fP[n] = AssetDatabase.GUIDToAssetPath(fG[n]);
                        }

                        for(int n = 0; n <= fP.Length - 1; n++)
                        {
                            if (fP[n].Contains(".asset"))
                            {
                                if (fP[n].Contains(clientIdx + ""))
                                {
                                    filePath = fP[n];
                                }
                            }
                        }
                        udbc = (UserDatabaseContainer)AssetDatabase.LoadAssetAtPath(filePath, typeof(UserDatabaseContainer));
                        Debug.Log("Loaded Database Container");
                        yield return true;
                        #endregion

                        #region Create UDB & Convert to byte[]
                        userDatabase udb = new userDatabase();
                        udb.constructDatabase(udbc);
                        Debug.Log("Created User Database");
                        yield return true;

                        BinaryFormatter bf = new BinaryFormatter();
                        MemoryStream ms = new MemoryStream();
                        bf.Serialize(ms, udb);
                        fDAT = ms.ToArray();
                        Debug.Log("Converted Database to Byte[]");
                        yield return true;

                        #endregion

                        #endregion

                        #region Step 2. Determine Size, Construct Response & Convert to List<byte>

                        #region Determine Size & Convert Size to Byte[]
                        fSize = fDAT.Length;
                        Debug.Log("File size determined to be " + fSize);
                        yield return true;
                        
                        byte[] bSize = Encoding.Default.GetBytes(fSize + "");
                        Debug.Log("File size converted to byte array. Proceed to constructing response");
                        yield return true;
                        #endregion

                        #region Construct Response
                        List<byte[]> pre = commsDictionary.getPresets(true)[1].getBytes(commsDictionary);
                        pre[0] = su;
                        pre[8] = us;
                        pre[5] = bSize;
                        pre[7] = fDAT;
                        Debug.Log("Response Constructed. Convert to list");
                        yield return true;
                        #endregion

                        #region Convert to List<byte>
                        List<byte> lBytes = new List<byte>();
                        int conv = 0;
                        while(conv < pre.Count)
                        {
                            for(int n = 0; n <= pre[conv].Length - 1; n++)
                            {
                                lBytes.Add(pre[conv][n]);
                            }

                            if (conv < pre.Count - 1)
                            {
                                conv++;
                            }
                            yield return true;
                        }
                        Debug.Log("Converted to list. Proceed to final conversion");
                        yield return true;
                        #endregion

                        #endregion

                        #region Step 3. Convert List to Byte[]
                        int com = 0;
                        byte[] dat = new byte[lBytes.Count];
                        while(com < lBytes.Count)
                        {
                            for(int n = 0; n <= 500; n++)
                            {
                                if (com < lBytes.Count)
                                {
                                    dat[com] = lBytes[com];
                                    com++;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            yield return true;
                        }
                        Debug.Log("Conversion complete. Proceed to send");
                        s = sendByPreset(idx, nStream, dat);
                        transferers[idx] = s;
                        VrrktCoroutine.start(s);
                        Requests[idx] = 0;
                        commStates[idx] = 4;
                        #endregion
                    }
                }
                #endregion

                #region Await Completion of Send & Await Next Request (commState 4 -> 6)
                while(commStates[idx] == 4)
                {
                    #region Await Completion of Send
                    while (transferers[idx] != null)
                    {
                        yield return true;
                    }
                    #endregion

                    #region Read Incoming Request
                    while (r == null)
                    {
                        if (nStream.CanRead)
                        {
                            r = ReadData(nStream, idx);
                            VrrktCoroutine.start(r);
                            yield return true;
                        }
                        else
                        {
                            yield return true;
                        }
                    }
                    #endregion

                    #region Await Completion of Read
                    while(Requests[idx] != 2)
                    {
                        yield return true;
                    }

                    commStates[idx] = 6;
                    #endregion
                }
                #endregion

                #region Handle Request 2 & Send Data(commState 6 -> 7)
                while(commStates[idx] == 6)
                {
                    if (Requests[idx] == 2)
                    {
                        #region Step 1. Find Path, Load IDBC, Create IDB

                        #region Startup Operations
                        byte[] fDAT = new byte[0];
                        List<byte> bDAT = new List<byte>();
                        imageDatabaseContainer idbc;
                        double fSize = 0;
                        string clientRef = commsDictionary.findReferenceFromByte(clientNames[idx]);
                        int clientIdx = -1;
                        string filePath = "";
                        for (int n = 0; n <= SYSDAT.userIDs.Count - 1; n++)
                        {
                            if (SYSDAT.userIDs[n].Contains(clientRef))
                            {
                                clientIdx = n;
                                clientRef = SYSDAT.userIDs[n];
                                break;
                            }
                        }
                        yield return true;
                        #endregion

                        #region Obtain Path & Load IDBC
                        Debug.Log("client#" + idx + " identified as user " + clientRef);
                        string[] fG = AssetDatabase.FindAssets("ImageDatabaseContainer*");
                        string[] fP = new string[fG.Length];
                        for (int n = 0; n <= fG.Length - 1; n++)
                        {
                            fP[n] = AssetDatabase.GUIDToAssetPath(fG[n]);
                        }

                        for (int n = 0; n <= fP.Length - 1; n++)
                        {
                            if (fP[n].Contains(".asset"))
                            {
                                if (fP[n].Contains(clientIdx + ""))
                                {
                                    filePath = fP[n];
                                }
                            }
                        }
                        idbc = (imageDatabaseContainer)AssetDatabase.LoadAssetAtPath(filePath, typeof(imageDatabaseContainer));
                        Debug.Log("Loaded Database Container");
                        yield return true;
                        #endregion

                        #region Create IDB & Convert to byte[]
                        //Utilize IENUM to populate idb from idbc.
                        imageDatabase idb = new imageDatabase();
                        if (uIDb == null)
                        {
                            uIDb = new List<imageDatabase>();
                        }
                        int dx = uIDb.Count;
                        uIDb.Add(idb);
                        IEnumerator i = fillIDB(idbc, dx);
                        VrrktCoroutine.start(i);
                        while(!idb.getConstructorLock())
                        {
                            yield return true;
                        }
                        Debug.Log("Created Image Database");
                        yield return true;

                        BinaryFormatter bf = new BinaryFormatter();
                        MemoryStream ms = new MemoryStream();
                        bf.Serialize(ms, idb);
                        fDAT = ms.ToArray();
                        Debug.Log("Converted Database to Byte[]");
                        yield return true;
                        #endregion

                        #endregion

                        #region Step 2. Determine Size, Construct Response & Convert to List<byte>

                        #region Determine Size & Convert Size to Byte[]
                        fSize = fDAT.Length;
                        Debug.Log("File size determined to be " + fSize);
                        yield return true;

                        byte[] bSize = Encoding.Default.GetBytes(fSize + "");
                        Debug.Log("File size converted to byte array. Proceed to constructing response");
                        yield return true;
                        #endregion

                        #region Construct Response
                        List<byte[]> pre = commsDictionary.getPresets(true)[2].getBytes(commsDictionary);
                        pre[0] = su;
                        pre[8] = us;
                        pre[5] = bSize;
                        pre[7] = fDAT;
                        Debug.Log("Response Constructed. Proceed to final conversion");
                        yield return true;
                        #endregion

                        #region Convert to List<byte>
                        List<byte> lBytes = new List<byte>();
                        int conv = 0;
                        while (conv < pre.Count)
                        {
                            for (int n = 0; n <= pre[conv].Length - 1; n++)
                            {
                                lBytes.Add(pre[conv][n]);
                            }

                            if (conv < pre.Count - 1)
                            {
                                conv++;
                            }
                            yield return true;
                        }
                        Debug.Log("Converted to list. Proceed to final conversion");
                        yield return true;
                        #endregion

                        #endregion

                        #region Step 3. Convert List to Byte[]
                        int com = 0;
                        byte[] dat = new byte[lBytes.Count];
                        while (com < lBytes.Count)
                        {
                            for (int n = 0; n <= 500; n++)
                            {
                                if (com < lBytes.Count)
                                {
                                    dat[com] = lBytes[com];
                                    com++;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            yield return true;
                        }
                        Debug.Log("Conversion complete. Proceed to send");
                        s = sendByPreset(idx, nStream, dat);
                        transferers[idx] = s;
                        VrrktCoroutine.start(s);
                        Requests[idx] = 0;
                        commStates[idx] = 4;
                        #endregion
                    }
                }
                #endregion

                #region Await Completion of Data Send & Recieve Completion Response(commState 7 ->  8)
                while (commStates[idx] == 7)
                {
                    #region Await Completion of Send
                    while (transferers[idx] != null)
                    {
                        yield return true;
                    }
                    #endregion

                    #region Await Completion Response
                    while (r == null)
                    {
                        if (nStream.CanRead)
                        {
                            r = ReadData(nStream, idx);
                            VrrktCoroutine.start(r);
                            yield return true;
                        }
                        else
                        {
                            yield return true;
                        }
                    }
                    #endregion

                    #region Completed
                    while(Requests[idx] != -1)
                    {
                        yield return true;
                    }

                    commStates[idx] = 8;
                    #endregion
                }
                #endregion

                #region Close Connection 
                while(commStates[idx] == 8)
                {
                    nStream.Close();
                    commStates[idx] = 0;
                    connections[idx] = false;
                    clientNames[idx] = null;
                    clients[idx].Close();
                    clients[idx] = null;
                }
                #endregion
            }
            yield return false;
        }
        #endregion
        
        #region Read Data ENUM
        IEnumerator ReadData(NetworkStream net, int idx)
        {
            #region Read Data Startup
            string msg;
            List<byte> datIn = new List<byte>();
            int depth = 0;
            #endregion

            #region Read DataStream
            byte[] reading = new byte[16 * 1024];
            while (net.CanRead && (depth = net.Read(reading, depth, reading.Length)) > 0)
            {
                for (int n = 0; n <= reading.Length - 1; n++)
                {
                    datIn.Add(reading[n]);
                }
                yield return true;
            }
            #endregion
            
            #region Identify Client
            byte[] id = new byte[2];
            byte[] di = new byte[2];
            id[0] = datIn[0];
            id[1] = datIn[1];

            di[0] = datIn[datIn.Count - 2];
            di[1] = datIn[datIn.Count - 1];

            if (id[0] == di[1] && id[1] == di[0])
            {
                msg = "Opener & Closer match";
                serverConsole.Add(msg);
                Debug.Log(msg);
                int[] loc = new int[2];
                int[] col = new int[2];
                loc = commsDictionary.findDefinition(id);
                if (clientNames[idx] == null && loc[0] == 0)
                {
                    clientNames[idx] = id;
                }
                yield return true;
            }
            #endregion

            #region Preset Expectation Logic
            if (commStates[idx] < 8)
            {
                #region Identify Preset out of Data
                byte[] rq = commsDictionary.getDefinition(1, 0);
                int comp = 0;
                for (int n = 2; n <= 3; n++)
                {
                    if (rq[n - 2] == datIn[n])
                    {
                        comp++;
                    }
                }
                if (comp == 2)
                {
                    Debug.Log("Second array identified as request");
                }
                else
                {
                    Debug.Log("Comparison failed at second array");
                }
                comp = 0;
                byte[] ps = commsDictionary.getDefinition(2, 3);
                for (int n = 4; n <= 5; n++)
                {
                    if (ps[n - 4] == datIn[n])
                    {
                        comp++;
                    }
                }
                if (comp == 2)
                {
                    Debug.Log("Third array identified as preset");
                }
                else
                {
                    Debug.Log("Comparison failed at third array");
                }
                byte[] num = new byte[1];
                num[0] = datIn[6];
                string numC = Encoding.Default.GetString(num, 0, 1);
                int N;
                if (int.TryParse(numC, out N))
                {
                    Debug.Log("Preset request identified as #" + N);
                    Requests[idx] = N;
                }
                yield return false;
                #endregion
            }
            #endregion

            #region Completion Expectation Logic
            else
            {
                int compl = 0;
                while (compl != 3)
                {
                    byte[] XP = new byte[2];
                    byte[] XA = new byte[2];
                    byte[] RS = new byte[2];
                    for (int n = 0; n <= 1; n++)
                    {
                        XP[n] = datIn[n + 2];
                        XA[n] = datIn[n + 4];
                        RS[n] = datIn[n + 6];
                    }

                    if (commsDictionary.findReferenceFromByte(XP).Contains("IB"))
                    {
                        compl++;
                    }

                    if (commsDictionary.findReferenceFromByte(XA).Contains("RS"))
                    {
                        compl++;
                    }

                    if (commsDictionary.findReferenceFromByte(RS).Contains("OK"))
                    {
                        compl++;
                    }

                    if (compl == 3)
                    {
                        Requests[idx] = -1;
                        Debug.Log("Completion!!");
                    }
                    else
                    {
                        Debug.Log("Something is wrong");
                    }
                }
            }
            #endregion
        }
        #endregion

        #region Send By Preset
        IEnumerator sendByPreset(int idx, NetworkStream net, byte[] toSend)
        {
            using (net)
            {
                int wrote = 0;
                while(wrote != toSend.Length)
                {
                    net.WriteByte(toSend[wrote]);
                    wrote++;
                    yield return true;
                }
            }
            transferers[idx] = null;
            yield return false;
        }
        #endregion

        #region Populate IDB from IDBC ENUM
        IEnumerator fillIDB(imageDatabaseContainer dbc, int iIdx)
        {
            imageDatabase idb = uIDb[iIdx];
            Texture2D[] tx = new Texture2D[2];
            int limit = 0;
            int c1, c2, prog = 0;
            dbc.getListCounts(out c1, out c2);
            if (c1 != c2)
            {
                Debug.Log("List counts not ==");
            }
            else
            {
                Debug.Log("Limit is " + c1);
            }
            limit = c1;
            idb.setDimensions((int)dbc.getDimensions().x, (int)dbc.getDimensions().y);

            while(prog < limit)
            {
                for(int n = 0; n <= 100; n++)
                {
                    if(prog < limit)
                    {
                        tx = dbc.getImages(prog);
                        idb.constructDatabase(tx[0], tx[1]);
                        prog++;
                    }
                    else
                    {
                        break;
                    }
                }
                yield return true;
            }
            idb.setConstructorLock();
            Debug.Log("Database is complete");
            yield return false;
        }
        #endregion
    }
}
#endregion