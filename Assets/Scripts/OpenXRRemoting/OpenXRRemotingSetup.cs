using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.OpenXR.Remoting;


namespace Microsoft.MixedReality.OpenXR.BasicSample {
    public class OpenXRRemotingSetup : MonoBehaviour
    {        
        [SerializeField]
        private ushort hostPort = 8265;

        [SerializeField]
        private uint maxBitrateInKbps = 20000;

        private RemotingConnectConfiguration remotingConnectConfiguration = new RemotingConnectConfiguration();
        
        public GameObject debugObjectPrefab;
        public GameObject debugArrowObjectPrefab;
        private GameObject debugObject;
        private GameObject debugArrowObject;
        private bool debugging = false;

        void Start()
        {
            string textFilePath = Application.streamingAssetsPath + "/host_ip.txt";
            if (File.Exists(textFilePath)) {
                string hostIP = File.ReadAllText(textFilePath);
                if (!string.IsNullOrEmpty(hostIP)) {
                    remotingConnectConfiguration.RemoteHostName = hostIP;
                }
            } else {
                string defaultHostIp = "127.0.0.1";
                File.WriteAllText(textFilePath, defaultHostIp);
                remotingConnectConfiguration.RemoteHostName = defaultHostIp;
            }

            remotingConnectConfiguration.RemotePort = hostPort;
            remotingConnectConfiguration.EnableAudio = true;
            remotingConnectConfiguration.MaxBitrateKbps = maxBitrateInKbps;
           
            AppRemoting.StartConnectingToPlayer(remotingConnectConfiguration);
        }

        void Update() {
            // Create objects to check if rendering works
            if (Input.GetKeyUp(KeyCode.C)) {
                ToggleDebugObjects();            
            }
        }

        void ToggleDebugObjects() {
            if (debugging) {
                Destroy(debugObject);
                Destroy(debugArrowObject);
                debugging = false;
            } else {
                debugging = true;
                debugObject = Instantiate(debugObjectPrefab, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
                debugArrowObject = Instantiate(debugArrowObjectPrefab, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
            }
        }

        void Awake() 
        {
            UnityEngine.Rendering.TextureXR.maxViews = 2;
        }        
        
        void OnApplicationQuit()
        {
            AppRemoting.Disconnect();
            Debug.Log("Disconnected XR remoting");
        }
    }
}
