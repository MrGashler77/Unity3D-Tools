#region Libraries
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.Networking;
#endregion

#region Main Class
namespace Verruckt.Operations
{
    public class VrrktCoroutine
    {
        #region Variables
        private static List<IEnumerator> routines;
        private static vrrktEditorVersion2 vE;
        #endregion

        #region Constructor

        #region Normal Constructor
        public static VrrktCoroutine start(IEnumerator routine)
        {
            VrrktCoroutine coroutine = new VrrktCoroutine(routine, true);
            if (routines == null)
            {
                routines = new List<IEnumerator>();
            }

            if (routines.Count <= 1)
            {
                coroutine.start();
            }

            return coroutine;
        }
        #endregion

        #region Repainting Constructor
        public static VrrktCoroutine start(IEnumerator routine, vrrktEditorVersion2 vEd)
        {
            VrrktCoroutine coroutine = new VrrktCoroutine(routine, true, vEd);
            if (routines == null)
            {
                routines = new List<IEnumerator>();
            }

            if (routines.Count <= 1)
            {
                coroutine.start();
            }

            return coroutine;
        }
        #endregion

        #region Speed Constructor
        public static VrrktCoroutine start(IEnumerator routine, bool f)
        {
            VrrktCoroutine coroutine = new VrrktCoroutine(routine, true);
            if (routines == null)
            {
                routines = new List<IEnumerator>();
            }

            if (routines.Count <= 1)
            {
                coroutine.start(true);
            }

            return coroutine;
        }
        #endregion

        #region Speed Repainting Constructor
        public static VrrktCoroutine start(IEnumerator routine, vrrktEditorVersion2 vEd, bool f)
        {
            VrrktCoroutine coroutine = new VrrktCoroutine(routine, true, vEd);
            if (routines == null)
            {
                routines = new List<IEnumerator>();
            }

            if (routines.Count <= 1)
            {
                coroutine.start(true);
            }

            return coroutine;
        }
        #endregion

        #endregion

        #region Destructor
        public static VrrktCoroutine destruct(IEnumerator routine)
        {
            VrrktCoroutine coroutine = new VrrktCoroutine(routine, false);
            return null;
        }
        #endregion

        #region Total Wipe
        public static VrrktCoroutine totalWipe()
        {
            if (routines != null)
            {
                if (routines.Count > 0)
                {
                    foreach (IEnumerator i in routines)
                    {
                        destruct(i);
                    }
                }
            }
            return null;
        }
        #endregion

        #region Definers

        #region Normal Definer
        public VrrktCoroutine(IEnumerator routine, bool IO)
        {
            if (IO)
            {
                if (routines == null)
                {
                    routines = new List<IEnumerator>();
                }

                routines.Add(routine);
            }
            else
            {
                stop(routine);
            }
        }
        #endregion

        #region Repainting Definer
        public VrrktCoroutine(IEnumerator routine, bool IO, vrrktEditorVersion2 v)
        {
            if (IO)
            {
                if (routines == null)
                {
                    routines = new List<IEnumerator>();
                }

                if (v != null && vE != v)
                {
                    vE = v;
                }

                routines.Add(routine);
            }
            else
            {
                stop(routine);
            }
        }
        #endregion

        #region Empty Definer
        public VrrktCoroutine()
        {
            routines = null;
            vE = null;
        }
        #endregion

        #endregion

        #region Start Function
        void start()
        {
            //Debug.Log("Start called");
            EditorApplication.update += update;
            EditorApplication.update += update;
            EditorApplication.update += update;
        }

        void start(bool f)
        {
            //Debug.Log("Start called");
            EditorApplication.update += update;
            EditorApplication.update += update;
            EditorApplication.update += update;
            EditorApplication.update += update;
        }
        #endregion

        #region Stop Function
        public void stop(IEnumerator ie)
        {
            //Debug.Log("Stopped");
            List<IEnumerator> conv = new List<IEnumerator>();

            foreach (IEnumerator i in routines)
            {
                if (i != ie)
                {
                    conv.Add(i);
                }
            }
            routines = conv;

            if (routines.Count <= 0)
            {
                EditorApplication.update -= update;
                routines = new List<IEnumerator>();
            }
        }
        #endregion

        #region Update Function
        void update()
        {
            if (routines != null)
            {
                #region 5 Cycles
                for (int n = 0; n <= routines.Count - 1; n++)
                {
                    if (routines[n] != null)
                    {
                        if (!routines[n].MoveNext())
                        {
                            stop(routines[n]);
                        }
                    }
                }

                for (int n = 0; n <= routines.Count - 1; n++)
                {
                    if (routines[n] != null)
                    {
                        if (!routines[n].MoveNext())
                        {
                            stop(routines[n]);
                        }
                    }
                }

                for (int n = 0; n <= routines.Count - 1; n++)
                {
                    if (routines[n] != null)
                    {
                        if (!routines[n].MoveNext())
                        {
                            stop(routines[n]);
                        }
                    }
                }

                for (int n = 0; n <= routines.Count - 1; n++)
                {
                    if (routines[n] != null)
                    {
                        if (!routines[n].MoveNext())
                        {
                            stop(routines[n]);
                        }
                    }
                }

                for (int n = 0; n <= routines.Count - 1; n++)
                {
                    if (routines[n] != null)
                    {
                        if (!routines[n].MoveNext())
                        {
                            stop(routines[n]);
                        }
                    }
                }
                if (vE != null)
                {
                    vE.Repaint();
                }
                #endregion

                #region 5 Cycles
                for (int n = 0; n <= routines.Count - 1; n++)
                {
                    if (routines[n] != null)
                    {
                        if (!routines[n].MoveNext())
                        {
                            stop(routines[n]);
                        }
                    }
                }

                for (int n = 0; n <= routines.Count - 1; n++)
                {
                    if (routines[n] != null)
                    {
                        if (!routines[n].MoveNext())
                        {
                            stop(routines[n]);
                        }
                    }
                }

                for (int n = 0; n <= routines.Count - 1; n++)
                {
                    if (routines[n] != null)
                    {
                        if (!routines[n].MoveNext())
                        {
                            stop(routines[n]);
                        }
                    }
                }

                for (int n = 0; n <= routines.Count - 1; n++)
                {
                    if (routines[n] != null)
                    {
                        if (!routines[n].MoveNext())
                        {
                            stop(routines[n]);
                        }
                    }
                }

                for (int n = 0; n <= routines.Count - 1; n++)
                {
                    if (routines[n] != null)
                    {
                        if (!routines[n].MoveNext())
                        {
                            stop(routines[n]);
                        }
                    }
                }
                if (vE != null)
                {
                    vE.Repaint();
                }
                #endregion
                
                #region 5 Cycles
                for (int n = 0; n <= routines.Count - 1; n++)
                {
                    if (routines[n] != null)
                    {
                        if (!routines[n].MoveNext())
                        {
                            stop(routines[n]);
                        }
                    }
                }

                for (int n = 0; n <= routines.Count - 1; n++)
                {
                    if (routines[n] != null)
                    {
                        if (!routines[n].MoveNext())
                        {
                            stop(routines[n]);
                        }
                    }
                }

                for (int n = 0; n <= routines.Count - 1; n++)
                {
                    if (routines[n] != null)
                    {
                        if (!routines[n].MoveNext())
                        {
                            stop(routines[n]);
                        }
                    }
                }

                for (int n = 0; n <= routines.Count - 1; n++)
                {
                    if (routines[n] != null)
                    {
                        if (!routines[n].MoveNext())
                        {
                            stop(routines[n]);
                        }
                    }
                }

                for (int n = 0; n <= routines.Count - 1; n++)
                {
                    if (routines[n] != null)
                    {
                        if (!routines[n].MoveNext())
                        {
                            stop(routines[n]);
                        }
                    }
                }
                if (vE != null)
                {
                    vE.Repaint();
                }
                #endregion
                
                #region 5 Cycles
                for (int n = 0; n <= routines.Count - 1; n++)
                {
                    if (routines[n] != null)
                    {
                        if (!routines[n].MoveNext())
                        {
                            stop(routines[n]);
                        }
                    }
                }

                for (int n = 0; n <= routines.Count - 1; n++)
                {
                    if (routines[n] != null)
                    {
                        if (!routines[n].MoveNext())
                        {
                            stop(routines[n]);
                        }
                    }
                }

                for (int n = 0; n <= routines.Count - 1; n++)
                {
                    if (routines[n] != null)
                    {
                        if (!routines[n].MoveNext())
                        {
                            stop(routines[n]);
                        }
                    }
                }

                for (int n = 0; n <= routines.Count - 1; n++)
                {
                    if (routines[n] != null)
                    {
                        if (!routines[n].MoveNext())
                        {
                            stop(routines[n]);
                        }
                    }
                }

                for (int n = 0; n <= routines.Count - 1; n++)
                {
                    if (routines[n] != null)
                    {
                        if (!routines[n].MoveNext())
                        {
                            stop(routines[n]);
                        }
                    }
                }
                if (vE != null)
                {
                    vE.Repaint();
                }
                #endregion
            }
            else
            {
                //Debug.Log("Was Null");
                routines = new List<IEnumerator>();
            }
        }
        #endregion

        #region Get Number Running 
        public static int getCountRunning()
        {
            if (routines != null)
            {
                return routines.Count;
            }
            else
            {
                return -1;
            }
        }
        #endregion

        #region Get Names Of Running Enumerators
        public static List<string> getRunningNames()
        {
            List<string> s = new List<string>();
            for (int n = 0; n <= routines.Count - 1; n++)
            {
                s.Add(routines[n].ToString());
            }

            return s;
        }
        #endregion
    }
}
#endregion
