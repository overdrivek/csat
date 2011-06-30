[VERTEX]
#ifdef SHADOWS
uniform mat4 glTextureMatrix;
uniform mat4 glProjectionMatrix;
uniform mat4 glModelViewMatrix;
uniform mat4 glNormalMatrix;
attribute vec4 glVertex;
attribute vec4 glNormal;
attribute vec2 glTexCoord;
varying vec4 vNormal;
varying vec2 vUV;
varying vec4 vUV2;

void main()
{
	vUV = glTexCoord;
	vUV2 = glTextureMatrix * glVertex;
	vNormal = glNormalMatrix * glNormal;
	gl_Position = glProjectionMatrix * glModelViewMatrix * glVertex;
}
#endif

[FRAGMENT]
#ifdef SHADOWS
uniform sampler2D textureMap;
uniform sampler2D lightmaskMap;
uniform sampler2DShadow depthMap;
uniform vec4 materialDiffuse;
uniform vec3 glLight;
varying vec4 vNormal;
varying vec2 vUV;
varying vec4 vUV2;

void main()
{
	vec3 N = normalize(vNormal.xyz);
	vec3 L = normalize(glLight);

	vec4 lm = texture2DProj(lightmaskMap, vUV2);
	float shadow = shadow2DProj(depthMap, vUV2).r;
	if(shadow<0.1) shadow=0.1;
	gl_FragColor = texture2D(textureMap, vUV) * materialDiffuse * lm * shadow * max(dot(N, L), 0.0);
}
#endif
