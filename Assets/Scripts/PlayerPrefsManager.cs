using UnityEngine;

public class PlayerPrefsManager : MonoBehaviour {

    const string VS_COMPUTER = "vsComputer";
        
	public static void SetVsComputer(bool vsComputer) {
        PlayerPrefs.SetInt(VS_COMPUTER, vsComputer ? 1 : 0);
    }

    public static bool GetVsComputer() {
        return PlayerPrefs.GetInt(VS_COMPUTER) == 1;
    }
}
