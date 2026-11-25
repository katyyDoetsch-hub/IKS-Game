// Wasseroberflaeche von unten gesehen.
// - Snellsches Fenster: steil nach oben sieht man den hellen Himmel, flacher wird die Oberflaeche
//   zum dunkleren Spiegel (Totalreflexion).
// - Sonne als heller Fleck mit weichem Hof, leicht von den Wellen verzerrt.
Shader "ConcreteLife/WaterSurface"
{
    Properties
    {
        _RippleTex ("Wellen-Rauschen", 2D) = "gray" {}
        [HDR] _ShallowColor ("Himmel dunkel", Color) = (0.5, 0.8, 0.75, 1)
        [HDR] _BrightColor ("Himmel hell (Wellenkaemme)", Color) = (1.6, 2.2, 2.0, 1)
        [HDR] _ReflectColor ("Spiegel (Totalreflexion)", Color) = (0.16, 0.3, 0.27, 1)
        _WindowCos ("Fensterrand (cos)", Range(0,1)) = 0.66
        _SunDir ("Richtung zur Sonne", Vector) = (0.1, 0.7, 0.7, 0)
        [HDR] _SunColor ("Sonnenfarbe", Color) = (6, 6, 5, 1)
        _Scale ("Wellen-Groesse", Float) = 0.06
        _Speed ("Geschwindigkeit", Float) = 0.02
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalPipeline" }
        Cull Off

        Pass
        {
            Name "Unlit"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_RippleTex); SAMPLER(sampler_RippleTex);
            CBUFFER_START(UnityPerMaterial)
                float4 _RippleTex_ST;
                half4 _ShallowColor;
                half4 _BrightColor;
                half4 _ReflectColor;
                float _WindowCos;
                float4 _SunDir;
                half4 _SunColor;
                float _Scale;
                float _Speed;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings  { float4 positionCS : SV_POSITION; float3 ws : TEXCOORD0; float fog : TEXCOORD1; };

            Varyings vert (Attributes i)
            {
                Varyings o;
                o.ws = TransformObjectToWorld(i.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.ws);
                o.fog = ComputeFogFactor(o.positionCS.z);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float2 uv = i.ws.xz * _Scale;
                float t = _Time.y * _Speed;
                half n1 = SAMPLE_TEXTURE2D(_RippleTex, sampler_RippleTex, uv + float2(t, t * 0.6)).r;
                // zweite Ebene gedreht und mit "krummem" Massstab -> kein sichtbares Kachelmuster
                float2 uv2 = float2(uv.x * 0.8 - uv.y * 0.6, uv.x * 0.6 + uv.y * 0.8) * 1.37;
                half n2 = SAMPLE_TEXTURE2D(_RippleTex, sampler_RippleTex, uv2 + float2(-t * 0.7, t * 0.4)).r;
                half w = smoothstep(0.38, 0.78, (n1 + n2) * 0.5);

                float3 V = normalize(i.ws - _WorldSpaceCameraPos);
                // Wellen verzerren den Blick leicht
                V = normalize(V + float3(n1 - 0.5, 0, n2 - 0.5) * 0.06);

                half inWindow = smoothstep(_WindowCos - 0.05, _WindowCos + 0.06, V.y);
                half3 sky = lerp(_ShallowColor.rgb, _BrightColor.rgb, w);
                half3 mirror = _ReflectColor.rgb * (0.8 + 0.5 * w);
                half3 c = lerp(mirror, sky, inWindow);

                float sd = saturate(dot(V, normalize(_SunDir.xyz)));
                c += _SunColor.rgb * (pow(sd, 300) * 4.0 + pow(sd, 60) * 0.5) * inWindow;

                c = MixFog(c, i.fog);
                return half4(c, 1);
            }
            ENDHLSL
        }

        // Schreibt die Oberflaeche ins Tiefenbild (sonst "sieht" die Tiefenunschaerfe den Balken dahinter
        // und es entsteht eine harte Linie an dessen Oberkante).
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            ZWrite On
            ColorMask R
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings  { float4 positionCS : SV_POSITION; };

            Varyings vert (Attributes i)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
                return o;
            }

            half frag (Varyings i) : SV_Target { return 0; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode"="DepthNormals" }
            ZWrite On
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings  { float4 positionCS : SV_POSITION; };

            Varyings vert (Attributes i)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
                return o;
            }

            half4 frag (Varyings i) : SV_Target { return half4(0, -1, 0, 0); }
            ENDHLSL
        }
    }
}
