using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class _temp : MonoBehaviour {

    public Animator anim;

    Vector3 feetFacing;
    float fakeVelocity;

    void LateUpdate()
    {
        float b = (Mathf.Atan2(feetFacing.z, feetFacing.x) - Mathf.Atan2(transform.forward.z, transform.forward.x))
            * Mathf.Rad2Deg;
        if (b > 180) b = -360 + b;


        anim.SetFloat("Velocity", fakeVelocity);
        anim.SetFloat("Feet Angle", b);
        anim.SetFloat("Feet Angle mod", b % 60);

        Debug.Log(feetFacing);

        if (fakeVelocity < 0.3f) 
        {
            if (b > 60 || b < -60) // not calling abs
            {
                feetFacing = transform.forward;
                anim.SetTrigger("Restep");
            }
        }
    }

    public void Update()
    {
        Vector3 moveToward = new Vector3(
                Input.GetAxis("Horizontal"),
                0f,
                Input.GetAxis("Vertical"));

        if (moveToward.magnitude > float.Epsilon)
            Move(moveToward);


        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            float anlge = Vector3.Angle(
                            transform.forward,
                            hit.point - transform.position
                          );

            LookAt(hit.point);
        }
    }

    public void Move(Vector3 toward)
    {
        feetFacing = toward.normalized;

        transform.position += toward * .1f;
        //anim.SetFloat("Velocity", speed);

        fakeVelocity = toward.magnitude > .3f ? toward.magnitude : 0;
    }

    public void LookAt(Vector3 point)
    {
        var difNorm = (point - transform.position).normalized;
        float angle = Mathf.Atan2(difNorm.z, difNorm.x) * Mathf.Rad2Deg;
        if (Mathf.Abs(angle) > 0.05f)
            transform.rotation = Quaternion.Euler(0f, 90f - angle, 0f);
    }

}
