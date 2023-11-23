using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreferenceData 
{
    static public float MouseSensitivity = 25f;

    static public int ResolutionWidth;
    static public int ResolutionHeight;
    static public int FullScreenMode;

    static public int Framerate;
    static public int TextureQuality;
    static public int ShadowQuality;
    static public int AntiAliasing;
    static public int VSync;
    static public int AnisotropicFiltering;

    static public void ApplyGraphicOptionSetting()
    {
        Screen.SetResolution(ResolutionWidth, ResolutionHeight, (FullScreenMode)FullScreenMode);
        Application.targetFrameRate = Framerate;
        QualitySettings.globalTextureMipmapLimit = TextureQuality;
        if (ShadowQuality == -1)
        {
            QualitySettings.shadows = UnityEngine.ShadowQuality.Disable;
            QualitySettings.shadowResolution = ShadowResolution.Low;
        }
        else
        {
            QualitySettings.shadows = UnityEngine.ShadowQuality.All;
            QualitySettings.shadowResolution = (ShadowResolution)ShadowQuality;
        }
        QualitySettings.antiAliasing = AntiAliasing;
        QualitySettings.vSyncCount = VSync;
        QualitySettings.anisotropicFiltering = (AnisotropicFiltering)AnisotropicFiltering;
    }
}
