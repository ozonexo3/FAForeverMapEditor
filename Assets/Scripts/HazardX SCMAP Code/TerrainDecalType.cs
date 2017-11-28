// ***************************************************************************************
// * SCMAP Loader
// * Copyright Unknown
// * Filename: TerrainDecalType.cs
// * Source: http://www.hazardx.com/details.php?file=82
// ***************************************************************************************


public enum TerrainDecalType:int
{
   TYPE_UNDEFINED,
   TYPE_ALBEDO,
   TYPE_NORMALS,
   TYPE_WATER_MASK,
   TYPE_WATER_ALBEDO,
   TYPE_WATER_NORMALS,
   TYPE_GLOW,
   TYPE_NORMALS_ALPHA,
   TYPE_GLOW_MASK,
   TYPE_FORCE_DWORD,
}


public enum TerrainDecalTypeString : int
{
	UNDEFINED,
	ALBEDO,
	NORMALS,
	WATER_MASK,
	WATER_ALBEDO,
	WATER_NORMALS,
	GLOW,
	NORMALS_ALPHA,
	GLOW_MASK,
	FORCE_DWORD,
}