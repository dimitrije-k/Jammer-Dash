using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using JammerDash.Game.Player;
using JammerDash.Tech;
using JammerDash.Tech.Levels;
using JammerDash.Editor.Basics;

namespace JammerDash.Game
{
    public class FinishLine : MonoBehaviour
    {
        private Rigidbody2D rb;
        public PlayerMovement player0;
        public Text score;
        public Text deadScore;
        public CubeCounter scores;
        public GameObject finishParticles;
        public GameObject player;
        public AudioSource finishSound;
        public GameObject finishMenu;
        public Text scoreText;
        public Animation anim;
        [Header("Scores")]
        public Text five;
        public Text three;
        public Text one;
        public Text miss;

        private void Start()
        {

            float num1 = float.NegativeInfinity;
            itemUnused itemUnused1 = null;
            foreach (itemUnused itemUnused2 in FindObjectsOfType<itemUnused>())
            {
                float num2 = itemUnused2.transform.position.x - transform.position.x;
                if (num2 > num1)
                {
                    num1 = num2;
                    itemUnused1 = itemUnused2;
                }
            }
            if (itemUnused1 != null)
            {
                transform.position = new Vector3((itemUnused1.transform.position + new Vector3(5f, 0f, 0.0f)).x, 0.0f, 0.0f);
                if (itemUnused1.GetComponent<SpriteRenderer>().size.x > 1)
                {
                    transform.position = new Vector3((itemUnused1.transform.position + new Vector3(5f + itemUnused1.GetComponent<SpriteRenderer>().size.x, 0f, 0.0f)).x, 0.0f, 0.0f);
                }
            }

        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.tag == "Player")
            {
                StartCoroutine(End());
                player.transform.position = transform.position;
                finishParticles.transform.position = player.transform.position;
                Instantiate(finishParticles, new Vector3(player.transform.position.x, player.transform.position.y, player.transform.position.z), Quaternion.identity);
                finishSound.Play();
            }


        }

        private void Update()
        {
            player = GameObject.FindGameObjectWithTag("Player");
            player0 = player.GetComponent<PlayerMovement>();
            scores = player.GetComponent<CubeCounter>();
            float num1 = float.NegativeInfinity;
            itemUnused itemUnused1 = null;
            foreach (itemUnused itemUnused2 in FindObjectsOfType<itemUnused>())
            {
                float num2 = itemUnused2.transform.position.x - transform.position.x;
                if (num2 > num1)
                {
                    num1 = num2;
                    itemUnused1 = itemUnused2;
                }
            }
            if (itemUnused1 != null && transform.position.x < itemUnused1.transform.position.x)
            {
                transform.position = new Vector3((itemUnused1.transform.position + new Vector3(5f, 0f, 0.0f)).x, 0.0f, 0.0f);

            }


            if (player0 == null || player == null)
            {

                player0 = player.GetComponent<PlayerMovement>();
                player = player0.gameObject;

            }
            if (player.transform.position.x >= transform.position.x && player != null)
            {
                player.transform.position = new Vector2(transform.position.x, transform.position.y + 0.5f);
                player0.enabled = false;
            }

            deadScore.text = "There's always another time! Maybe it's after you restart?";
        }

        void SaveLevelData(float actualdest, float destruction)
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == "LevelDefault")
            {
                SaveLevelDataForLevelDefault(actualdest, destruction);
            }
            else
            {
                SaveLevelDataDef(SceneManager.GetActiveScene().buildIndex, scores.GetTier(actualdest), "dest" + sceneName, actualdest, "scores", destruction, player0.five, player0.three, player0.one, player0.misses);
            }
        }
        void SaveLevelDataForLevelDefault(float actualdest, float destruction)
        {
            // Construct the path based on conditions
            string levelsPath = Path.Combine(Application.persistentDataPath,
                string.IsNullOrEmpty(CustomLevelDataManager.Instance.levelName)
                    ? Path.Combine("scenes", LevelDataManager.Instance.levelName)
                    : Path.Combine("levels", "extracted", CustomLevelDataManager.Instance.levelName));

            string[] levelFiles = Directory.GetFiles(levelsPath, "*.json", SearchOption.AllDirectories);
            string levelName = "";

            foreach (string file in levelFiles)
            {
                if (Path.GetFileName(file).Equals("LevelDefault.jdl"))
                {
                    continue; // Skip LevelDefault.jdl
                }
                levelName = Path.GetFileNameWithoutExtension(file);
                break; // Stop after finding the first valid level file
            }

            if (!string.IsNullOrEmpty(levelName))
            {
                string json = File.ReadAllText(Path.Combine(levelsPath, levelName + ".json"));
                SceneData sceneData = SceneData.FromJson(json);
                Debug.Log(sceneData.levelName);
                SaveLevelDataDef(sceneData.ID, scores.GetTier(actualdest), "dest" + sceneData.levelName, actualdest, "scores", destruction, player0.five, player0.three, player0.one, player0.misses);
            }
            else
            {
                Debug.LogWarning("No valid level found in the directory: " + levelsPath);
            }
        }

        void SaveLevelDataDef(int levelID, string tierName, string destName, float actualdest, string fileName, float destruction, int five, int three, int one, int miss)
        {
            string filePath = Path.Combine(Application.persistentDataPath, fileName + ".dat");
            using (StreamWriter writer = File.AppendText(filePath))
            {
                string formattedActualDest = actualdest.ToString("0.#################");
                string formattedDestruction = destruction.ToString("0.#################");

                writer.WriteLine($"{levelID},{tierName},{destName},{formattedActualDest},{formattedDestruction}");
            }
        }

        long LoadLevelData(int levelID, string fileName, long destruction)
        {
            string filePath = Path.Combine(Application.persistentDataPath, fileName + ".dat");

            if (File.Exists(filePath))
            {
                // Read the file and parse data as needed
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    if (line == SceneManager.GetActiveScene().name)
                    {
                        string[] data = line.Split(',');
                        // Assuming the file format is consistent with the data you're saving
                        float actualdest = int.Parse(data[3]);
                        destruction = int.Parse(data[4]);
                        // Process the loaded data as needed
                        return destruction;
                    }
                    else
                    {
                        string[] data = line.Split(',');
                        if (int.Parse(data[0]) == levelID)
                        {
                            float actualdest = float.Parse(data[3]);
                            destruction = int.Parse(data[4]);
                            // Process the loaded data as needed
                            Debug.LogError(destruction);
                            return destruction;
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("File does not exist: " + filePath);
            }

            return 0;
        }
        private IEnumerator End()
        {
            PlayerMovement objectOfType = FindObjectOfType<PlayerMovement>();
            long destruction = objectOfType.counter.score;
            float actualdest = (float)player0.counter.destructionPercentage;
            LevelSystem.Instance.GainXP(destruction); 
            score.text = $"{player0.SPInt:N0} sp\nAccuracy: {player0.counter.accCount / player0.Total * 100}%\nScore: {player0.counter.score:N0}\nLevel XP: {LevelSystem.Instance.totalXP:N0} <color=lime>(+{player0.counter.score})</color>\nCombo: {player0.highestCombo}x\n";
           if (scoreText != null)
            scoreText.text = $"{player0.counter.GetTier(player0.counter.accCount / player0.Total * 100)}";
            player.transform.localScale = Vector3.zero;
            objectOfType.enabled = false;


            SaveLevelData(actualdest, destruction);


            yield return new WaitForSecondsRealtime(2f);
            AudioSource[] audios = FindObjectsOfType<AudioSource>();
            Debug.Log(audios);
            finishMenu.SetActive(true);
            if (anim != null)
            anim.Play();
           

            five.text = $"{player0.five} (Great!)";
            three.text = $"{player0.three} (Good)";
            one.text = $"{player0.one} (Meh)";
            miss.text = $"{player0.misses} (Bad)";
            yield return new WaitForSecondsRealtime(0.75f);
            FindAnyObjectByType<AudioSource>().PlayOneShot(Resources.Load<AudioClip>($"Audio/SFX/ranking/{player0.counter.GetNoColorTier(player0.counter.accCount / player0.Total * 100)} Rank"));
               

        }

    }

    
}