using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using System.ComponentModel;

// ADT: What is our game loop?
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private GameObject guiStats;
    private UIDocument uiDocument;
    //TODO: public GameObject gameOverUI;
    private Label timeLabel;
    private Label livesLabel;
    private Label scoreLabel;
    [SerializeField] private int timeToCompleteRound = 30;
    [SerializeField] private int timeBonusMultiplier = 20;
    [SerializeField] private int pointsForNewFarthestRow = 10;
    [SerializeField] private int pointsForOccupyingAHome = 50;
    [SerializeField] private int pointsForClearingAllHomes = 1000;
    public Frogger frogger;
    [SerializeField] private Home[] homes;
    private int time;
    private int score;
    private int lives;
    
    void Awake()
    {
        if (Instance != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Instance = this;
            Application.targetFrameRate = 60;
            DontDestroyOnLoad(gameObject);
        }
    }
    private void OnEnable()
    {
        if (guiStats == null)
        {
            Debug.LogError("Missing guiStats");
            return;
        }
        uiDocument = guiStats.GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            Debug.LogError("Missing guiStats");
            return;
        }
        VisualElement rootVisualElement = uiDocument.rootVisualElement;


        VisualElement container = rootVisualElement.Q<VisualElement>("Container");
        
        GroupBox groupBox = container.Q<GroupBox>("GroupBox");
        timeLabel = groupBox.Q<Label>("Time");
        livesLabel = groupBox.Q<Label>("Lives");
        scoreLabel = groupBox.Q<Label>("Score");
    }

    private void Start()
    {
        if (guiStats == null)
        {
            Debug.LogError("Missing guiStats");
            return;
        }
        if (homes == null)
        {
            Debug.LogError("Missing homes");
            return;
        }
        if (homes.Length != 5)
        {
            Debug.LogError("Missing 5 homes only have " + homes.Length);
            return;
        }
        if (frogger == null)
        {
            Debug.LogError("Missing frogger");
            return;
        }
        //TODO:
        //if (gameOverUI == null)
        //{
        //    Debug.LogError("Missing game over ui");
        //    return;
        //}
        uiDocument = guiStats.GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("Missing guiStats");
            return;
        }
        if (guiStats == null)
        {
            Debug.LogError("Missing guiStats");
            return;
        }
        VisualElement rootVisualElement = uiDocument.rootVisualElement;


        VisualElement container = rootVisualElement.Q<VisualElement>("Container");

        GroupBox groupBox = container.Q<GroupBox>("GroupBox");
        timeLabel = groupBox.Q<Label>("Time");
        livesLabel = groupBox.Q<Label>("Lives");
        scoreLabel = groupBox.Q<Label>("Score");

        if (timeLabel == null)
        {
            Debug.LogError("Missing time text");
            return;
        }
        if (livesLabel == null)
        {
            Debug.LogError("Missing lives text");
            return;
        }
        if (scoreLabel == null)
        {
            Debug.LogError("Missing score text");
            return;
        }

        NewGame();
    }
    private void NewGame() // Start completly fresh reset score to 0 and initalize the starting lives 
    {
        // TODO: gameOverUI.SetActive(false);
        SetScore(0);
        SetLives(3);
        NewLevel();
    }
    private void NewLevel() // Clear all 5 homes, you start a new level which resets all the homes and you gain a flat +1000 points or something
    {
        for(int i = 0; i < homes.Length; i++)
        {
            homes[i].enabled = false;
        }
        Respawn();
    }

    private void Respawn() // Previously called NewRound, everytime you occupy a new home you start a new round. This is because in frogger theres a timer and the faster you get inside a home the more points you get +points
    {
        frogger.Respawn();
        StopAllCoroutines();
        StartCoroutine(Timer(timeToCompleteRound));
    }

    private IEnumerator Timer(int duration)
    {
        time = duration;
        timeLabel.text = "Time " + time.ToString();

        while (time > 0)
        {
            yield return new WaitForSeconds(1);
            time--;
            timeLabel.text = "Time " + time.ToString();
        }

        frogger.Death();
    }
    public void Died()
    {
        SetLives(lives - 1);
        if (lives > 0)
        {
            Invoke(nameof(Respawn), 1f);
        }
        else
        {
            Invoke(nameof(GameOver), 1f); // Game is over
        }
    }
    private void GameOver() // Hide frogger entirely then display game over ui prompting user to play again. Pressing 'enter' restarts game
    {
        frogger.gameObject.SetActive(false);
        // TODO: gameOverUI.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(CheckForPlayAgain());
    }

    bool playAgain;
    private IEnumerator CheckForPlayAgain()
    {
        playAgain = false;
        InputManager.Instance.OnInteract += SetPlayAgainTrue;

        while (!playAgain)
        {
            yield return null; // Wait until the OnInteract event sets playAgain to true
        }

        InputManager.Instance.OnInteract -= SetPlayAgainTrue; // Unsubscribe to clean up

        NewGame();
    }

    private void SetPlayAgainTrue()
    {
        playAgain = true;
    }

    public void UpdateFurthestRow()
    {
        SetScore(score + pointsForNewFarthestRow);
    }

    public void HomeOccupied()
    {
        frogger.gameObject.SetActive(false);

        int bonusPoints = time * timeBonusMultiplier;
        SetScore(score + bonusPoints + pointsForOccupyingAHome);

        if (Cleared())
        {
            SetScore(score + pointsForClearingAllHomes);
            // TODO: SetLives(lives + 1);
            Invoke(nameof(NewLevel), 1f);
        }
        else
        {
            Invoke(nameof(Respawn), 1f);
        }
    }
    private bool Cleared()
    {
        for (int i = 0; i < homes.Length; i++)
        {
            if (!homes[i].enabled)
            {
                return false;
            }
        }
        return true;
    }
    private void SetScore(int score)
    {
        this.score = score;
        scoreLabel.text = "Score " + score.ToString();
    }
    private void SetLives(int lives)
    {
        this.lives = lives;
        livesLabel.text = "Lives " + lives.ToString();
    }
 
}
