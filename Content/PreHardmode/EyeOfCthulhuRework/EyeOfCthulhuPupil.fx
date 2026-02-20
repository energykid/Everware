sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float4 LightingColor;

float PupilThreshold;
float IrisThreshold;

texture PupilGradient;
sampler2D PupilGradientSampler = sampler_state
{
    Texture = (PupilGradient);
    AddressU = WRAP;
    AddressV = WRAP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

float SkewLerp(float time, float start, float end, float pointA, float pointB, float defaultValue)
{
    if (time >= start && time <= end)
    {
        return lerp(pointA, pointB, (time - start) / (end - start));
    }
    return defaultValue;
}

float4 PupilEffect(float2 coords : TEXCOORD0) : COLOR0
{
    float4 col = tex2D(uImage0, coords);
    if (col.a > 0.0)
    {
        float baseValue = col.r;
    
        float value = SkewLerp(baseValue, 0.0, 0.33, 0, IrisThreshold, baseValue);
        value = SkewLerp(baseValue, 0.33, 0.66, IrisThreshold, PupilThreshold, value);
        value = SkewLerp(baseValue, 0.66, 1.0, PupilThreshold, 1.0, value);

        float4 extraCol = tex2D(PupilGradientSampler, float2(value, 0));
        
        return extraCol * float4(LightingColor.r, LightingColor.g, LightingColor.b, LightingColor.a);
    }
    return col;
}

technique PupilShader
{
    pass PupilEffect
    {
        PixelShader = compile ps_2_0 PupilEffect();
    }
}
