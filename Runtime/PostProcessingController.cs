using UnityEngine;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
// using Unity.Cinemachine.Editor;
// #if UNITY_EDITOR
// // using
// #endif


[ExecuteInEditMode]
public class PostProcessingController : MonoBehaviour
{
    [ShowInInspector][ReadOnly]
    private PostProcessingManager _manager;
    
    public int index
    {
        get
        {
            return _index;
        }
    }

    [ShowInInspector][ReadOnly]
    private int _index;

    public void SetIndex(int controllerIndex)
    {
        _index = controllerIndex;
    }
    

    [OnValueChanged("SetScreenCenterPos")]
    [LabelText("自定义屏幕中心")] 
    public Vector2 customScreenCenterPos = new Vector2(0.5f, 0.5f);

    private Vector2 _lastCustomScreenCenterPos = new Vector2(0.5f, 0.5f);

    
    // [OnValueChanged("SetToggles")]
    [ToggleGroup("chromaticAberrationToggle", "色散")]
    [OnValueChanged("InitAllSettings")]
    public bool chromaticAberrationToggle = false;
    
    [OnValueChanged("SetUVFromDistort")]
    [LabelText("色散UV跟随后处理扭曲")]
    [ToggleGroup("chromaticAberrationToggle")]
    public bool caFromDistort = false;

    [LabelText("色散强度")] 
    [ToggleGroup("chromaticAberrationToggle")]
    public float chromaticAberrationIntensity = 0.2f;
    
    [HideIf("caFromDistort")]
    [LabelText("色散位置")] 
    [ToggleGroup("chromaticAberrationToggle")]
    public float chromaticAberrationPos = 0.5f;
    
    [HideIf("caFromDistort")]
    [LabelText("色散过渡范围")] 
    [ToggleGroup("chromaticAberrationToggle")]
    public float chromaticAberrationRange = 0.5f;

    // [OnValueChanged("SetToggles")]
    [ToggleGroup("distortSpeedToggle", "扭曲")]
    public bool distortSpeedToggle = false;

    [ToggleGroup("distortSpeedToggle")]
    [LabelText("后处理走常规屏幕坐标")] 
    [OnValueChanged("SetUVFromDistort")]
    public bool distortScreenUVMode = false;
    
    [LabelText("后处理扭曲")] 
    [ToggleGroup("distortSpeedToggle")]
    [OnValueChanged("InitAllSettings")]
    public Texture2D distortSpeedTexture;

    [LabelText("扭曲贴图中间值")]
    [ToggleGroup("distortSpeedToggle")]
    [OnValueChanged("SetTexture")]
    public float distortTextureMidValue = 1;

    [LabelText("扭曲贴图ST")]
    [ToggleGroup("distortSpeedToggle")]
    public Vector4 distortSpeedTexSt = new Vector4(30,1,0,0);
    

    [LabelText("扭曲强度")] 
    [ToggleGroup("distortSpeedToggle")]
    public float distortSpeedIntensity = 1f;

    [HideIf("distortScreenUVMode")]
    [LabelText("扭曲位置")] 
    [ToggleGroup("distortSpeedToggle")]
    public float distortSpeedPosition = 0.5f;

    [HideIf("distortScreenUVMode")]
    [LabelText("扭曲范围")] 
    [ToggleGroup("distortSpeedToggle")]
    public float distortSpeedRange = 1f;

    [LabelText("扭曲纹理流动X")] 
    [ToggleGroup("distortSpeedToggle")]
    public float distortSpeedMoveSpeedX = 0.1f;
    [LabelText("扭曲纹理流动Y")] 
    [ToggleGroup("distortSpeedToggle")]
    public float distortSpeedMoveSpeed = -0.5f;//因为老的做法是没有X偏移的，只有Y的偏移，不改变量兼容老做法。
    private readonly int _distortSpeedTextureID = Shader.PropertyToID("_SpeedDistortMap");
    private readonly int _distortSpeedTextureStID = Shader.PropertyToID("_SpeedDistortMap_ST");

    [ToggleGroup("radialBlurToggle", "径向模糊")]
    [OnValueChanged("InitAllSettings")]
    public bool radialBlurToggle = false;

    [OnValueChanged("SetUVFromDistort")]
    [ToggleGroup("radialBlurToggle")] [LabelText("径向模糊跟随后处理扭曲")]
    public bool radialBlurFromDistort = false;
    
    [ToggleGroup("radialBlurToggle")]
    [LabelText("采样次数")]
    [Range(1,12)]
    public int radialBlurSampleCount = 4;
    [ToggleGroup("radialBlurToggle")]
    [LabelText("强度")]
    public float radialBlurIntensity = 1;
    [HideIf("radialBlurFromDistort")]
    [LabelText("位置")]
    [ToggleGroup("radialBlurToggle")]
    public float radialBlurPos = 0.5f;
    [HideIf("radialBlurFromDistort")]
    [LabelText("过度范围")]
    [ToggleGroup("radialBlurToggle")]
    public float radialBlurRange = 0.5f;
    
    // [OnValueChanged("SetToggles")]
    [ToggleGroup("cameraShakeToggle", "震屏")]
    [OnValueChanged("InitAllSettings")]
    public bool cameraShakeToggle = false;

    [ToggleGroup("cameraShakeToggle")]
    [LabelText("绑定Cinemachine相机")]
    [OnValueChanged("InitCinemachineCamera")]
    public CinemachineCamera cinemachineCamera;
    
    [ToggleGroup("cameraShakeToggle")] 
    [LabelText("相机震动强度")]
    public float cameraShakeIntensity;
    
    [ToggleGroup("overlayTextureToggle", "肌理叠加图")]
    [OnValueChanged("InitAllSettings")]
    public bool overlayTextureToggle = false;

    [LabelText("肌理图极坐标模式")] [ToggleGroup("overlayTextureToggle")]
    [OnValueChanged("SetTexture")]
    public bool overlayTexturePolarCoordMode = false;

    [ToggleGroup("overlayTextureToggle")] 
    [LabelText("肌理图")]
    [OnValueChanged("SetTexture")]
    public Texture2D overlayTexture;
    
    private readonly int _overlayTextureID = Shader.PropertyToID("_TextureOverlay");
    private readonly int _overlayTextureStID = Shader.PropertyToID("_TextureOverlay_ST");
    private readonly int _textureOverlayAnimProperty = Shader.PropertyToID("_TextureOverlayAnim");
    private readonly int _textureOverlayMaskProperty = Shader.PropertyToID("_TextureOverlayMask");
    private readonly int _textureOverlayMaskStProperty = Shader.PropertyToID("_TextureOverlayMask_ST");


    [LabelText("肌理图缩放平移")] [ToggleGroup("overlayTextureToggle")]
    public Vector4 overlayTextureSt = new Vector4(1, 1, 0, 0);
    [LabelText("肌理图偏移动画")] [ToggleGroup("overlayTextureToggle")]
    public Vector2 overlayTextureAnim = new Vector2(0, 0); 
    [LabelText("肌理图强度")] [ToggleGroup("overlayTextureToggle")]
    public float overlayTextureIntensity = 1f;
    [LabelText("肌理图蒙板")] [ToggleGroup("overlayTextureToggle")]
    [OnValueChanged("SetTexture")]
    public Texture2D overlayMaskTexture;
    [LabelText("肌理图蒙板缩放平移")] [ToggleGroup("overlayTextureToggle")]
    public Vector4 overlayMaskTextureSt;
    
    [ToggleGroup("flashToggle", "反闪")] 
    [OnValueChanged("InitAllSettings")]
    public bool flashToggle = false;
    [LabelText("反转度")] [ToggleGroup("flashToggle")]
    public float flashInvertIntensity = 1f;
    [LabelText("饱和度")] [ToggleGroup("flashToggle")]
    public float flashDeSaturateIntensity = 1f;
    [LabelText("对比度")] [ToggleGroup("flashToggle")]
    public float flashContrast = 1f;
    
    [ToggleGroup("vignetteToggle", "暗角")] 
    [OnValueChanged("InitAllSettings")]
    public bool vignetteToggle = false;
    [ToggleGroup("vignetteToggle")] 
    [LabelText("暗角颜色")]
    public Color vignetteColor = Color.black;
    private readonly int _vignetteColorID = Shader.PropertyToID("_VignetteColor");
    [ToggleGroup("vignetteToggle")] 
    [LabelText("暗角强度")]
    public float vignetteIntensity = 1;
    [ToggleGroup("vignetteToggle")] 
    [LabelText("暗角圆度")]
    public float vignetteRoundness = 1;
    [ToggleGroup("vignetteToggle")] 
    [LabelText("暗角平滑度")]
    public float vignetteSmothness = 10;
    
   
    public void InitController()
    {
        if (this.isActiveAndEnabled == false)
        {
            return;
        }

        _manager = PostProcessingManager.Instance;
        
        _manager.InitController(this);
        InitAllSettings();
        
    }

    void InitAllSettings()
    {
        SetScreenCenterPos();
        SetToggles();
        SetUVFromDistort();
        SetTexture();
    }

    void SetScreenCenterPos()
    {
        PostProcessingManager.customScreenCenterPos = customScreenCenterPos;
        _lastCustomScreenCenterPos = customScreenCenterPos;
    }

    void SetUVFromDistort()
    {
        PostProcessingManager.isDistortScreenUVMode = distortScreenUVMode;
        PostProcessingManager.isCaByDistort = caFromDistort;
        PostProcessingManager.isRadialBlurByDistort = radialBlurFromDistort;
    }

    private void SetTexture()
    {
        if(PostProcessingManager.Material == null) return;
        if (distortSpeedToggle)
        {
            if (distortSpeedTexture != null)
            {
                PostProcessingManager.Material.SetTexture(_distortSpeedTextureID, distortSpeedTexture);
                PostProcessingManager.distortTextureMidValue = distortTextureMidValue;
            }
        }

        if (overlayTextureToggle)
        {
            if (overlayTexturePolarCoordMode)
            {
                PostProcessingManager.flags.SetFlagBits(Mh2CustomPostprocessFlags.FLAG_BIT_OVERLAYTEXTURE_POLLARCOORD);
            }
            else
            {
                PostProcessingManager.flags.ClearFlagBits(Mh2CustomPostprocessFlags.FLAG_BIT_OVERLAYTEXTURE_POLLARCOORD);
            }
            if (overlayTexture)
            {
                PostProcessingManager.Material.SetTexture(_overlayTextureID,overlayTexture);
            }

            if (overlayMaskTexture)
            {
                PostProcessingManager.Material.SetTexture(_textureOverlayMaskProperty,overlayMaskTexture);
                PostProcessingManager.flags.SetFlagBits(Mh2CustomPostprocessFlags.FLAG_BIT_OVERLAYTEXTURE_MASKMAP);
            }
            else
            {
                PostProcessingManager.flags.ClearFlagBits(Mh2CustomPostprocessFlags.FLAG_BIT_OVERLAYTEXTURE_MASKMAP);
            }
        }
        
    }

    private void SetToggles()
    {
        SetBit(ref PostProcessingManager.chromaticAberrationToggles,index,chromaticAberrationToggle);
        SetBit(ref PostProcessingManager.distortSpeedToggles,index,distortSpeedToggle);
        SetBit(ref PostProcessingManager.cameraShakeToggles,index,cameraShakeToggle);
        SetBit(ref PostProcessingManager.overlayTextureToggles,index,overlayTextureToggle);
        SetBit(ref PostProcessingManager.flashToggles,index,flashToggle);
        SetBit(ref PostProcessingManager.radialBlurToggles,index,radialBlurToggle);
        SetBit(ref PostProcessingManager.vignetteToggles,index,vignetteToggle);

#if UNITY_EDITOR
        SetTexture();
#endif
        
    }

    private void ClearToggles()
    {
        SetBit(ref PostProcessingManager.chromaticAberrationToggles,index,false);
        SetBit(ref PostProcessingManager.distortSpeedToggles,index,false);
        SetBit(ref PostProcessingManager.cameraShakeToggles,index,false);
        SetBit(ref PostProcessingManager.overlayTextureToggles,index,false);
        SetBit(ref PostProcessingManager.flashToggles,index,false);
        SetBit(ref PostProcessingManager.radialBlurToggles,index,false);
        SetBit(ref PostProcessingManager.vignetteToggles,index,false);
    }

    private bool _lastChormaticAberrationToggle = false;
    private bool _lastDistortSpeedToggle = false;
    private bool _lastCamerashakeToggle = false;
    private bool _lastOverlayTextureToggle = false;
    private bool _lastFlashToggle= false;
    private bool _lastRadialBlurToggle= false;
    private bool _lastVignetteToggle= false;

    bool checkIfToggleChanged(ref bool lastTogggle, bool currentToggle)
    {
        if (lastTogggle != currentToggle)
        {
            lastTogggle = currentToggle;
            return true;
        }
        else
        {
            return false;
        }
    }
    
    

    private static void SetBit(ref int bit, int index,bool bitToggle)
    {
        if (bitToggle)
        {
            bit |= (1 << index);
        }
        else
        {
            bit &= ~(1 << index);
        }
    }

    // [LabelText("跳过数值锁定")] public bool isSkipUpdate = false;

#if UNITY_EDITOR
        [Button("选择当前Manager")]
        void FindManager()
        {
            UnityEditor.Selection.activeObject = _manager.gameObject;
        }

        [Button("选择当前VituralCamera")]
        public void FindVirtualCamera()
        {
            UnityEditor.Selection.activeObject = _manager.currentVirtualCamera;
        }

        public void InitCinemachineCamera()
        {
            if (cinemachineCamera)
            {
                // _perlin = ;
                if (!cinemachineCamera.gameObject.TryGetComponent<CinemachineBasicMultiChannelPerlin>(out var _perlin))
                {
                    _perlin = cinemachineCamera.gameObject.AddComponent<CinemachineBasicMultiChannelPerlin>();
                }

                if (_perlin)
                {
                    _perlin.NoiseProfile =
                            UnityEditor.AssetDatabase.LoadAssetAtPath<NoiseSettings>(
                                "Packages/com.r2.render.postprocessing/3DPostionShake.asset");
                    _perlin.FrequencyGain = 5f; //做一个自定义
                    _perlin.AmplitudeGain = 0f; //一开始先不要震动
                }

                #if  UNITY_EDITOR
                    if (_manager)
                    {
                        _manager.currentVirtualCamera = cinemachineCamera;
                    }
                #endif
            }
            
        }
#endif


    private void OnEnable()
    {
        // Debug.Log("InitController");
        InitController();
    }

    private void OnDisable()
    {
        // Debug.Log("EndController");
        EndController();
    }
         
    // Update is called once per frame
    void Update()
    {
//#if UNITY_EDITOR
        //Odin的ToggleGroup和OnValueChange功能冲突，导致不一定生效。不好调试。所以用手动的方式更新。
        bool isToggleChanged = false;
        isToggleChanged |= checkIfToggleChanged(ref _lastChormaticAberrationToggle, chromaticAberrationToggle);
        isToggleChanged |= checkIfToggleChanged(ref _lastDistortSpeedToggle, distortSpeedToggle);
        isToggleChanged |= checkIfToggleChanged(ref _lastCamerashakeToggle, cameraShakeToggle);
        isToggleChanged |= checkIfToggleChanged(ref _lastOverlayTextureToggle, overlayTextureToggle);
        isToggleChanged |= checkIfToggleChanged(ref _lastFlashToggle, flashToggle);
        isToggleChanged |= checkIfToggleChanged(ref _lastRadialBlurToggle, radialBlurToggle);
        isToggleChanged |= checkIfToggleChanged(ref _lastVignetteToggle, vignetteToggle);
        if (isToggleChanged)
        {
            SetToggles();
        }
//#endif

        if (customScreenCenterPos != _lastCustomScreenCenterPos)
        {
            SetScreenCenterPos();
        }
        
        
        if (chromaticAberrationToggle)
        {
            PostProcessingManager.chromaticAberrationIntensity =
                Mathf.Max(PostProcessingManager.chromaticAberrationIntensity, chromaticAberrationIntensity);
            PostProcessingManager.chromaticAberrationPos =
                Mathf.Max(PostProcessingManager.chromaticAberrationPos, chromaticAberrationPos); 
            PostProcessingManager.chromaticAberrationRange =
                Mathf.Max(PostProcessingManager.chromaticAberrationRange, chromaticAberrationRange);
        }

        if (distortSpeedToggle)
        {
            if (index == PostProcessingManager.laseUpdateControllerIndex)
            {
                //只有这里才会更新Scale
                PostProcessingManager.Material.SetVector(_distortSpeedTextureStID, distortSpeedTexSt);
            }

            PostProcessingManager.distortSpeedIntensity =
                Mathf.Max(PostProcessingManager.distortSpeedIntensity, distortSpeedIntensity);
            PostProcessingManager.distortSpeedPosition =
                Mathf.Max(PostProcessingManager.distortSpeedPosition, distortSpeedPosition);
            PostProcessingManager.distortSpeedRange =
                Mathf.Max(PostProcessingManager.distortSpeedRange, distortSpeedRange);
            PostProcessingManager.distortSpeedMoveSpeedX =
                Mathf.Abs(PostProcessingManager.distortSpeedMoveSpeedX) > Mathf.Abs(distortSpeedMoveSpeedX)
                    ? PostProcessingManager.distortSpeedMoveSpeedX
                    : distortSpeedMoveSpeedX;
            PostProcessingManager.distortSpeedMoveSpeedY =
                Mathf.Abs(PostProcessingManager.distortSpeedMoveSpeedY) > Mathf.Abs(distortSpeedMoveSpeed)
                    ? PostProcessingManager.distortSpeedMoveSpeedY
                    : distortSpeedMoveSpeed;
        }

        if (cameraShakeToggle)
        {
            PostProcessingManager.cameraShakeIntensity =
                Mathf.Max(PostProcessingManager.cameraShakeIntensity, cameraShakeIntensity);

        }

        if (overlayTextureToggle)
        {
            if (index == PostProcessingManager.laseUpdateControllerIndex)
            {
                PostProcessingManager.Material.SetVector(_overlayTextureStID, overlayTextureSt);
                PostProcessingManager.Material.SetVector(_textureOverlayMaskStProperty, overlayMaskTextureSt);
                PostProcessingManager.Material.SetVector(_textureOverlayAnimProperty,overlayTextureAnim);
            }

            PostProcessingManager.overlayTextureIntensity = Mathf.Max(PostProcessingManager.overlayTextureIntensity,
                overlayTextureIntensity);
        }

        if (flashToggle)
        {
            PostProcessingManager.flashDesaturateIntensity =
                Mathf.Max(PostProcessingManager.flashDesaturateIntensity, flashDeSaturateIntensity);
            PostProcessingManager.flashInvertIntensity =
                Mathf.Max(PostProcessingManager.flashInvertIntensity, flashInvertIntensity);
            PostProcessingManager.flashContrast =
                Mathf.Max(PostProcessingManager.flashContrast, flashContrast);
        }

        if (radialBlurToggle)
        {
            PostProcessingManager.radialBlurIntensity =
                Mathf.Max(PostProcessingManager.radialBlurIntensity, radialBlurIntensity);
            PostProcessingManager.radialBlurSampleCount = Mathf.Max(PostProcessingManager.radialBlurSampleCount,
                radialBlurSampleCount);
            PostProcessingManager.radialBlurPos = Mathf.Max(PostProcessingManager.radialBlurPos, radialBlurPos);
            PostProcessingManager.radialBlurRange = Mathf.Max(PostProcessingManager.radialBlurRange, radialBlurRange);
        }

        if (vignetteToggle)
        {
            PostProcessingManager.vignetteIntensity =
                Mathf.Max(PostProcessingManager.vignetteIntensity, vignetteIntensity);
            PostProcessingManager.vignetteRoundness = Mathf.Max(PostProcessingManager.vignetteRoundness, vignetteRoundness);
            PostProcessingManager.vignetteSmothness = Mathf.Max(PostProcessingManager.vignetteSmothness, vignetteSmothness);
            if (index == PostProcessingManager.laseUpdateControllerIndex)
            {
                PostProcessingManager.Material.SetColor(_vignetteColorID,vignetteColor);
            }
        }


    }


    void EndController()
    {
        ClearToggles();
        _manager.EndController(this);
    }


    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        // Ensure continuous Update calls.
        if (!Application.isPlaying)
        {
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            UnityEditor.SceneView.RepaintAll();
        }
#endif
    }


#if UNITY_EDITOR
    [UnityEditor.MenuItem("GameObject/创建自定义后处理特效")]
    static void CreatMenu()
    {
        GameObject Effect = new GameObject();
        Effect.name = "CustomPostprocessController";
        PostProcessingController controller = Effect.AddComponent<PostProcessingController>();

        UnityEditor.Selection.activeObject = Effect;
    }
#endif
}