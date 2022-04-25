#version 330 core
out vec4 FragColor;

struct Material {
    sampler2D diffuse;
    sampler2D specular;
    float shininess;
}; 
struct DirLight {
    vec3 direction;
	
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};
struct PointLight {
    vec3 position;
    
    float constant;
    float linear;
    float quadratic;
	
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

struct SpotLight {
    vec3 position;
    vec3 direction;
    float cutOff;
    float outerCutOff;
  
    float constant;
    float linear;
    float quadratic;
  
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;       
};


in VS_OUT {
    vec3 FragPos;
    vec3 Normal;
    vec2 TexCoords;
} fs_in;

uniform vec3 viewPos;
uniform vec3 lightPos;
uniform DirLight dirLight;

uniform PointLight pointLight;
uniform SpotLight spotLight;

uniform Material material;
uniform sampler2D Texture;
uniform bool blinn;

vec3 CalcSpotLight(SpotLight light, vec3 Normal, vec3 FragPos, vec3 viewDir)
{
    vec3 lightDir = normalize(light.position - FragPos);
    // diffuse shading
    float diff = max(dot(Normal, lightDir), 0.0);
    // specular shading
    vec3 reflectDir = reflect(-lightDir, Normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    // attenuation
    float distance = length(light.position - FragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));    
    // spotlight intensity
    float theta = dot(lightDir, normalize(-light.direction)); 
    float epsilon = light.cutOff - light.outerCutOff;
    float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);
    // combine results
    vec3 ambient = light.ambient * vec3(texture(material.diffuse,fs_in.TexCoords));
    vec3 diffuse = light.diffuse * diff * vec3(texture(material.diffuse,fs_in.TexCoords));
    vec3 specular = light.specular * spec * vec3(texture(material.specular,fs_in.TexCoords));
    ambient *= attenuation * intensity;
    diffuse *= attenuation * intensity;
    specular *= attenuation * intensity;
    return (ambient + diffuse + specular);
}

void main()
{    
    vec3 norm = normalize(fs_in.Normal);
    vec3 viewDir = normalize(viewPos - fs_in.FragPos);
    vec3 result = CalcSpotLight(spotLight, norm, fs_in.FragPos, viewDir);
    // properties
    vec3 color = texture(Texture, fs_in.TexCoords).rgb;
    // ambient
    vec3 ambient = 0.05 * color;
    // diffuse
    vec3 lightDir = normalize(lightPos - fs_in.FragPos);
    vec3 Normal = normalize(fs_in.Normal);
    float diff = max(dot(lightDir, fs_in.Normal), 0.0);
    vec3 diffuse = diff * color;
    // specular
    vec3 reflectDir = reflect(-lightDir, fs_in.Normal);
    float spec = 0.0;
    if(blinn)
    {
        vec3 halfwayDir = normalize(lightDir + viewDir);  
        spec = pow(max(dot(fs_in.Normal, halfwayDir), 0.0), 32.0);
    }
    else
    {
        vec3 reflectDir = reflect(-lightDir, fs_in.Normal);
        spec = pow(max(dot(viewDir, reflectDir), 0.0), 8.0);
    }
    vec3 specular = vec3(0.3) * spec; // assuming bright white light color
    result += (ambient + diffuse + specular);
    // phase 2: point lights

    
    FragColor = vec4(result, 1.0);
}

// calculates the color when using a point light.
// vec3 CalcPointLight(PointLight light, vec3  fs_in.Normal, vec3 fs_in.FragPos, vec3 viewDir)
// {
//     vec3 lightDir =  normalize(light.position - fs_in.FragPos);
//     // diffuse shading
//     float diff = max(dot( fs_in.Normal, lightDir), 0.0);
//     // specular shading
//     vec3 reflectDir = reflect(-lightDir,  fs_in.Normal);
//     float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
//     // attenuation
//     float distance = length(light.position - fs_in.FragPos);
//     float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));    
//     // combine results
//     vec3 ambient = light.ambient * vec3(texture(material.diffuse,fs_in.TexCoords));
//     vec3 diffuse = light.diffuse * diff * vec3(texture(material.diffuse,fs_in.TexCoords));
//     vec3 specular = light.specular * spec * vec3(texture(material.specular,fs_in.TexCoords));
//     ambient *= attenuation;
//     diffuse *= attenuation;
//     specular *= attenuation;
//     return (ambient + diffuse + specular);
// }

// calculates the color when using a spot light.
