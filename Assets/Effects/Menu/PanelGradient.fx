#include "../Utilities.fxh"

sampler uImage0 : register(s0);

float4 Color[3];
float4 MultColor;

float4 Effect(float2 coords : TEXCOORD0) : COLOR0
{
    float4 col = tex2D(uImage0, coords);
    
    if (col.a > 0.0)
    {
        float length = 3;
    
        float c = (coords.x + coords.y / 2.0);
        return (col + lerp(Color[floor(c)], Color[floor(c) + 1], c % 1.0)) * MultColor;
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
