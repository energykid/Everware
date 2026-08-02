sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float4 MultiplyColor;

float4 ColorEffect(float2 coords : TEXCOORD0) : COLOR0
{
    float4 col = tex2D(uImage0, coords);
    
    if (col.a == 1.0)
        return MultiplyColor;
    
    return float4(0.0, 0.0, 0.0, 0.0);
}
technique ColorShader
{
    pass ColorEffect
    {
        PixelShader = compile ps_2_0 ColorEffect();
    }
}
