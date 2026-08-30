#include "../Utilities.fxh"

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float Progress;
float2 NoiseScale;

float2 Resolution;
float2 Resolution2;
float2 ScreenPosition;
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

float4 GlowEffect(float2 coords : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float2 NoiseScale2 = float2(8.0, 5.0);

    float p = Progress / 100;

    float4 c = float4(color.r, color.g, color.b, 0.0);

    float4 col = tex2D(uImage0, coords);
    float2 a = ((pixelateCoords(coords * NoiseScale, Resolution)) + float2(0.0, p));
    float2 a2 = ((pixelateCoords(coords * NoiseScale2, Resolution)) + float2(0.0, p));
    float4 col1 = tex2D(NoiseSampler, pixelateCoords(a + (ScreenPosition * NoiseScale), Resolution));
    float4 col2 = tex2D(NoiseSampler, pixelateCoords(a2 + (ScreenPosition * NoiseScale2), Resolution));
    
    col1.r *= 1.25;
    
    float4 finalCol = col * pow(abs(0.5 + col1.r), 4.0) * lerp(c, float4(1.0, 1.0, 1.0, 0.0), color.r + color.g + color.b / 3.0);
    finalCol.a = 0.0;
    
    finalCol *= 1.0 + col2;
    finalCol.rgb *= col1.r - 0.65;
    finalCol.gb *= lerp(col1.r - 0.65, 1.0, 0.5);
    
    if (col1.r > 0.65)
        return finalCol;
        
    return float4(0.0, 0.0, 0.0, 0.0);
}

technique GlowShader
{
    pass Glow
    {
        PixelShader = compile ps_3_0 GlowEffect();
    }
}