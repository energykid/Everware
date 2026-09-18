
sampler uImage0 : register(s0);

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


float Progress;
float Clip;

texture StarTexture;
sampler2D StarSampler = sampler_state
{
    Texture = (StarTexture);
    AddressU = WRAP;
    AddressV = WRAP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

float4 Effect(float2 coords : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float2 NoiseScale = float2(0.2, 0.2);
    float4 colClip = tex2D(NoiseSampler, coords / NoiseScale);
    float4 col = tex2D(uImage0, coords);
    if (colClip.r < Clip)
        col = tex2D(uImage0, (coords * float2(0.5, 1.0)) + float2(0.5, 0.0));
    if (col.a > 0)
    {
        col.rgb += tex2D(StarSampler, coords + float2(Progress, 0.0)).rgb;
    }
    return col * color;
}

technique Shader
{
    pass Effect
    {
        PixelShader = compile ps_3_0 Effect();
    }
}