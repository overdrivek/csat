// http://myheroics.wordpress.com/2008/09/04/glsl-bloom-shader/
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
uniform sampler2D textureMap;
uniform float size;
varying vec2 vUV;

void main()
{
	vec4 sum = vec4(0.0);
	int i, j;

	for( i= -4 ;i < 4; i++)
	{
		for (j = -3; j < 3; j++)
		{
			sum += texture2D(textureMap, vUV + vec2(j, i) * size) * 0.25;
		}
	}

	if (texture2D(textureMap, vUV).r < 0.3)
	{
		gl_FragColor = sum*sum*0.012 + texture2D(textureMap, vUV);
	}
	else
	{
		if (texture2D(textureMap, vUV).r < 0.5)
		{
			gl_FragColor = sum*sum*0.009 + texture2D(textureMap, vUV);
		}
		else
		{
			gl_FragColor = sum*sum*0.0075 + texture2D(textureMap, vUV);
		}
	}
}
