//--------------------------------------------------------------
//              Sunao Shader Core
//                      Copyright (c) 2020 揚茄子研究所
//--------------------------------------------------------------


//-------------------------------------Include

	#include "UnityCG.cginc"
	#include "AutoLight.cginc"
	#include "Lighting.cginc"
	#include "SunaoShader_Function.cginc"

//-------------------------------------変数宣言

//----Main
	UNITY_DECLARE_TEX2D(_MainTex);
	uniform float4    _MainTex_ST;
	uniform float4    _Color;
	uniform float     _Cutout;
	uniform float     _Alpha;
	uniform sampler2D _BumpMap;
	UNITY_DECLARE_TEX2D_NOSAMPLER(_OcclusionMap);
	UNITY_DECLARE_TEX2D_NOSAMPLER(_AlphaMask);
	uniform float     _Bright;
	uniform float     _BumpScale;
	uniform float     _OcclusionStrength;
	uniform float     _OcclusionMode;
	uniform float     _AlphaMaskStrength;
	uniform bool      _VertexColor;
	uniform float     _UVScrollX;
	uniform float     _UVScrollY;
	uniform float     _UVAnimation;
	uniform uint      _UVAnimX;
	uniform uint      _UVAnimY;
	uniform bool      _UVAnimOtherTex;
	uniform bool      _DecalEnable;
	uniform sampler2D _DecalTex;
	uniform float4    _DecalTex_TexelSize;
	uniform float4    _DecalColor;
	uniform float     _DecalPosX;
	uniform float     _DecalPosY;
	uniform float     _DecalSizeX;
	uniform float     _DecalSizeY;
	uniform float     _DecalRotation;
	uniform uint      _DecalMode;
	uniform uint      _DecalMirror;
	uniform float     _DecalScrollX;
	uniform float     _DecalScrollY;
	uniform float     _DecalAnimation;
	uniform uint      _DecalAnimX;
	uniform uint      _DecalAnimY;

//----Shading & Lighting
	UNITY_DECLARE_TEX2D_NOSAMPLER(_ShadeMask);
	uniform float     _Shade;
	uniform float     _ShadeWidth;
	uniform float     _ShadeGradient;
	uniform float     _ShadeColor;
	uniform float4    _CustomShadeColor;
	uniform bool      _ToonEnable;
	uniform uint      _Toon;
	uniform float     _ToonSharpness;
	uniform sampler2D _LightMask;
	uniform float     _LightBoost;
	uniform float     _Unlit;
	uniform bool      _MonochromeLit;

//----Emission
	uniform bool      _EmissionEnable;
	uniform sampler2D _EmissionMap;
	uniform float4    _EmissionMap_ST;
	uniform float4    _EmissionColor;
	uniform float     _Emission;
	uniform sampler2D _EmissionMap2;
	uniform float4    _EmissionMap2_ST;
	uniform uint      _EmissionMode;
	uniform float     _EmissionBlink;
	uniform float     _EmissionFrequency;
	uniform uint      _EmissionWaveform;
	uniform float     _EmissionScrX;
	uniform float     _EmissionScrY;
	uniform float     _EmissionAnimation;
	uniform uint      _EmissionAnimX;
	uniform uint      _EmissionAnimY;
	uniform bool      _EmissionLighting;
	uniform bool      _IgnoreTexAlphaE;
	uniform float     _EmissionInTheDark;

//----Parallax Emission
	uniform bool      _ParallaxEnable;
	uniform sampler2D _ParallaxMap;
	uniform float4    _ParallaxMap_ST;
	uniform float4    _ParallaxColor;
	uniform float     _ParallaxEmission;
	uniform float     _ParallaxDepth;
	UNITY_DECLARE_TEX2D_NOSAMPLER(_ParallaxDepthMap);
	uniform float4    _ParallaxDepthMap_ST;
	uniform sampler2D _ParallaxMap2;
	uniform float4    _ParallaxMap2_ST;
	uniform uint      _ParallaxMode;
	uniform float     _ParallaxBlink;
	uniform float     _ParallaxFrequency;
	uniform uint      _ParallaxWaveform;
	uniform float     _ParallaxPhaseOfs;
	uniform float     _ParallaxScrX;
	uniform float     _ParallaxScrY;
	uniform float     _ParallaxAnimation;
	uniform uint      _ParallaxAnimX;
	uniform uint      _ParallaxAnimY;
	uniform bool      _ParallaxLighting;
	uniform bool      _IgnoreTexAlphaPE;
	uniform float     _ParallaxInTheDark;

//----Reflection
	uniform bool      _ReflectionEnable;
	uniform sampler2D _MetallicGlossMap;
	UNITY_DECLARE_TEX2D_NOSAMPLER(_MatCap);
	uniform float     _Specular;
	uniform float     _Metallic;
	uniform float     _MatCapStrength;
	uniform float     _GlossMapScale;
	uniform bool      _SpecularTexColor;
	uniform bool      _MetallicTexColor;
	uniform bool      _MatCapTexColor;
	uniform bool      _SpecularSH;
	uniform float     _SpecularMask;
	uniform uint      _ReflectLit;
	uniform uint      _MatCapLit;
	uniform bool      _IgnoreTexAlphaR;

//----Rim Lighting
	uniform bool      _RimLitEnable;
	UNITY_DECLARE_TEX2D_NOSAMPLER(_RimLitMask);
	uniform float     _RimLit;
	uniform float     _RimLitGradient;
	uniform float4    _RimLitColor;
	uniform bool      _RimLitLighthing;
	uniform bool      _RimLitTexColor;
	uniform uint      _RimLitMode;
	uniform bool      _IgnoreTexAlphaRL;

//----Other
	uniform float     _DirectionalLight;
	uniform float     _PointLight;
	uniform float     _SHLight;
	uniform bool      _LightLimitter;
	uniform float     _MinimumLight;

	uniform bool      _EnableGammaFix;
	uniform float     _GammaR;
	uniform float     _GammaG;
	uniform float     _GammaB;

	uniform bool      _EnableBlightFix;
	uniform float     _BlightOutput;
	uniform float     _BlightOffset;

	uniform bool      _LimitterEnable;
	uniform float     _LimitterMax;


//-------------------------------------頂点シェーダ入力構造体

struct VIN {
	float4 vertex  : POSITION;
	float2 uv      : TEXCOORD;
	float3 normal  : NORMAL;
	float4 tangent : TANGENT;
	float3 color   : COLOR;
};


//-------------------------------------頂点シェーダ出力構造体

struct VOUT {

	float4 pos     : SV_POSITION;
	float2 uv      : TEXCOORD0;
	float4 uvanm   : TEXCOORD1;
	float4 decal   : TEXCOORD2;
	float4 decal2  : TEXCOORD3;
	float4 decanm  : TEXCOORD4;
	float3 normal  : NORMAL;
	float3 color   : COLOR0;
	float4 tangent : TANGENT;
	float3 ldir    : LIGHTDIR0;
	float3 view    : TEXCOORD5;
	float4 toon    : TEXCOORD6;
	float3 tanW    : TEXCOORD7;
	float3 tanB    : TEXCOORD8;
	float3 matcapv : TEXCOORD9;
	float3 matcaph : TEXCOORD10;
	float4 euv     : TEXCOORD11;
	float3 eprm    : TEXCOORD12;
	float4 peuv    : TEXCOORD13;
	float2 pduv    : TEXCOORD14;
	float3 peprm   : TEXCOORD15;
	float3 pview   : TEXCOORD16;

	#ifdef PASS_FB
		float3 shdir   : LIGHTDIR1;
		float3 shmax   : COLOR1;
		float3 shmin   : COLOR2;
		float4 vldirX  : LIGHTDIR2;
		float4 vldirY  : LIGHTDIR3;
		float4 vldirZ  : LIGHTDIR4;
		float4 vlcorr  : TEXCOORD17;
		float4 vlatn   : TEXCOORD18;
	#endif

	UNITY_FOG_COORDS(19)
	#ifdef PASS_FA
		LIGHTING_COORDS(20 , 21)
	#endif

};


//-------------------------------------頂点シェーダ

	#include "SunaoShader_Vert.cginc"


//-------------------------------------フラグメントシェーダ

	#include "SunaoShader_Frag.cginc"
