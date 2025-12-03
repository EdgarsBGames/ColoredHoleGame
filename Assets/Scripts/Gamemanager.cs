using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using MoreMountains.Feedbacks;

public class Gamemanager : MonoBehaviour
{
    public static Gamemanager Instance { get; private set; }

    public InputTestingScript ballManager;

    private int score;
    private int previousScore;
    private int highScore = 0;

    public float timeLeft;
    public float startingTime;
    public float timerSpeed; // for difficulity increase
    public float maxTimerSpeed;

    public TextMeshProUGUI timerText,scoreText,endScreenScoreText,highScoreText,onScorePopupText;
    public GameObject endGamePanel,gamePanel;

    public string[] titles;

    public MMF_Player scoreTitleFeedback;

    //Hole distribution variables
    public Transform[] spawnLocations; // 15 spots
    public List<int> availableSpots = new List<int>();
    public GameObject[] objectsToPlace;

    public bool gameEnded { get; private set; }

   // private bool gameEnded;

    private void Awake()
    {
        //If an instance already exists AND it's not this one removes new one
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        // set this instance
        Instance = this;
    }
    void Start()
    {
        highScore = PlayerPrefs.GetInt("HighScore",0); //load in highscore data
        highScoreText.text = "HIGHSCORE: " + highScore.ToString();
        //initialize available spots
        for (int i = 0; i < spawnLocations.Length; i++)
            availableSpots.Add(i);

        AssignObjectsToLocations();
        ballManager = ballManager.GetComponent<InputTestingScript>();
        UpdateTextValue();
    }

    // Update is called once per frame
    void Update()
    {
        GameTimer();
    }

    void GameTimer()
    {
        if (!gameEnded)
        {
            timerSpeed = 0.6f + (score * 0.01f);
            timerSpeed = Mathf.Clamp(timerSpeed, 0.1f, maxTimerSpeed); //limit difficulity

            timeLeft -= Time.deltaTime * timerSpeed;
            UpdateTextValue();
           // Debug.Log(timeLeft);
            if (timeLeft < 0)
            {
                EndGameScreen();
                Debug.Log("timer over");
            }
        }
        else
        {
            return;
        }

    }
    
    //Adds time to timer and updates its text value
    public void AddOrRemoveTime(float amount)
    {
        timeLeft += amount;
        UpdateTextValue();
    }

    public void AddScore(int amount)
    {
        score += amount;
        scoreText.text = "SCORE: " + score.ToString();

        DisplayOneOfTheTitles();
    }


    //Updates timer text , and sets it to have no commas flat numbers for timer
    void UpdateTextValue()
    {
        timerText.text = timeLeft.ToString("0");
    }

    
    //sets text value to one of titles from array and displays it on screen
    public void DisplayOneOfTheTitles()
    {
        if(titles.Length > 0)
        {
            int titleNumber = Random.Range(0, titles.Length);
            onScorePopupText.text = titles[titleNumber];
            scoreTitleFeedback?.PlayFeedbacks();
        }
        else
        {
            Debug.Log("No display titles added");
        }
    }


    //enables game end screen , UI + option to play again
    void EndGameScreen()
    {
        gameEnded = true;

        //sets highscore 
        if (highScore<score)
        {
            highScore = score;
            //updates highscore text
            highScoreText.text = "HIGHSCORE: " + highScore.ToString();
            SaveHighScoreData();
        }

        gamePanel.SetActive(false);

        endScreenScoreText.text = score.ToString();
        endGamePanel.SetActive(true);
    }


    //resets game state to a fresh run
   public void PlayAgain()
    {
        timeLeft = startingTime;
        score = 0;
        scoreText.text = "SCORE: " + score.ToString(); // update score text
        gameEnded = false;
        UpdateTextValue();
        gamePanel.SetActive(true);
        endGamePanel.SetActive(false);       
        
    }


    //Hole setup logic
    void AssignObjectsToLocations()
    {
        List<int> taken = new List<int>();

        for (int i = 0; i < objectsToPlace.Length; i++)
        {
            int index;

            do
            {
                index = Random.Range(0, spawnLocations.Length);
            }
            while (taken.Contains(index));

            taken.Add(index);
            objectsToPlace[i].transform.position = spawnLocations[index].position;
        }
    }

    public void MoveObject(GameObject obj)
    {
        // Remove old spot (find the spot the object currently sits on)
        int oldIndex = FindLocationIndex(obj.transform.position);
        if (oldIndex != -1 && !availableSpots.Contains(oldIndex))
            availableSpots.Add(oldIndex);

        // Choose a free spot
        int newSpotIndex = availableSpots[Random.Range(0, availableSpots.Count)];

        // Move the object
        obj.transform.position = spawnLocations[newSpotIndex].position;

        // Mark the new spot as taken
        availableSpots.Remove(newSpotIndex);
    }

    int FindLocationIndex(Vector3 pos)
    {
        for (int i = 0; i < spawnLocations.Length; i++)
        {
            if (Vector3.Distance(spawnLocations[i].position, pos) < 0.01f)
                return i;
        }
        return -1;
    }

    void SaveHighScoreData()
    {
        PlayerPrefs.SetInt("HighScore", highScore);
        PlayerPrefs.Save();
    }

}
