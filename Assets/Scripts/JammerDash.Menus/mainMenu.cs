﻿using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Audio;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using UnityEngine.Video;
using System.Diagnostics;
using System.IO.Compression;
using Button = UnityEngine.UI.Button;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using JammerDash.Audio;
using JammerDash.Menus.Play;
using SimpleFileBrowser;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;
using JammerDash.Tech;
using JammerDash.Menus.Main;
using System.Xml.Serialization;
using UnityEngine.Localization.Settings;
using JammerDash.Difficulty;

namespace JammerDash.Menus
{
    public class mainMenu : MonoBehaviour, IPointerClickHandler
    {
        public bool areLevelsImported = false;


        public GameObject musicAsset;
        public Image bg;
        public Sprite[] sprite;
        private bool quittingAllowed = false;
        public SettingsData data;
        public float quitTimer = 1f;
        public float quitTime = 0f;
        public Image quitPanel2;
        PlayerData playerData;
        public GameObject accessoverPanel;
        public Text nolevelerror;
        public GameObject[] disableAccess;
        string oldSeconds;
        public Image shuffleImage;
        public Animator idle;
        public Animator notIdle;
        public float afkTime;
        bool hasPlayedIdle;
        [Header("Account System")]
        public InputField usernameInput;
        public InputField password;
        public InputField email;
        public Image countryIMG;
        string cc;
        [Header("Clock")]
        public GameObject hour;
        public GameObject min;
        public GameObject sec;

        [Header("Panels")]
        public GameObject mainPanel;
        public GameObject playPanel;
        public GameObject creditsPanel;
        public GameObject settingsPanel;
        public GameObject additionalPanel;
        public GameObject levelInfo;
        public GameObject quitPanel;
        public GameObject community;
        public GameObject musicPanel;
        public GameObject changelogs;
        public GameObject overPanel;
        public GameObject accPanel;
        public GameObject multiPanel;

        [Header("LevelInfo")]
        public GameObject levelInfoPanelPrefab;
        public Transform levelInfoParent;
        public GameObject levelCreatePanel;
        public InputField song;
        public InputField artists;
        public InputField map;
        public GameObject cubePrefab;
        public GameObject sawPrefab;
        public GameObject playPrefab;
        public Transform playlevelInfoParent;
        public string songName;
        public string mapper;
        public string artist;
        public string path;
        public Text songMP3Name;
        public int levelRow = -1;
      
        [Header("Video Background")]
        public GameObject videoPlayerObject;
        public GameObject videoImage;
        public VideoPlayer videoPlayer;
        public VideoClip[] videoClips;
        private List<string> videoUrls = new List<string>();
        private int currentVideoIndex = 0;

       
        [Header("Music")]
        public AudioMixer audioMixer;
        AudioSource source;
        private float lowpassTargetValue;
        private float fadeDuration = 0.25f;
        private float currentLerpTime = 0f;
        public bool focus = true;

        [Header("Profile")]
        public Slider levelSlider;
        public Text levelText;
        public Text[] usernames;
        public Text spMain;
        public Text[] sps;
        public Text bigStatsText;

        [Header("Parallax")]
        public Transform logo;
        public Transform background;
        public Transform backgroundVideo;
        public float backgroundParallaxSpeed;
        public float maxMovementOffset;
        public float scaleMultiplier;
        public float edgeMargin;

        private Vector3 lastMousePosition;

        private Camera mainCamera;
        private RectTransform canvasRect;

        public Text clock;

        void Start()
        {
            Time.timeScale = 1f;
            AudioManager.Instance.source.pitch = 1f;
            playerData = Account.Instance.LoadData();
            data = SettingsFileHandler.LoadSettingsFromFile();
            Debug.unityLogger.logEnabled = true;
           
                foreach (GameObject go in disableAccess)
                {
                    go.SetActive(true);
                }

            LoadLevelsFromFiles();
            LoadLevelFromLevels();
            StartCoroutine(SetCountry());
            SetSpectrum();
            LoadRandomBackground();
            string path = Path.Combine(Application.persistentDataPath, "levels");
            if (Directory.GetFiles(path, "*.jdl").Length == 0)
            {
                FileBrowser.m_instance = Instantiate(Resources.Load<GameObject>("SimpleFileBrowserCanvas")).GetComponent<FileBrowser>();
                FileBrowser.SetFilters(false, new FileBrowser.Filter("Jammer Dash Level", ".jdl"));
                FileBrowser.SetDefaultFilter("Levels");
                FileBrowser.SetDefaultFilter("Levels");
                FileBrowser.ShowLoadDialog(ImportLevel, null, FileBrowser.PickMode.Files, true, Path.Combine(Application.streamingAssetsPath, "levels"), null, "Import Level...", "Import");
            }

            spMain.text = $"{LocalizationSettings.StringDatabase.GetLocalizedString("lang", "Jams")}: 0" +
                $"\t\t{LocalizationSettings.StringDatabase.GetLocalizedString("lang", "Performance")}: {Mathf.RoundToInt(Difficulty.Calculator.CalculateSP("scores.dat"))}sp" +
                $"\t\t{LocalizationSettings.StringDatabase.GetLocalizedString("lang", "Accuracy")}: {Difficulty.Calculator.CalculateAccuracy("scores.dat"):0.00}%";
            foreach (Text sp in sps)
            {
                sp.text = $"{Difficulty.Calculator.CalculateSP("scores.dat"):0}sp";
            }
        }
        public void SetSpectrum()
        {
            SimpleSpectrum[] spectrums = FindObjectsByType<SimpleSpectrum>(FindObjectsSortMode.None);

            foreach (SimpleSpectrum spectrum in spectrums)
            {
                spectrum.audioSource = AudioManager.Instance.source;
            }
        }
        public string FormatTime(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);

            // Ensure seconds don't go beyond 59
            seconds = Mathf.Clamp(seconds, 0, 59);

            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        public string FormatNumber(long number)
        {
            string formattedNumber;
             
            if (number < 1000)
            {
                formattedNumber = number.ToString();
                return formattedNumber;
            }
            else if (number >= 1000 && number < 1000000)
            {
                formattedNumber = (number / 1000f).ToString("F1") + "K";
                return formattedNumber;
            }
            else if (number >= 1000000 && number < 1000000000)
            {
                formattedNumber = (number / 1000000f).ToString("F2") + "M";
                return formattedNumber;
            }
            else if (number >= 1000000000 && number < 1000000000000)
            {
                formattedNumber = (number / 1000000000f).ToString("F2") + "B";
                return formattedNumber;
            }
            else if (number >= 1000000000000 && number <= 1000000000000000)
            {
                formattedNumber = (number / 1000000000000f).ToString("F2") + "T";
                return formattedNumber;
            }
            else
            {
                formattedNumber = (number / 1000000000000f).ToString("F3") + "Q";
                return formattedNumber;
            }
        }
        public void LoadLevelFromLevels()
        {
            foreach (Transform child in playlevelInfoParent)
            {
                Destroy(child.gameObject);
            }

            string defaultLevelsPath = Path.Combine(Application.streamingAssetsPath, "levels");
            string levelsPath = Path.Combine(Application.persistentDataPath, "levels");

            if (!Directory.Exists(levelsPath))
            {
                UnityEngine.Debug.LogError("The 'levels' folder does not exist in persistentDataPath.");
                Directory.CreateDirectory(levelsPath);
                return;
            }

            string[] levelFiles = Directory.GetFiles(levelsPath, "*.jdl", SearchOption.AllDirectories);
            string[] defaultLevelFiles = Directory.GetFiles(defaultLevelsPath, "*.jdl");

            string[] allLevelFiles = new string[levelFiles.Length + defaultLevelFiles.Length];
            levelFiles.CopyTo(allLevelFiles, 0);
            defaultLevelFiles.CopyTo(allLevelFiles, levelFiles.Length);

            HashSet<int> processedIDs = new HashSet<int>();

            foreach (string filePath in allLevelFiles)
            {
                if (Path.GetFileName(filePath).Equals("LevelDefault.jdl"))
                {
                    continue; // Skip LevelDefault.jdl
                }

                // Create a temporary folder
                string tempFolder = Path.Combine(Application.temporaryCachePath, "tempExtractedJson");
                Directory.CreateDirectory(tempFolder);

                try
                {
                    // Extract JSON data from JDL file to the temporary folder
                    string jsonFilePath = ExtractJSONFromJDL(filePath);

                    if (jsonFilePath == null)
                    {
                        UnityEngine.Debug.LogError("Failed to extract JSON from JDL: " + filePath);
                        continue;
                    }

                    // Read JSON content from the extracted JSON file
                    string json = File.ReadAllText(jsonFilePath);

                    // Deserialize JSON data into SceneData object
                    SceneData sceneData = SceneData.FromJson(json);

                    if (sceneData == null)
                    {
                        UnityEngine.Debug.LogError("Failed to deserialize JSON from file: " + jsonFilePath);
                        continue;
                    }

                    // Check if the sceneData.ID has already been processed
                    if (processedIDs.Contains(sceneData.ID))
                    {
                        continue; // Skip duplicate levels
                    }

                    // Add the ID to the processed set
                    processedIDs.Add(sceneData.ID);

                    // Log the level name to verify if sceneData is successfully deserialized
                    UnityEngine.Debug.LogWarning(sceneData.sceneName);

                    // Create a directory with the level name
                    string extractedPath = Path.Combine(levelsPath, "extracted", sceneData.ID + " - " + sceneData.sceneName);
                    Directory.CreateDirectory(extractedPath);

                    // Move the JSON file to the new directory
                    string jsonDestinationPath = Path.Combine(extractedPath, sceneData.sceneName + ".json");
                    if (File.Exists(jsonDestinationPath))
                    {
                        File.Delete(jsonDestinationPath);
                    }
                    File.Move(jsonFilePath, jsonDestinationPath);
                    ExtractOtherFromJDL(filePath, extractedPath);
                    GameObject levelInfoPanel = Instantiate(playPrefab, playlevelInfoParent);
                    // Display level information on UI
                    DisplayCustomLevelInfo(sceneData, levelInfoPanel.GetComponent<CustomLevelScript>());
                    levelInfoPanel.GetComponent<CustomLevelScript>().SetSceneData(sceneData);
                }
                finally
                {
                    // Clean up the temporary folder
                    Directory.Delete(tempFolder, true);
                }
            }
        }
        public void Import()
        {
            FileBrowser.m_instance = Instantiate(Resources.Load<GameObject>("SimpleFileBrowserCanvas")).GetComponent<FileBrowser>();
            FileBrowser.SetFilters(false, new FileBrowser.Filter("Jammer Dash Level", ".jdl"));
            FileBrowser.SetDefaultFilter("Music");
            FileBrowser.ShowLoadDialog(ImportLevel, null, FileBrowser.PickMode.Files, true, null, null, "Import Level...", "Import");
        }

        void ImportLevel(string[] paths)
        {
            if (paths.Length >= 0)
            {
                foreach (string path in paths)
                {
                    File.Move(path, Path.Combine(Application.persistentDataPath, "levels", Path.GetFileName(path)));
                }
            }

            LoadLevelFromLevels();
        }
        public void Accounts()
        {
            accPanel.SetActive(!accPanel.activeSelf);
        }

        public void SaveAcc()
        {
            Account.Instance.Apply(usernameInput.text, password.text, email.text, cc);
        }

        public static string ExtractJSONFromJDL(string jdlFilePath)
        {
            try
            {
                if (!File.Exists(jdlFilePath))
                {
                    UnityEngine.Debug.LogError("File does not exist: " + jdlFilePath);
                    return null;
                }

                UnityEngine.Debug.Log("Attempting to open JDL file: " + jdlFilePath);
                UnityEngine.Debug.Log("File size: " + new FileInfo(jdlFilePath).Length + " bytes");

                if (!IsZipFile(jdlFilePath))
                {
                    UnityEngine.Debug.LogError("The file is not a valid ZIP archive: " + jdlFilePath);
                    return null;
                }

                string tempFolder = Path.Combine(Application.temporaryCachePath, "tempExtractedJson");
                Directory.CreateDirectory(tempFolder);

                using (ZipArchive archive = ZipFile.OpenRead(jdlFilePath))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (!entry.FullName.EndsWith(".json"))
                        {
                            continue;
                        }

                        string extractedFilePath = Path.Combine(tempFolder, Path.GetFileName(entry.FullName));
                        entry.ExtractToFile(extractedFilePath, true);
                        return extractedFilePath;
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("Error extracting JSON from JDL: " + e.Message);
            }

            return null;
        }

        private static bool IsZipFile(string filePath)
        {
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var zipStream = new ZipArchive(fileStream, ZipArchiveMode.Read))
                    {
                        return zipStream.Entries.Count > 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }


        public static void ExtractOtherFromJDL(string jdlFilePath, string destinationFilePath)
        {
            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(jdlFilePath))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string entryFileName = entry.FullName;

                        if (entryFileName.EndsWith(".mp3") || entryFileName.EndsWith(".png"))
                        {
                            // Combine the destination directory path with the MP3 filename
                            string destinationFullPath = Path.Combine(destinationFilePath, Path.GetFileName(entryFileName));

                            // Extract the MP3 file to the specified destination file path
                            entry.ExtractToFile(destinationFullPath, overwrite: true);
                            return; // Exit the method after extracting the MP3 file
                        }
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("Error extracting Audio from JDL: " + e.Message);
            }
        }

      
        public void URL(string url)
        {
            Application.OpenURL(url);
        }
        public void LoadRandomBackground()
        {
            UnityEngine.Debug.Log(data);

            switch (data.backgroundType)
            {
                case 1:
                    sprite = Resources.LoadAll<Sprite>("backgrounds/default");
                    if (videoImage != null)
                    {
                        videoPlayerObject.SetActive(false);
                        videoImage.SetActive(false);
                    }
                    if (DateTime.Now.Month == 12)
                    {
                        sprite = Resources.LoadAll<Sprite>("backgrounds/christmas");
                        if (videoImage != null)
                        {
                            videoPlayerObject.SetActive(false);
                            videoImage.SetActive(false);
                        }
                    }
                    else if (DateTime.Now.Month == 2 && DateTime.Now.Day == 14)
                    {
                        sprite = Resources.LoadAll<Sprite>("backgrounds/valentine");
                        if (videoImage != null)
                        {
                            videoPlayerObject.SetActive(false);
                            videoImage.SetActive(false);
                        }
                    }
                    break;
                case 2:
                    // Implement server-side seasonal backgrounds

                    break;
                case 3:
                    _ = LoadCustomBackgroundAsync();
                    break;
                case 4:
                    videoPlayerObject.SetActive(true);
                    videoImage.SetActive(true);    
                    string videoDirectory = Path.Combine(Application.persistentDataPath, "backgrounds");
                        List<string> validVideoFiles = GetValidVideoFiles(videoDirectory, 250 * 1024 * 1024); // 250MB limit
                        if (validVideoFiles.Count == 0)
                        {
                            Debug.LogWarning("No valid video files found within size constraints.");
                            break;
                        }

                        // Assign video clips or URLs to the video player
                        foreach (string file in validVideoFiles)
                        {
                            videoUrls.Add(file);
                            Debug.Log("Loading video: " + file);
                            AddVideoToPlayer(file);
                        }
                        break;
                default:
                    sprite = Resources.LoadAll<Sprite>("backgrounds/basic");
                    if (videoPlayerObject != null)
                    {
                        videoPlayerObject.SetActive(false);
                        videoImage.SetActive(false);
                    }
                    break;
            }
           
                bg.color = Color.white;
            if (sprite.Length > 0)
            {
                int randomIndex = (sprite.Length == 1) ? 0 : Random.Range(0, sprite.Length);
                bg.sprite = sprite[randomIndex];
            }
           


        }
        private async Task LoadCustomBackgroundAsync()
        {
            string path = Application.persistentDataPath + "/backgrounds";

            // Supported image file types in Unity
            string[] supportedExtensions = new string[] { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.tga" };

            // Gather all files with supported extensions
            List<string> files = new List<string>();
            foreach (var extension in supportedExtensions)
            {
                files.AddRange(Directory.GetFiles(path, extension, SearchOption.AllDirectories));
            }

            if (files.Count == 0)
            {
                Debug.LogWarning("No image files found in the directory: " + path);
                return;
            }

            // Choose a random file path
            string randomFilePath = files[UnityEngine.Random.Range(0, files.Count)];
            await LoadSpriteAsync(randomFilePath);
        }


        private async Task LoadSpriteAsync(string filePath)
        {
            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file://" + filePath))
            {
                await uwr.SendWebRequest();

                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Failed to download texture: {uwr.error}");
                    return;
                }

                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                // Set the loaded sprite
                SetBackgroundSprite(sprite);
            }
        }
       
        public IEnumerator SetCountry()
        {
            string path = Application.persistentDataPath + "/playerData.dat";

            if (File.Exists(path))
            {
                XmlSerializer formatter = new XmlSerializer(typeof(PlayerData));
                FileStream stream = new FileStream(path, FileMode.Open);

                PlayerData data = formatter.Deserialize(stream) as PlayerData;
                stream.Close();

                
                cc = data.country;
            }
            if (cc == null)
            {

                string ip = new System.Net.WebClient().DownloadString("https://api.ipify.org");
                string uri = $"https://ipapi.co/{ip}/json/";


                using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
                {
                    yield return webRequest.SendWebRequest();

                    string[] pages = uri.Split('/');
                    int page = pages.Length - 1;

                    IpApiData ipApiData = IpApiData.CreateFromJSON(webRequest.downloadHandler.text);

                    cc = ipApiData.country.ToLower();
                    countryIMG.sprite = Resources.Load<Sprite>("icons/countries/" + cc);

                }
            }
            else
            {
                cc = Account.Instance.cc;
                countryIMG.sprite = Resources.Load<Sprite>("icons/countries/" + cc);
            }
        }
        private void SetBackgroundSprite(Sprite sprite)
        {
            if (videoPlayerObject != null)
            {
                videoPlayerObject.SetActive(false);
                videoImage.SetActive(false);
            }

            this.sprite = new Sprite[1];
            this.sprite[0] = sprite;
            Resources.UnloadUnusedAssets();
        }
        private List<string> GetValidVideoFiles(string directory, long maxSizeBytes)
        {
            List<string> validFiles = new List<string>();

            if (Directory.Exists(directory))
            {
                string[] files = Directory.GetFiles(directory, "*.mp4");
                foreach (string file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    if (fileInfo.Length <= maxSizeBytes)
                    {
                        validFiles.Add(file);
                    }
                    else
                    {
                        Debug.LogError($"Video file size exceeds the limit ({maxSizeBytes / (1024 * 1024)}MB): {file}");
                    }
                }
            }
            else
            {
                Debug.LogError("Directory does not exist: " + directory);
            }

            return validFiles;
        }

        private void AddVideoToPlayer(string filePath)
        {
            string fileUrl = "file://" + filePath;

            if (videoPlayer != null)
            {
                videoPlayer.url = fileUrl; // This assumes a single video at a time.
                videoPlayer.Prepare();
                videoPlayer.prepareCompleted += OnVideoPrepareCompleted;
            }
            else
            {
                Debug.LogError("VideoPlayer is not assigned.");
            }
        }
        private void PlayVideoAtIndex(int index)
        {
            if (videoUrls.Count == 0) return;

            currentVideoIndex = index % videoUrls.Count;
            videoPlayer.url = videoUrls[currentVideoIndex];
            videoPlayer.isLooping = true;

            if (!isPrepareEventAttached)
            {
                videoPlayer.prepareCompleted += OnVideoPrepareCompleted;
                isPrepareEventAttached = true;
            }

            videoPlayer.Prepare();
        }

        private bool isPrepareEventAttached = false;

        private void OnVideoPrepareCompleted(VideoPlayer vp)
        {
            Debug.Log("Video prepared. Starting playback.");
            vp.Play();
        }

       
        public float elapsedTime;
      
        public void CreatePanel()
        {
            levelCreatePanel.SetActive(true);
        }

        public void CreatePanelClose()
        {
            levelCreatePanel.SetActive(false);
        }
        public void CreateNewLevel()
        {
            float newDifficulty = 0;

            songName = song.text;
            artist = artists.text;
            mapper = map.text;
            SceneData newLevelData = new SceneData
            {
                sceneName = songName,
                levelName = songName,
                calculatedDifficulty = newDifficulty,
                songName = songName,
                artist = artist,
                creator = mapper,
                ID = Random.RandomRange(int.MinValue, int.MaxValue)
            };

            // Save the new level data to a JSON file
            SaveLevelToFile(newLevelData);


            // Reload levels to update the UI with the new level
            LoadLevelsFromFiles();
        }


        void SaveLevelToFile(SceneData sceneData)
        {
            // Convert SceneData to JSON
            string json = sceneData.ToJson();

            // Save JSON to a new file in the persistentDataPath + scenes & levels folder
            string namepath = Path.Combine(Application.persistentDataPath, "scenes", sceneData.ID + " - " + sceneData.levelName);
            string filePath = Path.Combine(namepath, $"{sceneData.levelName}.json");
            string musicPath = Path.Combine(namepath, $"{sceneData.artist} - {sceneData.songName}.mp3");
            if (Directory.Exists(namepath))
            {
                songMP3Name.text = "A level with this name currently exists. Try something else or delete the level.";
            }
            else
            {
                Directory.CreateDirectory(namepath);
                File.WriteAllText(filePath, json);
                File.Copy(path, Path.Combine(Application.persistentDataPath, "scenes", sceneData.ID + " - " + sceneData.levelName, $"{artist} - {songName}.mp3"));
            }

        }

        public void LoadLevelsFromFiles()
        {
            // Clear existing level information panels
            foreach (Transform child in levelInfoParent)
            {
                Destroy(child.gameObject);
            }
            string levelsPath = Path.Combine(Application.persistentDataPath, "scenes");

            if (!Directory.Exists(levelsPath))
            {
                UnityEngine.Debug.LogError("The 'scenes' folder does not exist in persistentDataPath.");
                Directory.CreateDirectory(levelsPath);
                return;
            }

            string[] levelFiles = Directory.GetFiles(levelsPath, "*.json", SearchOption.AllDirectories);

            foreach (string filePath in levelFiles)
            {
                string json = File.ReadAllText(filePath);
                SceneData sceneData = SceneData.FromJson(json);

                // Instantiate the level information panel prefab
                GameObject levelInfoPanel = Instantiate(levelInfoPanelPrefab, levelInfoParent);

                // Display level information on UI
                DisplayLevelInfo(sceneData, levelInfoPanel.GetComponent<LevelScript>());
                levelInfoPanel.GetComponent<LevelScript>().SetSceneData(sceneData);
            }
        }

        private SceneData LoadSceneData(string filePath)
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                SceneData sceneData = SceneData.FromJson(json);
                return sceneData;
            }
            else
            {
                UnityEngine.Debug.LogWarning("Scene data file not found: " + filePath);
                return null;
            }
        }

        private void InstantiateObjects(SceneData sceneData)
        {
            // Instantiate cubes and saws based on sceneData
            foreach (Vector3 cubePos in sceneData.cubePositions)
            {
                Instantiate(cubePrefab, cubePos, Quaternion.identity);
            }

            foreach (Vector3 sawPos in sceneData.sawPositions)
            {
                Instantiate(sawPrefab, sawPos, Quaternion.identity);
            }

            // Other initialization logic for objects
        }

        // Display level information on UI
        void DisplayLevelInfo(SceneData sceneData, LevelScript level)
        {

            // Check if LevelScript component is not null
            if (level != null)
            {
                UnityEngine.Debug.Log(sceneData);
                // Assuming YourPanelScript has methods to set text or image properties
                level.SetLevelName($"{sceneData.sceneName}");
                level.SetSongName($"♫ {sceneData.artist} - {sceneData.songName}");
                level.SetDifficulty($"{sceneData.calculatedDifficulty:0.00} sn");

            }
            else
            {
                // Logging for debugging purposes
                UnityEngine.Debug.LogError("Failed to load level " + sceneData.ID);
            }

        }
        public void LoadCustomMusic()
        {
            FileBrowser.m_instance = Instantiate(Resources.Load<GameObject>("SimpleFileBrowserCanvas")).GetComponent<FileBrowser>();
            FileBrowser.SetFilters(false, new FileBrowser.Filter("Music", ".mp3"));
            FileBrowser.SetDefaultFilter("Music");
            FileBrowser.ShowLoadDialog(SongSelected, null, FileBrowser.PickMode.Files, false, null, null, "Load Local song...", "Choose");
        }
        void SongSelected(string[] paths)
        {
            if (paths.Length == 1)
            {
                path = paths[0];
                songMP3Name.text = path;
            }

            else
            {
                songMP3Name.text = "You can't have more than one song selected.";
            }
        }


        void DisplayCustomLevelInfo(SceneData sceneData, CustomLevelScript level)
        {

            // Check if LevelScript component is not null
            if (level != null)
            {
                // Assuming YourPanelScript has methods to set text or image properties
                level.SetLevelName($"{sceneData.sceneName}");
                level.SetInfo($"♫ {sceneData.artist} - {sceneData.songName} // {LocalizationSettings.StringDatabase.GetLocalizedString("lang", "mapped by")} {sceneData.creator}");
            }
            else
            {
                // Logging for debugging purposes
                UnityEngine.Debug.LogError("Failed to load level " + sceneData.ID);
            }

            // Method to toggle menu panels
        }

        // Method to toggle menu panels
        private void ToggleMenuPanel(GameObject panel)
        {
            // Toggle the specified panel directly if it's changelogs or creditsPanel
            if (panel == changelogs || panel == creditsPanel || panel == community)
            {
                panel.SetActive(!panel.activeSelf);

                if (panel.active)
                {
                    // Turn off all panels
                    mainPanel.SetActive(false);
                    playPanel.SetActive(false);
                    creditsPanel.SetActive(false);
                    settingsPanel.SetActive(false);
                    musicPanel.SetActive(false);
                    levelInfo.SetActive(false);
                    community.SetActive(false);
                    changelogs.SetActive(false);
                    additionalPanel.SetActive(false);
                    multiPanel.SetActive(false);
                   
                }
                if (!panel.active)
                {
                    panel.SetActive(true);
                }

            }
            else if (panel == settingsPanel)
            {
                settingsPanel.SetActive(!panel.activeSelf);
            }
            else
            {
                // Turn off all panels
                mainPanel.SetActive(false);
                playPanel.SetActive(false);
                creditsPanel.SetActive(false);
                settingsPanel.SetActive(false);
                musicPanel.SetActive(false);
                levelInfo.SetActive(false);
                community.SetActive(false);
                changelogs.SetActive(false);
                additionalPanel.SetActive(false);
                multiPanel.SetActive(false);


                // Enable the specified panel if it's not null
                if (panel != null)
                {
                    AudioManager.Instance.source.loop = false;
                    panel.SetActive(true);
                }
                // Enable mainPanel if none of the specific panels are active
                else if (!playPanel.activeSelf || !creditsPanel.activeSelf || !settingsPanel.activeSelf)
                {
                    mainPanel.SetActive(true);
                    LoadRandomBackground();
                    AudioManager.Instance.source.loop = false;
                }

                if (playPanel.activeSelf)
                {
                    ButtonClickHandler[] levels = FindObjectsOfType<ButtonClickHandler>();

                    foreach (ButtonClickHandler level in levels)
                    {
                        if (level.isSelected)
                        {
                            levelRow = Array.IndexOf(levels, level);
                            level.Change();
                            AudioManager.Instance.source.loop = true;
                        }
                        else
                        {
                            AudioManager.Instance.source.loop = true;
                            if (levelRow == -1)
                            {
                                int randomIndex = Random.Range(0, levels.Length);
                                levelRow = randomIndex;
                                new WaitUntil(() => levels[randomIndex].GetComponent<leaderboard>().panelContainer != null);
                                StartCoroutine(levels[randomIndex].PlayAudioOnlyMode());
                                levels[randomIndex].Change();
                                
                            }
                            else
                            {
                                new WaitUntil(() => levels[levelRow].GetComponent<leaderboard>().panelContainer != null);
                                levels[levelRow].Change();
                                StartCoroutine(levels[levelRow].Move(1));
                                if (AudioManager.Instance.source.clip.name != $"{levels[levelRow].GetComponent<CustomLevelScript>().sceneData.artist} - {levels[levelRow].GetComponent<CustomLevelScript>().sceneData.songName}")
                                    StartCoroutine(levels[levelRow].PlayAudioOnlyMode());
                            }
                        }
                    }
                }
                else
                {

                }
            }
        }

        // Menu buttons
        public void Home()
        {
            ToggleMenuPanel(mainPanel);
        }
        public void Play()
        {
            AudioManager.Instance.source.loop = true;
            ToggleMenuPanel(playPanel);
            InputSystem.pollingFrequency = 1000;
            InputSystem.settings.maxQueuedEventsPerUpdate = 1000;
        }

        public void Multi()
        {
            ToggleMenuPanel(multiPanel);
        }

        public void PlayRandomSFX()
        {
            UnityEngine.Object[] clips = Resources.LoadAll("Audio/SFX");
            foreach (var obj in clips)
            {
                UnityEngine.Debug.Log("Loaded object: " + obj.name);
            }
            FindObjectOfType<AudioSource>().PlayOneShot((AudioClip)clips[Random.Range(0, clips.Length)]);

        }
        public void Credits()
        {
            ToggleMenuPanel(creditsPanel);
        }

        public void Settings()
        {
            ToggleMenuPanel(settingsPanel);
        }

        public void OpenMusic()
        {
            musicPanel.SetActive(!musicPanel.activeSelf);
        }

        public void OpenChangelogs()
        {
            ToggleMenuPanel(changelogs);
        }

        public void AdditionalOpen()
        {
            additionalPanel.SetActive(true);
        }

        public void AdditionalClose()
        {
            additionalPanel.SetActive(false);
        }

        public void Quit()
        {
            StartCoroutine(QuitGame());
        }

        IEnumerator QuitGame()
        {
            ShowQuit();
            yield return null;
        }


        public void ShowQuit()
        {
            quitPanel.SetActive(true);


            Application.wantsToQuit += QuitHandler;
            PostProcessVolume volume = FindFirstObjectByType<PostProcessVolume>();
            if (volume != null)
            {
                PostProcessProfile profile = volume.profile;
                Vignette vignette;
                if (profile.TryGetSettings(out vignette))
                {
                    vignette.intensity.value = 0.75f;
                }
                else
                {
                    // Settings couldn't be retrieved
                    UnityEngine.Debug.LogWarning("Vignette settings not found in the Post Process Profile.");
                }
            }
            else
            {
                // PostProcessVolume not found in the scene
                UnityEngine.Debug.LogWarning("Post Process Volume not found in the scene.");
            }
        }

        // Call this method when hiding the quit panel
        public void HideQuit()
        {
            quitPanel.SetActive(false);

            Application.wantsToQuit -= QuitHandler;
            PostProcessVolume volume = FindFirstObjectByType<PostProcessVolume>();
            if (volume != null)
            {
                PostProcessProfile profile = volume.profile;
                Vignette vignette;
                if (profile.TryGetSettings(out vignette))
                {
                    vignette.intensity.value = 0f;
                }
                else
                {
                    // Settings couldn't be retrieved
                    UnityEngine.Debug.LogWarning("Vignette settings not found in the Post Process Profile.");
                }
            }
            else
            {
                // PostProcessVolume not found in the scene
                UnityEngine.Debug.LogWarning("Post Process Volume not found in the scene.");
            }
        }

        // Coroutine for fading the Lowpass filter
        private IEnumerator FadeLowpass()
        {
            SettingsData data = SettingsFileHandler.LoadSettingsFromFile();

            lowpassTargetValue = data.lowpassValue;
            float timer = 0f;
            float startValue = lowpassTargetValue;  // Initial value based on fade in or out

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float t = timer / fadeDuration;

                // Lerp between start and target values
                float currentValue = Mathf.Lerp(startValue, lowpassTargetValue, t);

                // Set the Lowpass filter parameter on the Master AudioMixer
                audioMixer.SetFloat("Lowpass", currentValue);

                yield return null;
            }

            // Ensure the target value is set after the fade completes
            audioMixer.SetFloat("Lowpass", lowpassTargetValue);
        }


      



        private bool QuitHandler()
        {
            if (quittingAllowed)
            {

                return true; // Return true to allow quitting
            }
            else
            {

                return false; // Return false to prevent quitting
            }
        }

        public void AllowQuitting()
        {
            quittingAllowed = true;
            Application.Quit();
            UnityEngine.Debug.Log("Quitting the application");
        }

        public void CancelQuitting()
        {
            quittingAllowed = false;
            HideQuit();
            UnityEngine.Debug.Log("User canceled quitting");
            // Set the Lowpass filter parameter on the Master AudioMixer
            audioMixer.SetFloat("Lowpass", 22000);
        }



        public void Menu()
        {
            AudioManager.Instance.source.loop = false;
            StartCoroutine(AudioManager.Instance.ChangeSprite());
            settingsPanel.SetActive(false);
            creditsPanel.SetActive(false);
            playPanel.SetActive(false);
            community.SetActive(false);
            mainPanel.SetActive(true);
            multiPanel.SetActive(false);
        }


        // Play section

        public void CommunityOpen()
        {
            ToggleMenuPanel(community);
        }


        public void OpenInfo()
        {
            levelInfo.SetActive(true);
            mainPanel.SetActive(false);
        }

        public void CloseInfo()
        {
            mainPanel.SetActive(true);
            levelInfo.SetActive(false);
        }

        public void OpenEditor()
        {
            SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
        }

        public void OpenLevel(int index)
        {
            SceneManager.LoadScene(index);
        }

        #region SOCIAL_MEDIA
        public void Discord()
        {
            Application.OpenURL("https://discord.gg/dJU8X2kDpn");
        }

        public void RegionDiscord(string ID)
        {
            Application.OpenURL($"https://discord.gg/{ID}");
        }

        public void Twitter()
        {
            Application.OpenURL("https://twitter.com/JammerDash");
        }

        public void YouTube()
        {
            Application.OpenURL("https://www.youtube.com/@Jammer_Dash");
        }

        public void Newgrounds()
        {
            Application.OpenURL("https://www.newgrounds.com/bbs/topic/1530441");
        }

        public void Twitch()
        {
            Application.OpenURL("https://twitch.tv/prickletylive");
        }

        public void TikTok()
        {
            Application.OpenURL("https://tiktok.com/@pricklety");
        }

        public void Instagram()
        {
            Application.OpenURL("https://www.instagram.com/pricklety/");
        }


        #endregion

        public void CrashReports()
        {
            // File path of the player log
            string logFilePath = Application.persistentDataPath + "/Player.log";

            // Open the player log file using the default application associated with its file type
            Process.Start(logFilePath);
        }

        public void SettingsFile()
        {
            // File path of the player log
            string logFilePath = Application.persistentDataPath + "/settings.json";

            // Open the player log file using the default application associated with its file type
            Process.Start(logFilePath);
        }


        bool IsPointerOverUIButNotButton()
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            foreach (RaycastResult result in results)
            {
                // Check if the object under the pointer is a UI button
                if (result.gameObject.GetComponent<Button>() != null || result.gameObject.GetComponent<Dropdown>() != null || result.gameObject.GetComponent<Slider>() != null)
                {
                    return true;
                }
            }
            return false;
        }
        string FormatElapsedTime(float elapsedTime)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(elapsedTime);
            return string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        }

        public void Shuffle()
        {
            AudioManager.Instance.shuffle = !AudioManager.Instance.shuffle;
        }
        public void FixedUpdate()
        {
            if (AudioManager.Instance.shuffle)
            {
                shuffleImage.color = Color.HSVToRGB(0.33f, 0.47f, 1);
            }
            else
            {
                shuffleImage.color = Color.white;
            }
            string timeInfo = DateTime.Now.ToString("hh:mm:ss tt") + "\n";
            if (FindObjectOfType<GameTimer>() != null)
            {
                timeInfo += "running " + FormatElapsedTime(GameTimer.GetRunningTime());
            }
            clock.text = timeInfo;

            foreach (Text text in usernames)
            {
                if (string.IsNullOrEmpty(Account.Instance.username))
                {
                    text.text = "Guest";
                }
                else
                {
                    text.text = Account.Instance.username;
                }

            }

           
            
            if (Account.Instance.currentXP >= 0)
            {
                levelText.text = "Level: " + Account.Instance.level.ToString() + $" (XP: {FormatNumber(Account.Instance.totalXP)})";

            }
            PlayerStats stats = Calculator.CalculateOtherPlayerInfo("scores.dat");
            bigStatsText.text = $"Playtime: {Account.Instance.ConvertPlaytimeToReadableFormat()}\r\n" +
                $"Play count: {stats.TotalPlays:N0}\r\n" +
                $"SS+: {stats.RankCounts["SS+"]:N0}\r\n" +
                $"SS: {stats.RankCounts["SS"]:N0}\r\n" +
                $"S: {stats.RankCounts["S"]:N0}\r\n" +
                $"A: {stats.RankCounts["A"]:N0}\r\n" +
                $"Highest combo: {stats.HighestCombo:N0}x\r\n" +
                $"Accuracy: {Calculator.CalculateAccuracy("scores.dat"):0.00}%";



            if (quitPanel.activeSelf)
            {
                // Set the Lowpass filter parameter on the Master AudioMixer
                audioMixer.SetFloat("Lowpass", data.lowpassValue);
            }
            else
            {
                // Set the Lowpass filter parameter on the Master AudioMixer
                audioMixer.SetFloat("Lowpass", 22000);
            }
            data = SettingsFileHandler.LoadSettingsFromFile();
            if (data.parallax && (Screen.fullScreenMode == FullScreenMode.FullScreenWindow || Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen || Application.isEditor))
            {

                // Background parallax effect
                background.localScale = new Vector3(scaleMultiplier, scaleMultiplier, 1);
                backgroundVideo.localScale = background.localScale;

                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3 mouseDelta = new Vector3(-mouseWorldPos.x / 1.5f, -mouseWorldPos.y / 30, 0);

                float cameraMovement = Mathf.Clamp(mouseDelta.x, -maxMovementOffset, maxMovementOffset) * backgroundParallaxSpeed * Time.unscaledDeltaTime;
                Vector3 backgroundOffset = new Vector3(cameraMovement, 0, 0);
                background.position = backgroundOffset + new Vector3(mouseDelta.x / 100, mouseDelta.y, mouseDelta.z);
                backgroundVideo.position = background.position;
            }
            else
            {
                background.localScale = Vector3.one;
                background.position = Vector3.zero;
                logo.position = new Vector3(-4.2f, 0, 0);
            }  
           
            if (Input.GetKey(KeyCode.Escape))
            {

                // Check if quitTime exceeds quitTimer
                if (quitTime >= quitTimer && !quitPanel.activeSelf)
                {
                    audioMixer.SetFloat("Lowpass", 500);
                    quitPanel2.color = new Color(quitPanel2.color.r, quitPanel2.color.g, quitPanel2.color.b, 1.0f); // Set color to fully opaque
                    Application.Quit(); // Quit application
                }
                else if (quitTime < quitTimer && !quitPanel.activeSelf)
                {
                    UnityEngine.Debug.Log(quitTime);
                    quitTime += Time.unscaledDeltaTime;
                    quitPanel2.color = Color.Lerp(new Color(0, 0, 0, 0), new Color(0, 0, 0, 1), quitTime / quitTimer);
                    float startValue = 22000;
                    float currentValue = Mathf.Lerp(startValue, 500, quitTime / quitTimer);

                    audioMixer.SetFloat("Lowpass", currentValue);

                }


            }
            else
            {

                quitTime = 0f;
                quitPanel2.color = new Color(quitPanel2.color.r, quitPanel2.color.g, quitPanel2.color.b, 0f); // Set color to fully transparent
                audioMixer.SetFloat("Lowpass", 22000);
            }

            if ((quitTime < quitTimer || quitTime >= quitTimer) && quitPanel.activeSelf)
            {
                audioMixer.SetFloat("Lowpass", data.lowpassValue);
            }

            if (mainPanel.activeSelf && AudioManager.Instance.source.loop)
            {
                AudioManager.Instance.source.loop = false;
            }
            if (playlevelInfoParent.childCount == 0)
            {
                nolevelerror.gameObject.SetActive(true);
                LoadLevelFromLevels();
            }
            else
            {
                nolevelerror.gameObject.SetActive(false);

            }
        }
      
        void Update()
        {
            levelSlider.maxValue = 1;
            levelSlider.value = (float)Account.Instance.currentXP / (float)Account.Instance.xpRequiredPerLevel[Account.Instance.level];
            string seconds = System.DateTime.Now.ToLocalTime().ToString("ss");
            SimpleSpectrum[] spectrums = FindObjectsByType<SimpleSpectrum>(FindObjectsSortMode.None);
            AudioManager.Instance.levelIndex = levelRow;
            foreach (SimpleSpectrum spectrum in spectrums)
            {
                if (spectrum.audioSource == null)
                {
                    SetSpectrum();
                }
            }
           
            if (Input.GetKeyDown(KeyCode.B) && (EventSystem.current.currentSelectedGameObject == null || EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() == null))
            {
                LoadRandomBackground();
            }

            if ((Input.GetAxis("Mouse X") == 0 || Input.GetAxis("Mouse Y") == 0) && mainPanel.activeSelf)
            {
                afkTime += Time.unscaledDeltaTime;

                if (afkTime > 25f && (!settingsPanel.activeSelf && !accPanel.activeSelf && !additionalPanel.activeSelf))
                {
                    if (!hasPlayedIdle)
                    {
                        ToggleMenuPanel(mainPanel);
                        Cursor.visible = false;
                        idle.PlayInFixedTime("Idle");
                        hasPlayedIdle = true;
                    }
                }
                if (afkTime > 25f && (settingsPanel.activeSelf || accPanel.activeSelf || additionalPanel.activeSelf))
                {
                    Cursor.visible = true;
                    notIdle.PlayInFixedTime("notIdle");
                    hasPlayedIdle = false;
                }
                else if (afkTime < 25f)
                {
                    Cursor.visible = true;
                }
            }
            else
            {

                Cursor.visible = true;
                notIdle.PlayInFixedTime("notIdle");
                hasPlayedIdle = false;
                afkTime = 0f;
                
            }


                if (seconds != oldSeconds)
            {
                UpdateTimer();
                oldSeconds = seconds;
            } 
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleMenuPanel(mainPanel);
            }
            
            if (Input.GetKeyDown(KeybindingManager.reloadData))
            {
                LoadLevelFromLevels();
                LoadLevelsFromFiles();
                
                Notifications.instance.Notify($"Level list reloaded. \n{Directory.GetFiles(Path.Combine(Application.persistentDataPath, "levels"), "*.jdl").Count()} levels total", null);
            }
            if (Input.GetKeyDown(KeybindingManager.debug))
            {
                additionalPanel.SetActive(!additionalPanel.active);
            }

            
        }

        void UpdateTimer()
        {
            int secInt = int.Parse(DateTime.UtcNow.ToLocalTime().ToString("ss"));
            int minInt = int.Parse(DateTime.UtcNow.ToLocalTime().ToString("mm"));
            int hourInt = int.Parse(DateTime.UtcNow.ToLocalTime().ToString("hh"));
            Debug.Log($"{secInt}, {minInt}, {hourInt}");

            sec.transform.localRotation = Quaternion.Euler(0, 0, -secInt * 6);
            min.transform.localRotation = Quaternion.Euler(0, 0, -minInt * 6);
            hour.transform.localRotation = Quaternion.Euler(0, 0, -hourInt * 30);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Check if the clicked position is outside the panel
            RectTransform rectTransform = musicPanel.GetComponent<RectTransform>();
            if (!RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition))
            {
                // Click is outside the panel, disable it
                musicPanel.SetActive(false);
            }
        }
    }

    [Serializable]
    public class IpApiData
    {
        public string country;

        public static IpApiData CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<IpApiData>(jsonString);
        }
    }


}