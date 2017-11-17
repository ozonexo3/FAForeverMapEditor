Shader "Ozone/OnPreRender Shader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		//_MainTex ("Albedo (RGB)", 2D) = "white" {}
		//_BumpMap ("Normal ", 2D) = "bump" {}
		_Clip ("Alpha cull", Range (0, 1)) = 0.5
		//_Glossiness ("Smoothness", Range(0,1)) = 0.5
		//_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="TransparentCutout" "Queue" = "Geometry" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Lambert noforwardadd novertexlights noshadowmask halfasview interpolateview

		#pragma target 3.5

		half _Clip;
		struct Input {
			float2 uv_MainTex;
		};
		fixed4 _Color;


		void surf (Input IN, inout SurfaceOutput o) {
			o.Albedo = _Color.rgb;


			clip(_Color.a - _Clip);
			o.Alpha = _Color.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
