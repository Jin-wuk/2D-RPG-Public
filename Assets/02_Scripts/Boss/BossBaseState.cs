using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BossBaseState
{
    protected BossController owner;

    public BossBaseState(BossController owner)
    {
        this.owner = owner;
    }

    public virtual void OnGroundTouch() { } // 땅체크 가상함수

    public abstract void Enter();           // 상태가 시작될 때 실행
    public abstract void Update();          // 매 프레임 실행
    public abstract void PhysicsUpdate();   // 물리 연산 시 실행
    public abstract void Exit();            // 상태가 끝날 때 실행
}
