Shader "MapEditor/FaTerrainShader" {
Properties {
	_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
	_Shininess ("Shininess", Range (0.03, 1)) = 0.078125
	[MaterialToggle] _Water("Has Water", Int) = 0
	[MaterialToggle] _Grid("Grid", Int) = 0
	[MaterialToggle] _Slope("Slope", Int) = 0
	
	_LightingMultiplier ("LightingMultiplier ", Range (0, 10)) = 1
	_SunColor ("Sun Color", Color) = (0.5, 0.5, 0.5, 1)
	_SunAmbience ("Ambience Color", Color) = (0.5, 0.5, 0.5, 1)
	_ShadowColor ("Shadow Color", Color) = (0.5, 0.5, 0.5, 1)
	

	_WaterRam ("Control (RGBA)", 2D) = "blue" {}
	_WaterLevel ("Water Level", Range (0.03, 5)) = 0.078125
	_AbyssLevel ("Abyss Level", Range (0.03, 5)) = 0.078125
	
	[MaterialToggle] _Area("Area", Int) = 0
	_AreaX ("Area X", Range (0, 2048)) = 0
	_AreaY ("Area Y", Range (0, 2048)) = 0
	_AreaWidht ("Area Widht", Range (0, 2048)) = 0
	_AreaHeight ("Area Height", Range (0, 2048)) = 0

	// set by terrain engine
	[HideInInspector] _Control ("Control (RGBA)", 2D) = "red" {}
	[HideInInspector] _Splat3 ("Layer 3 (A)", 2D) = "white" {}
	[HideInInspector] _Splat2 ("Layer 2 (B)", 2D) = "white" {}
	[HideInInspector] _Splat1 ("Layer 1 (G)", 2D) = "white" {}
	[HideInInspector] _Splat0 ("Layer 0 (R)", 2D) = "white" {}
	[HideInInspector] _Normal3 ("Normal 3 (A)", 2D) = "bump" {}
	[HideInInspector] _Normal2 ("Normal 2 (B)", 2D) = "bump" {}
	[HideInInspector] _Normal1 ("Normal 1 (G)", 2D) = "bump" {}
	[HideInInspector] _Normal0 ("Normal 0 (R)", 2D) = "bump" {}
	// used in fallback on old cards & base map
	[HideInInspector] _MainTex ("BaseMap (RGB)", 2D) = "white" {}
	[HideInInspector] _Color ("Main Color", Color) = (1,1,1,1)

	[MaterialToggle] _Brush ("Brush", Int) = 0
	_BrushTex ("BaseMap (RGB)", 2D) = "white" {}
	_BrushSize ("Brush Size", Range (0, 128)) = 0
	_BrushUvX ("Brush X", Range (0, 1)) = 0
	_BrushUvY ("Brush Y", Range (0, 1)) = 0

	//Lower Stratum
	_SplatLower ("Layer Lower (R)", 2D) = "white" {}
	_NormalLower ("Normal Lower (R)", 2D) = "bump" {}
	_LowerScale ("Abyss Level", Range (0.1, 3)) = 1
	
	_GridScale ("Grid Scale", Range (0, 2048)) = 512
	_GridTexture ("Grid Texture", 2D) = "white" {}
	_GridCamDist ("Grid Scale", Range (0, 10)) = 5
}
	
SubShader {
	Tags {
		"SplatCount" = "4"
		"Queue" = "Geometry-100"
		"RenderType" = "Opaque"
	}
	CGPROGRAM
	#pragma surface surf SimpleLambert vertex:vert noambient fullforwardshadows addshadow 
	#pragma target 3.0

	struct Input {
		float2 uv_Control : TEXCOORD0;
		float2 uv_Splat0 : TEXCOORD1;
		float2 uv_Splat2 : TEXCOORD3;
		float2 uv_Splat3 : TEXCOORD4;
		float3 worldPos;
		float SlopeLerp;
	};

	void vert (inout appdata_full v, out Input o){
		UNITY_INITIALIZE_OUTPUT(Input,o);
		v.tangent.xyz = cross(v.normal, float3(0,0,1));
		v.tangent.w = -1;
		o.SlopeLerp = dot(v.normal, half3(0,1,0));
		//v.color = _Abyss;
	}

	sampler2D _Control;
	sampler2D _Splat0,_Splat1,_Splat2,_Splat3, _SplatLower;
	sampler2D _Normal0,_Normal1,_Normal2,_Normal3, _NormalLower;
	sampler2D _WaterRam;
	half _Shininess;
	half _WaterLevel;
	half _AbyssLevel;
	half _LowerScale;
	fixed4 _Abyss;
	fixed4 _Deep;
	int _Water;
	int _Grid;
	int _Slope;

	half _GridScale;
	half _GridCamDist;
	sampler2D _GridTexture;

	half _LightingMultiplier;
	fixed4 _SunColor;
	fixed4 _SunAmbience;
	fixed4 _ShadowColor;

	int _Brush;
	sampler2D _BrushTex;
	half _BrushSize;
	half _BrushUvX;
	half _BrushUvY;

	half4 LightingSimpleLambert (SurfaceOutput s, half3 lightDir, half atten) {
	              half NdotL = dot (s.Normal, lightDir);
	              
	              half4 c;
	              float3 spec = float3(0,0,0);
	              
	              float3 light =  _SunColor.rgb * (NdotL) + spec;
	              light *= atten;
	              light = _LightingMultiplier * light  + _SunAmbience.rgb + _ShadowColor.rgb * (1 - light);
	              c.rgb = s.Albedo * light;
	              c.a = s.Alpha;
	              return c;
	          }

	float3 ApplyWaterColor( float depth, float3  inColor){
		float4 wcolor = tex2D(_WaterRam, float2(depth,0));
		return lerp( inColor.rgb, wcolor.rgb, wcolor.a );
		//return inColor;
	}

	void surf (Input IN, inout SurfaceOutput o) {
		fixed4 splat_control = tex2D (_Control, IN.uv_Control);
		fixed4 col;
			float WaterDepth = (_WaterLevel - IN.worldPos.y) / (_WaterLevel - _AbyssLevel);

		col = tex2D (_SplatLower, IN.uv_Splat0 * _LowerScale);
		col = lerp(col, tex2D (_Splat0, IN.uv_Splat0), splat_control.r);
		col = lerp(col, tex2D (_Splat1, IN.uv_Splat0), splat_control.g);
		col = lerp(col, tex2D (_Splat2, IN.uv_Splat2), splat_control.b);
		col = lerp(col, tex2D (_Splat3, IN.uv_Splat3), splat_control.a);
		
			half4 nrm;
		nrm = tex2D (_NormalLower, IN.uv_Splat0 * _LowerScale);
		nrm = lerp(nrm, tex2D (_Normal0, IN.uv_Splat0), splat_control.r);
		nrm =  lerp(nrm, tex2D (_Normal1, IN.uv_Splat0), splat_control.g);
		nrm =  lerp(nrm, tex2D (_Normal2, IN.uv_Splat2), splat_control.b);
		nrm =  lerp(nrm, tex2D (_Normal3, IN.uv_Splat3), splat_control.a);
		
		
		nrm.rgb = normalize(nrm.rgb);
		//nrm = half4(nrm.r, nrm.g, nrm.b, nrm.a);
		//fixed4 finalnormals;
		//finalnormals = float4(nrm.rgb * 0.5 + 0.5, nrm.a);
		
		o.Normal = UnpackNormal(nrm);
		
		if(_Slope > 0){
			if(IN.worldPos.y < _WaterLevel){
				if(IN.SlopeLerp > 0.75) col.rgb = half3(0,0.4,1);
				else col.rgb = half3(0.6,0,1);
			}
			else if(IN.SlopeLerp > 0.99) col.rgb = half3(0,0.8,0);
			else if(IN.SlopeLerp > 0.85) col.rgb = half3(0.5,1,0);
			else col.rgb = half3(1,0,0);
			//col.rgb = lerp(half3(1,0,0), half3(0,1,0), IN.SlopeLerp);
		}
		
		if(_Grid > 0){
			fixed4 GridColor = tex2D (_GridTexture, IN.uv_Control * _GridScale - float2(-0.00, -0.00));
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
			o.Emission = GridFinal * GridColor.a;;
		}

		if(_Brush > 0){
			fixed4 BrushColor = tex2D (_BrushTex, ((IN.uv_Control - float2(_BrushUvX, _BrushUvY)) * _GridScale) / (_BrushSize * _GridScale * 0.002)  );
			o.Emission += half3(0, BrushColor.r * 0.3, BrushColor.r * 0.7);
		}

		if(_Water > 0) o.Albedo = ApplyWaterColor(WaterDepth, col.rgb);	
		else o.Albedo = col;	
		

		o.Gloss = 0;
		o.Specular = 0;

		o.Alpha = 0.0;
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
				else if(IN.worldPos.z < _AreaY - 51.2){
				 	o.Emission = fixed4(0,0,0,1);
				}
				else if(IN.worldPos.z > _AreaHeight - 51.2){
				 	o.Emission = fixed4(0,0,0,1);
				}
			}
		}
        ENDCG
}


FallBack "Diffuse"
}
