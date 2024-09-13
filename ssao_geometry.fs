#version 330 core
layout (location = 0) out vec3 gPosition;
layout (location = 1) out vec4 gNormal;
layout (location = 2) out vec4 gAlbedo;

in vec2 TexCoords;
in vec3 FragPos;
in vec3 Normal;
in vec3 WorldPos;



uniform sampler2D texture_diffuse1;
uniform sampler2D texture_specular1;
uniform samplerCube skybox;
uniform vec3 cameraPos;

float computeFog()
{
float fogDensity = 0.05f;
float fragmentDistance = length(FragPos);
float fogFactor = exp(-pow(fragmentDistance * fogDensity, 2));
return clamp(fogFactor, 0.0f, 1.0f);
}

void main()
{    
    float metallic  = texture(texture_specular1, TexCoords).b;
    float roughness = texture(texture_specular1, TexCoords).g;
    // store the fragment position vector in the first gbuffer texture
    gPosition = FragPos;
    // also store the per-fragment normals into the gbuffer
    gNormal.xyz = normalize(Normal);
    // and the diffuse per-fragment color
    // gAlbedo.rgb = vec3(0.65);
    float fogFactor = computeFog();
    vec3 fogColor = vec3(0.5f, 0.5f, 0.5f);

    vec3 color = texture(texture_diffuse1, TexCoords).rgb;// * (fogColor * fogFactor).rgb;
    
    // gAlbedo.rgb = fogColor * (1 – fogFactor) + fogColor * fogFactor;
    gAlbedo.rgb = color;//* mix(fogColor, fogColor, fogFactor);
    gAlbedo.a = roughness;
    gNormal.a = metallic;
    //gAlbedo.rgb = vec3(0.95);
    // gAlbedo.rgb += (fogColor * (1 – fogFactor) + texture(texture_diffuse1, TexCoords).rgb * fogFactor).rgb;
    //gAlbedo.rgb = vec3(0.95);
    // store specular intensity in gAlbedoSpec's alpha component
    // gAlbedo.a = texture(texture_specular1, TexCoords).r;
    // vec3 I = normalize(gPosition - cameraPos);
    // vec3 R = reflect(I, normalize(Normal));
    //FragColor = vec4(texture(skybox, R).rgb, 1.0);
    //gAlbedo.rgb += texture(skybox, R).rgb;
    
}