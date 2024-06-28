using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneCheker : MonoBehaviour
{
    void OnEnable()
    {
        if (FindObjectOfType<ConfigManager>() == null)
            SceneManager.LoadScene(0);//load loader scene
    }
}
