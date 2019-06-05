using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [Header("General")]
    [SerializeField] Canvas gameCanvas;
    [SerializeField] GameObject instructions;
    [SerializeField] GameObject pause;
    [SerializeField] int startingWave = 1;
    [Header("Color Shcemes")]
    [SerializeField] ColorScheme[] ColorSchemes;
    [SerializeField] GameObject backgroundObject;
    [Header("Sounds")]
    [SerializeField] AudioSource music;
    [SerializeField] AudioClip waveStartSound;
    [SerializeField] AudioClip waveCompleteSound;
    [SerializeField] AudioClip gameOverSound;

    public static GameController Instance;

    enum GameState { Menu, Statistics, Settings, Game, Pause }
    GameState gameState;

    BlocksController blocksController;
    Lifes lifes;
    ColorScheme currentColorScheme;
    WaveDisplay waveDisplay;
    Animator canvasAnimator;
    Stopwatch gameTime;

    public int currentWave { get; private set; }
    public bool paused { get; private set; }
    
    

    #region behaviour
    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 120;
        Instance = this;
    }

    private void Start()
    {
        gameTime = new Stopwatch();

        blocksController = FindObjectOfType<BlocksController>();
        canvasAnimator = gameCanvas.GetComponent<Animator>();
        gameState = GameState.Menu;
        music.volume = PlayerPrefs.GetFloat(GameConstants.MusicVolume, 1);
        PlayerPrefs.GetFloat(GameConstants.SFXVolume, 0.8f);
    }
    #endregion

    #region general
    private void UpdateStats()
    {
        int mWave = PlayerPrefs.GetInt(GameConstants.MaxWave);
        if (currentWave > mWave)
        {
            PlayerPrefs.SetInt(GameConstants.MaxWave, currentWave);
        }

        int mScore = PlayerPrefs.GetInt(GameConstants.MaxScore);
        if (Score.currentPoints > mScore)
        {
            PlayerPrefs.SetInt(GameConstants.MaxScore, (int)Score.currentPoints);
        }

        int tScore = PlayerPrefs.GetInt(GameConstants.TotalScore);
        PlayerPrefs.SetInt(GameConstants.TotalScore, (int)Score.currentPoints + tScore);

        gameTime.Stop();
        float tt = PlayerPrefs.GetFloat(GameConstants.TotalTime, 0);
        PlayerPrefs.SetFloat(GameConstants.TotalTime, tt + (float)gameTime.Elapsed.TotalSeconds);
    }

    private IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(1f);
        gameCanvas.GetComponent<Animator>().SetTrigger("GameOver");
        AudioSource.PlayClipAtPoint(gameOverSound, Camera.main.transform.position, PlayerPrefs.GetFloat(GameConstants.SFXVolume));
        yield return new WaitForSeconds(3.5f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator GameCycle()
    {
        if (PlayerPrefs.GetInt(GameConstants.StartWaveBool) == 1)
        {
            startingWave = PlayerPrefs.GetInt(GameConstants.MaxWave, 0);
        }
        startingWave = (startingWave <= 0) ? 1 : startingWave;

        currentWave = startingWave;
        waveDisplay.UpdateWave(currentWave);
        SetColorScheme();
        blocksController.SetColorScheme(currentColorScheme);

        gameTime.Start();

        int numberOfBlocks = Mathf.Clamp(12 + 4 * currentWave, 16, 72);
        float spawnConsistency = Mathf.Clamp(0.5f - 0.025f * currentWave, 0.1f, 0.5f);
        float ballSpeed = Mathf.Clamp(10f + Mathf.Sqrt(currentWave), 10f, 16f);

        FindObjectOfType<Ball>().SetTargetSpeed(ballSpeed);

        yield return new WaitForSeconds(1);
        instructions.SetActive(true);
        yield return new WaitUntil(() => Input.GetMouseButton(0));
        instructions.SetActive(false);

        AudioSource.PlayClipAtPoint(waveStartSound, Camera.main.transform.position, PlayerPrefs.GetFloat(GameConstants.SFXVolume));
        blocksController.SpawnNewBlocks(numberOfBlocks, spawnConsistency);

        yield return null;
        while (true)
        {
            yield return new WaitUntil(() => blocksController.numberOfBlocks == 0 || Input.GetKeyDown(KeyCode.Escape));
            if (blocksController.numberOfBlocks == 0)
            {
                PlayerPrefs.SetInt(GameConstants.WaveTotal, PlayerPrefs.GetInt(GameConstants.WaveTotal) + 1);
                AudioSource.PlayClipAtPoint(waveCompleteSound, Camera.main.transform.position, PlayerPrefs.GetFloat(GameConstants.SFXVolume));
                gameCanvas.GetComponent<Animator>().SetTrigger("NextWave");
                currentWave++;
                waveDisplay.UpdateWave(currentWave);
                FindObjectOfType<Ball>().DestroyBall();
                lifes.AddLife();

                yield return new WaitForSeconds(1f);
                SetColorScheme();
                blocksController.SetColorScheme(currentColorScheme);
                yield return new WaitForSeconds(1f);

                numberOfBlocks = Mathf.Clamp(12 + 4 * currentWave, 16, 72);
                spawnConsistency = Mathf.Clamp(0.5f - 0.025f * currentWave, 0.1f, 0.5f);
                ballSpeed = Mathf.Clamp(10f + Mathf.Sqrt(currentWave), 10f, 16f);

                FindObjectOfType<Ball>().SetTargetSpeed(ballSpeed);
                AudioSource.PlayClipAtPoint(waveStartSound, Camera.main.transform.position, PlayerPrefs.GetFloat(GameConstants.SFXVolume));
                blocksController.SpawnNewBlocks(numberOfBlocks, spawnConsistency);
                yield return null;
            }
            else
            {
                if (FindObjectOfType<Lifes>().LifesCount > 0)
                {
                    pause.SetActive(true);
                    gameTime.Stop();
                    paused = true;
                    yield return null;
                    yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Escape));

                    gameTime.Start();

                    pause.SetActive(false);
                    paused = false;
                    yield return null;
                }
            }
        }
    }

    private void SetColorScheme()
    {
        int i = Random.Range(0, ColorSchemes.Length);
        //i = 5;
        currentColorScheme = ColorSchemes[i];
        backgroundObject.GetComponent<SpriteRenderer>().color = currentColorScheme.background;
    }
    #endregion
    
    #region menu
    public void StartGame()
    {
        if (gameState != GameState.Game)
        {
            canvasAnimator.SetTrigger("StartGame");
            lifes = FindObjectOfType<Lifes>();
            waveDisplay = FindObjectOfType<WaveDisplay>();
            paused = false;
            gameState = GameState.Game;
            StartCoroutine(GameCycle());
        }
    }

    public void OpenSettings()
    {
        if (gameState == GameState.Menu)
        {
            gameState = GameState.Settings;
            canvasAnimator.SetTrigger("OpenSettings");
        }
    }

    public void CloseSettings()
    {
        if (gameState == GameState.Settings)
        {
            gameState = GameState.Menu;
            canvasAnimator.SetTrigger("CloseSettings");
        }
    }

    public void OpenStatistic()
    {
        if (gameState == GameState.Menu)
        {
            gameState = GameState.Statistics;
            canvasAnimator.SetTrigger("OpenStatistics");
        }
    }

    public void CloseStatistic()
    {
        if (gameState == GameState.Statistics)
        {
            gameState = GameState.Menu;
            canvasAnimator.SetTrigger("CloseStatistics");
        }
    }

    public void ResetStatistic()
    {
        PlayerPrefs.SetInt(GameConstants.MaxWave, 0);
        PlayerPrefs.SetInt(GameConstants.WaveTotal, 0);
        PlayerPrefs.SetInt(GameConstants.MaxScore, 0);
        PlayerPrefs.SetInt(GameConstants.TotalScore, 0);
        PlayerPrefs.SetInt(GameConstants.BlocksTotal, 0);
        PlayerPrefs.SetInt(GameConstants.BlocksHitted, 0);
        PlayerPrefs.SetInt(GameConstants.SPUsed, 0);
        PlayerPrefs.SetInt(GameConstants.MUsed, 0);
        PlayerPrefs.SetFloat(GameConstants.TotalTime, 0);
    }

    public void GameOver()
    {
        UpdateStats();
        StartCoroutine(GameOverRoutine());
    }

    public void ExitGame()
    {
        Application.Quit();
    }
    #endregion

}

public static class GameConstants
{
    public const string StartWaveBool = "StartWaveBool";
    public const string MusicVolume = "MusicVolume";
    public const string SFXVolume = "SFXVolume";
    public const string MaxWave = "MaxWave";
    public const string WaveTotal = "WaveTotal";
    public const string MaxScore = "MaxScore";
    public const string TotalScore = "TotalScore";
    public const string BlocksHitted = "BlocksHitted";
    public const string BlocksTotal = "BlocksTotal";
    public const string SPUsed = "SPUsed";
    public const string MUsed = "MUsed";
    public const string TotalTime = "TotalTime";
}
