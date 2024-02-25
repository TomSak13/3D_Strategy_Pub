using UnityEngine.SceneManagement;

public static class SceneChanger
{
    public const string GameScene = "SampleScene";

    public static void LoadGameScene(string gameScene)
    {
        SceneManager.LoadScene(gameScene);
    }
}
