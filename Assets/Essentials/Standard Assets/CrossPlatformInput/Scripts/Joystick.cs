using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityStandardAssets.CrossPlatformInput
{
	public class Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
	{
		public enum AxisOption
		{
			// Options for which axes to use
			Both, // Use both
			OnlyHorizontal, // Only horizontal
			OnlyVertical // Only vertical
		}

        public Image lockIcon;
        public Image lockWaitIcon;
		public int MovementRange = 100;
		public AxisOption axesToUse = AxisOption.Both; // The options for the axes that the still will use
		public string horizontalAxisName = "Horizontal"; // The name given to the horizontal axis for the cross platform input
		public string verticalAxisName = "Vertical"; // The name given to the vertical axis for the cross platform input

        private bool isSprintLocked = false;
        private bool isWaitingToLock = false;
        
		Vector3 m_StartPos;
		bool m_UseX; // Toggle for using the x axis
		bool m_UseY; // Toggle for using the Y axis
		CrossPlatformInputManager.VirtualAxis m_HorizontalVirtualAxis; // Reference to the joystick in the cross platform input
		CrossPlatformInputManager.VirtualAxis m_VerticalVirtualAxis; // Reference to the joystick in the cross platform input

		void OnEnable()
		{
			CreateVirtualAxes();
		}

        void Start()
        {
            m_StartPos = transform.position;
        }

		void UpdateVirtualAxes(Vector3 value)
		{
			var delta = m_StartPos - value;
			delta.y = -delta.y;
			delta /= MovementRange;
			if (m_UseX)
			{
				m_HorizontalVirtualAxis.Update(-delta.x);
			}

			if (m_UseY)
			{
				m_VerticalVirtualAxis.Update(delta.y);
			}
		}

		void CreateVirtualAxes()
		{
			// set axes to use
			m_UseX = (axesToUse == AxisOption.Both || axesToUse == AxisOption.OnlyHorizontal);
			m_UseY = (axesToUse == AxisOption.Both || axesToUse == AxisOption.OnlyVertical);

			// create new axes based on axes to use
			if (m_UseX)
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
            }
			if (m_UseY)
			{
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
		}


		public void OnDrag(PointerEventData data)
		{
			Vector3 newPos = Vector3.zero;
			if (m_UseX)
			{
				int delta = (int)(data.position.x - m_StartPos.x);
				delta = Mathf.Clamp(delta, - MovementRange, MovementRange);
				newPos.x = delta;
			}
			if (m_UseY)
			{
				int delta = (int)(data.position.y - m_StartPos.y);
				delta = Mathf.Clamp(delta, -MovementRange, MovementRange);
				newPos.y = delta;
                if (delta > (MovementRange - (MovementRange / 10)))
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
                else if(isWaitingToLock) 
                {
                    isWaitingToLock = false;
                }
            }
			transform.position = new Vector3(m_StartPos.x + newPos.x, m_StartPos.y + newPos.y, m_StartPos.z + newPos.z);
			UpdateVirtualAxes(transform.position);
		}

        private IEnumerator WaitCheckForLock() 
        {
            if (isWaitingToLock) 
            {
                int i = 0;
                while(i < 5)
                {
                    lockWaitIcon.enabled = !lockWaitIcon.enabled;
                    i++;
                    if (isWaitingToLock) 
                    {
                        yield return new WaitForSeconds(0.25f);
                    }
                    else{break;}
                }
                if (isWaitingToLock) 
                {
                    lockWaitIcon.enabled = false;
                    lockIcon.enabled = true;
                    isSprintLocked = true;
                    transform.position = m_StartPos;
                }
                else 
                {
                    lockWaitIcon.enabled = false;
                }
            }
            else 
            {
            }
        }


		public void OnPointerUp(PointerEventData data)
		{
            transform.position = m_StartPos;
            if (isSprintLocked) 
            {
                UpdateVirtualAxes(new Vector3(m_StartPos.x, transform.position.y, m_StartPos.z));
            }
            else 
            {
                UpdateVirtualAxes(m_StartPos);
            }
            if (isWaitingToLock) 
            {
                isWaitingToLock = false;
            }
		}


		public void OnPointerDown(PointerEventData data) 
        {
            transform.position = m_StartPos;
            if (isSprintLocked) 
            {
                isSprintLocked = false;
                lockIcon.enabled = false;
                UpdateVirtualAxes(m_StartPos);
            }
        }

		void OnDisable()
		{
			if (m_UseX)
			{
				m_HorizontalVirtualAxis.Remove();
			}
			if (m_UseY)
			{
				m_VerticalVirtualAxis.Remove();
			}
		}
	}
}