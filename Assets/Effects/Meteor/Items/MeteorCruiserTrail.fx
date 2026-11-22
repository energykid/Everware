#include "../../Utilities.fxh"

sampler uImage0 : register(s0);

texture FillTexture;
sampler2D FillSampler = sampler_state
{
    Texture = (FillTexture);
    AddressU = WRAP;
    AddressV = WRAP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

float2 Resolution;
float2 NoiseOffset;
float2 NoiseScale;

float Amount;
float Time;

float4 OutlineColor;
float4 FillColor;
float4 ExtraColor;

bool Outline;

float4 Effect(float2 coords : TEXCOORD0) : COLOR0
{
    return tex2D(uImage0, coords);
}
technique Shader
{
    pass Effect
    {
        PixelShader = compile ps_3_0 Effect();
    }
}
