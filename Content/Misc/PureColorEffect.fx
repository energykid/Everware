sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float4 MultiplyColor;

float4 ColorEffect(float2 coords : TEXCOORD0) : COLOR0
{
    float4 col = tex2D(uImage0, coords);
    
    return lerp(col, MultiplyColor, col.a);
}
technique ColorShader
{
    pass ColorEffect
    {
        PixelShader = compile ps_2_0 ColorEffect();
    }
}
