// Bildschirm-Verlauf fuer die Wassertiefe: oben heller tuerkiser Schleier, unten dunkler.
// Sitzt als Quad direkt vor der Kamera und wird zuletzt gezeichnet.
Shader "ConcreteLife/DepthGradient"
{
    Properties
    {
        _TopColor ("Oben (heller Schleier)", Color) = (0.55, 0.85, 0.8, 0.18)
        _BottomColor ("Unten (Tiefe)", Color) = (0.02, 0.07, 0.07, 0.55)
        _TopStart ("Schleier ab (0..1)", Range(0,1)) = 0.55
        _BottomEnd ("Dunkel bis (0..1)", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+200" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            Name "Unlit"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _TopColor;
                half4 _BottomColor;
                float _TopStart;
                float _BottomEnd;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings  { float4 positionCS : SV_POSITION; float4 screenPos : TEXCOORD0; };

            Varyings vert (Attributes i)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
                o.screenPos = ComputeScreenPos(o.positionCS);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float y = i.screenPos.y / i.screenPos.w;
                half top = smoothstep(_TopStart, 1.0, y) * _TopColor.a;
                half bottom = (1 - smoothstep(0.0, _BottomEnd, y)) * _BottomColor.a;
                half a = saturate(top + bottom);
                half3 c = (top * _TopColor.rgb + bottom * _BottomColor.rgb) / max(top + bottom, 1e-4);
                return half4(c, a);
            }
            ENDHLSL
        }
    }
}
