using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleButton : MonoBehaviour
{
    public GameObject modalWindow;
    public void clicked(string button)
    {
    ARDebugManager.Instance.LogInfo($"{button} button clicked!");
    switch(button)
    {
        case "openColorModal":
            modalWindow.SetActive(true);
            break;
        case "closeColorModal":
            modalWindow.SetActive(false);
            break;
    }
    
    }
}
