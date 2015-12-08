
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

                        
        
            #define vec2 float2
            #define vec3 float3
            #define vec3(x) float3(x,x,x)
            #define vec3(x,y,z) float3(x,y,z)
            //float3 vec3(float x) { return float3(x, x, x); }
            //float3 vec3(float x, float y, float z) { return float3(x, y, z); }
            #define vec3 float3
            #define vec4 float4

            #define mat2 float2x2
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

                        // Star Nest by Pablo Román Andrioli

// This content is under the MIT License.

#define iterations 17
#define formuparam 0.53

#define volsteps 20
#define stepsize 0.1

#define zoom   0.800
#define tile   0.850
#define speed  0.010 

#define brightness 0.0015
#define darkmatter 0.300
#define distfading 0.730
#define saturation 0.850


void mainImage( out vec4 fragColor, in vec2 fragCoord )
{
fragColor = vec4(1,1,1,1);
	//get coords and direction
	vec2 uv=fragCoord.xy/_ScreenParams.xy-.5;
	uv.y*=_ScreenParams.y/_ScreenParams.x;
	vec3 dir=vec3(uv*zoom,1.);
	float time=_Time.y*speed+.25;

	//mouse rotation
	float a1=.5+float2(0.0,0.0).x/_ScreenParams.x*2.;
	float a2=.8+float2(0.0,0.0).y/_ScreenParams.y*2.;
	mat2 rot1=mat2(cos(a1),sin(a1),-sin(a1),cos(a1));
	mat2 rot2=mat2(cos(a2),sin(a2),-sin(a2),cos(a2));
	// error
	//dir.xz*=rot1;
	//dir.xy*=rot2;
	dir.xz=mul(dir.xz, rot1);
	dir.xy=mul(dir.xz, rot2);
	vec3 from=vec3(1.,.5,0.5);
	from+=vec3(time*2.,time,-2.);
	// error
	//from.xz*=rot1;
	//from.xy*=rot2;
	from.xz=mul(from.xz, rot1);
	from.xy=mul(from.xz, rot2);
	
	//volumetric rendering
	//float s=0.1,fade=1.0;
	vec3 v=vec3(0.0);
	for (int r=0; r<volsteps; r++) {
	/*
		vec3 p=from+s*dir*.5;
		p = abs(vec3(tile)-mod(p,vec3(tile*2.))); // tiling fold
		float pa,a=pa=0.;
		for (int i=0; i<iterations; i++) { 
			p=abs(p)/dot(p,p)-formuparam; // the magic formula
			a+=abs(length(p)-pa); // absolute sum of average change
			pa=length(p);
		}
		float dm=max(0.,darkmatter-a*a*.001); //dark matter
		a*=a*a; // add contrast
		if (r>6) fade*=1.-dm; // dark matter, don't render near
		//v+=vec3(dm,dm*.5,0.);
		v+=fade;
		v+=vec3(s,s*s,s*s*s*s)*a*brightness*fade; // coloring based on distance
		fade*=distfading; // distance fading
		s+=stepsize;
	*/
	}
	//v=mix(vec3(length(v),0.0,0.0),v,saturation); //color adjust
	//v=mix(vec3(1.0,0.0,0.0),v,saturation); //color adjust
	/*
	fragColor = vec4(v*.01,1.);	
	*/
	
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
        