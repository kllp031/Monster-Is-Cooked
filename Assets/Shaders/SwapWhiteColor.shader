// Swaps the hue of red pixels (the knight's scarf) to the player's assigned color.
// Saturation and brightness are preserved so shading looks natural.
Shader "Custom/SwapWhiteColor"
{
    Properties
    {
        _MainTex       ("Sprite Texture", 2D)           = "white" {}
        _PlayerColor   ("Player Color",   Color)        = (0, 0.8, 1, 1)
        _HueTolerance  ("Hue Tolerance",  Range(0, 0.3)) = 0.08
        _MinSaturation ("Min Saturation", Range(0, 1))   = 0.4
    }

    SubShader
    {
        Tags
        {
            "Queue"             = "Transparent"
            "IgnoreProjector"   = "True"
            "RenderType"        = "Transparent"
            "PreviewType"       = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull     Off
        Lighting Off
        ZWrite   Off
        Blend    SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color  : COLOR;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color  : COLOR;
                float2 uv     : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4    _PlayerColor;
            float     _HueTolerance;
            float     _MinSaturation;

            // ── HSV helpers ──────────────────────────────────────────────────

            float3 RGBtoHSV(float3 c)
            {
                float4 K = float4(0.0, -1.0/3.0, 2.0/3.0, -1.0);
                float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
                float4 q = lerp(float4(p.xyw, c.r),  float4(c.r, p.yzx), step(p.x, c.r));
                float  d = q.x - min(q.w, q.y);
                float  e = 1e-10;
                return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }

            float3 HSVtoRGB(float3 c)
            {
                float4 K = float4(1.0, 2.0/3.0, 1.0/3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
            }

            // ─────────────────────────────────────────────────────────────────

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv     = v.uv;
                o.color  = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col *= i.color;         // vertex color (hurt flash, etc.)
                clip(col.a - 0.01);

                float3 hsv = RGBtoHSV(col.rgb);

                // Circular distance from red hue (0). Red wraps at both 0 and 1.
                float redDist = min(hsv.x, 1.0 - hsv.x);

                // 1 for saturated-red pixels, 0 otherwise
                float isScarf = smoothstep(_HueTolerance, 0.0, redDist)
                              * step(_MinSaturation, hsv.y);

                // Hue of the player's assigned color
                float playerHue = RGBtoHSV(_PlayerColor.rgb).x;

                // Replace hue; keep saturation and brightness intact
                hsv.x = lerp(hsv.x, playerHue, isScarf);
                col.rgb = HSVtoRGB(hsv);

                return col;
            }
            ENDCG
        }
    }
}
