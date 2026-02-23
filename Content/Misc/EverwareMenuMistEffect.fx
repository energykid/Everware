sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float Timer;
float2 TextureSize;
float2 NoiseTextureSize;

float GloobLength;

float4 MultiplyColor;

bool Glow;

texture NoiseTexture;
sampler2D NoiseSampler = sampler_state
{
    Texture = (NoiseTexture);
    AddressU = WRAP;
    AddressV = WRAP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

float4 WiggleEffect(float2 coords : TEXCOORD0) : COLOR0
{
    coords *= (TextureSize / 2.0);
    coords = float2(floor(coords.x), floor(coords.y));
    coords /= (TextureSize / 2.0);
    
    float4 extraCol = tex2D(NoiseSampler, (coords / 10.0) + float2(Timer / 20.0, Timer / 20.0));
    
    float2 extraCoords = float2(extraCol.r, extraCol.g);
    
    extraCoords *= (TextureSize / 2.0);
    extraCoords = float2(floor(extraCoords.x), floor(extraCoords.y));
    extraCoords /= (TextureSize / 2.0);
    
    float4 baseColor = tex2D(uImage0, coords + extraCoords);
    
    float threshold = lerp(sin(baseColor.r * 5.0), 1.0, clamp((baseColor.g - 0.5) * 2.0, 0.0, 1.0));
    
    if (!Glow && threshold > 0.5)
    {
        return MultiplyColor;
    }
    
    if (Glow && threshold <= 0.5)
    {
        return lerp(float4(0.0, 0.0, 0.0, 0.0), MultiplyColor, threshold / 2.0);
    }
    
    return float4(0.0, 0.0, 0.0, 0.0);
}

technique WiggleShader
{
    pass WiggleEffect
    {
        PixelShader = compile ps_2_0 WiggleEffect();
    }
}
