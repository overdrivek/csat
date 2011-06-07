[SETUP]
//#define TEXTURE
//CAMERA eyePos


[VERTEX]

#ifdef SKY
  void main()
  {
          gl_TexCoord[0] = gl_MultiTexCoord0;
          gl_FrontColor=gl_Color;
          gl_Position = ftransform();
  }
#else
varying vec3 lightDir,normal;
varying vec4 diffuse, ambient;

void main()
{
        vec4 pos = gl_ModelViewMatrix * gl_Vertex;
        lightDir = normalize(vec3(gl_LightSource[0].position-pos));
        normal = normalize(gl_NormalMatrix * gl_Normal);
#ifdef TEXTURE
        gl_TexCoord[0] = gl_MultiTexCoord0;
#endif
//        gl_FrontColor=gl_Color;
        gl_Position = ftransform();
}
#endif

[FRAGMENT]

#ifdef SKY
  uniform sampler2D texture;
  void main()
  {
          gl_FragColor = texture2D(texture, gl_TexCoord[0].st);
  }
#else
varying vec3 lightDir,normal;
varying vec4 diffuse, ambient;
uniform sampler2D textureMap;

void main()
{
        float diffuse_value = max(dot(normal, lightDir), 0.0);
        vec4 color = /*gl_Color **/ vec4(diffuse_value,diffuse_value,diffuse_value,1);
#ifdef TEXTURE
        color = color * texture2D(textureMap, gl_TexCoord[0].st);
#endif
        gl_FragColor = color;
}
#endif
