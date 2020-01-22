using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace RectFrames
{

    public class RectFrameLayouter : MonoBehaviour
    {
        [SerializeField]
        float m_dpi = 72.0f;

        [SerializeField]
        int m_count = 5;

        [SerializeField]
        Camera m_ortho;

        List<RectFrame> m_frames = new List<RectFrame>();
        Dictionary<Transform, RectFrame> m_frameMap = new Dictionary<Transform, RectFrame>();

        Transform m_topLeft;

        (int, int) m_screenSize;

        [SerializeField]
        uWindowCapture.UwcWindowTextureManager m_manager;

        Dictionary<uWindowCapture.UwcWindowTexture, RectFrame> m_windowMap = new Dictionary<uWindowCapture.UwcWindowTexture, RectFrame>();

        RectFrame CreateFrameForWindow(uWindowCapture.UwcWindowTexture texture)
        {
            var go = new GameObject(texture.window.title);
            var frame = go.AddComponent<RectFrame>();
            frame.Collider = texture.GetComponent<Collider>();
            m_frames.Add(frame);
            frame.transform.SetParent(m_topLeft);
            // frame.Setup(GetColor(i));
            texture.transform.SetParent(frame.transform);
            // frame.SetPosition();
            // frame.RandomPosition(factor, i, m_ortho.pixelWidth, m_ortho.pixelHeight);

            m_frameMap.Add(frame.Collider.transform, frame);
            return frame;
        }

        void UpdateWindows()
        {
            // var pos = Vector3.zero;
            foreach (var kv in m_manager.windows)
            {
                var windowTexture = kv.Value;
                windowTexture.scaleControlType = uWindowCapture.WindowTextureScaleControlType.Manual;
                if (!m_windowMap.TryGetValue(windowTexture, out RectFrame frame))
                {
                    frame = CreateFrameForWindow(windowTexture);
                    frame.transform.localScale = new Vector3(m_factor, m_factor, 1.0f);
                    m_windowMap.Add(windowTexture, frame);
                }
                // frame.SetFrameSize(windowTexture.transform, windowTexture.window.rawWidth, windowTexture.window.rawHeight);
                var w = windowTexture.window.rawWidth;
                var h = windowTexture.window.rawHeight;
                var t = windowTexture.transform;
                // if (t.localScale.x != w || t.localScale.y != h)
                {
                    // Debug.Log($"window: {w} x {h}");
                    windowTexture.transform.localScale = new Vector3(w, h, 1);
                    windowTexture.transform.localPosition = new Vector3(w / 2, -h / 2, 0);
                }
            }
        }

        float m_factor = 1.0f;

        void SetScreenSize()
        {
            int width = m_ortho.pixelWidth;
            int height = m_ortho.pixelHeight;
            if (m_screenSize.Item1 == width && m_screenSize.Item2 == height)
            {
                return;
            }
            m_screenSize.Item1 = width;
            m_screenSize.Item2 = height;
            Debug.Log($"update screen size: {width} x {height}");

            m_factor = 1.0f / m_dpi;
            m_ortho.orthographicSize = height * m_factor / 2;

            m_topLeft.localPosition = new Vector3(-width / 2 * m_factor, height / 2 * m_factor);
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

        void CreateRandomFrame(float factor, string name, int i)
        {
            var go = new GameObject(name);
            var frame = go.AddComponent<RectFrame>();
            m_frames.Add(frame);
            frame.transform.SetParent(m_topLeft);
            frame.Setup(GetColor(i));
            frame.RandomPosition(factor, i, m_ortho.pixelWidth, m_ortho.pixelHeight);

            m_frameMap.Add(frame.Collider.transform, frame);
        }

        void Awake()
        {
            if (m_ortho is null)
            {
                m_ortho = Camera.main;
            }

            m_topLeft = new GameObject("origin").transform;
            m_topLeft.SetParent(m_ortho.transform);
        }

        void OnEnable()
        {
            SetScreenSize();

            Debug.Log("OnEnable");
            var factor = 1.0f / m_dpi;
            // for (int i = 0; i < m_count; ++i)
            // {
            //     CreateRandomFrame(factor, $"Frame:{i}", i + 1);
            // }
        }

        void OnDisable()
        {
            Debug.Log("OnDisable");
            foreach (var f in m_frames)
            {
                if (f)
                {
                    GameObject.Destroy(f.gameObject);
                }
            }
            m_frames.Clear();
        }

        Transform m_lastHit;
        RectFrame m_hover;

        void Hover(RaycastHit hit)
        {
            if (m_lastHit == hit.transform)
            {
                return;
            }
            m_lastHit = hit.transform;
            if (m_lastHit is null)
            {
                m_hover = null;
            }
            else if (m_frameMap.TryGetValue(hit.transform, out RectFrame frame))
            {
                m_hover = frame;
                Debug.Log($"{m_hover.name} => {hit.distance}");
            }
            else
            {
                Debug.Log($"{hit.transform.name} => {hit.distance}");
            }
        }

        void Reorder(RectFrame hover, int i)
        {
            m_frames.Remove(hover);
            m_frames.Insert(i, hover);
            int z = 1;
            foreach (var f in m_frames)
            {
                var p = f.transform.localPosition;
                p.z = z++;
                f.transform.localPosition = p;
            }
        }

        RectFrame m_mouseLeft;

        void LeftMouseDown()
        {
            if (m_hover is null)
            {
                m_mouseLeft = null;
                return;
            }

            Reorder(m_hover, 0);
            m_mouseLeft = m_hover;
        }

        void LeftMouseUp()
        {
            m_mouseLeft = null;
        }

        void LeftMouseDrag(Vector3 delta)
        {
            m_mouseLeft.transform.localPosition += delta;
        }

        void RightMouseDown()
        {
            if (m_hover is null)
            {
                return;
            }

            Reorder(m_hover, m_frames.Count - 1);
        }

        Vector3 m_mousePosition;

        void Update()
        {
            UpdateWindows();

            //
            SetScreenSize();

            var factor = 1.0f / m_dpi;
            var delta = Input.mousePosition - m_mousePosition;
            m_mousePosition = Input.mousePosition;

            // hover
            var ray = m_ortho.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow);

            Hover(Physics.RaycastAll(ray).OrderBy(x => x.distance).FirstOrDefault());

            // cursor

            // click
            if (Input.GetMouseButtonDown(0))
            {
                LeftMouseDown();
            }
            else
            {
                if (!(m_mouseLeft is null))
                {
                    LeftMouseDrag(delta * factor);
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                LeftMouseUp();
            }

            if (Input.GetMouseButtonDown(1))
            {
                RightMouseDown();
            }

            // drag

            // keyboard
        }
    }
}
