TEXTURE2D(_LensFlareTex);
SAMPLER(sampler_LensFlareTex);

half4 _lensFlareGlobalVector; //xy:光晕中心的屏幕位置 z:透明 w:整体缩放
half4 _lensFlareColor01;
half4 _lensFlareColor02;
half4 _lensFlareColor03;
half4 _lensFlareColor04;
half4 _lensFlareColor05;
half4 _lensFlareColor06;
half4 _lensFlareColor07;
half4 _lensFlareColor08;
half4 _lensFlareColor09;
half4 _lensFlareColor10;

half4 _lensFlareVector01;  // xy:面片位置 z:面片旋转弧度 w:面片尺寸
half4 _lensFlareVector02;
half4 _lensFlareVector03;
half4 _lensFlareVector04;
half4 _lensFlareVector05;
half4 _lensFlareVector06;
half4 _lensFlareVector07;
half4 _lensFlareVector08;
half4 _lensFlareVector09;
half4 _lensFlareVector10;

half4 _lensFlareOcclusionVector;  //x:采样开关 y:采样半径 z:采样次数 w:深度

int _LensFlareFlag;     //0关闭 1加算 2premul 3透明混合

CBUFFER_START(UnityPerMaterial)

CBUFFER_END

float2 Rotate(float2 v, float cos0, float sin0)
{
    return float2(v.x * cos0 - v.y * sin0,
                  v.x * sin0 + v.y * cos0);
}

float GetLinearDepthValue(float2 uv)
{
    float depth = LOAD_TEXTURE2D_X_LOD(_CameraDepthTexture, uint2(uv * GetScaledScreenParams().xy), 0).x;
    return LinearEyeDepth(depth, _ZBufferParams);
}

float GetOcclusion()
{
    if (_lensFlareOcclusionVector.z == 0.0f)
        return 1.0f;

    float contrib = 0.0f;
    float sample_Contrib = 1.0f / _lensFlareOcclusionVector.z;

    for (uint i = 0; i < (uint)_lensFlareOcclusionVector.z; i++)
    {
        float2 dir = _lensFlareOcclusionVector.y * SampleDiskUniform(Hash(2 * i + 0), Hash(2 * i + 1));
        float2 pos0 = _lensFlareGlobalVector.xy + dir;
        float2 pos = pos0 * 0.5f + 0.5f;
        #ifdef UNITY_UV_STARTS_AT_TOP
        pos.y = 1.0f - pos.y;
        #endif

        if (all(pos >= 0) && all(pos <= 1))
        {
            float depth0 = GetLinearDepthValue(pos);
            if (depth0 > _lensFlareOcclusionVector.w || NearlyEqual_Float(depth0, _lensFlareOcclusionVector.w))
            {
                contrib += sample_Contrib;
            }
        }
    }
    return contrib;
}

struct appdata
{
    float4 vertex   : POSITION;
    float2 uv       : TEXCOORD0;
};

struct v2f
{
    float4 pos      : SV_POSITION;
    float2 uv       : TEXCOORD0;
    half4 v_col     : TEXCOORD1; 
};

v2f vert_lensflare(appdata v)
{
    v2f o;
    float4 pos = float4(v.vertex.xy, 1.0, 1.0);
    half4 v_col;
    float2 v_pos;
    float v_rot;
    float v_size;
    //half boolNum = pow(v.color.r, 2.2);
    half boolNum = abs(v.vertex.z);
    //读取顶点色 or Z轴判断层级， 8层 - 10层，赋予参数;
    
    if(boolNum < 1)
    {
        v_col = _lensFlareColor01; v_pos = _lensFlareVector01.xy; v_rot = _lensFlareVector01.z; v_size = _lensFlareVector01.w;
    }
    else if(boolNum < 2)
    {
        v_col = _lensFlareColor02; v_pos = _lensFlareVector02.xy; v_rot = _lensFlareVector02.z; v_size = _lensFlareVector02.w;
    }
    else if(boolNum < 3)
    {
        v_col = _lensFlareColor03; v_pos = _lensFlareVector03.xy; v_rot = _lensFlareVector03.z; v_size = _lensFlareVector03.w;
    }
    else if(boolNum < 4)
    {
        v_col = _lensFlareColor04; v_pos = _lensFlareVector04.xy; v_rot = _lensFlareVector04.z; v_size = _lensFlareVector04.w;
    }
    else if(boolNum < 5)
    {
        v_col = _lensFlareColor05; v_pos = _lensFlareVector05.xy; v_rot = _lensFlareVector05.z; v_size = _lensFlareVector05.w;
    }
    else if(boolNum < 6)
    {
        v_col = _lensFlareColor06; v_pos = _lensFlareVector06.xy; v_rot = _lensFlareVector06.z; v_size = _lensFlareVector06.w;
    }
    else if(boolNum < 7)
    {
        v_col = _lensFlareColor07; v_pos = _lensFlareVector07.xy; v_rot = _lensFlareVector07.z; v_size = _lensFlareVector07.w;
    }
    else if(boolNum < 8)
    {
        v_col = _lensFlareColor08; v_pos = _lensFlareVector08.xy; v_rot = _lensFlareVector08.z; v_size = _lensFlareVector08.w;
    }
    else if(boolNum < 9)
    {
        v_col = _lensFlareColor09; v_pos = _lensFlareVector09.xy; v_rot = _lensFlareVector09.z; v_size = _lensFlareVector09.w;
    }
    else
    {
        v_col = _lensFlareColor10; v_pos = _lensFlareVector10.xy; v_rot = _lensFlareVector10.z; v_size = _lensFlareVector10.w;
    }
    
    pos.xy *= v_size * _lensFlareGlobalVector.w; //缩放
    
    //弧度制旋转
    pos.xy = Rotate(pos.xy, cos(v_rot), sin(v_rot));
    
    //校准为正常比例
    float2 screenParam = GetScaledScreenParams().xy;
    pos.x *= screenParam.y / screenParam.x;
    
    //屏幕位置+自身位置
    pos.xy += _lensFlareGlobalVector.xy + v_pos;

    //遮挡
    float occlusion = 1;
    UNITY_BRANCH
    if(_lensFlareOcclusionVector.x != 0)
    {
        occlusion = GetOcclusion();
    }
    
    o.pos = pos;
    o.uv = v.uv;
    o.v_col = v_col * occlusion;
    return o;
}

half4 frag_lensflare(v2f i) : SV_Target
{
    half4 color = i.v_col;
    color *= SAMPLE_TEXTURE2D(_LensFlareTex, sampler_LensFlareTex, i.uv) * _lensFlareGlobalVector.z;
    return color;
}