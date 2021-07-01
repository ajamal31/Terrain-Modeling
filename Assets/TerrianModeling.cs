/* Purpose: Builds the terrain */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrianModeling : MonoBehaviour {

    // Use this for initialization
    void Start() {
        MakeTerrain();
    }


    /* Pupose: Builds a terrain using fractal technique
     * Pre: None
     * Post: A terrain built at runtime
     */
    void MakeTerrain() {

        // Size of the inital mesh.
        int size = 4;

        // Final size of the terrian
        int finalSize = 2048;

        Vector3[,] vertex2D = new Vector3[size + 1, size + 1];
        Vector3 location;

        // Make the initial mesh.
        for (int z = 0; z <= size; z++) {
            for (int x = 0; x <= size; x++) {
                location = new Vector3(x, Random.Range(-500f, 1200f), z);
                vertex2D[z, x] = location;
            }
        }

        // Will hold all the vertices for the entire terrain.
        Vector3[,] newVertex = new Vector3[finalSize + 1, finalSize + 1];

        // Do the first fractal.
        size *= 2;
        newVertex = MakeFractal(vertex2D, size);

        // Keep doing fractal until the final size is reached.
        for (size = size * 2; size <= finalSize; size *= 2) {
            newVertex = MakeFractal(newVertex, size);
        }

        int startX = 0;
        int startZ = 0;
        int zCount = 128;
        int xCount = 128;
        int buildSize = 2048;

        Texture2D texture = new Texture2D(2049, 2049);
        Vector3 texLocation;

        // Creates the texture map.
        for (int z = 0; z < newVertex.GetLength(0); z++) {
            for (int x = 0; x < newVertex.GetLength(1); x++) {
                texLocation = newVertex[z, x];
                // Blue color is set here for altitudes below 0.
                if (texLocation.y <= 0) {
                    texture.SetPixel(x, z, new Color32(0, 89, 179, 0));
                    // Green color is set here for altitudes between 0 and 400.
                } else if (texLocation.y > 0 && texLocation.y <= 400) {
                    texture.SetPixel(x, z, new Color32(0, 51, 0, 0));
                    // Grey color is set here for altitudes between 400 and 700.
                } else if (texLocation.y > 400 && texLocation.y <= 700) {
                    texture.SetPixel(x, z, new Color32(128, 128, 128, 0));
                    // White color is set here for altitudes higher than 700.
                } else if (texLocation.y > 700) {
                    texture.SetPixel(x, z, Color.white);
                }
            }
        }
        texture.Apply();

        // Builds the submeshes (129 by 129 each). Each submesh is attached to a game object.
        for (int z = 0; z < buildSize; z += 128) {
            startX = 0;
            for (int x = 0; x < buildSize; x += 128) {
                GameObject go = new GameObject();
                go.AddComponent<MeshFilter>();
                go.AddComponent<MeshRenderer>();
                Renderer rend = go.GetComponent<Renderer>();
                rend.material.mainTexture = texture;
                MeshFilter filter = go.GetComponent<MeshFilter>();
                // Makes the mesh here.
                filter.mesh = MakeMesh(newVertex, startX, startX + xCount, startZ, startZ + zCount);
                startX += 128;
            }
            startZ += 128;
        }
    }

    /* Pupose: Builds a mesh
     * Parameters: vertex - the vertices that get added to the mesh.
     * 			   startX, endX, startZ, endZ - start and end points to determine which part of the 2k by 2k to build.
     * Pre: Have a vertex 2d array that contains Vector3 objects
     * Post: Return a submesh
     */
    Mesh MakeMesh(Vector3[,] vertex, int startX, int endX, int startZ, int endZ) {

        // Variable declaration and intialization.
        Mesh mesh = new Mesh();
        int size = endX - startX;
        Vector3[,] subVertex = new Vector3[size + 1, size + 1];
        Vector2[] uv = new Vector2[(size + 1) * (size + 1)];
        int[,] pt = new int[size + 1, size + 1];
        int xCount = 0;
        int zCount = 0;
        int idx = 0;

        // Build the submeshes vertex and uv.
        for (int z = startZ; z <= endZ; z++) {
            xCount = 0;
            for (int x = startX; x <= endX; x++) {
                subVertex[zCount, xCount] = vertex[z, x];
                pt[zCount, xCount] = idx;
                Vector2 tex = new Vector2((vertex[z, x].x) / 2048, (vertex[z, x].z) / 2048);
                uv[idx] = tex;
                idx++;
                xCount++;
            }
            zCount++;
        }

        // Attach the components to the mesh.
        mesh.vertices = ConvertTo1D(subVertex);
        mesh.uv = uv;
        mesh.triangles = MakeTriangles(pt, startX, endX);
        mesh.RecalculateNormals();
        return mesh;
    }

    /* Pupose: Build the triangles array that the mesh object needs.
     * Parameters: pt - array that contains the indexes of each point.
     * 			   startX, endX - start and end points to determine the returning array
     * Pre: Have a 2d array that contains the indexes at each point.
     * Post: 1D array that contains the vertices index order for the mesh object.
     */
    int[] MakeTriangles(int[,] pt, int start, int end) {
        int size = end - start;
        int tCount = 6 * (size * size);
        int[] triangle = new int[tCount];
        int tPosn = 0;

        // Builds the triangle array.
        for (int z = 0; z < size; z++) {
            for (int x = 0; x < size; x++) {
                // Left triangle
                triangle[tPosn++] = pt[z, x];
                triangle[tPosn++] = pt[z + 1, x];
                triangle[tPosn++] = pt[z + 1, x + 1];

                // Right triangle
                triangle[tPosn++] = pt[z, x];
                triangle[tPosn++] = pt[z + 1, x + 1];
                triangle[tPosn++] = pt[z, x + 1];
            }
        }
        return triangle;
    }

    /* Pupose: Returns a 1D Vector array but converting a 2D Vector3 
     * Parameters: vertex2D - a 2D Vector3 array
     * Pre: Have a 2d Vector3 array
     * Post: returns the data from the 2D array as a 1D array for the mesh object
     */
    Vector3[] ConvertTo1D(Vector3[,] vertex2D) {
        int size = vertex2D.GetLength(0);
        int vCount = size * size;
        Vector3[] vertex = new Vector3[vCount];
        int count = 0;

        // Assign the values from the 2d array to the 1d array.
        for (int z = 0; z < size; z++) {
            for (int x = 0; x < size; x++) {
                vertex[count] = vertex2D[z, x];
                count++;
            }
        }
        return vertex;
    }

    /* Pupose: Does the fractal technique on a 2D vertex array.
     * Parameters: currVertex - a 2D Vector3 array
     * 			   size - the size of the resulting array
     * Pre: Have a 2d Vector3 array
     * Post: 2D array that has the fractal calculations performed.
     */
    Vector3[,] MakeFractal(Vector3[,] currVertex, int size) {

        Vector3[,] newVertex = new Vector3[size + 1, size + 1]; // Will hold the new Vertices
        Vector3 location;
        Vector3 newLocation;
        float scaleX = 2f;
        float scaleZ = 2f;

        // Goes through the current vertices and use it to expand the mesh.
        for (int z = 0; z < (currVertex.GetLength(0)); z++) {
            for (int x = 0; x < (currVertex.GetLength(1)); x++) {
                location = currVertex[z, x];
                // Sets the current point.
                newLocation = new Vector3(location.x * scaleX, location.y, location.z * scaleZ);
                newVertex[(int)location.z * 2, (int)location.x * 2].x = newLocation.x;
                newVertex[(int)location.z * 2, (int)location.x * 2].y = newLocation.y;
                newVertex[(int)location.z * 2, (int)location.x * 2].z = newLocation.z;
            }
        }

        // Get the values of the center points by averaging the neighbours
        AverageNeighbours(newVertex);

        return newVertex;
    }

    /* Purpose: Calculates the points that average of the diagonal neighbours and the points that average the cardinal neighbours.
     * Post: The center points have the heights calculated
     */
    void AverageNeighbours(Vector3[,] vertex) {

        float averageHeight;
        float maxSeperation;
        float[] neighboursSeperation = new float[4];

        // Fill in the center points (average diagonal)
        for (int z = 1; z < vertex.GetLength(0); z += 2) {
            for (int x = 1; x < vertex.GetLength(1); x += 2) {
                // Calculation to get the height of the center point by averaging and randomizing with the max seperation.
                averageHeight = (vertex[z - 1, x - 1].y + vertex[z - 1, x + 1].y + vertex[z + 1, x - 1].y + vertex[z + 1, x + 1].y) / 4;
                neighboursSeperation[0] = Mathf.Abs(averageHeight - vertex[z - 1, x - 1].y);
                neighboursSeperation[1] = Mathf.Abs(averageHeight - vertex[z - 1, x + 1].y);
                neighboursSeperation[2] = Mathf.Abs(averageHeight - vertex[z + 1, x - 1].y);
                neighboursSeperation[3] = Mathf.Abs(averageHeight - vertex[z + 1, x + 1].y);
                maxSeperation = Mathf.Max(neighboursSeperation) * 0.5f; // 0.5f is the bumpiness coefficient.
                vertex[z, x].y = Random.Range(averageHeight - maxSeperation, averageHeight + maxSeperation);
                vertex[z, x].x = vertex[z - 1, x - 1].x + 1;
                vertex[z, x].z = vertex[z - 1, x - 1].z + 1;
            }
        }

        // Calculate the heights of the cardinal neighbours because they are averaged next.
        CalculateCardinals(vertex);

        // Average cardinal neighbours
        for (int z = 1; z < vertex.GetLength(0); z += 2) {
            for (int x = 1; x < vertex.GetLength(1); x += 2) {
                // Calculation to get the height of the center point by averaging and randomizing with the max seperation.
                averageHeight = (vertex[z, x - 1].y + vertex[z + 1, x].y + vertex[z, x + 1].y + vertex[z - 1, x].y) / 4;
                neighboursSeperation[0] = Mathf.Abs(averageHeight - vertex[z, x - 1].y);
                neighboursSeperation[1] = Mathf.Abs(averageHeight - vertex[z + 1, x].y);
                neighboursSeperation[2] = Mathf.Abs(averageHeight - vertex[z, x + 1].y);
                neighboursSeperation[3] = Mathf.Abs(averageHeight - vertex[z - 1, x].y);
                maxSeperation = Mathf.Max(neighboursSeperation) * 0.5f; // 0.5f is the bumpiness coefficient.
                vertex[z, x].y = Random.Range(averageHeight - maxSeperation, averageHeight + maxSeperation);
            }
        }
    }

    /* Purpose: Calculate the values of the cardinals neighbours
     * Post: the cardinals have the values.
     */
    void CalculateCardinals(Vector3[,] vertex) {
        float averageHeight;
        int size = vertex.GetLength(0);
        // Calculate the values of the cardinals neighbours starting a z=0 (for even rows)
        for (int z = 0; z < size; z += 2) {
            for (int x = 1; x < size; x += 2) {
                if (z == 0) {
                    averageHeight = (vertex[z, x - 1].y + vertex[z + 1, x].y + vertex[z, x + 1].y) / 3;
                } else if (z == (size - 1)) {
                    averageHeight = (vertex[z, x - 1].y + vertex[z - 1, x].y + vertex[z, x + 1].y) / 3;
                } else {
                    averageHeight = (vertex[z, x - 1].y + vertex[z + 1, x].y + vertex[z - 1, x].y + vertex[z, x + 1].y) / 4;
                }
                // Assign the x, y and z values to the point.
                vertex[z, x].x = x;
                vertex[z, x].y = averageHeight;
                vertex[z, x].z = z;
            }
        }

        // Calculate the values of the cardinals neighbours starting a z=1 (for odd rows)
        for (int z = 1; z < size; z += 2) {
            for (int x = 0; x < size; x += 2) {
                if (x == 0) {
                    averageHeight = (vertex[z - 1, x].y + vertex[z, x + 1].y + vertex[z + 1, x].y) / 3;
                } else if (x == (size - 1)) {
                    averageHeight = (vertex[z, x - 1].y + vertex[z - 1, x].y + vertex[z + 1, x].y) / 3;
                } else {
                    averageHeight = (vertex[z, x - 1].y + vertex[z + 1, x].y + vertex[z - 1, x].y + vertex[z, x + 1].y) / 4;
                }
                // Assign the x, y and z values to the point.
                vertex[z, x].x = x;
                vertex[z, x].y = averageHeight;
                vertex[z, x].z = z;
            }
        }
    }

    // Update is called once per frame
    void Update() {

    }
}
