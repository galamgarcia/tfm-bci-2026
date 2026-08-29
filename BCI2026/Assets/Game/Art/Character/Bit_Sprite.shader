Shader "bit/spriteunlit"
{
    Properties
    {
        _Color ("color", Color) = (1, 1, 1, 1)
        _EmissionStrength ("emission strength", Range(0, 1)) = 0
        _EdgeColor ("edge color", Color) = (1, 1, 1, 1)
        _EdgeStrength ("edge strength", Range(0, 1)) = 0
        _EdgeWidth ("edge width", Range(0, 0.1)) = 0.02
        _GlowIntensity ("glow intensity", Range(0, 8)) = 0
        _CornerRadius ("corner radius", Range(0, 0.25)) = 0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct attributes
            {
                float4 position : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct varyings
            {
                float4 position : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            half4 _Color;
            half _EmissionStrength;
            half4 _EdgeColor;
            half _EdgeStrength;
            half _EdgeWidth;
            half _GlowIntensity;
            half _CornerRadius;

            varyings vert(attributes input)
            {
                varyings output;
                output.position = TransformObjectToHClip(input.position.xyz);
                output.color = input.color * _Color;
                output.uv = input.uv;
                return output;
            }

            half4 frag(varyings input) : SV_Target
            {
                float2 offset = abs(input.uv - 0.5);
                float2 box = offset - (0.5 - _CornerRadius);
                float dist = length(max(box, 0)) + min(max(box.x, box.y), 0) - _CornerRadius;
                float softness = max(fwidth(dist), 0.0001);
                half alpha = 1 - smoothstep(0, softness, dist);
                half edge = smoothstep(-_EdgeWidth, 0, dist);
                half4 color = input.color * (1 + _EmissionStrength);
                color = lerp(color, color + _EdgeColor, edge * _EdgeStrength);
                color.rgb += _EdgeColor.rgb * edge * _GlowIntensity;
                color.a *= alpha;
                return color;
            }
            ENDHLSL
        }
    }
}
