using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public string GameScene = "SampleScene";

    public void LoadGameScene()
    {
        SceneManager.LoadScene(GameScene);
    }
}
