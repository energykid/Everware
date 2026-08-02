#include "../Utilities.fxh"

sampler uImage0 : register(s0);

float4 Color[10];
float ColorNumber;

float4 Effect(float2 coords : TEXCOORD0) : COLOR0
{
    float4 col = tex2D(uImage0, coords);
    
    if (col.a > 0.0)
    {
        float length = ColorNumber;
    
        float c = (col.r * (ColorNumber - 0.1));
        return lerp(Color[floor(c)], Color[floor(c) + 1], c % 1.0);
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
