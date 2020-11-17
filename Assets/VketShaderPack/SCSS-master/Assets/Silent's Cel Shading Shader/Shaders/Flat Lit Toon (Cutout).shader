Shader "Silent's Cel Shading/Cutout"
{
	Properties
	{
		[Header(Main)]
		_MainTex("Main Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_Cutoff("Alpha Cutoff", Range(0,1)) = 0.5
		[Toggle(_)]_AlphaSharp("Disable Dithering for Cutout", Float) = 0.0
		[Space]
		_ColorMask("Color Mask Map", 2D) = "white" {}
		_BumpMap("Normal Map", 2D) = "bump" {}
		_BumpScale("Normal Map Scale", Float) = 1.0
		[Enum(VertexColorType)]_VertexColorType ("Vertex Colour Type", Float) = 2.0
		[Space]
		[Header(Emission)]
		_EmissionMap("Emission Map", 2D) = "white" {}
		[HDR]_EmissionColor("Emission Color", Color) = (0,0,0,1)
		[Space]
		[Header(Lighting)]
		[Enum(LightRampType)]_LightRampType ("Light Ramp Type", Float) = 0.0
		_Ramp ("Lighting Ramp", 2D) = "white" {}
		[Space]
		[Enum(ShadowMaskType)] _ShadowMaskType ("Shadow Mask Type", Float) = 0.0
		_ShadowMask("Shadow Mask", 2D) = "white" {} 
		_ShadowMaskColor("Shadow Mask Color", Color) = (1,1,1,1)
		_Shadow("Shadow Mask Power", Range(0, 1)) = 0.5
		_ShadowLift("Shadow Offset", Range(-1, 1)) = 0.0
		_IndirectLightingBoost("Indirect Lighting Boost", Range(0, 1)) = 0.0
		[Space]
		[Header(Outline)]
		[Enum(OutlineMode)] _OutlineMode("Outline Mode", Float) = 1.0
		_outline_width("Outline Width", Float) = 0.1
		_outline_color("Outline Colour", Color) = (0.5,0.5,0.5,1)
		[Space]
		[Header(Rim Lighting)]
		[Enum(AmbientFresnelType)]_UseFresnel ("Use Fresnel", Float) = 0.0
		_FresnelWidth ("Fresnel Strength", Range(0, 20)) = .5
		_FresnelStrength ("Fresnel Softness", Range(0.1, 0.9999)) = 0.5
		[HDR]_FresnelTint("Fresnel Tint", Color) = (1,1,1,1)
		[Space]
		[Header(Specular)]
		[Enum(SpecularType)] _SpecularType ("Specular Type", Float) = 0.0
		_SpecGlossMap ("Specular Map", 2D) = "black" {}
		[Toggle(_)]_UseMetallic ("Use as Metallic", Float) = 0.0
		[Toggle(_)]_UseEnergyConservation ("Energy Conservation", Float) = 1.0
		_Smoothness ("Smoothness", Range(0, 1)) = 1
		_Anisotropy("Anisotropy", Range(-1,1)) = 0.8
		[Space]
		[Header(Matcap)]
		[Enum(MatcapType)]_UseMatcap ("Matcap Type", Float) = 0.0
		_MatcapMask("Matcap Mask", 2D) = "white" {}
		[Space]_Matcap1("Matcap 1", 2D) = "black" {}
		_Matcap1Strength("Matcap 1 Strength", Range(0, 2)) = 1.0
		[Enum(MatcapBlendModes)]_Matcap1Blend("Matcap 1 Blend Mode", Float) = 0.0
		[Space]_Matcap2("Matcap 2", 2D) = "black" {}
		_Matcap2Strength("Matcap 2 Strength", Range(0, 2)) = 1.0
		[Enum(MatcapBlendModes)]_Matcap2Blend("Matcap 2 Blend Mode", Float) = 0.0
		[Space]_Matcap3("Matcap 3", 2D) = "black" {}
		_Matcap3Strength("Matcap 3 Strength", Range(0, 2)) = 1.0
		[Enum(MatcapBlendModes)]_Matcap3Blend("Matcap 3 Blend Mode", Float) = 0.0
		[Space]_Matcap4("Matcap 4", 2D) = "black" {}
		_Matcap4Strength("Matcap 4 Strength", Range(0, 2)) = 1.0
		[Enum(MatcapBlendModes)]_Matcap4Blend("Matcap 4 Blend Mode", Float) = 0.0
		[Space]
		[Header(Detail)]
		[Toggle(_DETAIL_MULX2)]_UseDetailMaps("Enable Detail Maps", Float ) = 0.0
		_DetailAlbedoMap ("Detail Albedo Map", 2D) = "gray" {}
		_DetailAlbedoMapScale ("Detail Albedo Map Scale", Float) = 1.0
		_DetailNormalMap("Detail Normal Map", 2D) = "bump" {}
		_DetailNormalMapScale("Detail Normal Map Scale", Float) = 1.0
		_SpecularDetailMask ("Specular Detail Mask", 2D) = "white" {}
		_SpecularDetailStrength ("Specular Detail Strength", Range(0, 1)) = 1.0
		_DetailEmissionMap("Emission Map", 2D) = "white" {}
		[HDR]_EmissionDetailParams("Emission Detail Params", Vector) = (0,0,0,0)
		[Enum(UV0,0,UV1,1)]_UVSec ("UV Set Secondary", Float) = 0
		[Space]
		[Header(Subsurface Scattering)]
		[Toggle(_)]_UseSubsurfaceScattering ("Use Subsurface Scattering", Float) = 0.0
		_ThicknessMap("Thickness Map", 2D) = "black" {}
		[Toggle(_)]_ThicknessMapInvert("Invert Thickness", Float) = 0.0
		_ThicknessMapPower ("Thickness Map Power", Range(0.01, 10)) = 1
		_SSSCol ("Scattering Color", Color) = (1,1,1,1)
		_SSSIntensity ("Scattering Intensity", Range(0, 10)) = 1
		_SSSPow ("Scattering Power", Range(0.01, 10)) = 1
		_SSSDist ("Scattering Distance", Range(0, 10)) = 1
		_SSSAmbient ("Scattering Ambient", Range(0, 1)) = 0
		[Space]
		[Header(Other)]
		[Toggle(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A)]_AlbedoAlphaMode("Albedo Alpha Mode", Float) = 0.0
		[HDR]_CustomFresnelColor("Emissive Fresnel Color", Color) = (0,0,0,1)
		[Toggle(_)]_PixelSampleMode("Sharp Sampling Mode", Float) = 0.0
		[Space]
		[Header(System Lighting)]
		[Enum(LightingCalculationType)] _LightingCalculationType ("Lighting Calculation Type", Float) = 0.0
		_LightSkew ("Light Skew", Vector) = (1, 0.1, 1, 0)
		[Space]
		[Header(System Internal)]
		[ToggleOff(_SPECULARHIGHLIGHTS_OFF)]_SpecularHighlights ("Specular Highlights", Float) = 1.0
		[ToggleOff(_GLOSSYREFLECTIONS_OFF)]_GlossyReflections ("Glossy Reflections", Float) = 1.0
		[Space]
        // Advanced options.
		[Header(System Render Flags)]
        [Enum(RenderingMode)] _Mode("Rendering Mode", Float) = 0                                     // "Opaque"
        [Enum(CustomRenderingMode)] _CustomMode("Mode", Float) = 0                                   // "Opaque"
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Source Blend", Float) = 1                 // "One"
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Destination Blend", Float) = 0            // "Zero"
        [Enum(UnityEngine.Rendering.BlendOp)] _BlendOp("Blend Operation", Float) = 0                 // "Add"
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("Depth Test", Float) = 4                // "LessEqual"
        [Enum(DepthWrite)] _ZWrite("Depth Write", Float) = 1                                         // "On"
        [Enum(UnityEngine.Rendering.ColorWriteMask)] _ColorWriteMask("Color Write Mask", Float) = 15 // "All"
        [Enum(UnityEngine.Rendering.CullMode)] _CullMode("Cull Mode", Float) = 2                     // "Back"
        _RenderQueueOverride("Render Queue Override", Range(-1.0, 5000)) = -1
		[Space]
        // Stencil options.
		[Header(System Stencil Flags)]
	    [IntRange] _Stencil ("Stencil ID [0;255]", Range(0,255)) = 0
	    _ReadMask ("ReadMask [0;255]", Int) = 255
	    _WriteMask ("WriteMask [0;255]", Int) = 255
	    [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comparison", Int) = 0
	    [Enum(UnityEngine.Rendering.StencilOp)] _StencilOp ("Stencil Operation", Int) = 0
	    [Enum(UnityEngine.Rendering.StencilOp)] _StencilFail ("Stencil Fail", Int) = 0
	    [Enum(UnityEngine.Rendering.StencilOp)] _StencilZFail ("Stencil ZFail", Int) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue"="AlphaTest" "RenderType" = "TransparentCutout" "IgnoreProjector"="True"
		}

        Blend[_SrcBlend][_DstBlend]
        BlendOp[_BlendOp]
        ZTest[_ZTest]
        ZWrite[_ZWrite]
        Cull[_CullMode]
        ColorMask[_ColorWriteMask]

        Stencil
        {
            Ref [_Stencil]
            ReadMask [_ReadMask]
            WriteMask [_WriteMask]
            Comp [_StencilComp]
            Pass [_StencilOp]
            Fail [_StencilFail]
            ZFail [_StencilZFail]
        }

		Pass
		{

			Name "FORWARD"
			Tags { "LightMode" = "ForwardBase" }
			
            AlphaToMask On

			CGPROGRAM

			#define UNITY_PASS_FORWARDBASE
			#pragma multi_compile _ VERTEXLIGHT_ON
			#pragma multi_compile ___ UNITY_HDR_ON

			#pragma multi_compile_fwdbase
			#pragma multi_compile_fog

			#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#pragma shader_feature ___ _DETAIL_MULX2
			#pragma shader_feature ___ _METALLICGLOSSMAP _SPECGLOSSMAP
			#pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
			#pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
			#pragma shader_feature _ _GLOSSYREFLECTIONS_OFF
			
			#include "SCSS_Core.cginc"

			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag

			#include "SCSS_Forward.cginc"

			ENDCG
		}


		Pass
		{
			Name "FORWARD_DELTA"
			Tags { "LightMode" = "ForwardAdd" }
			
            AlphaToMask On
			Blend [_SrcBlend] One

			CGPROGRAM

			#define UNITY_PASS_FORWARDADD

			#pragma multi_compile_fwdadd_fullshadows
			#pragma multi_compile_fog

			#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#pragma shader_feature ___ _DETAIL_MULX2
			#pragma shader_feature ___ _METALLICGLOSSMAP _SPECGLOSSMAP
			#pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
			#pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
			#pragma shader_feature _ _GLOSSYREFLECTIONS_OFF

			#include "SCSS_Core.cginc"

			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag

			#include "SCSS_Forward.cginc"

			ENDCG
		}

		Pass
		{
			Name "SHADOW_CASTER"
			Tags{ "LightMode" = "ShadowCaster" }

            Blend[_SrcBlend][_DstBlend]
            BlendOp[_BlendOp]
            ZTest[_ZTest]
            ZWrite[_ZWrite]
            Cull[_CullMode]
            ColorMask[_ColorWriteMask]

			CGPROGRAM
			#define UNITY_PASS_SHADOWCASTER
			
			#pragma multi_compile_shadowcaster

			#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			
			#include "SCSS_Shadows.cginc"

			#pragma vertex vertShadowCaster
			#pragma fragment fragShadowCaster
			ENDCG
		}
	}
	FallBack "Diffuse"
	CustomEditor "SilentCelShading.Unity.Inspector"
}