// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'
// Upgrade NOTE: replaced '_ProjectorClip' with 'unity_ProjectorClip'

Shader "Projector/Decal Bump" { 
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_BumpMap("Bump", 2D) = "Bump" {}
	}
	 
	Subshader {
		Tags {"Queue"="Transparent"}
		Pass {
			ZWrite Off
			Fog { Color (0, 0, 0) }
			ColorMask RGB
			//Blend DstColor One
			Blend SrcAlpha OneMinusSrcAlpha
			Offset -1, -1
	 
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc" // for _LightColor0
			
			struct v2f {
				float4 uvShadow : TEXCOORD0;
				float4 pos : SV_POSITION;
				fixed4 diff : COLOR0;
				float3 worldPos : TEXCOORD1;
				half3 tspace0 : TEXCOORD2;
				half3 tspace1 : TEXCOORD3;
				half3 tspace2 : TEXCOORD4;
				half3 worldNormal : TEXCOORD5;
			};
			
			float4x4 unity_Projector;
			
			v2f vert (float4 vertex : POSITION, float3 normal : NORMAL, float4 tangent : TANGENT, float2 uv : TEXCOORD0)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (vertex);
				o.uvShadow = mul (unity_Projector, vertex);


				o.worldPos = mul(unity_ObjectToWorld, vertex).xyz;
				half3 wNormal = UnityObjectToWorldNormal(normal);
				half3 wTangent = UnityObjectToWorldDir(tangent.xyz);
				half tangentSign = tangent.w * unity_WorldTransformParams.w;
				half3 wBitangent = cross(wNormal, wTangent) * tangentSign;
				o.tspace0 = half3(wTangent.x, wBitangent.x, wNormal.x);
				o.tspace1 = half3(wTangent.y, wBitangent.y, wNormal.y);
				o.tspace2 = half3(wTangent.z, wBitangent.z, wNormal.z);


				o.worldNormal = UnityObjectToWorldNormal(normal);
				// dot product between normal and light direction for
				// standard diffuse (Lambert) lighting
				// factor in the light color
				o.diff = _LightColor0;

				return o;
			}
			
			fixed4 _Color;
			sampler2D _BumpMap;
			
			fixed4 frag (v2f i) : SV_Target
			{

				half3 tnormal = UnpackNormal(tex2Dproj(_BumpMap, UNITY_PROJ_COORD(i.uvShadow)));
				half3 worldNormal;
				worldNormal.x = dot(i.tspace0, tnormal);
				worldNormal.y = dot(i.tspace1, tnormal);
				worldNormal.z = dot(i.tspace2, tnormal);

				float NormalAlpha = clamp((0.995 - dot(worldNormal, i.worldNormal)) * 100, 0, 1);

				half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));

				fixed4 texS = 0;

				// OnlyNormal
				if (nl < 0.5)
					texS.rgb = 0;
				else
					texS.rgb = i.diff.rgb;

				//texS.rgb = i.diff.rgb * nl - 1;
				//texS.a = (1 - NormalAlpha) * 2;
				//texS.a = abs(nl - 0.5) * (1 - NormalAlpha) * 2;

				texS.a = abs(nl - 0.5) * (NormalAlpha) ;

				//texS.rgb = NormalAlpha;
				//texS.a = NormalAlpha;

				float2 UvTest = i.uvShadow.xy / i.uvShadow.w;
				clip(min(UvTest.r, UvTest.g));
				clip(min(1 - UvTest.r, 1 - UvTest.g));

				clip(texS.a - 0.02);
				return texS;
			}
			ENDCG
		}


	}
}
