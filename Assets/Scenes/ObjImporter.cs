using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

public class ObjImporter
{
    // Initialize Data Structures
    private struct meshStruct
    {
        public List<Vector3> vertices;
        public List<Vector3> normals;
        public List<Vector2> uv;
        public List<int> triangles;
        public List<Vector3> faceData;
        public string fileName;
    }

    // Populate Data Structures
    public Mesh ImportFile(string filePath)
    {
        Debug.Log("Initializing.");

        meshStruct newMesh = createMeshStruct(filePath);
        Mesh mesh = new Mesh();

        Vector3[] newVerts = new Vector3[newMesh.faceData.Count];
        Vector2[] newUVs = new Vector2[newMesh.faceData.Count];
        Vector3[] newNormals = new Vector3[newMesh.faceData.Count];

        Parallel.For(0, newMesh.faceData.Count, i =>
        {
            Vector3 v = newMesh.faceData[i];
            newVerts[i] = newMesh.vertices[(int)v.x - 1];
            if (v.y >= 1) newUVs[i] = newMesh.uv[(int)v.y - 1];
            if (v.z >= 1) newNormals[i] = newMesh.normals[(int)v.z - 1];
        });

        mesh.vertices = newVerts;
        mesh.uv = newUVs;
        mesh.normals = newNormals;
        mesh.triangles = newMesh.triangles.ToArray();

        mesh.RecalculateBounds();
        mesh.Optimize();

        return mesh;
    }
    // Create a meshStruct and parse the object file
    private static meshStruct createMeshStruct(string filename)
    {
        meshStruct mesh = new meshStruct
        {
            fileName = filename,
            vertices = new List<Vector3>(),
            normals = new List<Vector3>(),
            uv = new List<Vector2>(),
            triangles = new List<int>(),
            faceData = new List<Vector3>()
        };

        using (StreamReader stream = File.OpenText(filename))
        {
            string line;
            char[] splitIdentifier = { ' ' };
            char[] splitIdentifier2 = { '/' };
            while ((line = stream.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#") || IsIgnoredLine(line)) continue;

                string[] brokenString = line.Split(splitIdentifier, System.StringSplitOptions.RemoveEmptyEntries);
                switch (brokenString[0])
                {
                    case "v":
                        mesh.vertices.Add(new Vector3(
                            float.Parse(brokenString[1]),
                            float.Parse(brokenString[2]),
                            float.Parse(brokenString[3])));
                        break;
                    case "vt":
                        mesh.uv.Add(new Vector2(
                            float.Parse(brokenString[1]),
                            float.Parse(brokenString[2])));
                        break;
                    case "vn":
                        mesh.normals.Add(new Vector3(
                            float.Parse(brokenString[1]),
                            float.Parse(brokenString[2]),
                            float.Parse(brokenString[3])));
                        break;
                    case "f":
                        ParseFace(brokenString, mesh, splitIdentifier2);
                        break;
                }
            }
        }
        return mesh;
    }

    // Skip unimportant lines
    private static bool IsIgnoredLine(string line)
    {
        return line.StartsWith("usemtl") || line.StartsWith("g") || line.StartsWith("mtllib") || line.StartsWith("usemap");
    }

    // Parse face lines
    private static void ParseFace(string[] brokenString, meshStruct mesh, char[] splitIdentifier2)
    {
        List<int> intArray = new List<int>();
        for (int j = 1; j < brokenString.Length; j++)
        {
            string[] faceComponents = brokenString[j].Split(splitIdentifier2);
            Vector3 faceData = new Vector3
            {
                x = int.Parse(faceComponents[0]),
                y = faceComponents.Length > 1 && faceComponents[1] != "" ? int.Parse(faceComponents[1]) : -1,
                z = faceComponents.Length > 2 && faceComponents[2] != "" ? int.Parse(faceComponents[2]) : -1
            };
            mesh.faceData.Add(faceData);
            intArray.Add(mesh.faceData.Count - 1);
        }

        for (int j = 1; j + 2 < brokenString.Length; j++)
        {
            mesh.triangles.Add(intArray[0]);
            mesh.triangles.Add(intArray[j]);
            mesh.triangles.Add(intArray[j + 1]);
        }
    }
}