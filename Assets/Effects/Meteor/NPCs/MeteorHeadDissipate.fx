
sampler uImage0 : register(s0);

float Clip;

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

float4 Effect(float2 coords : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float2 NoiseScale = float2(2.5, 2.5);
    if (tex2D(NoiseSampler, coords * NoiseScale).r < Clip)
        return float4(0.0, 0.0, 0.0, 0.0);
        
    float4 col = tex2D(uImage0, coords);
    
    if (col.a == 1.0 && tex2D(NoiseSampler, coords / NoiseScale).r < Clip + 0.2)
        return float4(1.0, 0.6, 0.0, 1.0);
        
    return col * color;
}

technique Shader
{
    pass Effect
    {
        PixelShader = compile ps_3_0 Effect();
    }
}