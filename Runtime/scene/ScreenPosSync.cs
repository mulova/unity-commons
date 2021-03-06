﻿using UnityEngine;
using System.Collections;
using UnityEngine.Ex;

namespace mulova.comunity
{
    [ExecuteAlways]
    public class ScreenPosSync : MonoBehaviour
    {
        public Transform srcObj;
        public Vector3 offset;
        public bool srcMoving = true;
        public float depth = 1;
        private Camera _srcCam;
        private bool moving;

        public Camera srcCam
        {
            get
            {
                if (_srcCam == null)
                {
                    _srcCam = CameraEx.GetCamera(srcObj.gameObject.layer);
                }
                return _srcCam;
            }
        }

        private Camera _dstCam;

        public Camera dstCam
        {
            get
            {
                if (_dstCam == null)
                {
                    _dstCam = CameraEx.GetCamera(gameObject.layer);
                }
                return _dstCam;
            }
        }

        void Start()
        {
            moving = srcMoving || (!Application.isPlaying&&Application.isEditor);
            if (!moving)
            {
                Sync();
                if (Application.isPlaying)
                {
                    enabled = false;
                }
            }
        }
    
        void Update()
        {
            if (moving)
            {
                Sync();
            }
        }

        [ContextMenu("Sync")]
        public void Sync()
        {
            if (srcObj == null || srcCam == dstCam)
            {
                return;
            }
            Vector3 screenPos = srcCam.WorldToScreenPoint(srcObj.position);
            screenPos.z = depth;
            Vector3 dstPoint = dstCam.ScreenToWorldPoint(screenPos);
            transform.position = dstPoint+offset;
        }
    }
}