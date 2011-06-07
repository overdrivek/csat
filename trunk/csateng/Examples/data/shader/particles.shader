// by Joe Forte - http://www.joeforte.net/projects/soft-particles/
[VERTEX]
// Projection space depth information (before divide)
varying float Depth;

// Poition information of the pixels
varying vec2 pos;

void main()
{
	gl_TexCoord[0] = gl_MultiTexCoord0;
	gl_FrontColor = gl_Color;
	gl_Position = ftransform();
	
	// Transforms the position data to the range [0,1]
	pos = (gl_Position.xy / gl_Position.w + 1.0) / 2.0;
	
	Depth = gl_Position.w;
}


[FRAGMENT]
uniform sampler2D textureMap;
uniform sampler2D depthMap;

// Power used in the contrast function
uniform float power;

// Projection space depth information (before divide)
varying float Depth;

// Position of the pixel
varying vec2 pos;

float Contrast(float d)
{
	float val = clamp( 2.0*( (d > 0.5) ? 1.0-d : d ), 0.0, 1.0);
	float a = 0.5 * pow(val, power);
	return (d > 0.5) ? 1.0 - a : a;
}

void main()
{
	float d = texture2D(depthMap, pos.xy).x; // Scene depth
	vec4 c = texture2D(textureMap, gl_TexCoord[0].st) * gl_Color;
	
	// Computes alpha based on the particles distance to the rest of the scene
	c.a = c.a * Contrast(d - Depth);
	
	gl_FragColor = c;
}
