			uniform half _LightingMultiplier;
			uniform fixed4 _SunColor;
			uniform fixed4 _SunAmbience;
			uniform fixed4 _ShadowColor;

/*
float4 LightingSimpleLambertLight  (SurfaceOutput s, float3 lightDir, half atten) {
	float NdotL = dot (lightDir, s.Normal);
			              
	float4 c;
	float3 spec = float3(0,0,0);

	float3 light =  _SunColor.rgb * 2 * saturate(NdotL) * atten + _SunAmbience.rgb * 2;
	light = _LightingMultiplier * light + _ShadowColor.rgb * 2 * (1 - light);


	c.rgb = (s.Albedo + spec) * light;
	c.a = s.Alpha;
	return c;
}*/


			
inline float4 LightingSimpleLambertLight  (SurfaceOutput s, UnityLight light)
{
	float NdotL = dot (light.dir, s.Normal);
	fixed diff = max (0, dot (s.Normal, light.dir));
						 
	float4 c;
	float3 spec = float3(0,0,0);

	float3 lighting = light.color * saturate(NdotL);
	lighting = _LightingMultiplier * lighting + _ShadowColor.rgb * 2 * (1 - lighting);


	c.rgb = (s.Albedo + spec) * lighting;
	c.a = s.Alpha;
	return c;
}

inline fixed4 LightingSimpleLambert_PrePass (SurfaceOutput s, half4 light)
{
	fixed4 c;
	c.rgb = s.Albedo * light.rgb;
	c.rgb = s.Albedo;
	c.a = s.Alpha;
	return c;
}

inline fixed4 LightingSimpleLambert (SurfaceOutput s, UnityGI gi)
{
	fixed4 c;
	c = LightingSimpleLambertLight (s, gi.light);

	//#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
	//	c.rgb += s.Albedo * gi.indirect.diffuse;
	//#endif

	return c;
}

inline half4 LightingSimpleLambert_Deferred (SurfaceOutput s, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2)
{
	UnityStandardData data;
	data.diffuseColor   = s.Albedo;
	data.occlusion      = 1;
	data.specularColor  = 0;
	data.smoothness     = 0;
	data.normalWorld    = s.Normal;

	UnityStandardDataToGbuffer(data, outGBuffer0, outGBuffer1, outGBuffer2);

	half4 emission = half4(s.Emission, 1);

	//#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
	//	emission.rgb += s.Albedo * gi.indirect.diffuse;
	//#endif

	return emission;
}

inline void LightingSimpleLambert_GI (
		SurfaceOutput s,
		UnityGIInput data,
		inout UnityGI gi)
	{
		gi = UnityGlobalIllumination (data, 1.0, s.Normal);
	}