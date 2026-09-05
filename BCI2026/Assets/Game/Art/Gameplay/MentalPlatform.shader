/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

Shader "Bit/MentalPlatform"
{
    Properties
    {
        _BaseColor ("base color", Color) = (1, 0.19, 0.35, 1)
        _EmissionColor ("emission color", Color) = (1, 0.19, 0.35, 1)
        _GlowIntensity ("glow intensity", Range(0, 8)) = 1.2
        _ActiveAmount ("active amount", Range(0, 1)) = 0
        _IsOutline ("is outline", Float) = 0
        _DashAxis ("dash axis", Float) = 0
        _DashFrequency ("dash frequency", Float) = 12
        _DashGap ("dash gap", Range(0, 1)) = 0.45
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "MentalPlatform"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 position : POSITION;
            };

            struct Varyings
            {
                float4 position : SV_POSITION;
                float3 localPosition : TEXCOORD0;
            };

            half4 _BaseColor;
            half4 _EmissionColor;
            half _GlowIntensity;
            half _ActiveAmount;
            half _IsOutline;
            half _DashAxis;
            half _DashFrequency;
            half _DashGap;

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.position = TransformObjectToHClip(input.position.xyz);
                output.localPosition = input.position.xyz;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half coordinate = lerp(input.localPosition.x, input.localPosition.y, _DashAxis) + 0.5h;
                half dash = step(_DashGap, frac(coordinate * _DashFrequency));
                half continuous = step(0.99h, _ActiveAmount);
                half outlineAlpha = lerp(dash, 1.0h, continuous);
                half alpha = lerp(_ActiveAmount, outlineAlpha, _IsOutline);
                half4 color = _BaseColor + (_EmissionColor * _GlowIntensity * _IsOutline);
                return half4(color.rgb, alpha);
            }
            ENDHLSL
        }
    }
}
