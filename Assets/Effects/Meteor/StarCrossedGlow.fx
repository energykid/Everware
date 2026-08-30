#include "../Utilities.fxh"

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float4 Color[5];
float Progress;

texture BlueGlow;
sampler2D BlueGlowSampler = sampler_state
{
    Texture = (BlueGlow);
    AddressU = WRAP;
    AddressV = WRAP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

float4 GlowEffect(float2 coords : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float lrp = tex2D(BlueGlowSampler, coords).r;
    
    float4 c2 = float4(1.0, 1.0, 1.0, 1.0);
    
    float4 col = tex2D(uImage0, coords);
    
    if (col.a > 0.0)
    {
        float length = 5;
    
        float c = (col.r * 4.0);
        c2 = lerp(Color[floor(c)], Color[floor(c) + 1], c % 1.0);
    }
    else
        return float4(0.0, 0.0, 0.0, 0.0);

    float vv = 0.5 + (sin(((lrp * 15.0) + Progress) / 2.0) * 0.5);
        
    if (lrp > vv) return c2;
    else return tex2D(uImage0, coords) * color;
}

technique GlowShader
{
    pass Glow
    {
        PixelShader = compile ps_3_0 GlowEffect();
    }
}