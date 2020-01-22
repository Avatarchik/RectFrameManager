using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RectFrames
{

    public class RectFrameServer : MonoBehaviour
    {
        [SerializeField]
        float m_dpi = 72.0f;

        [SerializeField]
        int m_count = 5;

        [SerializeField]
        Camera m_ortho;

        List<RectFrame> m_frames = new List<RectFrame>();

        (int, int) m_screenSize;

        void SetScreenSize(int width, int height)
        {
            if (m_screenSize.Item1 == width && m_screenSize.Item2 == height)
            {
                return;
            }
            m_screenSize.Item1 = width;
            m_screenSize.Item2 = height;

            var factor = 1.0f / m_dpi;
            m_ortho.orthographicSize = Screen.height * factor * 0.5f; ;

            foreach (var frame in m_frames)
            {
                // frame.transform.localScale = new Vector3(factor, factor, 1.0f);
            }
        }

        List<Color> m_colors = new List<Color>
        {
            Color.white,
            Color.red,
            Color.green,
            Color.blue,
            Color.cyan,
            Color.magenta,
            Color.yellow,
        };

        Color GetColor(int i)
        {
            return m_colors[i % m_colors.Count];
        }

        void CreateRandomFrame(string name, int i)
        {
            var go = new GameObject(name);

            var rect = GameObject.CreatePrimitive(PrimitiveType.Quad);
            rect.transform.SetParent(go.transform);

            var frame = go.AddComponent<RectFrame>();
            LayoutFrame(frame, rect.transform, i);
            m_frames.Add(frame);
            frame.transform.SetParent(m_ortho.transform);

            var renderer = rect.GetComponent<Renderer>();
            var m = new Material(Shader.Find("Unlit/Color"));
            m.color = GetColor(i);
            renderer.sharedMaterial = m;
        }

        void SetFramePosition(Transform t, int x, int y, int z)
        {
            var factor = 1.0f / m_dpi;
            t.localPosition = new Vector3(
                (x - Screen.width / 2) * factor,
                (y + Screen.height / 2) * factor,
                z);
            t.localScale = new Vector3(factor, factor, 1.0f);
        }

        void SetFrameSize(Transform t, int w, int h)
        {
            var factor = 1.0f / m_dpi;
            t.localScale = new Vector3(w, h, 1);
            t.localPosition = new Vector3(w / 2, -h / 2, 0);
        }

        void LayoutFrame(RectFrame frame, Transform rect, int z)
        {
            int x = Random.Range(0, Screen.width);
            int y = Random.Range(0, Screen.height);
            SetFramePosition(frame.transform, x, y, z);

            int w = Random.Range(300, 1000);
            int h = Random.Range(300, 1000);
            SetFrameSize(rect, w, h);
        }

        void Awake()
        {
            if (m_ortho is null)
            {
                m_ortho = Camera.main;
            }
        }

        void Start()
        {
            for (int i = 0; i < m_count; ++i)
            {
                CreateRandomFrame($"Frame:{i}", i + 1);
            }
        }

        void Update()
        {
            SetScreenSize(Screen.width, Screen.height);

            // hover

            // cursor

            // click

            // drag

            // keyboard
        }
    }
}
