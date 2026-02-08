using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
  void OnTriggerEnter2D(Collider2D other)
  {
    Coin2DController coin = other.GetComponent<Coin2DController>();
    if (coin != null)
      coin.OnScored();
  }
}
