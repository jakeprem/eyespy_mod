using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using Verse;

namespace EyeSpy
{
    [StaticConstructorOnStartup]
    public class EyeSpyController : MonoBehaviour
    {
        private UdpClient client;
        private IPEndPoint remoteEndPoint;

        static EyeSpyController()
        {
            GameObject initializer = new UnityEngine.GameObject("EyeSpyObject");
            initializer.AddComponent<EyeSpyController>();
            UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)initializer);
        }

        public void Start()
        {
            enabled = false;
            this.client = new UdpClient();
            remoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8083);

            Log.Message("Sending to 127.0.0.1");
            byte[] data = Encoding.UTF8.GetBytes("Test text");
            client.Send(data, data.Length, remoteEndPoint);
        }

        public void OnLevelWasLoaded(int level)
        {
            if (level == 0)
            {            
                enabled = false;
            }
            else if (level == 1)
            {
                enabled = true;
            }
        }

        public void Update()
        {
           if (Find.VisibleMap != null)
            {
                try
                {
                    foreach (Pawn current in PawnsFinder.AllMapsAndWorld_Alive)
                    {
                        if (current.IsColonist)
                        {
                            var name = current.Name;
                            var name_short = name.ToStringShort;   
                            
                            var text = "{\"name\": \"" + name_short + "\", \"job\": \"" + current.jobs.curDriver.GetReport().CapitalizeFirst() + "\", \"id\": \"" + current.ThingID + "\"}";
                            byte[] data = Encoding.UTF8.GetBytes(text);
                            client.Send(data, data.Length, remoteEndPoint);

                        }
                    }

                }
                catch (Exception ex)
                {
                    // enabled = false;
                    var st = new StackTrace(ex, true);
                    var frame = st.GetFrame(0);
                    var line = frame.GetFileLineNumber();
                    Log.Error(line.ToString());
                    Log.Error(ex.Message);
                }
            }
        }
    }
}
