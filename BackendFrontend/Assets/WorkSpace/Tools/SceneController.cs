using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : SingletonMonoBehaviour<SceneController>
{
    [SerializeField] string sceneName;
    [SerializeField] bool onAwakeMoveScene;
    private void Start()
    {
        if (onAwakeMoveScene) MoveScene();
    }
    public void MoveScene()
    {
        SceneManager.LoadSceneAsync(sceneName);
    }
    public void MoveScene(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName);
    }
}
