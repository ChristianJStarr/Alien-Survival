using TMPro;
using UnityEngine;

public class GetVersionText : MonoBehaviour
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
