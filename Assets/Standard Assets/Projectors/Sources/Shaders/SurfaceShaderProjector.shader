// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Projector/SurfaceShaderProjector" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "Queue" = "Transparent" }
		ZWrite Off
		Fog{ Color(0, 0, 0) }
		ColorMask RGB
		//Blend DstColor One
		Blend SrcAlpha OneMinusSrcAlpha
		Offset -1, -1

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Lambert vertex:vert alpha noshadow

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0
		#include "UnityCG.cginc"

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float2 uvProj;
			float2 uvProj2;
		};

		fixed4 _Color;

		float4x4 unity_Projector;

		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			//o.uv_MainTex = v.texcoord;
			//o.uvProj = mul( float4(v.vertex.xyz, 1), _Object2World);
			//o.uvProj = UnityObjectToClipPos(v.vertex);

			float4 WorldVert = mul(unity_Projector, mul(unity_ObjectToWorld, v.vertex));
			//o.uvProj = WorldVert.xyzw;
			o.uvProj = float2(WorldVert.x, WorldVert.y);
			o.uvProj2 = float2(WorldVert.z, WorldVert.w);

			/* Do some cool things here */
			// transform back into local space

			//v.vertex = mul(_World2Object, world_space_vertex);
			//o.uvProj = float4(v.vertex.xyz, 1);
		}

		void surf (Input IN, inout SurfaceOutput o) {
			// Albedo comes from a texture tinted by color
			//fixed4 c = tex2Dproj (_MainTex, UNITY_PROJ_COORD(IN.uvProj));
			//fixed4 c = tex2Dproj(_MainTex, UNITY_PROJ_COORD( mul(unity_Projector, IN.uvProj)));
			//fixed4 c = tex2Dproj(_MainTex, UNITY_PROJ_COORD(mul(UNITY_MATRIX_V, float4(IN.uv_MainTex, 0, 1)).xy));

			float4 NewCoord = float4(IN.uvProj.r, IN.uvProj.g, IN.uvProj2.r, IN.uvProj2.g);
			fixed4 c = tex2Dproj(_MainTex, UNITY_PROJ_COORD(NewCoord));
			//fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = 0;
			o.Emission = c.rgb;
			//o.Emission = float3(IN.uvProj.r, IN.uvProj.g, IN.uvProj2.r);
			//clip(c.a - 0.1);
			o.Alpha = 1;
			//o.Albedo = c.rgb;
			//o.Alpha = c.a;
		}
		ENDCG
	}
}
