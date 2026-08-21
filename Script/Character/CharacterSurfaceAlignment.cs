using UnityEngine;

public class CharacterSurfaceAlignment
{
    private const float probeDistance = 1.2f;
    private readonly PlayerDataBox box;
    private LayerMask groundLayer;
    private int stepsSinceLastGround = 0;
    private int stepsSinceLastJump = 0;

    public CharacterSurfaceAlignment(PlayerDataBox box)
    {
        this.box = box;

        groundLayer = LayerMask.GetMask("Ground");
    }
    
    public void UpdateState()
    {
        stepsSinceLastGround += 1;
        stepsSinceLastJump += 1;

        if(box.sensor.IsGround || SnapToGround())
            stepsSinceLastGround = 0;
    }

    public void OnJump()
    {
        stepsSinceLastJump = 0;
    }

    public bool SnapToGround()
    {    
        if(stepsSinceLastJump <= 20 ||  stepsSinceLastGround > 2) return false;

        Vector3 velocity = box.rigid.linearVelocity;
        
        if(!Physics.Raycast(box.rigid.position, Vector3.down, out RaycastHit hit, probeDistance, groundLayer)) //땅이 찍히지않음 => 끝냄
           return false;    
        
        if(hit.normal.y < box.sensor.MinGroundDotProduct) // 찍힌 Y값이 비교값보다 낮으면 하지않음
            return false;
        
        float speed = box.rigid.linearVelocity.magnitude;
        float dot = Vector3.Dot(velocity, hit.normal);  

        if(dot > 0f) //현재 속도 에서 위로 향하는 속도를 뺀후 속도를 다시 곱함
            box.rigid.linearVelocity = (velocity - hit.normal * dot).normalized * speed;

        box.sensor.SetGrounded();

        return true;
    }
}
