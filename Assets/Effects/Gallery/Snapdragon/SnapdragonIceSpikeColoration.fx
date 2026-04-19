#include "../../Utilities.fxh"

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float2 Resolution;

float2 Parallax;

float Timer;

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

float4 Effect(float2 coords : TEXCOORD0) : COLOR0
{
    float4 col = tex2D(uImage0, coords);
    
    float4 col2 = float4(0.0, 0.0, 0.0, 0.0);
    if (col.a > 0.1)
        col2 = float4(0.25, 0.35, 0.85, 0.85);
    if (col.a > 0.35)
        col2 = float4(0.2, 0.3, 0.75, 0.9);
    if (col.a > 0.65)
        col2 = float4(0.4, 0.55, 1.0, 0.98);
    if (col2.a == 0)
    {
        float4 colOut1 = tex2D(uImage0, coords + float2(1.0 / Resolution.x, 0.0));
        float4 colOut2 = tex2D(uImage0, coords + float2(-1.0 / Resolution.x, 0.0));
        float4 colOut3 = tex2D(uImage0, coords + float2(0.0, 1.0 / Resolution.y));
        float4 colOut4 = tex2D(uImage0, coords + float2(0.0, -1.0 / Resolution.y));
        
        if (colOut1.a > 0.0 || colOut2.a > 0.0 || colOut3.a > 0.0 || colOut4.a > 0.0)
        {
            return float4(0.1, 0.1, 0.2, 0.9);
        }
    }
    else
    {
        float4 colOut = tex2D(uImage0, coords + float2(0.0, -1.0 / Resolution.y));
        
        if (colOut.a <= 0.0)
        {
            float4 c = float4(85.0 / 255.0, 178.0 / 255.0, 230.0, 1.0);
        
            return lerp(col2, c, abs(sin((coords.x / 2.0) + (coords.y / 2.0) + Timer)));
        }
    }
    col2 *= lerp(tex2D(FillSampler, pixelateCoords(coords, Resolution)), tex2D(FillSampler, pixelateCoords(coords + Parallax, Resolution)), 0.5);
    
    if (col2.a > 0.0)
        col2.a = lerp(abs(sin((coords.x / 2.0) + (coords.y / 2.0) + Timer)), 1.0, 0.5);
        
    return col2;
}

technique Shader
{
    pass Effect
    {
        PixelShader = compile ps_2_0 Effect();
    }
}