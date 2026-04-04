float2 pixelateCoords(float2 coords, float2 pixelGridSize)
{
    return floor(coords * pixelGridSize) / pixelGridSize;
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