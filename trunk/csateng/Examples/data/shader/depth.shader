// by Joe Forte - http://www.joeforte.net/projects/soft-particles/
[VERTEX]
// Projection space depth information (before divide)
varying float Depth;
void main()
{
	gl_Position = ftransform();
	Depth = gl_Position.w;
}

[FRAGMENT]
// Projection space depth information (before divide)
varying float Depth;
void main()
{
	gl_FragData[0] = vec4(Depth, 0.0, 0.0, 1.0);
}
