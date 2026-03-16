using EzySlice;
using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateRobotPose : MonoBehaviour
{
    public bool rivz_connect = false;
    private GameObject lbr_link_1;
    private GameObject lbr_link_2;
    private GameObject lbr_link_3;
    private GameObject lbr_link_4;
    private GameObject lbr_link_5;
    private GameObject lbr_link_6;
    private GameObject lbr_link_7;

    private GameObject teach_tool;
    private GameObject ee_tool;

    private JointStateSubscriberLBR jointStateSubscriber;
    private TriggerSignal TriggerSubscriber;
    private IKClient kClient;
    private CastingSlicing slicer;
    
    private void Start()
    {
        lbr_link_1 = GameObject.Find("lbr_link_1");
        lbr_link_2 = GameObject.Find("lbr_link_2");
        lbr_link_3 = GameObject.Find("lbr_link_3");
        lbr_link_4 = GameObject.Find("lbr_link_4");
        lbr_link_5 = GameObject.Find("lbr_link_5");
        lbr_link_6 = GameObject.Find("lbr_link_6");
        lbr_link_7 = GameObject.Find("lbr_link_7");
        jointStateSubscriber = FindObjectOfType<JointStateSubscriberLBR>();
        TriggerSubscriber = FindObjectOfType<TriggerSignal>();
        kClient = FindObjectOfType<IKClient>();
        slicer = FindObjectOfType<CastingSlicing>();

        teach_tool = GameObject.Find("Tool");
        ee_tool = GameObject.Find("EE");
    }
    void Update()
    {

        if (jointStateSubscriber.JointAngles.Count == 0)
        {
            Debug.LogWarning("Waiting for JointState messages...");
            return;
        }

        if ((TriggerSubscriber.RosStream || rivz_connect) && !TriggerSubscriber.recordTrigger)
        {
            rivz_connect = true;
            slicer.physics = false;
            // lbr_joint_1: z (ROS) -> y (Unity)
            lbr_link_1.transform.localEulerAngles = new Vector3(0, -jointStateSubscriber.GetJointAngle("lbr_joint_1"), 0);
            // lbr_joint_2: y (ROS) -> -x (Unity)
            lbr_link_2.transform.localEulerAngles = new Vector3(jointStateSubscriber.GetJointAngle("lbr_joint_2"), 0, 0);
            // lbr_joint_3: z (ROS) -> y (Unity)
            lbr_link_3.transform.localEulerAngles = new Vector3(0, -jointStateSubscriber.GetJointAngle("lbr_joint_3"), 0);
            // lbr_joint_4: -y (ROS) -> x (Unity)
            lbr_link_4.transform.localEulerAngles = new Vector3(-jointStateSubscriber.GetJointAngle("lbr_joint_4"), 0, 0);
            // lbr_joint_5: z (ROS) -> y (Unity)
            lbr_link_5.transform.localEulerAngles = new Vector3(0, -jointStateSubscriber.GetJointAngle("lbr_joint_5"), 0);
            // lbr_joint_6: y (ROS) -> -x (Unity)
            lbr_link_6.transform.localEulerAngles = new Vector3(jointStateSubscriber.GetJointAngle("lbr_joint_6"), 0, 0);
            // lbr_joint_7: z (ROS) -> y (Unity)
            lbr_link_7.transform.localEulerAngles = new Vector3(0, -jointStateSubscriber.GetJointAngle("lbr_joint_7"), 0);

            teach_tool.transform.position = ee_tool.transform.position;
            teach_tool.transform.rotation = ee_tool.transform.rotation;
        }
        else
        {
            rivz_connect = false;
            lbr_link_1.transform.localEulerAngles = new Vector3(0, -(kClient.ik_j1), 0);
            lbr_link_2.transform.localEulerAngles = new Vector3((kClient.ik_j2), 0, 0);
            lbr_link_3.transform.localEulerAngles = new Vector3(0, -(kClient.ik_j3), 0);
            lbr_link_4.transform.localEulerAngles = new Vector3(-(kClient.ik_j4), 0, 0);
            lbr_link_5.transform.localEulerAngles = new Vector3(0, -(kClient.ik_j5), 0);
            lbr_link_6.transform.localEulerAngles = new Vector3((kClient.ik_j6), 0, 0);
            lbr_link_7.transform.localEulerAngles = new Vector3(0, -(kClient.ik_j7), 0);
        }
    }
}
