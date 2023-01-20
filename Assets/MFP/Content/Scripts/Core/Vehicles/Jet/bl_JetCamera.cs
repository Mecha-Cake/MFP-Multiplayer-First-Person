using UnityEngine;

public class bl_JetCamera : MonoBehaviour {

    [SerializeField]
    private Transform Target = null;
    [Range(3, 20)]
    public float distance = 10.0f;
    [Range(0, 20)]
    public float Vertical = 3.0f;
    public float heightDamping = 2.0f;
    public float rotationDamping = 3.0f;

    void LateUpdate()
    {
        if (!Target)
            return;

        float wantedRotationAngle = Target.eulerAngles.y;
        float wantedHeight = Target.position.y + Vertical;

        float currentRotationAngle = transform.eulerAngles.y;
        float currentHeight = transform.position.y;

        float  wantedRotationAngleUp = Target.eulerAngles.x;
       float currentRotationAngleUp = transform.eulerAngles.x;

        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);
        currentRotationAngleUp = Mathf.LerpAngle(currentRotationAngleUp, wantedRotationAngleUp, rotationDamping * Time.deltaTime);

        currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);

        Quaternion currentRotation = Quaternion.Euler(currentRotationAngleUp, currentRotationAngle, 0);

        transform.position = Target.position;
        transform.position -= currentRotation * Vector3.forward * distance;
        Vector3 temp = transform.position;
        temp.y = currentHeight;
        transform.position = temp;

        transform.LookAt(Target);
    }
}