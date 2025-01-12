Shader "Mh2/ColorBlit"
{
    HLSLINCLUDE
    #pragma target 2.0
    #pragma editor_sync_compilation
    #pragma multi_compile _ DISABLE_TEXTURE2D_X_ARRAY
    #pragma multi_compile _ BLIT_SINGLE_SLICE
    // Core.hlsl for XR dependencies
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
    ENDHLSL

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"
        }
               ZWrite Off Cull Off
        Pass
        {
            Name "ColorBlitPass0"

            HLSLPROGRAM
            #pragma vertex Vert
            // #pragma fragment FragNearest
            #pragma fragment frag

            Texture2D _CameraTexture;
              half4 frag (Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float4 color = SAMPLE_TEXTURE2D_X(_CameraTexture, sampler_LinearRepeat, input.texcoord);
                return color;
            }
            ENDHLSL
        }
        Pass
        {
            Name "BoxDownsample"

            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragBoxDownsample
            

            Texture2D _CameraTexture;
            SAMPLER(sampler_CameraTexture);
            
            #if UNITY_VERSION < 202320
            float4 _BlitTexture_TexelSize;
            #endif
            

            float _SampleOffset;

            half4 FragBoxDownsample(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);
                float4 d = _BlitTexture_TexelSize.xyxy * float4(-_SampleOffset, -_SampleOffset, _SampleOffset,
                                                                _SampleOffset);

                half4 s;
                s = SAMPLE_TEXTURE2D_X(_CameraTexture, sampler_CameraTexture, uv + d.xy);
                s += SAMPLE_TEXTURE2D_X(_CameraTexture, sampler_CameraTexture, uv + d.zy);
                s += SAMPLE_TEXTURE2D_X(_CameraTexture, sampler_CameraTexture, uv + d.xw);
                s += SAMPLE_TEXTURE2D_X(_CameraTexture, sampler_CameraTexture, uv + d.zw);

                return s * 0.25h;
            }
            ENDHLSL
        }
    }
}