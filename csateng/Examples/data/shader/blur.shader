// http://www.gamerendering.com/2008/10/11/gaussian-blur-filter-shader/
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
uniform sampler2D textureMap; // the texture with the scene you want to blur
uniform float size;
varying vec2 vUV;

void main()
{
	vec4 sum = vec4(0.0, 0.0, 1.0, 1.0);

#ifdef HORIZ
	// take nine samples, with the distance size between them
	sum = texture2D(textureMap, vec2(vUV.x - 4.0*size, vUV.y)) * 0.05;
	sum += texture2D(textureMap, vec2(vUV.x - 3.0*size, vUV.y)) * 0.09;
	sum += texture2D(textureMap, vec2(vUV.x - 2.0*size, vUV.y)) * 0.12;
	sum += texture2D(textureMap, vec2(vUV.x - size, vUV.y)) * 0.15;
	sum += texture2D(textureMap, vec2(vUV.x, vUV.y)) * 0.16;
	sum += texture2D(textureMap, vec2(vUV.x + size, vUV.y)) * 0.15;
	sum += texture2D(textureMap, vec2(vUV.x + 2.0*size, vUV.y)) * 0.12;
	sum += texture2D(textureMap, vec2(vUV.x + 3.0*size, vUV.y)) * 0.09;
	sum += texture2D(textureMap, vec2(vUV.x + 4.0*size, vUV.y)) * 0.05;
#endif

#ifdef VERT
	// take nine samples, with the distance size between them
	sum = texture2D(textureMap, vec2(vUV.x, vUV.y - 4.0*size)) * 0.05;
	sum += texture2D(textureMap, vec2(vUV.x, vUV.y - 3.0*size)) * 0.09;
	sum += texture2D(textureMap, vec2(vUV.x, vUV.y - 2.0*size)) * 0.12;
	sum += texture2D(textureMap, vec2(vUV.x, vUV.y - size)) * 0.15;
	sum += texture2D(textureMap, vec2(vUV.x, vUV.y)) * 0.16;
	sum += texture2D(textureMap, vec2(vUV.x, vUV.y + size)) * 0.15;
	sum += texture2D(textureMap, vec2(vUV.x, vUV.y + 2.0*size)) * 0.12;
	sum += texture2D(textureMap, vec2(vUV.x, vUV.y + 3.0*size)) * 0.09;
	sum += texture2D(textureMap, vec2(vUV.x, vUV.y + 4.0*size)) * 0.05;
#endif
	gl_FragColor = sum;
}
