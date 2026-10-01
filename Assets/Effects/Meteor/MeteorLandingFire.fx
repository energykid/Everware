#include "../Utilities.fxh"

sampler2D Noise : register(s0);

float Time;
float4 StartColor;
float4 MiddleColor;
float4 EndColor;

#define TAU (6.28318530718)
#define PI (3.14159265359)
#define PIOVER2 (1.57079632679)

float Map(float value, float start1, float stop1, float start2, float stop2)
{
    return start2 + (stop2 - start2) * ((value - start1) / (stop1 - start1));
}

float4 MeteorFireShaderFragment(float2 uv : TEXCOORD0) : COLOR0
{
    float2 flameUv = uv;
    
    float startBoost = pow(uv.x, 4);
    
    float falloff = saturate(uv.x - sin(uv.y * PI) * 0.13);
    
    float endFade = 1 - pow(1 - uv.x, 2);
    
    float startFade = (1 - pow(saturate(saturate(uv.x - sin(uv.y * PI) * 0.08) + 0.09), 7));
    
    uv.x = Map(uv.x, 0.8, 1, 0, 1);
    
    flameUv.x -= saturate(sin(uv.y * PI) * 0.3 * falloff);
    flameUv.x *= 0.6;
    flameUv.x += Time * 0.7;
    
    float n = tex2D(Noise, flameUv).r;
    n *= n;
    
    float brightness = saturate((sin(flameUv.x * 6.2 * TAU) * 0.4) + 0.4 + (startBoost * (1 + n)));
    
    float fade = sin(uv.y * PI) * endFade * startFade;
    
    n *= brightness * fade;
    n += pow(brightness, 7) * fade;
    n = 1 - pow(1 - saturate(n + 0.02), 2);
    
    const int steps = 5;
    n *= steps;
    n = floor(n);
    n /= steps;
    
    const int color_steps = 3;
    float4 color = lerp(MiddleColor * 1.9, StartColor * 1.9, (uv.x - sin(uv.y * PI)) + 1.2 + (n * 0.3));
    
    color = lerp(color, EndColor, 1 - endFade);
    
    return color * n;
}

technique Technique1
{
    pass MeteorFireShader
    {
        PixelShader = compile ps_3_0 MeteorFireShaderFragment();
    }
}