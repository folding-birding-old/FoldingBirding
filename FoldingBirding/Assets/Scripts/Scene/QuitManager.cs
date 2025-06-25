using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitManager : MonoBehaviour
{
    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Three, OVRInput.Controller.LTouch))
        {
            Debug.Log("[EXIT] X 버튼이 눌려 앱을 종료합니다.");
            Application.Quit();
        }
    }

    public void OnClickQuitBtn()
    {
        Application.Quit();
    }
}
