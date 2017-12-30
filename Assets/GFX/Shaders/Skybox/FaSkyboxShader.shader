// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "MapEditor/FaSkybox"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_HorizonHeight ("Grid Scale", Float) = 0
		_HorizonColor ("Horizon Color", Color) = (0.5, 0.5, 0.5, 1)
		_ZenithHeight ("Grid Scale", Float) = 256
		_ZenithColor ("Zenith Color", Color) = (0.5, 0.5, 0.5, 1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
		ZTest Less

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 WorldPos : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			half _HorizonHeight, _ZenithHeight;
			fixed4 _HorizonColor, _ZenithColor;

			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.WorldPos = mul(unity_ObjectToWorld, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);

				col = lerp(_HorizonColor, _ZenithColor, smoothstep(_HorizonHeight, _ZenithHeight, i.WorldPos.y));

				return col;
			}
			ENDCG
		}
	}
}
