[VERTEX]
uniform mat4 glProjectionMatrix;
uniform mat4 glModelViewMatrix;
attribute vec4 glVertex;
attribute vec2 glTexCoord;
varying vec4 vNormal;
varying vec2 vUV;

void main()
{
	vUV = glTexCoord;
	gl_Position = glProjectionMatrix * glModelViewMatrix * glVertex;
}

[FRAGMENT]
uniform sampler2D textureMap;
uniform vec4 materialDiffuse;
varying vec2 vUV;

void main()
{
	gl_FragColor = texture2D(textureMap, vUV) * materialDiffuse;
}
