#include "../Utilities.fxh"

sampler uImage0 : register(s0);

float4 Color1[3];
float4 Color2[3];
float ColorNumber;

float Time;
float Alpha;

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

float4 Effect(float2 coords : TEXCOORD0) : COLOR0
{
    float4 col = tex2D(uImage0, coords);
    float4 col2 = tex2D(FillSampler, (coords / Resolution) + float2(Time, 0));
    
    if (col.a > 0.0)
    {
        float length = ColorNumber;
    
        float c = (col.r * (ColorNumber - 0.001));
        
        return lerp(lerp(Color1[floor(c)], Color1[floor(c) + 1], c % 1.0) * Alpha, lerp(Color2[floor(c)], Color2[floor(c) + 1], c % 1.0) * Alpha, col2.r);
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
