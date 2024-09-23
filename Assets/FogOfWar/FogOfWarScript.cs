using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class FogOfWarScript : MonoBehaviour
{
    public static FogOfWarScript instance;
    //w public List<GameObject> m_fogOfWarPlanes;
    public GameObject m_fogOfWarPlane;
    public Transform m_player => PlayerController.Instance.transform;
    public LayerMask m_fogLayer;
    public float m_radius = 5f;
    [SerializeField] private float alphaSize;
    private float m_radiusSqr { get { return m_radius * m_radius; } }
    private float m_radiusSqrAlphaArea { get { return ((m_radius + alphaSize) * (m_radius + alphaSize)); } }
    [SerializeField] private List<Transform> visible_Game_Objects = new List<Transform>();

    [SerializeField] private List<GameObject> planes = new();
    [SerializeField] private List<GameObject> players = new();

    [SerializeField]
    private Mesh m_mesh;
    private Vector3[] m_vertices;
    private Color[] m_colors;

    // Use this for initialization

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        Application.targetFrameRate = 60;

    }
    public void OnStart()
    {
        m_fogOfWarPlane = planes[CaveGameManager.Instance.activeCaveIndex];
        Initialize();
    }

    float avgFrameRate;
    [SerializeField] private TextMeshProUGUI display_Text;
    // Update is called once per frame
    void Update()
    {

        if (m_fogOfWarPlane != null)
        {
            Ray r = new Ray(transform.position, m_player.position - transform.position);
            RaycastHit hit;
            if (Physics.Raycast(r, out hit, 1000, m_fogLayer, QueryTriggerInteraction.Collide))
            {
                for (int i = 0; i < m_vertices.Length; i++)
                {
                    Vector3 v = m_fogOfWarPlane.transform.TransformPoint(m_vertices[i]);
                    v.y = 0;
                    float dist = Vector3.SqrMagnitude(v - new Vector3(hit.point.x, 0, hit.point.z));

                    if (dist < (m_radiusSqrAlphaArea))
                    {
                        if (dist < m_radiusSqr)
                        {
                            m_colors[i].a = 0;
                        }
                        else
                        {
                            //float alphaValue = (dist - m_radiusSqr) / (m_radiusSqrAlphaArea - m_radiusSqr);
                            float alpha = Mathf.Min(m_colors[i].a, ((dist - m_radiusSqr) / (m_radiusSqrAlphaArea - m_radiusSqr)));
                            m_colors[i].a = alpha;
                        }
                    }
                }
            }
            UpdateColor();

        }

    }
    //public List<Mesh> m_meshs = new();
    void Initialize()
    {

        m_mesh = m_fogOfWarPlane.GetComponent<MeshFilter>().mesh;
        m_vertices = m_mesh.vertices;
        m_colors = new Color[m_vertices.Length];
        for (int i = 0; i < m_colors.Length; i++)
        {
            m_colors[i] = Color.black;
        }
        UpdateColor();
    }

    void UpdateColor()
    {
        m_mesh.colors = m_colors;
    }
}
