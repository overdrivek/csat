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
#endif

void main()
{
	vec4 c;

#ifdef SOFT
	float d = texture2D(depthMap, vPos).x; // Scene depth
	c = texture2D(textureMap, vUV) * clamp((d - vDepth)*power, 0.0, 1.0);
#else
	c = texture2D(textureMap, vUV);
	if(c.a == 0.0) discard;
#endif

	gl_FragColor = c * materialDiffuse;
}
