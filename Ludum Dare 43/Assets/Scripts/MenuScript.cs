using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    
    public void Play()
    {
        GetComponent<Animator>().SetTrigger("Transition");
        StartCoroutine(
            FindObjectOfType<SceneFader>().FadeAndLoadScene(SceneFader.FadeDirection.In, "_GAME"));
    }
    
    public void Help()
    {
        GetComponent<Animator>().SetTrigger("Transition");
        StartCoroutine(
            FindObjectOfType<SceneFader>().FadeAndLoadScene(SceneFader.FadeDirection.In, "_HELP"));
    }
    
}