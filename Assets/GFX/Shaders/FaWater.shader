// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "MapEditor/FaWater" {
	Properties {
		waterColor  ("waterColor", Color) = (0.0, 0.7, 1.5, 1)
		_UtilitySamplerC ("_UtilitySamplerC", 2D) = "white" {}
		SkySampler("SkySampler", CUBE) = "" {}
		ReflectionSampler ("ReflectionSampler", 2D) = "white" {}
		
		RefractionSampler ("RefractionSampler", 2D) = "white" {}
		_Reflection("Reflection", CUBE) = "" {}
		FresnelSampler ("FresnelSampler", 2D) = "white" {}
		NormalSampler0 ("NormalSampler0", 2D) = "white" {}
		NormalSampler1 ("NormalSampler1", 2D) = "white" {}
		NormalSampler2 ("NormalSampler2", 2D) = "white" {}
		NormalSampler3 ("NormalSampler3", 2D) = "white" {}
		_WaterScale ("Water Scale", Range (-5000, -500)) = -102.4
	}
    SubShader {
    Tags { "Queue"="Transparent" "RenderType"="Transparent" }
   		Offset 1000, 0
	    GrabPass 
	     {
	     "_WaterGrabTexture"
	     } 
        Pass {
			Offset 0, 0
			LOD 200
			Blend SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM
		    #pragma vertex vert
		    #pragma fragment frag
		    #pragma target 3.0

		    #include "UnityCG.cginc"



			//************ Water Params
			
			float _WaterScale;
			float waveCrestThreshold = 1;
			float3 waveCrestColor = float3( 1, 1, 1);
			float refractionScale = 0.015;

			// reflection amount
			float unitreflectionAmount = 0.5;

			// sky reflection amount
			float skyreflectionAmount = 1.5;

			// 3 repeat rate for 3 texture layers
			float4  normalRepeatRate = float4(0.0009, 0.009, 0.05, 0.5);
			

			// 3 vectors of normal movements
			//float2 normal1Movement = float2(0.5, -0.95);
			float2 normal1Movement = float2(5.5, -9.95);
			float2 normal2Movement = float2(0.05, -0.095);
			float2 normal3Movement = float2(0.01, 0.03);
			float2 normal4Movement = float2(0.0005, 0.0009);
			
			//*********** End Water Params
			sampler2D _WaterGrabTexture;


		    struct vertexInput {
		        float4 vertex : POSITION;
		        float4 texcoord0 : TEXCOORD0;
		    };

		    struct fragmentInput{
		        float4 position 	: 	SV_POSITION;
				float2 mTexUV		: 	TEXCOORD0;
				float2 mLayer0      : 	TEXCOORD1;
				float2 mLayer1      : 	TEXCOORD2;
				float2 mLayer2      : 	TEXCOORD3;
			    float2 mLayer3      : 	TEXCOORD4;	
				float3 mViewVec     : 	TEXCOORD5;
				float4 mScreenPos	: 	TEXCOORD6;
				float4 AddVar		: 	TEXCOORD7;
				float4 GrabPos		: 	TEXCOORD8;
		    };
		    
			// Vertex Shader
		    fragmentInput vert(vertexInput i){
		        fragmentInput o;
		        o.position = mul (UNITY_MATRIX_MVP, i.vertex);
		        o.mScreenPos = ComputeScreenPos(o.position);
		        o.mScreenPos.xy /=  o.mScreenPos.w;
		        //o.mScreenPos.xy /= _ScreenParams.xy * 0.1;
		        
		        o.mTexUV = i.texcoord0;
		        o.mLayer0 = (i.vertex.xz * _WaterScale + (float2(5.5, -9.95) * _Time.y)) * 0.0009;
		        o.mLayer1 = (i.vertex.xz * _WaterScale + (float2(0.05, -0.095) * _Time.y)) * 0.09;
		        o.mLayer2 = (i.vertex.xz * _WaterScale + (float2(0.01, 0.03) * _Time.y)) * 0.05;
		        o.mLayer3 = (i.vertex.xz * _WaterScale + (float2(0.0005, 0.0009) * _Time.y)) * 0.5;

		        //o.mScreenPos = mul (UNITY_MATRIX_MVP, float4(0,0,0,1));
		        //o.mScreenPos.xy /= o.mScreenPos.w;
		        
		        o.mViewVec = mul (unity_ObjectToWorld, i.vertex).xyz - _WorldSpaceCameraPos;
		        o.mViewVec = normalize(o.mViewVec);
		        o.AddVar = float4(length(_WorldSpaceCameraPos - mul(unity_ObjectToWorld, i.vertex).xyz), 0, 0, 0);

		        float4 hpos = mul (UNITY_MATRIX_MVP, i.vertex);
		        o.GrabPos = ComputeGrabScreenPos(hpos);
		        //o.mViewVec *= 0.03;
		        return o;
		    }
		    
		    fixed4 waterColor;
		    uniform sampler2D _UtilitySamplerC;
		    uniform sampler2D RefractionSampler;
		    uniform sampler2D  ReflectionSampler;
		    uniform sampler2D FresnelSampler;
		    uniform sampler2D NormalSampler0, NormalSampler1, NormalSampler2, NormalSampler3;
		     samplerCUBE _Reflection;
		     samplerCUBE SkySampler;
		    
		    // Fragment Shader
		    float4 frag(fragmentInput i) : SV_Target {
		    
		    	float4 ViewportScaleOffset = float4((_ScreenParams.x / _ScreenParams.y) * 1, 1.0, (_ScreenParams.x / _ScreenParams.y) * -0.25, 0);
		    	float3 SunDirection = normalize(float3( -0.2 , -0.967, -0.453));
		    	float SunShininess = 50;
		    	float3 SunColor = normalize(float3( 1.1, 0.7, 0.5 ));
		    	float sunReflectionAmount = 5;
		    	float unitreflectionAmount = 0.5;
		    	float2 waterLerp = 0.3;
		    	float3 waveCrestColor = float3( 1, 1, 1);
		    	float SunGlow = 1;
		    	
		    	// calculate the depth of water at this pixel, we use this to decide
    			// how to shade and it should become lesser as we get shallower
    			// so that we don't have a sharp line
		    	float4 waterTexture = tex2D( _UtilitySamplerC, i.mTexUV * float2(-1, 1) );
    			float waterDepth = waterTexture.g;
    			
		        // calculate the correct viewvector
   				float3 viewVector = normalize(i.mViewVec);
   				
			    float2 screenPos = UNITY_PROJ_COORD(i.mScreenPos.xy);
			    
   				// calculate the background pixel
   				float4 backGroundPixels = tex2D( _WaterGrabTexture, UNITY_PROJ_COORD(i.GrabPos) );

    			float mask = saturate(backGroundPixels.a * 255);
		    
		        // calculate the normal we will be using for the water surface
			    float4 W0 = tex2D( NormalSampler0, i.mLayer0 );
				float4 W1 = tex2D( NormalSampler1, i.mLayer1 );
				float4 W2 = tex2D( NormalSampler2, i.mLayer2 );
				float4 W3 = tex2D( NormalSampler3, i.mLayer3 );

			    float4 sum = W0 + W1 + W2 + W3;
			    float waveCrest = saturate( sum.a - waveCrestThreshold );
			    
			    // average, scale, bias and normalize
   				float3 N = 2.0 * sum.xyz - 4.0;
   				
   				// flatness
			   	N = normalize(N.xzy); 
			    float3 up = float3(0,1,0);
			  	N = lerp(up, N, 0.8);
			    
			    // calculate the reflection vector
				float3 R = reflect( i.mViewVec, N );
		    
		        // calculate the sky reflection color
				float4 skyReflection = texCUBE( SkySampler, R );
		    		    	
		    	// get the correct coordinate for sampling refraction and reflection
			    float2 refractionPos = screenPos;
			    refractionPos -=  0.015 * N.xz * 10;
		    	
		    	// calculate the refract pixel, corrected for fetching a non-refractable pixel
			    float4 refractedPixels = tex2D( _WaterGrabTexture, UNITY_PROJ_COORD(i.GrabPos));

			    // because the range of the alpha value that we use for the water is very small
			    // we multiply by a large number and then saturate
			    // this will also help in the case where we filter to an intermediate value
			    refractedPixels.xyz = lerp(refractedPixels, backGroundPixels, saturate((i.AddVar.x - 40) / 30 ) ).xyz; //255
    
    
    			float4 reflectedPixels = tex2D( ReflectionSampler, refractionPos);
    
    
		        float fresnel;    
		   		float  NDotL = saturate( dot(-viewVector, N) );
				fresnel = tex2D( FresnelSampler, float2(waterDepth, NDotL ) ).r;
    
    			// figure out the sun reflection
        		float3 sunReflection = pow( saturate(dot(-R, SunDirection)), SunShininess) * SunColor;
        		// lerp the reflections together
       			reflectedPixels = lerp( skyReflection, reflectedPixels, saturate(unitreflectionAmount * reflectedPixels.w));
       			
       			// we want to lerp in some of the water color based on depth, but
    			// not totally on depth as it gets clamped
   				float waterLerp2 = clamp(waterDepth, waterLerp.x, waterLerp.y);
   				
   				// lerp in the color
    			refractedPixels.xyz = lerp( refractedPixels.xyz, waterColor.rgb, waterLerp2);
    			
    			// implement the water depth into the reflection
			    float depthReflectionAmount = 10;
			    float skyreflectionAmount = 1.5;
			    skyreflectionAmount *= saturate(waterDepth * depthReflectionAmount);
			    
			   	// lerp the reflection into the refraction   
    			refractedPixels = lerp( refractedPixels, reflectedPixels, saturate(skyreflectionAmount * fresnel));
    			
    			// add in the sky reflection
			   	//sunReflection = sunReflection * fresnel;
			    refractedPixels.xyz +=  sunReflection;
			    
			    //refractedPixels.xyz = lerp( refractedPixels.xyz, waveCrestColor * 0.1, ( 1 - waterTexture.a ) * waveCrest);
    
        		float4 returnPixels = refractedPixels;
    			returnPixels.a = waterDepth;
    			return returnPixels;
    			//return float4(UNITY_PROJ_COORD(screenPos).xy, 0, 1);
    			//return float4((i.AddVar.x - 40) / 50, 0, 0, 1);
		  		//return float4(sunReflection, 1);
		    }
            ENDCG
        }
    }
}