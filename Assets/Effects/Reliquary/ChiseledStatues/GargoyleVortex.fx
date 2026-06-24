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

float ExtraRotation;
float Amount;
float Time;

float4 Color[4];

/// sourced from Shin0155 on shadertoy: https://www.shadertoy.com/view/l3fGWS
float2 swirlWithSpirals(float2 uv, float2 center, float amt, float extraRotation)
{
    float2 dir = uv - center;
    float dist = length(dir);
    float angle = atan2(dir.y, dir.x);
    return center + dist * float2(cos(angle + (dist * amt) + extraRotation),
    sin(angle + (dist * amt) + extraRotation));
}

float4 Effect(float2 coords : TEXCOORD0) : COLOR0
{
    float2 c = swirlWithSpirals(coords, float2(0.5, 0.5), Amount, ExtraRotation);
    float2 c2 = pixelateCoords(c, Resolution);
    
    float4 col = tex2D(uImage0, c2);
    float4 col1 = tex2D(FillSampler, coords + float2(Time, 0.0));
    
    if (col.a > 0.0)
    {
        float c = ((col1.r + col.r) * 3.9);
        return lerp(Color[floor(c)], Color[floor(c) + 1], c % 1.0);
    }
    
    return col;
}
technique Shader
{
    pass Effect
    {
        PixelShader = compile ps_3_0 Effect();
    }
}
