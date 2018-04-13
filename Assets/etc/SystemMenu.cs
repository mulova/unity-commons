using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using comunity;

namespace etc
{
    public class SystemMenu : MonoBehaviour {
        
        public bool menuButton = true;
        public bool backButton = true;
        
        private bool captureMode = false;
        private float captureTime = 2.0f;
        private float menuPressedTime = 0.0f;
        private KeyCode menuCode = KeyCode.Menu;
        
        private static SystemMenu instance;
        
        void Awake() {
            if (Platform.isEditor) {
                menuCode = KeyCode.Space;
            }
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        void OnDestroy() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            //      InputListenerStack.Clear();
        }
        
        public static SystemMenu Instance {
            get { return instance; }
        }
        
        private bool menuPressed;
        void Update() {
            if (menuButton) {
                if (Input.GetKeyDown(menuCode)) {
                    menuPressedTime = 0;
                    menuPressed = true;
                }
                if (Input.GetKeyUp(menuCode)) {
                    menuPressed = false;
                    if (captureMode) {
                        captureMode = false;
                    } else {
                        InputListenerStack.inst.OnButton(KeyCode.Menu, ButtonState.Released);
                    }
                }
                if (menuPressed && menuPressedTime < captureTime) {
                    menuPressedTime += Time.deltaTime;
                    if (menuPressedTime >= captureTime) {
                        CaptureScreenshot();
                        captureMode = true;
                    }
                }
            }
            
            if (backButton) {
                if (Input.GetKeyUp(KeyCode.Escape)) {
                    InputListenerStack.inst.OnButton(KeyCode.Escape, ButtonState.Released);
                }
            }
        }
        
        public static void QuitGame(bool ok, object[] objParams) {
            if (ok) {
                QuitGame();
            }
        }
        
        public static void QuitGame() {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #elif UNITY_IOS
            Application.LoadLevel(0);
            #elif !UNITY_WEBGL
            // New application quit - Process kill 
            ProcessThreadCollection pt = Process.GetCurrentProcess().Threads;
            foreach(ProcessThread p in pt)  {
                p.Dispose();
            }
            System.Diagnostics.Process.GetCurrentProcess().Kill();                      
            #endif
        }
        
        private void CaptureScreenshot() {
            DateTime captureDateTime = DateTime.Now;
            captureDateTime.GetDateTimeFormats();
            string filename = string.Format("LineUp_{0}.png" , DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
            ScreenCapture.CaptureScreenshot(filename);
            StartCoroutine(ShowScreenshotMessage());
        }
        
        IEnumerator ShowScreenshotMessage() {
            yield return new WaitForSeconds(0.5f);
            //      SplashMessage.Open("스크린샷이 저장되었습니다.");
        }
    }
}
