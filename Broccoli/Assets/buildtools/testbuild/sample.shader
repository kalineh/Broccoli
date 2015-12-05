Shader "Unlit/ShaderToyManualConvertShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
            float4 _MainTex_ST;
            

            #define vec2 float2
            #define vec3 float3
            #define vec4 float4

            #define mat2 float2x2
            #define mat3 float3x3
            #define mat4 float4x4

            #define Texture2D(a,b,c) Tex2D(a,b)

            #define atan(x, y) atan2(y, x)
            #define mix lerp

            void mainImage( out vec4 fragColor, in vec2 fragCoord )
            {
                vec2 uv = fragCoord.xy / _ScreenParams.xy;
                fragColor = vec4(1,1,0,1);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                //fixed4 col = tex2D(_MainTex, i.uv);
                //// apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);             
                //col.x = 1;
                //col.z = 0;
                //return col;

                float4 color;
                float2 uv;

                mainImage(color, uv);

                return color;   
            }


            ENDCG
        }
    }
}