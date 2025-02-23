using UnityEngine;

public class UISwingNatural : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector3 originalRotation;
    private float rotationVelocity = 0f; // Simulated angular velocity
    private float swingSpeed = 5f; // How quickly it reacts to movement
    private float returnSpeed = 3f; // Damping effect for return
    private float maxSwingAngle = 15f; // Max rotation angle
    private float sensitivity = 0.1f; // Mouse movement sensitivity
    private Vector3 lastMousePos;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalRotation = transform.eulerAngles;
        lastMousePos = Input.mousePosition;
    }

    void Update()
    {
        // Detect hover using RectTransform
        bool isHovered = RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition);

        if (isHovered)
        {
            // Get mouse movement direction
            Vector3 mouseDelta = Input.mousePosition - lastMousePos;
            lastMousePos = Input.mousePosition;

            // Apply force based on mouse movement
            float targetRotation = Mathf.Clamp(mouseDelta.x * sensitivity, -maxSwingAngle, maxSwingAngle);
            rotationVelocity += (targetRotation - transform.eulerAngles.z) * swingSpeed * Time.deltaTime;
        }

        // Apply rotation using SmoothDamp for natural motion
        float newRotation = Mathf.SmoothDampAngle(transform.eulerAngles.z, originalRotation.z, ref rotationVelocity, returnSpeed * Time.deltaTime);
        transform.eulerAngles = new Vector3(0, 0, newRotation);
    }
}
