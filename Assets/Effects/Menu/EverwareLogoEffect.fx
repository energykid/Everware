sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float2 TextResolution;
float2 FillResolution;

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

float4 LogoEffect(float2 coords : TEXCOORD0) : COLOR0
{
    float l = tex2D(uImage0, coords).r * tex2D(uImage0, coords).a;

    return tex2D(uImage0, coords) * lerp(tex2D(FillSampler, (coords + float2(Timer, 0)) * (float2(1.0, 1.0) / FillResolution * TextResolution)), float4(1.0, 1.0, 1.0, 1.0), l - 0.5);
}

technique LogoShader
{
    pass LogoEffect
    {
        PixelShader = compile ps_2_0 LogoEffect();
    }
}
