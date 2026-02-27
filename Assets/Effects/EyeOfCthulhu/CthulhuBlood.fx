sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float4 LightingColor;

float ColorClip;
float ColorClipUpper;

texture BloodGradient;
sampler2D BloodGradientSampler = sampler_state
{
    Texture = (BloodGradient);
    AddressU = WRAP;
    AddressV = WRAP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

float4 BloodEffect(float2 coords : TEXCOORD0) : COLOR0
{
    float4 col = tex2D(uImage0, coords);
    if (col.a > 0.0 && col.r > ColorClip && col.r < ColorClipUpper)
    {
        float value = col.r - ColorClip;

        float4 extraCol = tex2D(BloodGradientSampler, float2(value, 0));
        
        return extraCol * float4(LightingColor.r, LightingColor.g, LightingColor.b, LightingColor.a);
    }
    return float4(0.0, 0.0, 0.0, 0.0);
}

technique BloodShader
{
    pass BloodEffect
    {
        PixelShader = compile ps_2_0 BloodEffect();
    }
}
