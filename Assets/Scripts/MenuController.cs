using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour {

    public Toggle toggle;

    // Use this for initialization
    void Start() {
        toggle.isOn = PlayerPrefsManager.GetVsComputer();
    }

    public void on3Button() {
        PlayerPrefsManager.SetVsComputer(toggle.isOn);
        SceneManager.LoadScene("3board");
    }

    public void on4Button() {
        PlayerPrefsManager.SetVsComputer(toggle.isOn);
        SceneManager.LoadScene("4board");
    }
}
