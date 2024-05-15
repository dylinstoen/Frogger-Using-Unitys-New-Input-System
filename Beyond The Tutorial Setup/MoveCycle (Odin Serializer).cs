using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MoveCycle : MonoBehaviour
{
    public enum moveOptionsEnum {Left, Right};
    static readonly Vector2[] moveOptions = new Vector2[] {Vector2.left, Vector2.right};
    [EnumToggleButtons, LabelText("Direction")]
    public moveOptionsEnum directionOptions;
    [SerializeField] float speed = 1f;
    [SerializeField] int size = 1;
    private Vector2 direction;
    private Vector3 leftEdge;
    private Vector3 rightEdge;
    void Start()
    {
        leftEdge = Camera.main.ViewportToWorldPoint(Vector3.zero); 
        rightEdge = Camera.main.ViewportToWorldPoint(Vector3.right);
        direction = moveOptions[(int)directionOptions];
    }
 
    void Update()
    {
        if (direction.x > 0 && (transform.position.x - size) > rightEdge.x) // is the enviorment moving to the right and is the left edge of the sprite past the right edge of the screen 
        {
            Vector3 position = transform.position;
            position.x = leftEdge.x - size; // bring it back to the left edge of the screen
            transform.position = position;
        }
        else if(direction.x < 0 && (transform.position.x + size) < leftEdge.x) // is the enviorment moving to the left and is the right edge of the sprite past the left edge of the screen 
        {
            Vector3 position = transform.position;
            position.x = rightEdge.x + size; // bring it back to the right edge of the screen
            transform.position = position;
        } else // move enviorment
        {
            transform.Translate(direction * speed * Time.deltaTime);
        }
    }
}
