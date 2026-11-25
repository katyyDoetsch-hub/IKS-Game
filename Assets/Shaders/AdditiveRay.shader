// Weicher, additiver Lichtstrahl (God Ray) fuer Unterwasser-Stimmung.
Shader "ConcreteLife/AdditiveRay"
{
    Properties
    {
        _BaseMap ("Textur", 2D) = "white" {}
        [HDR] _Color ("Farbe", Color) = (1,1,1,1)
        _FadeDistance ("Weiche Kante (m)", Float) = 2.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }
        Blend One One
        ZWrite Off
        Cull Off

        Pass
        {
            Name "Unlit"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _Color;
                float _FadeDistance;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };
            struct Varyings  { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; float fog : TEXCOORD1; float4 screenPos : TEXCOORD2; float eyeDepth : TEXCOORD3; };

            Varyings vert (Attributes i)
            {
                Varyings o;
                float3 ws = TransformObjectToWorld(i.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(ws);
                o.uv = TRANSFORM_TEX(i.uv, _BaseMap);
                o.color = i.color;
                o.fog = ComputeFogFactor(o.positionCS.z);
                o.screenPos = ComputeScreenPos(o.positionCS);
                o.eyeDepth = -TransformWorldToView(ws).z;
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                half4 t = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _Color * i.color;
                // weich ausblenden, wo der Strahl in Mauer/Boden eintaucht und nah an der Kamera
                float2 suv = i.screenPos.xy / i.screenPos.w;
                float sceneDepth = LinearEyeDepth(SampleSceneDepth(suv), _ZBufferParams);
                float fade = saturate((sceneDepth - i.eyeDepth) / max(_FadeDistance, 0.01));
                fade *= saturate((i.eyeDepth - _ProjectionParams.y) / 1.5);
                half3 c = t.rgb * t.a * fade;
                c = MixFogColor(c, half3(0,0,0), i.fog);
                return half4(c, 1);
            }
            ENDHLSL
        }
    }
}
