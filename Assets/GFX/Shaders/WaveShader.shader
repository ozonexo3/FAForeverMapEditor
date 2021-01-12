// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'

// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'
// Upgrade NOTE: upgraded instancing buffer 'MyProperties' to new syntax.

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MapEditor/Wave" {
Properties {
_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	_Ramp("Ramp", 2D) = "white" {}
	_Cutoff("Alpha cutoff", Range(0,1)) = 0.5
	_Velocity("Velocity", Vector) = (0,0,0,0)
	_First("First", Vector) = (0,0,0,0)
	_Second("Second", Vector) = (0,0,0,0)
	_Frames("Frames", Vector) = (0,0,0,0)
}
SubShader{
	Tags {"Queue" = "Transparent+5" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
	LOD 100

	Lighting Off
		//ZTest Always
			ZWrite Off
		Blend One OneMinusSrcAlpha
		Offset 0.00001, 0.00001
			Cull Off

	Pass {  
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
				half3 lifetime : TEXCOORD1;
				//UNITY_VERTEX_INPUT_INSTANCE_ID // necessary only if you want to access instanced properties in the fragment Shader.
			};

			fixed4 _Color;
			sampler2D _MainTex;
			sampler2D _Ramp;
			float4 _MainTex_ST;
			fixed _Cutoff;

			float _SupComTime;

			UNITY_INSTANCING_BUFFER_START(MyProperties)
				UNITY_DEFINE_INSTANCED_PROP(float4, _Velocity)
				#define _Velocity_arr MyProperties
				UNITY_DEFINE_INSTANCED_PROP(float4, _First)
				#define _First_arr MyProperties
				UNITY_DEFINE_INSTANCED_PROP(float4, _Second)
				#define _Second_arr MyProperties
				UNITY_DEFINE_INSTANCED_PROP(float4, _Frames)
				#define _Frames_arr MyProperties
			UNITY_INSTANCING_BUFFER_END(MyProperties)

			v2f vert (appdata_t v)
			{
				v2f o;

				UNITY_SETUP_INSTANCE_ID(v);
				//UNITY_TRANSFER_INSTANCE_ID(v, o); // necessary only if you want to access instanced properties in the fragment Shader.

				float4 first = UNITY_ACCESS_INSTANCED_PROP(_First_arr, _First);
				float4 second = UNITY_ACCESS_INSTANCED_PROP(_Second_arr, _Second);
				float4 frames = UNITY_ACCESS_INSTANCED_PROP(_Frames_arr, _Frames);

				//o.lifetime.x = ((_SupComTime * 10 + frames.w + first.y) % first.x) / first.x;
				//o.lifetime.y = (_SupComTime * 10 + frames.w + second.y - first.x) % second.x / second.x;

				//float secondOffset = first.y;
				//float totalTime = (_SupComTime - frames.w) % (secondOffset + second.x / 60. + second.y);
				float localTime = clamp(_SupComTime - frames.w, 0.0, 1000000.0);
				float totalTime = localTime % (first.y + second.y);

				o.lifetime.x = saturate(totalTime / first.y);
				o.lifetime.y = saturate((totalTime - first.y) / second.y);
				//o.lifetime.y = 1;

				o.lifetime.z = saturate(v.normal.y * 1000.0);

				//UNITY_BRANCH
				if (o.lifetime.z > 0.5) {

					if (o.lifetime.x < 1.0) {
						float4 vel = float4(UNITY_ACCESS_INSTANCED_PROP(_Velocity_arr, _Velocity).xyz, 0);
						//vel.x *= -1;
						vel = mul(unity_WorldToObject, vel);
						//vel.z *= -1;

						v.vertex.xyz *= lerp(first.z, second.z, o.lifetime.x);
						v.vertex.xyz -= vel.xyz * (o.lifetime.x);

					}
				}
				else {
					v.normal.y = -v.normal.y;
					if (o.lifetime.y < 1.0) {
						float4 vel = float4(UNITY_ACCESS_INSTANCED_PROP(_Velocity_arr, _Velocity).xyz, 0);
						//vel.x *= -1;
						vel = mul(unity_WorldToObject, vel);
						//vel.z *= -1;
						v.vertex.xyz *= lerp(first.z, second.z, o.lifetime.y);
						v.vertex.xyz -= vel.xyz * (o.lifetime.y);

						//v.vertex.xyz *= second.z * 2;


					}
				}

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				//UNITY_SETUP_INSTANCE_ID(i);
				fixed4 col = tex2D(_MainTex, i.texcoord);
				fixed4 ramp = tex2D(_Ramp, float2((i.lifetime.z > 0.5) ? i.lifetime.x : i.lifetime.y, 0.5));

				//col.rgb *= _Color.rgb;

				col.a *= ramp.a;

				col.rgb *= col.a;

				//clip(col.a - _Cutoff);


				return col;
			}
		ENDCG
	}
}

}