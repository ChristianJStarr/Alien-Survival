using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
/// <summary>
/// Apply Game Settings to Engine.
/// </summary>
public class ApplySettings : MonoBehaviour
{
    public Settings settings;

    private void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name; //Get current scene.
        //Check if current scene is the load scene.

        if (sceneName == "LoadScene") 
        {
            if (!settings.validated)
            {
                settings = (Settings)ScriptableObject.CreateInstance("Settings");
                settings.validated = true;
            }


            //Check if quality settings need to be changed.
            if (QualitySettings.GetQualityLevel() != settings.quality)
            {
                //Change quality settings.
                QualitySettings.SetQualityLevel(settings.quality, true);
            }
            RenderPipelineAsset renderAsset = GetAsset(); //Get render pipeline asset that coresonds to stored settings.
            //Check if render pipeline asset needs to be changed.
            if (GraphicsSettings.renderPipelineAsset != renderAsset)
            {
                //Change render pipeline asset.
                GraphicsSettings.renderPipelineAsset = renderAsset;
            }
        }
        //Change camera in current scene to correct settings.
        int set_3 = settings.postpro; //Settings Post Processing
        if(Camera.main != null) 
        {
            var cameraData = Camera.main.GetUniversalAdditionalCameraData();
            if (set_3 == 1)
            {
                //Turn off post processing.
                cameraData.renderPostProcessing = false;
            }
            else
            {
                //Turn off post processing.
                cameraData.renderPostProcessing = true;
            }
        }
    }
    

    //Get RenderPipeline Asset
    public RenderPipelineAsset GetAsset()
    {
        //Get current stored settings.
        int set_1 = settings.shadow; //Settings Shadows
        int set_2 = settings.aliasing; //Settings Anti-Aliasing
        int set_3 = settings.postpro; //Settings Post Processing
        //Set defaults
        string value_1 = "OFF";
        string value_2 = "OFF";
        string value_3 = "OFF";
        //Covert int 1,2,3 to name string.
        if (set_1 == 1) { value_1 = "OFF"; }
        if (set_1 == 2) { value_1 = "HARD"; }
        if (set_1 == 3) { value_1 = "SOFT"; }
        if (set_2 == 1) { value_2 = "OFF"; }
        if (set_2 == 2) { value_2 = "2X"; }
        if (set_2 == 3) { value_2 = "4X"; }
        if (set_3 == 1) { value_3 = "LDR";}
        if (set_3 == 2) { value_3 = "LDR"; }
        if (set_3 == 3) { value_3 = "HDR"; }
        //Build filename string
        string filename = value_1 + "-" + value_2 + "-" + value_3;
        //Load render pipeline asset from filename in resources and return.
        return Resources.Load("Data/URPAssets/" + filename) as RenderPipelineAsset;
    }
}
