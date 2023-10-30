#ifndef __GRASS_INCLUDE__
#define __GRASS_INCLUDE__
 

struct GrassBlade
{    
    float3 position;
    float rotationY;
    float windNoise;
    //float ageNoise;
};

struct BoundInfo
{
    float3 boundsCenter;
    float3 boundsExtents;
};

#endif