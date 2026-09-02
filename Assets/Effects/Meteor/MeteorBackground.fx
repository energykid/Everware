
float4 Effect(float2 coords : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    return lerp(float4(0.0, 0.0, 0.0, 0.0), float4(0.2, 0.1, 0.25, 0.0) * color, coords.y - (sin(coords.x / 13.0) * 0.1));
}

technique Shader
{
    pass Effect
    {
        PixelShader = compile ps_3_0 Effect();
    }
}