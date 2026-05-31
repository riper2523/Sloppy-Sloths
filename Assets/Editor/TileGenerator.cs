#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

public class TileGenerator : EditorWindow
{
    [MenuItem("Vehicle Tools/Generator Kafelków (Kierunkowe)")]
    public static void ShowWindow() => GetWindow<TileGenerator>("Tile Generator");

    public Texture2D baseTex; // Baza (Opcjonalnie)
    public Texture2D mainTex; // Główna część / Lufa (Wymagane)
    public string savePath = "Assets/Tiles/Generated/";

    // Kąty i ich przełączniki (zaznaczone domyślnie)
    private readonly int[] angles = { 0, 45, 90, 135, 180, 225, 270, 315 };
    public bool[] generateAngles = new bool[8] { true, true, true, true, true, true, true, true };

    void OnGUI()
    {
        GUILayout.Label("Generator Części (8 Kierunków)", EditorStyles.boldLabel);

        baseTex = (Texture2D)EditorGUILayout.ObjectField("Baza (Opcjonalne)", baseTex, typeof(Texture2D), false);
        mainTex = (Texture2D)EditorGUILayout.ObjectField("Część Obrotowa", mainTex, typeof(Texture2D), false);

        GUILayout.Space(10);
        GUILayout.Label("Wybierz rotacje do wygenerowania:", EditorStyles.boldLabel);

        // Rysowanie checkboxów w ładnej siatce
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < 8; i++)
        {
            generateAngles[i] = EditorGUILayout.ToggleLeft($"{angles[i]}°", generateAngles[i], GUILayout.Width(50));
            // Przejście do nowej linii po 4 elementach
            if ((i + 1) % 4 == 0)
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
            }
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);
        savePath = EditorGUILayout.TextField("Folder zapisu", savePath);

        GUILayout.Space(20);
        if (GUILayout.Button("Generuj Wybrane Kafelki", GUILayout.Height(40)))
        {
            if (mainTex == null)
            {
                Debug.LogError("Musisz przypisać przynajmniej Część Obrotową!");
                return;
            }
            if (baseTex != null && (baseTex.width != mainTex.width || baseTex.height != mainTex.height))
            {
                Debug.LogError("Jeśli używasz bazy, obie tekstury muszą mieć identyczne wymiary (np. 128x128)!");
                return;
            }
            GenerateTiles();
        }
    }

    void GenerateTiles()
    {
        bool generatedAny = false;

        for (int i = 0; i < 8; i++)
        {
            if (!generateAngles[i]) continue; // Pomijamy odznaczone kąty

            int angle = angles[i];

            // Obracamy główny obrazek zaawansowaną funkcją
            Texture2D rotatedMain = RotateTexturePixelArt(mainTex, angle);
            Texture2D finalTex;

            // Jeśli baza istnieje, mieszamy. Jeśli nie, po prostu zapisujemy obrócony obrazek.
            if (baseTex != null)
            {
                finalTex = CloneTexture(baseTex);
                MergeTextures(finalTex, rotatedMain);
            }
            else
            {
                finalTex = rotatedMain;
            }

            SaveTextureAsset(finalTex, angle);
            generatedAny = true;
        }

        if (generatedAny) Debug.Log("Zakończono generowanie kafelków!");
        AssetDatabase.Refresh();
    }

    // --- ZAAWANSOWANA FUNKCJA OBRACAJĄCA PIKSELE (Z TRYGONOMETRIĄ) ---
    // --- ZAAWANSOWANA FUNKCJA OBRACAJĄCA PIKSELE (Z TRYGONOMETRIĄ) ---
    Texture2D RotateTextureSmooth(Texture2D original, int angle)
    {
        if (angle == 0) return CloneTexture(original);

        int w = original.width;
        int h = original.height;
        Texture2D rotated = new Texture2D(w, h, TextureFormat.RGBA32, false);

        // KLUCZ 1: Precyzyjny środek siatki (dla indeksów 0 do w-1)
        float cx = (w - 1) / 2f;
        float cy = (h - 1) / 2f;

        float rad = -angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                float nx = x - cx;
                float ny = y - cy;

                float srcX = nx * cos - ny * sin + cx;
                float srcY = nx * sin + ny * cos + cy;

                if (srcX >= -0.5f && srcX < w - 0.5f && srcY >= -0.5f && srcY < h - 0.5f)
                {
                    // KLUCZ 2: Dodajemy +0.5f, aby idealnie trafić w środek piksela (UV space)
                    Color color = original.GetPixelBilinear((srcX + 0.5f) / w, (srcY + 0.5f) / h);
                    rotated.SetPixel(x, y, color);
                }
                else
                {
                    rotated.SetPixel(x, y, Color.clear);
                }
            }
        }
        rotated.Apply();
        return rotated;
    }

    Texture2D CloneTexture(Texture2D source)
    {
        Texture2D clone = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        clone.SetPixels(source.GetPixels());
        clone.Apply();
        return clone;
    }

    void MergeTextures(Texture2D baseTex, Texture2D overlayTex)
    {
        Color[] basePixels = baseTex.GetPixels();
        Color[] overlayPixels = overlayTex.GetPixels();

        for (int i = 0; i < basePixels.Length; i++)
        {
            if (overlayPixels[i].a > 0.05f)
            {
                basePixels[i] = Color.Lerp(basePixels[i], overlayPixels[i], overlayPixels[i].a);
            }
        }
        baseTex.SetPixels(basePixels);
        baseTex.Apply();
    }

    void SaveTextureAsset(Texture2D tex, int angle)
    {
        byte[] bytes = tex.EncodeToPNG();
        if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);

        string prefix = baseTex != null ? baseTex.name : mainTex.name;
        string pngName = $"Tile_{prefix}_{angle}deg.png";
        string fullPath = savePath + pngName;

        File.WriteAllBytes(fullPath, bytes);
        AssetDatabase.Refresh();

        // 1. Zmiana ustawień importu wygenerowanego PNG
        TextureImporter importer = AssetImporter.GetAtPath(fullPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;

            // --- DODAJ TE DWIE LINIJKI ---
            importer.filterMode = FilterMode.Point; // Gwarantuje ostre, "kwadratowe" piksele
            importer.textureCompression = TextureImporterCompression.Uncompressed; // Wyłącza kompresję psującą jakość
            // -----------------------------

            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);

            settings.spriteAlignment = (int)SpriteAlignment.Center;
            settings.spriteMeshType = SpriteMeshType.FullRect;

            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        // 2. Automatyczne stworzenie kafelka w Unity (Asset .asset)
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(fullPath);
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;

        string tilePath = savePath + $"Tile_{prefix}_{angle}deg.asset";
        AssetDatabase.CreateAsset(tile, tilePath);
    }
    // --- FUNKCJA OBRACAJĄCA PIKSELE (OSTRE KRAWĘDZIE - NEAREST NEIGHBOR) ---
    Texture2D RotateTexturePixelArt(Texture2D original, int angle)
    {
        if (angle == 0) return CloneTexture(original);

        int w = original.width;
        int h = original.height;
        Texture2D rotated = new Texture2D(w, h, TextureFormat.RGBA32, false);

        float cx = (w - 1) / 2f;
        float cy = (h - 1) / 2f;

        float rad = -angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                float nx = x - cx;
                float ny = y - cy;

                float srcX = nx * cos - ny * sin + cx;
                float srcY = nx * sin + ny * cos + cy;

                if (srcX >= -0.5f && srcX < w - 0.5f && srcY >= -0.5f && srcY < h - 0.5f)
                {
                    // KLUCZ: Zamiast GetPixelBilinear, zaokrąglamy twardo do liczby całkowitej (Nearest Neighbor)
                    int px = Mathf.RoundToInt(srcX);
                    int py = Mathf.RoundToInt(srcY);

                    Color color = original.GetPixel(px, py);
                    rotated.SetPixel(x, y, color);
                }
                else
                {
                    rotated.SetPixel(x, y, Color.clear);
                }
            }
        }
        rotated.Apply();
        return rotated;
    }
}
#endif