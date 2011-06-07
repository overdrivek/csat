[SETUP]

[VERTEX]
varying vec3 normal;
void main()
{
        normal = normalize(gl_NormalMatrix * gl_Normal);
        gl_Position = ftransform();
}

[FRAGMENT]
varying vec3 normal;
void main()
{
        vec4 color;
        // [-1,1] -> [0,1]
        color.r = (normal.r + 1.0) * 0.5;
        color.g = (normal.g + 1.0) * 0.5;
        color.z = (normal.b + 1.0) * 0.5;
        color.a = 1.0;
        gl_FragColor = color;
}
