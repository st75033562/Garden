using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveControl : MonoBehaviour {

    private CharacterController character;

    private float speed;

    void Start()
    {

        character = this.GetComponent<CharacterController>();

        speed = 1f;

    }

    // Update is called once per frame

    void Update()
    {

        Move();

    }

    void Move()

    {

        float horizontal = Input.GetAxis("Horizontal"); //A D 左右

        float vertical = Input.GetAxis("Vertical"); //W S 上 下

        character.Move(new Vector3(horizontal, 0, vertical) * speed * Time.deltaTime);

    }
}
