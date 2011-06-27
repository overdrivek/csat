// http://www.joeforte.net/projects/soft-particles/
// flags:
//   -none-  uses default rendering (no lighting)
//   SOFT    uses softparticles (init with Particles.SetSoftParticles)

[VERTEX]
uniform mat4 glProjectionMatrix;
uniform mat4 glModelViewMatrix;
attribute vec4 glVertex;
attribute vec2 glTexCoord;
varying vec2 vUV;

#ifdef SOFT
varying float vDepth; // Projection space vDepth information (before divide)
varying vec2 vPos; // Poition information of the pixels
#endif

void main()
{
	vUV = glTexCoord;
	gl_Position = glProjectionMatrix * glModelViewMatrix * glVertex;

#ifdef SOFT
	// Transforms the vPosition data to the range [0,1]
	vPos = (gl_Position.xy / gl_Position.w + 1.0) / 2.0;
	vDepth = gl_Position.w;
#endif

}

[FRAGMENT]
uniform sampler2D textureMap;
uniform vec4 materialDiffuse;
varying vec2 vUV;

#ifdef SOFT
uniform sampler2D depthMap;
uniform float power;
varying float vDepth; // Projection space vDepth information (before divide)
varying vec2 vPos; // Poition information of the pixels
float Contrast(float d)
{
	float val = clamp( 2.0*( (d > 0.5) ? 1.0-d : d ), 0.0, 1.0);
	float a = 0.5 * pow(val, power);
	return (d > 0.5) ? 1.0 - a : a;
}
#endif

void main()
{
	vec4 c;

#ifdef SOFT
	float d = texture2D(depthMap, vPos.xy).x; // Scene vDepth
	c = texture2D(textureMap, vUV.st);
	c.a = c.a * Contrast(d - vDepth); // Computes alpha based on the particles distance to the rest of the scene
#else
	c = texture2D(textureMap, vUV.xy);
	if(c.a < 0.1) discard;
#endif

	gl_FragColor = c * materialDiffuse;
}
