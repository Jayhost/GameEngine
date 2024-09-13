
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

const float PI = 3.14159265359;
// ----------------------------------------------------------------------------
// Easy trick to get tangent-normals to world-space to keep PBR code simplified.
// Don't worry if you don't get what's going on; you generally want to do normal 
// mapping the usual way for performance anyways; I do plan make a note of this 
// technique somewhere later in the normal mapping tutorial.

// ----------------------------------------------------------------------------
float DistributionGGX(vec3 N, vec3 H, float roughness)
{
    float a = roughness*roughness;
    float a2 = a*a;
    float NdotH = max(dot(N, H), 0.0);
    float NdotH2 = NdotH*NdotH;

    float nom   = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;

    return nom / denom;
}
// ----------------------------------------------------------------------------
float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r*r) / 8.0;

    float nom   = NdotV;
    float denom = NdotV * (1.0 - k) + k;

    return nom / denom;
}
// ----------------------------------------------------------------------------
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2 = GeometrySchlickGGX(NdotV, roughness);
    float ggx1 = GeometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
}
// ----------------------------------------------------------------------------
vec3 fresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}

void main()
{             
    // retrieve data from gbuffer
    vec3 FragPos = texture(gPosition, TexCoords).rgb;
    vec3 Normal = texture(gNormal, TexCoords).rgb;
    vec3 Diffuse = texture(gAlbedo, TexCoords).rgb; //change this back
    float metallic  = texture(gNormal, TexCoords).a;
    float roughness = texture(gAlbedo, TexCoords).a;
    //vec3 Diffuse = vec3(0.1,0.1,0.1);
    //float Specular = texture(gAlbedo, TexCoords).a;
    float AmbientOcclusion = texture(ssao, TexCoords).r;//new
    //AmbientOcclusion = AmbientOcclusion * 1.2;
    // then calculate lighting as usual
     vec3 ambient = vec3( 0.55 * Diffuse * AmbientOcclusion); //original
     //ambient = ambient * 2;
    // vec3 ambient = vec3( 0.3 * Diffuse); 
    //vec3 ambient = vec3(AmbientOcclusion);
    // vec3 ambient = vec3((vec3(0.1,0.1,0.3)) * (AmbientOcclusion));

    // vec3 ambient = vec3(Diffuse);


    vec3 lighting = ambient;

    // vec3 lighting  = ambient; 
    //vec3 viewDir  = normalize(viewPos - FragPos);
    vec3 viewDir  = normalize(- FragPos);
    //testing delete
    vec3 N = Normal;
    
   // vec3 diffuse = max(dot(Normal, lightDir), 0.0) * Diffuse * lights[0].Color;
    vec3 diffuse;
    vec3 specular;
    vec3 F0 = vec3(0.04); 
    F0 = mix(F0, Diffuse, metallic);

    // reflectance equation
    vec3 Lo = vec3(0.0);
    // for(int i = 0; i < NR_LIGHTS; ++i)
    // {
    //     // calculate distance between light source and current fragment
    //     float distance = length(lights[i].Position - FragPos);
    //     if(distance < lights[i].Radius)
    //     {
    //         // diffuse
    //         vec3 lightDir = normalize(lights[i].Position - FragPos);
    //         diffuse = max(dot(Normal, lightDir), 0.0) * Diffuse * lights[i].Color;
    //         //vec3 diffuse = max(dot(Normal, lightDir), 0.0) * Diffuse * light.Color; old one light
    //         // specular
    //         vec3 halfwayDir = normalize(lightDir + viewDir);  
    //         float spec = pow(max(dot(Normal, halfwayDir), 0.0), 8.0);
    //         specular = lights[i].Color * spec;
    //         // attenuation
    //         float attenuation = 1.0 / (1.0 + lights[i].Linear * distance + lights[i].Quadratic * distance * distance);
    //         diffuse *= attenuation;// change
    //         specular *= attenuation;
    //         //lighting += diffuse;
    //         lighting += diffuse + specular;
            
            
            
    //     }
    // }  
    for(int i = 0; i < NR_LIGHTS; ++i) 
    {
        vec3 lightDir = normalize(lights[i].Position - FragPos);
        vec3 V = lightDir;
        float distance = length(lights[i].Position - FragPos);
        if(distance < lights[i].Radius){
        // calculate per-light radiance
        vec3 L = normalize(lights[i].Position - FragPos);//figure out world pos thing later
        vec3 H = normalize(V + L);
        float distance = length(lights[i].Position - FragPos);
        float attenuation = 1.0 / (distance * distance);
        vec3 radiance = lights[i].Color * attenuation;

        // Cook-Torrance BRDF
        float NDF = DistributionGGX(N, H, roughness);   
        float G   = GeometrySmith(N, V, L, roughness);      
        vec3 F    = fresnelSchlick(max(dot(H, V), 0.0), F0);
           
        vec3 numerator    = NDF * G * F; 
        float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.0001; // + 0.0001 to prevent divide by zero
        vec3 specular = numerator / denominator;
        
        // kS is equal to Fresnel
        vec3 kS = F;
        // for energy conservation, the diffuse and specular light can't
        // be above 1.0 (unless the surface emits light); to preserve this
        // relationship the diffuse component (kD) should equal 1.0 - kS.
        vec3 kD = vec3(1.0) - kS;
        // multiply kD by the inverse metalness such that only non-metals 
        // have diffuse lighting, or a linear blend if partly metal (pure metals
        // have no diffuse light).
        kD *= 1.0 - metallic;	  

        // scale light by NdotL
        float NdotL = max(dot(N, L), 0.0);        

        // add to outgoing radiance Lo
        Lo += (kD * Diffuse / PI + specular) * radiance * NdotL;  // note that we already multiplied the BRDF by the Fresnel (kS) so we won't multiply by kS again
        }
    }   
     
     //FragColor = vec4(vec3((Diffuse * 0.3) * AmbientOcclusion), 1.0); 
    //FragColor = vec4(ambient, 1.0); 
     //FragColor = vec4(vec3(1-(AmbientOcclusion*1.1)), 1.0); 
    vec3 color = ambient + Lo;
      FragColor = vec4(vec3(color), 1.0);
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