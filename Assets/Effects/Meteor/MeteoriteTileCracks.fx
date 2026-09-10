#include "../Utilities.fxh"

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float Progress;

float2 ScreenPosition;
float2 ScreenResolution;

float2 NoiseScale;
float2 NoiseResolution;

float4 OutlineColor1;
float4 OutlineColor2;

texture Lightmap;
sampler2D LightmapSampler = sampler_state
{
    Texture = (Lightmap);
    AddressU = WRAP;
    AddressV = WRAP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

texture CrackTexture;
float2 CrackResolution;
sampler2D CrackSampler = sampler_state
{
    Texture = (CrackTexture);
    AddressU = WRAP;
    AddressV = WRAP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

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


bool equals(float4 col1, float4 col2)
{
    return col1.r == col2.r && col1.g == col2.g && col1.b == col2.b && col1.a == col2.a;
}

float4 Effect(float2 coords : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float4 light = tex2D(LightmapSampler, coords);

    float2 offset = ScreenPosition / ScreenResolution;

    float2 NoiseScale2 = float2(8.0, 5.0);

    float4 c = float4(color.r, color.g, color.b, 0.0);
    
    float4 col = tex2D(uImage0, coords);
    
    float2 a = pixelateCoords(coords * NoiseScale, ScreenResolution / NoiseScale);
    
    float2 a2 = coords / CrackResolution * ScreenResolution / 2.0;
    float2 aa2 = (ScreenPosition / CrackResolution * ScreenResolution / 2.0);
    
    float2 aa1 = pixelateCoords(ScreenPosition * NoiseScale, ScreenResolution / NoiseScale);
    
    float4 col1 = tex2D(NoiseSampler, pixelateCoords(a + aa1, ScreenResolution));
    float4 col2 = tex2D(CrackSampler, a2 + aa2);
    
    if (col.a == 0.0)
        return float4(0.0, 0.0, 0.0, 0.0);
    
    float amt = 1.0 / ScreenResolution / 2.0;
    
    float2 cc = pixelateCoords(coords, ScreenResolution);
    
    float4 colExtra1 = tex2D(uImage0, cc + float2(amt, 0));
    float4 colExtra2 = tex2D(uImage0, cc + float2(-amt, 0));
    float4 colExtra3 = tex2D(uImage0, cc + float2(0, amt));
    float4 colExtra4 = tex2D(uImage0, cc + float2(0, -amt));
    
    if (col2.a == 1.0)
    {
        if (equals(col2, float4(0.0, 0.0, 0.0, 1.0)))
            return col * float4(0.95, 0.85, 0.65, 1.0);
    
        if (equals(col, OutlineColor1) || equals(col, OutlineColor2))
            return float4(0.0, 0.0, 0.0, 0.0);
        else
        {
            if (equals(colExtra1, OutlineColor1) || equals(colExtra2, OutlineColor1) || equals(colExtra3, OutlineColor1) || equals(colExtra4, OutlineColor1))
                return OutlineColor1;
                
            if (equals(colExtra1, OutlineColor2) || equals(colExtra2, OutlineColor2) || equals(colExtra3, OutlineColor2) || equals(colExtra4, OutlineColor2))
                return OutlineColor2;
        }
        
        return col2;
    }
    return (col * lerp(light, float4(1.0, 1.0, 1.0, 1.0), avg(light.rgb) * 1.5)) * float4(1.0 + col2.a * 1.2, 1.0, 1.0, 1.0);
}

technique Shader
{
    pass Glow
    {
        PixelShader = compile ps_3_0 Effect();
    }
}