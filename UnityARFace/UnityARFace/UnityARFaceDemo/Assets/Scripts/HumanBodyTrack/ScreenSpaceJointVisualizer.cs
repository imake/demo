
using System.Collections.Generic;
using UnityEngine;

public class ScreenSpaceJointVisualizer : MonoBehaviour
{
    private static ScreenSpaceJointVisualizer m_instance;

    public static ScreenSpaceJointVisualizer Instance
    {
        get
        {
            return m_instance;
        }
    }

    public GameObject m_LineRendererPrefab;

    public GameObject m_sphere;

    Dictionary<int, GameObject> m_LineRenderers;
    static HashSet<int> s_JointSet = new HashSet<int>();

    private List<HumanBodyPose2DJoint> joints = new List<HumanBodyPose2DJoint>();

    List<Vector2> positions = new List<Vector2>();

    private int jointCount = 15;

    private List<Vector2> jointList = new List<Vector2>();

    private List<GameObject> jointSphereList;

    private List<LineRenderer> lineRendererList;

    void Awake()
    {
        m_instance = this;

        m_LineRenderers = new Dictionary<int, GameObject>();
        jointSphereList = new List<GameObject>();
        lineRendererList = new List<LineRenderer>();
    }

    void UpdateRenderer(List<HumanBodyPose2DJoint> joints, int index, string name)
    {
        //HideJointLines();

        GameObject lineRendererGO;
        if (!m_LineRenderers.TryGetValue(index, out lineRendererGO))
        {
            lineRendererGO = Instantiate(m_LineRendererPrefab, transform);
            m_LineRenderers.Add(index, lineRendererGO);
            lineRendererGO.name = name;
        }
        //lineRendererGO.SetActive(true);
        var lineRenderer = lineRendererGO.GetComponent<LineRenderer>();
        lineRendererList.Add(lineRenderer);

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

        for (int j = 0; j < jointSphereList.Count; j++)
        {
            GameObject.Destroy(jointSphereList[j]);
        }
        jointSphereList.Clear();

        for (int m = 0; m < lineRendererList.Count; m++)
        {
            lineRendererList[m].positionCount = 0;
        }
        lineRendererList.Clear();

        for (int i = joints.Count - 1; i >= 0; i--)
        {
            if (joints[i].parentIndex != -1)
            {
                UpdateRenderer(joints, i, joints[i].name);
                GameObject go = Instantiate(m_sphere);
                Vector3 pos = joints[i].position;
                var worldPos = Camera.main.ViewportToWorldPoint(
            new Vector3(pos.x, pos.y, Camera.main.nearClipPlane));
                go.transform.SetParent(transform);
                go.transform.localPosition = worldPos;
                jointSphereList.Add(go);
            }
        }
    }

    void HideJointLines()
    {
        foreach (var lineRenderer in m_LineRenderers)
        {
            lineRenderer.Value.SetActive(false);
        }
    }

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
        int heightRate = 20;
        int widthRate = 15;
        float height = 640;
        float width = 480;
        float heightValue = 21;
        float widthValue = 17;

        jointList.Clear();
        Vector2 pos = new Vector2(-((width - posList[0].x)/ widthRate - widthValue), (height - posList[0].y)/ heightRate - heightValue);//Heads->0
        jointList.Add(pos);
        pos = new Vector2(-((width - posList[12].x) / widthRate - widthValue), (height - posList[12].y) / heightRate - heightValue);//RightShoulder1->12
        jointList.Add(pos);
        pos = new Vector2(-((width - posList[14].x) / widthRate - widthValue), (height - posList[14].y) / heightRate - heightValue);//RightForearm->14
        jointList.Add(pos);
        pos = new Vector2(-((width - posList[16].x) / widthRate - widthValue), (height - posList[16].y) / heightRate - heightValue);//RightHand->16
        jointList.Add(pos);
        pos = new Vector2(-((width - posList[11].x) / widthRate - widthValue), (height - posList[11].y) / heightRate - heightValue);//LeftShoulder1->11
        jointList.Add(pos);
        pos = new Vector2(-((width - posList[13].x) / widthRate - widthValue), (height - posList[13].y) / heightRate - heightValue);//LeftForearm->13
        jointList.Add(pos);
        pos = new Vector2(-((width - posList[15].x) / widthRate - widthValue), (height - posList[15].y) / heightRate - heightValue);//LeftHand->15
        jointList.Add(pos);
        pos = new Vector2(-((width - posList[8].x) / widthRate - widthValue), (height - posList[8].y) / heightRate - heightValue);//RightEye->8
        jointList.Add(pos);
        pos = new Vector2(-((width - posList[7].x) / widthRate - widthValue), (height - posList[7].y) / heightRate - heightValue);//LeftEye->7
        jointList.Add(pos);
        pos = new Vector2(-((width - posList[23].x) / widthRate - widthValue), (height - posList[23].y) / heightRate - heightValue);//Root1->23
        jointList.Add(pos);
        pos = new Vector2(-((width - posList[24].x) / widthRate - widthValue), (height - posList[24].y) / heightRate - heightValue);//Root2->24
        jointList.Add(pos);

        //全身
        if (posList.Count>=33)
        {
            pos = new Vector2(-((width - posList[26].x) / widthRate - widthValue), (height - posList[26].y) / heightRate - heightValue);//RightLeg->26
            jointList.Add(pos);
            pos = new Vector2(-((width - posList[28].x) / widthRate - widthValue), (height - posList[28].y) / heightRate - heightValue);//RightFoot->28
            jointList.Add(pos);
            pos = new Vector2(-((width - posList[25].x) / widthRate - widthValue), (height - posList[25].y) / heightRate - heightValue);//LeftLeg->25
            jointList.Add(pos);
            pos = new Vector2(-((width - posList[27].x) / widthRate - widthValue), (height - posList[27].y) / heightRate - heightValue);//LeftFoot->27
            jointList.Add(pos);
        }

        InitHumanBodyPose2DJoint(jointList);
    }

    void InitHumanBodyPose2DJoint(List<Vector2> posList)
    {
        joints.Clear();
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
        parentIndex = 10;
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

        //RightEye->8
        name = "RightEye";
        index = 7;
        parentIndex = 0;
        pos = posList[index];
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //LeftEye->7
        name = "LeftEye";
        index = 8;
        parentIndex = 0;
        pos = posList[index];
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //Root1->23
        name = "Root1";
        index = 9;
        parentIndex = 4;
        pos = posList[index];
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //Root2->24
        name = "Root2";
        index = 10;
        parentIndex = 9;
        pos = posList[index];
        joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
        joints.Add(joint);

        //全身
        if (posList.Count>=15)
        {
            //RightLeg->26
            name = "RightLeg";
            index = 11;
            parentIndex = 10;
            pos = posList[index];
            joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
            joints.Add(joint);

            //RightFoot->28
            name = "RightFoot";
            index = 12;
            parentIndex = 11;
            pos = posList[index];
            joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
            joints.Add(joint);

            //LeftLeg->25
            name = "LeftLeg";
            index = 13;
            parentIndex = 9;
            pos = posList[index];
            joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
            joints.Add(joint);

            //LeftFoot->27
            name = "LeftFoot";
            index = 14;
            parentIndex = 13;
            pos = posList[index];
            joint = new HumanBodyPose2DJoint(name, index, parentIndex, pos, true);
            joints.Add(joint);
        }

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
