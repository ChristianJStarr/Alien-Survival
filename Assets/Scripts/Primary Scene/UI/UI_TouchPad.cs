using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(Image))]
public class UI_TouchPad : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public string horizontalAxisName = "TouchHorizontal";
    public string verticalAxisName = "TouchVertical";

    private CrossPlatformInputManager.VirtualAxis m_HorizontalVirtualAxis;
    private CrossPlatformInputManager.VirtualAxis m_VerticalVirtualAxis;
    private Image m_Image;
    private Vector3 m_Center;
    private Vector2 m_Previous;
    private float tune = 2;
    private int m_Id = -1;
    private bool m_Dragging = false;
    private bool reset = true;

    //Configuration
    private bool C_useAcceleration = true;
    private int C_maxInputMovement = 40;
    private float C_linearity = 2.08F;
    private float C_intersection = 10.86F; 


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
        Vector2 pointerDelta;
#if !UNITY_EDITOR
        m_Center = m_Previous;
        m_Previous = Input.touches[m_Id].position;
        if (reset)
        {
            m_Center = Input.touches[m_Id].position;
            reset = false;
        }
        pointerDelta = new Vector2(Input.touches[m_Id].position.x - m_Center.x, Input.touches[m_Id].position.y - m_Center.y);   
#else
        pointerDelta = new Vector2(Input.mousePosition.x - m_Previous.x, Input.mousePosition.y - m_Previous.y);
        m_Previous = Input.mousePosition;
#endif
        return pointerDelta;
    }

    //Apply Acceleration to Vector 
    private Vector2 ApplyAcceleration(Vector2 input) 
    {
        if (C_useAcceleration) 
        {
            // (1/(int+lin))*  x*(Abs(x)+lin)
            input.x = (1F / (C_intersection + C_linearity)) * input.x * (Mathf.Abs(input.x) * C_linearity);
            input.y = (1F / (C_intersection + C_linearity)) * input.y * (Mathf.Abs(input.y) * C_linearity);
            input.x = Mathf.Clamp(input.x, -C_maxInputMovement, C_maxInputMovement);
            input.y = Mathf.Clamp(input.y, -C_maxInputMovement, C_maxInputMovement);
        }
        return input;
    }
}