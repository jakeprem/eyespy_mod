using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private Dictionary<String, String> _colonistHashes;

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

            this._colonistHashes = new Dictionary<String, String>();

            remoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8083);

            Log.Message("Sending to 127.0.0.1");
            SendPacket("InitialPacket");            
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
                    var colonistsToSend = new List<ColonistData>();

                    foreach (Pawn current in PawnsFinder.AllMapsAndWorld_Alive)
                    {
                        if (current.IsColonist)
                        {
                            var newColonist = new ColonistData{ ID = current.ThingID, Name = current.Name.ToStringShort, CurrentJob = current.jobs.curDriver.GetReport().CapitalizeFirst() };
                            var newColonistHash = HashColonist(newColonist);

                            if (_colonistHashes.ContainsKey(newColonist.ID))
                            {                                
                                var currentColonistHash = _colonistHashes[newColonist.ID];                                
                                if (currentColonistHash != newColonistHash)
                                {
                                    SendPacket(newColonistHash + " : " + newColonist.Name + " : " + newColonist.CurrentJob);

                                    colonistsToSend.Add(newColonist);
                                    _colonistHashes[newColonist.ID] = newColonistHash;                                                                                                            
                                }


                            } else
                            {
                                SendPacket("New colonist: " + newColonistHash + " : " + newColonist.Name + " : " + newColonist.CurrentJob);

                                _colonistHashes[newColonist.ID] = newColonistHash;
                                colonistsToSend.Add(newColonist);
                            }
                        }
                    }                    
                    
                    //string json = JsonConvert.SerializeObject(colonistsToSend, Formatting.Indented);                    
                    //string json = JsonHelper.ToJson(colonistsToSend.ToArray());                    

                }
                catch (Exception ex)
                {
                    enabled = false;
                    Log.Error(ex.Message);
                }
            }
        }

        private void SendPacket(string textToSend)
        {
            byte[] data = Encoding.UTF8.GetBytes(textToSend);
            client.Send(data, data.Length, remoteEndPoint);
        }

        // Works but not serializing currently
        [Serializable]
        private class ColonistData
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string CurrentJob { get; set; }
        }

        // Works
        private string HashColonist(ColonistData colonist)
        {
            var text = colonist.ID + colonist.Name + colonist.CurrentJob;
            return Base64Encode(text);
        }

        // Works
        private string Base64Encode(string plainText)
        {
            var textBytes = Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(textBytes);
        }
    }
}
