using UnityEngine;

namespace Animas.UI
{
    // Puts a Humanoid model (the Padre in the menu) in a natural standing pose without an animation clip:
    // arms down at his sides, and a slow breath in the chest. It writes Unity's "muscle" values (the
    // Humanoid system describes any pose as numbers per muscle) through a HumanPoseHandler every frame.
    [RequireComponent(typeof(Animator))]
    public class HumanStandPose : MonoBehaviour
    {
        [Tooltip("-1 = arm fully down along the body, 0 = the model's rest pose (arms out).")]
        [Range(-1f, 0f)] [SerializeField] float armsDown = -0.62f;
        [Range(0f, 1f)] [SerializeField] float headDown = 0.15f;
        [SerializeField] float breathSeconds = 4f;

        HumanPoseHandler handler;
        HumanPose pose;
        Transform hips;
        Vector3 hipsPosition;
        Quaternion hipsRotation;
        int leftArm, rightArm, leftForearm, rightForearm, chest, head;

        void Start()
        {
            var animator = GetComponent<Animator>();
            handler = new HumanPoseHandler(animator.avatar, transform);
            handler.GetHumanPose(ref pose);
            // Only the limbs change: the hips stay exactly where the model was placed. (Writing the
            // body position back through the handler moves Mixamo models, whose skeleton is scaled
            // inside the FBX, so the hips are restored after each pose instead.)
            hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            hipsPosition = hips.localPosition;
            hipsRotation = hips.localRotation;
            leftArm = Muscle("Left Arm Down-Up");
            rightArm = Muscle("Right Arm Down-Up");
            leftForearm = Muscle("Left Forearm Stretch");
            rightForearm = Muscle("Right Forearm Stretch");
            chest = Muscle("Chest Front-Back");
            head = Muscle("Head Nod Down-Up");
        }

        static int Muscle(string name) => System.Array.IndexOf(HumanTrait.MuscleName, name);

        void LateUpdate()
        {
            if (handler == null) return;
            handler.GetHumanPose(ref pose);
            float breath = Mathf.Sin(Time.time * Mathf.PI * 2f / breathSeconds) * 0.06f;
            Set(leftArm, armsDown); Set(rightArm, armsDown);
            Set(leftForearm, 0.85f); Set(rightForearm, 0.85f);
            Set(chest, breath);
            Set(head, -headDown);
            handler.SetHumanPose(ref pose);
            hips.SetLocalPositionAndRotation(hipsPosition, hipsRotation);
        }

        void Set(int index, float value)
        {
            if (index >= 0) pose.muscles[index] = value;
        }

        void OnDestroy() => handler?.Dispose();
    }
}
