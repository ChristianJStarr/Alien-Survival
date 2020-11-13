using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(Image))]
public class TouchPad : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool acceleration = true;
    public string horizontalAxisName = "TouchHorizontal"; // The name given to the horizontal axis for the cross platform input
    public string verticalAxisName = "TouchVertical"; // The name given to the vertical axis for the cross platform input
    public float Xsensitivity = 1f;
    public float Ysensitivity = 1f;
    public Settings settings;
    CrossPlatformInputManager.VirtualAxis m_HorizontalVirtualAxis; // Reference to the joystick in the cross platform input
    CrossPlatformInputManager.VirtualAxis m_VerticalVirtualAxis; // Reference to the joystick in the cross platform input
    bool m_Dragging;
    int m_Id = -1;
    private float tune = 2;
    Vector2 m_PreviousTouchPos;
    private bool reset = true;
    private Vector3 m_Center;
    private Image m_Image;
    Vector3 m_PreviousMouse;


    //Utility
    private UtilAverage verticalAvg;
    private UtilAverage horizontalAvg;



    void OnEnable()
    {
        CreateVirtualAxes();
        SettingsMenu.ChangedSettings += Change;
    }

    void OnDisable()
    {
        SettingsMenu.ChangedSettings -= Change;

        if (CrossPlatformInputManager.AxisExists(horizontalAxisName))
            CrossPlatformInputManager.UnRegisterVirtualAxis(horizontalAxisName);

        if (CrossPlatformInputManager.AxisExists(verticalAxisName))
            CrossPlatformInputManager.UnRegisterVirtualAxis(verticalAxisName);
    }

    void Start()
    {
        Xsensitivity = settings.xSensitivity;
        Ysensitivity = settings.ySensitivity;
        m_Image = GetComponent<Image>();
        m_Center = m_Image.transform.position;

        verticalAvg = new UtilAverage();
        horizontalAvg = new UtilAverage();
        verticalAvg.minMax = true;
        horizontalAvg.minMax = true;
        verticalAvg.avgName = "Touch Vertical ";
        horizontalAvg.avgName = "Touch Horizontal ";
    }

    void Update()
    {
        if (!m_Dragging)
        {
            return;
        }
        if (Input.touchCount >= m_Id + 1 && m_Id != -1)
        {
#if !UNITY_EDITOR
                MobileDragUpdate();
#else
            EditorUpdate();
#endif
        }

    }



    //Change touch sensitivity. Standard Settings Change() function.
    public void Change()
    {
        //Change touch sensitivity.
        Xsensitivity = settings.xSensitivity;
        Ysensitivity = settings.ySensitivity;
    }

    //Create the Virtual Axes
    void CreateVirtualAxes()
    {
        m_HorizontalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(horizontalAxisName);
        CrossPlatformInputManager.RegisterVirtualAxis(m_HorizontalVirtualAxis);
        m_VerticalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(verticalAxisName);
        CrossPlatformInputManager.RegisterVirtualAxis(m_VerticalVirtualAxis);
    }

    //Update the Virtual Axes
    void UpdateVirtualAxes(Vector3 value)
    {
        m_HorizontalVirtualAxis.Update(value.x);
        horizontalAvg.Input(value.x);
        m_VerticalVirtualAxis.Update(value.y);
        verticalAvg.Input(value.y);
    }

    //On Pointer DOWN
    public void OnPointerDown(PointerEventData data)
    {
        m_Dragging = true;
        m_Id = data.pointerId;
    }

    //On Pointer UP
    public void OnPointerUp(PointerEventData data)
    {
        m_Dragging = false;
        m_Id = -1;
        UpdateVirtualAxes(Vector3.zero);
        reset = true;
    }

    //Mobile DRAG 
    private void MobileDragUpdate()
    {
        Vector2 pointerDelta;
        m_Center = m_PreviousTouchPos;
        m_PreviousTouchPos = Input.touches[m_Id].position;
        if (reset)
        {
            m_Center = Input.touches[m_Id].position;
            reset = false;
        }
        tune = 12; // mobileTune // DPI?
        pointerDelta = new Vector2(Input.touches[m_Id].position.x - m_Center.x, Input.touches[m_Id].position.y - m_Center.y);
        //Apply Sensitivity
        pointerDelta.x *= Xsensitivity / tune;
        pointerDelta.y *= Ysensitivity / tune;
        //Apply Speed
        if (acceleration)
        {
            Vector2 speed = pointerDelta;
            if (speed.x < 0) { speed.x *= -1; }
            if (speed.y < 0) { speed.y *= -1; }
            speed /= Time.deltaTime;
            pointerDelta *= speed.normalized;
            pointerDelta.x = Mathf.Clamp(pointerDelta.x, -10, 10);
            pointerDelta.y = Mathf.Clamp(pointerDelta.y, -10, 10);
        }
        //Update Axies
        UpdateVirtualAxes(new Vector3(pointerDelta.x, pointerDelta.y, 0));
    }

    //Editor DRAG
    private void EditorUpdate()
    {
        Vector2 pointerDelta;
        //Get Distance Traveled
        pointerDelta.x = Input.mousePosition.x - m_PreviousMouse.x;
        pointerDelta.y = Input.mousePosition.y - m_PreviousMouse.y;
        tune = 12;
        //Apply Sensitivity
        pointerDelta.x *= Xsensitivity / tune;
        pointerDelta.y *= Ysensitivity / tune;

        //Apply Speed
        if (acceleration)
        {
            Vector2 speed = pointerDelta;
            if (speed.x < 0) { speed.x *= -1; }
            if (speed.y < 0) { speed.y *= -1; }
            speed /= Time.deltaTime;
            pointerDelta *= speed.normalized;
            pointerDelta.x = Mathf.Clamp(pointerDelta.x, -10, 10);
            pointerDelta.y = Mathf.Clamp(pointerDelta.y, -10, 10);
        }
        //Update Previous
        m_PreviousMouse = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f);

        //Update Axies
        UpdateVirtualAxes(new Vector3(pointerDelta.x, pointerDelta.y, 0));

    }

    //GTP, Ghetto Touch Pad
    //Last Modified 10/10
}