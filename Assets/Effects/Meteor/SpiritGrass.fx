#include "../Utilities.fxh"

float2 ScreenPosition;
float Progress;

float2 TargetResolution;
float2 NoiseResolution;

float2 NoiseScale;

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

texture NoiseTexture;
sampler2D NoiseSampler = sampler_state
{
    Texture = (NoiseTexture);
    AddressU = WRAP;
    AddressV = WRAP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

float4 SpiritEffect(float2 coords : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float4 c = tex2D(uImage0, coords);
    
    float2 cc = coords + pixelateCoords(float2(0.0, abs(avg(tex2D(NoiseSampler, pixelateCoords(((coords) + float2(0.0, Progress)) * NoiseScale, TargetResolution) + pixelateCoords(ScreenPosition * NoiseScale, TargetResolution)).rgb))), TargetResolution);
    
    if (c.a == 0.0 || avg(c.rgb) < 0.75)
        return tex2D(uImage0, cc);
    return tex2D(uImage0, coords);
}

technique SpiritShader
{
    pass Spirit
    {
        PixelShader = compile ps_3_0 SpiritEffect();
    }
}