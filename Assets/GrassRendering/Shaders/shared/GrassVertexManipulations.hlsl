#ifndef __GRASS_VERTEX_MANIPULATIONS_INCLUDE__
#define __GRASS_VERTEX_MANIPULATIONS_INCLUDE__

#include "Transformations.cginc"

float4 positionVertexInWorld(GrassBlade grassBlade, float4 positionOS) {
    // generate a translation matrix to move the vertex
    float4x4 scaleMatrix = float4x4(
    0.2, 0, 0, 0,
    0, 0.3, 0, 0,
    0, 0, 0.2, 0,
    0, 0, 0, 1);
    
    float4x4 translationMatrix = getTranslation_Matrix(grassBlade.position);
    float4x4 rotationMatrix = getRotationY_Matrix(grassBlade.rotationY);    
    float4x4 transformationMatrix = mul(translationMatrix, mul(scaleMatrix, rotationMatrix));

    return mul(transformationMatrix, positionOS);
}

float4 normalVertexInWorld(float rotationY, float3 normalOS) {
    // generate a translation matrix to move the vertex    
    float4x4 rotationMatrix = getRotationY_Matrix(rotationY);    

    // translate the object pos to world pos
    float4 normalWS = mul(unity_ObjectToWorld, float4(normalOS, 1));
    // then use the matrix to translate and rotate it
    normalWS = mul(rotationMatrix, normalWS);

    return normalWS;
}

float4 applyWind(GrassBlade grassBlade, float4 worldPosition, float3 windDirection, float windForce) {
    float3 displaced = worldPosition.xyz + (normalize(windDirection) * windForce * grassBlade.windNoise);
    float4 displacedByWind = float4(displaced, 1);

    // base of the grass needs to be static on the floor
    return lerp(worldPosition, displacedByWind, (worldPosition.y - grassBlade.position.y) / 1.4);//uv.y);
}

float3 positionGrassVertexInHClipPos(
    StructuredBuffer<GrassBlade> GrassBladesBuffer,
    uint instance_id,
    out GrassBlade grassBlade,
    float4 positionOS,    
    float3 windDirection,
    float windForce
) {
    // get the instanced grass blade
    grassBlade = GrassBladesBuffer[instance_id];

    float4 worldPosition = positionVertexInWorld(grassBlade, positionOS);
    worldPosition = applyWind(grassBlade, worldPosition, windDirection, windForce);

    // translate the world pos to clip pos
    //return TransformWorldToHClip(worldPosition);
    return worldPosition.xyz;
}

#endif
