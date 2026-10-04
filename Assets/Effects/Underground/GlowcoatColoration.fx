sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float4 Color;

float4 Effect(float2 coords : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float4 col = tex2D(uImage0, coords);
    if (col.a > 0)
        return Color * color;
    else
        return col * color;
}

technique GradientShader
{
    pass Effect
    {
        PixelShader = compile ps_2_0 Effect();
    }
}
