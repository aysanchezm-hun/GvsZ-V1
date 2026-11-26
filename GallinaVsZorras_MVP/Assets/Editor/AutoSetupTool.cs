using System.IO;
using UnityEditor;
using UnityEngine;
using LabSW.Core;
using LabSW.View;
using LabSW.Controller;

namespace LabSW.Editor
{
    public static class AutoSetupTool
    {
        private const string ArtPath = "Assets/Art/Generated/";

        [MenuItem("LABSW/Construir Escena MVP")]
        public static void BuildMVPScene()
        {
            EnsureDirectoriesExist();
            GenerateSprites();
            AssetDatabase.Refresh();

            Sprite whiteTile = AssetDatabase.LoadAssetAtPath<Sprite>(ArtPath + "cell_white.png");
            Sprite darkTile = AssetDatabase.LoadAssetAtPath<Sprite>(ArtPath + "cell_dark.png");
            Sprite highlight = AssetDatabase.LoadAssetAtPath<Sprite>(ArtPath + "cell_highlight.png");
            Sprite foxTriangle = AssetDatabase.LoadAssetAtPath<Sprite>(ArtPath + "fox_triangle.png");
            Sprite chickenCircle = AssetDatabase.LoadAssetAtPath<Sprite>(ArtPath + "chicken_circle.png");

            CleanScene();
            ConfigureCamera();

            GameObject boardContainer = new GameObject("BoardContainer");
            BoardView boardView = boardContainer.AddComponent<BoardView>();
            boardView.SetSprites(whiteTile, darkTile, highlight, foxTriangle, chickenCircle);

            GameObject managerObj = new GameObject("GameManager");
            GameManager manager = managerObj.AddComponent<GameManager>();

            Selection.activeGameObject = boardContainer;
            Debug.Log("<b>[LABSW] MVP Construido con Éxito.</b> Tablero de 64 casillas (piezas en casillas blancas), 4 Zorras (Triángulos) y 1 Gallina (Círculo).");
        }

        private static void EnsureDirectoriesExist()
        {
            if (!Directory.Exists(ArtPath)) Directory.CreateDirectory(ArtPath);
        }

        private static void GenerateSprites()
        {
            CreateSquarePNG(ArtPath + "cell_white.png", 64, new Color(0.95f, 0.95f, 0.92f, 1.0f));
            CreateSquarePNG(ArtPath + "cell_dark.png", 64, new Color(0.35f, 0.38f, 0.42f, 1.0f));
            CreateSquarePNG(ArtPath + "cell_highlight.png", 64, new Color(0.20f, 0.95f, 0.30f, 0.55f));
            CreateTrianglePNG(ArtPath + "fox_triangle.png", 64, new Color(0.95f, 0.40f, 0.10f, 1.0f));
            CreateCirclePNG(ArtPath + "chicken_circle.png", 64, new Color(1.0f, 0.88f, 0.20f, 1.0f));
        }

        private static void CreateSquarePNG(string path, int size, Color color)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pix = new Color[size * size];
            for (int i = 0; i < pix.Length; i++) pix[i] = color;
            tex.SetPixels(pix);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            ImportAsSprite(path, false);
        }

        private static void CreateCirclePNG(string path, int size, Color color)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size / 2.0f;
            float radius = (size / 2.0f) - 3.0f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    tex.SetPixel(x, y, (dist <= radius) ? color : Color.clear);
                }
            }
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            ImportAsSprite(path, true);
        }

        private static void CreateTrianglePNG(string path, int size, Color color)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

            Vector2 p1 = new Vector2(size / 2.0f, size - 4.0f);
            Vector2 p2 = new Vector2(4.0f, 4.0f);
            Vector2 p3 = new Vector2(size - 4.0f, 4.0f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pt = new Vector2(x, y);
                    tex.SetPixel(x, y, IsPointInTriangle(pt, p1, p2, p3) ? color : Color.clear);
                }
            }
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            ImportAsSprite(path, true);
        }

        private static bool IsPointInTriangle(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3)
        {
            float d1 = Sign(pt, v1, v2);
            float d2 = Sign(pt, v2, v3);
            float d3 = Sign(pt, v3, v1);
            bool has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);
            return !(has_neg && has_pos);
        }

        private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }

        private static void ImportAsSprite(string path, bool alpha)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            TextureImporter imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp != null)
            {
                imp.textureType = TextureImporterType.Sprite;
                imp.spriteImportMode = SpriteImportMode.Single;
                imp.spritePixelsPerUnit = 64;
                imp.alphaIsTransparency = alpha;
                imp.SaveAndReimport();
            }
        }

        private static void CleanScene()
        {
            GameObject b = GameObject.Find("BoardContainer");
            if (b != null) Object.DestroyImmediate(b);
            GameObject m = GameObject.Find("GameManager");
            if (m != null) Object.DestroyImmediate(m);
        }

        private static void ConfigureCamera()
        {
            Camera main = Camera.main;
            if (main == null)
            {
                GameObject cam = new GameObject("Main Camera");
                main = cam.AddComponent<Camera>();
                cam.tag = "MainCamera";
            }
            main.orthographic = true;
            main.orthographicSize = 5.2f;
            main.transform.position = new Vector3(0f, 0f, -10f);
            main.backgroundColor = new Color(0.15f, 0.17f, 0.22f);
        }
    }
}