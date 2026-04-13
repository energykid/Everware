
#include "../../Utilities.fxh"

sampler uImage0 : register(s0);

texture uTexture0;
sampler2D tex0 = sampler_state
{
    texture = <uTexture0>;
    magfilter = POINT;
    minfilter = POINT;
    mipfilter = POINT;
    AddressU = wrap;
    AddressV = wrap;
};

texture uTexture1;
sampler2D tex1 = sampler_state
{
    texture = <uTexture1>;
    magfilter = POINT;
    minfilter = POINT;
    mipfilter = POINT;
    AddressU = wrap;
    AddressV = wrap;
};

float2 Offset0;
float2 Offset1;
float Clip;
float2 Dimensions;
float4 GlowColor;
matrix TransformMatrix;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float3 Coord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float3 Coord : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    output.Position = mul(input.Position, TransformMatrix);
    output.Color = input.Color;
    output.Coord = input.Coord;
    return output;
}

float4 PixelShaderFunction(in VertexShaderOutput input) : COLOR0
{
    float2 uv = input.Coord;
    
    float2 xx = vignetteMult(uv);
    
    uv += Offset0;
    
    uv /= Dimensions;
    
    float4 f42 = tex2D(tex1, uv + Offset1);
    float4 f4 = tex2D(tex0, uv + f42.r);
    if (f4.r > Clip)
        return f4 * GlowColor * xx.x;
    
    return float4(0.0, 0.0, 0.0, 0.0);
}

technique Technique1
{
    pass FrostBreathPass
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}

