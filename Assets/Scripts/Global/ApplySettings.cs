using UnityEngine;
using UnityEngine.Rendering;
public class ApplySettings : MonoBehaviour
{
    public Settings settings;

    public RenderPipelineAsset low_Pipeline, med_Pipeline, high_Pipeline, ultra_Pipeline;

    //------------------------//
    //   - QUALITY - KEY -    //
    //  1 = LOW
    //  2 = MEDIUM
    //  3 = HIGH
    //  4 = ULTRA
    //------------------------//


    private void Start()
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
    

    //Get RenderPipeline Asset
    public RenderPipelineAsset GetAsset()
    {
        if(settings.quality == 4) 
        {
            return ultra_Pipeline;
        }
        else if (settings.quality == 3)
        {
            return high_Pipeline;
        }
        else if (settings.quality == 2)
        {
            return med_Pipeline;
        }
        else
        {
            return low_Pipeline;
        }
    }
}
