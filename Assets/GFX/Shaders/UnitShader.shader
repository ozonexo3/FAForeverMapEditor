Shader "Custom/UnitShader" {
	Properties {
		[PerRendererData] _Color ("Army", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap ("Normal ", 2D) = "gray" {}
		_SpecTeam("SpecTeam ", 2D) = "black" {}
	}
	SubShader {
		Tags { "RenderType"="TransparentCutout" "Queue" = "Transparent+2" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf BlinnPhong vertex:vert addshadow

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
		uniform float3 SunDirection;
		uniform samplerCUBE environmentSampler;

		UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
			UNITY_DEFINE_INSTANCED_PROP(half, _Wreckage)
		UNITY_INSTANCING_BUFFER_END(Props)

		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.mViewVec = normalize(mul(unity_ObjectToWorld, v.vertex).xyz - _WorldSpaceCameraPos);

			half IsWreckage = UNITY_ACCESS_INSTANCED_PROP(Props, _Wreckage);
			if (IsWreckage > 0) {

				float SinOff = v.vertex.x + v.vertex.y * 0.5 + v.vertex.z;
				float Scale = mul(unity_ObjectToWorld, float3(0, 1, 0)).y;

				v.vertex.xyz += normalize(float3(v.vertex.x, v.vertex.y * 0.5, v.vertex.z)) * ((sin(SinOff * 6)) * 0.006 / Scale);

				//v.vertex.x += sin(v.vertex.x * 80) * 0.5;
				//v.vertex.y += sin(v.vertex.y * 6) * 0.1;
				//v.vertex.z += sin(v.vertex.z * 20) * 0.5;
			}
		}

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

			half IsWreckage = UNITY_ACCESS_INSTANCED_PROP(Props, _Wreckage);

			// Albedo comes from a texture tinted by color
			fixed4 albedo = tex2D (_MainTex, IN.uv_MainTex);
			fixed4 bump = tex2D(_BumpMap, IN.uv_MainTex);
			bump.a = 1 - bump.a;
			//B - 
			//A - 
			o.Normal = UnpackNormalDXT5nm(bump);			

			float3 WorldSpaceNormal = WorldNormalVector(IN, o.Normal);

			fixed4 specular = tex2D(_SpecTeam, IN.uv_MainTex);
			//R
			//G
			//B - 
			//A - Team

			if (IsWreckage > 0) {
				albedo.rgb *= 0.15f;
				albedo.rgb += 0.03f;
				specular.g *= 0.1f;
				specular.g += 0.05;
				specular.a = 0;

			}

			float3 AlbedoColor = lerp(UNITY_ACCESS_INSTANCED_PROP(Props, _Color).rgb * 2, albedo.rgb, 1 - specular.a);


			//Lighting
			float dotLightNormal = dot(SunDirection, o.Normal);


			float3 environment = texCUBE(environmentSampler, reflect(-IN.mViewVec, WorldSpaceNormal));

			float phongAmount = saturate(dot(reflect(SunDirection, WorldSpaceNormal), -IN.mViewVec));
			float3 phongAdditive = NormalMappedPhongCoeff * pow(phongAmount, 2) * specular.g;
			float3 phongMultiplicative = clamp(float3(environment * specular.r) * specular.g, 0, 2);


			//Setters

			o.Albedo = AlbedoColor;

			o.Specular = specular.g;
			o.Gloss = 0.1;
			o.Emission = specular.b * 4 * albedo.rgb + phongAdditive * 2 * _SunColor + phongMultiplicative;
			//o.Emission = environment;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
