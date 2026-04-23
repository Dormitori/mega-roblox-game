Shader "Game/GuideLine/ArrowsURP"
{
    Properties
    {
        _BaseColor ("Arrow Color", Color) = (0.2, 1.0, 1.0, 1.0)
        _BackgroundColor ("Background Color", Color) = (0.0, 0.0, 0.0, 0.0)
        _ArrowSize ("Arrow Length", Range(0.1, 1.0)) = 0.75
        _ArrowWidth ("Arrow Width", Range(0.05, 1.0)) = 0.38
        _ArrowHeight ("Arrow Height", Range(0.05, 4.0)) = 0.5
        _EdgeSoftness ("Edge Softness", Range(0.001, 0.2)) = 0.03
        _ScrollSpeed ("Scroll Speed", Range(-20, 20)) = 4
        _Direction ("Arrow Facing (1 or -1)", Float) = -1
        _Opacity ("Opacity", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float4 color       : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _BackgroundColor;
                float _ArrowSize;
                float _ArrowWidth;
                float _ArrowHeight;
                float _EdgeSoftness;
                float _ScrollSpeed;
                float _Direction;
                float _Opacity;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            float ArrowMask(float2 uv)
            {
                float arrowHeight = max(_ArrowHeight, 0.0001);
                float x = frac((uv.x + _Time.y * _ScrollSpeed) / arrowHeight);
                x = _Direction >= 0.0 ? x : (1.0 - x);
                float y = abs(uv.y - 0.5) * 2.0;

                float arrowBody = saturate(1.0 - x / max(_ArrowSize, 0.0001));
                float widthAtX = arrowBody * _ArrowWidth;

                float verticalMask = smoothstep(widthAtX + _EdgeSoftness, widthAtX - _EdgeSoftness, y);
                float horizontalMask = smoothstep(_ArrowSize, _ArrowSize - _EdgeSoftness, x);
                return saturate(verticalMask * horizontalMask);
            }

            float4 Frag(Varyings input) : SV_Target
            {
                float mask = ArrowMask(input.uv);
                float4 arrowColor = _BaseColor * input.color;
                float4 bgColor = _BackgroundColor * input.color;

                float4 finalColor = lerp(bgColor, arrowColor, mask);
                finalColor.a *= _Opacity;
                return finalColor;
            }
            ENDHLSL
        }
    }
}
