// ChatGPT generated, with tweaks for perlin noise, instead of instead of a texture-based approach.
Shader "Custom/BiomeSkyboxPerlinFBM"
{
    Properties
    {
        _SkyColor ("Sky Color", Color) = (0.3,0.6,1,1)
        _FogColor ("Fog Color", Color) = (0.5,0.5,0.5,1)
        _CloudColor ("Cloud Color", Color) = (1,1,1,1)

        _CloudAmount ("Cloud Amount", Range(0,1)) = 0.45
        _CloudScale ("Cloud Scale", Float) = 0.0003       // lower freq
        _CloudSpeed ("Cloud Speed", Float) = 0.01
        _WarpStrength ("Warp Strength", Float) = 0.8
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Opaque" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; };
            struct v2f {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            float4 _SkyColor, _FogColor, _CloudColor;
            float _CloudAmount, _CloudScale, _CloudSpeed, _WarpStrength;

            // ----------------------------------------------------------
            // Value noise (same as before)
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1,311.7))) * 43758.5453);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                float a = hash(i);
                float b = hash(i + float2(1,0));
                float c = hash(i + float2(0,1));
                float d = hash(i + float2(1,1));

                float2 u = f*f*(3.0 - 2.0*f);

                return lerp(a, b, u.x) +
                       (c - a) * u.y * (1.0 - u.x) +
                       (d - b) * u.x * u.y;
            }

            // ----------------------------------------------------------
            // fBm (only 3 octaves for smoother shapes)
            float fbm(float2 p)
            {
                float value = 0.0;
                float amp = 0.5;
                float freq = 1.0;

                for (int i = 0; i < 3; i++)
                {
                    value += amp * noise(p * freq);
                    freq *= 2.0;
                    amp *= 0.5;
                }
                return value;
            }

            // ----------------------------------------------------------
            // Domain warping (creates clumps instead of dots!)
            float warpedFBM(float2 p)
            {
                float2 warp;
                warp.x = fbm(p + float2(5.2, 1.3));
                warp.y = fbm(p + float2(1.7, 9.2));

                p += (warp - 0.5) * _WarpStrength;

                return fbm(p);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = _SkyColor;

                // Fog near horizon
                float fogFactor = saturate(1 - normalize(i.worldPos).y);
                col = lerp(col, _FogColor, fogFactor);

                // ------------------------------------------------------
                // CLOUD UV + animation
                float2 uv = i.worldPos.xz * _CloudScale;
                uv += float2(_Time.y * _CloudSpeed, 0);

                // ------------------------------------------------------
                // big smooth cloud formations!
                float n = warpedFBM(uv);

                // softer cloud blending
                float mask = smoothstep(_CloudAmount - 0.1, _CloudAmount + 0.1, n);

                col = lerp(col, _CloudColor, mask);

                return col;
            }
            ENDCG
        }
    }
}
