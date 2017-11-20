// Based on: Kim, Pope: Screen Space Decals in Warhammer 40,000: Space Marine. SIGGRAPH 2012.
// http://www.popekim.com/2012/10/siggraph-2012-screen-space-decals-in.html

Shader "Ozone/Deferred Decal"
{
	Properties
	{
		_Mask("Mask", 2D) = "white" {}
		//[PerRendererData] _MaskMultiplier("Mask (Multiplier)", Float) = 1.0
		//_MaskNormals("Mask Normals?", Float) = 1.0

		_MainTex("Albedo", 2D) = "white" {}
		[HDR] _Color("Albedo (Multiplier)", Color) = (1,1,1,1)
		_Glow("Glow", 2D) = "black" {}

		[Normal] _NormalTex ("Normal", 2D) = "yellow" {}
		_NormalMultiplier ("Normal (Multiplier)", Float) = 1.0

		_NormalBlendMode("Normal Blend Mode", Float) = 0
		_AngleLimit("Angle Limit", Float) = 0.5

		[PerRendererData] _CutOffLOD("CutOffLOD", Float) = 0.5
		[PerRendererData] _NearCutOffLOD("NearCutOffLOD", Float) = 0.5
	}

	// Use custom GUI for the decal shader
	//CustomEditor "ThreeEyedGames.DecalShaderGUI"

	SubShader
	{
		Cull Front
		ZTest GEqual
		ZWrite Off

		// Pass 0: Albedo and emission/lighting
		Pass
		{
			Blend One OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.5
			#pragma multi_compile ___ UNITY_HDR_ON
			#pragma multi_compile_instancing
			#include "DecalsCommon.cginc"

//			float4 _Color;

			sampler2D _Mask;
			sampler2D _Glow;
			sampler2D _WaterRam;
			sampler2D _UtilitySamplerC;

			
			float3 ApplyWaterColor( float depth, float3  inColor){
				float4 wcolor = tex2D(_WaterRam, float2(depth,0));
				return lerp( inColor.rgb, wcolor.rgb, wcolor.a );
			}

			void frag(v2f i, out float4 outAlbedo : SV_Target0, out float4 outGlow : SV_Target1) // 
			{
				// Common header for all fragment shaders
				DEFERRED_FRAG_HEADER

				// Get normal from GBuffer
				//float3 gbuffer_normal = tex2D(_CameraGBufferTexture2, uv) * 2.0f - 1.0f;
				//clip(dot(gbuffer_normal, i.decalNormal) - _AngleLimit); // 60 degree clamp

				// Get color from texture and property
				float4 color = tex2D(_MainTex, texUV);// * _Color;

				
				float4 waterTexture = tex2D( _UtilitySamplerC, wpos.xz * half2(0.009765, -0.009765) + half2(0, 0));
				color.rgb = ApplyWaterColor( waterTexture.g, color.rgb);	

				color.a *= blend * tex2D(_Mask, texUV).r;
								float RawAlpha = color.a;

				//color.rgb = blend * 1000;


				// Write albedo, premultiply for proper blending
				outAlbedo = float4(color.rgb * color.a, color.a);
				//color *= 1 - float4(ShadeSH9(float4(gbuffer_normal, 1.0f)), 1.0f);

				//color.rgb = 10000 * RawAlpha;

				// Handle logarithmic encoding in Gamma space
#ifndef UNITY_HDR_ON
				//color *= float4(ShadeSH9(float4(gbuffer_normal, 1.0f)), 1.0f);
				//color.rgb = exp2(-color.rgb);
#endif

				// Write emission, premultiply for proper blending
				//outEmission = float4(color.rgb * color.a, color.a) + tex2D(_Glow, texUV);

				//outAlbedo = tex2D(_CameraGBufferTexture4Copy, uv);
				//outAlbedo.rgb = lerp(outAlbedo.rgb, color.rgb * color.a, color.a);

				//outEmission = tex2D(_CameraGBufferTexture4Copy, uv);
				//outEmission.rgb = lerp(outEmission.rgb, outAlbedo.rgb, outAlbedo.a);

				//outEmission.rgb = 0.2;
				//outEmission.a = 1;

				//outEmission.rgb = color.rgb * tex2D(_Glow, texUV).rgb * (blend * 5 * RawAlpha);
				//outEmission.a = RawAlpha;
				outGlow = float4(1,0,0,RawAlpha);
			}
			ENDCG
		}

		// Pass 1: Normals and specular / smoothness
		Pass
		{
			// Manual blending
			Blend One OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.5
			#pragma multi_compile_instancing
			#include "UnityStandardUtils.cginc"
			#include "DecalsCommon.cginc"

			void frag(v2f i, out float4 outNormal : SV_Target1)
			{
				DEFERRED_FRAG_HEADER

				// Get normal from GBuffer
				fixed3 gbuffer_normal = tex2D(_CameraGBufferTexture2Copy, uv) * 2.0f - 1.0f;
				//clip(dot(gbuffer_normal, i.decalNormal) - _AngleLimit); // 60 degree clamp

				float3 decalBitangent;
				if (_NormalBlendMode == 0)
				{
					// Reorient decal
					i.decalNormal = gbuffer_normal;
					decalBitangent = cross(i.decalNormal, i.decalTangent);
					float3 oldDecalTangent = i.decalTangent;
					i.decalTangent = cross(i.decalNormal, decalBitangent);
					if (dot(oldDecalTangent, i.decalTangent))
						i.decalTangent *= -1;
				}
				else
				{
					decalBitangent = cross(i.decalNormal, i.decalTangent);
				}

				// Get normal from normal map
				//float3 normal = UnpackScaleNormal(tex2D(_NormalTex, texUV), _NormalMultiplier);
				//float3 normal = UnpackNormalDXT5nm(tex2D(_NormalTex, texUV));
				float4 decalRaw = tex2D(_NormalTex, texUV);
				float3 normal;
				normal.xz = decalRaw.ag * 2 - 1;
				normal.y = sqrt(1 - dot(normal.xz,normal.xz)) ;

				

				normal = UnpackNormalDXT5nm(tex2D(_NormalTex, texUV));
				normal.y *= 0.5;
				normal.xy *= blend;

				normal = normalize(normal);
				
				// Clip to blend it with other normal maps
				float AlphaNormal = clamp(dot(normal, half3(0,0,1)) * 10, 0, 1);
				//clip(0.999 -  AlphaNormal);
				//clip(0.5 - normal.y);
				clip(AlphaNormal - 0.1);

				normal = mul(normal, half3x3(i.decalTangent, decalBitangent, i.decalNormal));

				// Simple alpha blending of normals
				//float normalMask = _MaskNormals ? mask : UNITY_ACCESS_INSTANCED_PROP(_MaskMultiplier);
				//float normalMask = 1;
				//normal = (1.0f - normalMask) * gbuffer_normal + normalMask * normal;
				//normal = normalize(normal);


				// Write normal
				outNormal = float4(normal * 0.5f + 0.5f, AlphaNormal);
			}
			ENDCG
		}
	}

	Fallback Off
}
