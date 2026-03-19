Shader "Custom/URP/ShopZone_Pulse"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0,1,1,1)
        _EmissionColor ("Emission Color", Color) = (0,1,1,1)

        _MainTex ("Texture", 2D) = "white" {}

        _MinOpacity ("Min Opacity", Range(0,1)) = 0.2
        _MaxOpacity ("Max Opacity", Range(0,1)) = 0.8
        _PulseSpeed ("Pulse Speed", Float) = 2

        _EmissionStrength ("Emission Strength", Float) = 2
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off   // ← двухсторонний

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _BaseColor;
            float4 _EmissionColor;
            float _MinOpacity;
            float _MaxOpacity;
            float _PulseSpeed;
            float _EmissionStrength;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                // Пульсация от 0 до 1
                float pulse = (sin(_Time.y * _PulseSpeed) * 0.5 + 0.5);

                // Интерполяция между min и max
                float opacity = lerp(_MinOpacity, _MaxOpacity, pulse);

                float3 baseCol = tex.rgb * _BaseColor.rgb;
                float alpha = tex.a * opacity * _BaseColor.a;

                float3 emission = tex.rgb * _EmissionColor.rgb * _EmissionStrength;

                return float4(baseCol + emission, alpha);
            }

            ENDHLSL
        }
    }
}

