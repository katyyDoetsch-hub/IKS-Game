// Unterwasser-Ansicht als Bildschirm-Effekt (Quad direkt vor der Kamera, zuletzt gezeichnet).
// - Dunst nur fuer den Teil der Sichtlinie, der UNTER Wasser liegt. Ueber Wasser bleibt alles klar.
//   Dadurch funktioniert auch das geteilte Bild beim Auftauchen (oben klar, unten Wasser).
// - Tiefenverlauf: oben heller Schleier, unten dunkler (nur unter Wasser).
// - Feine helle Wasserlinie, wenn die Kamera genau auf Hoehe der Oberflaeche ist.
Shader "ConcreteLife/UnderwaterView"
{
    Properties
    {
        _FogColor ("Wasserfarbe (Dunst)", Color) = (0.25, 0.38, 0.32, 1)
        _Density ("Dunst-Dichte", Float) = 0.06
        _WaterLevel ("Wasserhoehe", Float) = 7.6
        _TopColor ("Verlauf oben (heller Schleier)", Color) = (0.55, 0.85, 0.8, 0.16)
        _BottomColor ("Verlauf unten (Tiefe)", Color) = (0.02, 0.07, 0.07, 0.5)
        _TopStart ("Schleier ab (0..1)", Range(0,1)) = 0.55
        _BottomEnd ("Dunkel bis (0..1)", Range(0,1)) = 0.5
        [HDR] _LineColor ("Wasserlinie", Color) = (1.2, 1.5, 1.4, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+150" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _FogColor;
                float _Density;
                float _WaterLevel;
                half4 _TopColor;
                half4 _BottomColor;
                float _TopStart;
                float _BottomEnd;
                half4 _LineColor;
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

            // a ueber b legen (normales Alpha-Blending), Ergebnis als vormultiplizierte Farbe + Alpha
            void Layer(inout half3 premul, inout half a, half3 col, half alpha)
            {
                premul = col * alpha + premul * (1 - alpha);
                a = alpha + a * (1 - alpha);
            }

            half4 frag (Varyings i) : SV_Target
            {
                float2 uv = i.screenPos.xy / i.screenPos.w;
                float raw = SampleSceneDepth(uv);
                float3 P = ComputeWorldSpacePosition(uv, raw, UNITY_MATRIX_I_VP);
                float3 C = _WorldSpaceCameraPos;
                float3 d = P - C;
                float len = length(d);
                float W = _WaterLevel;

                // Laenge der Sichtlinie unter Wasser
                float under;
                if (C.y <= W && P.y <= W) under = len;
                else if (C.y > W && P.y > W) under = 0;
                else
                {
                    float t = saturate((W - C.y) / (P.y - C.y));
                    under = (C.y <= W) ? len * t : len * (1 - t);
                }
                float frac = len > 1e-4 ? under / len : (C.y <= W ? 1 : 0);

                half3 premul = 0;
                half a = 0;

                // 1) Dunst
                half fog = 1 - exp(-_Density * under);
                Layer(premul, a, _FogColor.rgb, fog);

                // 2) Tiefenverlauf (nur unter Wasser)
                half top = smoothstep(_TopStart, 1.0, uv.y) * _TopColor.a * frac;
                half bottom = (1 - smoothstep(0.0, _BottomEnd, uv.y)) * _BottomColor.a * frac;
                Layer(premul, a, _BottomColor.rgb, bottom);
                Layer(premul, a, _TopColor.rgb, top);

                // 3) Wasserlinie, wenn die Kamera auf Hoehe der Oberflaeche ist
                float3 V = d / max(len, 1e-4);
                half nearSurface = saturate(1 - abs(C.y - W) / 0.25);
                half lineMask = exp(-pow(V.y / 0.006, 2)) * nearSurface;
                Layer(premul, a, _LineColor.rgb, lineMask * 0.7);

                half3 c = premul / max(a, 1e-4);
                return half4(c, a);
            }
            ENDHLSL
        }
    }
}
