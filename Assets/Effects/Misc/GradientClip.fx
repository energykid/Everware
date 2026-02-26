sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float4 LightingColor;

float ColorClip;
float ColorClipUpper;

float Range;

texture Gradient;
sampler2D GradientSampler = sampler_state
{
    Texture = (Gradient);
    AddressU = WRAP;
    AddressV = WRAP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

float4 box(sampler2D tex, float2 coords)
{
    //Texture color sum and weight sum for computing the average color
    float4 tex_sum = float4(0.0, 0.0, 0.0, 0.0);
    float weight_sum = 0.0;

    //Loop through desired texel "range"
    for (int x = -2; x <= 2; x++)
    {
        for (int y = -2; y <= 2; y++)
        {
            tex_sum += tex2D(tex, coords + float2(x, y));
            weight_sum += 1.0;
        }
    }    
    return tex_sum / weight_sum;
}

float4 Effect(float2 coords : TEXCOORD0) : COLOR0
{
    float4 col = tex2D(uImage0, coords);
    float4 col2 = box(uImage0, coords);
    if (col.a > 0.0 && col.r > ColorClip && col.r < ColorClipUpper)
    {
        float value = col.r - ColorClip;

        float4 extraCol = tex2D(GradientSampler, float2(value, 0));
        
        return extraCol * LightingColor;
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
