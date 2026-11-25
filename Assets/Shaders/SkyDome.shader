// Einfacher Himmel fuer die Sicht ueber Wasser (klarer Tag): Verlauf Horizont -> Zenit + Sonne.
Shader "ConcreteLife/SkyDome"
{
    Properties
    {
        _HorizonColor ("Horizont", Color) = (0.78, 0.87, 0.93, 1)
        _ZenithColor ("Zenit", Color) = (0.32, 0.56, 0.88, 1)
        _SunDir ("Richtung zur Sonne", Vector) = (0.1, 0.7, 0.7, 0)
        [HDR] _SunColor ("Sonne", Color) = (8, 7.5, 6.5, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+500" "RenderPipeline"="UniversalPipeline" }
        Cull Front
        ZWrite On

        Pass
        {
            Name "Unlit"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _HorizonColor;
                half4 _ZenithColor;
                float4 _SunDir;
                half4 _SunColor;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings  { float4 positionCS : SV_POSITION; float3 ws : TEXCOORD0; };

            Varyings vert (Attributes i)
            {
                Varyings o;
                o.ws = TransformObjectToWorld(i.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.ws);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float3 V = normalize(i.ws - _WorldSpaceCameraPos);
                half h = saturate(V.y);
                half3 c = lerp(_HorizonColor.rgb, _ZenithColor.rgb, pow(h, 0.6));
                float sd = saturate(dot(V, normalize(_SunDir.xyz)));
                c += _SunColor.rgb * (pow(sd, 800) * 3 + pow(sd, 40) * 0.08);
                return half4(c, 1);
            }
            ENDHLSL
        }
    }
}
