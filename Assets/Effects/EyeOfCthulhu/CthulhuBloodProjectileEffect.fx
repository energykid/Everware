sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float4 LightingColor;

float Threshold;

float Timer;

texture BloodGradient;
sampler2D BloodGradientSampler = sampler_state
{
    Texture = (BloodGradient);
    AddressU = WRAP;
    AddressV = WRAP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

texture NoiseTexture;
sampler2D NoiseTextureSampler = sampler_state
{
    Texture = (NoiseTexture);
    AddressU = WRAP;
    AddressV = WRAP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

float4 BloodEffect(float2 coords : TEXCOORD0) : COLOR0
{
    float4 col = tex2D(uImage0, coords);
    
    float4 col2 = tex2D(NoiseTextureSampler, (coords + float2(0.0, -Timer / 2.0)) / float2(20, 10) * 4.0);
    
    float4 col3 = lerp(col * col2, float4(1.0, 1.0, 1.0, 1.0), col * col2);
    
    if (col3.r > Threshold)
    {
        return tex2D(BloodGradientSampler, float2(1.0 - col3.r, 0.0)) * LightingColor;
    }
    return float4(0.0, 0.0, 0.0, 0.0);
}

technique BloodShader
{
    pass BloodEffect
    {
        PixelShader = compile ps_2_0 BloodEffect();
    }
}
