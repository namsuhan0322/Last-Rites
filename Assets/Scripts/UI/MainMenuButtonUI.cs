using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuButtonUI : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject settingMenu;

    public void BacktoMenu()
    {
        settingMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void OpenSetting()
    {
        settingMenu.SetActive(true);
        mainMenu.SetActive(false);
    }
}
