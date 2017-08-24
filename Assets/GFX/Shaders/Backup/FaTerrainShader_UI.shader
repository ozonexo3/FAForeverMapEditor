// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MapEditor/FaTerrainShader (backup)" {
Properties {
	_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
	_Shininess ("Shininess", Range (0.03, 1)) = 0.078125
	[MaterialToggle] _Water("Has Water", Int) = 0
	[MaterialToggle] _Grid("Grid", Int) = 0
	[MaterialToggle] _Slope("Slope", Int) = 0
	[MaterialToggle] _UseSlopeTex("Use Slope Data", Int) = 0
	_SlopeTex ("Slope data", 2D) = "black" {}
	[MaterialToggle] _TTerrainXP("_TTerrainXP", Int) = 0
	
	//_LightingMultiplier ("LightingMultiplier ", Range (0, 10)) = 1
	//_SunColor ("Sun Color", Color) = (0.5, 0.5, 0.5, 1)
	//_SunAmbience ("Ambience Color", Color) = (0.5, 0.5, 0.5, 1)
	//_ShadowColor ("Shadow Color", Color) = (0.5, 0.5, 0.5, 1)
	

	//_WaterRam ("Water Ramp (RGBA)", 2D) = "blue" {}
	//_UtilitySamplerC ("_UtilitySamplerC", 2D) = "white" {}
	_WaterLevel ("Water Level", float) = 0.078125
	_DepthLevel ("Depth Level", float) = 0.078125
	_AbyssLevel ("Abyss Level", float) = 0.078125
	
	[MaterialToggle] _Area("Area", Int) = 0
	_AreaX ("Area X", Range (0, 2048)) = 0
	_AreaY ("Area Y", Range (0, 2048)) = 0
	_AreaWidht ("Area Widht", Range (0, 2048)) = 0
	_AreaHeight ("Area Height", Range (0, 2048)) = 0

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

	_Splat0XP ("Layer 1 (R)", 2D) = "black" {}
	_Splat1XP ("Layer 2 (G)", 2D) = "black" {}
	_Splat2XP ("Layer 3 (B)", 2D) = "black" {}
	_Splat3XP ("Layer 4 (A)", 2D) = "black" {}
	_Splat4XP ("Layer 5 (R)", 2D) = "black" {}
	_Splat5XP ("Layer 6 (G)", 2D) = "black" {}
	_Splat6XP ("Layer 7 (B)", 2D) = "black" {}
	_Splat7XP ("Layer 8 (A)", 2D) = "black" {}

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
	_SplatNormal0 ("Normal 1 (A)", 2D) = "bump" {}
	_SplatNormal1 ("Normal 2 (B)", 2D) = "bump" {}
	_SplatNormal2 ("Normal 3 (G)", 2D) = "bump" {}
	_SplatNormal3 ("Normal 4 (R)", 2D) = "bump" {}
	_SplatNormal4 ("Normal 5 (A)", 2D) = "bump" {}
	_SplatNormal5 ("Normal 6 (B)", 2D) = "bump" {}
	_SplatNormal6 ("Normal 7 (G)", 2D) = "bump" {}
	_SplatNormal7 ("Normal 8 (R)", 2D) = "bump" {}

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
	_GridCamDist ("Grid Scale", Range (0, 10)) = 5
	_WaterScaleX ("Water Scale X", float) = 1024
		_WaterScaleZ ("Water Scale Z", float) = 1024
}
	
	SubShader {

			Blend One One
			CGPROGRAM
			#pragma surface surf Lambert nofog
			
				struct Input {
				float2 uv_Control : TEXCOORD0;
					float3 worldPos;
				};
				
			int _Slope, _UseSlopeTex;
			sampler2D _SlopeTex;
			half _GridScale;
			
				void surf (Input IN, inout SurfaceOutput o) {
					o.Albedo = fixed4(0,0,0,1);
					//o.Emission = fixed4(1,1,1,1);
					if(_Slope > 0 && _UseSlopeTex > 0){
						float2 UV = IN.uv_Control * fixed2(1, 1) + half2(0, 0) - float2(-0.05, -0.05) / _GridScale;

						float4 splat_control = tex2D (_SlopeTex, UV);
						o.Emission = splat_control.rgb;
					}
				}
		        ENDCG

			Blend One One
			CGPROGRAM
			#pragma surface surf Lambert nofog
			
				struct Input {
					float2 uv_Control : TEXCOORD0;
					float3 worldPos;
					float SlopeLerp;
				};

				void vert (inout appdata_full v, out Input o){
					UNITY_INITIALIZE_OUTPUT(Input,o);
					//v.tangent.xyz = cross(v.normal, float3(0,0,1));
					//v.tangent.w = -1;
					o.SlopeLerp = dot(v.normal, half3(0,1,0));
				}


				half _GridScale;
				half _GridCamDist;
				sampler2D _GridTexture;
				int _Grid;
				int _Slope;
				half _WaterLevel;

				void surf (Input IN, inout SurfaceOutput o) {
					o.Albedo = fixed4(0,0,0,1);
					o.Emission = fixed4(0,0,0,0);

					half4 col = half4(0,0,0,1);

					
					if(_Grid > 0){
						fixed4 GridColor = tex2D (_GridTexture, IN.uv_Control * _GridScale - float2(-0.05, -0.05));
						fixed4 GridFinal = fixed4(0,0,0,GridColor.a);
						if(_GridCamDist < 1){
							GridFinal.rgb = lerp(GridFinal.rgb, fixed3(1,1,1), GridColor.r * lerp(1, 0, _GridCamDist));
							GridFinal.rgb = lerp(GridFinal.rgb, fixed3(0,1,0), GridColor.g * lerp(1, 0, _GridCamDist));
							GridFinal.rgb = lerp(GridFinal.rgb, fixed3(0,1,0), GridColor.b * lerp(0, 1, _GridCamDist));
						}
						else{
						GridFinal.rgb = lerp(GridFinal.rgb, fixed3(0,1,0), GridColor.b);
						}
						  
						col.rgb = lerp(col.rgb, GridFinal.rgb, GridColor.a);
						o.Emission = GridFinal * GridColor.a;
					}
					o.Emission = col.rgb;
				}
		        ENDCG

			Blend One One
			CGPROGRAM
			#pragma surface surf Lambert nofog
			
				struct Input {
					float2 uv_Control : TEXCOORD0;
				};


				int _Brush;
				sampler2D _BrushTex;
				half _BrushSize;
				half _BrushUvX;
				half _BrushUvY;
				half _GridScale;

				void surf (Input IN, inout SurfaceOutput o) {
					o.Albedo = fixed4(0,0,0,1);
					o.Emission = fixed4(0,0,0,0);


					if(_Brush > 0){	
						fixed4 BrushColor = tex2D (_BrushTex, ((IN.uv_Control - float2(_BrushUvX, _BrushUvY)) * _GridScale) / (_BrushSize * _GridScale * 0.002)  );

						half LerpValue = clamp(_BrushSize / 20, 0, 1);

						half From = 0.1f;
						half To = lerp(0.2f, 0.13f, LerpValue);
						half Range = lerp(0.015f, 0.008f, LerpValue);

						if(BrushColor.r >= From && BrushColor.r <= To){
							half AA = 1;

							if (BrushColor.r < From + Range)
								AA = (BrushColor.r - From) / Range;
							else if(BrushColor.r > To - Range)
								AA = 1 - (BrushColor.r - (To - Range)) / Range;

							AA = clamp(AA, 0, 1);

							o.Emission += half3(0, 0.3, 1) * (AA * 0.8);
						}

						o.Emission += half3(0, BrushColor.r * 0.1, BrushColor.r * 0.2);
					}
				}
		        ENDCG
			
			Blend DstColor Zero
			CGPROGRAM
			#pragma surface surf Lambert nofog
			
				struct Input {
					float3 worldPos;
				};
				
					int _Area;

			half _AreaX;
			half _AreaY;
			half _AreaWidht;
			half _AreaHeight;
			half _GridScale;
			
				void surf (Input IN, inout SurfaceOutput o) {
					o.Albedo = fixed4(0,0,0,1);
					o.Emission = fixed4(1,1,1,1);
					if(_Area > 0){
						fixed4 dark = fixed4(0.05, 0.05, 0.05, 1); 
						if(IN.worldPos.x < _AreaX){
						 	o.Emission = fixed4(0,0,0,1);
						}
						else if(IN.worldPos.x > _AreaWidht){
						 	o.Emission = fixed4(0,0,0,1);
						}
						else if(IN.worldPos.z < _AreaY - _GridScale){
						 	o.Emission = fixed4(0,0,0,1);
						}
						else if(IN.worldPos.z > _AreaHeight - _GridScale){
						 	o.Emission = fixed4(0,0,0,1);
						}
					}
				}
		        ENDCG

}


FallBack "Diffuse"
}
