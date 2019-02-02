using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GridWorld : MonoBehaviour {
    [SerializeField] Renderer planeRenderer = null;
    [SerializeField] Texture2D tex = null;
    [SerializeField] Vector2 cursor;
    Vector2Int cursorInt => new Vector2Int((int)cursor.x, (int)cursor.y);
    static readonly Color32 RED = new Color32(255, 0, 0, 255);
    static readonly Color32 BLUE = new Color32(0, 0, 255, 255);
    static readonly Color32 WHITE_TIK = new Color32(255, 255, 255, 0);
    static readonly Color32 WHITE_TOK = new Color32(255, 255, 255, 255);
    static readonly Vector2Int[] crossPattern = {
        new Vector2Int(0, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(+1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(0, +1),
    };

    static readonly int texSize = 256;
    // inclusive
    static readonly Vector2Int minCursorInt = new Vector2Int(1, 1);
    // exclusive
    static readonly Vector2Int maxCursorInt = new Vector2Int(texSize - 1, texSize - 1);
    // inclusive
    static readonly Vector2 minCursor = new Vector2(minCursorInt.x, minCursorInt.y);
    // exclusive
    static readonly Vector2 maxCursor = new Vector2(maxCursorInt.x, maxCursorInt.y);

    void Start() {
        tex = new Texture2D(texSize, texSize, TextureFormat.RGB24, false);
        tex.SetPixels32(Enumerable.Repeat(WHITE_TOK, tex.width * tex.height).ToArray());
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.alphaIsTransparency = false;
        planeRenderer.material.mainTexture = tex;
        cursor = Vector2.one * texSize / 2;
    }

    bool tikTok = false;

    void Update() {
        
        var bitmap = tex.GetPixels32(0);

        //Debug.Log(bitmap[0]);

        var oldWhite = tikTok ? WHITE_TIK : WHITE_TOK;
        var newWhite = tikTok ? WHITE_TOK : WHITE_TIK;
        FloodFill(bitmap, Vector2Int.zero, oldWhite, newWhite);

        for (int i = 0; i < bitmap.Length; i++) {
            if (ColorMatch(bitmap[i], oldWhite)) {
                bitmap[i] = RED;
            }
        }

        tikTok = !tikTok;


        tex.SetPixels32(bitmap);
        SetPixelsByPattern(cursorInt, crossPattern, BLUE);
        tex.Apply();

        cursor = new Vector2(Mathf.Clamp(cursor.x - Input.GetAxis("Horizontal"), minCursor.x, maxCursor.x - 1), Mathf.Clamp(cursor.y - Input.GetAxis("Vertical"), minCursor.y, maxCursor.y - 1));
    }

    void SetPixelsByPattern(Vector2Int cursorInt, Vector2Int[] pattern, Color32 color) {
        foreach (var v in pattern) {
            var cv = cursorInt + v;
            if (cv.x >= minCursorInt.x && cv.x < maxCursorInt.x && cv.y >= minCursorInt.y && cv.y < maxCursorInt.y) {
                tex.SetPixels32(cv.x, cv.y, 1, 1, new Color32[] { color });
            }
        }
    }

    private static bool ColorMatch(Color32 a, Color32 b) {
        return a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
    }

    Color32 GetPixel(Color32[] bitmap, int x, int y) {
        return bitmap[x + y * texSize];
    }

    void SetPixel(Color32[] bitmap, int x, int y, Color c) {
        bitmap[x + y * texSize] = c;
    }
    
    void FloodFill(Color32[] bitmap, Vector2Int pt, Color32 targetColor, Color32 replacementColor) {
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        q.Enqueue(pt);
        while (q.Count > 0) {
            var n = q.Dequeue();
            if (!ColorMatch(GetPixel(bitmap, n.x, n.y), targetColor))
                continue;
            Vector2Int w = n, e = new Vector2Int(n.x + 1, n.y);
            while ((w.x >= 0) && ColorMatch(GetPixel(bitmap, w.x, w.y), targetColor)) {
                SetPixel(bitmap, w.x, w.y, replacementColor);
                if ((w.y > 0) && ColorMatch(GetPixel(bitmap, w.x, w.y - 1), targetColor))
                    q.Enqueue(new Vector2Int(w.x, w.y - 1));
                if ((w.y < texSize - 1) && ColorMatch(GetPixel(bitmap, w.x, w.y + 1), targetColor))
                    q.Enqueue(new Vector2Int(w.x, w.y + 1));
                w.x--;
            }
            while ((e.x <= texSize - 1) && ColorMatch(GetPixel(bitmap, e.x, e.y), targetColor)) {
                SetPixel(bitmap, e.x, e.y, replacementColor);
                if ((e.y > 0) && ColorMatch(GetPixel(bitmap, e.x, e.y - 1), targetColor))
                    q.Enqueue(new Vector2Int(e.x, e.y - 1));
                if ((e.y < texSize - 1) && ColorMatch(GetPixel(bitmap, e.x, e.y + 1), targetColor))
                    q.Enqueue(new Vector2Int(e.x, e.y + 1));
                e.x++;
            }
        }
    }
}
