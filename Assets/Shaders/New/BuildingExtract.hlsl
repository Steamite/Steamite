#ifndef BUILDING_UNPACK_2BIT_INCLUDED
#define BUILDING_UNPACK_2BIT_INCLUDED

struct PackedBuilding
{
    uint data;
};

// Declare the buffer set via MaterialPropertyBlock
//#if defined(UNITY_SUPPORT_INSTANCING)
    StructuredBuffer<PackedBuilding> _BuildingInstanceBuffer;
//#endif
/*
 Bit Layout Breakdown (32-bit uint):
 -------------------------------------------------------------
 Bits [00 .. 11] : X Grid Position (12 bits -> range 0..4095)
 Bits [12 .. 23] : Z Grid Position (12 bits -> range 0..4095)
 Bits [24 .. 25] : Y Rotation      (2 bits  -> 0: Down, 1: Right, 2: Up, 3: Left)
 Bits [26 .. 31] : Unused / Extra Flags (6 bits free for variant IDs, damage states, etc.)

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
    uint GridY,
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
    
    
    float c = cosTable[rot2];
    float s = sinTable[rot2];

    // Compute Y-rotation around center
    float3 rotatedVertex;
    rotatedVertex.x = localVertexPosition.x * c - localVertexPosition.z * s;
    rotatedVertex.y = localVertexPosition.y;
    rotatedVertex.z = localVertexPosition.x * s + localVertexPosition.z * c;
    // Translate to grid cell position
    float3 worldPos = float3((float) (gridX * cellSize) + offset, GridY, (float) (gridZ * cellSize) + offset);
    
    
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
    out float3 Rotation)
{
    uint packedUint = 0;
    uint RotationIndex = 0;
    uint GridY;
    uint GridX;
    uint GridZ;
    //#if defined(UNITY_SUPPORT_INSTANCING)
        // Fetch the uint for this specific instance using the Instance ID
        packedUint = _BuildingInstanceBuffer[(uint)instanceID].data;
    //#endif
    
    UnpackBuildingData2Bit(packedUint, GridX, GridZ, GridY, RotationIndex);
    WorldPosition = TransformInstanceVertex2Bit(localPosition, GridX, GridZ, GridY, RotationIndex, offset, cellSize, Rotation);
}

#endif // BUILDING_UNPACK_2BIT_INCLUDED