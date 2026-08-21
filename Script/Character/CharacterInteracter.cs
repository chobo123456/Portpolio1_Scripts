using UnityEngine;

public class CharacterInteracter
{
    private readonly LayerMask interactLayer;
    private readonly PlayerDataBox box;
    private readonly CapsuleCollider collider;
    private readonly float interactRadius;
    private float interactTime = -999f, interactIgnoreMinTime = 0.1f;
    public CharacterInteracter(PlayerDataBox box)
    {
        this.box = box;
        collider = box.rigid.GetComponent<CapsuleCollider>();
        interactRadius = 2f;
        interactLayer = LayerMask.GetMask("Item","NpcInteract");
    }

    public void OnUpdate()
    {
        if(box.input.IsInput(InputType.Interact) && Time.time - interactTime >= interactIgnoreMinTime)
        {
            Interact();
        }
    }
    public void Interact()
    {
        interactTime = Time.time;

        Vector3 startPos = box.rigid.transform.TransformPoint(collider.center);

        Collider[] cols = Physics.OverlapSphere(startPos, interactRadius, interactLayer);

        if(cols.Length > 0)
        {
            var comp = cols[0].GetComponent<IInteract>();

            comp?.Interact();

            if(comp is IItemObject itemComp)
            {
                itemComp?.ItemFollow(box.rigid.transform);
            }
        }
    }
}
