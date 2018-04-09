using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Windows : MonoBehaviour {

    public GameObject CurrentWindow,
                      messageBoxWindow;
    public Text tx_messageBox;


                                        // can't integrate multiarg with unity button event
    public void OpenWindow(GameObject window /*, bool closeCurrent = true*/) 
    {
        if(CurrentWindow!=null)
            CurrentWindow.SetActive(false);
        CurrentWindow = window;
        CurrentWindow.SetActive(true);
    }

    public void OpenOverWindow(GameObject window /*, bool closeCurrent = true*/)
    {
        CurrentWindow = window;
        CurrentWindow.SetActive(true);
    }

    public void CloseWindow(GameObject window)
    {
        window.SetActive(false);
    }

    public void MessageBox (string message)
    {
        tx_messageBox.text = message;
        OpenWindow(messageBoxWindow);
    }

}
