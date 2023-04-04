#version 330 core
out vec4 FragColor;

//In order to calculate some basic lighting we need a few things per model basis, and a few things per fragment basis:
uniform vec3 lightColor; //The color of the light.
uniform vec3 lightPos; //The position of the light(position).
uniform vec3 lightDirection; // The direction of the light(rotation).

uniform sampler2D texture0;
in vec2 texCoord;

in vec3 Normal; //The normal of the fragment is calculated in the vertex shader.
in vec3 FragPos; //The fragment position.

void main()
{
    //The ambient color is the color where the light does not directly hit the object.
    //You can think of it as an underlying tone throughout the object. Or the light coming from the scene/the sky (not the sun).
    float ambientStrength = 0.1f;
    vec3 ambient = ambientStrength * lightColor;

    //We calculate the light direction, and make sure the normal is normalized.
    vec3 norm = normalize(Normal);

    //The diffuse part of the phong model.
    //This is the part of the light that gives the most, it is the color of the object where it is hit by light.
    float diff = max(dot(norm, lightDirection), 0.0); //We make sure the value is non negative with the max function.
    vec3 diffuse = diff * lightColor;

    //At last we add all the light components together and multiply with the color of the object. Then we set the color
    //and makes sure the alpha value is 1
    vec4 result = vec4((ambient + diffuse), 1) * texture(texture0, texCoord);
    FragColor = result;
    FragColor = vec4(1, 1, 1, 1);
    
    //Note we still use the light color * object color from the last tutorial.
    //This time the light values are in the phong model (ambient, diffuse and specular)
}
