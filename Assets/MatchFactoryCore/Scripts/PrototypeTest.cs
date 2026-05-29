using MatchFactoryCore.Scripts.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PrototypeTest : MonoBehaviour
{
    [SerializeField] ControllerMatchFactory controller;
    [SerializeField] private Button resetButton;
    [SerializeField] private Text stateText;

    private void OnEnable()
    {
        resetButton.onClick.AddListener(ResetGame);
    }

    void OnDisable()
    {
        resetButton.onClick.RemoveListener(ResetGame);
    }

    private void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void Update()
    {
        stateText.text = controller.stateName;
    }
}