using UnityEngine;
using UnityEngine.UI;

namespace UIGlobal
{
    [AddComponentMenu("UI/Rect Outline")]
    public class UIRectOutline : Graphic
    {
        [Tooltip("Толщина обводки в локальных единицах (пикселях).")]
        [SerializeField] private float thickness = 6f;

        [Tooltip("Отступ обводки наружу от rect (положительный — выходит наружу).")]
        [SerializeField] private float padding = 0f;

        public float Thickness
        {
            get => thickness;
            set { thickness = Mathf.Max(0f, value); SetVerticesDirty(); }
        }

        public float Padding
        {
            get => padding;
            set { padding = value; SetVerticesDirty(); }
        }

    #if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            thickness = Mathf.Max(0f, thickness);
            SetVerticesDirty();
        }
    #endif

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            Rect r = rectTransform.rect;

            float pad = padding;
            float t = thickness;

            // Outer rect (expanded by padding)
            float oL = r.xMin - pad;
            float oR = r.xMax + pad;
            float oT = r.yMax + pad;
            float oB = r.yMin - pad;

            // Ограничиваем толщину, чтобы inner не перевернулся
            float maxHalfWidth = (oR - oL) * 0.5f;
            float maxHalfHeight = (oT - oB) * 0.5f;
            float maxAllowedT = Mathf.Min(maxHalfWidth, maxHalfHeight);
            if (t > maxAllowedT) t = maxAllowedT;

            // Inner rect (inset from outer by thickness)
            float iL = oL + t;
            float iR = oR - t;
            float iT = oT - t;
            float iB = oB + t;

            // Цвет вершин
            UIVertex vert = UIVertex.simpleVert;
            vert.color = color;

            // Вспомог: добавить квад (четыре вершины), и два треугольника
            void AddQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
            {
                int start = vh.currentVertCount;

                vert.position = a; vh.AddVert(vert);
                vert.position = b; vh.AddVert(vert);
                vert.position = c; vh.AddVert(vert);
                vert.position = d; vh.AddVert(vert);

                vh.AddTriangle(start + 0, start + 1, start + 2);
                vh.AddTriangle(start + 2, start + 3, start + 0);
            }

            // Top quad: outer TL, outer TR, inner TR, inner TL
            AddQuad(new Vector2(oL, oT), new Vector2(oR, oT), new Vector2(iR, iT), new Vector2(iL, iT));

            // Bottom quad: outer BL, inner BL, inner BR, outer BR
            AddQuad(new Vector2(oL, oB), new Vector2(iL, iB), new Vector2(iR, iB), new Vector2(oR, oB));

            // Left quad: outer TL, inner TL, inner BL, outer BL
            AddQuad(new Vector2(oL, oT), new Vector2(iL, iT), new Vector2(iL, iB), new Vector2(oL, oB));

            // Right quad: inner TR, outer TR, outer BR, inner BR
            AddQuad(new Vector2(iR, iT), new Vector2(oR, oT), new Vector2(oR, oB), new Vector2(iR, iB));
        }
    }
}