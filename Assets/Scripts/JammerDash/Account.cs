using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Xml.Serialization;
using Newtonsoft.Json.Linq;
using JammerDash.Difficulty;
using Newtonsoft.Json;
using System.Globalization;
using System.Linq;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace JammerDash
{
    public class Account : MonoBehaviour
    {
        [Header("Account info")]
        public string uuid;
        public string nickname;
        public string username;
        public string user;
        public string email;
        public string cc;
        public string url;
        string token;

        [Header("Level")]
        public int level = 0;
        public long currentXP = 0;
        public long[] xpRequiredPerLevel;
        public long totalXP = 0;

        [Header("Local data")]
        public PlayerData p;

        [Header("Internet Check")]
        public GameObject checkInternet;

        [Header("Playtime")]
        public float playtime;
        public static Account Instance { get; private set; }

        public bool loggedIn;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogWarning("Duplicate instance of Account found. Destroying the new one.");
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            CalculateXPRequirements();
            LoadData();
            StartCoroutine(SavePlaytimeEverySecond());
            LoginData(); // Try to load login data only once at the start
        }

        public void GainXP(long amount)
        {
            currentXP += amount;

            // Update totalXP by reading the scores.dat file and summing every 5th entry
            totalXP = CalculateTotalXPFromFile();

            // Check if the player has enough XP to level up
            if (currentXP >= xpRequiredPerLevel[level])
            {
                LevelUp();
                SaveLocalData();
            }
            else
            {
                SaveLocalData();
            }
        }
        private bool IsFileInUse(string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    // File is available for read/write
                    return false;
                }
            }
            catch (IOException)
            {
                // File is in use
                return true;
            }
        }

        private static readonly object fileLock = new object();
        private void SaveLocalData()
        {
            lock (fileLock) // Ensure thread safety
            {
                try
                {
                    string playerDataPath = Path.Combine(Application.persistentDataPath, "playerData.dat");
                    if (!IsFileInUse(playerDataPath))
                    {
                        PlayerData data = new PlayerData
                        {
                            username = username,
                            level = level,
                            currentXP = currentXP,
                            country = cc,
                            isLocal = true,
                            isOnline = true,
                            sp = Calculator.CalculateSP("scores.dat"),
                            playCount = Calculator.CalculateOtherPlayerInfo("scores.dat").TotalPlays
                        };

                        XmlSerializer formatter = new XmlSerializer(typeof(PlayerData));
                        using (FileStream stream = new FileStream(playerDataPath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            formatter.Serialize(stream, data);
                        }
                    }

                    string loginDataPath = Path.Combine(Application.persistentDataPath, "loginData.dat");
                    if (!IsFileInUse(loginDataPath))
                    {
                        LoginData login = new LoginData
                        {
                            uuid = uuid,
                            username = username,
                            nickname = nickname,
                            password = user,
                            token = token,
                            hardware_id = SystemInfo.deviceUniqueIdentifier
                        };

                        XmlSerializer formatter1 = new XmlSerializer(typeof(LoginData));
                        using (FileStream stream1 = new FileStream(loginDataPath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            formatter1.Serialize(stream1, login);
                        }
                    }
                }
                catch (IOException ex)
                {
                    Debug.LogError($"File access error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Unexpected error: {ex.Message}");
                }
            }
        }

        // Method to calculate totalXP by summing every 5th entry from scores.dat
        private long CalculateTotalXPFromFile()
        {
            long sum = 0;
            string filePath = Path.Combine(Application.persistentDataPath, "scores.dat");

            try
            {
                if (!File.Exists(filePath))
                {
                    Debug.LogError($"File does not exist at path: {filePath}");
                    return 0;
                }

                var lines = File.ReadAllLines(filePath);
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    var parts = line.Split(',');
                    if (parts.Length > 4 && long.TryParse(parts[4], out long score))
                    {
                        sum += score;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error reading the file: {ex.Message}");
            }

            return sum;
        }

        private void LevelUp()
        {
            if (currentXP >= xpRequiredPerLevel[level] && level <= 299)
            {
                currentXP -= xpRequiredPerLevel[level];
                level++;
                LevelUp();
            }
            Notifications.instance.Notify($"Level up! ({level - 1} -> {level})", null);
        }

        public void Apply(string username, string user, string email, string cc)
        {
            this.username = username;
            this.user = user;
            this.cc = cc;
            this.email = email;
            SavePlayerData(user, email);
        }

        public IEnumerator ApplyLogin(string username, string user, string email)
        {
            this.username = username;
            this.user = user;
            SaveLocalData();
            LoginData loginData = new LoginData
            {
                username = username,
                password = user,
                hardware_id = SystemInfo.deviceUniqueIdentifier
            };

            string json = JsonConvert.SerializeObject(loginData, Formatting.None, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            });
            using (UnityWebRequest request = new UnityWebRequest(url + "/v1/account/login", "POST"))
            {

                request.SetRequestHeader("content-type", "application/json");

                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();

                // Ensure HTTPS
                if (!url.StartsWith("https"))
                {
                    Notifications.instance.Notify("Login failed: insecure connection.", null);
                    yield break;
                }
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    try
                    {
                        var errorResponse = JObject.Parse(request.downloadHandler.text);
                        var errors = errorResponse["errors"];
                        Notifications.instance.Notify($"{errors.Count()} error(s) occurred. More info in the player logs (click).", () => Process.Start($@"{Path.Combine(Application.persistentDataPath, "Player.log")}"));
                        Debug.LogError(errors);
                    }
                    catch
                    {
                        Notifications.instance.Notify("An unknown error occurred. Please try again.", null);
                    }
                }
                else
                {

                    try
                    {
                        var successResponse = JObject.Parse(request.downloadHandler.text);
                        Notifications.instance.Notify($"Successfully logged in as {loginData.username}", null);
                        string token = successResponse["token"].ToString();
                        string uuid = successResponse["user"]["id"].ToString();
                        this.uuid = uuid;
                        this.token = token;
                       

                            
                        SaveLocalData();

                        // Mark user as logged in
                        loggedIn = true;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error parsing success response: {ex}");
                        Notifications.instance.Notify("Login succeeded, but a response error occurred.", null);
                        SaveLocalData();
                    }
                }
            };
        }

        public void CalculateXPRequirements()
        {
            long initialXP = 10000000L;
            float growthRate = 1.03f;
            xpRequiredPerLevel = new long[300];

            xpRequiredPerLevel[0] = initialXP;
            for (int i = 1; i < xpRequiredPerLevel.Length; i++)
            {
                xpRequiredPerLevel[i] = (long)(xpRequiredPerLevel[i - 1] * growthRate);
            }
        }

        public void SavePlayerData(string pass, string email)
        {
            LoginData loginData = new LoginData
            {
                nickname = username,
                username = username,
                email = email,
                password = pass,
                hardware_id = SystemInfo.deviceUniqueIdentifier
            };

            // Register player
            StartCoroutine(Register(url + "/v1/account/signup", loginData, pass));
        }

        public IEnumerator Register(string url, LoginData bodyJsonObject, string inputPassword)
        {
            string bodyJsonString = JsonUtility.ToJson(bodyJsonObject); 
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.SetRequestHeader("content-type", "application/json");

                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(bodyJsonString);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();

                // Ensure HTTPS
                if (!url.StartsWith("https"))
                {
                    Debug.LogError("Insecure connection detected. HTTPS is required.");
                    Notifications.instance.Notify("Registration failed: insecure connection.", null);
                    yield break;
                }

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    


                    try
                    {
                        var errorResponse = JObject.Parse(request.downloadHandler.text);
                        var errors = errorResponse["errors"];
                        Notifications.instance.Notify($"{errors.Count()} error(s) occurred. More info in the player logs (click).", () => Process.Start($@"{Path.Combine(Application.persistentDataPath, "Player.log")}"));
                        Debug.LogError(errors);
                    }
                    catch
                    {
                        Notifications.instance.Notify("An unknown error occurred. Please try again.", null);
                    }
                }
                else
                {

                    try
                    {
                        var successResponse = JObject.Parse(request.downloadHandler.text);
                        Notifications.instance.Notify($"Successfully registered as {bodyJsonObject.username}", null);

                        // Safely pass the password for immediate login if needed
                        StartCoroutine(ApplyLogin(bodyJsonObject.username, inputPassword, bodyJsonObject.email));

                        // Mark user as logged in
                        loggedIn = true;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error parsing success response: {ex}");
                        Notifications.instance.Notify("Registration succeeded, but a response error occurred.", null);
                    }
                }

                // Clear sensitive data from memory
                inputPassword = null;
            }
        }

        void LoginData()
        {
            string path = Application.persistentDataPath + "/loginData.dat"; // No file extension here

            if (File.Exists(path))
            {
                XmlSerializer formatter = new XmlSerializer(typeof(LoginData));

                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    LoginData login = (LoginData)formatter.Deserialize(stream);

                    // Apply login data and avoid calling login multiple times
                    if (!loggedIn)
                    {
                        StartCoroutine(ApplyLogin(login.username, login.password, login.email));
                    }
                }
            }
            else
            {
                File.Create(path);
                Debug.LogError("Login data file not found.");
            }
        }

        public PlayerData LoadData()
        {
            string path = Application.persistentDataPath + "/playerData.dat";
            string playtime = Application.persistentDataPath + "/playtime.dat";

            string play = File.ReadAllText(playtime);
            this.playtime = float.Parse(play);
            if (File.Exists(path))
            {
                XmlSerializer formatter = new XmlSerializer(typeof(PlayerData));
                FileStream stream = new FileStream(path, FileMode.Open);

                PlayerData data = formatter.Deserialize(stream) as PlayerData;
                stream.Close();

                username = data.username;
                cc = data.country;
                level = data.level;
                return data;
            }
            else
            {
                
                return null;
            }

            
        }

        void Update()
        {
            playtime += Time.deltaTime;
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                checkInternet.SetActive(true);
            }
            else
            {
                checkInternet.SetActive(false);
            }
        }
        public string ConvertPlaytimeToReadableFormat()
        {
            int totalSeconds = Mathf.FloorToInt(playtime);

            int days = totalSeconds / 86400; // 86400 seconds in a day
            if (days > 0)
            {
                int hours = (totalSeconds % 86400) / 3600; // Remaining hours within the day
                return $"{days}d {hours}h";
            }

            int hours1 = totalSeconds / 3600; // 3600 seconds in an hour
            if (hours1 > 0)
            {
                int minutes = (totalSeconds % 3600) / 60; // Remaining minutes within the hour
                return $"{hours1}h {minutes}m";
            }

            int minutes1 = totalSeconds / 60; // 60 seconds in a minute
            int seconds = totalSeconds % 60;
            return $"{minutes1}m {seconds}s";
        }

        private void SavePlaytime()
        {
            string playtimePath = Path.Combine(Application.persistentDataPath, "playtime.dat");
            try
            {
                using (StreamWriter writer = new StreamWriter(playtimePath, false))
                {
                    writer.WriteLine(playtime); // Save the playtime
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Error saving playtime: " + ex.Message);
            }
        }

        private IEnumerator SavePlaytimeEverySecond()
        {
            while (true)
            {
                SavePlaytime();
                yield return new WaitForSeconds(1f); // Wait for 1 second
            }
        }
    }
    [System.Serializable]
    public class PlayerData
    {
        [Header("Stats")]
        public int level;
        public long currentXP;
        public long[] xpRequiredPerLevel;
        public long totalXP;
        public float playtime;
        public float sp;
        public int playCount;

        [Header("Login data")]
        public string username;
        public string nickname;
        public string password;
        public string email;
        public string loginToken;

        [Header("Profile info")]
        public string country;
        public bool isLocal;
        public bool isOnline;
        public string id;
        public string token;
    }

    public class LoginData
    {
        public string uuid;
        public string nickname;
        public string username;
        public string email;
        public string password;
        public string token;
        public string hardware_id;
    }

}
