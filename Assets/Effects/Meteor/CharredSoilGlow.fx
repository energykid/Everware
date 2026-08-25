#include "../Utilities.fxh"

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float Progress;
float2 NoiseScale;

float4 Color;

float2 Resolution;
float2 Resolution2;
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

float4 GlowEffect(float2 coords : TEXCOORD0) : COLOR0
{
    float4 col = tex2D(uImage0, coords);
    float2 a = ((pixelateCoords(coords, Resolution)) + float2(0.0, Progress));
    float4 col1 = tex2D(NoiseSampler, a);
    
    if (col1.r > 0.65)
        return col * pow(abs(0.5 + col1.r), 4.0) * lerp(Color, float4(1.0, 1.0, 1.0, 1.0), Color.r + Color.g + Color.b / 3.0);
        
    return float4(0.0, 0.0, 0.0, 0.0);

}

technique GlowShader
{
    pass Glow
    {
        PixelShader = compile ps_3_0 GlowEffect();
    }
}