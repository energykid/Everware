sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float4 LightingColor;

float ColorClip;
float ColorClipUpper;

float Range;

texture Gradient;
sampler2D GradientSampler = sampler_state
{
    Texture = (Gradient);
    AddressU = WRAP;
    AddressV = WRAP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

float4 Effect(float2 coords : TEXCOORD0) : COLOR0
{
    float4 col = tex2D(uImage0, coords);

    if (col.a > 0.0 && col.r > (ColorClip % 1.0) && (col.r % 1.0) < (ColorClipUpper % 1.0))
    {
        float value = col.r - (ColorClip % 1.0);

        float4 extraCol = tex2D(GradientSampler, float2(value, 0));
        
        return extraCol * LightingColor;
    }
    
    if (col.a > 0.0 && ((col.r + 0.5) % 1.0) > (ColorClip % 1.0) && ((col.r + 0.5) % 1.0) < (ColorClipUpper % 1.0))
    {
        float value = ((col.r + 0.5) % 1.0) - (ColorClip % 1.0);

        float4 extraCol = tex2D(GradientSampler, float2(value, 0));
        
        return extraCol * LightingColor;
    }
    return float4(0.0, 0.0, 0.0, 0.0);
}

technique GradientShader
{
    pass Effect
    {
        PixelShader = compile ps_3_0 Effect();
    }
}
