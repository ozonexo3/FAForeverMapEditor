// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Custom/UnitShader" {
	Properties {
		_Color ("Army", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap ("Normal ", 2D) = "gray" {}
		_SpecTeam("SpecTeam ", 2D) = "black" {}
		//_Glossiness ("Smoothness", Range(0,1)) = 0.5
		//_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="TransparentCutout" "Queue" = "Transparent+2" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf BlinnPhong vertex:vert addshadow

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex, _BumpMap, _SpecTeam;
		struct Input {
			float2 uv_MainTex;
			float3 mViewVec     : 	TEXCOORD5;
			float3 worldNormal;
			INTERNAL_DATA
		};

		fixed4 _SunColor;
		//fixed4 _SunAmbience;
		//fixed4 _ShadowColor;


		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.mViewVec = normalize(mul(unity_ObjectToWorld, v.vertex).xyz - _WorldSpaceCameraPos);
		}

		//half _Glossiness;
		//half _Metallic;
		fixed4 _Color;
		uniform float3 SunDirection;
		uniform samplerCUBE environmentSampler;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		inline fixed3 UnpackNormalDXT5nmScaled (fixed4 packednormal, fixed scale)
		{
			fixed3 normal = 0;
			normal.xz = packednormal.wx * 2 - 1;
			normal.y = sqrt(1 - saturate(dot(normal.xz, normal.xz)));
			normal.xz *= scale;

			return normal.xzy;
		}

		static const float3 NormalMappedPhongCoeff = float3(0.6, 0.80, 0.90);

		void surf (Input IN, inout SurfaceOutput o) {





			// Albedo comes from a texture tinted by color
			fixed4 albedo = tex2D (_MainTex, IN.uv_MainTex);
			fixed4 bump = tex2D(_BumpMap, IN.uv_MainTex);
			//B - 
			//A - 
			o.Normal = UnpackNormalDXT5nm(bump);			

			float3 WorldSpaceNormal = WorldNormalVector(IN, o.Normal);

			fixed4 specular = tex2D(_SpecTeam, IN.uv_MainTex);
			//R
			//G
			//B - 
			//A - Team
			
			float dotLightNormal = dot(SunDirection, o.Normal);


			float3 environment = texCUBE(environmentSampler, reflect(-IN.mViewVec, WorldSpaceNormal));

			float phongAmount = saturate(dot(reflect(SunDirection, WorldSpaceNormal), -IN.mViewVec));
			float3 phongAdditive = NormalMappedPhongCoeff * pow(phongAmount, 2) * specular.g;
			float3 phongMultiplicative = clamp(float3(environment * specular.r) * Luminance(_Color.rgb), 0, 2);

			o.Albedo = lerp(_Color.rgb, albedo.rgb * 0.7, 1 - specular.a);

			o.Specular = specular.g;
			o.Gloss = 0.1;
			o.Emission = specular.b * 4 * albedo.rgb + phongAdditive * 2 * _SunColor + phongMultiplicative;
			//o.Emission = environment;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
