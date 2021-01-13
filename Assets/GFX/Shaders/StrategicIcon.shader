// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Unlit shader. Simplest possible colored shader.
// - no lighting
// - no lightmap support
// - no texture

Shader "Custom/Strategic icon 3"
	{
		Properties{ _MainTex("Texture", any) = "" {} 
	}

			CGINCLUDE
#pragma vertex vert
#pragma fragment frag
#pragma target 2.0

#include "UnityCG.cginc"

			struct appdata_t {
			float4 vertex : POSITION;
			fixed4 color : COLOR;
			float2 texcoord : TEXCOORD0;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct v2f {
			float4 vertex : SV_POSITION;
			fixed4 color : COLOR;
			float2 texcoord : TEXCOORD0;
			UNITY_VERTEX_OUTPUT_STEREO
		};

		sampler2D _MainTex;

		uniform float4 _MainTex_ST;

		v2f vert(appdata_t v)
		{
			v2f o;
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.color = v.color;
			o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
			return o;
		}

		fixed4 frag(v2f i) : SV_Target
		{
			fixed4 c = tex2D(_MainTex, i.texcoord);

			if (i.color.a <= 0.1) {
				
				if (c.r > 0.9)
					c.rgb = fixed3(0.9, 0.9, 0.9);
				else if (c.r < 0.1)
					c.rgb = fixed3(0.5, 0.5, 0.5);
				else
					c.rgb = fixed3(0.15, 0.15, 0.15);
				//c.rgb = (c.r > 0.9) ? c.rgb : fixed3(0.5, 0.5, 0.5);
				//c.rgb = (c.r < 0.1) ? c.rgb : fixed3(0.1, 0.1, 0.1);
			}
			else {
				c.rgb = (c.r > 0.1 && c.r < 0.9) ? i.color : c.rgb;
			}

			return c;
		}
			ENDCG

			SubShader {

			Tags{ "RenderType" = "Overlay" }

				Lighting Off
				Blend SrcAlpha OneMinusSrcAlpha, One One
				Cull Off
				ZWrite Off
				ZTest Always

				Pass{
					CGPROGRAM
					ENDCG
			}
		}

		SubShader{

			Tags { "RenderType" = "Overlay" }

			Lighting Off
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			ZWrite Off
			ZTest Always

			Pass {
				CGPROGRAM
				ENDCG
			}
		}

			Fallback off
	}
