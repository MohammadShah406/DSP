using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIscripts
{
    public class UIGradient : BaseMeshEffect
    {
        private enum GradientDirection
        {
            Vertical,
            Horizontal,
            Angle
        }
        
        public Gradient gradient;
        [SerializeField]
        private GradientDirection direction = GradientDirection.Vertical;
        [SerializeField]
        [Range(-180f, 180f)] private float angle;

        protected override void Awake()
        {
            base.Awake();
            // Initialize gradient if it's null
            if (gradient == null)
            {
                gradient = new Gradient();
                // Set default colors
                GradientColorKey[] colorKeys = new GradientColorKey[2];
                colorKeys[0] = new GradientColorKey(Color.white, 0f);
                colorKeys[1] = new GradientColorKey(Color.black, 1f);

                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
                alphaKeys[0] = new GradientAlphaKey(1f, 0f);
                alphaKeys[1] = new GradientAlphaKey(1f, 1f);

                gradient.SetKeys(colorKeys, alphaKeys);
            }
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || vh == null || gradient == null)
                return;

            List<UIVertex> vertexList = new List<UIVertex>();
            vh.GetUIVertexStream(vertexList);

            int count = vertexList.Count;
            if (count == 0)
                return;

            float minX = vertexList[0].position.x;
            float maxX = vertexList[0].position.x;
            float minY = vertexList[0].position.y;
            float maxY = vertexList[0].position.y;

            for (int i = 1; i < count; i++)
            {
                Vector3 pos = vertexList[i].position;
                if (pos.x < minX) minX = pos.x;
                if (pos.x > maxX) maxX = pos.x;
                if (pos.y < minY) minY = pos.y;
                if (pos.y > maxY) maxY = pos.y;
            }

            float width = maxX - minX;
            float height = maxY - minY; ;

            for (int i = 0; i < count; i++)
            {
                UIVertex vertex = vertexList[i];
                float t = 0f;

                switch (direction)
                {
                    case GradientDirection.Vertical:
                        if (height != 0)
                            t = (vertex.position.y - minY) / height;
                        break;

                    case GradientDirection.Horizontal:
                        if (width != 0)
                            t = (vertex.position.x - minX) / width;
                        break;

                    case GradientDirection.Angle:
                        if (width != 0 && height != 0)
                        {
                            float nx = (vertex.position.x - minX) / width;
                            float ny = (vertex.position.y - minY) / height;

                            float rad = angle * Mathf.Deg2Rad;
                            t = Mathf.Cos(rad) * nx + Mathf.Sin(rad) * ny;
                            t = Mathf.Clamp01(t);
                        }
                        break;
                }

                vertex.color *= gradient.Evaluate(t);
                vertexList[i] = vertex;
            }

            vh.Clear();
            vh.AddUIVertexTriangleStream(vertexList);
        }
    }
}