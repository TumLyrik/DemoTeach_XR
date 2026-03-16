using RosSharp.RosBridgeClient.MessageTypes.Geometry;
using RosSharp.RosBridgeClient.MessageTypes.Moveit;
using RosSharp.RosBridgeClient.MessageTypes.Sensor;
using RosSharp.RosBridgeClient.MessageTypes.Std;

namespace RosSharp.RosBridgeClient.MessageTypes.MoveitMsgs
{
    [System.Serializable]
    public class GetPositionIKRequest : Message
    {
        public const string RosMessageName = "moveit_msgs/GetPositionIKRequest";

        public PositionIKRequest ik_request;

        public GetPositionIKRequest()
        {
            this.ik_request = new PositionIKRequest();
        }
    }

    [System.Serializable]
    public class GetPositionIKResponse : Message
    {
        public const string RosMessageName = "moveit_msgs/GetPositionIKResponse";

        public RobotState solution;
        public MoveItErrorCodes error_code;

        public GetPositionIKResponse()
        {
            this.solution = new RobotState();
            this.error_code = new MoveItErrorCodes();
        }
    }

    [System.Serializable]
    public class PositionIKRequest : Message
    {
        public const string RosMessageName = "moveit_msgs/PositionIKRequest";

        public string group_name;
        public RobotState robot_state;
        public Constraints constraints;
        public PoseStamped pose_stamped;
        public string[] ik_link_names;
        public Pose[] pose_stamped_vector;
        public bool avoid_collisions;
        public double timeout;

        public PositionIKRequest()
        {
            this.group_name = "";
            this.robot_state = new RobotState();
            this.constraints = new Constraints();
            this.pose_stamped = new PoseStamped();
            this.ik_link_names = new string[0];
            this.pose_stamped_vector = new Pose[0];
            this.avoid_collisions = true;
            this.timeout = 0.0;
        }
    }

    [System.Serializable]
    public class RobotState : Message
    {
        public const string RosMessageName = "moveit_msgs/RobotState";

        public JointState joint_state;
        public MultiDOFJointState multi_dof_joint_state;
        public AttachedCollisionObject[] attached_collision_objects;
        public bool is_diff;

        public RobotState()
        {
            this.joint_state = new JointState();
            this.multi_dof_joint_state = new MultiDOFJointState();
            this.attached_collision_objects = new AttachedCollisionObject[0];
            this.is_diff = false;
        }
    }

    [System.Serializable]
    public class MoveItErrorCodes : Message
    {
        public const string RosMessageName = "moveit_msgs/MoveItErrorCodes";

        public int val;

        // 错误代码常量
        public const int SUCCESS = 1;
        public const int FAILURE = 99999;
        public const int PLANNING_FAILED = -1;
        public const int INVALID_MOTION_PLAN = -2;
        public const int MOTION_PLAN_INVALIDATED_BY_ENVIRONMENT_CHANGE = -3;
        public const int CONTROL_FAILED = -4;
        public const int UNABLE_TO_AQUIRE_SENSOR_DATA = -5;
        public const int TIMED_OUT = -6;
        public const int PREEMPTED = -7;
        public const int START_STATE_IN_COLLISION = -10;
        public const int START_STATE_VIOLATES_PATH_CONSTRAINTS = -11;
        public const int GOAL_IN_COLLISION = -12;
        public const int GOAL_VIOLATES_PATH_CONSTRAINTS = -13;
        public const int GOAL_CONSTRAINTS_VIOLATED = -14;
        public const int INVALID_GROUP_NAME = -15;
        public const int INVALID_GOAL_CONSTRAINTS = -16;
        public const int INVALID_ROBOT_STATE = -17;
        public const int INVALID_LINK_NAME = -18;
        public const int INVALID_OBJECT_NAME = -19;
        public const int FRAME_TRANSFORM_FAILURE = -21;
        public const int COLLISION_CHECKING_UNAVAILABLE = -22;
        public const int ROBOT_STATE_STALE = -23;
        public const int SENSOR_INFO_STALE = -24;
        public const int NO_IK_SOLUTION = -31;

        public MoveItErrorCodes()
        {
            this.val = SUCCESS;
        }
    }

    // 其他需要的辅助消息类型
    [System.Serializable]
    public class JointState : Message
    {
        public const string RosMessageName = "sensor_msgs/JointState";

        public Header header;
        public string[] name;
        public double[] position;
        public double[] velocity;
        public double[] effort;

        public JointState()
        {
            this.header = new Header();
            this.name = new string[0];
            this.position = new double[0];
            this.velocity = new double[0];
            this.effort = new double[0];
        }
    }

    [System.Serializable]
    public class PoseStamped : Message
    {
        public const string RosMessageName = "geometry_msgs/PoseStamped";

        public Header header;
        public Pose pose;

        public PoseStamped()
        {
            this.header = new Header();
            this.pose = new Pose();
        }
    }

    [System.Serializable]
    public class Constraints : Message
    {
        public const string RosMessageName = "moveit_msgs/Constraints";

        public string name;
        public JointConstraint[] joint_constraints;
        public PositionConstraint[] position_constraints;
        public OrientationConstraint[] orientation_constraints;
        public VisibilityConstraint[] visibility_constraints;

        public Constraints()
        {
            this.name = "";
            this.joint_constraints = new JointConstraint[0];
            this.position_constraints = new PositionConstraint[0];
            this.orientation_constraints = new OrientationConstraint[0];
            this.visibility_constraints = new VisibilityConstraint[0];
        }
    }

    // 可以根据需要继续添加其他必要的消息类型...
}