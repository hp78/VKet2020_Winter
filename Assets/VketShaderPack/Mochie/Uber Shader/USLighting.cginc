#ifndef LIGHTING_INCLUDED
#define LIGHTING_INCLUDED

float FadeShadows (g2f i, float3 atten) {
    #if HANDLE_SHADOWS_BLENDING_IN_GI
        float viewZ = dot(_WorldSpaceCameraPos - i.worldPos, UNITY_MATRIX_V[2].xyz);
        float shadowFadeDistance = UnityComputeShadowFadeDistance(i.worldPos, viewZ);
        float shadowFade = UnityComputeShadowFade(shadowFadeDistance);
        atten = saturate(atten + shadowFade);
    #endif
	return atten;
}

void ApplyLREmission(lighting l, inout float3 diffuse, float3 emiss){
	float interpolator = 0;
	if (_ReactToggle == 1){
		if (_CrossMode == 1){
			float2 threshold = saturate(float2(_ReactThresh-_Crossfade, _ReactThresh+_Crossfade));
			interpolator = smootherstep(threshold.x, threshold.y, l.worldBrightness); 
		}
		else {
			interpolator = l.worldBrightness;
		}
	}
	diffuse = lerp(diffuse+emiss, diffuse, interpolator);
}

float3 GetAO(g2f i){
	float3 ao = 1;
	#if PACKED_WORKFLOW || PACKED_WORKFLOW_BAKED
		#if PACKED_WORKFLOW
			ao = ChannelCheck(packedTex, _OcclusionChannel);
		#else
			ao = packedTex.b;
		#endif
	#else
		ao = UNITY_SAMPLE_TEX2D_SAMPLER(_OcclusionMap, _MainTex, i.uv.xy).g;
	#endif

	float3 tintTex = UNITY_SAMPLE_TEX2D_SAMPLER(_AOTintTex, _MainTex, i.uv.xy).rgb;

	if (_AOFiltering == 1){
		_AOTint.rgb *= tintTex;
		ao = lerp(_AOTint, 1, ao);
		ApplyPBRFiltering(ao, _AOContrast, _AOIntensity, _AOLightness, _AOFiltering, prevAO);
	}
	ao = lerp(1, ao, _OcclusionStrength);
	return ao;
}

float3 GetNormalDir(g2f i, lighting l, masks m){
	#if !OUTLINE_PASS && (NORMALMAP_ENABLED || DETAIL_NORMALMAP_ENABLED)
	
		#if X_FEATURES
			if (_Screenspace == 1)
				return normalize(i.normal);
		#endif

		#if NORMALMAP_ENABLED && DETAIL_NORMALMAP_ENABLED
			float3 normalMap = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_BumpMap, _MainTex, i.uv.xy), _BumpScale);
			float3 detailNormal = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_DetailNormalMap, _MainTex, i.uv2.xy), _DetailNormalMapScale * m.detailMask);
			normalMap = BlendNormals(normalMap, detailNormal);
		#endif

		#if NORMALMAP_ENABLED && !DETAIL_NORMALMAP_ENABLED
			float3 normalMap = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_BumpMap, _MainTex, i.uv.xy), _BumpScale);
			normalMap = normalize(normalMap);
		#endif

		#if !NORMALMAP_ENABLED && DETAIL_NORMALMAP_ENABLED		
			float3 normalMap = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_DetailNormalMap, _MainTex, i.uv2.xy), _DetailNormalMapScale * m.detailMask);
			normalMap = normalize(normalMap);
		#endif

		float3 hardNormals = normalize(cross(ddy(i.worldPos), ddx(i.worldPos)));
		i.normal = lerp(normalize(i.normal), hardNormals, _HardenNormals);
			
		return normalize(normalMap.x * l.tangent + normalMap.y * l.binormal + normalMap.z * i.normal);
	#else
		return normalize(i.normal);
	#endif
}

float NonlinearSH(float L0, float3 L1, float3 normal) {
    float R0 = L0;
    float3 R1 = 0.5f * L1;
    float lenR1 = length(R1);
    float q = dot(normalize(R1), normal) * 0.5 + 0.5;
    q = max(0, q);
    float p = 1.0f + 2.0f * lenR1 / R0;
    float a = (1.0f - lenR1 / R0) / (1.0f + lenR1 / R0);
    return R0 * (a + (1.0f - a) * (p + 1.0f) * pow(q, p));
}

float3 ShadeSHNL(float3 normal) {
    float3 indirect;
    indirect.r = NonlinearSH(unity_SHAr.w, unity_SHAr.xyz, normal);
    indirect.g = NonlinearSH(unity_SHAg.w, unity_SHAg.xyz, normal);
    indirect.b = NonlinearSH(unity_SHAb.w, unity_SHAb.xyz, normal);
    return max(0, indirect);
}

float3 ShadeSH9(float3 normal){
	return max(0, ShadeSH9(float4(normal,1)));
}

void GetVertexLightData(g2f i, inout lighting l){

	// Attenuation
	float4 toLightX = unity_4LightPosX0 - i.worldPos.x;
	float4 toLightY = unity_4LightPosY0 - i.worldPos.y;
	float4 toLightZ = unity_4LightPosZ0 - i.worldPos.z;

	float4 lengthSq = 0;
	lengthSq += toLightX * toLightX;
	lengthSq += toLightY * toLightY;
	lengthSq += toLightZ * toLightZ;

	float4 atten0 = 1.0 / (1.0 + lengthSq * unity_4LightAtten0);
	float4 atten1 = saturate(1 - (lengthSq * unity_4LightAtten0 / 25));
	float4 atten = min(atten0, atten1 * atten1);

	// Shadow ramp
	float4 NdotL = 0;
	NdotL += toLightX * l.normal.x;
	NdotL += toLightY * l.normal.y;
	NdotL += toLightZ * l.normal.z;

	if (_ShadowMode == 1){
		float4 ramp0 = smootherstep(float4(0,0,0,0), _RampWidth0, NdotL);
		float4 ramp1 = smootherstep(float4(0,0,0,0), _RampWidth1, NdotL);
		atten = lerp(ramp0, ramp1, _RampWeight) * atten;
	}
	else if (_ShadowMode == 2){
		float4 rampUV = NdotL * 0.5 + 0.5;
		float ramp0 = tex2D(_ShadowRamp, rampUV.xx);
		float ramp1 = tex2D(_ShadowRamp, rampUV.yy);
		float ramp2 = tex2D(_ShadowRamp, rampUV.zz);
		float ramp3 = tex2D(_ShadowRamp, rampUV.ww);
		atten = float4(ramp0, ramp1, ramp2, ramp3) * atten;
	}

	// Color
	float3 light0 = atten.x * unity_LightColor[0];
	float3 light1 = atten.y * unity_LightColor[1];
	float3 light2 = atten.z * unity_LightColor[2];
	float3 light3 = atten.w * unity_LightColor[3];

	l.vLightCol = (light0 + light1 + light2 + light3) * _VLightCont;

	// Direction
	float3 toLightXD = float3(unity_4LightPosX0.x, unity_4LightPosY0.x, unity_4LightPosZ0.x);
    float3 toLightYD = float3(unity_4LightPosX0.y, unity_4LightPosY0.y, unity_4LightPosZ0.y);
    float3 toLightZD = float3(unity_4LightPosX0.z, unity_4LightPosY0.z, unity_4LightPosZ0.z);
	float3 toLightWD = float3(unity_4LightPosX0.w, unity_4LightPosY0.w, unity_4LightPosZ0.w);

    float3 dirX = toLightXD - i.worldPos;
    float3 dirY = toLightYD - i.worldPos;
    float3 dirZ = toLightZD - i.worldPos;
	float3 dirW = toLightWD - i.worldPos;
	
	dirX *= length(toLightXD) * light0;
	dirY *= length(toLightYD) * light1;
	dirZ *= length(toLightZD) * light2;
	dirW *= length(toLightWD) * light3;
	
	l.vLightDir = dirX + dirY + dirZ + dirW;
}

float3 GetLightDir(g2f i, lighting l) {
	float3 lightDir = UnityWorldSpaceLightDir(i.worldPos);
	#if FORWARD_PASS
		lightDir *=  l.lightEnv;
		lightDir += (unity_SHAr.xyz + unity_SHAg.xyz + unity_SHAb.xyz) * !l.lightEnv;
		#if VERTEX_LIGHT
			lightDir += l.vLightDir;
		#endif
	#endif
	lightDir = lerp(lightDir, _StaticLightDir.xyz, _StaticLightDirToggle);
	return normalize(lightDir);
}

void GetLightColor(g2f i, inout lighting l, masks m){
	#if FORWARD_PASS
		float3 probeCol = float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);

		#if SHADING_ENABLED
			UNITY_BRANCH
			if (_NonlinearSHToggle == 1)
				l.indirectCol = ShadeSHNL(l.normal);
			else
				l.indirectCol = ShadeSH9(l.normal);
			l.indirectCol = lerp(probeCol, l.indirectCol, _SHStr*m.diffuseMask);

			if (l.lightEnv){
				l.directCol = _LightColor0 * _RTDirectCont;
				l.indirectCol *= _RTIndirectCont;
			}
			else {
				l.directCol = l.indirectCol * _DirectCont;
				l.indirectCol *= _IndirectCont;
			}
		#else
			l.indirectCol = probeCol;
			if (l.lightEnv){
				l.directCol = _LightColor0;
			}
			else {
				l.directCol = l.indirectCol * 0.6;
				l.indirectCol *= 0.5;
			}
		#endif

		l.worldBrightness = saturate(Average(l.directCol + l.indirectCol + l.vLightCol));
		l.directCol *= lerp(1, l.ao, _DirectAO);
		l.indirectCol *= lerp(1, l.ao, _IndirectAO);
	#else
		#if SHADING_ENABLED
			l.directCol = lerp(_LightColor0, saturate(_LightColor0), _ClampAdditive);
		#else
			l.directCol = saturate(_LightColor0);
		#endif
	#endif
}

lighting GetLighting(g2f i, masks m, float3 atten){
    lighting l = (lighting)0;
	l.ao = 1;

	#if FORWARD_PASS
		l.lightEnv = any(_WorldSpaceLightPos0);
	#endif

	l.screenUVs = i.screenPos.xy / (i.screenPos.w+0.0000000001);
	#if UNITY_SINGLE_PASS_STEREO
		l.screenUVs.x *= 2;
	#endif

    #if SHADING_ENABLED
		l.ao = GetAO(i);
		l.viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
		l.tangent = i.tangent;
		l.binormal = cross(i.normal, i.tangent.xyz) * (i.tangent.w * unity_WorldTransformParams.w);
		l.normalDir = GetNormalDir(i,l,m);
		l.normal = lerp(l.normalDir, normalize(i.normal), _ClearCoat);
		l.reflectionDir = reflect(-l.viewDir, l.normal);
		#if VERTEX_LIGHT
			GetVertexLightData(i, l);
		#endif
		l.lightDir = GetLightDir(i, l);
		l.halfVector = normalize(l.lightDir + l.viewDir);

		l.NdotL = DotClamped(l.normalDir, l.lightDir);
		l.NdotV = abs(dot(l.normal, l.viewDir));
		l.NdotH = DotClamped(l.normal, l.halfVector);
		l.LdotH = DotClamped(l.lightDir, l.halfVector);
		#if SPECULAR_ENABLED && !OUTLINE_PASS
			l.TdotH = dot(l.tangent, l.halfVector);
			l.BdotH = dot(l.binormal, l.halfVector);
		#endif
    #else
		#if VERTEX_LIGHT
			GetVertexLightData(i, l);
		#endif
		#if ADDITIVE_PASS
			l.lightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));
			l.normal = normalize(i.normal);
			l.NdotL = DotClamped(l.normal, l.lightDir);
		#endif
	#endif

	GetLightColor(i,l,m);

    return l;
}

#endif // LIGHTING_INCLUDED