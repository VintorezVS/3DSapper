using UnityEngine;
using UnityEngine.UIElements;

public class MenuManager : MonoBehaviour
{
    private Button startButton;

    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();

        startButton = uiDocument.rootVisualElement.Q<Button>("start");
        startButton.RegisterCallback<ClickEvent>(StartGame);
    }

    private void OnDisable()
    {
        startButton.UnregisterCallback<ClickEvent>(StartGame);
    }

    private void StartGame(ClickEvent clickEvent)
    {
        GameManager.Instance.StartGame();
    }
}
