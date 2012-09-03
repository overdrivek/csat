// flags:
//   -none-		only texturing
//   LIGHTING	simple shading
//   PERPIXEL	better shading (if using this, define LIGHTING and PERPIXEL flags)
//   FOG		well, fog

[VERTEX]
uniform mat4 glProjectionMatrix;
uniform mat4 glModelViewMatrix;
uniform int enabled;

attribute vec4 glNormal;
attribute vec4 glVertex;
attribute vec2 glTexCoord;
varying vec2 vUV;
varying vec4 vVertex;
#ifdef LIGHTING
	uniform mat4 glNormalMatrix;
	varying vec4 vNormal;
#endif

#ifdef FOG
	uniform float glFogDensity;
	varying float fogFactor;
#endif

void main() 
{
	vUV = glTexCoord;
	vVertex = glModelViewMatrix * glVertex;
	gl_Position = glProjectionMatrix * vVertex;
#ifdef LIGHTING
	vNormal = glNormalMatrix * glNormal;
#endif // LIGHTING

#ifdef FOG
	float z = length(vVertex);
	fogFactor = exp2( -glFogDensity * glFogDensity * z * z * 1.442695 );
#endif

}

[FRAGMENT]
uniform sampler2D textureMap;
uniform vec4 materialDiffuse;
uniform int enabled;

varying vec2 vUV;
varying vec4 vVertex;
#ifdef LIGHTING
	uniform vec3 glLight;
	varying vec4 vNormal;
	#ifdef PERPIXEL
		uniform vec4 materialSpecular, materialAmbient;
		uniform vec4 lightDiffuse, lightSpecular, lightAmbient;
		uniform float shininess;
	#endif
#endif

#ifdef FOG
	uniform vec3 glFogColor;
	varying float fogFactor;
#endif

void main() 
{
	vec4 color;
#ifdef LIGHTING
	vec3 N = normalize(vNormal.xyz);
	vec3 L = normalize(glLight);
	#ifdef PERPIXEL
		vec3 V = normalize(vVertex.xyz);
		vec3 R = reflect(V, N);
		vec4 ambient = materialDiffuse * materialAmbient;
		vec4 diffuse = materialDiffuse * (1.0 - materialAmbient) * max(dot(L, N), 0.0);
		vec4 specular = vec4(1.0, 1.0, 1.0, 1.0) * pow(max(dot(R, L), 0.0), shininess);
		color = texture2D(textureMap, vUV) * (ambient + diffuse + specular); // color
	#else
		color = texture2D(textureMap, vUV) * max(dot(N, L), 0.0) * materialDiffuse; // lighting color
	#endif // PERPIXEL
#else
	color = texture2D(textureMap, vUV) * materialDiffuse; // only texture color
#endif // LIGHTING

#ifdef FOG
	color = mix(vec4(glFogColor, 1.0), color, fogFactor);
#endif

	gl_FragColor = color;
}
