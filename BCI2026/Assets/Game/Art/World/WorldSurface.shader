/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

Shader "Bit/WorldSurface"
{
    Properties
    {
        _BaseColor ("base color", Color) = (0, 0, 0, 1)
        _EmissionColor ("emission color", Color) = (0, 1, 1, 1)
        _EmissionStrength ("emission strength", Range(0, 8)) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "WorldSurface"
            Tags { "LightMode" = "UniversalForward" }
            Cull Off

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
            };

            half4 _BaseColor;
            half4 _EmissionColor;
            half _EmissionStrength;

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.position = TransformObjectToHClip(input.position.xyz);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return _BaseColor + (_EmissionColor * _EmissionStrength);
            }
            ENDHLSL
        }
    }
}
