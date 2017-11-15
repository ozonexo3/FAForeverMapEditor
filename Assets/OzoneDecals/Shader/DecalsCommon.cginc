#ifndef DEFERRED_DECALS_COMMON_INCLUDED
#define DEFERRED_DECALS_COMMON_INCLUDED

#define UNITY_SHADER_NO_UPGRADE

#include "UnityCG.cginc"

sampler2D _MaskTex;
int _MaskNormals;

sampler2D _MainTex;
float4 _MainTex_ST;
float4 _Color;

sampler2D _NormalTex;
float _NormalMultiplier;

float _AngleLimit;

int _DecalBlendMode;
int _DecalSrcBlend;
int _DecalDstBlend;
int _NormalBlendMode;

sampler2D _CameraDepthTexture;
sampler2D _CameraGBufferTexture2;
sampler2D _CameraGBufferTexture2Copy;

struct appdata
{
	float4 vertex : POSITION;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
	float4 pos : SV_POSITION;
	float4 uv : TEXCOORD1;
	float3 ray : TEXCOORD2;
	half3 decalNormal : TEXCOORD3;
	half3 decalTangent : TEXCOORD4;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

UNITY_INSTANCING_CBUFFER_START(Fade)
UNITY_DEFINE_INSTANCED_PROP(float, _CutOffLOD)
UNITY_DEFINE_INSTANCED_PROP(float, _NearCutOffLOD)
UNITY_INSTANCING_CBUFFER_END

v2f vert(appdata v)
{
	v2f o;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_TRANSFER_INSTANCE_ID(v, o);
	o.pos = UnityObjectToClipPos(v.vertex);
	o.uv = ComputeScreenPos(o.pos);
	o.ray = mul(UNITY_MATRIX_MV, v.vertex).xyz * float3(-1, -1, 1);
	o.decalNormal  = normalize(mul((float3x3)unity_ObjectToWorld, float3(0, 1, 0)));
	o.decalTangent = normalize(mul((float3x3)unity_ObjectToWorld, float3(1, 0, 0)));
	return o;
}

// Common header, mainly taken from UnityDeferredLibrary.cginc
#define DEFERRED_FRAG_HEADER UNITY_SETUP_INSTANCE_ID (i); \
i.ray = i.ray * (_ProjectionParams.z / i.ray.z); \
float2 uv = i.uv.xy / i.uv.w; \
float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv); \
depth = Linear01Depth(depth); \
float CutoffDistance = UNITY_ACCESS_INSTANCED_PROP(_CutOffLOD) * 0.4; \
float blend = clamp((UNITY_ACCESS_INSTANCED_PROP(_CutOffLOD) - depth) / CutoffDistance, 0, 1 ); \
CutoffDistance = UNITY_ACCESS_INSTANCED_PROP(_NearCutOffLOD); \
blend *= clamp((depth - CutoffDistance) / CutoffDistance, 0, 1 ); \
float4 vpos = float4(i.ray * depth,1); \
float3 wpos = mul(unity_CameraToWorld, vpos).xyz; \
float3 clipPos = mul(unity_WorldToObject, float4(wpos, 1)).xyz; \
clip(0.5f - abs(clipPos.xyz)); \
float2 texUV = TRANSFORM_TEX((clipPos.xz + 0.5), _MainTex); \
texUV = half2(texUV.x, 1 - texUV.y);

#endif // DEFERRED_DECALS_COMMON_INCLUDED


//float mask = tex2D(_MaskTex, texUV).r; \
//clip(mask - 0.0005f);
//UNITY_ACCESS_INSTANCED_PROP(_NearCutOffLOD)