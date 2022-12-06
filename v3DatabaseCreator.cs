#region Libraries
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

public class v3DatabaseCreator
{
    #region Variables
    public int index;
    public int current;
    public int limit;
    public int GID;
    public int lower;
    public int upper;
    public bool done;
    public string operation;
    public string step;
    public string user;
    string[] names;
    vrrktEditorVersion2 ed;
    vrrktGIDTable table;
    VrrktCard card;
    VrrktCardImage image;
    VrrktUserDatabase UDBc;
    VrrktUserImageDatabase UDBi;
    FileStream f;
    #endregion

    #region Constructor
    public v3DatabaseCreator()
    {
        index = -1;
        current = -1;
        limit = -1;
        done = false;
        operation = "";
        step = "";
    }
    #endregion

    #region Begin GID Table Creation
    public void startGIDTable(int l, int u, int i, string usr, string[] nm, vrrktEditorVersion2 e)
    {
        names = nm;
        index = i;
        lower = l;
        upper = u;
        user = usr;
        ed = e;
        done = false;
        operation = "GID Table";
        constructGIDTable();
    }
    #endregion

    #region Begin Card Database
    public void startCardDatabase(int i, vrrktEditorVersion2 e)
    {
        index = i;
        ed = e;
        done = false;
        operation = "Create Card Database";
        constructCardDatabase();
        repainter();
    }
    #endregion

    #region Begin Image Database
    public void startImageDatabase(int i, vrrktEditorVersion2 e)
    {
        index = i;
        ed = e;
        done = false;
        operation = "Create Image Database";
        constructImageDatabase();
        repainter();
    }
    #endregion

    #region Create GID Table Methods

    #region Control Method
    async void constructGIDTable()
    {
        #region Remove GIDs outside of our range
        step = "Remove Outside Range";
        await removeRange();
        step = "Removed Outside Range. Set Values";
        #endregion

        #region Trim Names
        step = "Trim Names. Set Values";
        await trimNames();
        step = "Trimmed Names.";
        //Debug.Log(user + " Finished Trim Names.");
        #endregion

        #region Name Integrity Check
        step = "Integrity Verify";
        await nameCheck();
        step = "Integrity Verified";
        //Debug.Log(user + " Finished Name Integrity Check.");
        #endregion

        #region Add To GIDTable
        table = new vrrktGIDTable();
        table.userID = user;
        step = "Add To Table";
        await addToTable();
        //Debug.Log(user + " Finished Add To Table.");
        #endregion

        #region Save GIDTable
        step = "Save Table";
        await saveTable();
        #endregion

        step = "Finished";
        //Debug.Log("Finished GID Table Creation For User: " + table.userID);
        done = true;
    }
    #endregion

    #region Tasks

    #region Remove Range
    async Task removeRange()
    {
        List<string> lst = new List<string>();
        current = 0;
        limit = names.Length;
        while(current < limit)
        {
            if (current >= lower && current <= upper)
            {
                lst.Add(names[current]);
            }
            current++;
        }
        names = lst.ToArray();
        await Task.Delay(1);
    }
    #endregion

    #region Trim Names
    async Task trimNames()
    {
        current = 0;
        limit = names.Length;
        for (int n = 0; n <= limit - 1; n++)
        {
            names[current] = names[current].Substring(names[current].IndexOf("/Database/") + 10, names[current].LastIndexOf(".vrrkt"));
            current++;
        }
        await Task.Delay(1);
    }
    #endregion

    #region Name Check
    async Task nameCheck()
    {
        current = 0;
        limit = names.Length;
        int q = -1;
        for (int n = 0; n <= limit - 1; n++)
        {
            q = -1;
            if (!int.TryParse(names[current], out q))
            {
                //Debug.Log("Failed Integrity Verification. S: " + names[current]);
            }
            current++;
        }
        await Task.Delay(1);
    }
    #endregion

    #region Add To Table
    async Task addToTable()
    {
        int q = -1;
        current = 0;
        limit = names.Length;
        for (int n = 0; n <= limit - 1; n++)
        {
            q = -1;
            if (int.TryParse(names[current], out q))
            {
                table.table.Add(q);
            }
            current++;
        }
        await Task.Delay(1);
    }
    #endregion

    #region Save Table
    async Task saveTable()
    {
        f = File.Create("C:/Users/robzo/Desktop/VrrktCentralDatabase/Resources/" + index + ".vrrktGt");
        table.writeToStream(f);
        f.Close();
        f.Dispose();
        await Task.Delay(1);
    }
    #endregion

    #endregion

    #endregion

    #region Create Card Database
    async void constructCardDatabase()
    {
        #region Load GID Table
        step = "Load GID Table";
        f = File.Open("C:/Users/robzo/Desktop/VrrktCentralDatabase/Resources/" + index + ".vrrktGt", FileMode.Open);
        table = new vrrktGIDTable();
        await Task.Run(() => table.readFromStream(f));
        f.Close();
        f.Dispose();
        user = table.userID;
        #endregion

        #region Set Variables
        step = "Set Variables";
        current = 0;
        limit = table.table.Count;
        UDBc = new VrrktUserDatabase();
        UDBc.user = table.userID;
        #endregion

        #region Iterate Populate Database
        step = "Populate";
        while (current < limit)
        {
            await Task.Run(() =>
            {
                for (int n = 0; n <= 9999; n++)
                {
                    if (current < limit)
                    {
                        GID = table.table[current];
                        f = File.OpenRead("C:/Users/robzo/Desktop/VrrktCentralDatabase/Database/" + table.table[current] + ".vrrktCd");
                        card = new VrrktCard();
                        card.readFromStream(f);
                        f.Close();
                        f.Dispose();
                        UDBc.setCard(card);
                        card = null;
                        current++;
                    }
                    else
                    {
                        break;
                    }
                }
            });
        }
        #endregion

        #region Save Database
        step = "Save";
        current = 0;
        f = File.Create("C:/Users/robzo/Desktop/VrrktCentralDatabase/Database/" + UDBc.user + ".vrrktcDb");
        limit = UDBc.beginWriteToStream(f);

        while (current <= limit)
        {
            await Task.Run(() =>
            {
                for (int n = 0; n <= 9999; n++)
                {
                    if (current <= limit)
                    {
                        UDBc.writeToStream(current);
                        current++;
                    }
                    else
                    {
                        break;
                    }
                }
            });
        }
        f.Close();
        f.Dispose();
        #endregion

        step = "Finished";
        Debug.Log("Finished Card Database Creation For User: " + UDBc.user);
        done = true;
    }
    #endregion

    #region Create Image Database
    async void constructImageDatabase()
    {
        #region Load GID Table
        step = "Load GID Table";
        f = File.OpenRead("C:/Users/robzo/Desktop/VrrktCentralDatabase/Resources/" + index + ".vrrktGt");
        table = new vrrktGIDTable();
        await Task.Run(() => table.readFromStream(f));
        f.Close();
        f.Dispose();
        user = table.userID;
        #endregion

        #region Set Variables
        step = "Set Variables";
        current = 0;
        limit = table.table.Count;
        UDBi = new VrrktUserImageDatabase();
        UDBi.user = table.userID;
        #endregion

        #region Iterate Populate Database
        step = "Populate";
        while (current < limit)
        {
            for (int n = 0; n <= 999; n++)
            {
                if (current < limit)
                {
                    await Task.Run(() =>
                    {
                        f = File.OpenRead("C:/Users/robzo/Desktop/VrrktCentralDatabase/Database/" + table.table[current] + ".vrrktCi");
                        image = new VrrktCardImage();
                    });

                    image.readFromStream(f);

                    await Task.Run(() =>
                    {
                        f.Close();
                        f.Dispose();
                        UDBi.setImage(image);
                        f = null;
                        image = null;
                        current++;
                    });
                }
                else
                {
                    break;
                }
            }
        }
        #endregion

        #region Save Database
        step = "Save";
        current = 0;
        f = File.Create("C:/Users/robzo/Desktop/VrrktCentralDatabase/Database/" + UDBi.user + ".vrrktiDb");
        limit = UDBi.beginWriteToStream(f);

        while (current <= limit)
        {
            await Task.Run(() =>
            {
                for (int n = 0; n <= 99; n++)
                {
                    if (current <= limit)
                    {
                        UDBi.writeToStream(current);
                        current++;
                    }
                    else
                    {
                        break;
                    }
                }
            });
        }
        f.Close();
        f.Dispose();
        #endregion

        step = "Finished";
        Debug.Log("Finished Image Database Creation For User: " + UDBi.user);
        done = true;
        current = 0;
        limit = 0;
    }
    #endregion

    #region Repainter
    async void repainter()
    {
        while (!done)
        {
            ed.Repaint();
            await Task.Delay(1);
        }
    }
    #endregion
}
