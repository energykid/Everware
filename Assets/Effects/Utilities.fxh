float2 pixelateCoords(float2 coords, float2 pixelGridSize)
{
    return float2(floor(coords.x * pixelGridSize.x) / pixelGridSize.x, floor(coords.y * pixelGridSize.y) / pixelGridSize.y);
}

float vignetteMult(float2 coords)
{
    float lr = lerp(0.0, 1.0, coords.x * 2.0);
    if (coords.x > 0.5)
        lr = lerp(1.0, 0.0, (coords.x - 0.5) * 2.0);
        
    float lr2 = lerp(0.0, 1.0, coords.y * 2.0);
    if (coords.x > 0.5)
        lr2 = lerp(1.0, 0.0, (coords.y - 0.5) * 2.0);
        
    lr = lerp(lr, 1.0, lr);
    lr2 = lerp(lr2, 1.0, lr2);
    
    return lr * lr2;
}

float2 rotated(float2 base, float rotation)
{
    float x = cos(rotation * base.x) - sin(rotation * base.y);
    float y = sin(rotation * base.x) + cos(rotation * base.y);

    return float2(x, y);
}

float distance(float2 base, float2 dest)
{
    return abs(sqrt(pow(dest.x - base.x, 2) + pow(dest.y - base.y, 2)));
}

float length2(float2 base)
{
    return abs(sqrt(pow(base.x, 2) + pow(base.y, 2)));
}

float2 random2(float2 base, float2 seeds)
{
    return float2(sin(base.x * seeds.x), sin(base.y * seeds.y));
}

float random(float base, float seed)
{
    return sin(base * seed);
}

float2 noise(float2 pos, float2 seeds)
{
    float2 pL = float2(pos.x % 1.0, pos.y % 1.0);
    
    float2 p1 = pos - pL;
    float2 p2 = pos - pL + float2(1.0, 1.0);
    
    float2 a = lerp(random2(p1, seeds), random2(p2, seeds), pL);

    return a;
}

float skewLerp(float time, float start, float end, float pointA, float pointB, float defaultValue)
{
    if (time >= start && time <= end)
    {
        return lerp(pointA, pointB, (time - start) / (end - start));
    }
    return defaultValue;
}

float2 swirl(float2 coords, float2 center, float amount, float extra)
{
    return rotated(coords - center, (length2(coords - center) * amount) + extra) + center;
}

// thanks to existical on shadertoy for original source; adapted for tmodloader usage and memory efficiency at visual fidelity cost
float4 blur(float2 uv, sampler2D smp, float rad)
{
    float Pi = 6.28318530718; // Pi*2
    
    // GAUSSIAN BLUR SETTINGS {{{
    float Directions = 16.0; // BLUR DIRECTIONS (Default 16.0 - More is better but slower)
    float Quality = 4.0; // BLUR QUALITY (Default 4.0 - More is better but slower)
    // GAUSSIAN BLUR SETTINGS }}}
   
    float2 Radius = rad;
    
    // Pixel colour
    float4 Color = tex2D(smp, uv);
    
    // Blur calculations
    for (float d = 0.0; d < Pi; d += Pi / Directions)
    {
        for (float i = 1.0 / Quality; i <= 1.0; i += 1.0 / Quality)
        {
            Color += tex2D(smp, uv + float2(cos(d), sin(d)) * Radius * i);
        }
    }
    
    // Output to screen
    Color /= Quality * Directions - 15.0;
    return Color;
}