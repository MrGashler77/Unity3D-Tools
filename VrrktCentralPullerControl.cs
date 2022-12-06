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
using UnityEngine.Networking;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using Verruckt.Operations;
using System.Threading.Tasks;
#endregion

#region Puller Unit
namespace Verruckt.Editor
{
    public class VrrktPullerUnit
    {
        #region Variables

        #region Combined Async/ENUM Variables
        vrrktEditorVersion2 ed;
        bool asyncing;
        bool asynced;
        public bool running;
        public bool done;
        FileStream f;
        #endregion

        #region Puller Unit Variables
        public List<int> blanks { get; set; }
        public int current { get; set; }
        public int lower { get; set; }
        public int upper { get; set; }
        public int range { get; set; }
        public bool pullerComplete { get; set; }
        public Dictionary<string, string> colorDict { get; set; }
        public Dictionary<string, string> typeDict { get; set; }
        public Dictionary<string, string> lastDict { get; set; }
        private const string pathBase = "C:/Users/robzo/Desktop/VrrktCentralDatabase/Database/";
        private bool stop;
        private int i;
        private bool repullDone;
        public int repulled;
        #endregion

        #region Web Request Variables
        VrrktHttp vHTTP;
        public int stage { get; set; }
        public int step { get; set; }
        public const string dataBase = "https://gatherer.wizards.com/Pages/Card/Details.aspx?multiverseid=";
        private string retDat { get; set; }
        string o = "";
        private const string imgBase = "https://gatherer.wizards.com/Handlers/Image.ashx?multiverseid=";
        private const string imgEnd = "&type=card";
        #endregion
        
        #region Vrrkt Class Instances
        public VrrktCard vC { get; set; }
        public VrrktCardImage vCI { get; set; }
        #endregion
        
        #region Card 1 Variables
        private string c1retDat { get; set; }
        private List<string> c1Parts { get; set; }
        private List<string> c1Rules { get; set; }

        #endregion

        #region Card 1 Image Variables
        public Texture2D img1 { get; set; }
        public int i1GID { get; set; }
        #endregion
        
        #region Card 2 Variables
        private string c2retDat { get; set; }
        private List<string> c2Parts { get; set; }
        private List<string> c2Rules { get; set; }
        #endregion

        #region Card 2 Image Variables
        public Texture2D img2 { get; set; }
        public int i2GID { get; set; }
        #endregion
        
        #region Unit Variables
        private bool split { get; set; }
        private List<string> unitParts { get; set; }
        #endregion

        #endregion

        #region Constructor
        public VrrktPullerUnit(int low, int high)
        {
            lower = low;
            upper = high;
            if (lower == 0)
            {
                lower = 1;
            }
            current = lower - 1;
            if (current <= 0)
            {
                current = 1;
            }
            blanks = new List<int>();
            pullerComplete = false;
            stop = false;
            range = (upper - lower) + 1;
            retDat = "";
            vC = null;
            vCI = null;
            c1retDat = "";
            c2retDat = "";
            c1Parts = null;
            c2Parts = null;
            c1Rules = null;
            c2Rules = null;
            img1 = null;
            img2 = null;
            i1GID = -1;
            i2GID = -1;
            split = false;
            unitParts = null;

            stage = 1;
            step = 1;
        }
        #endregion
        
        #region Control Methods
        
        #region Resume
        public void resume()
        {
            stop = false;
            retDat = "";
            vC = null;
            vCI = null;
            c1retDat = "";
            c2retDat = "";
            c1Parts = null;
            c2Parts = null;
            c1Rules = null;
            c2Rules = null;
            img1 = null;
            img2 = null;
            i1GID = -1;
            i2GID = -1;
            split = false;
            unitParts = null;

            if (current < upper)
            {
                stage = 1;
                step = 1;
            }
            else
            {
                pullerComplete = true;
            }
        }
        #endregion

        #region Stop
        public void Stop()
        {
            stop = true;
        }
        #endregion

        #endregion
        
        #region Puller Methods

        #region Card Cycle Control

        #region Async Control Method
        public async void asyncCardCycle(vrrktEditorVersion2 e)
        {
            ed = e;
            repainter();
            vHTTP = new VrrktHttp();

            while (current <= upper)
            {
                await Task.Run(async () =>
                {
                    #region Stage 1
                    while (stage == 1)
                    {
                        #region Step 1
                        while (step == 1)
                        {
                            while (retDat == "")
                            {
                                try
                                {
                                    retDat = await vHTTP.getURLText(dataBase + current);
                                }
                                catch (Exception em)
                                {
                                    Debug.Log("Error occured on outer catch. \n" + em.Message);
                                    step = 1;
                                    stage = 1;
                                }

                                step++;
                            }
                        }

                        #endregion

                        #region Step 2
                        while (step == 2)
                        {
                            if (retDat.Contains("Card Name"))
                            {
                                step++;
                            }
                            else
                            {
                                step = 1;
                                retDat = "";
                                if (blanks == null)
                                {
                                    blanks = new List<int>();
                                }
                                blanks.Add(current);
                                if (current < upper)
                                {
                                    current++;
                                }
                                else
                                {
                                    pullerComplete = true;
                                }
                                split = false;
                            }
                        }
                        #endregion

                        #region Step 3
                        while (step == 3)
                        {
                            MatchCollection mC = Regex.Matches(retDat, "Card Name");
                            //Debug.Log("Card Name Instances = " + mC.Count);

                            if (mC.Count == 2)
                            {
                                split = true;
                                //Debug.Log("Split");
                                step++;
                            }
                            else
                            {
                                if (mC.Count == 1)
                                {
                                    split = false;
                                    //Debug.Log("!Split");
                                    step++;
                                }
                                else
                                {
                                    split = false;
                                    blanks.Add(current);
                                    //Debug.Log("Blank");
                                    step = 1;
                                    retDat = "";
                                    if (current < upper)
                                    {
                                        current++;
                                    }
                                    else
                                    {
                                        pullerComplete = true;
                                    }
                                    //Debug.Log("Card Is showing blank!!");
                                }
                            }
                        }
                        #endregion

                        #region Step 4
                        while (step == 4)
                        {
                            c1retDat = retDat.Substring(retDat.IndexOf("Card Name"));

                            if (split)
                            {
                                c1retDat = c1retDat.Substring(0, c1retDat.LastIndexOf("Card Name"));
                                c2retDat = retDat.Substring(retDat.LastIndexOf("Card Name"), retDat.Length - retDat.LastIndexOf("Card Name"));

                                o = retDat.Substring(retDat.LastIndexOf(".ashx?multiverseid=") + 19);
                                if (o.IndexOf("&type=card") < 0)
                                {
                                    if (o.IndexOf("&amp;type=card") >= 0)
                                    {
                                        o = o.Substring(0, o.IndexOf("&amp;type=card"));
                                    }
                                }
                                else
                                {
                                    o = o.Substring(0, o.IndexOf("&type=card"));
                                }

                                i = -1;
                                int.TryParse(o, out i);
                                if (i != -1)
                                {
                                    i2GID = i;
                                }
                                else
                                {
                                    i2GID = -1;
                                    Debug.Log("Unable to parse card 2 image GID");
                                }
                            }
                            else
                            {
                                i2GID = -1;
                            }
                            i1GID = current;

                            retDat = "";
                            stage = 2;
                            step = 1;
                        }
                        #endregion
                    }
                    #endregion

                    #region Stage 2
                    while (stage == 2)
                    {
                        #region Step 1
                        while (step == 1)
                        {
                            c1Parts = new List<string>();
                            c2Parts = new List<string>();

                            o = c1retDat.Substring(c1retDat.IndexOf("Card Name:"));
                            o = simpleTrim(o);
                            c1Parts.Add(o);

                            if (split)
                            {
                                o = c2retDat.Substring(c2retDat.IndexOf("Card Name:"));
                                o = simpleTrim(o);
                                c2Parts.Add(o);
                            }
                            step++;
                        }
                        #endregion

                        #region Step 2
                        while (step == 2)
                        {
                            if (c1retDat.Contains("Mana Cost:"))
                            {
                                o = c1retDat.Substring(c1retDat.IndexOf("Mana Cost:"));
                                o = o.Substring(0, o.IndexOf("<div id="));
                                o = o.Substring(o.IndexOf("value") + 7, o.LastIndexOf("</div>") - (o.IndexOf("value") + 7));
                                o = o.Substring(0, o.LastIndexOf("</div>"));
                                o = symbolProcessor(o);
                                c1Parts.Add(o);
                            }
                            else
                            {
                                c1Parts.Add("");
                            }

                            if (split)
                            {
                                if (c2retDat.Contains("Mana Cost:"))
                                {
                                    o = c2retDat.Substring(c2retDat.IndexOf("Mana Cost:"));
                                    o = o.Substring(0, o.IndexOf("<div id="));
                                    o = o.Substring(o.IndexOf("value") + 7, o.LastIndexOf("</div>") - (o.IndexOf("value") + 7));
                                    o = o.Substring(0, o.LastIndexOf("</div>"));
                                    o = symbolProcessor(o);
                                    c2Parts.Add(o);
                                }
                                else
                                {
                                    c2Parts.Add("");
                                }
                            }
                            step++;
                        }
                        #endregion

                        #region Step 3
                        while (step == 3)
                        {
                            if (c1retDat.Contains("Converted Mana Cost:"))
                            {
                                o = c1retDat.Substring(c1retDat.IndexOf("Converted Mana Cost:"));
                                o = simpleTrim(o);
                                c1Parts.Add(o);
                            }
                            else
                            {
                                c1Parts.Add("");
                            }

                            if (split)
                            {
                                if (c2retDat.Contains("Converted Mana Cost:"))
                                {
                                    o = c2retDat.Substring(c2retDat.IndexOf("Converted Mana Cost:"));
                                    o = simpleTrim(o);
                                    c2Parts.Add(o);
                                }
                                else
                                {
                                    c2Parts.Add("");
                                }
                            }
                            step++;
                        }
                        #endregion

                        #region Step 4
                        while (step == 4)
                        {
                            if (c1retDat.Contains("Types:"))
                            {
                                o = c1retDat.Substring(c1retDat.IndexOf("Types:"));
                                o = modifiedTrim(o, false);
                                c1Parts.Add(o);
                            }
                            else
                            {
                                c1Parts.Add("");
                            }

                            if (split)
                            {
                                if (c2retDat.Contains("Types:"))
                                {
                                    o = c2retDat.Substring(c2retDat.IndexOf("Types:"));
                                    o = modifiedTrim(o, false);
                                    c2Parts.Add(o);
                                }
                                else
                                {
                                    c2Parts.Add("");
                                }
                            }
                            step++;
                        }
                        #endregion

                        #region Step 5
                        while (step == 5)
                        {
                            if (c1retDat.Contains("Card Text:"))
                            {
                                o = c1retDat.Substring(c1retDat.IndexOf("Card Text:"));
                                o = o.Substring(0, o.IndexOf("<div id="));
                                o = o.Substring(o.IndexOf("value") + 7, o.LastIndexOf("</div>") - (o.IndexOf("value") + 7));
                                o = cardTextTrim(o);
                                c1Parts.Add(o);
                            }
                            else
                            {
                                c1Parts.Add("");
                            }

                            if (split)
                            {
                                if (c2retDat.Contains("Card Text:"))
                                {
                                    o = c2retDat.Substring(c2retDat.IndexOf("Card Text:"));
                                    o = o.Substring(0, o.IndexOf("<div id="));
                                    o = o.Substring(o.IndexOf("value") + 7, o.LastIndexOf("</div>") - (o.IndexOf("value") + 7));
                                    o = cardTextTrim(o);
                                    c2Parts.Add(o);
                                }
                                else
                                {
                                    c2Parts.Add("");
                                }
                            }
                            step++;
                        }
                        #endregion

                        #region Step 6
                        while (step == 6)
                        {
                            if (c1retDat.Contains("Flavor Text:"))
                            {
                                o = c1retDat.Substring(c1retDat.IndexOf("Flavor Text:"));
                                o = modifiedTrim(o, true);
                                c1Parts.Add(o);
                            }
                            else
                            {
                                c1Parts.Add("");
                            }

                            if (split)
                            {
                                if (c2retDat.Contains("Flavor Text:"))
                                {
                                    o = c2retDat.Substring(c2retDat.IndexOf("Flavor Text:"));
                                    o = modifiedTrim(o, true);
                                    c2Parts.Add(o);
                                }
                                else
                                {
                                    c2Parts.Add("");
                                }
                            }
                            step++;
                        }
                        #endregion

                        #region Step 7
                        while (step == 7)
                        {
                            if (c1retDat.Contains("P/T:"))
                            {
                                o = c1retDat.Substring(c1retDat.IndexOf("P/T:"));
                                o = simpleTrim(o);
                                c1Parts.Add(o);
                            }
                            else
                            {
                                c1Parts.Add("");
                            }

                            if (split)
                            {
                                if (c2retDat.Contains("P/T:"))
                                {
                                    o = c2retDat.Substring(c2retDat.IndexOf("P/T:"));
                                    o = simpleTrim(o);
                                    c2Parts.Add(o);
                                }
                                else
                                {
                                    c2Parts.Add("");
                                }
                            }
                            step++;
                        }
                        #endregion

                        #region Step 8
                        while (step == 8)
                        {
                            if (c1retDat.Contains("Loyalty:"))
                            {
                                o = c1retDat.Substring(c1retDat.IndexOf("Loyalty:"));
                                o = simpleTrim(o);
                                c1Parts.Add(o);
                            }
                            else
                            {
                                c1Parts.Add("");
                            }

                            if (split)
                            {
                                if (c2retDat.Contains("Loyalty:"))
                                {
                                    o = c2retDat.Substring(c2retDat.IndexOf("Loyalty:"));
                                    o = simpleTrim(o);
                                    c2Parts.Add(o);
                                }
                                else
                                {
                                    c2Parts.Add("");
                                }
                            }
                            step++;
                        }
                        #endregion

                        #region Step 9
                        while (step == 9)
                        {
                            if (c1retDat.Contains("Color Indicator:"))
                            {
                                o = c1retDat.Substring(c1retDat.IndexOf("Color Indicator:"));
                                o = simpleTrim(o);
                                c1Parts.Add(o);
                            }
                            else
                            {
                                c1Parts.Add("");
                            }
                            i1GID = current;

                            if (split)
                            {
                                if (c2retDat.Contains("Color Indicator:"))
                                {
                                    o = c2retDat.Substring(c2retDat.IndexOf("Color Indicator:"));
                                    o = simpleTrim(o);
                                    c2Parts.Add(o);
                                }
                                else
                                {
                                    c2Parts.Add("");
                                }
                            }
                            step++;
                        }
                        #endregion

                        #region Step 10
                        while (step == 10)
                        {
                            c1Rules = new List<string>();
                            c2Rules = new List<string>();
                            if (c1retDat.Contains("rulingsHeader"))
                            {
                                o = c1retDat.Substring(c1retDat.IndexOf("rulingsHeader"));
                                o = o.Substring(o.IndexOf("rulingsTable"), o.IndexOf("</table>") - o.IndexOf("rulingsTable"));
                                o = o.Substring(o.IndexOf(">") + 1);
                                o = o.Substring(0, o.LastIndexOf("</tr>"));
                                c1Rules = rulesTrim(o);
                            }

                            if (split)
                            {
                                if (c2retDat.Contains("rulingsHeader"))
                                {
                                    o = c2retDat.Substring(c2retDat.IndexOf("rulingsHeader"));
                                    o = o.Substring(o.IndexOf("rulingsTable"), o.IndexOf("</table>") - o.IndexOf("rulingsTable"));
                                    o = o.Substring(o.IndexOf(">") + 1);
                                    o = o.Substring(0, o.LastIndexOf("</tr>"));
                                    c2Rules = rulesTrim(o);
                                }
                            }
                            step++;
                        }
                        #endregion

                        #region Step 11
                        while (step == 11)
                        {
                            unitParts = new List<string>();
                            o = c1retDat.Substring(c1retDat.IndexOf("Expansion:"));
                            o = unitTrim(o, 0);
                            o = o.Replace(">", "");
                            unitParts.Add(o);

                            step++;
                        }
                        #endregion

                        #region Step 12
                        while (step == 12)
                        {
                            o = c1retDat.Substring(c1retDat.IndexOf("Rarity:"));
                            o = unitTrim(o, 1);
                            o = o.Substring(o.LastIndexOf(">") + 1, o.Length - (o.LastIndexOf(">") + 1));
                            unitParts.Add(o);

                            step++;
                        }
                        #endregion

                        #region Step 13
                        while (step == 13)
                        {
                            o = c1retDat.Substring(c1retDat.IndexOf("Artist:"));
                            o = unitTrim(o, 2);
                            o = o.Replace(">", "");
                            unitParts.Add(o);

                            step++;
                        }
                        #endregion

                        #region Step 14
                        while (step == 14)
                        {
                            for (int n = 0; n <= c1Parts.Count - 1; n++)
                            {
                                c1Parts[n] = symbolProcessor(c1Parts[n]);
                            }

                            if (c2Parts != null && c2Parts.Count > 0)
                            {
                                {
                                    for (int n = 0; n <= c2Parts.Count - 1; n++)
                                    {
                                        c2Parts[n] = symbolProcessor(c2Parts[n]);
                                    }
                                }
                            }

                            if (c1Rules != null && c1Rules.Count > 0)
                            {
                                {
                                    for (int n = 0; n <= c1Rules.Count - 1; n++)
                                    {
                                        c1Rules[n] = symbolProcessor(c1Rules[n]);
                                    }
                                }
                            }

                            if (c2Rules != null && c2Rules.Count > 0)
                            {
                                {
                                    for (int n = 0; n <= c2Rules.Count - 1; n++)
                                    {
                                        c2Rules[n] = symbolProcessor(c2Rules[n]);
                                    }
                                }
                            }

                            if (unitParts != null && unitParts.Count > 0)
                            {
                                {
                                    for (int n = 0; n <= unitParts.Count - 1; n++)
                                    {
                                        unitParts[n] = symbolProcessor(unitParts[n]);
                                    }
                                }
                            }

                            stage = 3;
                            step = 1;
                        }
                        #endregion
                    }
                    #endregion

                    #region Stage 3
                    while (stage == 3)
                    {
                        #region Step 1
                        while (step == 1)
                        {
                            vC = new VrrktCard(i1GID, split, c1Parts, c2Parts, unitParts[0], unitParts[1], unitParts[2], c1Rules, c2Rules, i2GID);
                            //Debug.Log("i1GID:" + vC.GID + ", i2GID:" + vC.i2GID);
                            vrrktSave(vC);
                            step++;
                        }
                        #endregion

                        #region Step 2
                        while (step == 2)
                        {
                            if (current < upper)
                            {
                                current++;
                            }
                            else
                            {
                                pullerComplete = true;
                            }
                            split = false;

                            //if (lower == 0 || lower == 1)
                            //{
                            //    if (current == 5 || current == 10 || current == 20)
                            //    {
                            //        Debug.Log(vC.getCardInfo());
                            //    }
                            //}

                            stage = 1;
                            step = 1;
                        }
                        #endregion
                    }
                    #endregion
                });
            }
        }
        #endregion

        #region Card Cycle
        public void startCardCycle(vrrktEditorVersion2 e)
        {
            ed = e;
            repainter();
            running = false;
            done = false;
            step = 1;
            stage = 1;
        }
        #endregion

        #region Computation Method
        public IEnumerator cardCycleComputation(int sg, int sp)
        {
            UnityWebRequest uweb = null;

            #region Stage 1
            if (sg == 1)
            {
                #region Step 1
                if (sp == 1)
                {
                    if (!File.Exists(pathBase + current + ".vrrktCd"))
                    {
                        done = true;
                    }
                    else
                    {
                        current++;
                        step = 1;
                        stage = 1;
                        running = false;
                        done = false;
                        yield return false;
                    }

                    yield return false;
                }
                #endregion

                #region Step 2
                if (sp == 2)
                {
                    uweb = new UnityWebRequest();
                    using (uweb = UnityWebRequest.Get(dataBase + current))
                    {
                        uweb.SendWebRequest();

                        while (!uweb.isDone)
                        {
                            yield return true;
                        }

                        if (uweb.isHttpError || uweb.isNetworkError)
                        {
                            //Debug.Log("Error occured on HTML GET. " + uweb.error);
                            step = 1;
                            stage = 1;
                            running = false;
                            done = false;
                            yield return false;
                        }
                        else
                        {
                            retDat = uweb.downloadHandler.text;
                            done = true;
                            yield return false;
                        }
                    }
                }
                #endregion

                #region Step 3
                if (sp == 3)
                {
                    if (retDat.Contains("Card Name"))
                    {
                        done = true;
                        yield return false;
                    }
                    else
                    {
                        retDat = "";
                        if (blanks == null)
                        {
                            blanks = new List<int>();
                        }
                        blanks.Add(current);
                        if (current < upper)
                        {
                            current++;
                        }
                        else
                        {
                            pullerComplete = true;
                        }
                        split = false;
                        step = 1;
                        running = false;
                        yield return false;
                    }
                }
                #endregion

                #region Step 4
                if (sp == 4)
                {
                    MatchCollection mC = Regex.Matches(retDat, "Card Name");
                    //Debug.Log("Card Name Instances = " + mC.Count);

                    if (mC.Count == 2)
                    {
                        split = true;
                        //Debug.Log("Split");
                        stage = 2;
                        step = 1;
                        running = false;
                        yield return false;
                    }
                    else
                    {
                        if (mC.Count == 1)
                        {
                            split = false;
                            //Debug.Log("!Split");
                            stage = 2;
                            step = 1;
                            running = false;
                            yield return false;
                        }
                        else
                        {
                            blanks.Add(current);
                            //Debug.Log("Blank");
                            if (current < upper)
                            {
                                current++;
                            }
                            else
                            {
                                pullerComplete = true;
                            }
                            //Debug.Log("Card Is showing blank!!");
                            split = false;
                            stage = 1;
                            step = 1;
                            retDat = "";
                            running = false;
                            yield return false;
                        }
                    }
                }
                #endregion
            }
            #endregion

            #region Stage 2
            if (sg == 2)
            {
                #region Step 1
                if (sp == 1)
                {
                    c1retDat = retDat.Substring(retDat.IndexOf("Card Name"));

                    if (split)
                    {
                        c1retDat = c1retDat.Substring(0, c1retDat.LastIndexOf("Card Name"));
                        c2retDat = retDat.Substring(retDat.LastIndexOf("Card Name"), retDat.Length - retDat.LastIndexOf("Card Name"));

                        o = retDat.Substring(retDat.LastIndexOf(".ashx?multiverseid=") + 19);
                        if (o.IndexOf("&type=card") < 0)
                        {
                            if (o.IndexOf("&amp;type=card") >= 0)
                            {
                                o = o.Substring(0, o.IndexOf("&amp;type=card"));
                            }
                        }
                        else
                        {
                            o = o.Substring(0, o.IndexOf("&type=card"));
                        }

                        i = -1;
                        int.TryParse(o, out i);
                        if (i != -1)
                        {
                            i2GID = i;
                        }
                        else
                        {
                            i2GID = -1;
                            Debug.Log("Unable to parse card 2 image GID");
                        }
                    }
                    else
                    {
                        i2GID = -1;
                    }
                    i1GID = current;

                    retDat = "";
                    done = true;
                    yield return false;
                }
                #endregion

                #region Step 2
                if (sp == 2)
                {
                    c1Parts = new List<string>();
                    c2Parts = new List<string>();

                    o = c1retDat.Substring(c1retDat.IndexOf("Card Name:"));
                    o = simpleTrim(o);
                    c1Parts.Add(o);

                    if (split)
                    {
                        o = c2retDat.Substring(c2retDat.IndexOf("Card Name:"));
                        o = simpleTrim(o);
                        c2Parts.Add(o);
                    }
                    done = true;
                    yield return false;
                }
                #endregion

                #region Step 3
                if (sp == 3)
                {
                    if (c1retDat.Contains("Mana Cost:"))
                    {
                        o = c1retDat.Substring(c1retDat.IndexOf("Mana Cost:"));
                        o = o.Substring(0, o.IndexOf("<div id="));
                        o = o.Substring(o.IndexOf("value") + 7, o.LastIndexOf("</div>") - (o.IndexOf("value") + 7));
                        o = o.Substring(0, o.LastIndexOf("</div>"));
                        o = symbolProcessor(o);
                        c1Parts.Add(o);
                    }
                    else
                    {
                        c1Parts.Add("");
                    }

                    if (split)
                    {
                        if (c2retDat.Contains("Mana Cost:"))
                        {
                            o = c2retDat.Substring(c2retDat.IndexOf("Mana Cost:"));
                            o = o.Substring(0, o.IndexOf("<div id="));
                            o = o.Substring(o.IndexOf("value") + 7, o.LastIndexOf("</div>") - (o.IndexOf("value") + 7));
                            o = o.Substring(0, o.LastIndexOf("</div>"));
                            o = symbolProcessor(o);
                            c2Parts.Add(o);
                        }
                        else
                        {
                            c2Parts.Add("");
                        }
                    }
                    done = true;
                    yield return false;
                }
                #endregion

                #region Step 4
                if (sp == 4)
                {
                    if (c1retDat.Contains("Converted Mana Cost:"))
                    {
                        o = c1retDat.Substring(c1retDat.IndexOf("Converted Mana Cost:"));
                        o = simpleTrim(o);
                        c1Parts.Add(o);
                    }
                    else
                    {
                        c1Parts.Add("");
                    }

                    if (split)
                    {
                        if (c2retDat.Contains("Converted Mana Cost:"))
                        {
                            o = c2retDat.Substring(c2retDat.IndexOf("Converted Mana Cost:"));
                            o = simpleTrim(o);
                            c2Parts.Add(o);
                        }
                        else
                        {
                            c2Parts.Add("");
                        }
                    }
                    done = true;
                    yield return false;
                }
                #endregion

                #region Step 5
                if (sp == 5)
                {
                    if (c1retDat.Contains("Types:"))
                    {
                        o = c1retDat.Substring(c1retDat.IndexOf("Types:"));
                        o = modifiedTrim(o, false);
                        c1Parts.Add(o);
                    }
                    else
                    {
                        c1Parts.Add("");
                    }

                    if (split)
                    {
                        if (c2retDat.Contains("Types:"))
                        {
                            o = c2retDat.Substring(c2retDat.IndexOf("Types:"));
                            o = modifiedTrim(o, false);
                            c2Parts.Add(o);
                        }
                        else
                        {
                            c2Parts.Add("");
                        }
                    }
                    done = true;
                    yield return false;
                }
                #endregion

                #region Step 6
                if (sp == 6)
                {
                    if (c1retDat.Contains("Card Text:"))
                    {
                        o = c1retDat.Substring(c1retDat.IndexOf("Card Text:"));
                        o = o.Substring(0, o.IndexOf("<div id="));
                        o = o.Substring(o.IndexOf("value") + 7, o.LastIndexOf("</div>") - (o.IndexOf("value") + 7));
                        o = cardTextTrim(o);
                        c1Parts.Add(o);
                    }
                    else
                    {
                        c1Parts.Add("");
                    }
                    done = true;
                    yield return false;
                }
                #endregion

                #region Step 7
                if (sp == 7)
                {
                    if (split)
                    {
                        if (c2retDat.Contains("Card Text:"))
                        {
                            o = c2retDat.Substring(c2retDat.IndexOf("Card Text:"));
                            o = o.Substring(0, o.IndexOf("<div id="));
                            o = o.Substring(o.IndexOf("value") + 7, o.LastIndexOf("</div>") - (o.IndexOf("value") + 7));
                            o = cardTextTrim(o);
                            c2Parts.Add(o);
                        }
                        else
                        {
                            c2Parts.Add("");
                        }
                    }
                    done = true;
                    yield return false;
                }
                #endregion

                #region Step 8
                if (sp == 8)
                {
                    if (c1retDat.Contains("Flavor Text:"))
                    {
                        o = c1retDat.Substring(c1retDat.IndexOf("Flavor Text:"));
                        o = modifiedTrim(o, true);
                        c1Parts.Add(o);
                    }
                    else
                    {
                        c1Parts.Add("");
                    }

                    done = true;
                    yield return false;
                }
                #endregion

                #region Step 9
                if (sp == 9)
                {
                    if (split)
                    {
                        if (c2retDat.Contains("Flavor Text:"))
                        {
                            o = c2retDat.Substring(c2retDat.IndexOf("Flavor Text:"));
                            o = modifiedTrim(o, true);
                            c2Parts.Add(o);
                        }
                        else
                        {
                            c2Parts.Add("");
                        }
                    }

                    done = true;
                    yield return false;
                }
                #endregion

                #region Step 10
                if (sp == 10)
                {
                    if (c1retDat.Contains("P/T:"))
                    {
                        o = c1retDat.Substring(c1retDat.IndexOf("P/T:"));
                        o = simpleTrim(o);
                        c1Parts.Add(o);
                    }
                    else
                    {
                        c1Parts.Add("");
                    }

                    done = true;
                    yield return false;
                }
                #endregion

                #region Step 11
                if (sp == 11)
                {
                    if (split)
                    {
                        if (c2retDat.Contains("P/T:"))
                        {
                            o = c2retDat.Substring(c2retDat.IndexOf("P/T:"));
                            o = simpleTrim(o);
                            c2Parts.Add(o);
                        }
                        else
                        {
                            c2Parts.Add("");
                        }
                    }

                    done = true;
                    yield return false;
                }
                #endregion

                #region Step 12
                if (sp == 12)
                {
                    if (c1retDat.Contains("Loyalty:"))
                    {
                        o = c1retDat.Substring(c1retDat.IndexOf("Loyalty:"));
                        o = simpleTrim(o);
                        c1Parts.Add(o);
                    }
                    else
                    {
                        c1Parts.Add("");
                    }

                    done = true;
                    yield return false;
                }
                #endregion

                #region Step 13
                if (sp == 13)
                {
                    if (split)
                    {
                        if (c2retDat.Contains("Loyalty:"))
                        {
                            o = c2retDat.Substring(c2retDat.IndexOf("Loyalty:"));
                            o = simpleTrim(o);
                            c2Parts.Add(o);
                        }
                        else
                        {
                            c2Parts.Add("");
                        }
                    }

                    done = true;
                    yield return false;
                }
                #endregion

                #region Step 14
                if (sp == 14)
                {
                    if (c1retDat.Contains("Color Indicator:"))
                    {
                        o = c1retDat.Substring(c1retDat.IndexOf("Color Indicator:"));
                        o = simpleTrim(o);
                        c1Parts.Add(o);
                    }
                    else
                    {
                        c1Parts.Add("");
                    }
                    i1GID = current;
                    done = true;
                    yield return false;
                }
                #endregion

                #region Step 15
                if (sp == 15)
                {
                    if (split)
                    {
                        if (c2retDat.Contains("Color Indicator:"))
                        {
                            o = c2retDat.Substring(c2retDat.IndexOf("Color Indicator:"));
                            o = simpleTrim(o);
                            c2Parts.Add(o);
                        }
                        else
                        {
                            c2Parts.Add("");
                        }
                    }
                    done = true;
                    yield return false;
                }
                #endregion

                #region Step 16
                if (sp == 16)
                {
                    c1Rules = new List<string>();
                    c2Rules = new List<string>();
                    if (c1retDat.Contains("rulingsHeader"))
                    {
                        o = c1retDat.Substring(c1retDat.IndexOf("rulingsHeader"));
                        o = o.Substring(o.IndexOf("rulingsTable"), o.IndexOf("</table>") - o.IndexOf("rulingsTable"));
                        o = o.Substring(o.IndexOf(">") + 1);
                        o = o.Substring(0, o.LastIndexOf("</tr>"));
                        c1Rules = rulesTrim(o);
                    }

                    done = true;
                    yield return false;
                }
                #endregion

                #region Step 17
                if (sp == 17)
                {
                    if (split)
                    {
                        if (c2retDat.Contains("rulingsHeader"))
                        {
                            o = c2retDat.Substring(c2retDat.IndexOf("rulingsHeader"));
                            o = o.Substring(o.IndexOf("rulingsTable"), o.IndexOf("</table>") - o.IndexOf("rulingsTable"));
                            o = o.Substring(o.IndexOf(">") + 1);
                            o = o.Substring(0, o.LastIndexOf("</tr>"));
                            c2Rules = rulesTrim(o);
                        }
                    }

                    done = true;
                    yield return false;
                }
                #endregion

                #region Step 18
                if (sp == 18)
                {
                    unitParts = new List<string>();
                    o = c1retDat.Substring(c1retDat.IndexOf("Expansion:"));
                    o = unitTrim(o, 0);
                    o = o.Replace(">", "");
                    unitParts.Add(o);

                    done = true;
                    yield return false;
                }
                #endregion

                #region Step 19
                if (sp == 19)
                {
                    o = c1retDat.Substring(c1retDat.IndexOf("Rarity:"));
                    o = unitTrim(o, 1);
                    o = o.Substring(o.LastIndexOf(">") + 1, o.Length - (o.LastIndexOf(">") + 1));
                    unitParts.Add(o);

                    done = true;
                    yield return false;
                }
                #endregion

                #region Step 20
                if (sp == 20)
                {
                    o = c1retDat.Substring(c1retDat.IndexOf("Artist:"));
                    o = unitTrim(o, 2);
                    o = o.Replace(">", "");
                    unitParts.Add(o);

                    done = true;
                    yield return false;
                }
                #endregion
                
                #region Step 21
                if (sp == 21)
                {
                    for (int n = 0; n <= c1Parts.Count - 1; n++)
                    {
                        c1Parts[n] = symbolProcessor(c1Parts[n]);
                    }

                    done = true;
                    yield return false;
                }
                #endregion

                #region Step 22
                if (sp == 22)
                {
                    if (c2Parts != null && c2Parts.Count > 0)
                    {
                        {
                            for (int n = 0; n <= c2Parts.Count - 1; n++)
                            {
                                c2Parts[n] = symbolProcessor(c2Parts[n]);
                            }
                        }
                    }

                    done = true;
                    yield return false;
                }
                #endregion

                #region Step 23
                if (sp == 23)
                {
                    if (c1Rules != null && c1Rules.Count > 0)
                    {
                        {
                            for (int n = 0; n <= c1Rules.Count - 1; n++)
                            {
                                c1Rules[n] = symbolProcessor(c1Rules[n]);
                            }
                        }
                    }

                    done = true;
                    yield return false;
                }
                #endregion

                #region Step 24
                if (sp == 24)
                {
                    if (c2Rules != null && c2Rules.Count > 0)
                    {
                        {
                            for (int n = 0; n <= c2Rules.Count - 1; n++)
                            {
                                c2Rules[n] = symbolProcessor(c2Rules[n]);
                            }
                        }
                    }

                    done = true;
                    yield return false;
                }
                #endregion

                #region Step 25
                if (sp == 25)
                {
                    if (unitParts != null && unitParts.Count > 0)
                    {
                        {
                            for (int n = 0; n <= unitParts.Count - 1; n++)
                            {
                                unitParts[n] = symbolProcessor(unitParts[n]);
                            }
                        }
                    }

                    stage = 3;
                    step = 1;
                    running = false;
                    yield return false;
                }
                #endregion
            }
            #endregion

            #region Stage 3
            if (sg == 3)
            {
                #region Step 1
                if (sp == 1)
                {
                    vC = new VrrktCard(i1GID, split, c1Parts, c2Parts, unitParts[0], unitParts[1], unitParts[2], c1Rules, c2Rules, i2GID);
                    asyncSave(vC, pathBase + vC.GID + ".vrrktCd");
                    done = true;
                    yield return false;
                }
                #endregion

                #region Step 2
                if (sp == 2)
                {
                    if (current < upper)
                    {
                        current++;
                    }
                    else
                    {
                        pullerComplete = true;
                    }
                    split = false;

                    //if (lower == 0 || lower == 1)
                    //{
                    //    if (current == 5 || current == 10 || current == 20)
                    //    {
                    //        Debug.Log(vC.getCardInfo());
                    //    }
                    //}

                    stage = 1;
                    step = 1;
                    running = false;
                    done = false;
                    yield return false;
                }
                #endregion
            }
            #endregion

            yield return false;
        }
        #endregion

        #region Data Methods

        #region Simple Trim
        string simpleTrim(string sIN)
        {
            string o = sIN.Substring(0, sIN.IndexOf("<div id="));
            o = o.Substring(o.IndexOf("value") + 7, o.LastIndexOf("</div>") - (o.IndexOf("value") + 7));
            o = o.Substring(0, o.IndexOf("</div>"));
            o = o.Trim();
            return o;
        }
        #endregion

        #region Modified Trim
        string modifiedTrim(string sIN, bool typeOrText)
        {
            string o = "";

            if (!typeOrText)
            {
                o = sIN.Substring(0, sIN.IndexOf("<div id="));
                o = o.Substring(o.IndexOf("value") + 7, o.LastIndexOf("</div>") - (o.IndexOf("value") + 7));
                o = o.Substring(0, o.LastIndexOf("</div>"));
                o = o.Trim();
            }
            else
            {
                o = sIN.Substring(sIN.IndexOf("value") + 7);
                o = o.Substring(o.IndexOf("flavortextbox") + 15, o.IndexOf("<div id=") - (o.IndexOf("flavortextbox") + 15));
                o = o.Substring(0, o.IndexOf("</div>"));
                o = o.Trim();
            }

            //Debug.Log("o:" + o);
            return o;
        }
        #endregion

        #region Symbol Processor
        string symbolProcessor(string sIN)
        {
            MatchCollection mX = Regex.Matches(sIN, "<img src=");
            MatchCollection mY = Regex.Matches(sIN, "/>");
            List<string> s = new List<string>();

            for (int n = 0; n <= mX.Count - 1; n++)
            {
                #region Trim For Replacement

                int x = sIN.IndexOf("<img src=");
                int y = sIN.IndexOf("/>") + 2;
                if (x < 0 || y < 0 || y - x < 0)
                {
                    //Debug.Log("x < 0, sIN:" + sIN);
                    continue;
                }
                string rep = sIN.Substring(sIN.IndexOf("<img src="), (sIN.IndexOf("/>") + 2) - sIN.IndexOf("<img src="));
                string sec = rep;
                sec = sec.Substring(sec.IndexOf("type=symbol") + 11);
                sec = sec.Substring(sec.IndexOf("alt=") + 5);
                if (sec.IndexOf("align=") - 2 <= 0)
                {
                    //Debug.Log("align < 0 sec:" + sec);
                    continue;
                }
                string post = sec.Substring(sec.IndexOf("align=") - 2, sec.IndexOf("/>") - (sec.IndexOf("align=") - 2));
                sec = sec.Replace(post, "");

                #endregion

                #region Convert

                #region Attempt Last
                foreach (KeyValuePair<string, string> k in lastDict)
                {
                    if (sec.Contains(k.Key))
                    {
                        sec = sec.Replace(k.Key, k.Value);
                    }
                }
                #endregion

                #region Attempt Type
                foreach (KeyValuePair<string, string> k in typeDict)
                {
                    if (sec.Contains(k.Key))
                    {
                        sec = sec.Replace(k.Key, k.Value);
                    }
                }
                #endregion

                #region Attempt Color
                foreach (KeyValuePair<string, string> k in colorDict)
                {
                    if (sec.Contains(k.Key))
                    {
                        sec = sec.Replace(k.Key, k.Value);
                    }
                }
                #endregion

                #region Attempt Int.Parse

                int j = -1;

                if (int.TryParse(sec, out j))
                {
                    sec = j + "";
                }
                #endregion

                #region Reformat For Replacement
                sec = "[" + sec + "]";
                sec = sec.Replace(" ", "");
                #endregion

                #endregion

                #region Replace
                if (sec.Contains("/>"))
                {
                    sec = sec.Replace("/>", "");
                }

                sIN = sIN.Replace(rep, sec);
                #endregion
            }
            sIN = sIN.Trim();

            //Debug.Log(sIN);

            return sIN;
        }
        #endregion

        #region Card Text Trim
        string cardTextTrim(string sIN)
        {
            string o = "";
            List<string> s = new List<string>();
            sIN = sIN.Substring(0, sIN.LastIndexOf("</div>"));
            MatchCollection m = Regex.Matches(sIN, "<div class=");
            for (int n = 0; n <= m.Count - 1; n++)
            {
                o = sIN.Substring(m[n].Index);
                o = o.Substring(0, o.IndexOf("</div>"));
                o = o.Substring(o.IndexOf(">") + 1);
                s.Add(o);
            }

            for (int n = 0; n <= s.Count - 1; n++)
            {
                o = symbolProcessor(s[n]);
                o = o.Replace('"', '^');
                o = o.Replace("^", "");
                s[n] = o;
            }

            o = "";

            for (int n = 0; n <= s.Count - 1; n++)
            {
                if (n < s.Count - 1)
                {
                    o += s[n] + "\n";
                }
                else
                {
                    o += s[n];
                }
            }

            return o;
        }
        #endregion

        #region Unit Trim
        string unitTrim(string sIN, int unitIDX)
        {
            string o = "";

            if (unitIDX == 0)
            {
                o = sIN.Substring(sIN.IndexOf("<div id") + 10);
                o = o.Substring(o.IndexOf("</a>") + 4, o.IndexOf("<div id") - (o.IndexOf("</a>") + 4));
                if (o.IndexOf("</a>") - (o.IndexOf("<a href=") + 8) > 0)
                {
                    o = o.Substring(o.IndexOf("<a href=") + 8, o.IndexOf("</a>") - (o.IndexOf("<a href=") + 8));
                    o = o.Substring(o.IndexOf(">"));
                }
                o.Trim();
            }
            else if (unitIDX == 1)
            {
                o = sIN.Substring(0, sIN.IndexOf("<div id="));
                if (o.IndexOf("</span>") - (o.IndexOf("class") + 6) > 0)
                {
                    o = o.Substring(o.IndexOf("class=") + 6, o.IndexOf("</span>") - (o.IndexOf("class") + 6));
                    o = o.Substring(o.IndexOf(">"));
                }
                o.Trim();
            }
            else if (unitIDX == 2)
            {
                o = sIN.Substring(sIN.IndexOf("value") + 7);
                o = o.Substring(o.IndexOf("<a href=") + 8, o.IndexOf("</a>") - (o.IndexOf("<a href=") + 8));
                o = o.Substring(o.IndexOf(">"));
                o.Trim();
            }

            return o;
        }
        #endregion

        #region Rules Trim
        List<string> rulesTrim(string sIN)
        {
            List<int> i = new List<int>();
            List<string> s = new List<string>();
            string oS = "";
            MatchCollection m = Regex.Matches(sIN, "rulingsText");
            for (int n = 0; n <= m.Count - 1; n++)
            {
                string o = sIN.Substring(m[n].Index + 13);
                o = o.Substring(0, o.IndexOf("</td>"));
                o = o.Replace('"', '^');
                o = o.Replace("^", "");
                s.Add(o);
                oS += o + "\n";
            }

            for (int n = 0; n <= s.Count - 1; n++)
            {
                s[n] = symbolProcessor(s[n]);
                oS += s[n] + "\n";
            }

            return s;
        }
        #endregion

        #endregion

        #endregion

        #region Image Cycle Control
        public IEnumerator combinedENUMImage(vrrktEditorVersion2 e)
        {
            #region Startup Operations
            ed = e;
            repainter();
            asyncing = false;
            asynced = false;
            UnityWebRequest uWeb;
            #endregion

            #region Main Cycle
            while (current <= upper)
            {
                #region Step 1 [Begin Operations]
                while (step == 1)
                {
                    vC = null;

                    if (!File.Exists(pathBase + current + ".vrrktCi"))
                    {
                        if (File.Exists(pathBase + current + ".vrrktCd"))
                        {
                            //Debug.Log("Finished Async step " + step);
                            step++;
                        }
                        else
                        {
                            //Debug.Log("Card Fail Async step " + step);
                            //step = 10;
                            //current = upper + 5;
                            current++;
                        }
                    }
                    else
                    {
                        //Debug.Log("Image Fail Async step " + step);
                        //step = 10;
                        //current = upper + 5;
                        current++;
                    }

                    yield return true;
                }
                #endregion

                #region Step 2
                while (step == 2)
                {
                    vC = new VrrktCard();
                    f = File.OpenRead(pathBase + current + ".vrrktCd");
                    vC.readFromStream(f);
                    f.Close();
                    f.Dispose();

                    if (vC != null)
                    {
                        i1GID = vC.GID;

                        if (vC.i2GID > 0)
                        {
                            i2GID = vC.i2GID;
                        }
                        else
                        {
                            i2GID = -1;
                        }

                        //Debug.Log("Finished Async step " + step);
                        img1 = null;
                        img2 = null;
                        vCI = null;
                        step++;
                    }
                    else
                    {
                        Debug.Log("Card is null in async step 2");
                        step = 10;
                        current = upper + 5;
                    }

                    yield return true;
                }
                #endregion

                #region Step 3
                while (step == 3 && img1 == null)
                {
                    //Debug.Log("Began getting image 1");
                    uWeb = new UnityWebRequest();
                    using (uWeb = UnityWebRequestTexture.GetTexture(imgBase + current + imgEnd))
                    {
                        uWeb.SendWebRequest();

                        while (!uWeb.isDone)
                        {
                            //Debug.Log("Waiting on img 1");
                            yield return true;
                        }

                        if (uWeb.isHttpError || uWeb.isNetworkError)
                        {
                            Debug.Log("Image 1 Error for #" + current);
                            step = 10;
                            current = upper + 5;
                            yield return false;
                        }
                        else
                        {
                            img1 = DownloadHandlerTexture.GetContent(uWeb);
                            //Debug.Log("Image 1 done");
                            step++;
                            yield return true;
                        }
                    }
                    
                    //Debug.Log("Finished ENUM step " + step);
                    yield return true;
                }
                #endregion

                #region Step 4
                while (step == 4)
                {
                    if (i2GID > 0)
                    {
                        uWeb = new UnityWebRequest();
                        using (uWeb = UnityWebRequestTexture.GetTexture(imgBase + i2GID + imgEnd))
                        {
                            uWeb.SendWebRequest();

                            while (!uWeb.isDone)
                            {
                                yield return true;
                            }

                            if (uWeb.isHttpError || uWeb.isNetworkError)
                            {
                                Debug.Log("Image 2 Error for #" + current);
                                step = 10;
                                current = upper + 5;
                                yield return false;
                            }
                            else
                            {
                                img2 = DownloadHandlerTexture.GetContent(uWeb);
                                step++;
                                //Debug.Log("Finished ENUM step " + step);
                                yield return true;
                            }
                        }
                    }
                    else
                    {
                        step++;
                    }
                    
                    yield return true;
                }

                #endregion

                #region Step 5
                while (step == 5)
                {
                    if (i2GID <= 0)
                    {
                        vCI = new VrrktCardImage(img1, img1.width, img1.height, i1GID);
                    }
                    else
                    {
                        vCI = new VrrktCardImage(img1, img1.width, img1.height, i1GID, img2, img2.width, img2.height, i2GID);
                    }

                    //Debug.Log("Finished Async step " + step);
                    step++;

                    yield return true;
                }
                #endregion

                #region Step 6
                while (step == 6)
                {
                    asyncSave(vCI, pathBase + current + ".vrrktCi");
                    //Debug.Log("Finished ENUM step " + step);
                    //step = 10;
                    //current = upper + 5;
                    step = 1;
                    current++;
                    yield return true;
                }
                #endregion

                yield return true;
            }
            #endregion

            Debug.Log("Finished ENUM");
            yield return false;
        }
        #endregion

        #endregion

        #region Async Verification Control Methods

        #region Card Integrity Check
        public IEnumerator cardIntegrity(vrrktEditorVersion2 e)
        {
            #region Startup Operations
            UnityWebRequest uweb = null;
            ed = e;
            repainter();
            step = 1;
            repulled = 0;
            int stg = 1;
            int stp = 1;
            bool integrity = false;
            #endregion

            while (current <= upper)
            {
                #region 1
                while (step == 1)
                {
                    vC = null;
                    repullDone = false;

                    if (File.Exists(pathBase + current + ".vrrktCd"))
                    {
                        step++;
                    }
                    else
                    {
                        step = 1;
                        current++;
                    }

                    yield return true;
                }
                #endregion

                #region 2
                while (step == 2)
                {
                    f = File.OpenRead(pathBase + current + ".vrrktCd");
                    vC = new VrrktCard();
                    integrity = vC.tryReconstruct(f);
                    step++;
                    yield return true;
                }
                #endregion

                #region 3
                while (step == 3)
                {
                    if (f != null)
                    {
                        f.Close();
                    }

                    repullDone = false;

                    if (integrity)
                    {
                        step = 1;
                        current++;
                    }
                    else
                    {
                        step++;
                    }
                    yield return true;
                }
                #endregion

                #region 4
                while (step == 4)
                {
                    #region Mini Repull
                    while (!repullDone)
                    {
                        #region Stage 1 [1-3]

                        #region Step 1
                        while (stg == 1 && stp == 1)
                        {
                            uweb = new UnityWebRequest();
                            using (uweb = UnityWebRequest.Get(dataBase + current))
                            {
                                uweb.SendWebRequest();

                                while (!uweb.isDone)
                                {
                                    yield return true;
                                }

                                if (uweb.isHttpError || uweb.isNetworkError)
                                {
                                    Debug.Log("Error occured on HTML GET");
                                    step = 1;
                                    stg = 1;
                                    yield return false;
                                }
                                else
                                {
                                    retDat = uweb.downloadHandler.text;
                                    step++;
                                    stp++;
                                    yield return true;
                                }
                            }
                        }
                        #endregion

                        #region Step 2
                        while (stg == 1 && stp == 2)
                        {
                            MatchCollection mC = Regex.Matches(retDat, "Card Name");

                            if (mC.Count == 2)
                            {
                                split = true;
                                stp++;
                            }
                            else
                            {
                                if (mC.Count == 1)
                                {
                                    split = false;
                                    stp++;
                                }
                            }
                            yield return true;
                        }
                        #endregion

                        #region Step 3
                        while (stg == 1 && stp == 3)
                        {
                            c1retDat = retDat.Substring(retDat.IndexOf("Card Name"));

                            if (split)
                            {
                                c1retDat = c1retDat.Substring(0, c1retDat.LastIndexOf("Card Name"));
                                c2retDat = retDat.Substring(retDat.LastIndexOf("Card Name"), retDat.Length - retDat.LastIndexOf("Card Name"));

                                o = retDat.Substring(retDat.LastIndexOf(".ashx?multiverseid=") + 19);
                                if (o.IndexOf("&type=card") < 0)
                                {
                                    if (o.IndexOf("&amp;type=card") >= 0)
                                    {
                                        o = o.Substring(0, o.IndexOf("&amp;type=card"));
                                    }
                                }
                                else
                                {
                                    o = o.Substring(0, o.IndexOf("&type=card"));
                                }

                                i = -1;
                                int.TryParse(o, out i);
                                if (i != -1)
                                {
                                    i2GID = i;
                                }
                                else
                                {
                                    i2GID = -1;
                                    Debug.Log("Unable to parse card 2 image GID");
                                }
                            }
                            else
                            {
                                i2GID = -1;
                            }
                            i1GID = current;
                            retDat = "";
                            stg = 2;
                            stp = 1;
                            yield return true;
                        }
                        #endregion

                        #endregion

                        #region Stage 2 [1-13]

                        #region Step 1
                        while (stg == 2 && stp == 1)
                        {
                            c1Parts = new List<string>();
                            c2Parts = new List<string>();

                            o = c1retDat.Substring(c1retDat.IndexOf("Card Name:"));
                            o = simpleTrim(o);
                            c1Parts.Add(o);

                            if (split)
                            {
                                o = c2retDat.Substring(c2retDat.IndexOf("Card Name:"));
                                o = simpleTrim(o);
                                c2Parts.Add(o);
                            }
                            stp++;
                            yield return true;
                        }
                        #endregion

                        #region Step 2
                        while (stg == 2 && stp == 2)
                        {
                            if (c1retDat.Contains("Mana Cost:"))
                            {
                                o = c1retDat.Substring(c1retDat.IndexOf("Mana Cost:"));
                                o = o.Substring(0, o.IndexOf("<div id="));
                                o = o.Substring(o.IndexOf("value") + 7, o.LastIndexOf("</div>") - (o.IndexOf("value") + 7));
                                o = o.Substring(0, o.LastIndexOf("</div>"));
                                o = symbolProcessor(o);
                                c1Parts.Add(o);
                            }
                            else
                            {
                                c1Parts.Add("");
                            }

                            if (split)
                            {
                                if (c2retDat.Contains("Mana Cost:"))
                                {
                                    o = c2retDat.Substring(c2retDat.IndexOf("Mana Cost:"));
                                    o = o.Substring(0, o.IndexOf("<div id="));
                                    o = o.Substring(o.IndexOf("value") + 7, o.LastIndexOf("</div>") - (o.IndexOf("value") + 7));
                                    o = o.Substring(0, o.LastIndexOf("</div>"));
                                    o = symbolProcessor(o);
                                    c2Parts.Add(o);
                                }
                                else
                                {
                                    c2Parts.Add("");
                                }
                            }
                            stp++;
                            yield return true;
                        }
                        #endregion

                        #region Step 3
                        while (stg == 2 && stp == 3)
                        {
                            if (c1retDat.Contains("Converted Mana Cost:"))
                            {
                                o = c1retDat.Substring(c1retDat.IndexOf("Converted Mana Cost:"));
                                o = simpleTrim(o);
                                c1Parts.Add(o);
                            }
                            else
                            {
                                c1Parts.Add("");
                            }

                            if (split)
                            {
                                if (c2retDat.Contains("Converted Mana Cost:"))
                                {
                                    o = c2retDat.Substring(c2retDat.IndexOf("Converted Mana Cost:"));
                                    o = simpleTrim(o);
                                    c2Parts.Add(o);
                                }
                                else
                                {
                                    c2Parts.Add("");
                                }
                            }
                            stp++;
                            yield return true;
                        }
                        #endregion

                        #region Step 4
                        while (stg == 2 && stp == 4)
                        {
                            if (c1retDat.Contains("Types:"))
                            {
                                o = c1retDat.Substring(c1retDat.IndexOf("Types:"));
                                o = modifiedTrim(o, false);
                                c1Parts.Add(o);
                            }
                            else
                            {
                                c1Parts.Add("");
                            }

                            if (split)
                            {
                                if (c2retDat.Contains("Types:"))
                                {
                                    o = c2retDat.Substring(c2retDat.IndexOf("Types:"));
                                    o = modifiedTrim(o, false);
                                    c2Parts.Add(o);
                                }
                                else
                                {
                                    c2Parts.Add("");
                                }
                            }
                            stp++;
                            yield return true;
                        }
                        #endregion

                        #region Step 5
                        while (stg == 2 && stp == 5)
                        {
                            if (c1retDat.Contains("Card Text:"))
                            {
                                o = c1retDat.Substring(c1retDat.IndexOf("Card Text:"));
                                o = o.Substring(0, o.IndexOf("<div id="));
                                o = o.Substring(o.IndexOf("value") + 7, o.LastIndexOf("</div>") - (o.IndexOf("value") + 7));
                                o = cardTextTrim(o);
                                c1Parts.Add(o);
                            }
                            else
                            {
                                c1Parts.Add("");
                            }

                            if (split)
                            {
                                if (c2retDat.Contains("Card Text:"))
                                {
                                    o = c2retDat.Substring(c2retDat.IndexOf("Card Text:"));
                                    o = o.Substring(0, o.IndexOf("<div id="));
                                    o = o.Substring(o.IndexOf("value") + 7, o.LastIndexOf("</div>") - (o.IndexOf("value") + 7));
                                    o = cardTextTrim(o);
                                    c2Parts.Add(o);
                                }
                                else
                                {
                                    c2Parts.Add("");
                                }
                            }
                            stp++;
                            yield return true;
                        }
                        #endregion

                        #region Step 6
                        while (stg == 2 && stp == 6)
                        {
                            if (c1retDat.Contains("Flavor Text:"))
                            {
                                o = c1retDat.Substring(c1retDat.IndexOf("Flavor Text:"));
                                o = modifiedTrim(o, true);
                                c1Parts.Add(o);
                            }
                            else
                            {
                                c1Parts.Add("");
                            }

                            if (split)
                            {
                                if (c2retDat.Contains("Flavor Text:"))
                                {
                                    o = c2retDat.Substring(c2retDat.IndexOf("Flavor Text:"));
                                    o = modifiedTrim(o, true);
                                    c2Parts.Add(o);
                                }
                                else
                                {
                                    c2Parts.Add("");
                                }
                            }
                            stp++;
                            yield return true;
                        }
                        #endregion

                        #region Step 7
                        while (stg == 2 && stp == 7)
                        {
                            if (c1retDat.Contains("P/T:"))
                            {
                                o = c1retDat.Substring(c1retDat.IndexOf("P/T:"));
                                o = simpleTrim(o);
                                c1Parts.Add(o);
                            }
                            else
                            {
                                c1Parts.Add("");
                            }

                            if (split)
                            {
                                if (c2retDat.Contains("P/T:"))
                                {
                                    o = c2retDat.Substring(c2retDat.IndexOf("P/T:"));
                                    o = simpleTrim(o);
                                    c2Parts.Add(o);
                                }
                                else
                                {
                                    c2Parts.Add("");
                                }
                            }
                            stp++;
                            yield return true;
                        }
                        #endregion

                        #region Step 8
                        while (stg == 2 && stp == 8)
                        {
                            if (c1retDat.Contains("Loyalty:"))
                            {
                                o = c1retDat.Substring(c1retDat.IndexOf("Loyalty:"));
                                o = simpleTrim(o);
                                c1Parts.Add(o);
                            }
                            else
                            {
                                c1Parts.Add("");
                            }

                            if (split)
                            {
                                if (c2retDat.Contains("Loyalty:"))
                                {
                                    o = c2retDat.Substring(c2retDat.IndexOf("Loyalty:"));
                                    o = simpleTrim(o);
                                    c2Parts.Add(o);
                                }
                                else
                                {
                                    c2Parts.Add("");
                                }
                            }
                            stp++;
                            yield return true;
                        }
                        #endregion

                        #region Step 9
                        while (stg == 2 && stp == 9)
                        {
                            if (c1retDat.Contains("Color Indicator:"))
                            {
                                o = c1retDat.Substring(c1retDat.IndexOf("Color Indicator:"));
                                o = simpleTrim(o);
                                c1Parts.Add(o);
                            }
                            else
                            {
                                c1Parts.Add("");
                            }
                            i1GID = current;

                            if (split)
                            {
                                if (c2retDat.Contains("Color Indicator:"))
                                {
                                    o = c2retDat.Substring(c2retDat.IndexOf("Color Indicator:"));
                                    o = simpleTrim(o);
                                    c2Parts.Add(o);
                                }
                                else
                                {
                                    c2Parts.Add("");
                                }
                            }
                            stp++;
                            yield return true;
                        }
                        #endregion

                        #region Step 10
                        while (stg == 2 && stp == 10)
                        {
                            c1Rules = new List<string>();
                            c2Rules = new List<string>();
                            if (c1retDat.Contains("rulingsHeader"))
                            {
                                o = c1retDat.Substring(c1retDat.IndexOf("rulingsHeader"));
                                o = o.Substring(o.IndexOf("rulingsTable"), o.IndexOf("</table>") - o.IndexOf("rulingsTable"));
                                o = o.Substring(o.IndexOf(">") + 1);
                                o = o.Substring(0, o.LastIndexOf("</tr>"));
                                c1Rules = rulesTrim(o);
                            }

                            if (split)
                            {
                                if (c2retDat.Contains("rulingsHeader"))
                                {
                                    o = c2retDat.Substring(c2retDat.IndexOf("rulingsHeader"));
                                    o = o.Substring(o.IndexOf("rulingsTable"), o.IndexOf("</table>") - o.IndexOf("rulingsTable"));
                                    o = o.Substring(o.IndexOf(">") + 1);
                                    o = o.Substring(0, o.LastIndexOf("</tr>"));
                                    c2Rules = rulesTrim(o);
                                }
                            }
                            stp++;
                            yield return true;
                        }
                        #endregion

                        #region Step 11
                        while (stg == 2 && stp == 11)
                        {
                            unitParts = new List<string>();
                            o = c1retDat.Substring(c1retDat.IndexOf("Expansion:"));
                            o = unitTrim(o, 0);
                            o = o.Replace(">", "");
                            unitParts.Add(o);

                            stp++;
                            yield return true;
                        }
                        #endregion

                        #region Step 12
                        while (stg == 2 && stp == 12)
                        {
                            o = c1retDat.Substring(c1retDat.IndexOf("Rarity:"));
                            o = unitTrim(o, 1);
                            o = o.Substring(o.LastIndexOf(">") + 1, o.Length - (o.LastIndexOf(">") + 1));
                            unitParts.Add(o);

                            stp++;
                            yield return true;
                        }
                        #endregion

                        #region Step 13
                        while (stg == 2 && stp == 13)
                        {
                            o = c1retDat.Substring(c1retDat.IndexOf("Artist:"));
                            o = unitTrim(o, 2);
                            o = o.Replace(">", "");
                            unitParts.Add(o);

                            stg = 3;
                            stp = 1;
                            yield return true;
                        }
                        #endregion

                        #region Step 14
                        while (stg == 2 && stp == 14)
                        {
                            for (int n = 0; n <= c1Parts.Count - 1; n++)
                            {
                                c1Parts[n] = symbolProcessor(c1Parts[n]);
                            }

                            if (c2Parts != null && c2Parts.Count > 0)
                            {
                                {
                                    for (int n = 0; n <= c2Parts.Count - 1; n++)
                                    {
                                        c2Parts[n] = symbolProcessor(c2Parts[n]);
                                    }
                                }
                            }

                            if (c1Rules != null && c1Rules.Count > 0)
                            {
                                {
                                    for (int n = 0; n <= c1Rules.Count - 1; n++)
                                    {
                                        c1Rules[n] = symbolProcessor(c1Rules[n]);
                                    }
                                }
                            }

                            if (c2Rules != null && c2Rules.Count > 0)
                            {
                                {
                                    for (int n = 0; n <= c2Rules.Count - 1; n++)
                                    {
                                        c2Rules[n] = symbolProcessor(c2Rules[n]);
                                    }
                                }
                            }

                            if (unitParts != null && unitParts.Count > 0)
                            {
                                {
                                    for (int n = 0; n <= unitParts.Count - 1; n++)
                                    {
                                        unitParts[n] = symbolProcessor(unitParts[n]);
                                    }
                                }
                            }

                            stage = 3;
                            step = 1;

                            yield return true;
                        }
                        #endregion

                        #endregion

                        #region Stage 3 [1]

                        #region Stage 3 Step 1
                        while (stg == 3 && stp == 1)
                        {
                            vC = new VrrktCard(i1GID, split, c1Parts, c2Parts, unitParts[0], unitParts[1], unitParts[2], c1Rules, c2Rules, i2GID);
                            repullDone = true;
                            yield return true;
                        }
                        #endregion

                        #endregion

                        yield return true;
                    }

                    repulled++;
                    stg = 1;
                    stp = 1;
                    repullDone = false;
                    step++;
                    #endregion

                    yield return true;
                }
                #endregion

                #region 5
                while (step == 5)
                {
                    asyncSave(vC, pathBase + vC.GID + ".vrrktCd");
                    step++;
                    yield return true;
                }
                #endregion

                #region 6
                while (step == 6)
                {
                    current++;
                    step = 1;
                    yield return true;
                }
                #endregion

                yield return true;
            }

            //Debug.Log("Finished Card Integrity Check");
            yield return false;
        }
        #endregion

        #region Async Card Integrity
        public async void integrityCheckControl(vrrktEditorVersion2 e)
        {
            #region Startup Operations
            ed = e;
            repainter();
            step = 1;
            repulled = 0;
            int stg = 1;
            int stp = 1;
            FileStream f = null;
            bool integrity = false;
            await Task.Delay(50);
            #endregion

            while (current <= upper)
            {
                for (int n = 0; n <= 9; n++)
                {
                    if (current <= upper)
                    {
                        await Task.Run(async () =>
                        {
                            #region 1
                            if (step == 1)
                            {
                                vC = null;
                                repullDone = false;

                                if (File.Exists(pathBase + current + ".vrrktCd"))
                                {
                                    step++;
                                }
                                else
                                {
                                    current++;
                                }
                            }
                            #endregion

                            #region 2
                            if (step == 2)
                            {
                                f = File.OpenRead(pathBase + current + ".vrrktCd");
                                vC = new VrrktCard();
                                integrity = vC.tryReconstruct(f);
                                step++;
                            }
                            #endregion

                            #region 3
                            if (step == 3)
                            {
                                if (f != null)
                                {
                                    f.Close();
                                }

                                repullDone = false;

                                if (integrity)
                                {
                                    step = 1;
                                    current++;
                                }
                                else
                                {
                                    step++;
                                }
                            }
                            #endregion

                            #region 4
                            if (step == 4)
                            {
                                #region Mini Repull
                                while (!repullDone)
                                {
                                    #region Stage 1 [1-3]

                                    #region Step 1
                                    if (stg == 1 && stp == 1)
                                    {
                                        while (retDat == "")
                                        {
                                            try
                                            {
                                                vHTTP = new VrrktHttp();
                                                retDat = await vHTTP.getURLText(dataBase + current);
                                            }
                                            catch (Exception em)
                                            {
                                                step = 1;
                                                stage = 1;
                                            }

                                            step++;
                                        }
                                    }
                                    #endregion

                                    #region Step 2
                                    if (stg == 1 && stp == 2)
                                    {
                                        MatchCollection mC = Regex.Matches(retDat, "Card Name");

                                        if (mC.Count == 2)
                                        {
                                            split = true;
                                            stp++;
                                        }
                                        else
                                        {
                                            if (mC.Count == 1)
                                            {
                                                split = false;
                                                stp++;
                                            }
                                        }
                                    }
                                    #endregion

                                    #region Step 3
                                    if (stg == 1 && stp == 3)
                                    {
                                        c1retDat = retDat.Substring(retDat.IndexOf("Card Name"));

                                        if (split)
                                        {
                                            c1retDat = c1retDat.Substring(0, c1retDat.LastIndexOf("Card Name"));
                                            c2retDat = retDat.Substring(retDat.LastIndexOf("Card Name"), retDat.Length - retDat.LastIndexOf("Card Name"));

                                            o = retDat.Substring(retDat.LastIndexOf(".ashx?multiverseid=") + 19);
                                            if (o.IndexOf("&type=card") < 0)
                                            {
                                                if (o.IndexOf("&amp;type=card") >= 0)
                                                {
                                                    o = o.Substring(0, o.IndexOf("&amp;type=card"));
                                                }
                                            }
                                            else
                                            {
                                                o = o.Substring(0, o.IndexOf("&type=card"));
                                            }

                                            i = -1;
                                            int.TryParse(o, out i);
                                            if (i != -1)
                                            {
                                                i2GID = i;
                                            }
                                            else
                                            {
                                                i2GID = -1;
                                                Debug.Log("Unable to parse card 2 image GID");
                                            }
                                        }
                                        else
                                        {
                                            i2GID = -1;
                                        }
                                        i1GID = current;
                                        retDat = "";
                                        stg = 2;
                                        stp = 1;
                                    }
                                    #endregion

                                    #endregion

                                    #region Stage 2 [1-13]

                                    #region Step 1
                                    if (stg == 2 && stp == 1)
                                    {
                                        c1Parts = new List<string>();
                                        c2Parts = new List<string>();

                                        o = c1retDat.Substring(c1retDat.IndexOf("Card Name:"));
                                        o = simpleTrim(o);
                                        c1Parts.Add(o);

                                        if (split)
                                        {
                                            o = c2retDat.Substring(c2retDat.IndexOf("Card Name:"));
                                            o = simpleTrim(o);
                                            c2Parts.Add(o);
                                        }
                                        stp++;
                                    }
                                    #endregion

                                    #region Step 2
                                    if (stg == 2 && stp == 2)
                                    {
                                        if (c1retDat.Contains("Mana Cost:"))
                                        {
                                            o = c1retDat.Substring(c1retDat.IndexOf("Mana Cost:"));
                                            o = o.Substring(0, o.IndexOf("<div id="));
                                            o = o.Substring(o.IndexOf("value") + 7, o.LastIndexOf("</div>") - (o.IndexOf("value") + 7));
                                            o = o.Substring(0, o.LastIndexOf("</div>"));
                                            o = symbolProcessor(o);
                                            c1Parts.Add(o);
                                        }
                                        else
                                        {
                                            c1Parts.Add("");
                                        }

                                        if (split)
                                        {
                                            if (c2retDat.Contains("Mana Cost:"))
                                            {
                                                o = c2retDat.Substring(c2retDat.IndexOf("Mana Cost:"));
                                                o = o.Substring(0, o.IndexOf("<div id="));
                                                o = o.Substring(o.IndexOf("value") + 7, o.LastIndexOf("</div>") - (o.IndexOf("value") + 7));
                                                o = o.Substring(0, o.LastIndexOf("</div>"));
                                                o = symbolProcessor(o);
                                                c2Parts.Add(o);
                                            }
                                            else
                                            {
                                                c2Parts.Add("");
                                            }
                                        }
                                        stp++;
                                    }
                                    #endregion

                                    #region Step 3
                                    if (stg == 2 && stp == 3)
                                    {
                                        if (c1retDat.Contains("Converted Mana Cost:"))
                                        {
                                            o = c1retDat.Substring(c1retDat.IndexOf("Converted Mana Cost:"));
                                            o = simpleTrim(o);
                                            c1Parts.Add(o);
                                        }
                                        else
                                        {
                                            c1Parts.Add("");
                                        }

                                        if (split)
                                        {
                                            if (c2retDat.Contains("Converted Mana Cost:"))
                                            {
                                                o = c2retDat.Substring(c2retDat.IndexOf("Converted Mana Cost:"));
                                                o = simpleTrim(o);
                                                c2Parts.Add(o);
                                            }
                                            else
                                            {
                                                c2Parts.Add("");
                                            }
                                        }
                                        stp++;
                                    }
                                    #endregion

                                    #region Step 4
                                    if (stg == 2 && stp == 4)
                                    {
                                        if (c1retDat.Contains("Types:"))
                                        {
                                            o = c1retDat.Substring(c1retDat.IndexOf("Types:"));
                                            o = modifiedTrim(o, false);
                                            c1Parts.Add(o);
                                        }
                                        else
                                        {
                                            c1Parts.Add("");
                                        }

                                        if (split)
                                        {
                                            if (c2retDat.Contains("Types:"))
                                            {
                                                o = c2retDat.Substring(c2retDat.IndexOf("Types:"));
                                                o = modifiedTrim(o, false);
                                                c2Parts.Add(o);
                                            }
                                            else
                                            {
                                                c2Parts.Add("");
                                            }
                                        }
                                        stp++;
                                    }
                                    #endregion

                                    #region Step 5
                                    if (stg == 2 && stp == 5)
                                    {
                                        if (c1retDat.Contains("Card Text:"))
                                        {
                                            o = c1retDat.Substring(c1retDat.IndexOf("Card Text:"));
                                            o = o.Substring(0, o.IndexOf("<div id="));
                                            o = o.Substring(o.IndexOf("value") + 7, o.LastIndexOf("</div>") - (o.IndexOf("value") + 7));
                                            o = cardTextTrim(o);
                                            c1Parts.Add(o);
                                        }
                                        else
                                        {
                                            c1Parts.Add("");
                                        }

                                        if (split)
                                        {
                                            if (c2retDat.Contains("Card Text:"))
                                            {
                                                o = c2retDat.Substring(c2retDat.IndexOf("Card Text:"));
                                                o = o.Substring(0, o.IndexOf("<div id="));
                                                o = o.Substring(o.IndexOf("value") + 7, o.LastIndexOf("</div>") - (o.IndexOf("value") + 7));
                                                o = cardTextTrim(o);
                                                c2Parts.Add(o);
                                            }
                                            else
                                            {
                                                c2Parts.Add("");
                                            }
                                        }
                                        stp++;
                                    }
                                    #endregion

                                    #region Step 6
                                    if (stg == 2 && stp == 6)
                                    {
                                        if (c1retDat.Contains("Flavor Text:"))
                                        {
                                            o = c1retDat.Substring(c1retDat.IndexOf("Flavor Text:"));
                                            o = modifiedTrim(o, true);
                                            c1Parts.Add(o);
                                        }
                                        else
                                        {
                                            c1Parts.Add("");
                                        }

                                        if (split)
                                        {
                                            if (c2retDat.Contains("Flavor Text:"))
                                            {
                                                o = c2retDat.Substring(c2retDat.IndexOf("Flavor Text:"));
                                                o = modifiedTrim(o, true);
                                                c2Parts.Add(o);
                                            }
                                            else
                                            {
                                                c2Parts.Add("");
                                            }
                                        }
                                        stp++;
                                    }
                                    #endregion

                                    #region Step 7
                                    if (stg == 2 && stp == 7)
                                    {
                                        if (c1retDat.Contains("P/T:"))
                                        {
                                            o = c1retDat.Substring(c1retDat.IndexOf("P/T:"));
                                            o = simpleTrim(o);
                                            c1Parts.Add(o);
                                        }
                                        else
                                        {
                                            c1Parts.Add("");
                                        }

                                        if (split)
                                        {
                                            if (c2retDat.Contains("P/T:"))
                                            {
                                                o = c2retDat.Substring(c2retDat.IndexOf("P/T:"));
                                                o = simpleTrim(o);
                                                c2Parts.Add(o);
                                            }
                                            else
                                            {
                                                c2Parts.Add("");
                                            }
                                        }
                                        stp++;
                                    }
                                    #endregion

                                    #region Step 8
                                    if (stg == 2 && stp == 8)
                                    {
                                        if (c1retDat.Contains("Loyalty:"))
                                        {
                                            o = c1retDat.Substring(c1retDat.IndexOf("Loyalty:"));
                                            o = simpleTrim(o);
                                            c1Parts.Add(o);
                                        }
                                        else
                                        {
                                            c1Parts.Add("");
                                        }

                                        if (split)
                                        {
                                            if (c2retDat.Contains("Loyalty:"))
                                            {
                                                o = c2retDat.Substring(c2retDat.IndexOf("Loyalty:"));
                                                o = simpleTrim(o);
                                                c2Parts.Add(o);
                                            }
                                            else
                                            {
                                                c2Parts.Add("");
                                            }
                                        }
                                        stp++;
                                    }
                                    #endregion

                                    #region Step 9
                                    if (stg == 2 && stp == 9)
                                    {
                                        if (c1retDat.Contains("Color Indicator:"))
                                        {
                                            o = c1retDat.Substring(c1retDat.IndexOf("Color Indicator:"));
                                            o = simpleTrim(o);
                                            c1Parts.Add(o);
                                        }
                                        else
                                        {
                                            c1Parts.Add("");
                                        }
                                        i1GID = current;

                                        if (split)
                                        {
                                            if (c2retDat.Contains("Color Indicator:"))
                                            {
                                                o = c2retDat.Substring(c2retDat.IndexOf("Color Indicator:"));
                                                o = simpleTrim(o);
                                                c2Parts.Add(o);
                                            }
                                            else
                                            {
                                                c2Parts.Add("");
                                            }
                                        }
                                        stp++;
                                    }
                                    #endregion

                                    #region Step 10
                                    if (stg == 2 && stp == 10)
                                    {
                                        c1Rules = new List<string>();
                                        c2Rules = new List<string>();
                                        if (c1retDat.Contains("rulingsHeader"))
                                        {
                                            o = c1retDat.Substring(c1retDat.IndexOf("rulingsHeader"));
                                            o = o.Substring(o.IndexOf("rulingsTable"), o.IndexOf("</table>") - o.IndexOf("rulingsTable"));
                                            o = o.Substring(o.IndexOf(">") + 1);
                                            o = o.Substring(0, o.LastIndexOf("</tr>"));
                                            c1Rules = rulesTrim(o);
                                        }

                                        if (split)
                                        {
                                            if (c2retDat.Contains("rulingsHeader"))
                                            {
                                                o = c2retDat.Substring(c2retDat.IndexOf("rulingsHeader"));
                                                o = o.Substring(o.IndexOf("rulingsTable"), o.IndexOf("</table>") - o.IndexOf("rulingsTable"));
                                                o = o.Substring(o.IndexOf(">") + 1);
                                                o = o.Substring(0, o.LastIndexOf("</tr>"));
                                                c2Rules = rulesTrim(o);
                                            }
                                        }
                                        stp++;
                                    }
                                    #endregion

                                    #region Step 11
                                    if (stg == 2 && stp == 11)
                                    {
                                        unitParts = new List<string>();
                                        o = c1retDat.Substring(c1retDat.IndexOf("Expansion:"));
                                        o = unitTrim(o, 0);
                                        o = o.Replace(">", "");
                                        unitParts.Add(o);

                                        stp++;
                                    }
                                    #endregion

                                    #region Step 12
                                    if (stg == 2 && stp == 12)
                                    {
                                        o = c1retDat.Substring(c1retDat.IndexOf("Rarity:"));
                                        o = unitTrim(o, 1);
                                        o = o.Substring(o.LastIndexOf(">") + 1, o.Length - (o.LastIndexOf(">") + 1));
                                        unitParts.Add(o);

                                        stp++;
                                    }
                                    #endregion

                                    #region Step 13
                                    if (stg == 2 && stp == 13)
                                    {
                                        o = c1retDat.Substring(c1retDat.IndexOf("Artist:"));
                                        o = unitTrim(o, 2);
                                        o = o.Replace(">", "");
                                        unitParts.Add(o);

                                        stg = 3;
                                        stp = 1;
                                    }
                                    #endregion

                                    #endregion

                                    #region Stage 3 [1]

                                    #region Stage 3 Step 1
                                    if (stg == 3 && stp == 1)
                                    {
                                        vC = new VrrktCard(i1GID, split, c1Parts, c2Parts, unitParts[0], unitParts[1], unitParts[2], c1Rules, c2Rules, i2GID);
                                        repullDone = true;
                                    }
                                    #endregion

                                    #endregion
                                }

                                repulled++;
                                stg = 1;
                                stp = 1;
                                repullDone = false;
                                step++;
                                #endregion
                            }
                            #endregion

                            #region 5
                            if (step == 5)
                            {
                                f = File.Create(pathBase + vC.GID + ".vrrktCd");

                                vC.writeToStream(f);
                                f.Close();
                                f.Dispose();
                                step++;
                            }
                            #endregion

                            #region 6
                            if (step == 6)
                            {
                                current++;
                                step = 1;
                            }
                            #endregion
                        });
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        #endregion

        #region Image Integrity Check
        public IEnumerator imageIntegrity(vrrktEditorVersion2 e)
        {
            #region Startup Operations
            UnityWebRequest uWeb = null;
            ed = e;
            repainter();
            step = 1;
            repulled = 0;
            int stp = 1;
            FileStream f = null;
            ed = e;
            repainter();
            step = 1;
            repulled = 0;
            #endregion

            while (current <= upper)
            {
                vCI = new VrrktCardImage();
                img1 = new Texture2D(1, 1);
                img2 = new Texture2D(1, 1);

                #region 1
                if (step == 1)
                {
                    vC = null;
                    repullDone = false;
                    f = null;
                    stp = 0;

                    if (File.Exists(pathBase + current + ".vrrktCi"))
                    {
                        step++;
                    }
                    else
                    {
                        current++;
                    }

                    yield return true;
                }
                #endregion

                #region 2
                if (step == 2)
                {
                    #region 0
                    if (stp == 0)
                    {
                        f = File.OpenRead(pathBase + current + ".vrrktCi");

                        if (vCI.stepwiseTryReconstruct(f, stp))
                        {
                            stp++;
                        }
                        else
                        {
                            repullDone = false;
                            f.Close();
                            stp = 1;
                            step++;
                        }

                        yield return true;
                    }
                    #endregion

                    #region 1/2
                    if (stp == 1 || stp == 2)
                    {
                        #region 1
                        if (stp == 1)
                        {
                            vCI.asyncReadBytes();

                            if (vCI.p1 == -1 || vCI.z1 <= vCI.p1 - 1)
                            {
                                repullDone = false;
                                f.Close();
                                stp = 1;
                                step++;
                            }
                            else
                            {
                                if (vCI.z1 > vCI.p1 - 1)
                                {
                                    stp = 2;
                                }
                            }

                            if (vCI.p2 == -1 || vCI.z2 <= vCI.p2 - 1)
                            {
                                repullDone = false;
                                f.Close();
                                stp = 1;
                                step++;
                            }
                            else
                            {
                                if (vCI.z2 > vCI.p2 - 1)
                                {
                                    stp = 3;
                                }
                            }

                            yield return true;
                        }
                        #endregion

                        yield return true;
                    }
                    #endregion

                    #region 3-8
                    if (stp > 2 && stp < 9)
                    {
                        if (vCI.stepwiseTryReconstruct(f, stp))
                        {
                            stp++;
                        }
                        else
                        {
                            repullDone = false;
                            f.Close();
                            stp = 1;
                            step++;
                        }

                        yield return true;
                    }
                    #endregion

                    #region 9
                    if (stp == 9)
                    {
                        f.Close();
                        stp = 1;
                        step = 1;
                        current++;

                        yield return true;
                    }
                    #endregion
                }
                #endregion

                #region 3
                if (step == 3)
                {
                    #region Mini Puller
                    if (!repullDone)
                    {
                        #region 1
                        if (stp == 1)
                        {
                            vC = new VrrktCard();
                            f = File.Open(pathBase + current + ".vrrktCd", FileMode.Open);
                            vC.readFromStream(f);
                            stp++;

                            yield return true;
                        }
                        #endregion

                        #region 2
                        if (stp == 2)
                        {
                            i1GID = vC.GID;

                            if (vC.i2GID > 0)
                            {
                                i2GID = vC.i2GID;
                            }
                            else
                            {
                                i2GID = -1;
                            }


                            stp++;

                            yield return true;
                        }
                        #endregion

                        #region 3
                        if (stp == 3)
                        {
                            //Debug.Log("Began getting image 1");
                            uWeb = new UnityWebRequest();
                            using (uWeb = UnityWebRequestTexture.GetTexture(imgBase + current + imgEnd))
                            {
                                uWeb.SendWebRequest();

                                while (!uWeb.isDone)
                                {
                                    //Debug.Log("Waiting on img 1");
                                    yield return true;
                                }

                                if (uWeb.isHttpError || uWeb.isNetworkError)
                                {
                                    Debug.Log("Image 1 Error for #" + current);
                                    step = 1;
                                    stp = 1;
                                    yield return false;
                                }
                                else
                                {
                                    img1 = DownloadHandlerTexture.GetContent(uWeb);
                                    //Debug.Log("Image 1 done");
                                    stp++;
                                    yield return true;
                                }
                            }

                            //Debug.Log("Finished ENUM step " + step);
                            yield return true;


                            yield return true;
                        }
                        #endregion

                        #region 4
                        if (stp == 4)
                        {
                            if (i2GID > 0)
                            {
                                uWeb = new UnityWebRequest();
                                using (uWeb = UnityWebRequestTexture.GetTexture(imgBase + i2GID + imgEnd))
                                {
                                    uWeb.SendWebRequest();

                                    while (!uWeb.isDone)
                                    {
                                        yield return true;
                                    }

                                    if (uWeb.isHttpError || uWeb.isNetworkError)
                                    {
                                        Debug.Log("Image 2 Error for #" + current);
                                        step = 1;
                                        stp = 1;
                                        yield return false;
                                    }
                                    else
                                    {
                                        img2 = DownloadHandlerTexture.GetContent(uWeb);
                                        stp++;
                                        //Debug.Log("Finished ENUM step " + step);
                                        yield return true;
                                    }
                                }
                            }
                            else
                            {
                                stp++;
                            }

                            yield return true;
                        }
                        #endregion

                        #region 5
                        if (stp == 5)
                        {
                            if (vCI == null || img1 == null)
                            {
                                if (vCI == null)
                                {
                                    Debug.Log("VCI is null at step 3, stp 4");
                                    step = 1;
                                    stp = 1;
                                }
                                else
                                {
                                    Debug.Log("img1 is null at step3, stp4");
                                    step = 1;
                                    stp = 1;
                                }
                            }
                            else
                            {
                                if (i2GID <= 0)
                                {
                                    vCI = new VrrktCardImage(img1, img1.width, img1.height, i1GID);
                                }
                                else
                                {
                                    vCI = new VrrktCardImage(img1, img1.width, img1.height, i1GID, img2, img2.width, img2.height, i2GID);
                                }

                                repullDone = true;
                            }

                            yield return true;
                        }
                        #endregion
                    }
                    #endregion

                    else
                    {
                        repulled++;
                        stp = 1;
                        repullDone = false;
                        step++;

                        yield return true;
                    }

                    yield return true;
                }
                #endregion

                #region 4
                if (step == 4)
                {
                    if (vCI.gid1 > 0)
                    {
                        asyncSave(vCI, pathBase + vCI.gid1 + ".vrrktCi");
                        step++;
                    }
                    else
                    {
                        step = 0;
                        break;
                    }

                    yield return true;
                }
                #endregion

                #region 5 
                if (step == 5)
                {
                    current++;
                    step = 1;

                    yield return true;
                }
                #endregion

                yield return true;
            }
            Debug.Log("Finished Card Integrity Check");
            yield return false;
        }
        #endregion

        #region Duplicate Filter Control
        public async void duplicateFilterControl(vrrktEditorVersion2 e)
        {
            #region Startup Operations
            ed = e;
            repainter();
            step = 1;
            repulled = 0;
            FileStream f = null;
            #endregion

            while (current <= upper)
            {
                await Task.Run(() =>
                {
                    for (int n = 0; n <= 9; n++)
                    {
                        if (current <= upper)
                        {
                            #region 1
                            if (step == 1)
                            {
                                if (File.Exists(pathBase + current + ".vrrktCd"))
                                {
                                    step++;
                                }
                                else
                                {
                                    step = 1;
                                    current++;
                                }
                            }
                            #endregion

                            #region 2
                            if (step == 2)
                            {
                                vC = null;
                                f = File.OpenRead(pathBase + current + ".vrrktCd");
                                vC = new VrrktCard();
                                vC.readFromStream(f);
                                f.Close();
                                f.Dispose();
                                f = null;
                                step++;
                            }
                            #endregion

                            #region 3
                            if (step == 3)
                            {
                                if (current == vC.i2GID)
                                {
                                    repulled++;
                                    File.Delete(pathBase + current + ".vrrktCd");
                                }
                                step = 1;
                                current++;
                            }
                            #endregion
                        }
                        else
                        {
                            break;
                        }
                    }
                });
            }

            step = 1;
        }
        #endregion

        #endregion
        
        #region Repainter
        async void repainter()
        {
            while (current <= upper)
            {
                ed.Repaint();
                await Task.Delay(1);
            }
        }
        #endregion
        
        #region Async Save Method
        async void asyncSave(VrrktCard c, string p)
        {
            FileStream f = File.Create(p);

            await Task.Run(() =>
            {
                c.writeToStream(f);
            });
            f.Close();
            f.Dispose();
        }

        async void asyncSave(VrrktCardImage c, string p)
        {
            FileStream f = File.Create(p);

            await Task.Run(() =>
            {
                c.writeToStream(f);
            });
            f.Close();
            f.Dispose();
        }
        #endregion

        #region VrrktFormatter Methods

        #region Save
        void vrrktSave(VrrktCard v)
        {
            //Debug.Log("Called Save");
            FileStream f = File.Create(pathBase + v.GID + ".vrrktCd");
            VrrktCoroutine.start(VrrktFormatter.serialize(f, v, this));
        }

        void vrrktSave(VrrktCardImage v)
        {
            FileStream f = File.Create(pathBase + v.gid1 + ".vrrktCi");
            VrrktCoroutine.start(VrrktFormatter.serialize(f, v, this));
        }
        #endregion

        #region Load Card
        void vrrktLoad(int n)
        {
            FileStream f = File.Open(pathBase + n + ".vrrktCd", FileMode.Open);
            VrrktCoroutine.start(VrrktFormatter.deserialize(f, this, pathBase + n + ".vrrktCd", true));
        }
        #endregion

        #region Passback
        public void vrrktPassback(int t, int tL, int sT, int sTL, bool done, object obj, Type type)
        {
            if (done)
            {
                if (obj != null)
                {
                    if (obj.GetType() == typeof(VrrktCard))
                    {
                        vC = (VrrktCard)obj;
                    }
                }
                else
                {

                }
            }
            else
            {
                // Debug.Log("T:" + t + ", tL:" + tL + ", sT:" + sT + ", sTL:" + sTL + ", Done:" + done);
            }
        }

        public void vrrktPassback2(object o)
        {
            if (o.GetType() == typeof(VrrktCard))
            {
                vC = (VrrktCard)o;
                //Debug.Log("Received card");
            }
            else
            {
                Debug.Log("Type did not match card");
            }
        }
        #endregion
        
        #endregion
    }
}
#endregion
