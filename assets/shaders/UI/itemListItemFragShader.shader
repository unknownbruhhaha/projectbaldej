#version 330

in vec2 texCoord;
in vec4 currentCoordinate;

uniform sampler2D texture0;

out vec4 outputColor;

void main()
{
	outputColor = texture(texture0, texCoord);
}
