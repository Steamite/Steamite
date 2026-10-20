#ifndef BUILDING_UNPACK_2BIT_INCLUDED
#define BUILDING_UNPACK_2BIT_INCLUDED

struct PackedBuilding
{
    uint data;
};

// Declare the buffer set via MaterialPropertyBlock
//#if defined(UNITY_SUPPORT_INSTANCING)
StructuredBuffer<PackedBuilding> _BuildingInstanceBuffer;
StructuredBuffer<float2> _BatchGroupBuffer;
int _Offset;
int _GroupIndex;
int _SelectedLevel;
//#endif
/*
 Bit Layout Breakdown (32-bit uint):
 -------------------------------------------------------------
/// 00 - 09 - x             <br/>
/// 10 - 19 - z             <br/>
/// 20 - 22 - y             <br/>
/// 23 - 24 - y (rotation)  <br/>
 -------------------------------------------------------------
*/

/// <summary>
/// Unpacks raw uint bitfield into discrete grid coordinates and cardinal rotation index.
/// </summary>
void UnpackBuildingData2Bit(
    uint packedData,
    out uint gridX,
    out uint gridZ,
    out uint gridY,
    out uint rot2)
{
    gridX = packedData & 0x3FF;
    gridZ = (packedData >> 10) & 0x3FF;
    gridY = (packedData >> 20) & 0x7;
    rot2 = (packedData >> 23) & 0x3; 
}

/// <summary>
/// Transforms object-space vertex to world-space using fast 2-bit cardinal rotation lookup.
/// </summary>
float3 TransformInstanceVertex2Bit(
    float3 localVertexPosition,
    uint gridX,
    uint gridZ,
    uint rot2,
    float offset,
    float cellSize,
    out float3 Rotation)
{
    // Exact Cosine and Sine lookups for 0, 90, 180, 270 degrees (Zero trig math required!)
    // static const arrays are embedded directly into GPU registers
    static const float cosTable[4] = { 1.0, 0.0, -1.0, 0.0 };
    static const float sinTable[4] = { 0.0, 1.0, 0.0, -1.0 };
    static const float degTable[4] = { 0.0, 90, 180, 270 };
    
    static const float2 offsetTable[4] = 
    { 
        float2(0, 0),
        float2(1, 0),
        float2(1, 1),
        float2(0, 1),
    };
    
    float c = cosTable[rot2];
    float s = sinTable[rot2];
    
    float2 rotatedPos;
    rotatedPos.x = localVertexPosition.x * c - localVertexPosition.z * s;
    rotatedPos.y = localVertexPosition.x * s + localVertexPosition.z * c;
    
    float2 off = offsetTable[rot2] * _BatchGroupBuffer[_GroupIndex];
    
    float3 rotatedVertex;
    rotatedVertex.x = rotatedPos.x + off.x;
    rotatedVertex.y = localVertexPosition.y;
    rotatedVertex.z = rotatedPos.y + off.y;
    
    // Translate to grid cell position
    float3 worldPos = float3((float) (gridX * cellSize) + offset, 0, (float) (gridZ * cellSize) + offset);
    
    
    Rotation = float3(0, radians(degTable[rot2]), 0);
    
    return rotatedVertex + worldPos;
}

// Shader Graph Custom Function Entry Point
void UnpackBuilding2Bit_float(
    float instanceID,
    float3 localPosition,
    float cellSize,
    float offset,

    out float3 WorldPosition,
    out float3 Rotation,
    out float instanceIDFixed)
{
    uint packedUint = 0;
    uint RotationIndex = 0;
    uint GridY;
    uint GridX;
    uint GridZ;
    
    #if defined(UNITY_ANY_INSTANCING_ENABLED)
        #if defined(SHADER_API_D3D11) || defined(SHADER_API_D3D12)
            instanceIDFixed = _Offset + (uint)instanceID;
        #else
            instanceIDFixed = _Offset + unity_InstanceID;
        #endif
    #endif
    
#if defined(SHADERGRAPH_PREVIEW)
    packedUint = 0;
#else
    
    
    packedUint = _BuildingInstanceBuffer[(uint) instanceIDFixed].data;
#endif
    
    UnpackBuildingData2Bit(packedUint, GridX, GridZ, GridY, RotationIndex);
    if (_SelectedLevel == GridY)
    {
        WorldPosition = TransformInstanceVertex2Bit(localPosition, GridX, GridZ, RotationIndex, offset, cellSize, Rotation);
    }
    else
    {
      WorldPosition = float3(0, 0, 0);
    }
    //Color = float3(instanceID, instanceID, instanceID);
}

#endif // BUILDING_UNPACK_2BIT_INCLUDED