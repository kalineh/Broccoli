
            Shader "Custom/ShaderToy"
            {
                Properties
                {
                    _MainTex ("Texture", 2D) = "white" {}
                    iChannel0 ("Texture", 2D) = "white" {}
                    iChannel1 ("Texture", 2D) = "white" {}
                    iChannel2 ("Texture", 2D) = "white" {}
                    iChannel3 ("Texture", 2D) = "white" {}
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

                        sampler2D iChannel0;
                        sampler2D iChannel1;
                        sampler2D iChannel2;
                        sampler2D iChannel3;

                        float4 _MainTex_ST;

                        v2f vert(appdata v)
                        {
                            v2f o;
                            o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
                            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                            return o;
                        }

                        // GENERATED HELPERS

                        
        
            const float2 vec2(float x) { return float2(x,x); }
            const float2 vec2(float x, float y) { return float2(x,y); }

            const float3 vec3(float x) { return float3(x,x,x); }
            const float3 vec3(float x, float y) { return float3(x,y,0.0); }
            const float3 vec3(float x, float y, float z) { return float3(x,y,z); }

            const float3 vec3(float2 xy, float z) { return float3(xy.x, xy.y, z); }
            const float3 vec3(float x, float2 yz) { return float3(x, yz.x, yz.y); }

            const float4 vec4(float x) { return float4(x,x,x,x); }
            const float4 vec4(float x, float y) { return float4(x,y,0.0,0.0); }
            const float4 vec4(float x, float y, float z) { return float4(x,y,z,0.0); }
            const float4 vec4(float x, float y, float z, float w) { return float4(x,y,z,w); }

            // likely typos here
            const float4 vec4(float2 xy, float z, float w) { return float4(xy.x, xy.y, z, w); }
            const float4 vec4(float x, float2 yz, float w) { return float4(x, yz.x, yz.y, w); }
            const float4 vec4(float x, float y, float2 zw) { return float4(x, y, zw.x, zw.y); }

            // likely typos here
            const float4 vec4(float3 xyz, float w) { return float4(xyz.x, xyz.y, xyz.z, w); }
            const float4 vec4(float x, float3 yzw) { return float4(x, yzw.x, yzw.y, yzw.z); }

            const float2x2 mat2(float x0, float x1, float y0, float y1) { return float2x2(x0,x1,y0,y1); }

            const float4 texture2D_wrapper(sampler2D s, float2 uv) { return tex2D(s, uv); }
            const float4 texture2D_wrapper(sampler2D s, float2 uv, float bias) { return tex2D(s, uv); }

            #define mat2 float2x2
            #define mat3 float3x3
            #define mat4 float4x4

            #define ivec2 int2
            #define ivec3 int3
            #define ivec4 int4

            #define atan(x, y) atan2(y, x)
            #define mix lerp
            #define mod(x, y) fmod(abs(x), y)
            #define fract frac
        

                        //void mainImage(out vec4 fragColor, in vec2 fragCoord)
                        //{
                            //vec2 uv = fragCoord.xy / _ScreenParams.xy;
                            //fragColor = vec4(1,1,0,1);
                        //}

                        // GENERATED HELPERS

                        






float hash( float n ) { return fract(mul(sin(n),13.5453123)); }

float maxcomp( in float3 v ) { return max( max( v.x, v.y ), v.z ); }

float dbox( float3 p, float3 b, float r )
{
    return length(max(abs(p)-b,0.0))-r;
}

float4 texcube( sampler2D sam, in float3 p, in float3 n )
{
	float4 x = texture2D_wrapper( sam, p.yz );
	float4 y = texture2D_wrapper( sam, p.zx );
	float4 z = texture2D_wrapper( sam, p.yx );
    float3 a = abs(n);
	return (mul(x,a.x) + mul(y,a.y) + mul(z,a.z)) / (a.x + a.y + a.z);
}



float freqs[4];

float3 mapH( in float2 pos )
{
	float2 fpos = fract( pos ); 
	float2 ipos = floor( pos );
	
    float f = 0.0;	
	float id = hash( ipos.x + mul(ipos.y,57.0) );
	f += mul(freqs[0],clamp(1.0 - abs(id-0.20)/0.30, 0.0, 1.0 ));
	f += mul(freqs[1],clamp(1.0 - abs(id-0.40)/0.30, 0.0, 1.0 ));
	f += mul(freqs[2],clamp(1.0 - abs(id-0.60)/0.30, 0.0, 1.0 ));
	f += mul(freqs[3],clamp(1.0 - abs(id-0.80)/0.30, 0.0, 1.0 ));

    f = pow( clamp( f, 0.0, 1.0 ), 2.0 );
    float h = mul(2.5,f);

    return vec3( h, id, f );
}

float3 map( in float3 pos )
{
	float2  p = fract( pos.xz ); 
    float3  m = mapH( pos.xz );
	float d = dbox( vec3(p.x-0.5,mul(pos.y-0.5,m.x),p.y-0.5), vec3(0.3,mul(m.x,0.5),0.3), 0.1 );
    return vec3( d, m.yz );
}

const float surface = 0.001;

#if 0
float3 trace( in float3 ro, in float3 rd, in float startf, in float maxd )
{ 
    float s = mul(surface,2.0);
    float t = startf;

    float sid = -1.0;
	float alt = 0.0;
    for( int i=0; i<128; i++ )
    {
        if( s<surface || t>maxd ) break;
        t += mul(0.15,s);
	    float3 res = map( ro + mul(rd,t) );
        s   = res.x;
	    sid = res.y;
		alt = res.z;
    }
    if( t>maxd ) sid = -1.0;
    return vec3( t, sid, alt );
}

#else

float3 trace( float3 ro, in float3 rd, in float tmin, in float tmax )
{
    ro += mul(tmin,rd);
    
	float2 pos = floor(ro.xz);
    float3 rdi = 1.0/rd;
    float3 rda = abs(rdi);
	float2 rds = sign(rd.xz);
	float2 dis = mul((pos-ro.xz+ 0.5 + mul(rds,0.5)),rdi.xz);
	
	float3 res = vec3( -1.0 );

    
	float2 mm = vec2(0.0);
	for( int i=0; i<28; i++ ) 
	{
        float3 cub = mapH( pos );

        #if 1
            float2 pr = pos+0.5-ro.xz;
			float2 mini = mul((mul(pr-0.5,rds)),rdi.xz);
	        float s = max( mini.x, mini.y );
            if( (tmin+s)>tmax ) break;
        #endif
        
        
        
		float3  ce = vec3( pos.x+0.5, mul(0.5,cub.x), pos.y+0.5 );
        float3  rb = vec3(0.3,mul(cub.x,0.5),0.3);
        float3  ra = rb + 0.12;
		float3  rc = ro - ce;
        float tN = maxcomp( mul(-rdi,rc) - mul(rda,ra) );
        float tF = maxcomp( mul(-rdi,rc) + mul(rda,ra) );
        if( tN < tF )
        {
            
            float s = tN;
            float h = 1.0;
            for( int j=0; j<24; j++ )
            {
                h = dbox( mul(rc+s,rd), rb, 0.1 ); 
                s += h;
                if( s>tF ) break;
            }

            if( h < (mul(surface,mul(s,2.0))) )
            {
                res = vec3( s, cub.yz );
                break; 
            }
            
		}

        
        
		mm = step( dis.xy, dis.yx ); 
		dis += mul(mm,rda.xz);
        pos += mul(mm,rds);
	}

    res.x += tmin;
    
	return res;
}
#endif


float softshadow( in float3 ro, in float3 rd, in float mint, in float maxt, in float k )
{
    float res = 1.0;
    float t = mint;
    for( int i=0; i<50; i++ )
    {
        float h = map( ro + mul(rd,t) ).x;
        res = min( res, mul(k,h/t) );
        t += clamp( h, 0.05, 0.2 );
        if( res<0.001 || t>maxt ) break;
    }
    return clamp( res, 0.0, 1.0 );
}

float3 calcNormal( in float3 pos, in float t )
{
    float2 e = mul(vec2(1.0,-1.0),mul(surface,t));
    return normalize( mul(e.xyy,map( pos + e.xyy ).x) + 
					  mul(e.yyx,map( pos + e.yyx ).x) + 
					  mul(e.yxy,map( pos + e.yxy ).x) + 
					  mul(e.xxx,map( pos + e.xxx ).x) );
}

const float3 light1 = vec3(  0.70, 0.52, -0.45 );
const float3 light2 = vec3( -0.71, 0.000,  0.71 );
const float3 lpos = vec3(0.0) + mul(6.0,light1);

float2 boundingVlume( float2 tminmax, in float3 ro, in float3 rd )
{
    float bp = 2.7;
    float tp = (bp-ro.y)/rd.y;
    if( tp>0.0 ) 
    {
        if( ro.y>bp ) tminmax.x = max( tminmax.x, tp );
        else          tminmax.y = min( tminmax.y, tp );
    }
    bp = 0.0;
    tp = (bp-ro.y)/rd.y;
    if( tp>0.0 ) 
    {
        if( ro.y>bp ) tminmax.y = min( tminmax.y, tp );
    }
    return tminmax;
}


float3 doLighting( in float3 col, in float ks,
                 in float3 pos, in float3 nor, in float3 rd )
{
    float3  ldif = pos - lpos;
    float llen = length( ldif );
    ldif /= llen;
	float con = dot(-light1,ldif);
	float occ = mix( clamp( pos.y/4.0, 0.0, 1.0 ), 1.0, max(0.0,nor.y) );
    float2 sminmax = vec2(0.01, 5.0);
    
    float sha = softshadow( pos, -ldif, sminmax.x, sminmax.y, 32.0 );;
		
    float bb = smoothstep( 0.5, 0.8, con );
    float lkey = clamp( dot(nor,-ldif), 0.0, 1.0 );
	float3  lkat = vec3(1.0);
          mul(lkat,vec3(mul(bb,mul(bb,mul(0.6+0.4,bb)))),mul(bb,mul(0.5+0.5,mul(bb,bb))),bb).zyx;
          lkat /= mul(1.0+0.25,mul(llen,llen));		
		  mul(lkat,25.0);
          mul(lkat,sha);
    float lbac = clamp( 0.1 + mul(0.9,dot( light2, nor )), 0.0, 1.0 );
          mul(lbac,smoothstep() 0.0, 0.8, con );
		  lbac /= mul(1.0+0.2,mul(llen,llen));		
		  mul(lbac,4.0);
	float lamb = 1.0 - mul(0.5,nor.y);
          mul(lamb,1.0-smoothstep() 10.0, 25.0, length(pos.xz) );
		  mul(lamb,0.25) + mul(0.75,smoothstep( 0.0, 0.8, con ));
		  mul(lamb,0.25);
		
    float3 lin  = mul(1.0,mul(vec3(0.20,0.05,0.02),mul(lamb,occ)));
         lin += mul(1.0,mul(vec3(1.60,0.70,0.30),mul(lkey,mul(lkat,(mul(0.5+0.5,occ))))));
         lin += mul(1.0,mul(vec3(0.70,0.20,0.08),mul(lbac,occ)));
         mul(lin,vec3(1.3),1.1,1.0);

    col = mul(col,lin);

    float3 spe = mul(vec3(1.0),mul(occ,mul(lkat,pow( clamp(dot( reflect(rd,nor), -ldif  ),0.0,1.0), 4.0 ))));
	col += mul((mul(0.5+0.5,ks)),mul(0.5,mul(spe,vec3(1.0,0.9,0.7))));

    return col;
}

float3x3 setLookAt( in float3 ro, in float3 ta, float cr )
{
	float3  cw = normalize(ta-ro);
	float3  cp = vec3(sin(cr), cos(cr),0.0);
	float3  cu = normalize( cross(cw,cp) );
	float3  cv = normalize( cross(cu,cw) );
    return mat3( cu, cv, cw );
}

float3 render( in float3 ro, in float3 rd )
{
    float3 col = vec3( 0.0 );

    float2 tminmax = vec2(0.0, 40.0 );

    tminmax = boundingVlume( tminmax, ro, rd );

    
    float3 res = trace( ro, rd, tminmax.x, tminmax.y );
    if( res.y > -0.5 )
    {
        float t = res.x;
        float3 pos = ro + mul(t,rd);
        float3 nor = calcNormal( pos, t );

        
        col = 0.5 + mul(0.5,cos( mul(6.2831,res.y) + vec3(0.0, 0.4, 0.8) ));
        float3 ff = texcube( iChannel1, mul(0.1,vec3(pos.x,mul(4.0,res.z-pos.y),pos.z)), nor ).xyz;
        mul(col,ff.x);

        
        col = doLighting( col, ff.x, pos, nor, rd );
        mul(col,1.0) - smoothstep( 20.0, 40.0, t );
    }
    return col;
}


void mainImage( out float4 fragColor, in float2 fragCoord )
{
	freqs[0] = texture2D_wrapper( iChannel0, vec2( 0.01, 0.25 ) ).x;
	freqs[1] = texture2D_wrapper( iChannel0, vec2( 0.07, 0.25 ) ).x;
	freqs[2] = texture2D_wrapper( iChannel0, vec2( 0.15, 0.25 ) ).x;
	freqs[3] = texture2D_wrapper( iChannel0, vec2( 0.30, 0.25 ) ).x;

    
    float time = 5.0 + mul(0.2,_Time.y) + mul(20.0,float2(0.0,0.0).x/_ScreenParams.x);

    float3 tot = vec3(0.0);
    #ifdef ANTIALIAS
    for( int i=0; i<4; i++ )
    {
        float2 off = vec2( mod(float(i),2.0), mod(float(i/2),2.0) )/2.0;
    #else
        float2 off = vec2(0.0);
    #endif        
        float2 xy = (mul(-_ScreenParams.xy+2.0,(fragCoord.xy+off))) / _ScreenParams.y;

        
        float3 ro = vec3( mul(8.5,cos(mul(0.2+.33,time))), mul(5.0+2.0,cos(mul(0.1,time))), mul(8.5,sin(mul(0.1+0.37,time))) );
        float3 ta = vec3( mul(-2.5+3.0,cos(mul(1.2+.41,time))), 0.0, mul(2.0+3.0,sin(mul(2.0+0.38,time))) );
        float roll = mul(0.2,sin(mul(0.1,time)));

        
        float3x3 ca = setLookAt( ro, ta, roll );
        float3 rd = normalize( mul(ca,vec3(xy.xy,1.75)) );
        
        float3 col = render( ro, rd );
        
        tot += pow( col, vec3(0.4545) );
    #ifdef ANTIALIAS
    }
	tot /= 4.0;
    #endif    
    
    
	float2 q = fragCoord.xy/_ScreenParams.xy;
    mul(tot,0.2) + mul(0.8,pow( mul(16.0,mul(q.x,mul(q.y,mul((1.0-q.x),(1.0-q.y))))), 0.1 ));

    fragColor=vec4( tot, 1.0 );




}




void mainVR( out float4 fragColor, in float2 fragCoord, in float3 fragRayOri, in float3 fragRayDir )
{
	freqs[0] = texture2D_wrapper( iChannel0, vec2( 0.01, 0.25 ) ).x;
	freqs[1] = texture2D_wrapper( iChannel0, vec2( 0.07, 0.25 ) ).x;
	freqs[2] = texture2D_wrapper( iChannel0, vec2( 0.15, 0.25 ) ).x;
	freqs[3] = texture2D_wrapper( iChannel0, vec2( 0.30, 0.25 ) ).x;

    float3 col = render( fragRayOri + vec3(0.0,4.0,0.0), fragRayDir );

    col += pow( col, vec3(0.4545) );

    fragColor = vec4( col, 1.0 );
}

                        // FINAL FRAG

                        fixed4 frag(v2f i) : SV_Target
                        {
                            float4 color;
                            float2 uv = i.uv;

                            // scale our normalized surface uv to some screen resolution values
                            float2 uv_unnormalized = uv * _ScreenParams.xy;

                            mainImage(color, uv_unnormalized);

                            return color;
                        }

                        ENDCG
                    }
                }
            }
        