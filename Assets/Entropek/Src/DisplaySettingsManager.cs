using System;
using System.Collections;
using System.Collections.Generic;
using Entropek;
using UnityEngine;

namespace Entropek
{
    public class DisplaySettingsManager : MonoBehaviour{
        
        public static DisplaySettingsManager Singleton {get; private set;}

        private const string PlayerPrefsResolutionPreset = "ResolutionPreset";
        private const string PlayerPrefsFullscreenPreset = "FullscreenPreset";
        private const string PlayerPrefsTargetFrameRatePreset = "TargetFrameRate";
        private const string PlayerPrefsVsyncPreset = "VsyncPreset";

        private Dictionary<int, Vector2Int> resolutions = new Dictionary<int, Vector2Int>(){
            {0, new Vector2Int(1280,720)},
            {1, new Vector2Int(1920,1080)},
            {2, new Vector2Int(2560,1440)},
            {3, new Vector2Int(3840,2160)},
        };

        private Dictionary<int, int> targetFrameRates = new Dictionary<int, int>(){
            {0, -1},
            {1, 30},
            {2, 60},
            {3, 120},
            {4, 165},
            {5, 244},
        };
        
        private int resolutionPreset = 0;
        private int targetFrameRatePreset = 0;
        private bool vsyncPreset = false;
        private bool fullscreen = false;

        public void LoadPlayerPrefs(){
            SetResolution(LoadResolutionPreset());
            SetFullscreen(LoadFullscreenPreset());
            SetVsync(LoadVsyncPreset());
            SetTargetFrameRatePreset(LoadFrameRatePreset());
        }

        public int LoadResolutionPreset(){
            return PlayerPrefs.GetInt(PlayerPrefsResolutionPreset, 0);
        }

        public bool LoadFullscreenPreset(){
            return PlayerPrefs.GetInt(PlayerPrefsFullscreenPreset,0) == 1? true: false;
        }

        private int LoadFrameRatePreset(){
            return PlayerPrefs.GetInt(PlayerPrefsTargetFrameRatePreset, 0);
        }

        public bool LoadVsyncPreset()
        {
            return PlayerPrefs.GetInt(PlayerPrefsVsyncPreset, 0)==1? true : false;
        }

        public void SavePlayerPrefs(){
            PlayerPrefs.SetInt(PlayerPrefsTargetFrameRatePreset,targetFrameRatePreset);
            PlayerPrefs.SetInt(PlayerPrefsFullscreenPreset, fullscreen == true?1:0);
            PlayerPrefs.SetInt(PlayerPrefsResolutionPreset, resolutionPreset);
            PlayerPrefs.SetInt(PlayerPrefsVsyncPreset, vsyncPreset == true?1:0);
            PlayerPrefs.Save();
        }

        public void SetResolution(int preset){
            resolutionPreset = preset;
            Vector2Int resolution = resolutions[preset];
            Screen.SetResolution(resolution.x, resolution.y, fullscreen);
        }
        
        public void SetFullscreen(bool toggle){
            fullscreen = toggle;
            Screen.fullScreenMode = fullscreen == false? FullScreenMode.Windowed : FullScreenMode.FullScreenWindow;
            
            // set resolution again so that full screen is applied.
            
            SetResolution(resolutionPreset);
        }

        public void SetTargetFrameRatePreset(int preset){
            targetFrameRatePreset = preset;
            Application.targetFrameRate = targetFrameRates[preset];
        }

        public void SetVsync(bool toggle)
        {
            vsyncPreset = toggle;
            QualitySettings.vSyncCount = toggle==true?1:0;
        }

        public int GetResolutionPreset()        => resolutionPreset;
        public bool GetIsFullscreen()           => fullscreen;
        public int GetTargetFrameRatePreset()   => targetFrameRatePreset;
        public bool GetVsyncPreset()            => vsyncPreset; 
    

        /// <summary>
        /// Bootstrap.
        /// </summary>


        private static class Bootstrap
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
            private static void Initialise()
            {
                GameObject main = new();
                Singleton = main.AddComponent<DisplaySettingsManager>();
                DontDestroyOnLoad(main);
            }
        }
    }
}
