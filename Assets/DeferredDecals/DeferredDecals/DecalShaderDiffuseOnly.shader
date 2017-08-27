// Upgrade NOTE: commented out 'float4x4 _CameraToWorld', a built-in variable
// Upgrade NOTE: replaced '_CameraToWorld' with 'unity_CameraToWorld'
// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// http://www.popekim.com/2012/10/siggraph-2012-screen-space-decals-in.html

Shader "Decal/DecalShader"
{
	Properties
	{
		_MainTex ("Diffuse", 2D) = "white" {}
		_SpecularTex ("Specular", 2D) = "white" {}
	}
	SubShader
	{
		Pass
		{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
			Fog { Mode Off } // no fog in g-buffers pass
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			//ZTest  Always
			//Cull Back
			//Offset -10000, -1

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma exclude_renderers nomrt
			
			#include "UnityCG.cginc"

			struct v2f
			{
				float4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
				float4 screenUV : TEXCOORD1;
				float3 ray : TEXCOORD2;
				half3 orientation : TEXCOORD3;
			};

			v2f vert (float3 v : POSITION)
			{
				v2f o;
				//v.y = 0;
				o.pos = UnityObjectToClipPos (float4(v,1));
								
				o.uv = v.xz+0.5;
				o.screenUV = ComputeScreenPos (o.pos);


				o.ray = mul (UNITY_MATRIX_MV, float4(v,1)).xyz * float3(-1,-1,1);
				o.orientation = mul ((float3x3)unity_ObjectToWorld, float3(0,1,0));
				return o;
			}

			CBUFFER_START(UnityPerCamera2)
			// float4x4 _CameraToWorld;
			CBUFFER_END

			sampler2D _MainTex;
			sampler2D_float _CameraDepthTexture;
			sampler2D _NormalsCopy;

			sampler2D _WaterRam;
			sampler2D _UtilitySamplerC;

			//void frag(
			//	v2f i,
			//	out half4 outDiffuse : COLOR0,			// RT0: diffuse color (rgb), --unused-- (a)
			//	out half4 outSpecRoughness : COLOR1,	// RT1: spec color (rgb), roughness (a)
			//	out half4 outNormal : COLOR2,			// RT2: normal (rgb), --unused-- (a)
			//	out half4 outEmission : COLOR3			// RT3: emission (rgb), --unused-- (a)
			//)

			float3 ApplyWaterColor( float depth, float3  inColor){
				float4 wcolor = tex2D(_WaterRam, float2(depth,0));
				return lerp( inColor.rgb, wcolor.rgb, wcolor.a );
			}

			fixed4 frag(v2f i) : SV_Target
			{
				i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
				float2 uv = i.screenUV.xy / i.screenUV.w;
				// read depth and reconstruct world position
				float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
				depth = Linear01Depth (depth);
				float4 vpos = float4(i.ray * depth,1);
				float3 wpos = mul (unity_CameraToWorld, vpos).xyz;
				float3 opos = mul (unity_WorldToObject, float4(wpos,1)).xyz;

				clip (float3(0.5,0.5,0.5) - abs(opos.xyz));


				i.uv = opos.xz+0.5;

				//half3 normal = tex2D(_NormalsCopy, uv).rgb;
				//fixed3 wnormal = normal.rgb * 2.0 - 1.0;
				//clip (dot(wnormal, i.orientation) - 0.3);

				
				
				fixed4 col = tex2D (_MainTex, half2(i.uv.x, 1 - i.uv.y));

				float4 waterTexture = tex2D( _UtilitySamplerC, wpos.xz * half2(0.009765, -0.009765) + half2(0, 0));
				col.rgb = ApplyWaterColor( waterTexture.g, col.rgb);	

				//col.rgb = wpos * 0.009;

				return col;
			}
			ENDCG
		}		

	}

	Fallback Off
}
