using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RectFrames
{
    public class RectFrame : MonoBehaviour
    {
        [SerializeField]
        Transform m_rect;

        public Collider Collider
        {
            get
            {
                return m_rect.GetComponent<Collider>();
            }
        }

        public void Setup(Color color)
        {
            m_rect = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
            m_rect.transform.SetParent(transform);
            var renderer = m_rect.GetComponent<Renderer>();
            var m = new Material(Shader.Find("Unlit/Color"));
            m.color = color;
            renderer.sharedMaterial = m;
        }

        void SetFramePosition(float factor, int x, int y, int z)
        {
            transform.localPosition = new Vector3(
                x * factor,
                y * factor,
                z);
            transform.localScale = new Vector3(factor, factor, 1.0f);
        }

        void SetFrameSize(float factor, Transform t, int w, int h)
        {
            t.localScale = new Vector3(w, h, 1);
            t.localPosition = new Vector3(w / 2, -h / 2, 0);
        }

        /// factor = 1.0f / DPI
        public void RandomPosition(float factor, int z, int screenWidth, int screenHeight)
        {
            int w = Random.Range(50, screenWidth);
            int h = Random.Range(50, screenHeight);
            SetFrameSize(factor, m_rect, w, h);

            int x = Random.Range(0, screenWidth - w);
            int y = Random.Range(0, screenHeight - h);
            Debug.Log($"SetPostion: [{x}/{screenWidth}, {y}/{screenHeight}, {z}] x {factor}");
            SetFramePosition(factor, x, -y, z);
        }

        public void SetZOrder(int z)
        {
            var p = transform.localPosition;
            p.z = z;
            transform.localPosition = p;
        }
    }
}
