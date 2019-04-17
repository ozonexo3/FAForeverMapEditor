// Based on: Kim, Pope: Screen Space Decals in Warhammer 40,000: Space Marine. SIGGRAPH 2012.
// http://www.popekim.com/2012/10/siggraph-2012-screen-space-decals-in.html

Shader "Ozone/Glow PostProcess"
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
			Blend One One

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.5
			#pragma multi_compile ___ UNITY_HDR_ON
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"

//			float4 _Color;

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _Mask;
			sampler2D _Glow;

			sampler2D _CameraGBufferTexture0Copy;

			struct appdata
			{
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			v2f vert(appdata v){
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.pos = float4(v.vertex.xy, 0.0, 0.5);
				o.uv = ComputeScreenPos(o.pos);
				return o;
			}

			void frag(v2f i, out float4 outEmission : SV_Target0)
			{
				float2 uv = i.uv.xy / i.uv.w;

				// Get color from texture and property
				float4 color = tex2D(_MainTex, uv);// * _Color;

				color.a *= tex2D(_Mask, uv).r;
				float RawAlpha = color.a;


				// Write emission, premultiply for proper blending
				//outEmission = tex2D(_CameraGBufferTexture0Copy, uv);
				outEmission = 0;
				outEmission.rgb = tex2D(_CameraGBufferTexture0Copy, uv).a + tex2D(_Glow, uv).rgb;
				//outEmission.rgb +=
				//outEmission.rgb += color.rgb * tex2D(_Glow, uv).rgb * (RawAlpha);
			}
			ENDCG
		}

	}

	Fallback Off
}
