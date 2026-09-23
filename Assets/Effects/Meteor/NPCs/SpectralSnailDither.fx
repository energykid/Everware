#include "../../Utilities.fxh"

sampler uImage0 : register(s0);
float2 Resolution;

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

float4 Color;
float Progress;
float Progress2;
float Frames;
float FrameNum;

float4 Effect(float2 coords : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float4 Transparent = float4(0.0, 0.0, 0.0, 0.0);

    float4 col = (tex2D(uImage0, coords) * color);
    
    float noise = tex2D(NoiseSampler, float2(coords.x * 0.2, Progress2)).r * 5.0;
    
    float fl = skewLerp(Progress, 0.0, 0.5, 0.0, 1.0, 0.0);
    fl = skewLerp(Progress, 0.5, 1.0, 1.0, 0.0, fl);
    
    float f = clamp((((coords.y * Frames) % 1.0) + Progress) + (noise * fl), 0.0, 1.0);
    
        bool b = dither(coords, Resolution);
        if (FrameNum % 2 == 0)
        {
            b = !b;
        }
        if (b)
        {
            return lerp(col, Transparent, f);
        }
        else
        {
            return lerp(lerp(col, Color * col.a, f), Transparent, f);
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