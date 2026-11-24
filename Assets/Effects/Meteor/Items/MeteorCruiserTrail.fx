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
    float4 col = tex2D(FillSampler, (coords * Resolution) + float2(Time, 0));

    float ex = tex2D(uImage0, coords).r;
    float exA = tex2D(uImage0, coords).a;
    
    if (tex2D(uImage0, coords).a == 0.0) return float4(0.0, 0.0, 0.0, 0.0);
    
    return col * float4(ex, ex, ex, 0.5) * float4(exA, exA, exA, exA);
}
technique Shader
{
    pass Effect
    {
        PixelShader = compile ps_3_0 Effect();
    }
}
