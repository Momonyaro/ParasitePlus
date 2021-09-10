Shader "Unlit/TransitionSlideTopBottom"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _FillColor ("Fill Color", Color) = (0,0,0,1)
        _TintB ("Tint B", Color) = (1,0,0,1)
        _TintA ("Tint A", Color) = (0.65, 0, 1, 1)
        _FillAmount ("Fill Amount", Range(0, 1)) = 0.5
        _TintSize ("Tint Size", Range(0, 0.25)) = 0.25
        
        [Header(Honestly just ignore this)]
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
 
        _ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Blend SrcAlpha OneMinusSrcAlpha
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
            fixed4 _FillColor;
            fixed4 _TintA;
            fixed4 _TintB;
            float _FillAmount;
            float _TintSize;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //Basically from fillAmount to 1, render the fillcolor & the two tints

                fixed4 col = _FillColor;
                fixed4 tintA = _TintA;
                fixed4 tintB = _TintB;

                float mainSplit = step(1.0 - _FillAmount, i.uv.y);
                //return fixed4(mainSplit, mainSplit, mainSplit, mainSplit);
                float tintBSplit = step(i.uv.y, 1.0 - (_FillAmount - (_TintSize * 2.5 * (1.0 - _FillAmount)))) * mainSplit;
                //return fixed4(tintBSplit, tintBSplit, tintBSplit, tintBSplit);
                float tintASplit = step(i.uv.y, 1.0 - (_FillAmount - (_TintSize * (1.0 - _FillAmount)))) * mainSplit;

                fixed4 exportCol = lerp(fixed4(0, 0, 0, 0), col, mainSplit);
                       exportCol = lerp(exportCol, tintB, tintBSplit);
                       exportCol = lerp(exportCol, tintA, tintASplit);
                
                
                return exportCol;
            }
            ENDCG
        }
    }
}
