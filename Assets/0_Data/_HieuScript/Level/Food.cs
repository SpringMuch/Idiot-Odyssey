using UnityEngine;

[DisallowMultipleComponent]
public class Food : ObjectController
{
    private bool _collected = false;

    public void EnableFood()
    {
        gameObject.SetActive(true);
        _collected = false;
    }

    protected override void OnTapped() { }
    protected override void OnPressed() { }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (_collected) return;
        FoodCounter counter = collision.GetComponent<FoodCounter>();
        if (counter == null) return;
        _collected = true;
        counter.RegisterFood();
        GameObjectSpawn.Instance.DeSapwn(this.gameObject);
    }
}
