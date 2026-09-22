#include "../../Utilities.fxh"

sampler uImage0 : register(s0);
float2 Resolution;

float Progress;
float Progress2;
float Frames;
float FrameNum;

float4 Effect(float2 coords : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float4 Transparent = float4(0.0, 0.0, 0.0, 0.0);

    float4 col = (tex2D(uImage0, coords) * color);
    
    float4 Shine = float4(84.0, 197.0, 233.0, 0.5) / 255.0 * float4(col.a, col.a, col.a, col.a);

    float f = clamp(((coords.y * Frames) % 1.0) + Progress, 0.0, 1.0);
    
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
        return lerp(lerp(col, Shine, f), Transparent, f);
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