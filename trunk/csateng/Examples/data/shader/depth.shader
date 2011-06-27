[VERTEX]
uniform mat4 glProjectionMatrix;
uniform mat4 glModelViewMatrix;
attribute vec4 glVertex;
#ifdef DEPTH_W
varying float vDepth;
#endif

void main()
{
	gl_Position = glProjectionMatrix * glModelViewMatrix * glVertex;

#ifdef DEPTH_W
	vDepth = gl_Position.w;
#endif
}

[FRAGMENT]
#ifdef DEPTH_W
varying float vDepth;
#endif

void main()
{
#ifdef DEPTH_W
	gl_FragColor = vec4(vDepth, 0.0, 0.0, 1.0);
#else
	gl_FragColor = vec4(1.0, 1.0, 1.0, 1.0);
#endif
}
