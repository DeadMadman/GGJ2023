using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityEngine.InputSystem;
using Unity.Mathematics;

public partial struct InputSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
    }

    public void OnDestroy(ref SystemState state)
    {

    }

    public void OnUpdate(ref SystemState state)
    {
        UnityEngine.InputSystem.InputSystem.Update();

        var keyboard = Keyboard.current;

        var cam = MainCamera.Instance;

        foreach (var input in SystemAPI.Query<RefRW<Input>>()) {
            input.ValueRW.justDodged = keyboard[Key.LeftShift].wasPressedThisFrame;
            var dir = Vector2.zero;
            if (keyboard[Key.A].isPressed) {
                dir += Vector2.left;
            }
            if (keyboard[Key.D].isPressed) {
                dir += Vector2.right;
            }
            if (keyboard[Key.W].isPressed) {
                dir += Vector2.up;
            }
            if (keyboard[Key.S].isPressed) {
                dir += Vector2.down;
            }
            dir.Normalize();

            input.ValueRW.plantButton = keyboard[Key.P].wasPressedThisFrame;

            Vector3 forward = math.cross(math.down(), cam.transform.right) * dir.y;
            Vector3 right = cam.transform.right * dir.x;
            input.ValueRW.movement = Vector3.Normalize(forward + right);
        }
    }
}
