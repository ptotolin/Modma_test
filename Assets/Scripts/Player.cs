using System;
using System.Text;
using UnityEngine;

public class Player : Unit
{
    private Rigidbody2D rb;
    private GUIStyle style;
    private PhysicsBasedMovement movement;
    
    protected override void Start()
    {
        base.Start();
        
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PhysicsBasedMovement>();
        style = new GUIStyle();
        style.fontSize = 40;
    }
    
    private void OnGUI()
    {
        if (rb != null) {
            var sb = new StringBuilder();
            sb.AppendLine($"Player speed: {rb.velocity.magnitude}");
            sb.AppendLine($"max speed: {movement.MaxSpeed}");
            var str = sb.ToString();
            GUI.Label(new Rect(10, 10, 200, 30), $"Player speed = {str}", style);
        }
    }
}
