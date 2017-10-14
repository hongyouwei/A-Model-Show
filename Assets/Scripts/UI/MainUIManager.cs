using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LCPrinter;

public class MainUIManager : MonoBehaviour {
    [Tooltip("用来截屏的相机")]
    public Camera m_PicCamera;      // 用来截屏的相机
    [Tooltip("打印名字")]
    public string printerName = ""; // 打印名字
    [Tooltip("打印份数")]
    public int copies = 1;      // 打印份数

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    // 截屏打印
    public void GetPhoto(GameObject obj)
    {
        StartCoroutine(StartPrintScreen(false));
    }

    IEnumerator StartPrintScreen(bool toPrint)
    {
        m_PicCamera.gameObject.SetActive(true);
        yield return new WaitForEndOfFrame();
        GetCameraPic.CaptureCamera(m_PicCamera, new Rect(0, 0, Screen.width, Screen.height));
        yield return new WaitForEndOfFrame();
        m_PicCamera.gameObject.SetActive(false);

        if (toPrint) {
            Print.PrintTextureByPath(GetCameraPic.GetPath(), copies, printerName);
        }
    }

    // 执行打印
    public void DoPrint(GameObject obj)
    {
        StartCoroutine(StartPrintScreen(true));
    }
}
