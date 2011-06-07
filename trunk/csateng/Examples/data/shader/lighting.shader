//http://www.lighthouse3d.com/opengl/glsl/index.php?pointlight
[SETUP]

[VERTEX]
varying vec4 diffuse, ambientGlobal, ambient;
varying vec3 normal, lightDir, halfVector;
varying float dist;

void main()
{
	vec4 ecPos;
	vec3 aux;

	normal = normalize(gl_NormalMatrix * gl_Normal);

	/* these are the new lines of code to compute the light's direction */
	ecPos = gl_ModelViewMatrix * gl_Vertex;
	aux = vec3(gl_LightSource[0].position-ecPos);
	lightDir = normalize(aux);
	dist = length(aux);

	halfVector = normalize(gl_LightSource[0].halfVector.xyz);

	/* Compute the diffuse, ambient and globalAmbient terms */
	diffuse = gl_FrontMaterial.diffuse * gl_LightSource[0].diffuse;

	/* The ambient terms have been separated since one of them */
	/* suffers attenuation */
	ambient = gl_FrontMaterial.ambient * gl_LightSource[0].ambient;
	ambientGlobal = gl_LightModel.ambient * gl_FrontMaterial.ambient;

        gl_TexCoord[0] = gl_MultiTexCoord0;

        gl_Position = ftransform();
}


[FRAGMENT]
varying vec4 diffuse,ambientGlobal, ambient;
varying vec3 normal, lightDir, halfVector;
varying float dist;
uniform sampler2D diffuseMap;

void main()
{
	vec3 n,halfV,viewV,ldir;
	float NdotL,NdotHV;
	vec4 color = ambientGlobal;
	float att;

	n = normalize(normal);
	NdotL = max(dot(n,normalize(lightDir)),0.0);

	if (NdotL > 0.0)
        {
        	att = 1.0 / (gl_LightSource[0].constantAttenuation +
				gl_LightSource[0].linearAttenuation * dist +
				gl_LightSource[0].quadraticAttenuation * dist * dist);
		color += att * (diffuse * NdotL + ambient);


		halfV = normalize(halfVector);
		NdotHV = max(dot(n,halfV),0.0);
		color += att * gl_FrontMaterial.specular * gl_LightSource[0].specular *
						pow(NdotHV,gl_FrontMaterial.shininess);
        }

        color = color * texture2D(diffuseMap, gl_TexCoord[0].st) * 2.0;
        gl_FragColor = color;
}
