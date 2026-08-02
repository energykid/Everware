sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float ColorClip;
float ColorClipUpper;

float4 Effect(float2 coords : TEXCOORD0) : COLOR0
{
    float4 col = tex2D(uImage0, coords);
    
    float lum = (col.r + col.g + col.b) / 3.0;
    
    if (col.a > 0.0 && lum > ColorClip && lum < ColorClipUpper)
    {
        return col;
    }
    return float4(0.0, 0.0, 0.0, 0.0);
}

technique GradientShader
{
    pass Effect
    {
        PixelShader = compile ps_2_0 Effect();
    }
}
