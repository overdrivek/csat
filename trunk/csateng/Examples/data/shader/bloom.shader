[VERTEX]
uniform mat4 glProjectionMatrix;
uniform mat4 glModelViewMatrix;
attribute vec4 glVertex;
attribute vec2 glTexCoord;
varying vec2 vUV;

void main()
{
	vUV = glTexCoord;
	gl_Position = glProjectionMatrix * glModelViewMatrix * glVertex;
}

[FRAGMENT]
// from http://daxnitro.wikia.com/wiki/Alternative_Shaders_(Shaders)

uniform sampler2D textureMap;
uniform float size = 3.0;
varying vec2 vUV;

const float BRIGHT_PASS_THRESHOLD = 0.5;
const float BRIGHT_PASS_OFFSET = 0.5;
float contrast = 1.05;

//Bloom
#define blurclamp 0.002 
#define bias 0.01


vec2 texcoord = vec2(vUV).st;
vec4 texcolor = texture2D(textureMap, vUV);

vec4 bright(vec2 coo)
{
	vec4 color = texture2D(textureMap, coo);
	
	color = max(color - BRIGHT_PASS_THRESHOLD, 0.0);
	
	return color / (color + BRIGHT_PASS_OFFSET);	
}

//Cross Processing
vec4 gradient(vec4 coo)
{
	vec4 stripes = coo;
	stripes.r =  stripes.r*1.3+0.01;
	stripes.g = stripes.g*1.2;
	stripes.b = stripes.b*0.7+0.15;
	stripes.a = texcolor.a;
	return stripes;
}

void main(void)
{
	vec2 blur = vec2(clamp( bias, -blurclamp, blurclamp ));
	
	vec4 col = vec4( 0, 0, 0, 0 );
	for ( float x = -size + 1.0; x < size; x += 1.0 )
	{
		for ( float y = -size + 1.0; y < size; y += 1.0 )
		{
			 col += bright( texcoord + vec2( blur.x * x, blur.y * y ) );
		}
	}
	col /= ((size+size)-1.0)*((size+size)-1.0);
	vec4 value = texture2D(textureMap, texcoord);
	vec4 fin = col + gradient(value);
	
	gl_FragColor = (fin - 0.5) * contrast + 0.5;
}
