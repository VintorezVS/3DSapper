using UnityEngine;

public class GameMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject controlsPanel;

    private void OnEnable()
    {
        Time.timeScale = 0;
    }

    private void OnDisable()
    {
        Time.timeScale = 1;
    }

    public void ToggleMenu()
    {
        gameObject.SetActive(!gameObject.activeSelf);
        HideControls();
    }

    public void ResumeGame()
    {
        gameObject.SetActive(false);
    }

    public void ShowControls()
    {
        mainPanel.SetActive(false);
        controlsPanel.SetActive(true);
    }

    public void HideControls()
    {
        mainPanel.SetActive(true);
        controlsPanel.SetActive(false);
    }

    public void RestartGame()
    {
        GameManager.Instance.StartGame();
    }

    public void GoToMainMenu()
    {
        GameManager.Instance.ShowMainMenu();
    }
}
