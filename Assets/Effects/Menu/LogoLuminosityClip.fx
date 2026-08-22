sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float SmallSpeedModifier;
float Frame;
float FrameCount;

float2 TextResolution;
float2 FillResolution;

float2 Offset;

bool GrayscaleOnly;

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

float ColorClip;
float ColorClipUpper;

float4 LogoEffect(float2 coords : TEXCOORD0) : COLOR0
{
    float l = tex2D(uImage0, coords).r * tex2D(uImage0, coords).a;

    float4 t = tex2D(FillSampler, (((coords + float2(Timer * SmallSpeedModifier, -(Frame / FrameCount))) * (float2(1.0, 1.0) / FillResolution * TextResolution)) + (Offset / TextResolution)) / float2(1.0, FrameCount));
    t.r += 0.05;
    t.b += 0.2;
    
    return tex2D(uImage0, coords) * lerp(t, float4(1.0, 1.0, 1.0, 1.0), l - 0.5);
}

float4 Effect(float2 coords : TEXCOORD0) : COLOR0
{
    float4 col = tex2D(uImage0, coords);
    
    float lum = (col.r + col.g + col.b) / 3.0;
    
    if (col.a > 0.0 && lum > ColorClip && lum < ColorClipUpper)
    {
        bool b = col.r != col.g && col.g != col.b;
        if (b || col.r == 1.0)
        {
            if (!GrayscaleOnly)
                return col;
            else
                return float4(0.0, 0.0, 0.0, 0.0);
        }
        return LogoEffect(coords);
    }
    return float4(0.0, 0.0, 0.0, 0.0);
}

technique GradientShader
{
    pass Effect
    {
        PixelShader = compile ps_2_0 Effect();
    }
}
