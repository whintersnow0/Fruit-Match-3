using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [Header("Controllers")]
    [SerializeField] private GameController gameController;
    [SerializeField] private GridController gridController;
    [SerializeField] private InputController inputController;
    [SerializeField] private ScoreController scoreController;

    private void Awake()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        gameController?.Initialize();
        gridController?.Initialize();
        inputController?.Initialize();
        scoreController?.Initialize();

        Debug.Log("Match 3 Game initialized with MVC architecture");
    }

    private void OnDestroy()
    {
        gameController?.Cleanup();
        gridController?.Cleanup();
        inputController?.Cleanup();
        scoreController?.Cleanup();
    }
}