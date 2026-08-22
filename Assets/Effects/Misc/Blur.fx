#include "../Utilities.fxh"

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float Radius;

float4 Effect(float2 coords : TEXCOORD0) : COLOR0
{
    return blur(coords, uImage0, Radius);
}
technique Shader
{
    pass Effect
    {
        PixelShader = compile ps_3_0 Effect();
    }
}
