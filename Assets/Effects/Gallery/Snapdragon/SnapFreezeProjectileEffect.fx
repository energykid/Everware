sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float4 LightingColor;

float Clip;

float Timer;

float2 Dimensions;

float2 NoiseWidth;
float2 NoiseOffset;

texture FreezeGradient;
sampler2D FreezeGradientSampler = sampler_state
{
    Texture = (FreezeGradient);
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

float4 Effect(float2 coords : TEXCOORD0) : COLOR0
{
    float4 col = tex2D(uImage0, coords);
    
    float4 col2 = tex2D(NoiseTextureSampler, (coords * NoiseWidth) + NoiseOffset);
    
    float4 col3 = col2 * lerp(float4(0.5, 0.5, 0.5, 1.0), float4(1.0, 1.0, 1.0, 1.0), coords.x);
    
    if (col3.r > Clip)
    {
        return col;
    }
    
    float4 extra_color1 = tex2D(uImage0, coords + float2(2 / Dimensions.x, 0));
    float4 extra_color2 = tex2D(uImage0, coords + float2(-2 / Dimensions.x, 0));
    float4 extra_color3 = tex2D(uImage0, coords + float2(0, 2 / Dimensions.y));
    float4 extra_color4 = tex2D(uImage0, coords + float2(0, -2 / Dimensions.y));
    
    if (extra_color1.a == 0.0 || extra_color2.a == 0.0 || extra_color3.a == 0.0 || extra_color4.a == 0.0)
    {
        return col;
    }
    
    float a = col3.r + col3.g + col3.b / 3.0;
    float b = col.r + col.g + col.b / 3.0;
    float v = lerp((a - Clip) / (1.0 - Clip), 1.0, 0.6);
    return tex2D(FreezeGradientSampler, v * b);
}

technique Shader
{
    pass Effect
    {
        PixelShader = compile ps_2_0 Effect();
    }
}
