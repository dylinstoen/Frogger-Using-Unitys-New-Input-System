using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ADT: What is our game loop?
public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<GameManager>();
            }

            return _instance;
        }
    }

    public GameObject gameOverMenu;

    [SerializeField] private int timeToCompleteRound = 30;
    [SerializeField] private int timeBonusMultiplier = 20;
    [SerializeField] private int pointsForNewFarthestRow = 10;
    [SerializeField] private int pointsForOccupyingAHome = 50;
    [SerializeField] private int pointsForClearingAllHomes = 1000;
    private Frogger frogger;
    private int time;
    private int score;
    private int lives;
    private Home[] homes;
    void Awake()
    {
        homes = GameObject.FindObjectsOfType<Home>();
        frogger = GameObject.FindObjectOfType<Frogger>();
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        NewGame();
    }
    private void NewGame() // Start completly fresh reset score to 0 and initalize the starting lives 
    {
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
        while (time > 0)
        {
            yield return new WaitForSeconds(1);
            time--;
        }
        frogger.Death();
    }



    private void SetScore(int score)
    {
        this.score = score;

    }
    private void SetLives(int lives)
    {
        this.lives = lives;
    }
    public void HomeOccupied()
    {
        frogger.gameObject.SetActive(false);

        int bonusPoints = time * timeBonusMultiplier;
        SetScore(score + bonusPoints + pointsForOccupyingAHome);

        if(Cleared())
        {
            SetScore(score + pointsForClearingAllHomes);
            // SetLives(lives + 1);
            Invoke(nameof(NewLevel),1f);
        }
        else
        {
            Invoke(nameof(Respawn), 1f);
        }
    }
    public void NewFarthestRow()
    {
        SetScore(score + pointsForNewFarthestRow);
    }

    public void Died()
    {
        SetLives(lives - 1);
        if(lives > 0)
        {
            Invoke(nameof(Respawn), 1f);
        } else
        {
            Invoke(nameof(GameOver), 1f); // Game is over
        }
    }

    private void GameOver() // Hide frogger entirely then display game over ui prompting user to play again. Pressing 'enter' restarts game
    {
        frogger.gameObject.SetActive(false);
        gameOverMenu.SetActive(true);

    }

    private bool Cleared()
    {
        for(int i = 0; i < homes.Length; i++)
        {
            if (!homes[i].enabled)
            {
                return false;
            }
        }
        return true;
    }
}
