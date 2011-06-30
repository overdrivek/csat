// flags:
//  -none-         default shadow mapping depth
//  ALPHATEST  shadow mapping depth   
//  DEPTH_W        soft particles depth (w) 

[VERTEX]
uniform mat4 glProjectionMatrix;
uniform mat4 glModelViewMatrix;
attribute vec4 glVertex;
#ifdef DEPTH_W
varying float vDepth;
#endif
#ifdef ALPHATEST
attribute vec2 glTexCoord;
varying vec2 vUV;
#endif

void main()
{
	gl_Position = glProjectionMatrix * glModelViewMatrix * glVertex;

#ifdef DEPTH_W
	vDepth = gl_Position.w;
#endif
#ifdef ALPHATEST
	vUV = glTexCoord;
#endif
}

[FRAGMENT]
#ifdef DEPTH_W
varying float vDepth;
#else
uniform sampler2D textureMap;
varying vec2 vUV;
#endif

void main()
{
#ifdef DEPTH_W
	gl_FragColor = vec4(vDepth, 0.0, 0.0, 1.0); // soft particles depth
#else
	#ifdef ALPHATEST
		vec4 c = texture2D(textureMap, vUV);
		if(c.a < 0.1) discard;
	#endif // ALPHATEST
	gl_FragColor = vec4(1.0, 1.0, 1.0, 1.0); // shadow mapping depth

#endif // DEPTH_W
}
