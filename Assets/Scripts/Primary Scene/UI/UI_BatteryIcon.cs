using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_BatteryIcon : MonoBehaviour
{
    float batteryLevel = 1;
    bool isCharging = false;
    private bool showBattery = false;
    public Settings settings;

    [SerializeField] Image bkgIcon;
    [SerializeField] RectTransform rect;

    public GameObject batteryObject;

    private void OnEnable()
    {
        SettingsMenu.ChangedSettings += Change;//Subscribe to Settings Change Event.
    }

    private void OnDisable()
    {
        SettingsMenu.ChangedSettings -= Change;//unSubscribe to Settings Change Event.
    }


    private void Start()
    {
        Change();
    }



    //Update the Battery Icon
    private void UpdateBatteryIcon() 
    {
        bkgIcon.color = GetColorFromLevel();
        rect.sizeDelta = new Vector2(GetWidthFromLevel(), rect.sizeDelta.y);
    }

    //Get Battery Icon Color From Battery Level
    private Color32 GetColorFromLevel()
    {
        
        if (!isCharging) 
        {
            if (batteryLevel < 0.20)
            {
                return new Color32(224, 40, 40, 255);
            }
            else if (batteryLevel < 0.35)
            {
                return new Color32(224, 224, 40, 255);
            }
            else
            {
                return new Color32(95, 238, 150, 255);
            }
        }
        else 
        {
            return new Color32(95, 238, 150, 255);
        }
    }

    //Get Battery Icon Width From Battery Level
    private float GetWidthFromLevel() 
    {
        if (!isCharging)
        {
            return Mathf.Clamp(batteryLevel * 20, 4, 18);
        }
        else
        {
            return 18;
        }
    }

    //Change Settings.
    private void Change()
    {
        if(showBattery != settings.showBattery) 
        {
            showBattery = settings.showBattery;
            batteryObject.SetActive(showBattery);
            StartCoroutine(BatteryCheckLoop());
        }
    }

    //Battery Check Loop (3 Seconds)
    private IEnumerator BatteryCheckLoop() 
    {
        WaitForSeconds wait = new WaitForSeconds(3);
        while (true) 
        {
            if (showBattery) 
            {
                if (isCharging != (BatteryStatus.Charging == SystemInfo.batteryStatus))
                {
                    isCharging = (BatteryStatus.Charging == SystemInfo.batteryStatus);
                    if (batteryLevel != SystemInfo.batteryLevel)
                    {
                        batteryLevel = SystemInfo.batteryLevel;
                        UpdateBatteryIcon();
                    }
                }
                else if (batteryLevel != SystemInfo.batteryLevel)
                {
                    batteryLevel = SystemInfo.batteryLevel;
                    UpdateBatteryIcon();
                }
                yield return wait;
            }
            else 
            {
                break;
            }
        }
    }
}
