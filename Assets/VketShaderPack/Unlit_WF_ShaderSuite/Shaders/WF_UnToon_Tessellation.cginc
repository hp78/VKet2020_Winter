﻿/*
 *  The MIT License
 *
 *  Copyright 2018-2020 whiteflare.
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
 *  to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
 *  and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 *  IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 *  TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

#ifndef INC_UNLIT_WF_UNTOON_TESSELLATION
#define INC_UNLIT_WF_UNTOON_TESSELLATION

    /*
     * authors:
     *      ver:2020/07/06 whiteflare,
     */

    #include "WF_UnToon.cginc"
    #include "Tessellation.cginc"

    struct HsConstantOutput {
        float tessFact[3]    : SV_TessFactor;
        float insideTessFact : SV_InsideTessFactor;
    };

    #define _TESS_MIN_DIST 0
    #define _TESS_MAX_DIST 2

    float       _TessType;
    float       _TessFactor;
    float       _Smoothing;
    sampler2D   _DispMap;   // vert内で取得するので独自のサンプラーを使う
    float       _DispMapScale;
    float       _DispMapLevel;

    [domain("tri")]
    [partitioning("integer")]
    [outputtopology("triangle_cw")]
    [patchconstantfunc("hullConst")]
    [outputcontrolpoints(3)]
    v2f hull(InputPatch<v2f, 3> i, uint id : SV_OutputControlPointID) {
        return i[id];
    }

    float4 worldDistanceBasedTess(float3 ws_vertex, float minDist, float maxDist, float tess) {
        float dist = distance(ws_vertex, worldSpaceViewPointPos());
        float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
        return UnityCalcTriEdgeTessFactors(f.xxx);
    }

    float4 worldEdgeLengthBasedTess(float3 ws_vertex0, float3 ws_vertex1, float3 ws_vertex2, float edgeLength) {
        float4 tess;
        tess.x = UnityCalcEdgeTessFactor(ws_vertex1, ws_vertex2, edgeLength);
        tess.y = UnityCalcEdgeTessFactor(ws_vertex2, ws_vertex0, edgeLength);
        tess.z = UnityCalcEdgeTessFactor(ws_vertex0, ws_vertex1, edgeLength);
        tess.w = (tess.x + tess.y + tess.z) / 3.0f;
        return tess;
    }

    HsConstantOutput hullConst(InputPatch<v2f, 3> i) {
        UNITY_SETUP_INSTANCE_ID(i[0]);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i[0]);

        // 2～16 の値域をもつ _TessFactor から tessFactor を計算する
        float4 tessFactor =
            // DISTANCE
            _TessType < 0.5 ? worldDistanceBasedTess(calcWorldSpaceBasePos(i[0].ws_vertex), _TESS_MIN_DIST, _TESS_MAX_DIST, _TessFactor)
                // EDGE_LENGTH
                : _TessType < 1.5 ? worldEdgeLengthBasedTess(i[0].ws_vertex, i[1].ws_vertex, i[2].ws_vertex, 64 / _TessFactor)
                // FIXED
                : _TessFactor.xxxx;

        HsConstantOutput o = (HsConstantOutput) 0;
        o.tessFact[0] = tessFactor.x;
        o.tessFact[1] = tessFactor.y;
        o.tessFact[2] = tessFactor.z;
        o.insideTessFact = tessFactor.w;

        return o;
    }

    v2f domainCore(HsConstantOutput hsConst, const OutputPatch<v2f, 3> i, float3 bary) {
        v2f o = i[0];
        UNITY_SETUP_INSTANCE_ID(o);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(o);

        #define MUL_BARY(array, member)   (bary.x * array[0].member + bary.y * array[1].member + bary.z * array[2].member)

        o.ws_vertex     = MUL_BARY(i, ws_vertex);
        o.uv            = MUL_BARY(i, uv);
        o.uv_lmap       = MUL_BARY(i, uv_lmap);
        o.normal        = normalize( MUL_BARY(i, normal) );
        #ifdef _NM_ENABLE
            o.tangent   = normalize( MUL_BARY(i, tangent) );
            o.bitangent = normalize( MUL_BARY(i, bitangent) );
        #endif

        // Phong Tessellation
        float3 phg[3];
        for (int a = 0; a < 3; a++) {
            phg[a] = i[a].normal * (dot( i[a].ws_vertex.xyz, i[a].normal ) - dot(o.ws_vertex.xyz, i[a].normal));
        }
        o.ws_vertex.xyz += MUL_BARY(phg, xyz) * _Smoothing / 2.0;

        // Displacement HeightMap
        float2 uv_main = TRANSFORM_TEX(o.uv, _MainTex);
        float disp = SAMPLE_MASK_VALUE_LOD(_DispMap, float4(uv_main, 0, 0), 0).r * _DispMapScale - _DispMapLevel;
        o.ws_vertex.xyz += o.normal * disp * 0.01;

        #undef MUL_BARY

        return o;
    }

    [domain("tri")]
    v2f domain(HsConstantOutput hsConst, const OutputPatch<v2f, 3> i, float3 bary : SV_DomainLocation) {
        v2f o = domainCore(hsConst, i, bary);
        o.vs_vertex = UnityWorldToClipPos(o.ws_vertex.xyz);
        return o;
    }

    [domain("tri")]
    v2f domain_outline(HsConstantOutput hsConst, const OutputPatch<v2f, 3> i, float3 bary : SV_DomainLocation) {
        v2f o = domainCore(hsConst, i, bary);
        // SV_POSITION を上書き
        o.vs_vertex = shiftOutlineVertex(o);

        return o;
    }

    [domain("tri")]
    v2f domain_emissiveScroll(HsConstantOutput hsConst, const OutputPatch<v2f, 3> i, float3 bary : SV_DomainLocation) {
        v2f o = domainCore(hsConst, i, bary);
        // SV_POSITION を上書き
        o.vs_vertex = shiftEmissiveScrollVertex(o);

        return o;
    }

#endif
