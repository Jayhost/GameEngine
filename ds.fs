
#version 330 core
out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D gPosition;
uniform sampler2D gNormal;
uniform sampler2D gAlbedo;
uniform sampler2D ssao;

struct Light {
    vec3 Position;
    vec3 Color;
    
    float Linear;
    float Quadratic;
    float Radius;
};
const int NR_LIGHTS = 100;
uniform Light lights[NR_LIGHTS];
uniform vec3 viewPos;

void main()
{             
    // retrieve data from gbuffer
    vec3 FragPos = texture(gPosition, TexCoords).rgb;
    vec3 Normal = texture(gNormal, TexCoords).rgb;
    vec3 Diffuse = texture(gAlbedo, TexCoords).rgb;
    //Diffuse = vec3(0.95,0.6,0.6) +(Diffuse * 0.1);
    float Specular = texture(gAlbedo, TexCoords).a;
    float AmbientOcclusion = texture(ssao, TexCoords).r;//new
    
    // then calculate lighting as usual
    // vec3 ambient = vec3((Diffuse* 0.3) * (AmbientOcclusion));
    vec3 ambient = vec3(AmbientOcclusion);
    // vec3 ambient = vec3((vec3(0.1,0.1,0.3)) * (AmbientOcclusion));

    // vec3 ambient = vec3(Diffuse);


    vec3 lighting = ambient;
    // vec3 lighting  = ambient; 
    //vec3 viewDir  = normalize(viewPos - FragPos);
    vec3 viewDir  = normalize(- FragPos);

    for(int i = 0; i < NR_LIGHTS; ++i)
    {
        // calculate distance between light source and current fragment
        float distance = length(lights[i].Position - FragPos);
        if(distance < lights[i].Radius)
        {
            // diffuse
            vec3 lightDir = normalize(lights[i].Position - FragPos);
              vec3 diffuse = max(dot(Normal, lightDir), 0.0) * Diffuse * lights[i].Color;
            // specular
            vec3 halfwayDir = normalize(lightDir + viewDir);  
            float spec = pow(max(dot(Normal, halfwayDir), 0.0), 18.0);
            vec3 specular = lights[i].Color * spec;
            // attenuation
            float attenuation = 1.0 / (1.0 + lights[i].Linear * distance + lights[i].Quadratic * distance * distance);
            diffuse *= attenuation;
            specular *= attenuation;
            // lighting += diffuse + specular;
            
            
            
        }
    }   
    // FragColor = vec4(vec3(AmbientOcclusion), 1.0); 
    FragColor = vec4(lighting, 1.0); 
    // FragColor = vec4(vec3(1-AmbientOcclusion), 1.0); 

    //  FragColor = vec4(lighting, 1.0);
}

// #version 330 core
// out vec4 FragColor;
  
// in vec2 TexCoords;

// uniform sampler2D gPosition;
// uniform sampler2D gNormal;
// uniform sampler2D gAlbedoSpec;
// uniform sampler2D ssao;

// struct Light {
//     vec3 Position;
//     vec3 Color;
    
//     float Linear;
//     float Quadratic;
//     float Radius;
// };
// const int NR_LIGHTS = 100;
// uniform Light lights[NR_LIGHTS];
// uniform vec3 viewPos;

// void main()
// {             
//     // retrieve data from G-buffer
//     vec3 FragPos = texture(gPosition, TexCoords).rgb;
//     vec3 Normal = texture(gNormal, TexCoords).rgb;
//     vec3 Albedo = texture(gAlbedoSpec, TexCoords).rgb;
//     float Specular = texture(gAlbedoSpec, TexCoords).a;
    
//     // then calculate lighting as usual
//     vec3 lighting = Albedo * 0.1; // hard-coded ambient component
//     vec3 viewDir = normalize(-FragPos);
//     for(int i = 0; i < NR_LIGHTS; ++i)
//     {
//         // diffuse
//         vec3 lightDir = normalize(lights[i].Position - FragPos);
//         vec3 diffuse = max(dot(Normal, lightDir), 0.0) * Albedo * lights[i].Color;
//         lighting += diffuse;
//     }
    
//     FragColor = vec4(Albedo, 1.0);
// }  