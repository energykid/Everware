sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float BandWidth;

float Timer;

float Parallax;

texture FillTexture;
sampler2D FillSampler = sampler_state
{
    Texture = (FillTexture);
    AddressU = WRAP;
    AddressV = WRAP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

float4 Effect(float2 coords : TEXCOORD0) : COLOR0
{
    float4 col1 = tex2D(FillSampler, coords);
    float4 col = tex2D(uImage0, coords + float2(Parallax, 0.0));
    if (col.a > 0.0)
    {
        col.a *= sin((Timer + coords.x) * BandWidth) * sin((Timer + coords.x) * BandWidth);
        col.a *= col1.r;
    }
    if (col1.r < 0.5)
    {
        col = float4(0.0, 0.0, 0.0, 0.0);
    }
    return col;
}

technique Shader
{
    pass Effect
    {
        PixelShader = compile ps_2_0 Effect();
    }
}
