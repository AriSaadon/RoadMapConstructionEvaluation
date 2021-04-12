using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    Vector3 cam;
    Vector3 start;

    void Update()
    {
        if(Input.GetMouseButtonDown(2))
        {
            start = Input.mousePosition;
            cam = this.transform.position;
        }

        if(Input.GetMouseButton(2))
        {
            Vector3 pos = (Input.mousePosition - start);
            transform.position = cam + 2* new Vector3(-pos.x, 0, -pos.y);
        }

        transform.position -= new Vector3(0, 100 * Input.mouseScrollDelta.y, 0);
    }
}
