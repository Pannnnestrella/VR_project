using UnityEngine;
using EzySlice;
using System.Collections;


public class TwoHandsInteractionController : MonoBehaviour
{
    public HandController_s leftHandController;
    public HandController_s rightHandController;
    public Material crossSectionMaterial;
    public float sliceDistanceThreshold = 0.1f;

    void Update()
    {
        if (BothHandsAreGrabbingTheSameObject())
        {
            HandController_s.RefreshAnchorsInScene();
            CheckAndPerformSlice();
        }

    }


    private bool BothHandsAreGrabbingTheSameObject()
    {
        return leftHandController.IsHoldingObject() && rightHandController.IsHoldingObject() &&
               leftHandController.GetGrabbedObject() == rightHandController.GetGrabbedObject();
    }

    private void CheckAndPerformSlice()
    {
        float leftHandMoveDistance = Vector3.Distance(leftHandController.initialGrabPosition, leftHandController.transform.position);
        float rightHandMoveDistance = Vector3.Distance(rightHandController.initialGrabPosition, rightHandController.transform.position);

        if (leftHandMoveDistance > sliceDistanceThreshold || rightHandMoveDistance > sliceDistanceThreshold)
        {
            PerformSlice(leftHandController.GetGrabbedObject());
            leftHandController.ReleaseObject();
            rightHandController.ReleaseObject();
        }
    }


    private void PerformSlice(ObjectAnchor_s objectToSlice)
    {


        if (objectToSlice != null)
        {

            Vector3 leftHandMove = leftHandController.transform.position - leftHandController.initialGrabPosition;
            Vector3 rightHandMove = rightHandController.transform.position - rightHandController.initialGrabPosition;

            Vector3 handPlaneNormal = (leftHandMove + rightHandMove).normalized;

            Vector3 planeNormal = Vector3.Cross(handPlaneNormal, Vector3.up).normalized;

            if (Mathf.Abs(Vector3.Dot(planeNormal, Vector3.up)) > 0.1)
            {
                planeNormal = Vector3.Cross(planeNormal, Vector3.up).normalized;
            }

            Vector3 midpoint = (leftHandController.transform.position + rightHandController.transform.position) / 2;

            SlicedHull slicedObject = objectToSlice.gameObject.Slice(midpoint, planeNormal, crossSectionMaterial);
            if (slicedObject != null)
            {
                GameObject upperHull = slicedObject.CreateUpperHull(objectToSlice.gameObject, crossSectionMaterial);
                GameObject lowerHull = slicedObject.CreateLowerHull(objectToSlice.gameObject, crossSectionMaterial);
                TriggerVibration(leftHandController);
                TriggerVibration(rightHandController);

                if (upperHull != null && lowerHull != null)
                {
                    upperHull.AddComponent<Rigidbody>().isKinematic = false;
                    upperHull.AddComponent<BoxCollider>();
                    lowerHull.AddComponent<Rigidbody>().isKinematic = false;
                    lowerHull.AddComponent<BoxCollider>();

                    ObjectAnchor_s upperAnchor = upperHull.AddComponent<ObjectAnchor_s>();
                    upperAnchor.graspingRadius = 1f;
                    upperAnchor.SetupAsGrabbable();

                    ObjectAnchor_s lowerAnchor = lowerHull.AddComponent<ObjectAnchor_s>();
                    lowerAnchor.graspingRadius = 1f;
                    lowerAnchor.SetupAsGrabbable();

                    upperHull.transform.position += Vector3.up * 0.1f;
                    lowerHull.transform.position += Vector3.down * 0.1f;

                    HandController_s.RefreshAnchorsInScene();

                    leftHandController.ReleaseObject();
                    rightHandController.ReleaseObject();
                    Destroy(objectToSlice.gameObject);
                }


                void TriggerVibration(HandController_s handController)
                {
                    float frequency = 1f;
                    float amplitude = 1f;
                    float duration = 0.5f;


                    OVRInput.Controller controller = (handController.handType == HandController_s.HandType.LeftHand)
                                                     ? OVRInput.Controller.LTouch
                                                     : OVRInput.Controller.RTouch;
                    OVRInput.SetControllerVibration(frequency, amplitude, controller);

                    StartCoroutine(StopVibrationAfterDelay(controller, duration));
                }

                IEnumerator StopVibrationAfterDelay(OVRInput.Controller controller, float delay)
                {
                    yield return new WaitForSeconds(delay);
                    OVRInput.SetControllerVibration(0, 0, controller);
                }


            }
        }
    }
}
    



    /*sliceable
     * private void PerformSlice(ObjectAnchor objectToSlice)
     {
         if (objectToSlice != null)
         {
             // 获取手的当前位置和初始位置
             Vector3 leftHandMove = leftHandController.transform.position - leftHandController.initialGrabPosition;
             Vector3 rightHandMove = rightHandController.transform.position - rightHandController.initialGrabPosition;

             // 手的移动平面的法向量为这两个移动向量的平均
             Vector3 handPlaneNormal = (leftHandMove + rightHandMove).normalized;

             // 找到一个既垂直于手移动方向，同时也水平的向量
             Vector3 planeNormal = Vector3.Cross(handPlaneNormal, Vector3.up).normalized;

             // 如果计算出的向量不是水平的，修正它
             if (Mathf.Abs(Vector3.Dot(planeNormal, Vector3.up)) > 0.1)
             {
                 // 保证平面法向量水平，再次校正
                 planeNormal = Vector3.Cross(planeNormal, Vector3.up).normalized;
             }

             // 计算两只手的中点，作为切割中心
             Vector3 midpoint = (leftHandController.transform.position + rightHandController.transform.position) / 2;

             // 使用 EzySlice 执行切割
             SlicedHull slicedObject = objectToSlice.gameObject.Slice(midpoint, planeNormal, crossSectionMaterial);
             if (slicedObject != null)
             {
                 GameObject upperHull = slicedObject.CreateUpperHull(objectToSlice.gameObject, crossSectionMaterial);
                 GameObject lowerHull = slicedObject.CreateLowerHull(objectToSlice.gameObject, crossSectionMaterial);

                 if (upperHull != null && lowerHull != null)
                 {
                     upperHull.AddComponent<Rigidbody>().isKinematic = false;
                     upperHull.AddComponent<MeshCollider>().convex = true;
                     upperHull.transform.position += Vector3.up * 0.1f;

                     lowerHull.AddComponent<Rigidbody>().isKinematic = false;
                     lowerHull.AddComponent<MeshCollider>().convex = true;
                     lowerHull.transform.position += Vector3.down * 0.1f;

                     Destroy(objectToSlice.gameObject);
                 }
             }
         }
     }
    */

