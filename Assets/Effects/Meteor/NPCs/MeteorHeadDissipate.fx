
sampler uImage0 : register(s0);

float Progress;

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
    float4 col = tex2D(uImage0, coords);
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