using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace JammerDash
{
    public class SettingsFileHandler : MonoBehaviour
    {
        public static void SaveSettingsToFile(SettingsData data)
        {
            string json = JsonUtility.ToJson(data, true);

            // Save the settings to a file
            string filePath = Application.persistentDataPath + "/settings.json";
            System.IO.File.WriteAllText(filePath, json);
        }
        public static SettingsData LoadSettingsFromFile()
        {
            // Load settings from the file
            string filePath = Application.persistentDataPath + "/settings.json";

            if (System.IO.File.Exists(filePath))
            {
                string json = System.IO.File.ReadAllText(filePath);
                return JsonUtility.FromJson<SettingsData>(json);
            }
            else
            {
                // Return default settings or handle the absence of a settings file
                return new SettingsData();
            }
        }

        public static string GetLanguageName()
        {
            // Detect system locale
            string systemLocale = CultureInfo.CurrentCulture.Name;

            if (systemLocale == "pl-PL")
            {
                systemLocale = "pl";
            }

            if (Enum.TryParse<Options.Language>(systemLocale, false, out _))
                return systemLocale;

            return "en-US";
            
        }
    }

    public class SettingsData
    {
        public int resolutionValue = 99;
        public int selectedFPS = 60;
        public bool vsync = true;
        public float volume = 0;
        public float musicVol = 0.75f;
        public float sfxVol = 1;
        public int backgroundType = 0;
        public bool sfx = true;
        public bool hitNotes = true;
        public bool focusVol = true;
        public int playerType = 0;
        public bool cursorTrail = false;
        public float noFocusVolume = -20f;
        public float lowpassValue = 500;
        public float mouseParticles = 1000;
        public bool isShowingFPS = false;
        public int hitType = 0;
        public float cursorFade = 0.24f;
        public bool parallax = true;
        public bool confinedMouse = false;
        public int bgTime = 0;
        public bool wheelShortcut = true;
        public bool volumeIncrease = false;
        public bool snow = false;
        public bool loadedLogoSFX = false;
        public bool canvasOff = false;
        public bool bass = false;
        public float bassgain = 1.5f;
        public float dim = 1;
        public bool shaders = false;
        public bool discordPlay = false;
        public bool discordAFK = false;
        public bool discordEdit = false;

        public bool region = true;
        public string language = SettingsFileHandler.GetLanguageName();
        public string gameVersion = Application.version;
        public string saveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

    }
}