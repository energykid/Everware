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

float4 Effect(float2 coords : TEXCOORD0) : COLOR0
{    
    float4 col = tex2D(uImage0, coords);
    
    if (col.r < 0.2 && col.g < 0.2 && col.b < 0.2)
    {
        col = float4(0.0, 0.0, 0.0, 0.0);
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
