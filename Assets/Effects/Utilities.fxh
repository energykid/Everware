float2 pixelateCoords(float2 coords, float2 pixelGridSize)
{
    return floor(coords * pixelGridSize) / pixelGridSize;
}