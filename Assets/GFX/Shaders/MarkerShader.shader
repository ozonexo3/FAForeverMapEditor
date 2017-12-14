// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Ozone/MarkerShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		//_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		[Gamma]_Metallic ("Metallic", Range(0,1)) = 0.0

		_EmissionColor("Color", Color) = (0,0,0)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		//ColorMask RGB
		
		CGPROGRAM
		#define UNITY_BRDF_PBS BRDF3_Unity_PBS
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf SimpleLambert fullforwardshadows

		#include "Assets/GFX/Shaders/SimpleLambert.cginc"

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		//sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color, _EmissionColor;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutput o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			//o.Metallic = _Metallic;
			//o.Smoothness = _Glossiness;
			o.Alpha = c.a;

			o.Emission = _EmissionColor;
		}
		ENDCG
	}
	FallBack "Standard"
}
