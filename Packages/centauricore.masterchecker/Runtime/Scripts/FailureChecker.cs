
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Centauri.FailureChecking
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class FailureChecker : UdonSharpBehaviour
    {
        [Tooltip("Failure threshold (s)")]
        public int FailureThreshold = 10;
        private int currentValue = 0;
        private bool hasFailed;

        [UdonSynced] private byte serializeTestVal;
        
        [Tooltip("All behaviours that the OnMasterFailure event is fired on!")]
        public UdonSharpBehaviour[] AllAttachedBehaviours = new UdonSharpBehaviour[0];
        private string FailureEventName = "OnMasterFailure";
        
        
        private void Start()
        {
            SendCustomEventDelayedSeconds(nameof(CheckLoop), 1f);
        }

        public void CheckLoop()
        {
            if (Networking.IsOwner(gameObject))
            {
                SerializeData();
            }
            else
            {
                currentValue+= 2;
                
                if (!hasFailed && currentValue >= FailureThreshold)
                {
                    foreach (var behaviour in AllAttachedBehaviours)
                    {
                        behaviour.SendCustomEvent(FailureEventName);
                    }

                    hasFailed = true;
                }
            }
            
            SendCustomEventDelayedSeconds(nameof(CheckLoop), 2);
        }

        public void OnMasterFailure()
        {
            Debug.Log("[<color=red>FAILURE CHECK</color>] Master has failed!");
        }

        #region Networking

        public void SerializeData()
        {
            if (VRCPlayerApi.GetPlayerCount() < 2)
            {
                OnPreSerialization();
            }
            
            RequestSerialization();
        }

        public override void OnPreSerialization()
        {
            currentValue = 0;
        }

        public override void OnDeserialization()
        {
            currentValue = 0;
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            currentValue = 0;
        }

        #endregion
    }
}
