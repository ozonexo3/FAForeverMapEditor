// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MapEditor/FaTerrainShader" {
Properties {
	_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
	_Shininess ("Shininess", Range (0.03, 1)) = 0.078125
	[MaterialToggle] _Grid("Grid", Int) = 0
	[MaterialToggle] _BuildGrid("Grid", Int) = 0
	[MaterialToggle] _Slope("Slope", Int) = 0
	[MaterialToggle] _UseSlopeTex("Use Slope Data", Int) = 0
	_SlopeTex ("Slope data", 2D) = "black" {}
		
	// set by terrain engine
	_Control ("Control (RGBA)", 2D) = "black" {}
	_ControlXP ("ControlXP (RGBA)", 2D) = "black" {}
	_Control2XP ("Control2XP (RGBA)", 2D) = "black" {}

	
	[MaterialToggle] _HideSplat0("Hide splat 2", Int) = 0
	[MaterialToggle] _HideSplat1("Hide splat 2", Int) = 0
	[MaterialToggle] _HideSplat2("Hide splat 3", Int) = 0
	[MaterialToggle] _HideSplat3("Hide splat 4", Int) = 0
	[MaterialToggle] _HideSplat4("Hide splat 5", Int) = 0
	[MaterialToggle] _HideSplat5("Hide splat 6", Int) = 0
	[MaterialToggle] _HideSplat6("Hide splat 7", Int) = 0
	[MaterialToggle] _HideSplat7("Hide splat 8", Int) = 0
	[MaterialToggle] _HideSplat8("Hide splat Upper", Int) = 0
	[MaterialToggle] _HideTerrainType("Hide Terrain Type", Int) = 0

	_SplatAlbedoArray ("Albedo array", 2DArray) = "" {}
	_SplatNormalArray ("Normal array", 2DArray) = "" {}


	_Splat0Scale ("Splat1 Level", Range (1, 1024)) = 10
	_Splat1Scale ("Splat2 Level", Range (1, 1024)) = 10
	_Splat2Scale ("Splat3 Level", Range (1, 1024)) = 10
	_Splat3Scale ("Splat4 Level", Range (1, 1024)) = 10
	_Splat4Scale ("Splat5 Level", Range (1, 1024)) = 10
	_Splat5Scale ("Splat6 Level", Range (1, 1024)) = 10
	_Splat6Scale ("Splat7 Level", Range (1, 1024)) = 10
	_Splat7Scale ("Splat8 Level", Range (1, 1024)) = 10

	// set by terrain engine
	[MaterialToggle] _GeneratingNormal("Generating Normal", Int) = 0
	_TerrainNormal ("Terrain Normal", 2D) = "bump" {}
	

	_Splat0ScaleNormal ("Splat1 Normal Level", Range (1, 1024)) = 10
	_Splat1ScaleNormal ("Splat2 Normal Level", Range (1, 1024)) = 10
	_Splat2ScaleNormal ("Splat3 Normal Level", Range (1, 1024)) = 10
	_Splat3ScaleNormal ("Splat4 Normal Level", Range (1, 1024)) = 10
	_Splat4ScaleNormal ("Splat5 Normal Level", Range (1, 1024)) = 10
	_Splat5ScaleNormal ("Splat6 Normal Level", Range (1, 1024)) = 10
	_Splat6ScaleNormal ("Splat7 Normal Level", Range (1, 1024)) = 10
	_Splat7ScaleNormal ("Splat8 Normal Level", Range (1, 1024)) = 10

	// used in fallback on old cards & base map
	[HideInInspector] _MainTex ("BaseMap (RGB)", 2D) = "white" {}
	[HideInInspector] _Color ("Main Color", Color) = (1,1,1,1)

	[MaterialToggle] _Brush ("Brush", Int) = 0
	[MaterialToggle] _BrushPainting ("Brush painting", Int) = 0
	_BrushTex ("Brush (RGB)", 2D) = "white" {}
	_BrushSize ("Brush Size", Range (0, 128)) = 0
	_BrushUvX ("Brush X", Range (0, 1)) = 0
	_BrushUvY ("Brush Y", Range (0, 1)) = 0

	//Lower Stratum
	_SplatLower ("Layer Lower (R)", 2D) = "white" {}
	_NormalLower ("Normal Lower (R)", 2D) = "bump" {}
	_LowerScale ("Lower Level", Range (1, 128)) = 1
	_LowerScaleNormal ("Lower Normal Level", Range (1, 128)) = 1

	//Upper Stratum
	_SplatUpper ("Layer Lower (R)", 2D) = "white" {}
	_NormalUpper ("Normal Lower (R)", 2D) = "bump" {}
	_UpperScale ("Upper Level", Range (1, 128)) = 1
	_UpperScaleNormal ("Upper Normal Level", Range (1, 128)) = 1

	_GridScale ("Grid Scale", Range (0, 2048)) = 512
	_GridTexture ("Grid Texture", 2D) = "white" {}
	_GridBuildTexture("Grid build Texture", 2D) = "white" {}
	
	_TerrainTypeAlbedo ("Terrain Type Albedo", 2D) = "black" {}
	_TerrainTypeCapacity ("Terrain Type Capacity", Range(0,1)) = 0.228
}
	
	SubShader {

			CGPROGRAM
			#define UNITY_BRDF_PBS BRDF3_Unity_PBS

			#pragma surface surf SimpleLambert vertex:vert  fullforwardshadows addshadow nometa
#pragma multi_compile_fog
			//#pragma debug
			#pragma target 3.5
			#pragma exclude_renderers gles
			#include "UnityLightingCommon.cginc"
			#include "UnityGBuffer.cginc"
			#include "UnityGlobalIllumination.cginc"
			#include "Assets/GFX/Shaders/SimpleLambert.cginc"
			#pragma multi_compile PREVIEW_OFF PREVIEW_ON

			struct Input {
				float2 uv_Control : TEXCOORD0;
				float3 worldPos;
				float SlopeLerp;
				float4 grabUV;
				half fog;
			};

			half _GeneratingNormal;
			uniform half4 unity_FogStart;
			uniform half4 unity_FogEnd;


			void vert (inout appdata_full v, out Input o){
				UNITY_INITIALIZE_OUTPUT(Input,o);

				o.SlopeLerp = dot(v.normal, half3(0,1,0));

				if(_GeneratingNormal == 0)
					v.normal = float3(0,1,0);
				v.tangent.xyz = cross(v.normal, float3(0,0,1));
				v.tangent.w = -1;
				//o.normal = v.normal;
				 float4 hpos = UnityObjectToClipPos (v.vertex);
		         o.grabUV = ComputeGrabScreenPos(hpos);
				//v.color = _Abyss;

				 float pos = length(UnityObjectToViewPos(v.vertex).xyz);
				 float diff = unity_FogEnd.x - unity_FogStart.x;
				 float invDiff = 1.0f / diff;
				 o.fog = saturate((unity_FogEnd.x - pos) * invDiff);
			}

			sampler2D _MyGrabTexture3;
			sampler2D _WaterRam;
			half _Shininess;
			fixed4 _Abyss;
			fixed4 _Deep;
			int _Water;
			float _WaterLevel;

			int _Slope, _UseSlopeTex;
			sampler2D _SlopeTex;

			int _Grid, _BuildGrid;
			half _GridScale;
			half _GridCamDist;
			sampler2D _GridTexture, _GridBuildTexture;

			//uniform
			sampler2D _ControlXP;
			sampler2D _Control2XP;
			uniform sampler2D _UtilitySamplerC;
			sampler2D _TerrainNormal;
			sampler2D _SplatLower, _SplatUpper;
			sampler2D _TerrainTypeAlbedo;
			sampler2D  _NormalLower;
			half _Splat0Scale, _Splat1Scale, _Splat2Scale, _Splat3Scale, _Splat4Scale, _Splat5Scale, _Splat6Scale, _Splat7Scale;
			half _Splat0ScaleNormal, _Splat1ScaleNormal, _Splat2ScaleNormal, _Splat3ScaleNormal, _Splat4ScaleNormal, _Splat5ScaleNormal, _Splat6ScaleNormal, _Splat7ScaleNormal;

			UNITY_DECLARE_TEX2DARRAY(_SplatAlbedoArray);
			 UNITY_DECLARE_TEX2DARRAY(_SplatNormalArray);

			int _HideSplat0, _HideSplat1, _HideSplat2, _HideSplat3, _HideSplat4, _HideSplat5, _HideSplat6, _HideSplat7, _HideSplat8;
			int _HideTerrainType;
			float _TerrainTypeCapacity;
		
			half _LowerScale, _UpperScale;
			half _LowerScaleNormal, _UpperScaleNormal;
			uniform int _TTerrainXP;
			uniform float _WaterScaleX, _WaterScaleZ;

			int _Brush, _BrushPainting;
			sampler2D _BrushTex;
			half _BrushSize;
			half _BrushUvX;
			half _BrushUvY;

			uniform int _Area;
			uniform half4 _AreaRect;



			float3 ApplyWaterColor( float depth, float3  inColor){
				float4 wcolor = tex2D(_WaterRam, float2(depth,0));
				return lerp( inColor.rgb, wcolor.rgb, wcolor.a );
			}

			float4 RenderGrid(sampler2D _GridTex, float2 uv_Control) {
				//float2 GridUv = IN.uv_Control * _GridScale;
				fixed4 GridColor = tex2D(_GridTex, uv_Control * _GridScale);
				fixed4 GridFinal = fixed4(0, 0, 0, GridColor.a);
				if (_GridCamDist < 1) {
					GridFinal.rgb = lerp(GridFinal.rgb, fixed3(1, 1, 1), GridColor.r * lerp(1, 0, _GridCamDist));
					GridFinal.rgb = lerp(GridFinal.rgb, fixed3(0, 1, 0), GridColor.g * lerp(1, 0, _GridCamDist));
					GridFinal.rgb = lerp(GridFinal.rgb, fixed3(0, 1, 0), GridColor.b * lerp(0, 1, _GridCamDist));
				}
				else {
					GridFinal.rgb = lerp(GridFinal.rgb, fixed3(0, 1, 0), GridColor.b);
				}

				GridFinal *= GridColor.a;

				half CenterGridSize = lerp(0.005, 0.015, _GridCamDist) / _GridScale;
				if (uv_Control.x > 0.5 - CenterGridSize && uv_Control.x < 0.5 + CenterGridSize)
					GridFinal.rgb = fixed3(0.4, 1, 0);
				else if (uv_Control.y > 0.5 - CenterGridSize && uv_Control.y < 0.5 + CenterGridSize)
					GridFinal.rgb = fixed3(0.4, 1, 0);

				//col.rgb = lerp(col.rgb, GridFinal.rgb, GridColor.a);
				return GridFinal;
			}

			inline fixed3 UnpackNormalDXT5nmScaled (fixed4 packednormal, fixed scale)
			{
				fixed3 normal = 0;
				normal.xz = packednormal.wx * 2 - 1;
				normal.y = sqrt(1 - saturate(dot(normal.xz, normal.xz)));
				normal.xz *= scale;

			   return normal.xzy;
			}

			#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)
			#define UNITY_SAMPLE_TEX2DARRAY_GRAD(tex,coord,dx,dy) tex.SampleGrad (sampler##tex,coord,dx,dy)
			#else
			#if defined(UNITY_COMPILER_HLSL2GLSL) || defined(SHADER_TARGET_SURFACE_ANALYSIS)
			#define UNITY_SAMPLE_TEX2DARRAY_GRAD(tex,coord,dx,dy) float4(0,0,0,0) //tex2DArray(tex,coord,dx,dy)
			#endif
			#endif

			void surf (Input IN, inout SurfaceOutput o) {

				float4 waterTexture = tex2D( _UtilitySamplerC, IN.uv_Control * float2(1, -1) + float2(1 / (_WaterScaleX * 1), 1 / (_WaterScaleZ * 1) + 1));

				float WaterDepth = waterTexture.g;

				float2 UVCtrl = IN.uv_Control * fixed2(1, -1) + half2(0, 1);
				float2 UV = IN.uv_Control * fixed2(1, -1) + half2(0, 1);
				float4 splat_control = saturate(tex2D (_ControlXP, UVCtrl) * 2 - 1);
				float4 splat_control2 = saturate(tex2D (_Control2XP, UVCtrl) * 2 - 1);


				float4 col = tex2D(_SplatLower, UV * _LowerScale);

				if(_HideSplat0 == 0)
					col = lerp(col, UNITY_SAMPLE_TEX2DARRAY(_SplatAlbedoArray, float3(UV * _Splat0Scale, 0)), splat_control.r);
				if(_HideSplat1 == 0)
					col = lerp(col, UNITY_SAMPLE_TEX2DARRAY(_SplatAlbedoArray, float3(UV * _Splat1Scale, 1)), splat_control.g);
				if(_HideSplat2 == 0)
					col = lerp(col, UNITY_SAMPLE_TEX2DARRAY(_SplatAlbedoArray, float3(UV * _Splat2Scale, 2)), splat_control.b);
				if(_HideSplat3 == 0)
					col = lerp(col, UNITY_SAMPLE_TEX2DARRAY(_SplatAlbedoArray, float3(UV * _Splat3Scale, 3)), splat_control.a);

				if (_TTerrainXP > 0) {
					if(_HideSplat4 == 0)
					col = lerp(col, UNITY_SAMPLE_TEX2DARRAY(_SplatAlbedoArray, float3(UV * _Splat4Scale, 4)), splat_control2.r);
					if(_HideSplat5 == 0)
					col = lerp(col, UNITY_SAMPLE_TEX2DARRAY(_SplatAlbedoArray, float3(UV * _Splat5Scale, 5)), splat_control2.g);
					if(_HideSplat6 == 0)
					col = lerp(col, UNITY_SAMPLE_TEX2DARRAY(_SplatAlbedoArray, float3(UV * _Splat6Scale, 6)), splat_control2.b);
					if(_HideSplat7 == 0)
					col = lerp(col, UNITY_SAMPLE_TEX2DARRAY(_SplatAlbedoArray, float3(UV * _Splat7Scale, 7)), splat_control2.a);
				}

				if(_HideSplat8 == 0){
					float4 UpperAlbedo = tex2D (_SplatUpper, UV * _UpperScale);
					col = lerp(col, UpperAlbedo, UpperAlbedo.a);
				}
				if(_HideTerrainType == 0) {
					float4 TerrainTypeAlbedo = tex2D (_TerrainTypeAlbedo, UV);
					col = lerp(col, TerrainTypeAlbedo, TerrainTypeAlbedo.a*_TerrainTypeCapacity);
				}

				half4 nrm;
				//UV *= 0.01;
				nrm = tex2D (_NormalLower, UV * _LowerScaleNormal);
				if(_HideSplat0 == 0)
				//nrm = lerp(nrm, tex2D (_SplatNormal0, UV * _Splat0ScaleNormal), splat_control.r);
				nrm = lerp(nrm, UNITY_SAMPLE_TEX2DARRAY(_SplatNormalArray, float3(UV * _Splat0ScaleNormal, 0)), splat_control.r);
				if(_HideSplat1 == 0)
				//nrm =  lerp(nrm, tex2D (_SplatNormal1, UV * _Splat1ScaleNormal), splat_control.g);
				nrm = lerp(nrm, UNITY_SAMPLE_TEX2DARRAY(_SplatNormalArray, float3(UV * _Splat1ScaleNormal, 1)), splat_control.g);
				if(_HideSplat2 == 0)
				//nrm =  lerp(nrm, tex2D (_SplatNormal2, UV * _Splat2ScaleNormal), splat_control.b);
				nrm = lerp(nrm, UNITY_SAMPLE_TEX2DARRAY(_SplatNormalArray, float3(UV * _Splat2ScaleNormal, 2)), splat_control.b);
				if(_HideSplat3 == 0)
				//nrm =  lerp(nrm, tex2D (_SplatNormal3, UV * _Splat3ScaleNormal), splat_control.a);
				nrm = lerp(nrm, UNITY_SAMPLE_TEX2DARRAY(_SplatNormalArray, float3(UV * _Splat3ScaleNormal, 3)), splat_control.a);

				if (_TTerrainXP > 0) {
					if(_HideSplat4 == 0)
					//nrm = lerp(nrm, tex2D(_SplatNormal4, UV * _Splat4ScaleNormal), splat_control2.r);
					nrm = lerp(nrm, UNITY_SAMPLE_TEX2DARRAY(_SplatNormalArray, float3(UV * _Splat4ScaleNormal, 4)), splat_control2.r);
					if(_HideSplat5 == 0)
					//nrm = lerp(nrm, tex2D(_SplatNormal5, UV * _Splat5ScaleNormal), splat_control2.g);
					nrm = lerp(nrm, UNITY_SAMPLE_TEX2DARRAY(_SplatNormalArray, float3(UV * _Splat5ScaleNormal, 5)), splat_control2.g);
					if(_HideSplat6 == 0)
					//nrm = lerp(nrm, tex2D(_SplatNormal6, UV * _Splat6ScaleNormal), splat_control2.b);
					nrm = lerp(nrm, UNITY_SAMPLE_TEX2DARRAY(_SplatNormalArray, float3(UV * _Splat6ScaleNormal, 6)), splat_control2.b);
					if(_HideSplat7 == 0)
					//nrm = lerp(nrm, tex2D(_SplatNormal7, UV * _Splat7ScaleNormal), splat_control2.a);
					nrm = lerp(nrm, UNITY_SAMPLE_TEX2DARRAY(_SplatNormalArray, float3(UV * _Splat7ScaleNormal, 7)), splat_control2.a);
				}



				//nrm = tex2D (_NormalLower, UV * 1000);
				//nrm.rg *= 5;
				//nrm.b = 1;
				//nrm.rgb = UnpackNormal(nrm);
				//nrm.rgb = nrm.rgb * 2 - half3(1, 1, 1);
				//nrm.rg *= 3;
				//nrm.rgb = normalize(nrm.rgb);
				//o.Normal = UnpackNormalDXT5nm(tex2D(_TerrainNormal, UV ));
				//o.Normal = (UnpackNormalDXT5nm(tex2D(_TerrainNormal, UV )) + UnpackNormalDXT5nmScaled(nrm.rgbg, 2));
				if(_GeneratingNormal == 0){
					half4 TerrainNormal = tex2D(_TerrainNormal, UVCtrl );
					half3 TerrainNormalVector = UnpackNormalDXT5nm( half4(TerrainNormal.r, 1 - TerrainNormal.g, TerrainNormal.b, TerrainNormal.a));
					IN.SlopeLerp = dot(TerrainNormalVector, half3(0,0,1));
					//o.Albedo = IN.SlopeLerp;
					o.Normal = BlendNormals(TerrainNormalVector , UnpackNormalDXT5nmScaled(nrm.rgbg, 1));

				}
				else
					o.Normal = UnpackNormalDXT5nmScaled(nrm.rgbg, 1);


				//				o.Normal.y *= 0.5f;
				//o.Normal = normalize(o.Normal);

				//half3 Emit = _SunAmbience.rgb * 2;
				half3 Emit = 0;

#if defined(PREVIEW_ON)
				if (_Water > 0) col.rgb = ApplyWaterColor(WaterDepth, col.rgb);
#else

				if (_Slope > 0) {
					o.Normal = half3(0, 0, 1);
					half3 SlopeColor = 0;
					if (_UseSlopeTex > 0) {
						//col = 0;
						float2 UV2 = IN.uv_Control * fixed2(1, 1) + half2(0, 0) - float2(-0.00, -0.00) / _GridScale;
						float4 splat_control = tex2D(_SlopeTex, UV2);
						SlopeColor = splat_control.rgb;

					}
					else {

						if (IN.worldPos.y < _WaterLevel) {
							if (IN.SlopeLerp > 0.75) SlopeColor = half3(0, 0.4, 1);
							else SlopeColor = half3(0.6, 0, 1);
						}
						else if (IN.SlopeLerp > 0.999) SlopeColor = half3(0, 0.8, 0);
						else if (IN.SlopeLerp > 0.95) SlopeColor = half3(0.3, 0.89, 0);
						else if (IN.SlopeLerp > 0.80) SlopeColor = half3(0.5, 0.8, 0);
						else SlopeColor = half3(1, 0, 0);

					}
					Emit = SlopeColor * 0.5;
					col.rgb = lerp(col.rgb, SlopeColor, 0.5);
				}
				else if (_Water > 0) col.rgb = ApplyWaterColor(WaterDepth, col.rgb);


				if (_Grid > 0) {
					if(_BuildGrid)
						Emit += RenderGrid(_GridBuildTexture, IN.uv_Control);
					else
						Emit += RenderGrid(_GridTexture, IN.uv_Control);
				}

				if (_Brush > 0) {
					float2 BrushUv = ((IN.uv_Control - float2(_BrushUvX, _BrushUvY)) * _GridScale) / (_BrushSize * _GridScale * 0.002);
					fixed4 BrushColor = tex2D(_BrushTex, BrushUv);

					if (BrushUv.x >= 0 && BrushUv.y >= 0 && BrushUv.x <= 1 && BrushUv.y <= 1) {

						half LerpValue = clamp(_BrushSize / 20, 0, 1);

						half From = 0.1f;
						half To = lerp(0.2f, 0.13f, LerpValue);
						half Range = lerp(0.015f, 0.008f, LerpValue);

						if (BrushColor.r >= From && BrushColor.r <= To) {
							half AA = 1;

							if (BrushColor.r < From + Range)
								AA = (BrushColor.r - From) / Range;
							else if (BrushColor.r > To - Range)
								AA = 1 - (BrushColor.r - (To - Range)) / Range;

							AA = clamp(AA, 0, 1);

							Emit += half3(0, 0.3, 1) * (AA * 0.8);
						}

						if (_BrushPainting <= 0)
							Emit += half3(0, BrushColor.r * 0.1, BrushColor.r * 0.2);
						else
							Emit += half3(0, BrushColor.r * 0.1, BrushColor.r * 0.2) * 0.2;
					}
				}
#endif

				//FOG
				col.rgb = lerp(0, col.rgb, IN.fog);
				Emit = lerp(unity_FogColor, Emit, IN.fog);
				//emission.rgb = lerp(emission.rgb, exp2(-unity_FogColor), saturate(IN.fog));


				o.Albedo = col;
				o.Emission = Emit;

				if(_Area > 0){
					fixed3 BlackEmit = -1;
					fixed3 Albedo = 0;
					if(IN.worldPos.x < _AreaRect.x){
						o.Emission = BlackEmit;
						o.Albedo = Albedo;
					}
					else if(IN.worldPos.x > _AreaRect.z){
						o.Emission = BlackEmit;
						o.Albedo = Albedo;
					}
					else if(IN.worldPos.z < _AreaRect.y - _GridScale){
						o.Emission = BlackEmit;
						o.Albedo = Albedo;
					}
					else if(IN.worldPos.z > _AreaRect.w - _GridScale){
						o.Emission = BlackEmit;
						o.Albedo = Albedo;
					}
				}

				//o.Albedo = 0.5;
				//o.Emission = tex2D (_SplatNormal3, UV * 10 );
				//o.Emission = nrm;


				if (_TTerrainXP > 0) {
					o.Gloss = (1 - col.a);
					o.Specular = col.a;
				}
				else {
					o.Gloss = (1 - col.a) + 0.01;
					o.Specular = col.a + 0.01;
				}



				o.Alpha = col.a;
			}
			ENDCG  


}


FallBack "Diffuse"
}
