
            Shader "Custom/ShaderToy"
            {
                Properties
                {
                    _MainTex ("Texture",  2D) = "white" {}
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
                        
                        #include "UnityCG.cginc"

                        struct appdata
                        {
                            float4 vertex : POSITION;
                            float2 uv : TEXCOORD0;
                        };

                        struct v2f
                        {
                            float2 uv : TEXCOORD0;
                            float4 vertex : SV_POSITION;
                        };

                        sampler2D _MainTex;
                        float4 _MainTex_ST;

                        v2f vert(appdata v)
                        {
                            v2f o;
                            o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
                            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                            return o;
                        }

                        // GENERATED HELPERS

                        
        
            float2 vec2(float x) { return float2(x,x); }
            float2 vec2(float x, float y) { return float2(x,y); }

            float3 vec3(float x) { return float3(x,x,x); }
            float3 vec3(float x, float y) { return float3(x,y,0.0); }
            float3 vec3(float x, float y, float z) { return float3(x,y,z); }

            float4 vec4(float x) { return float4(x,x,x,x); }
            float4 vec4(float x, float y) { return float4(x,y,0.0,0.0); }
            float4 vec4(float x, float y, float z) { return float4(x,y,z,0.0); }
            float4 vec4(float x, float y, float z, float w) { return float4(x,y,z,w); }

            float2x2 mat2(float x0, float x1, float y0, float y1) { return float2x2(x0,x1,y0,y1); }

            #define mat3 float3x3
            #define mat4 float4x4

            #define Texture2D(a,b,c) Tex2D(a,b)

            #define atan(x, y) atan2(y, x)
            #define mix lerp
            #define mod fmod
            #define fract frac
        

                        //void mainImage(out vec4 fragColor, in vec2 fragCoord)
                        //{
                            //vec2 uv = fragCoord.xy / _ScreenParams.xy;
                            //fragColor = vec4(1,1,0,1);
                        //}

                        // GENERATED HELPERS

                        void mainImage( out float4 fragColor, in float2 fragCoord )
{
	float2 p = (2.0*fragCoord.xy-_ScreenParams.xy)/_ScreenParams.y;
    float tau = 3.1415926535*2.0;
    float a = atan(p.x,p.y);
    float r = length(p)*0.75;
    float2 uv = vec2(a/tau,r);
	
	//get the color
	float xCol = (uv.x - (_Time.y / 3.0)) * 3.0;
	xCol = mod(xCol, 3.0);
	float3 horColour = vec3(0.25, 0.25, 0.25);
	
	if (xCol < 1.0) {
		
		horColour.r += 1.0 - xCol;
		horColour.g += xCol;
	}
	else if (xCol < 2.0) {
		
		xCol -= 1.0;
		horColour.g += 1.0 - xCol;
		horColour.b += xCol;
	}
	else {
		
		xCol -= 2.0;
		horColour.b += 1.0 - xCol;
		horColour.r += xCol;
	}

	// draw color beam
	uv = (2.0 * uv) - 1.0;
	float beamWidth = (0.7+0.5*cos(uv.x*10.0*tau*0.15*clamp(floor(5.0 + 10.0*cos(_Time.y)), 0.0, 10.0))) * abs(1.0 / (30.0 * uv.y));
	float3 horBeam = vec3(beamWidth);
	fragColor = vec4((( horBeam) * horColour), 1.0);
}

                        // FINAL FRAG

                        fixed4 frag(v2f i) : SV_Target
                        {
                            float4 color;
                            float2 uv = i.uv;

                            mainImage(color, uv);

                            return color;
                        }

                        ENDCG
                    }
                }
            }
        