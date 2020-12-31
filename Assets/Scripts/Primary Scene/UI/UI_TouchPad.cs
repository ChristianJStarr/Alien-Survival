using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(Image))]
public class UI_TouchPad : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool acceleration = true;
    public string horizontalAxisName = "TouchHorizontal";
    public string verticalAxisName = "TouchVertical";
    CrossPlatformInputManager.VirtualAxis m_HorizontalVirtualAxis;
    CrossPlatformInputManager.VirtualAxis m_VerticalVirtualAxis;
    bool m_Dragging;
    int m_Id = -1;
    private float tune = 2;
    Vector2 m_PreviousTouchPos;
    private bool reset = true;
    private Vector3 m_Center;
    private Image m_Image;
    Vector2 m_PreviousMouse;


    void OnEnable()
    {
        CreateVirtualAxes();
    }
    void OnDisable()
    {

        if (CrossPlatformInputManager.AxisExists(horizontalAxisName))
            CrossPlatformInputManager.UnRegisterVirtualAxis(horizontalAxisName);

        if (CrossPlatformInputManager.AxisExists(verticalAxisName))
            CrossPlatformInputManager.UnRegisterVirtualAxis(verticalAxisName);
    }

    void Start()
    {
#if !UNITY_SERVER
        m_Image = GetComponent<Image>();
        m_Center = m_Image.transform.position;
#else
        enabled = false;
#endif
    }
    void Update()
    {
        if (!m_Dragging)
        {
            return;
        }
        if (Input.touchCount >= m_Id + 1 && m_Id != -1)
        {
            UpdateVirtualAxes(ApplyAcceleration(GetPointer()));
        }
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
    void UpdateVirtualAxes(Vector2 value)
    {
        m_HorizontalVirtualAxis.Update(value.x);
        m_VerticalVirtualAxis.Update(value.y);
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

    //Get Current Pointer
    private Vector2 GetPointer() 
    {
#if UNITY_EDITOR
        Vector2 pointerDelta = new Vector2(Input.mousePosition.x - m_PreviousMouse.x, Input.mousePosition.y - m_PreviousMouse.y);
        m_PreviousMouse = Input.mousePosition;
        return pointerDelta;
#else
        m_Center = m_PreviousTouchPos;
        m_PreviousTouchPos = Input.touches[m_Id].position;
        if (reset)
        {
            m_Center = Input.touches[m_Id].position;
            reset = false;
        }
        return new Vector2(Input.touches[m_Id].position.x - m_Center.x, Input.touches[m_Id].position.y - m_Center.y);
#endif
    }

    //Apply Acceleration to Vector
    private Vector2 ApplyAcceleration(Vector2 input) 
    {
        if (acceleration) 
        {
            input *= input;
        }
        return input;
    }
}


// 26 - 26 - 26 TopBkg
// 12 - 12 - 12 Standard Btn
// 173 - 80 - 74 Alien Red
// 63 - 149 - 77 Exp bar
// 58 - 58 - 58 - 130 Trans Grey