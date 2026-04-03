sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float2 TextResolution;
float2 FillResolution;

float Parallax;

float4 MultColor;

float Timer;

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
    float4 col = tex2D(uImage0, coords);
    if (col.r > 0.0)
    {
        return tex2D(FillSampler, (coords + float2(Parallax, 0.0)) / FillResolution * TextResolution);
    }
    return float4(0.0, 0.0, 0.0, 0.0);
}

technique Shader
{
    pass Effect
    {
        PixelShader = compile ps_2_0 Effect();
    }
}
