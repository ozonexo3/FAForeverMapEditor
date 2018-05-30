// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UI/RawNormalChannel"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "black" {}
		_Color ("Tint", Color) = (1,1,1,1)
		
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255

		_ColorMask ("Color Mask", Float) = 15
		_Mipmap ("Mipmap", int) = 0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
		
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#pragma target 3.0
			#pragma exclude_renderers gles
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
			};
			
			fixed4 _Color;
			int _Red;
			int _Green;
			int _Blue;
			int _Alpha;
			int _Mipmap;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
#ifdef UNITY_HALF_TEXEL_OFFSET
				OUT.vertex.xy += (_ScreenParams.zw-1.0)*float2(-1,1);
#endif
				OUT.color = IN.color * _Color;
				return OUT;
			}

			sampler2D _MainTex;

			inline fixed3 UnpackNormalDXT5nmScaled(fixed4 packednormal, fixed scale)
			{
				fixed3 normal = 0;
				normal.xz = packednormal.wx * 2 - 1;
				normal.y = sqrt(1 - saturate(dot(normal.xz, normal.xz)));
				normal.xz *= scale;

				return normal.xzy;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				half4 color = tex2Dlod(_MainTex, fixed4(IN.texcoord, 0, _Mipmap));



				//color.rgb = UnpackNormalDXT5nmScaled(color, 1) * 0.5f + 0.5f;

				//if(color.a < 0.05)
				//	color.rgb = half3(0.5,0,0);

				//color.rgb *= lerp(0.3, 1, color.a * color.a);
				//color.rgb = lerp(half3(0.5,0,0), color.rgb, color.a * color.a);

				color.a = 1;
				color *= IN.color;

				//clip (color.a - 0.01);
				return color;
			}
		ENDCG
		}
	}
}