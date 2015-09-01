Shader "MapEditor/FaTerrainShader2" {
	Properties {
	_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
	_Shininess ("Shininess", Range (0.03, 1)) = 0.078125
	[MaterialToggle] _Water("Has Water", Int) = 0
	[MaterialToggle] _Grid("Grid", Int) = 0
	[MaterialToggle] _Slope("Slope", Int) = 0
	
	_ShadowColor ("Shadow Color", Color) = (0.5, 0.5, 0.5, 1)
	

	_WaterRam ("Control (RGBA)", 2D) = "blue" {}
	_WaterLevel ("Water Level", Range (0.03, 5)) = 0.078125
	_AbyssLevel ("Abyss Level", Range (0.03, 5)) = 0.078125
	_UtilitySamplerC ("_UtilitySamplerC", 2D) = "white" {}
	
	[MaterialToggle] _Area("Area", Int) = 0
	_AreaX ("Area X", Range (0, 2048)) = 0
	_AreaY ("Area Y", Range (0, 2048)) = 0
	_AreaWidht ("Area Widht", Range (0, 2048)) = 0
	_AreaHeight ("Area Height", Range (0, 2048)) = 0

	// set by terrain engine
	_Control ("Control (RGBA)", 2D) = "red" {}
	[HideInInspector] _Splat3 ("Layer 3 (A)", 2D) = "white" {}
	_Splat3Scale ("_Splat0Scale", Range (0, 100)) = 1
	[HideInInspector] _Splat2 ("Layer 2 (B)", 2D) = "white" {}
	_Splat2Scale ("_Splat0Scale", Range (0, 100)) = 1
	[HideInInspector] _Splat1 ("Layer 1 (G)", 2D) = "white" {}
	_Splat1Scale ("_Splat0Scale", Range (0, 100)) = 1
	[HideInInspector] _Splat0 ("Layer 0 (R)", 2D) = "white" {}
	_Splat0Scale ("_Splat0Scale", Range (0, 100)) = 1
	[HideInInspector] _Normal3 ("Normal 3 (A)", 2D) = "bump" {}
	[HideInInspector] _Normal2 ("Normal 2 (B)", 2D) = "bump" {}
	[HideInInspector] _Normal1 ("Normal 1 (G)", 2D) = "bump" {}
	[HideInInspector] _Normal0 ("Normal 0 (R)", 2D) = "bump" {}
	// used in fallback on old cards & base map
	[HideInInspector] _MainTex ("BaseMap (RGB)", 2D) = "white" {}
	[HideInInspector] _Color ("Main Color", Color) = (1,1,1,1)
	
	//Lower Stratum
	_SplatLower ("Layer Lower (R)", 2D) = "white" {}
	_NormalLower ("Normal Lower (R)", 2D) = "bump" {}
	_LowerScale ("Abyss Level", Range (0.1, 3)) = 1
	
	_GridScale ("Grid Scale", Range (0, 2048)) = 512
	_GridTexture ("Grid Texture", 2D) = "white" {}
	_GridCamDist ("Grid Scale", Range (0, 10)) = 5
	}
    SubShader {
        Pass {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		    #pragma vertex vert
		    #pragma fragment frag
		    #pragma target 3.0

		    #include "UnityCG.cginc"
		    
		    
sampler2D _Control;
sampler2D _Splat0,_Splat1,_Splat2,_Splat3, _SplatLower;
sampler2D _Normal0,_Normal1,_Normal2,_Normal3, _NormalLower;
sampler2D _WaterRam;
uniform sampler2D _UtilitySamplerC;
float _Splat0Scale, _Splat1Scale, _Splat2Scale, _Splat3Scale;
half _Shininess;
half _WaterLevel;
half _AbyssLevel;
half _LowerScale;
fixed4 _Abyss;
fixed4 _Deep;
int _Water;
int _Grid;
int _Slope;
int _Area;

half _AreaX;
half _AreaY;
half _AreaWidht;
half _AreaHeight;

half _GridScale;
half _GridCamDist;
sampler2D _GridTexture;

		   	float3 ApplyWaterColor( float depth, float3  inColor){
				float4 wcolor = tex2D(_WaterRam, float2(depth,0));
				return lerp( inColor.rgb, wcolor.rgb, wcolor.a );
			}

		    struct vertexInput {
		        float4 vertex : POSITION;
		        float4 texcoord0 : TEXCOORD0;
	          	float3 normal : NORMAL;
		    };

		    struct fragmentInput{
		        float4 position 	: 	SV_POSITION;
				float2 uv_Control		: 	TEXCOORD0;
				float3 NormalDir : NORMAL;
		    };
		    
		    

			// Vertex Shader
		    fragmentInput vert(vertexInput i){
		        fragmentInput o;
		        o.position = mul (UNITY_MATRIX_MVP, i.vertex);
		        o.uv_Control = i.texcoord0;
		        o.NormalDir = i.normal;
		        return o;
		    }
		    
		    // Fragment Shader
		    float4 frag(fragmentInput i) : SV_Target {
		    
		    //float4 mask = tex2D (_Control, i.mTexWT);
		   		float4 mask = saturate( tex2D (_Control, i.uv_Control) * 2 - 1 );
		   		float4 upperAlbedo = tex2D (_SplatLower, i.uv_Control * _LowerScale);
		   		float4 lowerAlbedo = tex2D (_SplatLower, i.uv_Control * _LowerScale);
		   		float4 stratum0Albedo = tex2D (_Splat0, i.uv_Control * _Splat0Scale);
		   		float4 stratum1Albedo = tex2D (_Splat1, i.uv_Control * _Splat1Scale);
		   		float4 stratum2Albedo = tex2D (_Splat2, i.uv_Control * _Splat2Scale);
		   		float4 stratum3Albedo = tex2D (_Splat3, i.uv_Control * _Splat3Scale);
		   		
	   		    float3 normal = i.NormalDir;
	   		    
   		    	float4 albedo = lowerAlbedo;
				albedo = lerp( albedo, stratum0Albedo, mask.r );
				albedo = lerp( albedo, stratum1Albedo, mask.g );
				albedo = lerp( albedo, stratum2Albedo, mask.b );
				albedo = lerp( albedo, stratum3Albedo, mask.a );
				
				float waterDepth = tex2D( _UtilitySamplerC, i.uv_Control).g;
		    
    			return lowerAlbedo;
		    }
		   		    
		ENDCG
        }
    }
}