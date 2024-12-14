using UnityEngine;

namespace SpaceSimFramework
{
public class GridOverlay : MonoBehaviour
{
    public bool ShouldRender = false;

    public int GridSizeX;
    public int GridSizeY;
    public int GridSizeZ;

    public float LargeStep;

    public float StartX;
    public float StartY;
    public float StartZ;

    private Material _lineMaterial;

    public Color MainColor = new Color(0f, 1f, 0f, 1f);

    void CreateLineMaterial()
    {
        // Unity has a built-in shader that is useful for drawing
        // simple colored things.
        var shader = Shader.Find("Hidden/Internal-Colored");
        _lineMaterial = new Material(shader);
        _lineMaterial.hideFlags = HideFlags.HideAndDontSave;
        // Turn on alpha blending
        _lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        _lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        // Turn backface culling off
        _lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        // Turn off depth writes
        _lineMaterial.SetInt("_ZWrite", 0);
    }

    void OnPostRender()
    {
        DrawSquareGrid();
        // Draw scanner circles
        foreach(GameObject ship in Player.Instance.Ships)
        {
            DrawCircle(ship.transform.position, ship.GetComponent<Ship>().ScannerRange);
        }
    }

    private void DrawSquareGrid()
    {
        if (!_lineMaterial)
        {
            CreateLineMaterial();
        }
        // set the current material
        _lineMaterial.SetPass(0);

        GL.Begin(GL.LINES);

        if (ShouldRender)
        {
            GL.Color(MainColor);

            //Layers
            for (float j = 0; j <= GridSizeY; j += LargeStep)
            {
                //X axis lines
                for (float i = 0; i <= GridSizeZ; i += LargeStep)
                {
                    GL.Vertex3(StartX, StartY + j, StartZ + i);
                    GL.Vertex3(StartX + GridSizeX, StartY + j, StartZ + i);
                }

                //Z axis lines
                for (float i = 0; i <= GridSizeX; i += LargeStep)
                {
                    GL.Vertex3(StartX + i, StartY + j, StartZ);
                    GL.Vertex3(StartX + i, StartY + j, StartZ + GridSizeZ);
                }
            }

            //Y axis lines
            for (float i = 0; i <= GridSizeZ; i += LargeStep)
            {
                for (float k = 0; k <= GridSizeX; k += LargeStep)
                {
                    GL.Vertex3(StartX + k, StartY, StartZ + i);
                    GL.Vertex3(StartX + k, StartY + GridSizeY, StartZ + i);
                }
            }
        }

        GL.End();
    }

    private void DrawCircle(Vector3 center, float radius)
    {
        if (!_lineMaterial)
        {
            CreateLineMaterial();
        }

        GL.PushMatrix();
        _lineMaterial.SetPass(0);
        GL.Begin(GL.LINES);
        GL.Color(Color.red);
        for (float theta = 0.0f; theta < (2 * Mathf.PI); theta += 0.01f)
        {
            Vector3 ci = (new Vector3(Mathf.Cos(theta) * radius + center.x, Mathf.Sin(theta) * radius + center.y, 0));
            GL.Vertex3(ci.x, 0, ci.y);
        }
        GL.End();
        GL.PopMatrix();
    }
}
}