using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityStandardAssets.CrossPlatformInput;
using System;
using UnityStandardAssets.Characters.FirstPerson;

public class KinematicDebug : MonoBehaviour
{
    public GameObject network_Player;
    public TextMeshProUGUI speed_Value;
    public TextMeshProUGUI simulate_text;
    public Slider speed_Slider;
    private float speed = 0;
    private bool isSimulating = false;
    private Vector3 start_Pos;
    private Animator player_Animator;
    FirstPersonController fps;
    CharacterController character_controller;

    CrossPlatformInputManager.VirtualAxis m_HorizontalVirtualAxis; 
    CrossPlatformInputManager.VirtualAxis m_VerticalVirtualAxis;

    private void Start()
    {
        character_controller = network_Player.GetComponent<CharacterController>();
        fps = network_Player.GetComponent<FirstPersonController>();
        m_HorizontalVirtualAxis = new CrossPlatformInputManager.VirtualAxis("Horizontal");
        CrossPlatformInputManager.RegisterVirtualAxis(m_HorizontalVirtualAxis);
    
		m_VerticalVirtualAxis = new CrossPlatformInputManager.VirtualAxis("Vertical");
		CrossPlatformInputManager.RegisterVirtualAxis(m_VerticalVirtualAxis);
        start_Pos = network_Player.transform.position;
        player_Animator = network_Player.GetComponent<Animator>();
    }

    private void Update()
    {
        if(network_Player.transform.position.z >= 400) 
        {
            ResetLocation();
        }
    }

    public void SliderChange() 
    {
        speed = speed_Slider.value;
        speed_Value.text = "SPEED: " + speed.ToString("F2");

        if (isSimulating) 
        {
            m_VerticalVirtualAxis.Update(speed);
            m_VerticalVirtualAxis.Update(speed);
        }
    }
    public void ResetWalk()
    {
        isSimulating = false;
        simulate_text.text = "SIMULATE";
        m_VerticalVirtualAxis.Update(0);
        ResetLocation();
    }
    public void SimulateWalk() 
    {
        Debug.Log("Simulate Walk");
        if (isSimulating) 
        {
            isSimulating = false;
            simulate_text.text = "SIMULATE";
            m_VerticalVirtualAxis.Update(0);
        }
        else 
        {
            isSimulating = true;
            simulate_text.text = "PAUSE";
            m_VerticalVirtualAxis.Update(speed);
        }
    }
    public void ResetLocation() 
    {
        fps.enabled = false;
        character_controller.enabled = false;
        network_Player.transform.position = start_Pos;
        character_controller.enabled = true;
        fps.enabled = true;
    }
}
