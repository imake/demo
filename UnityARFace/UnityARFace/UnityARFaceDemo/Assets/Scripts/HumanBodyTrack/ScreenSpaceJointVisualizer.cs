
using System.Collections.Generic;
using UnityEngine;

public class ScreenSpaceJointVisualizer : MonoBehaviour
{
    [SerializeField]
    [Tooltip("A prefab that contains a LineRenderer component that will be used for rendering lines, representing the skeleton joints.")]
    GameObject m_LineRendererPrefab;

    /// <summary>
    /// Get or set the Line Renderer prefab.
    /// </summary>
    public GameObject lineRendererPrefab
    {
        get { return m_LineRendererPrefab; }
        set { m_LineRendererPrefab = value; }
    }

    Dictionary<int, GameObject> m_LineRenderers;
    static HashSet<int> s_JointSet = new HashSet<int>();

    private List<HumanBodyPose2DJoint> joints = new List<HumanBodyPose2DJoint>();

    List<Vector2> positions = new List<Vector2>();

    private int jointCount = 15;

    private List<Vector2> jointList = new List<Vector2>();

    void Awake()
    {
        SelectJoint(null);
        InitHumanBodyPose2DJoint(jointList);
        m_LineRenderers = new Dictionary<int, GameObject>();

        SetJointsInfo();
    }

    void UpdateRenderer(List<HumanBodyPose2DJoint> joints, int index, string name)
    {
        GameObject lineRendererGO;
        if (!m_LineRenderers.TryGetValue(index, out lineRendererGO))
        {
            lineRendererGO = Instantiate(m_LineRendererPrefab, transform);
            m_LineRenderers.Add(index, lineRendererGO);
            lineRendererGO.name = name;
        }

        var lineRenderer = lineRendererGO.GetComponent<LineRenderer>();

        // Traverse hierarchy to determine the longest line set that needs to be drawn.
        CreatePositions();
        try
        {
            var boneIndex = index;
            int jointCount = 0;
            while (boneIndex >= 0)
            {
                var joint = joints[boneIndex];
                if (joint.tracked)
                {
                    positions[jointCount++] = joint.position;
                    if (!s_JointSet.Add(boneIndex))
                        break;
                }
                else
                    break;

                boneIndex = joint.parentIndex;
            }

            // Render the joints as lines on the camera's near clip plane.
            //lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = jointCount;
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            lineRenderer.startColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            lineRenderer.endColor = new Color(0.6f, 0.6f, 0.6f, 1f);
            for (int i = 0; i < jointCount; ++i)
            {
                var position = positions[i];
                var worldPosition = Camera.main.ViewportToWorldPoint(
                    new Vector3(position.x, position.y, Camera.main.nearClipPlane));
                lineRenderer.SetPosition(i, worldPosition);
            }
            lineRendererGO.SetActive(true);
        }
        finally
        {
            positions.Clear();
        }
    }

    void SetJointsInfo()
    {
        s_JointSet.Clear();
        for (int i = joints.Count - 1; i >= 0; i--)
        {
            if (joints[i].parentIndex != -1)
                UpdateRenderer(joints, i, joints[i].name);
        }
    }

    void HideJointLines()
    {
        foreach (var lineRenderer in m_LineRenderers)
        {
            lineRenderer.Value.SetActive(false);
        }
    }

    // 2D joint skeleton
    //enum JointIndices
    //{
    //    Invalid = -1,
    //    Head = 0, // parent: Neck1 [1]
    //    Neck1 = 1, // parent: Root [16]
    //    RightShoulder1 = 2, // parent: Neck1 [1]
    //    RightForearm = 3, // parent: RightShoulder1 [2]
    //    RightHand = 4, // parent: RightForearm [3]
    //    LeftShoulder1 = 5, // parent: Neck1 [1]
    //    LeftForearm = 6, // parent: LeftShoulder1 [5]
    //    LeftHand = 7, // parent: LeftForearm [6]
    //    RightUpLeg = 8, // parent: Root [16]
    //    RightLeg = 9, // parent: RightUpLeg [8]
    //    RightFoot = 10, // parent: RightLeg [9]
    //    LeftUpLeg = 11, // parent: Root [16]
    //    LeftLeg = 12, // parent: LeftUpLeg [11]
    //    LeftFoot = 13, // parent: LeftLeg [12]
    //    RightEye = 14, // parent: Head [0]
    //    LeftEye = 15, // parent: Head [0]
    //    Root = 16, // parent: <none> [-1]
    //}
    enum JointIndices
    {
        Invalid = -1,
        Head = 0, // parent: Invalid [-1]
        RightShoulder1 = 1, // parent: Invalid [14]
        RightForearm = 2, // parent: RightShoulder1 [1]
        RightHand = 3, // parent: RightForearm [2]
        LeftShoulder1 = 4, // parent: RightShoulder1 [1]
        LeftForearm = 5, // parent: LeftShoulder1 [4]
        LeftHand = 6, // parent: LeftForearm [5]
        RightLeg = 7, // parent: Root2 [14]
        RightFoot = 8, // parent: RightLeg [7]
        LeftLeg = 9, // parent: Root1 [13]
        LeftFoot = 10, // parent: LeftLeg [9]
        RightEye = 11, // parent: Head [0]
        LeftEye = 12, // parent: Head [0]
        Root1 = 13, // parent: <Root2> [4]
        Root2 = 14, // parent: <none> [13]
    }

    public void SelectJoint(List<Vector2> posList)
    {
        int rate = 20;
        float height = 640/rate;
        float value = 20;
        jointList.Clear();
        Vector2 pos = new Vector2(height - 261.14355f / rate - value, height - 114.13994f / rate - value);//Heads->0
        jointList.Add(pos);
        pos = new Vector2(height - 230.31705f / rate - value, height - 139.27298f / rate - value);//RightShoulder1->12
        jointList.Add(pos);
        pos = new Vector2(height - 221.9626f / rate - value, height - 182.99167f / rate - value);//RightForearm->14
        jointList.Add(pos);
        pos = new Vector2(height - 229.92441f / rate - value, height - 214.94897f / rate - value);//RightHand->16
        jointList.Add(pos);
        pos = new Vector2(height - 280.89972f / rate - value, height - 142.83371f / rate - value);//LeftShoulder1->11
        jointList.Add(pos);
        pos = new Vector2(height - 284.1551f / rate - value, height - 182.40266f / rate - value);//LeftForearm->13
        jointList.Add(pos);
        pos = new Vector2(height - 289.23422f / rate - value, height - 214.90384f / rate - value);//LeftHand->15
        jointList.Add(pos);
        pos = new Vector2(height - 225.00595f / rate - value, height - 274.51468f / rate - value);//RightLeg->26
        jointList.Add(pos);
        pos = new Vector2(height - 210.31467f / rate - value, height - 330.83328f / rate - value);//RightFoot->28
        jointList.Add(pos);
        pos = new Vector2(height - 260.6173f / rate - value, height - 275.4993f / rate - value);//LeftLeg->25
        jointList.Add(pos);
        pos = new Vector2(height - 254.0951f / rate - value, height - 328.40942f / rate - value);//LeftFoot->27
        jointList.Add(pos);
        pos = new Vector2(height - 250.6193f / rate - value, height - 110.42336f / rate - value);//RightEye->8
        jointList.Add(pos);
        pos = new Vector2(height - 269.80917f / rate - value, height - 112.069595f / rate - value);//LeftEye->7
        jointList.Add(pos);
        pos = new Vector2(height - 269.75006f / rate - value, height - 206.8741f / rate - value);//Root1->23
        jointList.Add(pos);
        pos = new Vector2(height - 233.8795f / rate - value, height - 204.1214f / rate - value);//Root2->24
        jointList.Add(pos);
    }

    void InitHumanBodyPose2DJoint(List<Vector2> posList)
    {
        //Heads->0
        string name = "Heads";
        int index = 0;
        int parentIndex = -1;
        Vector2 pos = posList[index];
        HumanBodyPose2DJoint joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //RightShoulder1->12
        name = "RightShoulder1";
        index = 1;
        parentIndex = 14;
        pos = posList[index];
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //RightForearm->14
        name = "RightForearm";
        index = 2;
        parentIndex = 1;
        pos = posList[index];
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //RightHand->16
        name = "RightHand";
        index = 3;
        parentIndex = 2;
        pos = posList[index];
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //LeftShoulder1->11
        name = "LeftShoulder1";
        index = 4;
        parentIndex = 1;
        pos = posList[index];
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //LeftForearm->13
        name = "LeftForearm";
        index = 5;
        parentIndex = 4;
        pos = posList[index];
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //LeftHand->15
        name = "LeftHand";
        index = 6;
        parentIndex = 5;
        pos = posList[index];
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //RightLeg->26
        name = "RightLeg";
        index = 7;
        parentIndex = 14;
        pos = posList[index];
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //RightFoot->28
        name = "RightFoot";
        index = 8;
        parentIndex = 7;
        pos = posList[index];
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //LeftLeg->25
        name = "LeftLeg";
        index = 9;
        parentIndex = 13;
        pos = posList[index];
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //LeftFoot->27
        name = "LeftFoot";
        index = 10;
        parentIndex = 9;
        pos = posList[index];
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //RightEye->8
        name = "RightEye";
        index = 11;
        parentIndex = 0;
        pos = posList[index];
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //LeftEye->7
        name = "LeftEye";
        index = 12;
        parentIndex = 0;
        pos = posList[index];
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //Root1->23
        name = "Root1";
        index = 13;
        parentIndex = 4;
        pos = posList[index];
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //Root2->24
        name = "Root2";
        index = 14;
        parentIndex = 13;
        pos = posList[index];
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        /*
        //Heads
        string name = "Heads";
        int index = 0;
        int parentIndex = 1;
        Vector3 pos = new Vector3(0, 0);
        HumanBodyPose2DJoint joint = new HumanBodyPose2DJoint(name,index, parentIndex,pos,true);
        joints.Add(joint);

        //Neck1
        name = "Neck1";
        index = 1;
        parentIndex = 16;
        pos = new Vector3(0, -1f);
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //RightShoulder1
        name = "RightShoulder1";
        index = 2;
        parentIndex = 1;
        pos = new Vector3(-1f, -1f);
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //RightForearm
        name = "RightForearm";
        index = 3;
        parentIndex = 2;
        pos = new Vector3(-2f, -1f);
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //RightHand
        name = "RightHand";
        index = 4;
        parentIndex = 3;
        pos = new Vector3(-3f, -1f);
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //LeftShoulder1
        name = "LeftShoulder1";
        index = 5;
        parentIndex = 1;
        pos = new Vector3(1f, -1f);
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //LeftForearm
        name = "LeftForearm";
        index = 6;
        parentIndex = 5;
        pos = new Vector3(2f, -1f);
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //LeftHand
        name = "LeftHand";
        index = 7;
        parentIndex = 6;
        pos = new Vector3(3f, -1f);
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //RightUpLeg
        name = "RightUpLeg";
        index = 8;
        parentIndex = 16;
        pos = new Vector3(-1f, -5f);
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //RightLeg
        name = "RightLeg";
        index = 9;
        parentIndex =8;
        pos = new Vector3(-2f, -5f);
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //RightFoot
        name = "RightFoot";
        index = 10;
        parentIndex = 9;
        pos = new Vector3(-3f, -5f);
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //LeftUpLeg
        name = "LeftUpLeg";
        index = 11;
        parentIndex = 16;
        pos = new Vector3(1f, -5f);
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //LeftLeg
        name = "LeftLeg";
        index = 12;
        parentIndex = 11;
        pos = new Vector3(2f, -5f);
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //LeftFoot
        name = "LeftFoot";
        index = 13;
        parentIndex = 12;
        pos = new Vector3(3f, -5f);
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //RightEye
        name = "RightEye";
        index = 14;
        parentIndex = 0;
        pos = new Vector3(0.5f, 0);
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //LeftEye
        name = "LeftEye";
        index = 15;
        parentIndex = 0;
        pos = new Vector3(-0.5f, 0);
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //Root
        name = "Root";
        index = 16;
        parentIndex = -1;
        pos = new Vector3(0, -5f);
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);
        */
    }

    public void ChangeHumanBodyPose2DJoint()
    {
        joints.Clear();

        //Heads
        string name = "Heads";
        int index = 0;
        int parentIndex = 1;
        Vector3 pos = new Vector3(0, 0);
        HumanBodyPose2DJoint joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //Neck1
        name = "Neck1";
        index = 1;
        parentIndex = 16;
        pos = new Vector3(0, -1f);
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //RightShoulder1
        name = "RightShoulder1";
        index = 2;
        parentIndex = 1;
        pos = new Vector3(-1f, -1f);
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //RightForearm
        name = "RightForearm";
        index = 3;
        parentIndex = 2;
        pos = new Vector3(-2f, -1f);
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //RightHand
        name = "RightHand";
        index = 4;
        parentIndex = 3;
        pos = new Vector3(-3f, -1f);
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //LeftShoulder1
        name = "LeftShoulder1";
        index = 5;
        parentIndex = 1;
        pos = new Vector3(1f, -1f);
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //LeftForearm
        name = "LeftForearm";
        index = 6;
        parentIndex = 5;
        pos = new Vector3(2f, -1f);
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //LeftHand
        name = "LeftHand";
        index = 7;
        parentIndex = 6;
        pos = new Vector3(3f, -1f);
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //RightUpLeg
        name = "RightUpLeg";
        index = 8;
        parentIndex = 16;
        pos = new Vector3(-1f, -5f);
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //RightLeg
        name = "RightLeg";
        index = 9;
        parentIndex = 8;
        pos = new Vector3(-2f, -6f);
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //RightFoot
        name = "RightFoot";
        index = 10;
        parentIndex = 9;
        pos = new Vector3(-3f, -7f);
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //LeftUpLeg
        name = "LeftUpLeg";
        index = 11;
        parentIndex = 16;
        pos = new Vector3(1f, -5f);
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //LeftLeg
        name = "LeftLeg";
        index = 12;
        parentIndex = 11;
        pos = new Vector3(2f, -6f);
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //LeftFoot
        name = "LeftFoot";
        index = 13;
        parentIndex = 12;
        pos = new Vector3(3f, -7f);
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //RightEye
        name = "RightEye";
        index = 14;
        parentIndex = 0;
        pos = new Vector3(0.5f, 0);
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //LeftEye
        name = "LeftEye";
        index = 15;
        parentIndex = 0;
        pos = new Vector3(-0.5f, 0);
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //Root
        name = "Root";
        index = 16;
        parentIndex = -1;
        pos = new Vector3(0, -5f);
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        SetJointsInfo();
    }

    public void CreatePositions()
    {
        for (int i = 0; i < jointCount; i++)
        {
            positions.Add(Vector2.zero);
        }
    }
}
public class HumanBodyPose2DJoint
{
    public string name;
    public int index;
    public int parentIndex;
    public Vector2 position;
    public bool tracked;

    public HumanBodyPose2DJoint()
    {

    }

    public HumanBodyPose2DJoint(string _name, int _index, int _parentIndex, Vector2 _position, bool _tracked)
    {
        name = _name;
        index = _index;
        parentIndex = _parentIndex;
        position = _position;
        tracked = _tracked;
    }
}
