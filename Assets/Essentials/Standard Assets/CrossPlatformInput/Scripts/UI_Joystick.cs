using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public class UI_Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public Image lockIcon; //Locked Icon
    public Image lockWaitIcon; //Waiting to Lock Icon
    private int MovementRange = 0; //Movement Range of JoyAxis
    private string horizontalAxisName = "Horizontal"; // The name given to the horizontal axis for the cross platform input
    private string verticalAxisName = "Vertical"; // The name given to the vertical axis for the cross platform input
    private float secondsUntilLock = 1.2F; //Seconds to wait unit locking
    private bool isSprintLocked = false; 
    private bool isWaitingToLock = false;
    private Vector3 joy_Center; //The Start Center of the Joy

    CrossPlatformInputManager.VirtualAxis m_HorizontalVirtualAxis; // Reference to the joystick in the cross platform input
    CrossPlatformInputManager.VirtualAxis m_VerticalVirtualAxis; // Reference to the joystick in the cross platform input

    void OnEnable()
    {
        CreateVirtualAxes();
    }
    void OnDisable()
    {
        m_HorizontalVirtualAxis.Remove();
        m_VerticalVirtualAxis.Remove();
    }
    void Start()
    {
        joy_Center = transform.position;
        MovementRange = Screen.height / 5;
    }

    //Force stop Auto-Sprint
    public void ForceStopAutoSprint() 
    {
        if (isSprintLocked) 
        {
            isSprintLocked = false;
            lockIcon.enabled = false;
            isWaitingToLock = false;
            transform.position = joy_Center;
            UpdateVirtualAxes(transform.position);
        }
    }

    //Update the Virtual Axes
    void UpdateVirtualAxes(Vector3 value)
    {
        var delta = joy_Center - value;
        delta.y = -delta.y;
        delta /= MovementRange;
        m_HorizontalVirtualAxis.Update(-delta.x);
        m_VerticalVirtualAxis.Update(delta.y);
    }

    //Create Virtual Axes
    void CreateVirtualAxes()
    {
        m_HorizontalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(horizontalAxisName);
        if (!CrossPlatformInputManager.AxisExists(horizontalAxisName))
        {
            CrossPlatformInputManager.RegisterVirtualAxis(m_HorizontalVirtualAxis);
        }
        else
        {
            CrossPlatformInputManager.UnRegisterVirtualAxis(horizontalAxisName);
            CrossPlatformInputManager.RegisterVirtualAxis(m_HorizontalVirtualAxis);
        }
        m_VerticalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(verticalAxisName);
        if (!CrossPlatformInputManager.AxisExists(verticalAxisName))
        {
            CrossPlatformInputManager.RegisterVirtualAxis(m_VerticalVirtualAxis);
        }
        else
        {
            CrossPlatformInputManager.UnRegisterVirtualAxis(verticalAxisName);
            CrossPlatformInputManager.RegisterVirtualAxis(m_VerticalVirtualAxis);
        }
    }

    //On Drag Event
    public void OnDrag(PointerEventData data)
    {
        if (!isSprintLocked) 
        {
            Vector3 newPosition = Vector3.zero;
            int deltaY = (int)(data.position.y - joy_Center.y);
            CheckForLock(deltaY);
            newPosition.y = deltaY;
            int deltaX = (int)(data.position.x - joy_Center.x);
            newPosition.x = deltaX;
            newPosition = Vector3.ClampMagnitude(newPosition, MovementRange);
            transform.position = joy_Center + newPosition;
            UpdateVirtualAxes(transform.position);
        }
    }

    //Check For Lock
    private void CheckForLock(float value)
    {
        if (value > (MovementRange * 3 - ((MovementRange) / 5)))
        {
            if (!isSprintLocked && !isWaitingToLock)
            {
                isWaitingToLock = true;
                StartCoroutine(WaitCheckForLock());
            }
        }
        else if (isSprintLocked)
        {
            isSprintLocked = false;
            lockIcon.enabled = false;
            isWaitingToLock = false;
        }
        else if (isWaitingToLock)
        {
            isWaitingToLock = false;
        }
    }

    //Check For Lock Loop
    private IEnumerator WaitCheckForLock()
    {
        if (isWaitingToLock)
        {
            int flashesPerSecond = 2;
            int flashesCount = 0;
            while (flashesCount < secondsUntilLock * flashesPerSecond)
            {
                lockWaitIcon.enabled = !lockWaitIcon.enabled;
                flashesCount++;
                if (isWaitingToLock)
                {
                    yield return new WaitForSeconds(secondsUntilLock / flashesPerSecond);
                }
                else { break; }
            }
            if (isWaitingToLock)
            {
                lockWaitIcon.enabled = false;
                lockIcon.enabled = true;
                isSprintLocked = true;
                transform.position = joy_Center;
            }
            else
            {
                lockWaitIcon.enabled = false;
            }
        }
    }

    //On Pointer Up Event
    public void OnPointerUp(PointerEventData data)
    {
        transform.position = joy_Center;
        if (isSprintLocked)
        {
            UpdateVirtualAxes(new Vector3(joy_Center.x, joy_Center.y + MovementRange, joy_Center.z));
        }
        else
        {
            UpdateVirtualAxes(joy_Center);
        }
        if (isWaitingToLock)
        {
            isWaitingToLock = false;
        }
        Debug.Log(joy_Center + " Firing");
    }

    //On Pointer Down Event
    public void OnPointerDown(PointerEventData eventData)
    {
        if (isSprintLocked) 
        {
            isSprintLocked = false;
            lockIcon.enabled = false;
            isWaitingToLock = false;
        }
    }
}
