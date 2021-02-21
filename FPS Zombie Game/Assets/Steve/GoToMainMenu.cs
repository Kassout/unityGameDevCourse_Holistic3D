using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToMainMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Invoke("SwitchToMain", 5);
    }

    private void SwitchToMain()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
