using UnityEngine;

public class PlayerMainCamera : MonoBehaviour
{

    public void Update()
    {
        transform.position = GameManager.Character.transform.position + new Vector3( 0, 0, -20f );
    }

}
