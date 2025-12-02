// ChatGPT generated shader besides the biome index system
Shader "Custom/BiomeBlendShaderURP"
{
    Properties
    {
        _PlainsTex("Plains Texture", 2D) = "white" {}
        _OceanTex("Ocean Texture", 2D) = "white" {}
        _MountainTex("Mountain Texture", 2D) = "white" {}
        _TaigaTex("Taiga Texture", 2D) = "white" {}
        _WaterRocksTex("Water Rocks Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // Textures
            TEXTURE2D(_PlainsTex);      SAMPLER(sampler_PlainsTex);
            TEXTURE2D(_OceanTex);       SAMPLER(sampler_OceanTex);
            TEXTURE2D(_MountainTex);    SAMPLER(sampler_MountainTex);
            TEXTURE2D(_TaigaTex);       SAMPLER(sampler_TaigaTex);
            TEXTURE2D(_WaterRocksTex);  SAMPLER(sampler_WaterRocksTex);
            TEXTURE2D(_LavaCastTex);    SAMPLER(sampler_LavaCastTex);
            TEXTURE2D(_TundraTex);      SAMPLER(sampler_TundraTex);
            TEXTURE2D(_DesertTex);      SAMPLER(sampler_DesertTex);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;     // biome index in vertex color
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float4 color       : COLOR;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                int biomeIndex = (int)(IN.color.r * 255.0);

                float3 col;
                if (biomeIndex == 0) // WaterRocks
                    col = SAMPLE_TEXTURE2D(_WaterRocksTex, sampler_WaterRocksTex, IN.uv).rgb;
                else if (biomeIndex == 1) // Ocean
                    col = SAMPLE_TEXTURE2D(_OceanTex, sampler_OceanTex, IN.uv).rgb;
                else if (biomeIndex == 2) // Plains
                    col = SAMPLE_TEXTURE2D(_PlainsTex, sampler_PlainsTex, IN.uv).rgb;
                else if (biomeIndex == 3) // Taiga
                    col = SAMPLE_TEXTURE2D(_TaigaTex, sampler_TaigaTex, IN.uv).rgb;
                else if (biomeIndex == 4) // LavaCast
                    col = SAMPLE_TEXTURE2D(_LavaCastTex, sampler_LavaCastTex, IN.uv).rgb;
                else if (biomeIndex == 5) // Mountain
                    col = SAMPLE_TEXTURE2D(_MountainTex, sampler_MountainTex, IN.uv).rgb;
                else if (biomeIndex == 6) // Tundra
                    col = SAMPLE_TEXTURE2D(_TundraTex, sampler_TundraTex, IN.uv).rgb;
                else if (biomeIndex == 7) // Desert
                    col = SAMPLE_TEXTURE2D(_DesertTex, sampler_DesertTex, IN.uv).rgb;
                else
                    col = SAMPLE_TEXTURE2D(_PlainsTex, sampler_PlainsTex, IN.uv).rgb; // Fallback

                return half4(col, 1.0);
            }
            ENDHLSL
        }
    }
}
