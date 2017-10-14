using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCameraConfig : MonoBehaviour {

    [Tooltip("手指移动的像素变化 除以 这个值")]
    public Vector2 m_rotateScale = new Vector2(3.5f, 5);

    [Tooltip("Y旋转变化值，如(0, 60）就是说最小为0， 最大为60")]
    public Vector2 m_rotateAngleY = new Vector2(0, 60);

    [Tooltip("缩放的像素变化 除以 这个值")]
    public float m_cameraZoomScale = 40.0f;

    [Tooltip("摄像机离人的最小与最大距离")]
    public Vector2 m_cameraZoomDistance = new Vector2(4, 12);



    [Tooltip("摇杆中心对应的像素位置")]
    public Vector2 m_JoyStickCenter = new Vector2(175, 175);

    [Tooltip("摇杆的半径")]
    public float m_joyStickRadius = 130f;

    [Tooltip("摇杆的半径扩展(像素）")]
    public float m_joyStickRadiusOffSet = 5f;

    [Tooltip("摄像机会缓慢转到人后的时间, Set to -1 to disable this")]
    public float MoveToHumanBackTime = 5f;

    public static MapCameraConfig Singleton;
    // Use this for initialization
    void Awake ()
    {
        Singleton = this;
        DontDestroyOnLoad(this.gameObject);

        AddMoreConnections = false;
    }

    public bool AddMoreConnections = false;
    public bool ShowMsgCount = false;
    void Update()
    {
       
    }

    public static bool IsInJoyStickArea(Vector2 fingerPos)
    {
        var dp = fingerPos - Singleton.m_JoyStickCenter;
        var threshold = Singleton.m_joyStickRadius;
        if ((dp.x < threshold && dp.x > -threshold) && (dp.y < threshold && dp.y > -threshold))
        {
            return true;
        }
        return false;
    }

    internal void SetJoyStickRadius(float magnitude)
    {
        this.m_joyStickRadius = magnitude + this.m_joyStickRadiusOffSet;
    }

    internal static Vector2 ConvertToJoyStickAxis(Vector2 fingerPos)
    {
        var dp = fingerPos - Singleton.m_JoyStickCenter;
        dp /= Singleton.m_joyStickRadius;
        if (dp.x > 1) dp.x = 1;
        if (dp.x < -1) dp.x = -1;

        if (dp.y > 1) dp.y = 1;
        if (dp.y < -1) dp.y = -1;
        return dp;
    }
}
