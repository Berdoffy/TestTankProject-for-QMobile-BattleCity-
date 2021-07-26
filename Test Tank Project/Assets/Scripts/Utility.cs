using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility 
{
  public static Vector2 GetDirection(Vector2 end, Vector2 start)
  {
    return (end - start).normalized;
  }
  public static float GetSqrMagintude(Vector2 end, Vector2 start)
  {
    return (end - start).SqrMagnitude();
  }
}
