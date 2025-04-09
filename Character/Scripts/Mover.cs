using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Mover : MonoBehaviour
{
    private float walkSpeed = 1.5f;       // Yürüme hızı
    private float runSpeed = 5.6f;        // Koşma hızı
    [SerializeField] float acceleration;      // Hızlanma oranı
    [SerializeField] float deceleration;      // Yavaşlama oranı
    [SerializeField] float turnSpeed;       // Dönüş hızı

    private NavMeshAgent agent;
    private Animator animator;
    private float currentSpeed = 0f;     // Karakterin anlık hızı
    private float targetSpeed = 0f;      // Hedef hız (yavaşlama/hızlanma durumu)

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        HandleMovement();
        UpdateAnimator();
    }

    private void HandleMovement()
    {
        float moveZ = Input.GetAxisRaw("Vertical");
        moveZ = moveZ == -1 ? 0 : moveZ; // S tuşuna basıldığında engellenir

        bool isRunning = Input.GetKey(KeyCode.LeftShift); // Shift tuşuna basılı mı?

        // Hızlanma ve durma işlemleri
        if (moveZ != 0)
        {
            targetSpeed = isRunning ? runSpeed : walkSpeed;
        }
        else
        {
            // Hiçbir tuşa basılmadığında yavaşça durma
            targetSpeed = 0f;
        }

        // Hızlanma veya yavaşlama
        if (!isRunning)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, deceleration * Time.deltaTime);  // Yavaşlama
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime); // Hızlanma
        }

        // NavMeshAgent hızını ayarlama
        agent.speed = currentSpeed;

        // Yavaşça durma işlemi için velocity sıfırlanması
        agent.velocity = transform.forward * currentSpeed;

        // A/D ile karakterin yönünü değiştirme (yumuşak dönüş)
        float rotateY = Input.GetAxisRaw("Horizontal"); // A (-1) - D (1)
        if (rotateY != 0)
        {
            float targetAngle = rotateY * turnSpeed * Time.deltaTime; // Yumuşak dönüş
            transform.Rotate(Vector3.up * targetAngle);
        }
    }

    private void UpdateAnimator()
    {
        animator.SetFloat("forwardSpeed", Mathf.Abs(currentSpeed)); // Animator’a hız aktar
    }

}
