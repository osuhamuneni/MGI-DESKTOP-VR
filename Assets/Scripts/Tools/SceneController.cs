using UnityEngine.SceneManagement;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    private string _sceneToReload;
    public string SceneToReload { get { return _sceneToReload; } set { _sceneToReload = value; } }
    void OnEnable()
    {
        if(_sceneToReload == null)
            SceneToReload = SceneManager.GetActiveScene().name;
    }
}
