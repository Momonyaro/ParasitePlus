Shader "Unlit/CombatBackShader"
{
    Properties
    {
        [Header(General Properties)]
        _ModuloX ("Modulo Split X", int) = 8
        _ModuloY ("Modulo Split Y", int) = 8
        
        [Header(Pattern A Properties)]
        _MainTex ("Pattern A", 2D) = "white" {}
        _MainColLow ("Low Color", COLOR) =   (0,0,0,1)
        _MainColHigh ("High Color", COLOR) = (1,1,1,1)
        
        [Header(Sin Settings)]
        [MaterialToggle] _SinTogglePrimary ("Use Sin", float) = 0
        _SinScalePrimary  ("Sin Scale", float) = 0
        _SinFreqPrimary   ("Sin Freq", float) = 0
        _SinAmpPrimary   ("Sin Amp", float) = 0
        
        [Header(Pattern B Properties)]
        _SecondTex ("Pattern B", 2D) = "white" {}
        _SecondColLow ("Low Color", COLOR) =   (0,0,0,1)
        _SecondColHigh ("High Color", COLOR) = (1,1,1,1)
        
        [Header(Sin Settings)]
        [MaterialToggle] _SinToggleSecondary ("Use Sin", float) = 0
        _SinScaleSecondary  ("Sin Scale", float) = 0
        _SinFreqSecondary   ("Sin Freq", float) = 0
        _SinAmpSecondary   ("Sin Amp", float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _SecondTex;
            fixed4 _MainColLow;
            fixed4 _MainColHigh;
            fixed4 _SecondColLow;
            fixed4 _SecondColHigh;
            
            float _SinScalePrimary;
            float _SinFreqPrimary;
            float _SinAmpPrimary;
            int _SinTogglePrimary;
            
            float _SinScaleSecondary;
            float _SinFreqSecondary;
            float _SinAmpSecondary;
            int _SinToggleSecondary;

            int _ModuloX;
            int _ModuloY;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            v2f sinWave(float amp, float freq, float scale, v2f i)
            {
                float sinX = sin((freq * i.uv.y) + (scale * _Time)); // Range : -1 - 1
                sinX *= 0.5; // Range : -.5 - .5
                sinX *= 0.5; // Range : -.25 - .25
                //sinX += 0.5; // Range : 0.25 - 0.75
                sinX *= amp;
                i.uv += float2(sinX, 1.); // I'm fucking stupid, jesus christ... Of course addition would work.

                return i;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                v2f u = i;
                v2f v = i;

                if (_SinTogglePrimary == 1) u = sinWave(_SinAmpPrimary, _SinFreqPrimary, _SinScalePrimary, u);
                if (_SinToggleSecondary == 1) v = sinWave(_SinAmpSecondary,_SinScaleSecondary, _SinFreqSecondary, v);
                
                // sample the texture
                fixed4 aCol = tex2D(_MainTex, u.uv);
                aCol = lerp(_MainColLow, _MainColHigh, aCol);
                fixed4 bCol = tex2D(_SecondTex, v.uv);
                bCol = lerp(_SecondColLow, _SecondColHigh, bCol);
                
                fixed4 col = (aCol + bCol) * 0.5;
                int modulatedX = (i.uv.x * _ModuloX) % 2;
                int modulatedY = (i.uv.y * _ModuloY) % 2;
                float finalMod = round((modulatedX + modulatedY) % 2);

                col = lerp(aCol, bCol, finalMod);
                
                return col;
            }
            ENDCG
        }
    }
}
