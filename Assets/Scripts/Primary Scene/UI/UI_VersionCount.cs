using TMPro;
using UnityEngine;

public class UI_VersionCount : MonoBehaviour
{
    public TextMeshProUGUI versionText;

    private void Start()
    {
        if (versionText != null) 
        {
            versionText.text = "v" + Application.version;
        }
        else
        {
            Destroy(this);
        }
    }
}
