// flags:
//   -none-    only texturing
//   LIGHTING  simple shading
// 	 PHONG     better shading (if using this, define LIGHTING and PHONG flags)
[VERTEX]
uniform mat4 glProjectionMatrix;
uniform mat4 glModelViewMatrix;
#ifdef LIGHTING
uniform mat4 glNormalMatrix;
varying vec3 vNormal;
 #ifdef PHONG
 varying vec3 vVertex;
 #endif
#endif
attribute vec3 glNormal;
attribute vec4 glVertex;
attribute vec2 glTexCoord;
varying vec2 vUV;

void main() 
{
	vUV = glTexCoord;
	gl_Position = glProjectionMatrix * glModelViewMatrix * glVertex;
#ifdef LIGHTING
	vNormal = (glNormalMatrix * vec4(glNormal, 1)).xyz;
 #ifdef PHONG
	vVertex = (glModelViewMatrix * glVertex).xyz;
 #endif
#endif // LIGHTING
}

[FRAGMENT]
uniform sampler2D textureMap;
uniform vec4 materialDiffuse;
#ifdef LIGHTING
uniform vec3 glLight;
varying vec3 vNormal;
 #ifdef PHONG
 uniform vec4 materialSpecular, materialAmbient;
 uniform vec4 lightDiffuse, lightSpecular, lightAmbient;
 uniform float shininess;
 varying vec3 vVertex;
 #endif
#endif
varying vec2 vUV;

void main() 
{
#ifdef LIGHTING
	vec3 N = normalize(vNormal);
	vec3 L = normalize(glLight);
  #ifdef PHONG
	vec3 V = normalize(vVertex);
	vec3 R = reflect(V, N);
	vec4 ambient = materialDiffuse * materialAmbient;
	vec4 diffuse = materialDiffuse * (1.0 - materialAmbient) * max(dot(L, N), 0.0);
	vec4 specular = vec4(1.0, 1.0, 1.0, 1.0) * pow(max(dot(R, L), 0.0), shininess);
	gl_FragColor = texture2D(textureMap, vUV.xy) * (ambient + diffuse + specular);
  #else
	gl_FragColor = texture2D(textureMap, vUV.xy) * max(dot(N, L), 0.0) * materialDiffuse;
  #endif // PHONG

#else
	gl_FragColor = texture2D(textureMap, vUV.xy) * materialDiffuse;
#endif // LIGHTING
}
