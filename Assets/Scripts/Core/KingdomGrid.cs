using UnityEngine;

/// <summary>
/// Controla o Grid lógico da cidade.
/// Converte posições do mundo para células
/// e células para posições do mundo.
/// </summary>
public class KingdomGrid : MonoBehaviour
{
    [Header("Configuração do Grid")]
    [SerializeField] private int width = 20;
    [SerializeField] private int height = 20;

    [SerializeField] private float cellSize = 2f;

    [Header("Visual")]
    [SerializeField] private bool showGrid = true;
    [SerializeField] private Color gridColor = Color.white;

    public int Width => width;
    public int Height => height;
    public float CellSize => cellSize;

    private void OnDrawGizmos()
    {
        if (!showGrid)
            return;

        Gizmos.color = gridColor;

        Vector3 origin = transform.position;

        for (int x = 0; x <= width; x++)
        {
            Vector3 start =
                origin + new Vector3(
                    x * cellSize,
                    0.05f,
                    0
                );

            Vector3 end =
                origin + new Vector3(
                    x * cellSize,
                    0.05f,
                    height * cellSize
                );

            Gizmos.DrawLine(start, end);
        }

        for (int z = 0; z <= height; z++)
        {
            Vector3 start =
                origin + new Vector3(
                    0,
                    0.05f,
                    z * cellSize
                );

            Vector3 end =
                origin + new Vector3(
                    width * cellSize,
                    0.05f,
                    z * cellSize
                );

            Gizmos.DrawLine(start, end);
        }
    }

    /// <summary>
    /// Converte uma posição do mundo para coordenada de célula.
    /// </summary>
    public Vector2Int WorldToCell(Vector3 worldPosition)
    {
        Vector3 localPosition =
            worldPosition - transform.position;

        int x = Mathf.FloorToInt(
            localPosition.x / cellSize
        );

        int z = Mathf.FloorToInt(
            localPosition.z / cellSize
        );

        return new Vector2Int(x, z);
    }

    /// <summary>
    /// Converte uma célula para o centro dela no mundo.
    /// </summary>
    public Vector3 CellToWorld(Vector2Int cell)
    {
        return transform.position +
               new Vector3(
                   cell.x * cellSize + cellSize * 0.5f,
                   0,
                   cell.y * cellSize + cellSize * 0.5f
               );
    }

    /// <summary>
    /// Verifica se uma célula está dentro do Grid.
    /// </summary>
    public bool IsInsideGrid(Vector2Int cell)
    {
        return cell.x >= 0 &&
               cell.x < width &&
               cell.y >= 0 &&
               cell.y < height;
    }
}