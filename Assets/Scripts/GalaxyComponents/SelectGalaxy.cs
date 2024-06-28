using System.Collections;
using UnityEngine;

public class SelectGalaxy : MonoBehaviour
{
    private FITSObject _fITSObject3;
    // Start is called before the first frame update
    void Start()
    {
        _fITSObject3 = GetComponent<FITSObject>();
    }

    private void OnMouseDown()
    {
        if (!GalaxyManager.manager.useRayCastForSelection)
        {
            GalaxyManager.manager.IsGalaxySelected = true;
            StartCoroutine(WaitOneFrameRoutine(() =>
            {
                //GalaxyManager.SelectGalaxy(_fITSObject3.CameraControl.transform);
                GalaxyManager.GetGalaxyColors(this.gameObject);
                GalaxyManager.manager.onSelectEvent.Invoke();
                GalaxyManager.SetSelectedGalaxy(this.gameObject);
                _fITSObject3.ShowHideUIInfos(true);
            }));
        }
    }

    private IEnumerator WaitOneFrameRoutine(System.Action action)
    {
        yield return null;
        // Code here will execute on the next frame
        action.Invoke();
    }
}
