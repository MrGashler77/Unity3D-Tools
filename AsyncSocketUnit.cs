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
public class AsyncSocketUnit
{
    #region Variables
    public Socket client;
    private AsyncSocketServer serv;
    VrrktUserDatabase cDB;
    VrrktUserImageDatabase iDB;
    VrrktSystemData vS;
    public int state;
    public int cP;
    private bool fileDone;
    private byte[] buff;
    private List<byte> append;
    public string id;
    public float sT, totTime;
    #endregion

    #region Constructor
    public AsyncSocketUnit(AsyncSocketServer srv)
    {
        client = null;
        serv = srv;
        state = 0;
        cP = 0;
        fileDone = false;
        buff = null;
        append = new List<byte>();
        id = "";
    }
    #endregion

    #region Begin Operation Function
    public void beginOperation(Socket cl)
    {
        client = cl;
        VrrktCoroutine.start(monitoring());
    }
    #endregion

    #region Monitoring ENUM
    IEnumerator monitoring()
    {
        #region Startup Operations
        int z = -1;
        MemoryStream m = new MemoryStream();
        #endregion

        #region Get Vsys
        while(vS == null)
        {
            vS = serv.getVsys();

            yield return true;
        }
        #endregion

        while (state < 9)
        {
            #region Request ID [0]
            while (state == 0)
            {
                if (cP == 0)
                {
                    serv.console.Add("Requesting ID 0");
                    asyncSend("RM!");
                    cP = -1;
                }
                else
                {
                    if (cP == 1)
                    {
                        cP = 0;
                        state++;
                    }
                }

                serv.Repaint();
                yield return true;
            }
            #endregion

            #region Receive ID [1]
            while (state == 1)
            {
                if (cP == 0)
                {
                    serv.console.Add("Receiving ID 1");
                    asyncReceive();
                    cP = -1;
                }
                else
                {
                    if (cP == 1)
                    {
                        cP = 0;
                        state++;
                    }
                }

                serv.Repaint();
                yield return true;
            }
            #endregion

            #region Accept/Deny(Get User Index) [2]
            while (state == 2)
            {
                if (cP == 0)
                {
                    for (int n = 0; n <= vS.userIDs.Count - 1; n++)
                    {
                        if (vS.userIDs[n].Contains(id))
                        {
                            z = n;
                            id = vS.userIDs[n];
                            break;
                        }
                    }

                    #region Accept
                    if (z >= 0)
                    {
                        serv.console.Add("Accept ID 2");
                        asyncSend("K!");
                    }
                    #endregion

                    #region Deny
                    else
                    {
                        serv.console.Add("Deny ID 2");
                        asyncSend("O!");
                    }
                    #endregion

                    cP = -1;
                }
                else
                {
                    if (cP == 1)
                    {
                        cP = 0;
                        state++;
                    }
                }

                serv.Repaint();
                yield return true;
            }
            #endregion

            #region Receive ACK [3]
            while (state == 3)
            {
                if (cP == 0)
                {
                    serv.console.Add("Receive Acknowledge 3");
                    asyncReceive();
                    cP = -1;
                }
                else
                {
                    if (cP == 1)
                    {
                        cP = 0;
                        state++;
                    }
                }

                serv.Repaint();
                yield return true;
            }
            #endregion

            #region Access & Send Card Database [4]
            while (state == 4)
            {
                if (cP == 0)
                {
                    serv.console.Add("Begin Card Database Operation 4");
                    cP = -1;
                    sT = Time.time;
                    fileControl();
                }
                else
                {
                    if (cP == 1)
                    {
                        cP = 0;
                        state++;
                    }
                    else
                    {
                        totTime = Time.time - sT;
                    }
                }

                serv.Repaint();
                yield return true;
            }
            #endregion

            #region Receive ACK [5]
            while (state == 5)
            {
                if (cP == 0)
                {
                    serv.console.Add("Receive Acknowledged 5");
                    asyncReceive();
                    cP = -1;
                }
                else
                {
                    if (cP == 1)
                    {
                        //cP = 0;
                        //state++;
                        if (!serv.console.Contains("Virtual End Point"))
                        {
                            serv.console.Add("Virtual End Point");
                        }
                    }
                }

                serv.Repaint();
                yield return true;
            }
            #endregion

            #region Access & Send Image Database [6]
            while (state == 6)
            {
                if (cP == 0)
                {
                    serv.console.Add("Begin Image Database Operation 6");
                    cP = -1;
                    fileControl();
                }
                else
                {
                    if (cP == 1)
                    {
                        if (cP == 1)
                        {
                            cP = 0;
                            state++;
                        }
                        else
                        {
                            totTime = Time.time - sT;
                        }
                    }
                }

                serv.Repaint();
                yield return true;
            }
            #endregion

            #region Receive Acknowledged [7]
            while (state == 7)
            {
                if (cP == 0)
                {
                    serv.console.Add("Receive Acknowledged 7");
                    asyncReceive();
                    cP = -1;
                }
                else
                {
                    if (cP == 1)
                    {
                        cP = 0;
                        state++;
                    }
                }

                serv.Repaint();
                yield return true;
            }
            #endregion

            #region Send Closing Notice [8]
            while (state == 8)
            {
                if (cP == 0)
                {
                    serv.console.Add("Send Closing 8");
                    asyncSend("X!");
                    cP = -1;
                }
                else
                {
                    if (cP == 1)
                    {
                        cP = 0;
                        state++;
                    }
                }

                serv.Repaint();
                yield return true;
            }
            #endregion

            yield return true;
        }

        #region End
        serv.console.Add("Server Operations Ended");
        client.Close();
        yield return false;
        #endregion
    }
    #endregion

    #region Send Functions

    #region Control Function
    async void fileControl()
    {
        byte[] b = null;
        FileStream f = null;
        
        #region Transfer Stream Bytes To Array

        #region Card Database
        if (state == 4)
        {
            serv.console.Add("Start Card Database Conversion");
            f = File.Open("C:/Users/robzo/Desktop/VrrktCentralDatabase/Database/" + id + ".vrrktcDb", FileMode.Open);
            b = new byte[f.Length];
            await f.ReadAsync(b, 0, b.Length);
            serv.console.Add("Wrote " + b.Length + " bytes for Card Database");
            serv.console.Add("Finish Card Database Conversion");
        }
        #endregion

        #region Image Database
        if (state == 6)
        {
            serv.console.Add("Start Image Database Conversion");
            f = File.Open("C:/Users/robzo/Desktop/VrrktCentralDatabase/Database/" + id + ".vrrktiDb", FileMode.Open);
            b = new byte[f.Length];
            await f.ReadAsync(b, 0, b.Length);
            serv.console.Add("Wrote " + b.Length + " bytes for Image Database");
            serv.console.Add("Finish Image Database Conversion");
        }
        #endregion

        #endregion

        #region Send Database & Await Callback
        serv.console.Add("Start Database Send");
        fileDone = false;
        try
        {
            fileSend(client, b);
        }
        catch (Exception e)
        {
            Debug.Log("Server exception in send.\n" + e.Message + "\n" + e.StackTrace);
        }
        try
        {
            await callbackCheck();
        }
        catch (Exception e)
        {
            Debug.Log("Server exception in callback Check.\n" + e.Message + "\n" + e.StackTrace);
        }
        f.Close();
        f.Dispose();
        while (!fileDone)
        {
            await Task.Delay(10);
        }
        serv.console.Add("Database Send Finished");
        cP = 1;
        #endregion
    }
    #endregion

    #region Send Message
    void asyncSend(string s)
    {
        byte[] bO;
        bO = Encoding.Default.GetBytes(s);

        client.BeginSend(bO, 0, bO.Length, SocketFlags.None, new AsyncCallback(sendCallback), client);
        //serv.console.Add("Began send message");
    }
    #endregion
    
    #region File Send Function
    void fileSend(Socket client, byte[] file)
    {
        List<byte> l = new List<byte>();
        byte[] bO;
        string s = "";

        #region Send Card Database [4]
        if (state == 4)
        {
            s = "C~";
            bO = Encoding.Default.GetBytes(s);

            for (int n = 0; n <= bO.Length - 1; n++)
            {
                l.Add(bO[n]);
            }

            for (int n = 0; n <= file.Length - 1; n++)
            {
                l.Add(file[n]);
            }

            s = "~!";
            bO = Encoding.Default.GetBytes(s);

            for (int n = 0; n <= bO.Length - 1; n++)
            {
                l.Add(bO[n]);
            }
        }
        #endregion

        #region Send Image Database [6]
        if (state == 6)
        {
            s = "I~";
            bO = Encoding.Default.GetBytes(s);

            for (int n = 0; n <= bO.Length - 1; n++)
            {
                l.Add(bO[n]);
            }

            for (int n = 0; n <= file.Length - 1; n++)
            {
                l.Add(file[n]);
            }

            s = "~!";
            bO = Encoding.Default.GetBytes(s);

            for (int n = 0; n <= bO.Length - 1; n++)
            {
                l.Add(bO[n]);
            }
        }
        #endregion

        bO = l.ToArray();

        serv.console.Add("Sending " + bO.Length + " bytes for database");
        client.BeginSend(bO, 0, bO.Length, SocketFlags.None, new AsyncCallback(sendCallback), client);
    }
    #endregion
    
    #region Send Callback Function
    void sendCallback(IAsyncResult ar)
    {
        Socket client = (Socket)ar.AsyncState;

        int bS = client.EndSend(ar);
        //serv.console.Add("Sent " + bS + " bytes");
        if (state == 4 || state == 6)
        {
            fileDone = true;
        }
        else
        {
            cP = 1;
        }
    }
    #endregion

    #region Callback Check Function
    async Task callbackCheck()
    {
        if (cP == -1)
        {
            while (!fileDone)
            {
                await Task.Delay(10);
            }
            serv.console.Add("Send finished");
        }
    }
    #endregion

    #endregion

    #region Receive Functions

    #region Receive Message
    void asyncReceive()
    {
        buff = new byte[client.Available];
        //serv.console.Add("Avail: " + client.Available);
        client.BeginReceive(buff, 0, client.Available, SocketFlags.None, new AsyncCallback(receiveCallback), client);
    }
    #endregion

    #region Receive Callback Function
    void receiveCallback(IAsyncResult ar)
    {
        #region Startup Operations
        bool end = false;
        bool error = false;
        string s = "";
        client = (Socket)ar.AsyncState;
        //serv.console.Add("Received " + client.EndReceive(ar) + " bytes.");
        #endregion

        #region Attempt Decode & Check for Delim
        s = Encoding.Default.GetString(buff);

        if (s.Contains("!"))
        {
            //serv.console.Add("Found delim");
            end = true;
        }
        else
        {
            //serv.console.Add("Delim not found");
            end = false;
        }

        //serv.console.Add("S: " + s);
        #endregion

        #region No End
        if (!end)
        {
            append = new List<byte>();
            for (int n = 0; n <= buff.Length - 1; n++)
            {
                append.Add(buff[n]);
            }
            asyncReceive();
        }
        #endregion

        #region End
        else
        {
            #region Check For Appended Data
            if (append.Count > 0)
            {
                for (int n = 0; n <= buff.Length - 1; n++)
                {
                    append.Add(buff[n]);
                }

                buff = append.ToArray();
                append = null;
            }
            #endregion

            #region Handle Message

            #region Receive ID [1]
            if (state == 1)
            {
                s = Encoding.Default.GetString(buff);

                if (s.Contains("M~"))
                {
                    string p = s.Substring(2);
                    p = p.Substring(0, p.IndexOf("~"));


                    for (int x = 0; x <= vS.userIDs.Count - 1; x++)
                    {
                        if (vS.userIDs[x].Contains(p))
                        {
                            //serv.console.Add("Client identified as " + vS.userIDs[x]);
                            id = vS.userIDs[x].Substring(3, 2);
                            cP = 1;
                            break;
                        }
                    }
                }
                else
                {
                    serv.console.Add("Error in message");
                    error = true;
                }
            }
            #endregion

            #region Receive ACK
            else
            {
                s = Encoding.Default.GetString(buff);

                if (s == "K!")
                {
                    cP = 1;
                }
                else
                {
                    serv.console.Add("Error in message");
                    error = true;
                }
            }
            #endregion

            #endregion
        }
        #endregion
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
            #region System 2
            if (i == 2)
            {
                vS = (VrrktSystemData)o;
            }
            #endregion
        }
        #endregion
    }
    #endregion

    #region Save Passback
    public void vrrktPassback(int t, int tL, int sT, int sTL, bool isDone, object re, Type ty)
    {
        if (cP == -3)
        {
            cP = -1;
        }
    }
    #endregion

    #endregion

}
#endregion
