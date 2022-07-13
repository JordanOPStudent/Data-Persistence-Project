using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    public static MainManager Instance;

    public Brick BrickPrefab;
    public int LineCount = 6;
    public Rigidbody Ball;

    public Text ScoreText;
    public GameObject GameOverText;
    public Text HighScoreText;

    public string CurrentPlayer = "";
    
    private bool m_Started = false;
    private int m_Points;
    
    private bool m_GameOver = false;

    private int m_bestScore = 0;
    private string m_bestPlayer = "Name";

    private void Awake()
    {
        // Prevents Duplicates
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadData();
    }

    // Start is called before the first frame update
    void Start()
    {
        const float step = 0.6f;
        int perLine = Mathf.FloorToInt(4.0f / step);
        
        int[] pointCountArray = new [] {1,1,2,2,5,5};
        for (int i = 0; i < LineCount; ++i)
        {
            for (int x = 0; x < perLine; ++x)
            {
                Vector3 position = new Vector3(-1.5f + step * x, 2.5f + i * 0.3f, 0);
                var brick = Instantiate(BrickPrefab, position, Quaternion.identity);
                brick.PointValue = pointCountArray[i];
                brick.onDestroyed.AddListener(AddPoint);
            }
        }

        SetBestPlayer();
    }

    private void Update()
    {
        if (!m_Started)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                m_Started = true;
                float randomDirection = Random.Range(-1.0f, 1.0f);
                Vector3 forceDir = new Vector3(randomDirection, 1, 0);
                forceDir.Normalize();

                Ball.transform.SetParent(null);
                Ball.AddForce(forceDir * 2.0f, ForceMode.VelocityChange);
            }
        }
        else if (m_GameOver)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }

    void AddPoint(int point)
    {
        m_Points += point;
        ScoreText.text = $"Score : {m_Points}";
    }

    public void SaveScore(string name = "", int score = 0)
    {
        SaveData data = new SaveData();
        data.Name = name;
        data.BestScore = score;

        string json = JsonUtility.ToJson(data);

        File.WriteAllText(Application.persistentDataPath + "/savefile.json", json);
    }

    public void LoadData()
    {
        string path = Application.persistentDataPath + "/savefile.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            m_bestPlayer = data.Name;
            m_bestScore = data.BestScore;
        }
    }

    private void SetBestPlayer()
    {
        string highscoreText = "Best Score: " + m_bestPlayer.ToString() + ": " + m_bestScore.ToString();
        HighScoreText.text = highscoreText;
    }

    public void GameOver()
    {
        m_GameOver = true;
        if (m_Points > m_bestScore)
        {
            m_bestPlayer = CurrentPlayer;
            m_bestScore = m_Points;
            SaveScore(CurrentPlayer, m_Points);
            SetBestPlayer();
        }
        GameOverText.SetActive(true);
    }

    [System.Serializable]
    public class SaveData
    {
        public string Name;
        public int BestScore;
    }
}
