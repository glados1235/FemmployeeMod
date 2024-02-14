using UnityEngine;

namespace FemmployeeMod
{
    public class FemmployeeSuitPreview : MonoBehaviour
    {
        public Camera modelViewCamera;
        public GameObject previewModel;
        private float modelRotationValue;


        public SkinnedMeshRenderer BodyMeshRenderer;

        public bool isBeingEdited;

        public float bulgeSize;

        public float breastSize;

        public void Update()
        {
            if (isBeingEdited)
            {
                BodyMeshRenderer.SetBlendShapeWeight(0, breastSize);
                BodyMeshRenderer.SetBlendShapeWeight(1, bulgeSize);
            }
        }

        public void RotatePreviewModel(int turnDirection)
        {
            // Calculate the rotation angle based on the turn direction
            float rotationAngle = Time.deltaTime * turnDirection;

            // Apply the rotation around the Y axis
            previewModel.transform.Rotate(0, rotationAngle, 0, Space.Self);
        }


    }
}
