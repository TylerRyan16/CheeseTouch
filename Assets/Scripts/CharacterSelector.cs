using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class CharacterStats
{
    public float health;
    public float speed;
    public float attack;
    public float stamina;
}
public class CharacterSelector : MonoBehaviour
{
    public Sprite greg;
    public Sprite rodrick;
    public Sprite manny;
    public Sprite frank;
    public Sprite susan;
    public Sprite rowley;

    public CharacterStats gregStats;
    public CharacterStats rodrickStats;
    public CharacterStats mannyStats;
    public CharacterStats frankStats;
    public CharacterStats susanStats;
    public CharacterStats rowleyStats;

    private Sprite currentCharacter;
    private CharacterStats currentStats;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);  
    }

    public void SelectGregAndSwitchScene()
    {
        SetCharacterAndSwitchScene(greg, gregStats, "Basketball_Court");
    }

    public void SelectRodrickAndSwitchScene()
    {
        SetCharacterAndSwitchScene(rodrick, rodrickStats, "Basketball_Court");
    }

    public void SelectMannyAndSwitchScene()
    {
        SetCharacterAndSwitchScene(manny, mannyStats, "Basketball_Court");
    }

    public void SelectFrankAndSwitchScene()
    {
        SetCharacterAndSwitchScene(frank, frankStats, "Basketball_Court");
    }

    public void SelectSusanAndSwitchScene()
    {
        SetCharacterAndSwitchScene(susan, susanStats, "Basketball_Court");
    }

    public void SelectRowleyAndSwitchScene()
    {
        SetCharacterAndSwitchScene(rowley, rowleyStats, "Basketball_Court");
    }

    private void SetCharacterAndSwitchScene(Sprite character, CharacterStats stats, string sceneName)
    {
        this.currentCharacter = character;
        this.currentStats = stats;
        SceneManager.LoadScene(sceneName);
    }

    public Sprite GetCurrentCharacter()
    {
        return currentCharacter;
    }

    public CharacterStats GetCurrentStats()
    {
        return currentStats;
    }
}
