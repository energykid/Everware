#include "../../Utilities.fxh"

sampler uImage0 : register(s0);

float2 Resolution;
float Clip;
float Frame;

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

float4 Effect(float2 coords : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float2 NoiseScale = float2(0.25, 0.25);
    float2 NoiseScale2 = float2(1.0, 5.0);
    if (tex2D(NoiseSampler, pixelateCoords((coords * NoiseScale * NoiseScale2) - (Frame / NoiseScale), Resolution)).r < Clip)
        return float4(0.0, 0.0, 0.0, 0.0);
        
    float4 col = tex2D(uImage0, coords);
    return col * color;
}

technique Shader
{
    pass Effect
    {
        PixelShader = compile ps_3_0 Effect();
    }
}