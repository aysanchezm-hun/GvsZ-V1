using System.Collections.Generic;
using UnityEngine;
using LabSW.Core;

namespace LabSW.View
{
    public class BoardView : MonoBehaviour
    {
        [SerializeField] private float tileSize = 1.0f;
        [SerializeField] private Sprite whiteTileSprite;
        [SerializeField] private Sprite darkTileSprite;
        [SerializeField] private Sprite highlightSprite;
        [SerializeField] private Sprite foxTriangleSprite;
        [SerializeField] private Sprite chickenCircleSprite;

        private readonly SpriteRenderer[,] tileRenderers = new SpriteRenderer[BoardState.BoardSize, BoardState.BoardSize];
        private readonly SpriteRenderer[,] highlights = new SpriteRenderer[BoardState.BoardSize, BoardState.BoardSize];
        private readonly Dictionary<Vector2Int, SpriteRenderer> pieceRenderers = new Dictionary<Vector2Int, SpriteRenderer>();

        private BoardTheme currentTheme;

        public Vector3 GridToWorld(int x, int y)
        {
            return new Vector3((x - 3.5f) * tileSize, (3.5f - y) * tileSize, 0f);
        }

        public bool WorldToGrid(Vector3 worldPos, out Vector2Int gridPos)
        {
            int x = Mathf.RoundToInt(worldPos.x / tileSize + 3.5f);
            int y = Mathf.RoundToInt(3.5f - worldPos.y / tileSize);
            gridPos = new Vector2Int(x, y);
            return BoardState.IsValidBounds(gridPos.x, gridPos.y);
        }

        public void SetSprites(Sprite whiteTile, Sprite darkTile, Sprite highlight, Sprite foxTriangle, Sprite chickenCircle)
        {
            whiteTileSprite = whiteTile;
            darkTileSprite = darkTile;
            highlightSprite = highlight;
            foxTriangleSprite = foxTriangle;
            chickenCircleSprite = chickenCircle;
        }

        public void RenderBoard()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyObject(transform.GetChild(i).gameObject);
            }
            pieceRenderers.Clear();

            GameObject tilesGroup = new GameObject("TilesGroup");
            tilesGroup.transform.SetParent(transform);

            GameObject highlightsGroup = new GameObject("HighlightsGroup");
            highlightsGroup.transform.SetParent(transform);

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    bool isWhite = BoardState.IsWhiteCell(x, y);
                    Vector3 pos = GridToWorld(x, y);

                    GameObject t = new GameObject($"Tile_{x}_{y}");
                    t.transform.SetParent(tilesGroup.transform);
                    t.transform.position = pos;
                    SpriteRenderer sr = t.AddComponent<SpriteRenderer>();
                    sr.sprite = isWhite ? whiteTileSprite : darkTileSprite;
                    sr.sortingOrder = 0;
                    tileRenderers[x, y] = sr;

                    GameObject hl = new GameObject($"Highlight_{x}_{y}");
                    hl.transform.SetParent(highlightsGroup.transform);
                    hl.transform.position = pos;
                    SpriteRenderer hsr = hl.AddComponent<SpriteRenderer>();
                    hsr.sprite = highlightSprite;
                    hsr.sortingOrder = 1;
                    hsr.enabled = false;
                    highlights[x, y] = hsr;
                }
            }
        }

        public void RenderPieces(BoardState state)
        {
            foreach (var kvp in pieceRenderers)
            {
                if (kvp.Value != null) DestroyObject(kvp.Value.gameObject);
            }
            pieceRenderers.Clear();

            GameObject piecesGroup = transform.Find("PiecesGroup")?.gameObject;
            if (piecesGroup == null)
            {
                piecesGroup = new GameObject("PiecesGroup");
                piecesGroup.transform.SetParent(transform);
            }

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    CellState cs = state.GetCell(x, y);
                    if (cs == CellState.Empty) continue;

                    Vector2Int gpos = new Vector2Int(x, y);
                    Vector3 wpos = GridToWorld(x, y);

                    GameObject p = new GameObject($"{cs}_{x}_{y}");
                    p.transform.SetParent(piecesGroup.transform);
                    p.transform.position = wpos;

                    SpriteRenderer psr = p.AddComponent<SpriteRenderer>();
                    psr.sprite = (cs == CellState.Fox) ? foxTriangleSprite : chickenCircleSprite;
                    psr.sortingOrder = 5;

                    // Tintar según el tema actual
                    psr.color = (cs == CellState.Fox) ? currentTheme.FoxColor : currentTheme.ChickenColor;

                    pieceRenderers[gpos] = psr;
                }
            }
        }

        public void ApplyTheme(BoardTheme theme)
        {
            currentTheme = theme;

            // 1. Aplicar colores a las casillas
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (tileRenderers[x, y] != null)
                    {
                        bool isWhite = BoardState.IsWhiteCell(x, y);
                        tileRenderers[x, y].color = isWhite ? theme.LightTileColor : theme.DarkTileColor;
                    }
                }
            }

            // 2. Aplicar color a las fichas
            foreach (var kvp in pieceRenderers)
            {
                if (kvp.Value != null)
                {
                    bool isFox = kvp.Value.name.StartsWith("Fox");
                    kvp.Value.color = isFox ? theme.FoxColor : theme.ChickenColor;
                }
            }
        }

        public void HighlightCells(List<Vector2Int> validMoves)
        {
            ClearHighlights();
            foreach (var m in validMoves)
            {
                if (BoardState.IsValidBounds(m.x, m.y) && highlights[m.x, m.y] != null)
                {
                    highlights[m.x, m.y].enabled = true;
                    highlights[m.x, m.y].color = currentTheme.HighlightColor;
                }
            }
        }

        public void ClearHighlights()
        {
            for (int x = 0; x < 8; x++)
                for (int y = 0; y < 8; y++)
                    if (highlights[x, y] != null) highlights[x, y].enabled = false;
        }

        private static void DestroyObject(Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying)
                Destroy(obj);
            else
                DestroyImmediate(obj);
        }
    }
}