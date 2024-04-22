using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Tao.OpenGl;
using Tao.DevIl;
using Tao.Math;
using Assimp;
using Assimp.Configs;
using MyShader;
using ModelLoadingByAssimp;

namespace ModelLoadingByAssimp
{
    struct Vector2
    {
        public float x;
        public float y;
    };
    struct Vector3
    {
        public float x;
        public float y;
        public float z;
    };
    struct Color4
    {
        public float R, G, B, A;
    };
    struct Vertex
    {
        // position
        public Vector3 Position;
        // normal
        public Vector3 Normal;
        // texCoords
        public Vector2 TexCoords;
        // tangent
        public Vector3 Tangent;
        // bitangent
        public Vector3 Bitangent;
    };
    class Material
    {
        public bool hasTexture = false;
        public bool hasAmbient = false;
        public bool hasDiffuse = false;
        public bool hasSpecular = false;
        public bool hasShininess = false;
        public Color4 ambient = new Color4();
        public Color4 diffuse = new Color4();
        public Color4 specular = new Color4();
        public float shininess = 0.0f;
    };
    class Texture
    {
        public uint id = 0;
        public string type = null;
        public string path = null;
    };

    class Mesh
    {
        // mesh Data
        List<Vertex> vertices;
        List<int> indices;
        List<Texture> textures;
        Material material = new Material();
        uint[] VAO = new uint[1];

        // constructor
        public Mesh(List<Vertex> vertices, List<int> indices, Material material, List<Texture> textures)
        {
            this.vertices = vertices;
            this.indices = indices;
            this.material = material;
            this.textures = textures;

            // now that we have all the required data, set the vertex buffers and its attribute pointers.
            setupMesh();
        }

        ~Mesh()
        {
        }

        public void ComputeBoundingBox(out Vector3 min, out Vector3 max)
        {
            min.x = min.y = min.z = 1e10f;
            max.x = max.y = max.z = 1e-10f;
            for (int i=0; i<vertices.Count; i++)
            {
                min.x = Math.Min(vertices[i].Position.x, min.x);
                min.y = Math.Min(vertices[i].Position.y, min.y);
                min.z = Math.Min(vertices[i].Position.z, min.z);
                max.x = Math.Max(vertices[i].Position.x, max.x);
                max.y = Math.Max(vertices[i].Position.y, max.y);
                max.z = Math.Max(vertices[i].Position.z, max.z);
            }
        }
        
        // render the mesh
        public void Draw(Shader shader)
        {
            // bind appropriate textures
            uint diffuseNr = 1;
            uint specularNr = 1;
            uint normalNr = 1;
            uint heightNr = 1;

            if (material.hasTexture)
            {
                Gl.glUniform1i(Gl.glGetUniformLocation(shader.Program, "material.nTextures"), textures.Count);
                for (int i = 0; i < textures.Count; i++)
                {
                    Gl.glActiveTexture(Gl.GL_TEXTURE0 + i); // active proper texture unit before binding
                                                            // retrieve texture number (the N in diffuse_textureN)
                    string number = "";
                    string name = textures[i].type;
                    if (name == "material.texture_diffuse")
                        number = (diffuseNr++).ToString();
                    else if (name == "material.texture_specular")
                        number = (specularNr++).ToString(); // transfer unsigned int to stream
                    else if (name == "material.texture_normal")
                        number = (normalNr++).ToString(); // transfer unsigned int to stream
                    else if (name == "material.texture_height")
                        number = (heightNr++).ToString(); // transfer unsigned int to stream

                    // now set the sampler to the correct texture unit
                    Gl.glUniform1i(Gl.glGetUniformLocation(shader.Program, name + number), i);
                    // and finally bind the texture
                    Gl.glBindTexture(Gl.GL_TEXTURE_2D, textures[i].id);
                }
            }
            else
            {
                Gl.glUniform1i(Gl.glGetUniformLocation(shader.Program, "material.nTextures"), 0);
            }

            if(material.hasAmbient)
            {
                Gl.glUniform1i(Gl.glGetUniformLocation(shader.Program, "material.hasAmbient"), 1);
                Gl.glUniform4f(Gl.glGetUniformLocation(shader.Program, "material.ambient"), material.ambient.R, material.ambient.G, material.ambient.B, material.ambient.A);
            }
            else Gl.glUniform1i(Gl.glGetUniformLocation(shader.Program, "material.hasAmbient"), 0);

            if (material.hasDiffuse)
            {
                Gl.glUniform1i(Gl.glGetUniformLocation(shader.Program, "material.hasDiffuse"), 1);
                Gl.glUniform4f(Gl.glGetUniformLocation(shader.Program, "material.diffuse"), material.diffuse.R, material.diffuse.G, material.diffuse.B, material.diffuse.A);
            }
            else Gl.glUniform1i(Gl.glGetUniformLocation(shader.Program, "material.hasDiffuse"), 0);

            if (material.hasSpecular)
            {
                Gl.glUniform1i(Gl.glGetUniformLocation(shader.Program, "material.hasSpecular"), 1);
                Gl.glUniform4f(Gl.glGetUniformLocation(shader.Program, "material.specular"), material.specular.R, material.specular.G, material.specular.B, material.specular.A);
            }
            else Gl.glUniform1i(Gl.glGetUniformLocation(shader.Program, "material.hasSpecular"), 0);

            if (material.hasShininess)
            {
                Gl.glUniform1i(Gl.glGetUniformLocation(shader.Program, "material.hasShininess"), 1);
                Gl.glUniform1f(Gl.glGetUniformLocation(shader.Program, "material.shininess"), material.shininess);
            }
            else Gl.glUniform1i(Gl.glGetUniformLocation(shader.Program, "material.hasShininess"), 0);

            // draw mesh
            Gl.glBindVertexArray(VAO[0]);
            Gl.glDrawElements(Gl.GL_TRIANGLES, indices.Count, Gl.GL_UNSIGNED_INT, (IntPtr)0);
            Gl.glBindVertexArray(0);

            // always good practice to set everything back to defaults once configured.
            Gl.glActiveTexture(Gl.GL_TEXTURE0);
        }

        public void DrawByOpenGL2()
        {
            if(material.hasTexture && textures.Count > 0)
            {
                Gl.glEnable(Gl.GL_TEXTURE_2D);
                Gl.glBindTexture(Gl.GL_TEXTURE_2D, textures[0].id); // We assume using the first texture
            }
            if (material.hasAmbient)
            {
                float[] ambient = new float[] { material.ambient.R, material.ambient.G, material.ambient.B, material.ambient.A};
                Gl.glMaterialfv(Gl.GL_FRONT_AND_BACK, Gl.GL_AMBIENT, ambient);
            }
            if (material.hasDiffuse)
            {
                float[] diffuse = new float[] { material.diffuse.R, material.diffuse.G, material.diffuse.B, material.diffuse.A };
                Gl.glMaterialfv(Gl.GL_FRONT_AND_BACK, Gl.GL_DIFFUSE, diffuse);
            }
            if (material.hasSpecular)
            {
                float[] specular = new float[] { material.specular.R, material.specular.G, material.specular.B, material.specular.A };
                Gl.glMaterialfv(Gl.GL_FRONT_AND_BACK, Gl.GL_SPECULAR, specular);
            }
            if (material.hasShininess) Gl.glMaterialf(Gl.GL_FRONT_AND_BACK, Gl.GL_SHININESS, material.shininess);

            Gl.glBegin(Gl.GL_TRIANGLES);
            for(int i=0; i< indices.Count; i++)
            {
                Gl.glTexCoord2f(vertices[indices[i]].TexCoords.x, vertices[indices[i]].TexCoords.y);
                Gl.glNormal3f(vertices[indices[i]].Normal.x, vertices[indices[i]].Normal.y, vertices[indices[i]].Normal.z);
                Gl.glVertex3f(vertices[indices[i]].Position.x, vertices[indices[i]].Position.y, vertices[indices[i]].Position.z); ;
            }
            Gl.glEnd();
            if (material.hasTexture && textures.Count > 0)
            {
                Gl.glBindTexture(Gl.GL_TEXTURE_2D, 0);
                Gl.glDisable(Gl.GL_TEXTURE_2D);
            }
        }

        // render data 
        private uint[] VBO = new uint[1];
        private uint[] EBO = new uint[1];

        // initializes all the buffer objects/arrays
        void setupMesh()
        {
            // Transfer to array so that thay can be used by glBufferData
            Vertex[] vertex_array = new Vertex[vertices.Count];
            int[] index_array = new int[indices.Count];
            for (int i = 0; i < vertices.Count; i++) vertex_array[i] = vertices[i];
            for (int i = 0; i < indices.Count; i++) index_array[i] = indices[i];

            // create buffers/arrays
            Gl.glGenVertexArrays(1, VAO);
            Gl.glGenBuffers(1, VBO);
            Gl.glGenBuffers(1, EBO);

            Gl.glBindVertexArray(VAO[0]);
            // load data into vertex buffers
            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, VBO[0]);
            // A great thing about structs is that their memory layout is sequential for all its items.
            // The effect is that we can simply pass a pointer to the struct and it translates perfectly to a glm::vec3/2 array which
            // again translates to 3/2 floats which translates to a byte array.
            Gl.glBufferData(Gl.GL_ARRAY_BUFFER, (IntPtr)(vertices.Count * Marshal.SizeOf(typeof(Vertex))), vertex_array, Gl.GL_STATIC_DRAW);

            Gl.glBindBuffer(Gl.GL_ELEMENT_ARRAY_BUFFER, EBO[0]);
            Gl.glBufferData(Gl.GL_ELEMENT_ARRAY_BUFFER, (IntPtr)(indices.Count * sizeof(int)), index_array, Gl.GL_STATIC_DRAW);

            // set the vertex attribute pointers
            // vertex Positions
            Gl.glEnableVertexAttribArray(0);
            Gl.glVertexAttribPointer(0, 3, Gl.GL_FLOAT, false, Marshal.SizeOf(typeof(Vertex)), (IntPtr)0);
            // vertex normals
            Gl.glEnableVertexAttribArray(1);
            Gl.glVertexAttribPointer(1, 3, Gl.GL_FLOAT, false, Marshal.SizeOf(typeof(Vertex)), Marshal.OffsetOf<Vertex>("Normal"));
            // vertex texture coords
            Gl.glEnableVertexAttribArray(2);
            Gl.glVertexAttribPointer(2, 2, Gl.GL_FLOAT, false, Marshal.SizeOf(typeof(Vertex)), Marshal.OffsetOf<Vertex>("TexCoords"));
            // vertex tangent
            Gl.glEnableVertexAttribArray(3);
            Gl.glVertexAttribPointer(3, 3, Gl.GL_FLOAT,false, Marshal.SizeOf(typeof(Vertex)), Marshal.OffsetOf<Vertex>("Tangent"));
            // vertex bitangent
            Gl.glEnableVertexAttribArray(4);
            Gl.glVertexAttribPointer(4, 3, Gl.GL_FLOAT, false, Marshal.SizeOf(typeof(Vertex)), Marshal.OffsetOf<Vertex>("Bitangent"));

            Gl.glBindVertexArray(0);
        }
    };

    class Model
    {
        // model data 
        public List<Texture> textures_loaded;    // stores all the textures loaded so far, optimization to make sure textures aren't loaded more than once.
        public List<Mesh> meshes;
        public string directory;
        public bool gammaCorrection;
        private int m_displayList;
        Scene scene;
  
        // constructor, expects a filepath to a 3D model.
        public Model(string path, bool gamma = false) 
        {
            textures_loaded = new List<Texture>();
            meshes = new List<Mesh>();
            gammaCorrection = gamma;
            loadModel(path);
        }

        ~Model()
        {
        }

        // draws the model, and thus all its meshes
        public void Draw(Shader shader)
        {
            for (int i = 0; i < meshes.Count; i++)
                meshes[i].Draw(shader);
        }


        // draws the model using functions in OpenGL 2 and eariler version
        public void DrawByOpenGL2()
        {
            if (m_displayList == 0)
            {
                m_displayList = Gl.glGenLists(1);
                Gl.glNewList(m_displayList, Gl.GL_COMPILE);
                for (int i = 0; i < meshes.Count; i++)
                    meshes[i].DrawByOpenGL2();
                Gl.glEndList();
            }
            Gl.glCallList(m_displayList);
        }

        public void ComputeBoundingBox(float[] min, float[] max)
        {
            Vector3 model_min = new Vector3();
            Vector3 model_max = new Vector3();
            ComputeBoundingBox(out model_min, out model_max);
            min[0] = model_min.x; min[1] = model_min.y; min[2] = model_min.z;
            max[0] = model_max.x; max[1] = model_max.y; max[2] = model_max.z;
        }

        public void ComputeBoundingBox(out Vector3 min, out Vector3 max)
        {
            min.x = min.y = min.z = 1e10f;
            max.x = max.y = max.z = 1e-10f;

            for (int i = 0; i < meshes.Count; i++)
            {
                Vector3 mesh_min = new Vector3();
                Vector3 mesh_max = new Vector3();
                meshes[i].ComputeBoundingBox(out mesh_min, out mesh_max);
                min.x = Math.Min(min.x, mesh_min.x);
                min.y = Math.Min(min.y, mesh_min.y);
                min.z = Math.Min(min.z, mesh_min.z);
                max.x = Math.Max(max.x, mesh_max.x);
                max.y = Math.Max(max.y, mesh_max.y);
                max.z = Math.Max(max.z, mesh_max.z);
            }
        }
        // loads a model with supported ASSIMP extensions from file and stores the resulting meshes in the meshes vector.
        void loadModel(string path)
        {
            // read file via ASSIMP
            AssimpContext importer = new AssimpContext();
            importer.SetConfig(new NormalSmoothingAngleConfig(66.0f));
            scene = importer.ImportFile(path, PostProcessPreset.TargetRealTimeMaximumQuality);
            //scene = importer.ImportFile(path, PostProcessSteps.Triangulate | PostProcessSteps.GenerateSmoothNormals | PostProcessSteps.FlipUVs | PostProcessSteps.CalculateTangentSpace);

            // check for errors
            if(scene==null || scene.MeshCount <=0) // if is Not Zero
            {
                throw new Exception("ASSIMP: Error in importing file!!!");
            }

            // retrieve the directory path of the filepath
            directory = path.Substring(0, path.LastIndexOf('\\'));

            Matrix4x4 trans = Matrix4x4.Identity;
            // process ASSIMP's root node recursively
            processNode(scene.RootNode, scene, trans);
        }

        // processes a node in a recursive fashion. Processes each individual mesh located at the node and repeats this process on its children nodes (if any).
        void processNode(Node node, Scene scene, Matrix4x4 parent_trans)
        {
            Matrix4x4 child_trans = parent_trans * node.Transform;
            // process each mesh located at the current node
            for (int i = 0; i < node.MeshCount; i++)
            {
                // the node object only contains indices to index the actual objects in the scene. 
                // the scene contains all the data, node is just to keep stuff organized (like relations between nodes).
                Assimp.Mesh mesh = scene.Meshes[node.MeshIndices[i]];
                meshes.Add(processMesh(mesh, scene, child_trans));
            }
            // after we've processed all of the meshes (if any) we then recursively process each of the children nodes
            for (int i = 0; i < node.ChildCount; i++)
            {
                processNode(node.Children[i], scene, child_trans);
            }
        }

        Mesh processMesh(Assimp.Mesh mesh, Scene scene, Matrix4x4 trans)
        {
            // data to fill
            List<Vertex> vertices = new List<Vertex>();
            List<int> indices = new List<int>();
            List<Texture> textures = new List<Texture>();

            Matrix3x3 normal_trans = new Matrix3x3(trans);
            Matrix3x3 trans3x3 = new Matrix3x3(trans);
            if (mesh.HasNormals)
            {
                normal_trans.Inverse();
                normal_trans.Transpose();
            }
            // walk through each of the mesh's vertices
            for (int i = 0; i < mesh.VertexCount; i++)
            {
                Vertex vertex = new Vertex();
                vertex.Position = new Vector3();

                float x = trans.A1 * mesh.Vertices[i].X + trans.A2 * mesh.Vertices[i].Y + trans.A3 * mesh.Vertices[i].Z + trans.A4;
                float y = trans.B1 * mesh.Vertices[i].X + trans.B2 * mesh.Vertices[i].Y + trans.B3 * mesh.Vertices[i].Z + trans.B4;
                float z = trans.C1 * mesh.Vertices[i].X + trans.C2 * mesh.Vertices[i].Y + trans.C3 * mesh.Vertices[i].Z + trans.C4; 
                float w = trans.D1 * mesh.Vertices[i].X + trans.D2 * mesh.Vertices[i].Y + trans.D3 * mesh.Vertices[i].Z + trans.D4; 

                // positions
                vertex.Position.x = x/w;
                vertex.Position.y = y/w;
                vertex.Position.z = z/w;

                // normals
                if (mesh.HasNormals)
                {
                    // normal vector is a direction
                    x = normal_trans.A1 * mesh.Normals[i].X + normal_trans.A2 * mesh.Normals[i].Y + normal_trans.A3 * mesh.Normals[i].Z;
                    y = normal_trans.B1 * mesh.Normals[i].X + normal_trans.B2 * mesh.Normals[i].Y + normal_trans.B3 * mesh.Normals[i].Z;
                    z = normal_trans.C1 * mesh.Normals[i].X + normal_trans.C2 * mesh.Normals[i].Y + normal_trans.C3 * mesh.Normals[i].Z;

                    // Do normalize
                    float len = (float)Math.Sqrt(x * x + y * y + z * z);
                    vertex.Normal = new Vector3();
                    vertex.Normal.x = x/len;
                    vertex.Normal.y = y/len;
                    vertex.Normal.z = z/len;
                }
                
                if (mesh.HasTangentBasis)
                {
                    // tangent vector is a direction (I guess using trans3x3, maybe wrong)
                    x = trans3x3.A1 * mesh.Tangents[i].X + trans3x3.A2 * mesh.Tangents[i].Y + trans3x3.A3 * mesh.Tangents[i].Z;
                    y = trans3x3.B1 * mesh.Tangents[i].X + trans3x3.B2 * mesh.Tangents[i].Y + trans3x3.B3 * mesh.Tangents[i].Z;
                    z = trans3x3.C1 * mesh.Tangents[i].X + trans3x3.C2 * mesh.Tangents[i].Y + trans3x3.C3 * mesh.Tangents[i].Z;

                    float len = (float)Math.Sqrt(x * x + y * y + z * z);
                    // tangent
                    vertex.Tangent = new Vector3();
                    vertex.Tangent.x = x/len;
                    vertex.Tangent.y = y/len;
                    vertex.Tangent.z = z/len;

                    // bitangent vector is a direction (I guess using trans3x3, maybe wrong)
                    x = trans3x3.A1 * mesh.BiTangents[i].X + trans3x3.A2 * mesh.BiTangents[i].Y + trans3x3.A3 * mesh.BiTangents[i].Z;
                    y = trans3x3.B1 * mesh.BiTangents[i].X + trans3x3.B2 * mesh.BiTangents[i].Y + trans3x3.B3 * mesh.BiTangents[i].Z;
                    z = trans3x3.C1 * mesh.BiTangents[i].X + trans3x3.C2 * mesh.BiTangents[i].Y + trans3x3.C3 * mesh.BiTangents[i].Z;

                    len = (float)Math.Sqrt(x * x + y * y + z * z);
                    // bitangent
                    vertex.Bitangent = new Vector3();
                    vertex.Bitangent.x = x/len;
                    vertex.Bitangent.y = y/len;
                    vertex.Bitangent.z = z/len;
                }


                // texture coordinates
                if (mesh.HasTextureCoords(0)) // does the mesh contain texture coordinates?
                {
                    vertex.TexCoords = new Vector2();
                    // a vertex can contain up to 8 different texture coordinates. We thus make the assumption that we won't 
                    // use models where a vertex can have multiple texture coordinates so we always take the first set (0).
                    vertex.TexCoords.x = mesh.TextureCoordinateChannels[0][i].X;
                    vertex.TexCoords.y = mesh.TextureCoordinateChannels[0][i].Y;
                }
                else
                {
                    vertex.TexCoords = new Vector2();
                    vertex.TexCoords.x = 0.0f;
                    vertex.TexCoords.y = 0.0f;
                }
                vertices.Add(vertex);
            }
            // now wak through each of the mesh's faces (a face is a mesh its triangle) and retrieve the corresponding vertex indices.
            for (int i = 0; i < mesh.FaceCount; i++)
            {
                Face face = mesh.Faces[i];
                // retrieve all indices of the face and store them in the indices vector
                for (int j = 0; j < face.IndexCount; j++)
                    indices.Add(face.Indices[j]);
            }

            // process materials
            Material material = new Material(); // Our materail
            Assimp.Material assimp_material = scene.Materials[mesh.MaterialIndex];
            // we assume a convention for sampler names in the shaders. Each diffuse texture should be named
            // as 'texture_diffuseN' where N is a sequential number ranging from 1 to MAX_SAMPLER_NUMBER. 
            // Same applies to other texture as the following list summarizes:
            // diffuse: texture_diffuseN
            // specular: texture_specularN
            // normal: texture_normalN
            
            // 1. diffuse maps
            List<Texture> diffuseMaps = loadMaterialTextures(assimp_material, TextureType.Diffuse, "texture_diffuse");
            for (int i = 0; i < diffuseMaps.Count; i++) textures.Add(diffuseMaps[i]);
            // 2. specular maps
            List<Texture> specularMaps = loadMaterialTextures(assimp_material, TextureType.Specular, "texture_specular");
            for (int i = 0; i < specularMaps.Count; i++) textures.Add(specularMaps[i]);
            // 3. normal maps
            List<Texture> normalMaps = loadMaterialTextures(assimp_material, TextureType.Normals, "texture_normal");
            for (int i = 0; i < normalMaps.Count; i++) textures.Add(normalMaps[i]);
            // 4. height maps
            List<Texture> heightMaps = loadMaterialTextures(assimp_material, TextureType.Height, "texture_height");
            for (int i = 0; i < heightMaps.Count; i++) textures.Add(heightMaps[i]);

            // if no texture, then get material ambient, diffuse, and specular 
            if (mesh.HasTextureCoords(0))
                material.hasTexture = true;
            else material.hasTexture = false;
   
            material.ambient.R = material.ambient.G = material.ambient.B = material.ambient.A = 1.0f;
            if (assimp_material.HasColorAmbient)
            {
                material.hasAmbient = true;
                material.ambient.R = assimp_material.ColorAmbient.R;
                material.ambient.G = assimp_material.ColorAmbient.G;
                material.ambient.B = assimp_material.ColorAmbient.B;
                material.ambient.A = assimp_material.ColorAmbient.A;
            }
            else material.hasAmbient = false;

            material.diffuse.R = material.diffuse.G = material.diffuse.B = material.diffuse.A = 1.0f;
            if (assimp_material.HasColorDiffuse)
            {
                material.hasDiffuse = true;
                material.diffuse.R = assimp_material.ColorDiffuse.R;
                material.diffuse.G = assimp_material.ColorDiffuse.G;
                material.diffuse.B = assimp_material.ColorDiffuse.B;
                material.diffuse.A = assimp_material.ColorDiffuse.A;
            }
            else material.hasDiffuse = false;

            material.specular.R = material.specular.G = material.specular.B = material.specular.A = 1.0f;
            if (assimp_material.HasColorSpecular)
            {
                material.hasSpecular = true;
                material.specular.R = assimp_material.ColorSpecular.R;
                material.specular.G = assimp_material.ColorSpecular.G;
                material.specular.B = assimp_material.ColorSpecular.B;
                material.specular.A = assimp_material.ColorSpecular.A;
            }
            else material.hasSpecular = false;

            material.shininess = 1.0f;
            if (assimp_material.HasShininess)
            {
                material.hasShininess = true;
                material.shininess = assimp_material.Shininess;
            }
            else material.hasShininess = false;

            // return a mesh object created from the extracted mesh data
            return new Mesh(vertices, indices, material, textures);
        }

        // checks all material textures of a given type and loads the textures if they're not loaded yet.
        // the required info is returned as a Texture struct.
        List<Texture> loadMaterialTextures(Assimp.Material mat, TextureType type, string typeName)
        {
            List<Texture> textures = new List<Texture>();
            for(int i = 0; i < mat.GetMaterialTextureCount(type); i++)
            {
                TextureSlot tex;
                mat.GetMaterialTexture(type, i, out tex);
                // check if texture was loaded before and if so, continue to next iteration: skip loading a new texture
                bool skip = false;
                for (int j = 0; j < textures_loaded.Count; j++)
                {
                    if(textures_loaded[j].path==tex.FilePath)
                    {
                        textures.Add(textures_loaded[j]);
                        skip = true; // a texture with the same filepath has already been loaded, continue to next one. (optimization)
                        break;
                    }
                }
                if (!skip)
                {   // if texture hasn't been loaded already, load it
                    Texture texture = new Texture();
                    texture.id = TextureFromFile(tex.FilePath, this.directory);
                    texture.type = typeName;
                    texture.path = tex.FilePath;
                    textures.Add(texture);
                    textures_loaded.Add(texture);  // store it as texture loaded for entire model, to ensure we won't unnecesery load duplicate textures.
                }
            }
            return textures;
        }
        uint TextureFromFile(string path, string directory, bool gamma = false)
        {
            string filename = path;
            filename = directory + '\\' + filename;

            uint[] textureID = new uint[1];
            Gl.glGenTextures(1, textureID);
            int BitsPerPixel;
            if (Il.ilLoadImage(filename)) //載入影像檔
            {
                BitsPerPixel = Il.ilGetInteger(Il.IL_IMAGE_BITS_PER_PIXEL); //取得儲存每個像素的位元數
                int format = Gl.GL_RGB;
                if (BitsPerPixel == 8)
                    format = Gl.GL_RED;
                else if (BitsPerPixel == 24)
                    format = Gl.GL_RGB;
                else if (BitsPerPixel == 32)
                    format = Gl.GL_RGBA;
                int width = Il.ilGetInteger(Il.IL_IMAGE_WIDTH); //取得影像寬度
                int height = Il.ilGetInteger(Il.IL_IMAGE_HEIGHT); //取得影像寬度
                width = (width / 4) * 4; // Let the width be the multiple of 4
                int Depth = Il.ilGetInteger(Il.IL_IMAGE_DEPTH);
                Ilu.iluScale(width, height, Depth); 
                Gl.glBindTexture(Gl.GL_TEXTURE_2D, textureID[0]);
                Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, format, width, height, 0, Il.ilGetInteger(Il.IL_IMAGE_FORMAT), Gl.GL_UNSIGNED_BYTE, Il.ilGetData());
                Gl.glGenerateMipmap(Gl.GL_TEXTURE_2D);

                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_REPEAT);
                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_REPEAT);
                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR_MIPMAP_LINEAR);
                //Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
            }
            else
            {
                throw new Exception("Texture failed to load at path: " + path);
            }
            return textureID[0];
        }
        private Matrix4 FromMatrix(Matrix4x4 mat)
        {
            Matrix4 m = new Matrix4();
            m.data[0, 0] = mat.A1;
            m.data[0, 1] = mat.A2;
            m.data[0, 2] = mat.A3;
            m.data[0, 3] = mat.A4;
            m.data[1, 0] = mat.B1;
            m.data[1, 1] = mat.B2;
            m.data[1, 2] = mat.B3;
            m.data[1, 3] = mat.B4;
            m.data[2, 0] = mat.C1;
            m.data[2, 1] = mat.C2;
            m.data[2, 2] = mat.C3;
            m.data[2, 3] = mat.C4;
            m.data[3, 0] = mat.D1;
            m.data[3, 1] = mat.D2;
            m.data[3, 2] = mat.D3;
            m.data[3, 3] = mat.D4;
            return m;
        }

        private Vector3 FromVector(Vector3D vec)
        {
            Vector3 v;
            v.x = vec.X;
            v.y = vec.Y;
            v.z = vec.Z;
            return v;
        }

        private Color4 FromColor(Color4D color)
        {
            Color4 c;
            c.R = color.R;
            c.G = color.G;
            c.B = color.B;
            c.A = color.A;
            return c;
        }
     };
}
