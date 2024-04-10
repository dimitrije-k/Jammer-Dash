﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Profiling;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Net.NetworkInformation;
using UnityEditor;
using System;
using System.IO;
using System.Diagnostics;
using UnityEngine.Networking.Types;
using Ping = UnityEngine.Ping;
using System.Collections;
using Debug = UnityEngine.Debug;
using System.Collections.Generic;

public class StatsMan : MonoBehaviour
{
    public Text gui;


    [SerializeField]
    private AudioMixer audioMixer; // Reference to your AudioMixer
    private AudioSource musicSource;
        
    // Update is called once per frame
    void FixedUpdate()
    {
            gui.text = $"Debug v10 - Jammer Dash {Application.version} ({Application.unityVersion})\n\n";

            DisplayAudioInfo();
            DisplayInputInfo();
            DisplayOptionsInfo();
            DisplaySystemInfo();
            DisplayGraphicsInfo();
            DisplayVideoInfo();

    } 
    
    void DisplaySystemInfo()
    {
        gui.text += "System Memory: " + (SystemInfo.systemMemorySize / 1000).ToString("f2") + "GB" +
                    "\nProcessor Type: " + SystemInfo.processorType +
                    "\nProcessor Count: " + SystemInfo.processorCount +
                    "\nDevice Type: " + SystemInfo.deviceType +
                    "\nOperating System: " + SystemInfo.operatingSystem +
                    "\nCPU Speed: " + SystemInfo.processorFrequency + "MHz" +
                    "\nSystem Language: " + Application.systemLanguage;
    }

    void DisplayGraphicsInfo()
    {
        gui.text += "\n\n" + "GPU: " + SystemInfo.graphicsDeviceName +
                    "\nGPU Type: " + SystemInfo.graphicsDeviceType +
                    "\nGPU Version: " + SystemInfo.graphicsDeviceVersion +
                    "\nGPU Memory: " + SystemInfo.graphicsMemorySize + "MB\n\n";
    }

    void DisplayAudioInfo()
    {
        gui.text += "Audio Mixer: " + (audioMixer != null ? audioMixer.name : "N/A");

        if (musicSource != null && musicSource.name == "mainmenu")
        {
            gui.text += "\nMusic Clip: " + (musicSource.clip != null ? musicSource.clip.name : "N/A") + 
                        "\nCurrent song index: " + musicSource.GetComponent<AudioManager>().currentClipIndex  +"\n\n";
        }
        else
        {
            musicSource = GameObject.Find("mainmenu").GetComponent<AudioSource>();
        }
    }

    void DisplayOptionsInfo()
    {
        SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
        bool scoretype = Convert.ToBoolean(data.scoreType);
        string a = scoretype ? "Old" : "New";
        gui.text += "Resolution value: " + data.resolutionValue + $" ({Screen.width}x{Screen.height})"+
                    "\nScreen Mode: " + data.windowMode + $" ({Screen.fullScreenMode})"+
                    "\nQuality Level: " + data.qualitySettingsLevel + $" ({GetQualityLevelName()})" +
                    "\nArtistic Backgrounds: " + data.artBG + $" ({Resources.LoadAll<Sprite>("backgrounds").Length} bgs)" +
                    "\nCustom Backgrounds: " + data.customBG + $" ({Directory.GetFiles(Application.persistentDataPath + "/backgrounds", "*.png").Length} bgs)" +
                    "\nVideo Backgrounds: " + data.vidBG + $" ({Directory.GetFiles(Application.persistentDataPath + "/backgrounds", "*.mp4").Length} bgs)" +
                    "\nSFX: " + data.sfx + 
                    "\nHit Notes: " + data.hitNotes + 
                    "\nPlayer Type: " + data.playerType + 
                    "\nAntialiasing: " + data.antialiasing + 
                    "\nCursor Trail: " + data.cursorTrail + 
                    "\nVisualizers: {" + data.allVisualizers + "," + data.lineVisualizer + "," + data.logoVisualizer + "," + data.bgVisualizer + "}" +
                    "\nNo focus volume: " + data.noFocusVolume + 
                    "\nLowpass value: " + data.lowpassValue + 
                    "\nScore Display Type: " + a + 
                    "\nMouse particle count: " + data.mouseParticles + 
                    "\nShowing FPS: " + data.isShowingFPS + "\n\n";

    }
        
    void DisplayVideoInfo()
    {
        gui.text += "Screen Full Screen: " + Screen.fullScreen +
                    "\nConnected Displays: " + Display.displays.Length + "\n\n";
    }


    void DisplayInputInfo()
    {
        gui.text += "Touch Support: " + Input.touchSupported +
                    "\nInput count: " + Input.touchCount + "\n\n";
    }
        

    private string[] qualityLevelNames = new string[]
    {
       "Low", "Medium", "High"
    };
    public string GetQualityLevelName()
    {
        int qualityLevelIndex = QualitySettings.GetQualityLevel();
        if (qualityLevelIndex >= 0 && qualityLevelIndex <= qualityLevelNames.Length)
        {
            return qualityLevelNames[qualityLevelIndex];
        }
        else
        {
            return "Unknown";
        }
    }


}