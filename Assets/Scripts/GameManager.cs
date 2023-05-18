using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourSingleton<GameManager>
{
    public bool IsGameInProgress { get; private set; } = true;

    public void StartGame()
    {
        IsGameInProgress = true;
        SceneManager.LoadScene(1);
    }

    public void GameOver()
    {
        IsGameInProgress = false;
    }

    public void ShowMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
