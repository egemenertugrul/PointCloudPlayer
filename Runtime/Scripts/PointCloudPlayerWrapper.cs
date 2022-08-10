using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PCP
{
    public class PointCloudPlayerWrapper : MonoBehaviour
    {
        public PointCloudPlayer[] players;
        public int MaxFPS = 30;

        public int MaxIndex
        {
            get
            {
                int maxIndex = -1;
                foreach (var player in players)
                {
                    maxIndex = Mathf.Max(maxIndex, player.PlayIndex);
                }
                return maxIndex;
            }
        }

        public int CurrentFPS
        {
            get
            {
                int maxFPS = -1;
                foreach (var player in players)
                {
                    maxFPS = Mathf.Max(maxFPS, player.FPS);
                }
                return maxFPS;
            }

            set
            {
                foreach (var player in players)
                {
                    player.FPS = value;
                }
            }
        }

        private Coroutine noObsoleteDataCheckerRoutine;

        private void Start()
        {
            foreach (var player in players)
            {
                player.OnObsoleteDataReceived.AddListener(OldObsoleteDataReceivedAction);
            }
        }

        private IEnumerator NoObsoleteDataReceivedChecker()
        {
            yield return new WaitForSeconds(1);
            if (CurrentFPS + 1 < MaxFPS)
            {
                ++CurrentFPS;
            }
        }
        private void OldObsoleteDataReceivedAction()
        {
            ReduceFPS();
            SyncIndex();

            if (noObsoleteDataCheckerRoutine != null)
            {
                StopCoroutine(noObsoleteDataCheckerRoutine);
            }
            noObsoleteDataCheckerRoutine = StartCoroutine(NoObsoleteDataReceivedChecker());
        }

        private void ReduceFPS()
        {
            foreach (var player in players)
            {
                --player.FPS;
            }
        }

        private void SyncIndex()
        {
            foreach (var player in players)
            {
                player.PlayIndex = MaxIndex;
            }
        }
    }
}