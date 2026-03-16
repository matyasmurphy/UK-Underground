using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerScript : MonoBehaviour
{
    public void ChangeToSampleScene()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
