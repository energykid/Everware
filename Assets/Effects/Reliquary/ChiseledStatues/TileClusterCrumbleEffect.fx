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
    float4 col = tex2D(uImage0, coords);
    
    float4 col2 = tex2D(FillSampler, pixelateCoords(coords + NoiseOffset * NoiseScale, Resolution / 2));
    
    float4 transparent = float4(0.0, 0.0, 0.0, 0.0);
    
    bool outCol = false;
    
    if (col.a == 0.0)
        outCol = true;
    else
    {
        if (tex2D(uImage0, coords + float2(1.0 / Resolution.x * 2, 0.0)).a == 0.0)
            outCol = true;
        if (tex2D(uImage0, coords - float2(1.0 / Resolution.x * 2, 0.0)).a == 0.0)
            outCol = true;
            
        if (tex2D(uImage0, coords + float2(0.0, 1.0 / Resolution.y * 2)).a == 0.0)
            outCol = true;
        if (tex2D(uImage0, coords - float2(0.0, 1.0 / Resolution.y * 2)).a == 0.0)
            outCol = true;
    }
    
    if (outCol || col2.r < Amount)
        return float4(0.0, 0.0, 0.0, 0.0);
    
    if (Outline)
        return lerp(OutlineColor * FillColor, ExtraColor, skewLerp(coords.y, 0.5, 0.75, 0.0, 1.0, 0.0));
    else
        return (col * FillColor) + lerp(float4(0.0, 0.0, 0.0, 0.0), ExtraColor, skewLerp(coords.y, 0.5, 0.75, 0.0, 0.25, 0.0));
}
technique Shader
{
    pass Effect
    {
        PixelShader = compile ps_3_0 Effect();
    }
}
