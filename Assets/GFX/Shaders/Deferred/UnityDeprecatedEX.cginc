inline half CustomDotClamped ( half3 a, half3 b ){ 
 #if (SHADER_TARGET < 30)
    return saturate(dot(a,b));
 #else
    return max(0.0h, dot(a,b));
#endif
}

inline half CustomLambertTerm ( half3 normal, half3 lightDir ) {
  //return smoothstep(0.0,0.05f, CustomDotClamped (normal, lightDir));
  return dot (lightDir, normal);
}