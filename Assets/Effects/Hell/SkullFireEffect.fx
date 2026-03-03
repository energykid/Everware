sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float Progress;
float4 ExplosionColor1;
float4 ExplosionColor2;
float4 ExplosionColorMid;

texture NoiseTexture;
sampler2D NoiseSampler = sampler_state
{
    Texture = (NoiseTexture);
    AddressU = WRAP;
    AddressV = WRAP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

float4 SkullFireEffect(float2 coords : TEXCOORD0) : COLOR0
{
    float4 col = tex2D(uImage0, coords);
    float2 v2 = float2(1.0, Progress % 0.5);
    float4 col1 = tex2D(NoiseSampler, coords + v2) * lerp(1.0, 0.0, coords.y);
    if (col.a > 0.0)
    {
        float4 col2 = ExplosionColor1;
        if (col1.r < 0.4)
            col2 = ExplosionColor2;
        if (col1.r < 0.3)
            col2 = ExplosionColorMid;
        if (col1.r < 0.2)
            col2 = float4(0.0, 0.0, 0.0, 0.0);
        return col2;
    }
    return col;
}

technique SkullFireShader
{
    pass SkullFire
    {
        PixelShader = compile ps_2_0 SkullFireEffect();
    }
}