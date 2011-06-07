[SETUP]
#define LIGHTMASK 6
#define SHADOWMAP 7

#define USE_LIGHTMASK

[VERTEX]
varying vec4 vertex;
varying vec3 normal;

void main()
{
    vertex = gl_ModelViewMatrix * gl_Vertex;
    normal = gl_NormalMatrix    * gl_Normal;

    gl_TexCoord[0] = gl_MultiTexCoord0;
    gl_TexCoord[SHADOWMAP] = gl_TextureMatrix[SHADOWMAP] * gl_Vertex;
    gl_Position = ftransform();
}


[FRAGMENT]
// renderoi texturen ja varjon.

uniform sampler2D diffuseMap;
uniform sampler2DShadow shadowMap;
uniform float lightEnergy;
uniform float ambient;
#ifdef USE_LIGHTMASK
uniform sampler2D lightmask;
#endif

varying vec3 position;
varying vec4 vertex;
varying vec3 normal;

void main()
{
    vec4  col = texture2D(diffuseMap, gl_TexCoord[0].xy) * ambient;
    float shadow=1.0;
    if(gl_TexCoord[SHADOWMAP].w>0.0)
        shadow = shadow2DProj(shadowMap, gl_TexCoord[SHADOWMAP]).r;

#ifdef NO_NORMALS
    vec3 lit=vec3(1,1,1);
#else
    vec3 N  = normalize(normal);
    vec3 L = normalize(gl_LightSource[0].position.xyz - vertex.xyz);
    vec3 lit=vec3(max(dot(N, L), 0.0));
#endif

#ifdef USE_LIGHTMASK
    vec3  lm = texture2DProj(lightmask, gl_TexCoord[SHADOWMAP]).rgb;
    vec3  d = lit * shadow * lm;
#else
    vec3  d = lit * shadow;
#endif

    gl_FragColor = vec4(col.rgb * (lightEnergy * gl_LightSource[0].diffuse.rgb 
                                * d + gl_LightModel.ambient.rgb), col.a);
}