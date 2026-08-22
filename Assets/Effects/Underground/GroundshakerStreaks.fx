sampler uImage0 : register(s0)
{
    AddressU = WRAP;
    AddressV = WRAP;
};
sampler uImage1 : register(s1);

float2 Coords;
float Time;
float Clip;
float4 Color;

float4 Effect(float2 coords : TEXCOORD0) : COLOR0
{
    float4 col = tex2D(uImage0, ((coords * Coords) + float2(0.0, Time)) * float2(1.0, 2.0));
    float lr = lerp(0.0, 1.0, coords.x * 2.0);
    if (coords.x > 0.5)
        lr = lerp(1.0, 0.0, (coords.x - 0.5) * 2.0);
        
    float lr2 = lerp(0.0, 1.0, coords.y * 2.0);
    if (coords.x > 0.5)
        lr2 = lerp(1.0, 0.0, (coords.y - 0.5) * 2.0);
        
    lr = lerp(lr, 1.0, lr);
    lr2 = lerp(lr2, 1.0, lr2);
        
    col.r = col.r * lr * lr2;
    if (col.r > Clip)
    {
        return Color;
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
