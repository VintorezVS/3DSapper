using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourSingleton<GameManager>
{
    public bool IsGameInProgress { get; private set; } = true;

    #region Events
    public event System.Action<bool> OnGameOver;
    #endregion

    public void StartGame()
    {
        IsGameInProgress = true;
        SceneManager.LoadScene(1);
    }

    public void Win()
    {
        GameOver(isWin: true);
    }

    public void Lose()
    {
        GameOver(isWin: false);
    }

    public void ShowMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    private void GameOver(bool isWin)
    {
        IsGameInProgress = false;
        OnGameOver?.Invoke(isWin);
    }
}
