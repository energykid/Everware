#include "../../Utilities.fxh"

sampler uImage0 : register(s0);

float2 Resolution;

float Outset;

float4 OutlineColor;

float4 Effect(float2 coords : TEXCOORD0) : COLOR0
{
    float4 col = tex2D(uImage0, coords);
    
    bool outCol = false;
    
    if (col.a == 0.0)
        outCol = true;
    else
    {
        if (tex2D(uImage0, coords + float2(1.0 / Resolution.x * 2 * Outset, 0.0)).a == 0.0)
            outCol = true;
        if (tex2D(uImage0, coords - float2(1.0 / Resolution.x * 2 * Outset, 0.0)).a == 0.0)
            outCol = true;
            
        if (tex2D(uImage0, coords + float2(0.0, 1.0 / Resolution.y * 2 * Outset)).a == 0.0)
            outCol = true;
        if (tex2D(uImage0, coords - float2(0.0, 1.0 / Resolution.y * 2 * Outset)).a == 0.0)
            outCol = true;
    }
    
    if (outCol)
        return float4(0.0, 0.0, 0.0, 0.0);

    return OutlineColor;
}
technique Shader
{
    pass Effect
    {
        PixelShader = compile ps_3_0 Effect();
    }
}
