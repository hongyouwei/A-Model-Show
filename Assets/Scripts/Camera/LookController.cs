using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 这个脚本挂在相机上，然后 target 指向目标 //
public class LookController : MonoBehaviour
{

    Vector2 _lastPos = Vector2.zero;
    Vector2 m_lastPos
    {
        get { return _lastPos; }
        set {
            var dp = _lastPos - value;
            if (dp.magnitude > 0.1f)
            {
                Debug.LogError("last pos change from " + _lastPos + " to " + value);
            }
            _lastPos = value;
        }
    }

    public static bool MoveToBack { get; internal set; }

    public GameObject m_target;
    [Tooltip("设置距离角色的距离")] //
    public float m_distance = 20;// 设置距离角色的距离
    [Tooltip("设置镜头斜视的角度")] //
    public float m_viewAngle = 30; // 设置镜头斜视的角度


    void Awake()
    {
    }

    void OnDestroy()
    {

    }

    void OnEnable()
    {
        FingerGestures.OnFingerDown += FingerGestures_OnFingerDown;
        FingerGestures.OnFingerUp += FingerGestures_OnFingerUp;
        //FingerGestures.OnFingerMove += FingerGestures_OnFingerMove;


        FingerGestures.OnPinchBegin += FingerGestures_OnPinchBegin;
#if UNITY_EDITOR
        FingerGestures.OnPinchMove += FingerGestures_OnPinchMove;
#endif
        FingerGestures.OnPinchEnd += FingerGestures_OnPinchEnd;
    }

    void OnDisable()
    {
        FingerGestures.OnFingerDown -= FingerGestures_OnFingerDown;
        FingerGestures.OnFingerUp -= FingerGestures_OnFingerUp;
        //FingerGestures.OnFingerMove -= FingerGestures_OnFingerMove;


        FingerGestures.OnPinchBegin -= FingerGestures_OnPinchBegin;
#if UNITY_EDITOR
        FingerGestures.OnPinchMove -= FingerGestures_OnPinchMove;
#endif
        FingerGestures.OnPinchEnd -= FingerGestures_OnPinchEnd;
    }

    void Start()
    {
        if (m_target != null)
        {
            transform.rotation = Quaternion.Euler(m_viewAngle, m_target.transform.rotation.eulerAngles.y, 0);
            transform.position = transform.rotation * new Vector3(0, 0, -m_distance) + m_target.transform.position;
        }
    }

    void Update()
    {
        if(fingerMap.Count > 0)
        {
            var iter = fingerMap.GetEnumerator();
            while (iter.MoveNext())
            {
                var key = iter.Current.Key;
                Vector2 lastPos = fingerPosMap[key];
                Vector2 currPos = iter.Current.Value.RealPos;
                var dpSqDis = (lastPos - currPos).sqrMagnitude;
                if(dpSqDis > 1)
                {
                    FingerGestures_OnFingerMove(key, currPos);
                }
            }
        }
    }
    void LateUpdate()
    {
        if (m_target != null)
        {
            HandleCamearDistance(m_distance);
        }


        if (JoyStickControl.TheMode == JoyStickMode.JoyStick)
        {
            var pos = fingerMap[JoyStickFinger].RealPos;
            var pos2 = fingerPosMap[JoyStickFinger];
            var dp = pos - pos2;
            //if(dp.magnitude > 0.5f)
            //{
            //    Debug.LogError("mother fucker... " + dp.magnitude);
            //}
            //else
            //{
            //    Debug.LogError("input pos... " + pos);
            //}
            //MapArrowController.UpdateArrow(pos);
        }
        else
        {
            //MapArrowController.HideArrow();
        }

        HandlePinch();
    }
    private void HandleCamearDistance(float distance)
    {
        var zoonDistance = MapCameraConfig.Singleton.m_cameraZoomDistance;
        if (distance < zoonDistance.x) distance = zoonDistance.x;
        if (distance > zoonDistance.y) distance = zoonDistance.y;

        //if (distance != m_distance)
        {
            if (MoveToBack)
            {
                var moveBackTime = MapCameraConfig.Singleton.MoveToHumanBackTime;
                if(moveBackTime > 1)
                {
                    var angle = this.transform.eulerAngles;
                    var targetAngle = m_target.transform.eulerAngles;
                    targetAngle.y -= 180;
                    if (targetAngle.y < 0) targetAngle.y += 360;

                    var angleY = angle.y + (targetAngle.y - angle.y) * Time.deltaTime / moveBackTime;
                    angle.y = angleY;

                    this.transform.eulerAngles = angle;
                }
            }
            var targetPos = this.transform.forward * -m_distance + m_target.transform.position;
            transform.position = targetPos;// transform.position * 0.05f + 0.95f * targetPos;
            //transform.position = transform.position * 0.05f + 0.95f * targetPos;
            m_distance = distance;
        }
    }


    private static int JoyStickFinger = -1;
    void FingerGestures_OnFingerDown(FingerGestures.Finger finger )
    {
        int fingerIndex = finger.Index;
        Vector2 fingerPos = finger.Position;
        if (UICamera.Raycast(fingerPos))
        {
            fingerMap.Remove(fingerIndex); //even though this should not happen. Just put here in case..
            //fingerMap[fingerIndex] = null;
            return;
        }


        fingerMap[fingerIndex] = finger;
        fingerPosMap[fingerIndex] = fingerPos;
//        Debug.LogError("FingerGestures_OnFingerDown " + fingerIndex + " " + fingerPos +  "   " + Time.frameCount);
        //check whether this is joy stick. If joy stick mode, this finger will be ignored.
        if (CheckWhetherJoyStickFinger(fingerIndex, fingerPos))
        {
            if(JoyStickControl.TheMode == JoyStickMode.None)
            {
                //Debug.LogError("joy stick pos " + fingerPos);
                JoyStickControl.TheMode = JoyStickMode.JoyStick;
                JoyStickFinger = fingerIndex;
            }
        }
        else
        {
            if (JoyStickControl.TheMode == JoyStickMode.None)
            {
                JoyStickControl.TheMode = JoyStickMode.DragControl;
            }

            if (lastFingerIndex == -1)
            {
                lastFingerIndex = fingerIndex;
            }
        }
    }

    public static bool CheckWhetherJoyStickFinger(int fingerIndex, Vector2 fingerPos)
    {
        if(MapCameraConfig.IsInJoyStickArea(fingerPos))
        {
            return true;
        }
        return false;
    }

    void FingerGestures_OnFingerUp(int fingerIndex, Vector2 fingerPos, float timeHeldDown)
    {
        //not in the finger map. just ignore!
        if (fingerMap.ContainsKey(fingerIndex) == false) return;

        fingerPosMap.Remove(fingerIndex);
        fingerMap.Remove(fingerIndex);
        twoFingerDistance = -1;
        if (fingerIndex == JoyStickFinger)
        {
            JoyStickFinger = -1;
            if (JoyStickControl.TheMode == JoyStickMode.JoyStick)
            { 
                JoyStickControl.TheMode = JoyStickMode.None; //give up joy stick control
                if(OnJoyFingerEndEvent != null)
                {
                    OnJoyFingerEndEvent(fingerPos);
                }
            }
        }
        else if(lastFingerIndex == fingerIndex)
        { 
            lastFingerIndex = -1;
            if (JoyStickControl.TheMode == JoyStickMode.DragControl)
            {
                if (fingerMap.Count == 0)
                { 
                    JoyStickControl.TheMode = JoyStickMode.None; //give up joy stick control
                }
                else
                {
                    var iter = fingerMap.GetEnumerator();
                    while (iter.MoveNext())
                    {
                        lastFingerIndex = iter.Current.Key;
                    }
                }
             }
        }
        return;
    }


    public delegate void OnJoyStickFingerMove(Vector2 fingerPos);
    public static OnJoyStickFingerMove OnJoyFingerMoveEvent;
    public static OnJoyStickFingerMove OnJoyFingerEndEvent;


    Dictionary<int, Vector2> fingerPosMap = new Dictionary<int, Vector2>();
    Dictionary<int, FingerGestures.Finger> fingerMap = new Dictionary<int, FingerGestures.Finger>();
    private int lastFingerIndex = -1;
    void FingerGestures_OnFingerMove(int fingerIndex, Vector2 fingerPos)
    {
        //not in the finger map. just ignore!
        if (fingerMap.ContainsKey(fingerIndex) == false) return;

        //Debug.LogError("FingerGestures_OnFingerMove " + fingerPos);
        var lastFingerPos = fingerPosMap[fingerIndex];
        fingerPosMap[fingerIndex] = fingerPos;
        if (GameUtil.IsReporterOn)
        {
            return;
        }

        CheckWhetherJoyStickFinger(fingerIndex, fingerPos);
        if (fingerIndex == JoyStickFinger)
        {
            if (OnJoyFingerMoveEvent != null)
            {
                OnJoyFingerMoveEvent(fingerPos);
            }
            return;
        }
        if (JoyStickControl.TheMode == JoyStickMode.DragControl)
        {
            if(fingerPosMap.Count >= 2)
            {
                return; //too many fingers... in the pinch mode
            }
        }

        if (lastFingerIndex != fingerIndex)
        {
            Debug.LogError("lastFingerIndex not match return " + lastFingerIndex + " vs " + fingerIndex);
            return;
        }

        if (Time.time - lastPinchEndTime < 0.25f)
        {
            return;
        }
        //Debug.LogError("FingerGestures_OnFingerMove " + fingerIndex + " " + fingerPos);


        var dp = fingerPos - lastFingerPos;

        var rotateScale = MapCameraConfig.Singleton.m_rotateScale;
        this.transform.Rotate(0, dp.x / rotateScale.x, 0, Space.World);
        var angle = this.transform.eulerAngles;
        var angleX = angle.x - dp.y / rotateScale.y;


        var rotateAngleY = MapCameraConfig.Singleton.m_rotateAngleY;
        if (angleX > rotateAngleY.y) angleX = rotateAngleY.y;
        if (angleX < rotateAngleY.x) angleX = rotateAngleY.x;
        angle.x = angleX;
        this.transform.eulerAngles = angle;

        HandleCamearDistance(m_distance);

        lastFingerPos = fingerPos;
    }


    void FingerGestures_OnPinchBegin(Vector2 fingerPos1, Vector2 fingerPos2)
    {
        if (JoyStickControl.TheMode == JoyStickMode.JoyStick)
        {
        }
        else
        {
        }
    }


    float twoFingerDistance = -1;
    void HandlePinch()
    {
        if (GameUtil.IsReporterOn) return;

        if (JoyStickControl.TheMode == JoyStickMode.JoyStick)
        {
            return;
        }
        if (fingerMap.Count < 2) return;

        if(twoFingerDistance < 0)
        {
            var dp = fingerMap[0].RealPos - fingerMap[1].RealPos;
            twoFingerDistance = dp.magnitude;
        }
        else
        {
            var dp = fingerMap[0].RealPos - fingerMap[1].RealPos;
            var fingerDis = dp.magnitude;
            var delta = fingerDis - twoFingerDistance;
            twoFingerDistance = fingerDis;


            Transform t = this.transform;
            var distance = m_distance;
            distance -= delta / MapCameraConfig.Singleton.m_cameraZoomScale;

            HandleCamearDistance(distance);
        }
    }

    //for editor mouse used only...
    void FingerGestures_OnPinchMove(Vector2 fingerPos1, Vector2 fingerPos2, float delta)
    {
        if (GameUtil.IsReporterOn) return;

        if (JoyStickControl.TheMode == JoyStickMode.JoyStick)
        {
            return;
        }
        Transform t = this.transform;


        var distance = m_distance;
        distance -= delta / MapCameraConfig.Singleton.m_cameraZoomScale;

        HandleCamearDistance(distance);
    }

    float lastPinchEndTime = 0;
    void FingerGestures_OnPinchEnd(Vector2 fingerPos1, Vector2 fingerPos2)
    {
        lastPinchEndTime = Time.time;
    }

}


public enum JoyStickMode
{
    None,
    JoyStick,
    DragControl,
}

public class JoyStickControl
{
    public static JoyStickMode TheMode = JoyStickMode.None;
}